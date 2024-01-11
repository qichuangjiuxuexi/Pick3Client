using System.Text;
using AppBase.Utils;

namespace AppBase.Network
{
    /// <summary>
    /// Json协议基类
    /// </summary>
    public class JsonProtoco : NetworkProtocol
    {
        /// <summary>
        /// 请求数据
        /// </summary>
        public string requestJson;
        
        /// <summary>
        /// 返回数据
        /// </summary>
        public string responseJson;
        
        /// <summary>
        /// 数据类型
        /// </summary>
        public override string contentType => "application/json";

        /// <summary>
        /// 请求字节数据
        /// </summary>
        public override byte[] requestBytes => string.IsNullOrEmpty(requestJson) ? null : Encoding.UTF8.GetBytes(requestJson);

        /// <summary>
        /// 返回字节数据
        /// </summary>
        public override byte[] responseBytes
        {
            set
            {
                responseJson = Encoding.UTF8.GetString(value);
            }
        }

        /// <summary>
        /// 发送前，打印请求数据
        /// </summary>
        internal sealed override bool OnInternalSend()
        {
            if (string.IsNullOrEmpty(requestJson)) return false;
            if (AppUtil.IsDebug)
            {
                Debugger.Log(TAG, $"Request: \"{service}/{action}\", Json: {requestJson}");
            }
            return true;
        }

        /// <summary>
        /// 接收到，打印请求数据
        /// </summary>
        internal sealed override bool OnInternalResponse()
        {
            if (string.IsNullOrEmpty(responseJson))
            {
                Debugger.LogError(TAG, $"Response: \"{service}/{action}\", responseJson is empty");
                return false;
            }
            if (AppUtil.IsDebug)
            {
                Debugger.Log(TAG, $"Response: \"{service}/{action}\", Json: {responseJson}");
            }
            return true;
        }
    }

    /// <summary>
    /// Json协议基类
    /// </summary>
    /// <typeparam name="T">请求类型</typeparam>
    /// <typeparam name="R">返回类型</typeparam>
    public class JsonProtoco<T, R> : JsonProtoco where T : new() where R : new()
    {
        /// <summary>
        /// 请求数据
        /// </summary>
        public T request;
        
        /// <summary>
        /// 返回数据
        /// </summary>
        public R response;

        public JsonProtoco()
        {
        }

        public JsonProtoco(T request)
        {
            this.request = request;
        }

        /// <summary>
        /// 请求字节数据
        /// </summary>
        public override byte[] requestBytes
        {
            get
            {
                if (request == null) return null;
                requestJson = JsonUtil.SerializeObject(request);
                return base.requestBytes;
            }
        }

        /// <summary>
        /// 返回字节数据
        /// </summary>
        public override byte[] responseBytes
        {
            set
            {
                base.responseBytes = value;
                if (!string.IsNullOrEmpty(responseJson))
                {
                    response = JsonUtil.DeserializeObject<R>(responseJson);
                }
            }
        }
    }
}
