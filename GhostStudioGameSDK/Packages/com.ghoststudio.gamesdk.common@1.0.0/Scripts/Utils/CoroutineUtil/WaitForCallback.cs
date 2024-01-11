using System;
using UnityEngine;

namespace AppBase.Utils
{
    /// <summary>
    /// 带超时的等待回调的协程
    /// </summary>
    public class WaitForCallback : CustomYieldInstruction
    {
        /// <summary>
        /// 是否完成
        /// </summary>
        private bool isFinished;
        
        /// <summary>
        /// 超时时间
        /// </summary>
        private float timeoutTime;
        
        /// <summary>
        /// 是否超时
        /// </summary>
        private bool isTimeout => timeoutTime > 0 && Time.realtimeSinceStartup > timeoutTime;
        
        /// <summary>
        /// 是否需要等待
        /// </summary>
        public override bool keepWaiting => !isFinished && !isTimeout;

        /// <summary>
        /// 带超时的等待回调的协程
        /// </summary>
        /// <param name="callback">回调</param>
        /// <param name="timeout">超时时间</param>
        public WaitForCallback(Action<Action> callback, float timeout = 0)
        {
            if (timeout > 0)
            {
                timeoutTime = Time.realtimeSinceStartup + timeout;
            }
            callback?.Invoke(OnFinish);
        }

        /// <summary>
        /// 设置完成
        /// </summary>
        private void OnFinish()
        {
            isFinished = true;
        }
    }
}