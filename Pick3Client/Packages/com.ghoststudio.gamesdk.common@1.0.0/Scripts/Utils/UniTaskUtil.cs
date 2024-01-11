using System;
using Cysharp.Threading.Tasks;

/// <summary>
/// UniTask工具类
/// </summary>
public static class UniTaskUtil
{
    /// <summary>
    /// 将一个UniTask转换为一个回调
    /// </summary>
    public static void Callback(this UniTaskCompletionSource source)
    {
        source.TrySetResult();
    }
    
    /// <summary>
    /// 将一个UniTask转换为一个回调
    /// </summary>
    public static void Callback<R>(this UniTaskCompletionSource<R> source, R result)
    {
        source.TrySetResult(result);
    }
    
    /// <summary>
    /// 创建一个UniTask，并将其转换为一个回调
    /// </summary>
    public static Action CreateCallback(out UniTask task)
    {
        var source = new UniTaskCompletionSource();
        task = source.Task;
        return source.Callback;
    }
    
    /// <summary>
    /// 创建一个UniTask，并将其转换为一个回调
    /// </summary>
    public static Action<R> CreateCallback<R>(out UniTask<R> task)
    {
        var source = new UniTaskCompletionSource<R>();
        task = source.Task;
        return source.Callback;
    }

    /// <summary>
    /// 将异步方法转换为UniTask
    /// </summary>
    public static UniTask.Awaiter GetAwaiter(this Action<Action> callback)
    {
        return InvokeAsync(callback).GetAwaiter();
    }

    /// <summary>
    /// 调用一个异步方法，并转换成UniTask
    /// </summary>
    public static async UniTask InvokeAsync(this Action<Action> callback)
    {
        if (callback == null) return;
        callback.Invoke(CreateCallback(out var task));
        await task;
    }

    /// <summary>
    /// 调用一个异步方法，并转换成UniTask
    /// </summary>
    public static async UniTask InvokeAsync<T>(this Action<T, Action> callback, T param)
    {
        if (callback == null) return;
        callback.Invoke(param, CreateCallback(out var task));
        await task;
    }

    /// <summary>
    /// 调用一个异步方法，并转换成UniTask
    /// </summary>
    public static async UniTask InvokeAsync<T1, T2>(this Action<T1, T2, Action> callback, T1 param1, T2 param2)
    {
        if (callback == null) return;
        callback.Invoke(param1, param2, CreateCallback(out var task));
        await task;
    }

    /// <summary>
    /// 调用一个异步方法，并转换成UniTask
    /// </summary>
    public static async UniTask InvokeAsync<T1, T2, T3>(this Action<T1, T2, T3, Action> callback, T1 param1, T2 param2, T3 param3)
    {
        if (callback == null) return;
        callback.Invoke(param1, param2, param3, CreateCallback(out var task));
        await task;
    }

    /// <summary>
    /// 调用一个异步方法，并转换成UniTask
    /// </summary>
    public static async UniTask InvokeAsync<T1, T2, T3, T4>(this Action<T1, T2, T3, T4, Action> callback, T1 param1, T2 param2, T3 param3, T4 param4)
    {
        if (callback == null) return;
        callback.Invoke(param1, param2, param3, param4, CreateCallback(out var task));
        await task;
    }
    
    /// <summary>
    /// 调用一个异步方法，并转换成UniTask
    /// </summary>
    public static async UniTask<R> InvokeAsync<R>(this Action<Action<R>> callback)
    {
        if (callback == null) return default;
        callback.Invoke(CreateCallback<R>(out var task));
        return await task;
    }
    
    /// <summary>
    /// 调用一个异步方法，并转换成UniTask
    /// </summary>
    public static async UniTask<R> InvokeAsync<T, R>(this Action<T, Action<R>> callback, T param)
    {
        if (callback == null) return default;
        callback.Invoke(param, CreateCallback<R>(out var task));
        return await task;
    }

    /// <summary>
    /// 调用一个异步方法，并转换成UniTask
    /// </summary>
    public static async UniTask<R> InvokeAsync<T1, T2, R>(this Action<T1, T2, Action<R>> callback, T1 param1, T2 param2)
    {
        if (callback == null) return default;
        callback.Invoke(param1, param2, CreateCallback<R>(out var task));
        return await task;
    }

    /// <summary>
    /// 调用一个异步方法，并转换成UniTask
    /// </summary>
    public static async UniTask<R> InvokeAsync<T1, T2, T3, R>(this Action<T1, T2, T3, Action<R>> callback, T1 param1, T2 param2, T3 param3)
    {
        if (callback == null) return default;
        callback.Invoke(param1, param2, param3, CreateCallback<R>(out var task));
        return await task;
    }

    /// <summary>
    /// 调用一个异步方法，并转换成UniTask
    /// </summary>
    public static async UniTask<R> InvokeAsync<T1, T2, T3, T4, R>(this Action<T1, T2, T3, T4, Action<R>> callback, T1 param1, T2 param2, T3 param3, T4 param4)
    {
        if (callback == null) return default;
        callback.Invoke(param1, param2, param3, param4, CreateCallback<R>(out var task));
        return await task;
    }
}