using System;
using AppBase.Module;
using Object = UnityEngine.Object;

namespace AppBase.Event
{
    /// <summary>
    /// 事件监听器
    /// </summary>
    public class EventListener
    {
        /// <summary>
        /// 优先级，越小越优先执行
        /// </summary>
        public int priority;

        /// <summary>
        /// 事件类型
        /// </summary>
        public Type eventType;
        
        /// <summary>
        /// 通用回调
        /// </summary>
        public Action<IEvent, Action> callback;
        
        /// <summary>
        /// 原始回调对象
        /// </summary>
        public object callbackObj;

        /// <summary>
        /// 目标对象，用于监控目标对象是否被销毁
        /// </summary>
        private object callbackTarget;

        public EventListener(Type eventType, int priority, Action<IEvent, Action> callback, object callbackObj, object callbackTarget)
        {
            this.eventType = eventType;
            this.callback = callback;
            this.callbackObj = callbackObj;
            this.priority = priority;
            this.callbackTarget = callbackTarget;
        }
        
        public void Invoke(IEvent e, Action callback)
        {
            //目标对象被销毁，不执行
            if (callbackTarget is Object unityObj && unityObj == null || callbackTarget is ModuleBase moduleBase && !moduleBase.IsModuleInited)
            {
                //目标对象被销毁，删除监听
                GameBase.Instance.GetModule<EventManager>().Unsubscribe(this);
                callback?.Invoke();
                return;
            }
            this.callback.Invoke(e, callback);
        }
    }
}