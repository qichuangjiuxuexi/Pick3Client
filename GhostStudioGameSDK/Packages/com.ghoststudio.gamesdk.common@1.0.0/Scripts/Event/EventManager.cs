using System;
using System.Collections;
using System.Collections.Generic;
using AppBase.Module;
using AppBase.Timing;

namespace AppBase.Event
{
    /// <summary>
    /// 事件管理器
    /// </summary>
    public class EventManager : ModuleBase
    {
        private Dictionary<Type, List<EventListener>> m_observerMap = new();

        /// <summary>
        /// 注册同步事件
        /// </summary>
        /// <param name="callback">事件通知回调</param>
        /// <param name="priority">优先级，越小越优先</param>
        /// <typeparam name="T">事件类型</typeparam>
        /// <returns>事件监听器</returns>
        public EventListener Subscribe<T>(Action<T> callback, int priority = 0) where T : IEvent
        {
            if (callback == null) return null;
            var listener = new EventListener(typeof(T), priority, (e, c) =>
            {
                callback.Invoke((T)e);
                c.Invoke();
            }, callback, callback.Target);
            return Subscribe(listener);
        }

        /// <summary>
        /// 注册异步事件，等callback回调被执行才调用下一个监听者
        /// </summary>
        /// <param name="callback">异步回调，Action参数回调被执行才调用下一个监听者</param>
        /// <param name="priority">优先级，越小越优先</param>
        /// <typeparam name="T">事件类型</typeparam>
        /// <returns>事件监听器</returns>
        [Obsolete("Use Subscribe<T>(Func<T, UniTask> callback, int priority = 0) instead")]
        public EventListener Subscribe<T>(Action<T, Action> callback, int priority = 0) where T : IEvent
        {
            if (callback == null) return null;
            var listener = new EventListener(typeof(T), priority, (e, c) =>
            {
                callback.Invoke((T)e, c);
            }, callback, callback.Target);
            return Subscribe(listener);
        }
        
        /// <summary>
        /// 注册协程事件，协程执行结束才调用下一个监听者
        /// </summary>
        /// <param name="callback">协程回调，协程执行结束才调用下一个监听者</param>
        /// <param name="priority">优先级，越小越优先</param>
        /// <typeparam name="T">事件类型</typeparam>
        /// <returns>事件监听器</returns>
        public EventListener Subscribe<T>(Func<T, IEnumerator> callback, int priority = 0) where T : IEvent
        {
            if (callback == null) return null;
            var listener = new EventListener(typeof(T), priority, (e, c) =>
            {
                GameBase.Instance.GetModule<TimingManager>().StartCoroutine(callback.Invoke((T)e), c);
            }, callback, callback.Target);
            return Subscribe(listener);
        }
        
        /// <summary>
        /// 注册事件
        /// </summary>
        /// <param name="listener">时间监听器</param>
        /// <returns>事件监听器</returns>
        public EventListener Subscribe(EventListener listener)
        {
            if (listener == null) return null;
            if (m_observerMap.TryGetValue(listener.eventType, out var list))
            {
                var oldListener = list.Find(x => x.callbackObj.Equals(listener.callbackObj));
                if (oldListener == null)
                {
                    var index = list.FindIndex(x => x.priority <= listener.priority);
                    list.Insert(Math.Max(index, 0), listener);
                }
                else
                {
                    Debugger.LogWarning(TAG, $"Subscribe callback already exist: {listener.eventType}");
                    return null;
                }
            }
            else
            {
                list = new List<EventListener> { listener };
                m_observerMap.Add(listener.eventType, list);
            }
            return listener;
        }
        
        /// <summary>
        /// 查找事件监听器
        /// </summary>
        /// <param name="type">事件类型</param>
        /// <param name="callbackObj">回调函数</param>
        /// <returns>事件监听器</returns>
        public EventListener FindEventListener(Type type, object callbackObj)
        {
            if (callbackObj == null) return null;
            if (m_observerMap.TryGetValue(type, out var list))
            {
                return list.Find(x => x.callbackObj.Equals(callbackObj));
            }
            return null;
        }

        /// <summary>
        /// 解注册事件
        /// </summary>
        /// <param name="listener">消息监听器</param>
        public EventListener Unsubscribe(EventListener listener)
        {
            if (listener == null) return null;
            if (m_observerMap.TryGetValue(listener.eventType, out var list))
            {
                list.Remove(listener);
                if (list.Count == 0)
                {
                    m_observerMap.Remove(listener.eventType);
                }
            }
            return listener;
        }

