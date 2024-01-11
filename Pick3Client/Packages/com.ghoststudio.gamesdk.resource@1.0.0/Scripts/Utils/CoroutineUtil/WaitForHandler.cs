using UnityEngine;
using UnityEngine.ResourceManagement.AsyncOperations;

namespace AppBase.Utils
{
    /// <summary>
    /// 带超时的等待操作句柄的协程
    /// </summary>
    public class WaitForHandler : CustomYieldInstruction
    {
        /// <summary>
        /// 等待的操作句柄
        /// </summary>
        protected AsyncOperationHandle handler;
        
        /// <summary>
        /// 超时截止时间
        /// </summary>
        protected float endTime;
        
        /// <summary>
        /// 是否超时
        /// </summary>
        protected bool isTimeout => endTime > 0 && Time.realtimeSinceStartup > endTime;
        
        /// <summary>
        /// 是否需要等待
        /// </summary>
        public override bool keepWaiting => !handler.IsDone && !isTimeout;
        
        /// <summary>
        /// 带超时的等待操作句柄的协程
        /// </summary>
        /// <param name="handler">操作句柄</param>
        /// <param name="timeout">超时时间</param>
        public WaitForHandler(AsyncOperationHandle handler, float timeout = 0)
        {
            if (timeout > 0)
            {
                endTime = Time.realtimeSinceStartup + timeout;
            }
            this.handler = handler;
        }
    }
}