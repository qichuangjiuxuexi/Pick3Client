using System;
using System.Collections.Generic;

namespace WordGame.Utils.Timer
{
    /// <summary>
    /// 时间刷新控制器
    /// </summary>
    public class TimeUpdateMgr : MonoSingleton<TimeUpdateMgr>
    {
        
        /// <summary>
        /// 已校验网络时间状态
        /// </summary>
        public bool getNetTimeState = false;

        /// <summary>
        /// 帧刷新数组
        /// </summary>
        private event Action frameUpdateArray;

        /// <summary>
        /// 半秒刷新数组
        /// </summary>
        private event Action halfSecondUpdateArray;

        /// <summary>
        /// 秒刷新数组
        /// </summary>
        private event Action secondUpdateArray;

        /// <summary>
        /// 分钟刷新数组
        /// </summary>
        private event Action minuteUpdateArray;

        /// <summary>
        /// 缓存半秒时间
        /// </summary>
        private DateTime tempHalfSecondTime;

        /// <summary>
        /// 缓存秒时间
        /// </summary>
        private DateTime tempSecondTime;

        /// <summary>
        /// 缓存分钟时间
        /// </summary>
        private DateTime tempMinuteTime;

        /// <summary>
        /// 半秒计算时间片
        /// </summary>
        private TimeSpan tempHalfSecondSpan;

        /// <summary>
        /// 秒计算时间片
        /// </summary>
        private TimeSpan tempSecondSpan;

        /// <summary>
        /// 分钟计算时间片
        /// </summary>
        private TimeSpan tempMinuteSpan;

        /// <summary>
        /// 初始化时间完成
        /// </summary>
        private bool inited = false;

        // Use this for initialization
        void Start()
        {
            //DontDestroyOnLoad(this);
            Init();
        }

        // Update is called once per frame
        void Update()
        {
            
        }

        private void FixedUpdate()
        {
            if (true) //判断是否初始化
            {
                Init();

                CheckFrame();
                CheckHalfSecond();
                CheckSecond();
                CheckMinute();
            }
        }

        #region 时间相关

        /// <summary>
        /// 应用开始utc时间
        /// </summary>
        private static DateTime beginGameUtcTime = DateTime.UtcNow;

        /// <summary>
        /// 应用开始时间
        /// </summary>
        private static DateTime beginGameTime = DateTime.Now;

        /// <summary>
        /// 时差
        /// </summary>
        private static TimeSpan timeOffset;

        /// <summary>
        /// 设置开始时间
        /// </summary>
        public static void SetBeginTime(DateTime nowUtcTime)
        {
            int openCount = TimeUtil.GetElapsedRealtime();

            beginGameUtcTime = nowUtcTime.AddSeconds(-openCount);
            beginGameTime = beginGameUtcTime + (DateTime.Now - DateTime.UtcNow);

            timeOffset = nowUtcTime - GetRealCurUTCTime();
        }

        /// <summary>
        /// 设置时差
        /// </summary>
        public static void SetTimeOffset()
        {
            int openCount = TimeUtil.GetElapsedRealtime();

            timeOffset = beginGameUtcTime.AddSeconds(openCount) - GetRealCurUTCTime();
        }

        public static DateTime GetCurTime()
        {
#if USEOLDTIMETOOL
        return GetRealCurTime();
#else
            //return DateTime.Now;
            if (false)
                return GetRealCurTime();
            else
                return GetRealCurTime() + timeOffset; //beginGameTime.AddSeconds(AY_Utils.GetElapsedRealtime());
#endif
        }

        public static DateTime GetCurUTCTime()
        {
#if USEOLDTIMETOOL
        return GetRealCurUTCTime();
#else
            //return DateTime.UtcNow;
            if (false)
                return GetRealCurUTCTime();
            else
                return GetRealCurUTCTime() + timeOffset; //beginGameUtcTime.AddSeconds(AY_Utils.GetElapsedRealtime());
#endif
        }

        /// <summary>
        /// 获取系统当前时间
        /// </summary>
        /// <returns></returns>
        public static DateTime GetRealCurTime()
        {
            return DateTime.Now;
        }

        /// <summary>
        /// 获取系统当前utc时间
        /// </summary>
        /// <returns></returns>
        public static DateTime GetRealCurUTCTime()
        {
            return DateTime.UtcNow;
        }

        #endregion

