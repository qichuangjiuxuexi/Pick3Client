using System;
using System.IO.Pipelines;
using System.Net.WebSockets;
using System.Threading;
using Cysharp.Threading.Tasks;
using PipeOptions = System.IO.Pipelines.PipeOptions;

namespace AppBase.Network.WebSocket
{
    internal class WebSocketPipe : IDuplexPipe
    {
        private ClientWebSocket _webSocket;
        private IDuplexPipe _transport;
        private IDuplexPipe _application;
        private volatile bool _aborted;
        private const int DefaultBufferSize = 1 * 1024 * 1024;
        private const int DefaultSocketBufferSize = 1024;
        internal UniTask Running { get; private set; } = UniTask.CompletedTask;
        public PipeReader Input => _transport!.Input;
        public PipeWriter Output => _transport!.Output;

        public WebSocketPipe()
        {
        }

        public async UniTask StartAsync(string url, Action<ClientWebSocketOptions> setOptions = null, CancellationToken ct = default)
        {
            _webSocket = new ClientWebSocket();
            setOptions?.Invoke(_webSocket.Options);
            try
            {
                await _webSocket.ConnectAsync(new Uri(url), ct);
            }
            catch
            {
                _webSocket.Dispose();
                throw;
            }
            var pipeOptions = new PipeOptions(
                pauseWriterThreshold: DefaultBufferSize,
                resumeWriterThreshold: DefaultBufferSize / 2,
                readerScheduler: PipeScheduler.ThreadPool,
                useSynchronizationContext: false);
            var input = new Pipe(pipeOptions);
            var output = new Pipe(pipeOptions);
            // The transport duplex pipe is used by the caller to
            // - subscribe to incoming websocket messages
            // - push messages to the websocket
            _transport = new NetworkPipe(output.Reader, input.Writer);
            // The application duplex pipe is used here to
            // - subscribe to incoming messages from the caller
            // - proxy incoming data from the websocket back to the subscriber
            _application = new NetworkPipe(input.Reader, output.Writer);
            Running = ProcessSocketAsync(_webSocket);
        }

        private async UniTask ProcessSocketAsync(System.Net.WebSockets.WebSocket socket)
        {
            using (socket)
            {
                var receiving = StartReceiving(socket);
                var sending = StartSending(socket);
                var trigger = await UniTask.WhenAny(receiving, sending);
                if (trigger == 0)
                {
                    _application!.Input.CancelPendingRead();
                    using var delayCts = new CancellationTokenSource();
                    var resultTask = await UniTask.WhenAny(sending, UniTask.Delay(TimeSpan.FromSeconds(5), true, cancellationToken: delayCts.Token));
                    if (resultTask != 0)
                    {
                        _aborted = true;
                        socket.Abort();
                    }
                    else
                    {
                        delayCts.Cancel();
                    }
                }
                else
                {
                    _aborted = true;
                    socket.Abort();
                    _application!.Output.CancelPendingFlush();
                }
            }
        }

        private async UniTask StartReceiving(System.Net.WebSockets.WebSocket socket)
        {
            try
            {
                while (true)
                {
                    ValueWebSocketReceiveResult receiveResult;
                    do
                    {
                        var memory = _application!.Output.GetMemory(DefaultSocketBufferSize);
                        receiveResult = await socket.ReceiveAsync(memory, CancellationToken.None);
                        _application.Output.Advance(receiveResult.Count);
                    } while (receiveResult.MessageType != WebSocketMessageType.Close && !receiveResult.EndOfMessage);
                    if (receiveResult.MessageType == WebSocketMessageType.Close)
                    {
                        if (socket.CloseStatus != WebSocketCloseStatus.NormalClosure)
                        {
                            throw new InvalidOperationException($"Websocket closed with error: {socket.CloseStatus}.");
                        }
                        return;
                    }
                    MessageFormatter.WriteRecordSeparator(_application.Output);
                    var flushResult = await _application.Output.FlushAsync();
                    if (flushResult.IsCanceled || flushResult.IsCompleted)
                    {
                        break;
                    }
                }
            }
            catch (OperationCanceledException ex)
            {
                if (!_aborted)
                {
                    await _application!.Output.CompleteAsync(ex);
                }
            }
            catch (Exception ex)
            {
                if (!_aborted)
                {
                    await _application!.Output.CompleteAsync(ex);
                }
            }
            finally
            {
                await _application!.Output.CompleteAsync();
            }
        }

        private async UniTask StartSending(System.Net.WebSockets.WebSocket socket)
        {
            Exception error = null;
            try
            {
                while (true)
                {
                    var result = await _application!.Input.ReadAsync();
                    var buffer = result.Buffer;
                    try
                    {
                        if (result.IsCanceled)
                        {
                            break;
                        }
                        if (!buffer.IsEmpty)
                        {
                            try
                            {
                                if (WebSocketCanSend(socket))
                                {
                                    await socket.SendAsync(buffer, WebSocketMessageType.Text);
                                }
                                else
                                {
                                    break;
                                }
                            }
                            catch (Exception)
                            {
                                break;
                            }
                        }
                        else if (result.IsCompleted)
                        {
                            break;
                        }
                    }
                    finally
                    {
                        _application.Input.AdvanceTo(buffer.End);
                    }
                }
            }
            catch (Exception ex)
            {
                error = ex;
            }
            finally
            {
                if (WebSocketCanSend(socket))
                {
                    try
                    {
                        await socket.CloseOutputAsync(
                            error != null ? WebSocketCloseStatus.InternalServerError : WebSocketCloseStatus.NormalClosure,
                            "",
                            CancellationToken.None);
                    }
                    catch (Exception)
                    {
                    }
                }
                _application!.Input.Complete();
            }
        }

        private static bool WebSocketCanSend(System.Net.WebSockets.WebSocket ws)
        {
            return !(ws.State == WebSocketState.Aborted ||
                     ws.State == WebSocketState.Closed ||
                     ws.State == WebSocketState.CloseSent);
        }

        public async UniTask StopAsync()
        {
            if (_application == null)
            {
                return;
            }
            _transport!.Output.Complete();
            _transport!.Input.Complete();
            _application.Input.CancelPendingRead();
            try
            {
                await Running;
            }
            catch (Exception)
            {
                return;
            }
            finally
            {
                _webSocket?.Dispose();
            }
        }
    }
}
