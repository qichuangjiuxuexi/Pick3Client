using System;
using System.Collections;
using System.Collections.Generic;
using AppBase.Timing;

namespace AppBase.Utils
{
    /// <summary>
    /// 回调链工具
    /// </summary>
    public static class FlowUtil
    {
        /// <summary>
        /// 创建回调链
        /// </summary>
        public static List<Action<Action>> Create(params Action<Action>[] list) => new List<Action<Action>>(list);

        /// <summary>
        /// 调用回调链
        /// </summary>
        public static void Invoke(this IList<Action<Action>> list, Action callBack = null, int index = 0)
        {
            if (list != null && list.Count > 0 && index < list.Count)
            {
                if (list[index] != null)
                {
                    list[index].Invoke(() =>
                    {
                        Invoke(list, callBack, index + 1);
                    });
                }
                else
                {
                    Invoke(list, callBack, index + 1);
                }
            }
            else
            {
                callBack?.Invoke();
            }
        }
        
        /// <summary>
        /// 添加异步方法
        /// </summary>
        public static void Add(this IList<Action<Action>> list, Action action) => list?.Add(Wrap(action));
        
        /// <summary>
        /// 添加协程方法
        /// </summary>
        public static void Add(this IList<Action<Action>> list, Func<IEnumerator> action) => list?.Add(Wrap(action));
        
        /// <summary>
        /// 包裹同步方法为异步方法
        /// </summary>
        public static Action<Action> Wrap(Action action) => next => { action(); next(); };

        /// <summary>
        /// 包裹协程方法为异步方法
        /// </summary>
        public static Action<Action> Wrap(Func<IEnumerator> action) => next => GameBase.Instance.GetModule<TimingManager>().StartCoroutine(action(), next);
    }
}