        /// <summary>
        /// 解注册事件
        /// </summary>
        /// <param name="callback">消息监听回调</param>
        /// <returns>被解注册的消息监听器</returns>
        public EventListener Unsubscribe<T>(Action<T> callback) where T : IEvent
        {
            return Unsubscribe(FindEventListener(typeof(T), callback));
        }

        /// <summary>
        /// 解注册事件
        /// </summary>
        /// <param name="callback">消息监听回调</param>
        /// <returns>被解注册的消息监听器</returns>
        public EventListener Unsubscribe<T>(Action<T, Action> callback) where T : IEvent
        {
            return Unsubscribe(FindEventListener(typeof(T), callback));
        }
        
        /// <summary>
        /// 解注册事件
        /// </summary>
        /// <param name="callback">消息监听回调</param>
        /// <returns>被解注册的消息监听器</returns>
        public EventListener Unsubscribe<T>(Func<T, IEnumerator> callback) where T : IEvent
        {
            return Unsubscribe(FindEventListener(typeof(T), callback));
        }

        /// <summary>
        /// 分发事件
        /// </summary>
        /// <param name="eventData">事件消息</param>
        /// <param name="callback">全部分发完成后的回调</param>
        public void Broadcast<T>(T eventData = default, Action callback = null) where T : IEvent
        {
            var type = typeof(T);
            if (m_observerMap.TryGetValue(type, out var list))
            {
                if (eventData is IParallelEvent)
                {
                    //并发事件
                    InvokeParallelListener(list, eventData, callback);
                }
                else
                {
                    //串行事件
                    InvokeListener(list, eventData, callback, list.Count - 1);
                }
            }
            else
            {
                callback?.Invoke();
            }
        }
        
        /// <summary>
        /// 分发事件
        /// </summary>
        /// <param name="eventData">事件消息</param>
        /// <param name="callback">全部分发完成后的回调</param>
        public void Broadcast(IEvent eventData, Action callback = null)
        {
            if (eventData == null) return;
            var type = eventData.GetType();
            if (m_observerMap.TryGetValue(type, out var list))
            {
                if (eventData is IParallelEvent)
                {
                    //并发事件
                    InvokeParallelListener(list, eventData, callback);
                }
                else
                {
                    //串行事件
                    InvokeListener(list, eventData, callback, list.Count - 1);
                }
            }
            else
            {
                callback?.Invoke();
            }
        }

        /// <summary>
        /// 调用串行事件
        /// </summary>
        private void InvokeListener(List<EventListener> list, IEvent eventData, Action callback, int index)
        {
            if (list == null || index < 0 || index >= list.Count)
            {
                callback?.Invoke();
                return;
            }
            var listener = list[index];
            var isInvoked = false;
            listener.Invoke(eventData, () =>
            {
                //当回调两次及以上，报错提示
                if (isInvoked)
                {
                    Debugger.LogError(TAG, $"callback twice: type = {eventData.GetType().Name}, listener = {listener.callbackObj}");
                    return;
                }
                isInvoked = true;
                //事件被取消，不再向后传播
                if (eventData is CancelableEvent cancelableEvent && cancelableEvent.IsCanceled)
                {
                    Debugger.Log(TAG, $"event cancelled = {eventData.GetType().Name}, listener = {listener.callbackObj}");
                    return;
                }
                InvokeListener(list, eventData, callback, index - 1);
            });
        }

        /// <summary>
        /// 调用并发事件
        /// </summary>
        private void InvokeParallelListener(List<EventListener> list, IEvent eventData, Action callback)
        {
            var totalCount = list.Count;
            var curCount = 0;
            for (int i = totalCount - 1; i >= 0; i--)
            {
                var listener = list[i];
                var isInvoked = false;
                listener.Invoke(eventData, () =>
                {
                    //当回调两次及以上，报错提示
                    if (isInvoked)
                    {
                        Debugger.LogError(TAG, $"callback twice: type = {eventData.GetType().Name}, listener = {listener.callbackObj}");
                        return;
                    }
                    isInvoked = true;
                    curCount++;
                    if (curCount >= totalCount)
                    {
                        callback?.Invoke();
                    }
                });
            }
        }

        /// <summary>
        /// 清理所有事件监听器
        /// </summary>
        protected override void OnDestroy()
        {
            m_observerMap.Clear();
        }
    }
}