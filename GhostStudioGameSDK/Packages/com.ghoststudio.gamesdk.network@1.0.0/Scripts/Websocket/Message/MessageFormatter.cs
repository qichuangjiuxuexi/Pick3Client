using System.Buffers;

namespace AppBase.Network.WebSocket
{
    internal static class MessageFormatter
    {
        public const byte RecordSeparator = 0x1e;

        public static void WriteRecordSeparator(IBufferWriter<byte> output)
        {
            var buffer = output.GetSpan(1);
            buffer[0] = RecordSeparator;
            output.Advance(1);
        }
    }
}
