using System.Buffers;
using System.Net.WebSockets;
using System.Threading;
using Cysharp.Threading.Tasks;

namespace AppBase.Network.WebSocket
{
    internal static class WebSocketExtensions
    {
        public static async UniTask SendAsync(this System.Net.WebSockets.WebSocket webSocket, ReadOnlySequence<byte> buffer, WebSocketMessageType webSocketMessageType, CancellationToken cancellationToken = default)
        {
            if (buffer.IsSingleSegment)
            {
                await webSocket.SendAsync(buffer.First, webSocketMessageType, endOfMessage: true, cancellationToken);
            }
            else
            {
                var position = buffer.Start;
                buffer.TryGet(ref position, out var prevSegment);
                while (buffer.TryGet(ref position, out var segment))
                {
                    await webSocket.SendAsync(prevSegment, webSocketMessageType, endOfMessage: false, cancellationToken);
                    prevSegment = segment;
                }
                await webSocket.SendAsync(prevSegment, webSocketMessageType, endOfMessage: true, cancellationToken);
            }
        }
    }
}