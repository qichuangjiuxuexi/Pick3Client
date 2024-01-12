using System;
using System.Buffers;
using System.Net.WebSockets;
using System.Threading;
using Cysharp.Threading.Tasks;

namespace AppBase.Network.WebSocket
{
    /// <summary>
    /// 可靠的自动重连的WebSocket客户端
    /// </summary>
    public class UnityWebSocket
    {
        private readonly WebSocketPipe _webSocketPipe;
        private readonly string _url;
        private Action<ClientWebSocketOptions> _setOptions = null;
        private int _keepAliveInterval;
        private int _reconnectInterval;
        private PlayerLoopTimer _keepAliveTimer;
        private PlayerLoopTimer _reconnectTimer;
        private Func<ArraySegment<byte>?> _keepAlivePayloadFunc;
        private Exception ConnectException { get; set; }
        private Exception CloseException { get; set; }
        private UniTask? ReceiveTask { get; set; }

        public ConnectionState ConnectionState { get; private set; }
        public event Func<UniTask> OnConnected;
        public event Func<Exception, UniTask> OnReconnecting;
        public event Func<UniTask> OnKeepAlive;
        public event Func<Exception, UniTask> OnClosed;
        public event Func<ReadOnlySequence<byte>, UniTask> OnReceived;

        public UnityWebSocket(string url)
        {
            _url = url;
            _webSocketPipe = new WebSocketPipe();
        }

        public async UniTask StartAsync(CancellationToken ct = default)
        {
            try
            {
                await _webSocketPipe.StartAsync(_url, _setOptions, ct);

                ConnectionState = ConnectionState.Connected;

                ReceiveTask = ReceiveLoop();

                await (OnConnected?.Invoke() ?? UniTask.CompletedTask);
            }
            catch (Exception ex)
            {
                ConnectException = ex;

                if (ConnectionState == ConnectionState.Reconnecting)
                {
                    _ = OnReconnecting?.Invoke(ConnectException);
                }
            }
        }

        public async UniTask SendAsync(ArraySegment<byte> data, CancellationToken ct = default)
        {
            await _webSocketPipe.Output.WriteAsync(data, ct);
        }

        public async UniTask StopAsync()
        {
            _keepAliveTimer?.Dispose();
            _keepAliveTimer = null;
            _reconnectTimer?.Dispose();
            _reconnectTimer = null;

            _webSocketPipe.Input.CancelPendingRead();

            await (ReceiveTask ?? UniTask.CompletedTask);

            await _webSocketPipe.StopAsync();
        }

        public UnityWebSocket SetOptions(Action<ClientWebSocketOptions> setOptions)
        {
            _setOptions = setOptions;

            return this;
        }

        public UnityWebSocket SetKeepAlive(Func<ArraySegment<byte>?> payloadFunc = null, int intervalMilliseconds = 30000)
        {
            _keepAliveInterval = intervalMilliseconds;
            _keepAlivePayloadFunc = payloadFunc;
            _keepAliveTimer?.Dispose();
            _keepAliveTimer = PlayerLoopTimer.Create(TimeSpan.FromMilliseconds(_keepAliveInterval), true, DelayType.UnscaledDeltaTime, PlayerLoopTiming.Update, CancellationToken.None, (objState) =>
            {
                if (ConnectionState != ConnectionState.Connected)
                {
                    return;
                }

                _ = KeepAlive();

            }, null);

            return this;
        }

        public UnityWebSocket SetAutomaticReconnect(int intervalMilliseconds = 10000)
        {
            _reconnectInterval = intervalMilliseconds;
            _reconnectTimer?.Dispose();
            _reconnectTimer = PlayerLoopTimer.Create(TimeSpan.FromMilliseconds(_reconnectInterval), true, DelayType.UnscaledDeltaTime, PlayerLoopTiming.Update, CancellationToken.None, (objState) =>
            {
                if (ConnectionState == ConnectionState.Connected)
                {
                    return;
                }

                ConnectionState = ConnectionState.Reconnecting;

                _ = StartAsync();

            }, null);
            return this;
        }

        private async UniTask ReceiveLoop()
        {
            var input = _webSocketPipe.Input;

            try
            {
                while (true)
                {
                    var result = await input.ReadAsync();
                    var buffer = result.Buffer;

                    try
                    {
                        if (result.IsCanceled)
                        {
                            break;
                        }

                        if (!buffer.IsEmpty)
                        {
                            while (MessageParser.TryParse(ref buffer, out var payload))
                            {
                                await (OnReceived?.Invoke(payload) ?? UniTask.CompletedTask);
                            }
                        }

                        if (result.IsCompleted)
                        {
                            break;
                        }
                    }
                    finally
                    {
                        input.AdvanceTo(buffer.Start, buffer.End);
                    }
                }
            }
            catch (Exception ex)
            {
                CloseException = ex;
            }
            finally
            {
                ConnectionState = ConnectionState.Disconnected;

                _ = OnClosed?.Invoke(CloseException);
            }
        }

        private async UniTask KeepAlive(CancellationToken ct = default)
        {
            var payload = _keepAlivePayloadFunc?.Invoke() ?? Memory<byte>.Empty;

            await _webSocketPipe.Output.WriteAsync(payload, ct);

            _ = OnKeepAlive?.Invoke();
        }
    }
}