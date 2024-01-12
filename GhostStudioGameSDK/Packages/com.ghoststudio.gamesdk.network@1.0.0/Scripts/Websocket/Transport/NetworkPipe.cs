using System.IO.Pipelines;

namespace AppBase.Network.WebSocket
{
    internal class NetworkPipe : IDuplexPipe
    {
        public NetworkPipe(PipeReader reader, PipeWriter writer)
        {
            Input = reader;
            Output = writer;
        }

        public PipeReader Input { get; }

        public PipeWriter Output { get; }
    }
}