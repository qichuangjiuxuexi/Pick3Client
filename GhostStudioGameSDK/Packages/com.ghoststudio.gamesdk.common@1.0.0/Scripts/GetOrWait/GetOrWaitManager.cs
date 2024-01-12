using System;
using System.Collections.Generic;
using AppBase.Module;
using AppBase.Utils;
using Object = UnityEngine.Object;

namespace AppBase.GetOrWait
{
    /// <summary>
    /// 顾名思义，本模块的作用就是让需要某些某块的某些数据的地方，通过此模块作为媒介，让它“要么拿、要么待会准备好了再拿”的地方。比如AIHelp的推送功能依赖FireBase的推送Token这个数据，但是FireBase拿到Token的时机却不是固定的。AIHelp初始化时如果直接向FireBase模块要，既产生了耦合，又不一定能拿到，因为Firebase初始化需要时间。
    /// 此时Firebase模块可以在拿到Token后存储在本模块中，AIHelp可以通过本模块来拿Firebase的Token，假如Token已经准备好了，就直接返回，否则就注册一个回调，等Token准备好后通过回调再回传给AIHelp模块
    /// </summary>
    public class GetOrWaitManager : ModuleBase
    {
        private Dictionary<string, object> dataMap = new();
        private Dictionary<string, List<GetOrWaitListener>> listeners = new();

        /// <summary>
        /// 数据提供者能提供对应的key的值时，调用此方法
        /// </summary>
        public void SetData(string key, object val)
        {
            if (dataMap.TryGetValue(key, out var value))
            {
                Debugger.LogWarning(TAG, $"key already set: {key}, old value is {value}, new value is {val}");
            }
            //存储数据
            dataMap[key] = val;
            //通知等待者
            if (listeners.Remove(key, out var list))
            {
                list.ForEach(f => f.Invoke(val));
                list.Clear();
            }
        }

        /// <summary>
        /// 需要用到某数据提供者的某些数据时，通过此方法，要么能拿到已经准备好的数据，要么传来一个回调，等需要的数据准备好时，通过回调传给调用方
        /// </summary>
        /// <param name="key">属性对应的key</param>
        /// <param name="callBack">当数据没准备好时，通过此回调将数据的key和data告诉调用方</param>
        /// <returns>当数据准备好时，可同步返回</returns>
        public T GetOrWait<T>(string key, Action<T> callBack = null)
        {
            if (dataMap.TryGetValue(key, out var val))
            {
                var value = (T)Convert.ChangeType(val, typeof(T));
                callBack?.Invoke(value);
                return value;
            }
            if (callBack == null)
            {
                return default;
            }
            if (!listeners.TryGetValue(key, out var list))
            {
                list = new List<GetOrWaitListener>();
                listeners[key] = list;
            }
            else if (list.Find(l => l.callbackObj.Equals(callBack)) != null)
            {
                Debugger.LogWarning(TAG, $"key:{key} already has callback:{callBack}");
                return default;
            }
            var listener = new GetOrWaitListener(obj => callBack.Invoke((T)Convert.ChangeType(obj, typeof(T))), callBack, callBack.Target);
            list.Add(listener);
            return default;
        }

        /// <summary>
        /// 解注册
        /// </summary>
        public bool Unsubscribe<T>(string key, Action<T> callBack)
        {
            if (!listeners.TryGetValue(key, out var list)) return false;
            var p = list.FindIndex(l => l.callbackObj.Equals(callBack));
            if (p < 0) return false;
            list.RemoveAt(p);
            if (list.Count == 0) listeners.Remove(key);
            return true;
        }

        private class GetOrWaitListener
        {
            public Action<object> callback;
            public object callbackObj;
            public object callbackTarget;
            
            public GetOrWaitListener(Action<object> callback, object callbackObj, object callbackTarget)
            {
                this.callback = callback;
                this.callbackObj = callbackObj;
                this.callbackTarget = callbackTarget;
            }
            
            public void Invoke(object obj)
            {
                if (callbackTarget is Object unityObj && unityObj == null) return;
                if (callbackTarget is ModuleBase moduleBase && !moduleBase.IsModuleInited) return;
                callback.Invoke(obj);
            }
        }
    }
}