using System;
using AppBase;
using AppBase.Event;
using Cysharp.Threading.Tasks;

/// <summary>
/// 事件管理器UniTask扩展
/// </summary>
public static class EventUniTaskExtension
{
    /// <summary>
    /// 注册UniTask异步事件，UniTask回调执行完才调用下一个监听者
    /// <param name="callback">UniTask回调，UniTask回调执行完才调用下一个监听者</param>
    /// <param name="priority">优先级，越小越优先</param>
    /// <typeparam name="T">事件类型</typeparam>
    /// </summary>
    public static EventListener Subscribe<T>(this EventManager @this, Func<T, UniTask> callback, int priority = 0) where T : IEvent
    {
        if (callback == null) return null;
        var listener = new EventListener(typeof(T), priority, (e, c) =>
        {
            callback.Invoke((T)e).ContinueWith(c).Forget();
        }, callback, callback.Target);
        return @this.Subscribe(listener);
    }

    /// <summary>
    /// 解注册事件
    /// </summary>
    /// <param name="callback">消息监听回调</param>
    /// <returns>被解注册的消息监听器</returns>
    public static EventListener Unsubscribe<T>(this EventManager @this, Func<T, UniTask> callback) where T : IEvent
    {
        return @this.Unsubscribe(@this.FindEventListener(typeof(T), callback));
    }

    /// <summary>
    /// 注册UniTask异步事件，UniTask回调执行完才调用下一个监听者
    /// <param name="callback">UniTask回调，UniTask回调执行完才调用下一个监听者</param>
    /// <param name="priority">优先级，越小越优先</param>
    /// <typeparam name="T">事件类型</typeparam>
    /// </summary>
    public static IEventObserver Subscribe<T>(this IEventObserver @this, Func<T, UniTask> callback, int priority = 0) where T : IEvent
    {
        var listener = GameBase.Instance.GetModule<EventManager>().Subscribe<T>(callback, priority);
        if (listener != null)
        {
            @this.listeners.Add(listener);
        }
        return @this;
    }

    /// <summary>
    /// 解注册事件
    /// <param name="callback">消息监听回调</param>
    /// </summary>
    public static bool Unsubscribe<T>(this IEventObserver @this, Func<T, UniTask> callback) where T : IEvent
    {
        var listener = GameBase.Instance.GetModule<EventManager>().Unsubscribe(callback);
        if (listener != null)
        {
            @this.listeners.Remove(listener);
        }
        return listener != null;
    }
}
