using System;
using System.Collections;
using System.Collections.Generic;
using AppBase.Module;

namespace AppBase.Event
{
    /// <summary>
    /// 事件通知的子模块
    /// </summary>
    public class EventModule : ModuleBase, IEventObserver
    {
        public List<EventListener> listeners { get; } = new();

        /// <summary>
        /// 注册同步事件
        /// </summary>
        /// <param name="callback">事件通知回调</param>
        /// <param name="priority">优先级，越小越优先</param>
        /// <typeparam name="T">事件类型</typeparam>
        public IEventObserver Subscribe<T>(Action<T> callback, int priority = 0) where T : IEvent
        {
            var listener = GameBase.Instance.GetModule<EventManager>().Subscribe<T>(callback, priority);
            if (listener != null)
            {
                listeners.Add(listener);
            }
            return this;
        }

        /// <summary>
        /// 注册异步事件，等callback回调被执行才调用下一个监听者
        /// </summary>
        /// <param name="callback">异步的事件通知回调，Action被执行才调用下一个监听者</param>
        /// <param name="priority">优先级，越小越优先</param>
        /// <typeparam name="T">事件类型</typeparam>
        [Obsolete("Use Subscribe<T>(Func<T, UniTask> callback, int priority = 0) instead")]
        public IEventObserver Subscribe<T>(Action<T, Action> callback, int priority = 0) where T : IEvent
        {
            var listener = GameBase.Instance.GetModule<EventManager>().Subscribe<T>(callback, priority);
            if (listener != null)
            {
                listeners.Add(listener);
            }
            return this;
        }
        
        /// <summary>
        /// 注册协程事件，协程执行结束才调用下一个监听者
        /// </summary>
        /// <param name="callback">协程回调，协程执行结束才调用下一个监听者</param>
        /// <param name="priority">优先级，越小越优先</param>
        /// <typeparam name="T">事件类型</typeparam>
        public IEventObserver Subscribe<T>(Func<T, IEnumerator> callback, int priority = 0) where T : IEvent
        {
            var listener = GameBase.Instance.GetModule<EventManager>().Subscribe<T>(callback, priority);
            if (listener != null)
            {
                listeners.Add(listener);
            }
            return this;
        }

        /// <summary>
        /// 解注册事件
        /// </summary>
        /// <param name="callback">消息监听回调</param>
        public bool Unsubscribe<T>(Action<T> callback) where T : IEvent
        {
            var listener = GameBase.Instance.GetModule<EventManager>().Unsubscribe(callback);
            if (listener != null)
            {
                listeners.Remove(listener);
            }
            return listener != null;
        }

        /// <summary>
        /// 解注册事件
        /// </summary>
        /// <param name="callback">消息监听回调</param>
        public bool Unsubscribe<T>(Action<T, Action> callback) where T : IEvent
        {
            var listener = GameBase.Instance.GetModule<EventManager>().Unsubscribe(callback);
            if (listener != null)
            {
                listeners.Remove(listener);
            }
            return listener != null;
        }
    
        /// <summary>
        /// 移除所有监听器
        /// </summary>
        public void UnregisterEvents()
        {
            for (int i = 0; i < listeners.Count; i++)
            {
                GameBase.Instance.GetModule<EventManager>().Unsubscribe(listeners[i]);
            }
            listeners.Clear();
        }

        protected override void OnDestroy()
        {
            UnregisterEvents();
        }
    }
}