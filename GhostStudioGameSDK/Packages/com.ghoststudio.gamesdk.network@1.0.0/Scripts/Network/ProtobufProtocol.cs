using AppBase.Utils;
using Google.Protobuf;

namespace AppBase.Network
{
    /// <summary>
    /// Protobuf协议基类
    /// </summary>
    /// <typeparam name="T">请求类型</typeparam>
    /// <typeparam name="R">返回类型</typeparam>
    public class ProtobufProtocol<T, R> : NetworkProtocol where T : IMessage, new() where R : IMessage, new()
    {
        /// <summary>
        /// 请求数据
        /// </summary>
        public T request;
        
        /// <summary>
        /// 返回数据
        /// </summary>
        public R response;
        
        /// <summary>
        /// 数据类型
        /// </summary>
        public override string contentType => "application/proto";

        /// <summary>
        /// 请求字节数据
        /// </summary>
        public override byte[] requestBytes => request?.ToByteArray();

        /// <summary>
        /// 返回字节数据
        /// </summary>
        public override byte[] responseBytes
        {
            set
            {
                response = new R();
                response.MergeFrom(value);
            }
        }

        public ProtobufProtocol()
        {
            request = new T();
        }

        public ProtobufProtocol(T request)
        {
            this.request = request;
        }

        /// <summary>
        /// 发送前，打印请求数据
        /// </summary>
        internal sealed override bool OnInternalSend()
        {
            if (request == null) return false;
            if (AppUtil.IsDebug)
            {
                var requestJson = JsonFormatter.Default.Format(request);
                Debugger.Log(TAG, $"Request: \"{service}/{action}\", Json: {requestJson}");
            }
            return true;
        }

        /// <summary>
        /// 接收到，打印请求数据
        /// </summary>
        internal sealed override bool OnInternalResponse()
        {
            if (response == null)
            {
                Debugger.LogError(TAG, $"Response: \"{service}/{action}\", Response is null");
                return false;
            }
            if (AppUtil.IsDebug)
            {
                var responseJson = JsonFormatter.Default.Format(response);
                Debugger.Log(TAG, $"Response: \"{service}/{action}\", Json: {responseJson}");
            }
            return true;
        }
    }
}
