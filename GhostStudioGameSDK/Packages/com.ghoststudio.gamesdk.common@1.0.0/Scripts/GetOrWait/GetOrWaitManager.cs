using System;
using System.Collections.Generic;
using AppBase.Module;
using AppBase.Utils;

namespace AppBase.GetOrWait
{
    /// <summary>
    /// 顾名思义，本模块的作用就是让需要某些某块的某些数据的地方，通过此模块作为媒介，让它“要么拿、要么待会准备好了再拿”的地方。比如AIHelp的推送功能依赖FireBase的推送Token这个数据，但是FireBase拿到Token的时机却不是固定的。AIHelp初始化时如果直接向FireBase模块要，既产生了耦合，又不一定能拿到，因为Firebase初始化需要时间。
    /// 此时Firebase模块可以在拿到Token后存储在本模块中，AIHelp可以通过本模块来拿Firebase的Token，假如Token已经准备好了，就直接返回，否则就注册一个回调，等Token准备好后通过回调再回传给AIHelp模块
    /// </summary>
    public class GetOrWaitManager : ModuleBase
    {
        private Dictionary<string, object> dataMap;
        Dictionary<string,List<Action<string, object>>> keyCallBackListMap;

        /// <summary>
        /// 数据提供者能提供对应的key的值时，调用此方法
        /// </summary>
        /// <param name="key"></param>
        /// <param name="val"></param>
        public void SetData(string key,object val)
        {
            dataMap ??= new Dictionary<string, object>();
            keyCallBackListMap ??= new Dictionary<string, List<Action<string, object>>>();
            if (dataMap.ContainsKey(key))
            {
                Debugger.LogDWarning($"key already set:{key},old value is {dataMap[key]},new value is {val}");
            }
            //存储数据
            dataMap[key] = val;
            if (keyCallBackListMap.TryGetValue(key, out var funcs))
            {
                for (int i = 0; i < funcs.Count; i++)
                {
                    if (dataMap.TryGetValue(key,out object data))
                    {
                        funcs[i]?.Invoke(key,data);
                    }
                }
                //因为已经回传给调用方了，可以直接拿到数据了，所以不需要这些回调了。
                funcs.Clear();
            }
        }

        /// <summary>
        /// 需要用到某数据提供者的某些数据时，通过此方法，要么能拿到已经准备好的数据，要么传来一个回调，等需要的数据准备好时，通过回调传给调用方
        /// </summary>
        /// <param name="key">属性对应的key</param>
        /// <param name="callBack">当数据没准备好时，通过此回调将数据的key和data告诉调用方</param>
        /// <returns></returns>
        public object GetOrWaitCallBack(string key,Action<string,object> callBack)
        {
            dataMap ??= new Dictionary<string, object>();
            //已经有数据了，直接返回
            if (dataMap.TryGetValue(key, out var val))
            {
                return val;
            }

            if (callBack == null)
            {
                return null;
            }
            keyCallBackListMap ??= new Dictionary<string, List<Action<string, object>>>();
            if (keyCallBackListMap.TryGetValue(key, out var funList))
            {
                if (!funList.Contains(callBack))
                {
                    funList.Add(callBack);
                }
                return null;
            }

            keyCallBackListMap[key] = new List<Action<string, object>>();
            keyCallBackListMap[key].Add(callBack);
            return null;
        }
    }
}