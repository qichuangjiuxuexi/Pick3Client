using System;
using AppBase.UI.Dialog;

namespace AppBase.UI.OneByOne
{
    /// <summary>
    /// 触发器数据
    /// </summary>
    public class OneByOneTriggerData
    {
        /// <summary>
        /// 触发器ID
        /// </summary>
        public string triggerId;

        /// <summary>
        /// 用户数据
        /// </summary>
        public object data;

        /// <summary>
        /// 触发器执行完成回调
        /// </summary>
        public event Action finishCallback;
        public void OnFinishCallback() => finishCallback?.Invoke();
        
        /// <summary>
        /// 创建触发器数据
        /// </summary>
        /// <param name="triggerId">触发器ID</param>
        /// <param name="data">用户数据</param>
        /// <param name="finishCallback">触发器链条执行完后回调，可留空</param>
        public OneByOneTriggerData(string triggerId, object data = null, Action finishCallback = null)
        {
            this.triggerId = triggerId;
            this.data = data;
            if (finishCallback != null)
            {
                this.finishCallback += finishCallback;
            }
        }
        
        /// <summary>
        /// 中断执行
        /// </summary>
        public void Interupt()
        {
            isInterupted = true;
            OnFinishCallback();
        }

        /// <summary>
        /// 检查是否有不可中断的事件还未执行完
        /// </summary>
        public bool CheckUnbreakable()
        {
            return GameBase.Instance.GetModule<OneByOneManager>().CheckUnbreakable(this);
        }

        #region 内部使用
        
        /// <summary>
        /// 下一个事件池索引，内部使用
        /// </summary>
        internal int nextPoolIndex;
        
        /// <summary>
        /// 下一个事件索引，内部使用
        /// </summary>
        internal int nextEventIndex;
        
        /// <summary>
        /// 等待事件数量，内部使用
        /// </summary>
        internal int waitingEvents;

        /// <summary>
        /// 是否在执行不可中断事件，内部使用
        /// </summary>
        internal bool isUnbreakable;
        
        /// <summary>
        /// 是否中断执行，内部使用
        /// </summary>
        internal bool isInterupted;

        /// <summary>
        /// 触发下一个事件，当前事件执行完毕后调用
        /// </summary>
        internal void TriggerNextEvent()
        {
            GameBase.Instance.GetModule<OneByOneManager>().TriggerEvent(this);
        }
        
        #endregion
    }
}