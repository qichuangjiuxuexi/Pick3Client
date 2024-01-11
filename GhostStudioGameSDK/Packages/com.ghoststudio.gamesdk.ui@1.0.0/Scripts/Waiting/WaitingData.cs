using System;
using System.Collections;
using UnityEngine;

namespace AppBase.UI.Waiting
{
    /// <summary>
    /// 等待遮罩数据
    /// </summary>
    public class WaitingData : IEnumerator
    {
        /// <summary>
        /// 地址
        /// </summary>
        public string address;

        /// <summary>
        /// 最多等待时间，0表示一直等待
        /// </summary>
        public float delayTime;
        
        /// <summary>
        /// 超过等待时间后的回调
        /// </summary>
        public event Action timeoutCallback;
        internal void OnTimeoutCallback()
        {
            timeoutCallback?.Invoke();
            timeoutCallback = null;
            isDone = true;
        }

        /// <summary>
        /// 被隐藏后的回调，如果超时则不会调用
        /// </summary>
        public event Action hideCallback;
        internal void OnHideCallback()
        {
            hideCallback?.Invoke();
            hideCallback = null;
            isDone = true;
        }
        
        /// <summary>
        /// 背景遮罩颜色
        /// </summary>
        public Color bgMaskColor = new Color(0, 0, 0, 0.8f);
        
        /// <summary>
        /// 背景遮罩出现时间
        /// </summary>
        public float bgMaskAppearDuration = 0.5f;
        
        /// <summary>
        /// 背景遮罩消失时间
        /// </summary>
        public float bgMaskDisappearDuration = 0.5f;

        /// <summary>
        /// 创建等待遮罩数据
        /// </summary>
        /// <param name="address">等待遮罩prefab地址</param>
        /// <param name="delayTime">最多等待时间（秒）</param>
        /// <param name="timeoutCallback">超时回调</param>
        /// <param name="hideCallback">关闭回调</param>
        public WaitingData(string address = null, float delayTime = 0, Action timeoutCallback = null, Action hideCallback = null)
        {
            this.address = address;
            this.delayTime = delayTime;
            if (timeoutCallback != null) this.timeoutCallback += timeoutCallback;
            if (hideCallback != null) this.hideCallback += hideCallback;
        }

        #region 协程相关

        private bool isDone;
        /// <summary>
        /// 协程等待直到遮罩超时或关闭
        /// </summary>
        public bool MoveNext() => !isDone;
        public void Reset() => isDone = false;
        public object Current => null;

        #endregion
    }
}