        /// <summary>
        /// 将方法加入到刷新队列中
        /// </summary>
        public void AddUpdateToArray(Action fuc, UpdateFunType type)
        {
            if (fuc == null || fuc.Target == null)
            {
                return;
            }

            RemoveUpdateFromArray(fuc);

            switch (type)
            {
                case UpdateFunType.frame:
                    frameUpdateArray += fuc;
                    break;
                case UpdateFunType.halfSecond:
                    halfSecondUpdateArray += fuc;
                    break;
                case UpdateFunType.second:
                    secondUpdateArray += fuc;
                    break;
                case UpdateFunType.minute:
                    minuteUpdateArray += fuc;
                    break;
                default:
                    break;
            }
        }

        /// <summary>
        /// 从刷新队列中移除
        /// </summary>
        public void RemoveUpdateFromArray(Action fun)
        {
            if (fun == null)
            {
                return;
            }

            frameUpdateArray -= fun;
            halfSecondUpdateArray -= fun;
            secondUpdateArray -= fun;
            minuteUpdateArray -= fun;
        }

        /// <summary>
        /// 从刷新队列中移除
        /// </summary>
        public void RemoveUpdateFromArray(Action fun, UpdateFunType type)
        {
            if (fun == null)
            {
                return;
            }

            switch (type)
            {
                case UpdateFunType.frame:
                    frameUpdateArray -= fun;
                    break;
                case UpdateFunType.halfSecond:
                    halfSecondUpdateArray -= fun;
                    break;
                case UpdateFunType.second:
                    secondUpdateArray -= fun;
                    break;
                case UpdateFunType.minute:
                    minuteUpdateArray -= fun;
                    break;
                default:
                    break;
            }
        }

        /// <summary>
        /// 增加延迟执行
        /// </summary>
        /// <param name="frame"></param>
        /// <param name="fun"></param>
        public void AddWaitFrameToPlay(int frame, Action fun)
        {
            if (fun == null)
            {
                return;
            }

            TimeUtil.WaitForFrames(frame, fun);
        }

        /// <summary>
        /// 增加延迟执行
        /// </summary>
        /// <param name="second"></param>
        /// <param name="fun"></param>
        public void AddWaitSecondToPlay(float second, Action fun)
        {
            if (fun == null)
            {
                return;
            }

            TimeUtil.WaitForSeconds(second, fun);
        }


        /// <summary>
        /// 初始化时间
        /// </summary>
        public void Init()
        {
            if (!inited && true)
            {
                inited = true;

                tempHalfSecondTime = GetRealCurUTCTime();
                tempSecondTime = GetRealCurUTCTime();
                tempMinuteTime = GetRealCurUTCTime();
            }
        }

        /// <summary>
        /// 帧检测
        /// </summary>
        private void CheckFrame()
        {
            if (frameUpdateArray != null && frameUpdateArray.Target != null)
            {
                frameUpdateArray();
            }
        }

        /// <summary>
        /// 半秒检测
        /// </summary>
        private void CheckHalfSecond()
        {
            tempHalfSecondSpan = GetRealCurUTCTime() - tempHalfSecondTime;
            if (tempHalfSecondSpan.TotalMilliseconds >= 500)
            {
                tempHalfSecondTime = GetRealCurUTCTime();

                if (halfSecondUpdateArray != null && halfSecondUpdateArray.Target != null)
                {
                    halfSecondUpdateArray();
                }
            }
        }

        /// <summary>
        /// 秒检测
        /// </summary>
        private void CheckSecond()
        {
            tempSecondSpan = GetRealCurUTCTime() - tempSecondTime;
            if (tempSecondSpan.TotalSeconds >= 1)
            {
                tempSecondTime = GetRealCurUTCTime();

                if (secondUpdateArray != null && secondUpdateArray.Target != null)
                {
                    secondUpdateArray();
                }
            }
        }

        /// <summary>
        /// 分钟检测
        /// </summary>
        private void CheckMinute()
        {
            tempMinuteSpan = GetRealCurUTCTime() - tempMinuteTime;
            if (tempMinuteSpan.TotalMinutes >= 1)
            {
                tempMinuteTime = GetRealCurUTCTime();

                if (minuteUpdateArray != null)
                {
                    minuteUpdateArray();
                }
            }
        }
    }

    /// <summary>
    /// 刷新时间类型
    /// </summary>
    public enum UpdateFunType
    {
        /// <summary>
        /// 每帧刷新
        /// </summary>
        frame = 0,

        /// <summary>
        /// 每半秒刷新
        /// </summary>
        halfSecond,

        /// <summary>
        /// 每秒刷新
        /// </summary>
        second,

        /// <summary>
        /// 每分钟刷新
        /// </summary>
        minute,
    }
}