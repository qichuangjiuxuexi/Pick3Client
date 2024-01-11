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
        /// 是否有目标对象
        /// </summary>
        private byte hasTarget;
        
        /// <summary>
        /// Unity目标对象，用于监控目标对象是否被销毁
        /// </summary>
        private Object targetObject;
        
        /// <summary>
        /// Module目标对象，用于监控目标对象是否被销毁
        /// </summary>
        private ModuleBase targetModule;

        public EventListener(Type eventType, int priority, Action<IEvent, Action> callback, object callbackObj, object callbackTarget)
        {
            this.eventType = eventType;
            this.callback = callback;
            this.callbackObj = callbackObj;
            this.priority = priority;
            switch (callbackTarget)
            {
                case Object obj:
                    hasTarget = 1;
                    targetObject = obj;
                    break;
                case ModuleBase module:
                    hasTarget = 2;
                    targetModule = module;
                    break;
            }
        }
        
        public void Invoke(IEvent e, Action callback)
        {
            //目标对象被销毁，不执行
            if (hasTarget == 1 && targetObject == null || hasTarget == 2 && !targetModule.IsModuleInited)
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