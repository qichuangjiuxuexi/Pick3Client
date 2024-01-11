using System;
using API.V1.Game;

namespace AppBase.Network
{
    /// <summary>
    /// 网络请求基类
    /// </summary>
    public abstract class NetworkProtocol
    {
        /// <summary>
        /// 服务/功能模块
        /// </summary>
        public virtual string service => null;
        
        /// <summary>
        /// 动作
        /// </summary>
        public virtual string action => null;

        /// <summary>
        /// 数据类型
        /// </summary>
        public virtual string contentType => null;
        
        /// <summary>
        /// 超时时间，秒
        /// </summary>
        public virtual int timeout => 20;

        /// <summary>
        /// 请求字节数据
        /// </summary>
        public virtual byte[] requestBytes => null;

        /// <summary>
        /// 返回字节数据
        /// </summary>
        public virtual byte[] responseBytes
        {
            set { }
        }

        /// <summary>
        /// 请求状态码
        /// </summary>
        public ErrorReason errorCode;
        protected string TAG => nameof(NetworkManager);
        
        /// <summary>
        /// 发送前调用，可进行一些检查，如登录状态等
        /// </summary>
        /// <param name="callback">完成后返回是否可发送</param>
        public virtual void OnBeforeSend(Action<bool> callback)
        {
            if (GameBase.Instance.GetModule<NetworkManager>().IsLogin)
            {
                callback?.Invoke(true);
            }
            else
            {
                GameBase.Instance.GetModule<NetworkManager>().Send<LoginProtocol>(new LoginProtocol(), callback);
            }
        }
        
        /// <summary>
        /// 当发送前调用
        /// </summary>
        /// <returns>是否可以发送，如果返回false，则直接发送失败</returns>
        public virtual bool OnSend()
        {
            return true;
        }
        
        /// <summary>
        /// 当收到成功结果后调用，如果收到失败结果不会调用
        /// </summary>
        /// <returns>是否处理成功，如果返回false，则向业务层返回失败</returns>
        public virtual bool OnResponse()
        {
            return true;
        }
        
        /// <summary>
        /// 当收到失败结果后调用，如果收到成功结果不会调用
        /// </summary>
        public virtual void OnFail()
        {
        }

        #region 内部使用

        /// <summary>
        /// 当发送前调用，内部使用
        /// </summary>
        internal virtual bool OnInternalSend()
        {
            return true;
        }

        /// <summary>
        /// 当发送前调用，内部使用
        /// </summary>
        internal virtual bool OnInternalResponse()
        {
            return true;
        }

        #endregion
    }
}