using System;

namespace AppBase.Resource
{
    /// <summary>
    /// 引用计数对象
    /// </summary>
    public class Retainable : IDisposable
    {
        /// <summary>
        /// 引用计数
        /// </summary>
        public int RetainCount { get; protected set; }
        
        /// <summary>
        /// 引用计数加1
        /// </summary>
        public int Retain()
        {
            return ++RetainCount;
        }
        
        /// <summary>
        /// 引用计数减1
        /// </summary>
        /// <returns>剩余的引用计数，如果为0会被析构</returns>
        public int Release()
        {
            --RetainCount;
            return CheckRetainCount();
        }
        
        /// <summary>
        /// 检查引用计数，如果为0会调用析构
        /// </summary>
        /// <returns>剩余的引用计数</returns>
        public int CheckRetainCount()
        {
            if (RetainCount <= 0)
            {
                Dispose();
            }
            return RetainCount;
        }

        /// <summary>
        /// 直接析构
        /// </summary>
        public void Dispose()
        {
            RetainCount = 0;
            OnDestroy();
        }
        
        /// <summary>
        /// 析构事件
        /// </summary>
        protected virtual void OnDestroy()
        {
        }
    }
}
