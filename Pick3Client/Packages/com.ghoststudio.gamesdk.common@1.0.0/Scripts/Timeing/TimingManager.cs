using System;
using System.Collections;
using System.Collections.Generic;
using AppBase.Event;
using AppBase.Module;
using UnityEngine;

namespace AppBase.Timing
{
    /// <summary>
    /// 游戏时机管理器
    /// </summary>
    public class TimingManager : MonoModule
    {
        private List<IUpdateFrame> updateFrameList = new();
        private List<IUpdateSecond> updateSecondList = new();
        private float lastUpdateTime;
        private TimingRuntimeComponent runtime;

        /// <summary>
        /// 服务器时间
        /// </summary>
        private DateTime serverTime;
        private float serverTimeOffset = float.NaN;

        protected override void OnInit()
        {
            base.OnInit();
            runtime = GameObject.GetComponent<TimingRuntimeComponent>();
            if (runtime == null) runtime = GameObject.AddComponent<TimingRuntimeComponent>();
            runtime.Init(this);
        }
        
        /// <summary>
        /// 注册每帧更新的管理器
        /// </summary>
        public void SubscribeFrameUpdate(IUpdateFrame updateFrame)
        {
            if (updateFrame == null) return;
            if (updateFrameList.Contains(updateFrame))
            {
                Debugger.LogError(TAG, "Register updateFrame already exist");
                return;
            }
            updateFrameList.Add(updateFrame);
        }
        
        /// <summary>
        /// 注册每秒更新的管理器
        /// </summary>
        public void SubscribeSecondUpdate(IUpdateSecond updateSecond)
        {
            if (updateSecond == null) return;
            if (updateSecondList.Contains(updateSecond))
            {
                Debugger.LogError(TAG, "Register updateSecond already exist");
                return;
            }
            updateSecondList.Add(updateSecond);
        }
        
        /// <summary>
        /// 取消注册每帧更新的管理器
        /// </summary>
        public void UnsubscribeFrameUpdate(IUpdateFrame updateFrame)
        {
            if (updateFrame == null) return;
            if (!updateFrameList.Contains(updateFrame)) return;
            updateFrameList.Remove(updateFrame);
        }
        
        /// <summary>
        /// 取消注册每秒更新的管理器
        /// </summary>
        public void UnsubscribeSecondUpdate(IUpdateSecond updateSecond)
        {
            if (updateSecond == null) return;
            if (!updateSecondList.Contains(updateSecond)) return;
            updateSecondList.Remove(updateSecond);
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();
            updateFrameList.Clear();
            updateSecondList.Clear();
        }

        public void Update()
        {
            for (int i = 0; i < updateFrameList.Count; i++)
            {
                updateFrameList[i].Update();
            }

            if (updateSecondList.Count != 0)
            {
                var time = Time.realtimeSinceStartup;
                if (time - lastUpdateTime >= 1f)
                {
                    lastUpdateTime = time;
                    for (int i = 0; i < updateSecondList.Count; i++)
                    {
                        if (updateSecondList[i] != null)
                        {
                            updateSecondList[i].OnUpdateSecond();
                        }
                    }
                }
            }
        }
        
        public void OnApplicationPause(bool pauseStatus)
        {
            if (pauseStatus)
            {
                GameBase.Instance.GetModule<EventManager>().Broadcast(new EventOnGamePause());
            }
            else
            {
                GameBase.Instance.GetModule<EventManager>().Broadcast(new EventOnGameFocus());
            }
        }
        
        public void OnApplicationQuit()
        {
            GameBase.Instance.GetModule<EventManager>().Broadcast(new EventOnGameQuit());
        }
        
        /// <summary>
        /// 全局的延迟回调
        /// </summary>
        public Coroutine GlobalDelayCall(float delay, Action callBack, bool isIgnoreTimeScale = true)
        {
            if (callBack == null) return null;
            var iEnumerator = _delayCallBack(delay, callBack, isIgnoreTimeScale);
            return runtime.StartCoroutine(iEnumerator);
        }
    
        private static IEnumerator _delayCallBack(float delay, Action callBack, bool isIgnoreTimeScale)
        {
            if (isIgnoreTimeScale)
            {
                yield return new WaitForSecondsRealtime(delay);
            }
            else
            {
                yield return new WaitForSeconds(delay);
            }
            callBack?.Invoke();
        }
        
        /// <summary>
        /// 全局的延迟帧数回调
        /// </summary>
        public Coroutine GlobalDelayCallFrame(int delay, Action callBack)
        {
            if (callBack == null) return null;
            var iEnumerator = _delayCallBackFrame(delay, callBack);
            return runtime.StartCoroutine(iEnumerator);
        }
        
        private static IEnumerator _delayCallBackFrame(int delay, Action callBack)
        {
            while (delay > 0)
            {
                delay--;
                yield return null;
            }
            callBack?.Invoke();
        }

        /// <summary>
        /// 运行协程，执行完成后回调
        /// </summary>
        /// <param name="enumerator">协程对象</param>
        /// <param name="callback">执行完成后的回调</param>
        public Coroutine StartCoroutine(IEnumerator enumerator, Action callback = null)
        {
            if (enumerator == null)
            {
                return null;
            }
            if (callback == null)
            {
                return runtime.StartCoroutine(enumerator);
            }
            else
            {
                return runtime.StartCoroutine(_InvokeCoroutine(enumerator, callback));
            }
        }
        
        private static IEnumerator _InvokeCoroutine(IEnumerator enumerator, Action callback)
        {
            yield return enumerator;
            callback?.Invoke();
        }
        
        /// <summary>
        /// 停止全局协程
        /// </summary>
        /// <param name="coroutine">协程对象</param>
        public void StopCoroutine(Coroutine coroutine)
        {
            if (coroutine == null) return;
            runtime.StopCoroutine(coroutine);
        }

        /// <summary>
        /// 获取服务器时间（UTC），如果未登录，则返回null
        /// </summary>
        public DateTime? ServerTime
        {
            get
            {
                if (float.IsNaN(serverTimeOffset)) return null;
                return serverTime.AddSeconds(Time.realtimeSinceStartup - serverTimeOffset);
            }
            set
            {
                if (value != null)
                {
                    serverTimeOffset = Time.realtimeSinceStartup;
                    serverTime = value.Value;
                }
            }
        }
        
        /// <summary>
        /// 获取服务器时间的时间戳秒数
        /// </summary>
        public long ServerTimeSeconds
        {
            get
            {
                if (ServerTime != null)
                {
                    return ServerTime.Value.Ticks / 10000000L - 62135596800L;
                }

                return 0;
            }
        }
    }
}
