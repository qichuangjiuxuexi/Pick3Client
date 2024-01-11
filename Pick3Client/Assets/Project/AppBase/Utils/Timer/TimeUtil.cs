using System;
using System.Collections;
using System.Globalization;
using UnityEngine;

namespace WordGame.Utils.Timer
{
    /// <summary>
    /// 日期时间相关工具类.
    ///
    /// 应用逻辑相关的工具类, 请使用ToolDateTime
    /// </summary>
    public static class TimeUtil
    {
        public const int INTERVAL_FIFTEEN_MINUTES = 900;
        public const int INTERVAL_HALF_HOUR = 1800;
        public const int INTERVAL_HOUR = 3600;
        public const int INTERVAL_HALF_DAY = 43200;
        public const int INTERVAL_DAY = 86400;
        public const int INTERVAL_WEEK = 604800;

        public static string GetDateStringYear_Month_Day(DateTime time)
        {
            string strResult = time.ToString("yyyy-MM-dd");
            return strResult;
        }
        
        public static string GetDateTimeFullString(DateTime time)
        {
            string strResult = time.ToString("yyyy-MM-dd HH:mm:ss");
            return strResult;
        }
        
        /// <summary>
        /// 格式化timespan为精确的时分秒，没有天，把天换算成时加在时的分量上，并保证每一个分量上有两位数字，如49：04 ：09
        /// </summary>
        /// <param name="ts"></param>
        /// <returns></returns>
        public static string FormatTimeSpan(TimeSpan ts)
        {
            return string.Format("{0}:{1}:{2}", (ts.Days * 24 + ts.Hours).ToString().PadLeft(2, '0'),
                ts.Minutes.ToString().PadLeft(2, '0'),
                ts.Seconds.ToString().PadLeft(2, '0'));
        }
        
        // 获取当前UTC时间 YY-MM-DDTHH:MM:SS格式
        public static string GetUtcTimeString()
        {
            return DateTime.UtcNow.ToString("s");
        }

        //2019-12-10T06:31:45.7811640Z
        public static string GetUtcTimeStringISO8601()
        {
            return DateTime.UtcNow.ToUniversalTime().ToString("yyyy-MM-ddTHH:mm:ss.fffZ");
        }
        
        public static string GetTimeStringISO8601(DateTime dateTime)
        {
            return dateTime.ToUniversalTime().ToString("yyyy-MM-ddTHH:mm:ss.fffZ");
        }
        
        /// <summary>
        /// 获得Unix时间戳
        /// 即从1970年1月1日午夜, 至今的毫秒数
        /// </summary>
        public static long GetTimeStamp()
        {
            return DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
        }
        
        /// <summary>
        /// 获得Unix时间戳
        /// 即从1970年1月1日午夜, 至今的秒数
        /// </summary>
        public static long GetTimeSecondStamp()
        {
            return DateTimeOffset.UtcNow.ToUnixTimeSeconds();
        }
        
        // 获取当前时区
        public static string GetTimeZone()
        {
            return DateTime.Now.ToString("%z");
        }
        
        public static int DiffDaysByDistance(DateTime toDateTime, DateTime fromDateTime)
        {
            return (toDateTime.Date - fromDateTime.Date).Days;
        }

        public static int DiffDays(DateTime toDateTime, DateTime fromDateTime)
        {
            int dayByDistance = DiffDaysByDistance(toDateTime, fromDateTime);
            //超24小时
            if (dayByDistance >= 1)
            {
                return dayByDistance;
            }
            //不到24小时
            else
            {
                //不在同一天
                if (toDateTime.Day != fromDateTime.Day)
                {
                    return 1;
                }
                else
                {
                    return 0;
                }
            }
        }

        public static int GetLocalDtSec()
        {
            return DateTimeToSec(DateTime.Now);
        }

        public static int GetUtcDtSec()
        {
            return DateTimeToSec(DateTime.UtcNow);
        }

        public static int DiffSeconds(DateTime dta, DateTime dtb)
        {
            return (int) (dta.Date - dtb.Date).TotalSeconds;
        }

        public static int DateTimeToSec(DateTime dateTime)
        {
            var dateTime1 = DateTime.Parse("1970-01-01");
            return (int) (dateTime - dateTime1).TotalSeconds;
        }

        public static DateTime SecToDateTime(long sec)
        {
            return DateTime.Parse("1970-01-01").AddSeconds(sec);
        }

        public static int GetTodayFirstUtcSec()
        {
            return DateTimeToSec(DateTime.UtcNow - (DateTime.Now - DateTime.Now.Date));
        }

        public static string GetTimezone()
        {
            return DateTime.Now.ToString("zzz").Replace(":", "");
        }

        public static float GetTimezoneFloat()
        {
            var strArray = DateTime.Now.ToString("zzz").Split(':');
            var num = 0.0f;
            if (strArray.Length >= 2)
            {
                var result1 = 0;
                int.TryParse(strArray[0], out result1);
                var result2 = 0;
                int.TryParse(strArray[1], out result2);
                num = result1 >= 0 ? result1 + result2 / 60f : result1 - result2 / 60f;
            }

            return num;
        }

        private static IEnumerator WaitForSecondsRealtimeEnumerator(
            float delay,
            Action callback,
            int iterations = 1)
        {
            if (iterations <= 0)
                while (true)
                {
                    yield return new WaitForSecondsRealtime(delay);
                    callback();
                }

            for (var i = 0; i < iterations; ++i)
            {
                yield return new WaitForSecondsRealtime(delay);
                callback();
            }
        }

        private static IEnumerator WaitForSecondsEnumerator(
            float delay,
            Action callback,
            int iterations = 1)
        {
            if (iterations <= 0)
                while (true)
                {
                    yield return new WaitForSeconds(delay);
                    callback();
                }

            for (var i = 0; i < iterations; ++i)
            {
                yield return new WaitForSeconds(delay);
                callback();
            }
        }

        private static IEnumerator WaitForFramesEnumerator(int delayFrame, Action callback)
        {
            for (; delayFrame > 0; --delayFrame)
                yield return null;
            if (callback != null)
                callback();
        }

        private static IEnumerator WaitForEndOfFrameEnumerator(Action callback)
        {
            yield return new WaitForEndOfFrame();
            if (callback != null)
                callback();
        }

        public static Coroutine WaitForSecondsRealtime(float delay, Action callback, int iterations = 1)
        {
            return CoroutineStarter.StartCoroutine(WaitForSecondsRealtimeEnumerator(delay, callback, iterations));
        }

        public static Coroutine WaitForSeconds(float delay, Action callback, int iterations = 1)
        {
            return CoroutineStarter.StartCoroutine(WaitForSecondsEnumerator(delay, callback, iterations));
        }

        public static Coroutine WaitForFrames(int delayFrame, Action callback)
        {
            return CoroutineStarter.StartCoroutine(WaitForFramesEnumerator(delayFrame, callback));
        }

        public static Coroutine WaitForEndOfFrame(Action callback)
        {
            return CoroutineStarter.StartCoroutine(WaitForEndOfFrameEnumerator(callback));
        }
        
        public static string GetDateTimeStringForGTA(long timeStampSeconds)
        {
            DateTime date = SecToDateTime(timeStampSeconds);
            return date.ToString("yyyy-MM-dd HH:mm:ss");
        }
        
        public static string GetDateTimeStringForGTA(DateTime dateTime)
        {
            return dateTime.ToString("yyyy-MM-dd HH:mm:ss");
        }

        public static DateTime GetLocalTimeFromUtc(DateTime utcTime)
        {
            DateTime localDateTime = TimeZoneInfo.ConvertTimeFromUtc(utcTime, TimeZoneInfo.Local);
            return localDateTime;
        }

        public static DateTime GetUtcTimeFromLocal(DateTime localTime)
        {
            DateTime utcTime = TimeZoneInfo.ConvertTimeToUtc(localTime, TimeZoneInfo.Local);
            return utcTime;
        }
        
        public static string GetLocalDateTimeStringForGTA(int timeStampSeconds,string format = "yyyy-MM-dd HH:mm:ss")
        {
            DateTime date = SecToDateTime(timeStampSeconds);
            DateTime localDateTime = TimeZoneInfo.ConvertTimeFromUtc(date, TimeZoneInfo.Local);
            return localDateTime.ToString(format);
        }
        
        public static DateTime GetLocalDateTimeByTimeStamp(long timeStampSeconds)
        {
            DateTime date = SecToDateTime(timeStampSeconds);
            DateTime localDateTime = TimeZoneInfo.ConvertTimeFromUtc(date, TimeZoneInfo.Local);
            return localDateTime;
        }
        
        public static string GetLocalDateTimeStringForGTA(DateTime dateTime)
        {
            DateTime localDateTime = TimeZoneInfo.ConvertTimeFromUtc(dateTime, TimeZoneInfo.Local);
            return localDateTime.ToString("yyyy-MM-dd HH:mm:ss");
        }

        /// <summary>
        /// 时间戳转换dateTime格式
        /// </summary>
        /// <param name="timestamp"></param>
        /// <param name="isUtc"></param>
        /// <returns></returns>
        public static DateTime GetDateTimeByTimeStamp(long timestamp,bool isUtc = true)
        {
            //目前只支持UTC和北京时间
            TimeZoneInfo timeZone = TimeZoneInfo.Local;//isUtc ? TimeZoneInfo.Utc : TimeZoneInfo.Local;
            DateTime dtStart = TimeZoneInfo.ConvertTime(new DateTime(1970, 1, 1), timeZone);
            long lTime = long.Parse(timestamp + "0000000");
            TimeSpan timeSpan = new TimeSpan(lTime);
            DateTime targetDt = isUtc ? dtStart.Add(timeSpan) : dtStart.Add(timeSpan).AddHours(8);
            return targetDt;
        }

        /// <summary>
        /// 时间字符串转dateTime
        /// </summary>
        /// <param name="timestr">时间字符串</param>
        /// <param name="convertUtc">字符串是本地时间，需要转换成UTC时间</param>
        /// <param name="format">时间格式</param>
        /// <param name="defaultTime">默认时间</param>
        /// <returns>时间对象</returns>
        public static DateTime GetDateTimeByString(string timestr, bool convertUtc = false, string format = "yyyy-MM-dd HH:mm:ss", DateTime defaultTime = default)
        {
            if (string.IsNullOrEmpty(timestr)) return defaultTime;
            if (!DateTime.TryParseExact(timestr, format, CultureInfo.InvariantCulture, DateTimeStyles.None, out var dateTime))
            {
                Debugger.LogDError($"GetDateTimeByString parse error: {timestr}");
                return defaultTime;
            }
            if (convertUtc)
            {
                dateTime = dateTime.ToUniversalTime();
            }
            return dateTime;
        }

        /// <summary>
        /// 获取当前运行时间
        /// </summary>
        /// <returns></returns>
        public static int GetElapsedRealtime()
        {
#if UNITY_EDITOR
            return Environment.TickCount / 1000;
#elif UNITY_IOS
            return Environment.TickCount / 1000;
            //return iOSUtils.GetElapsedRealtime();
#elif UNITY_ANDROID
            return Environment.TickCount / 1000;
            //return AndroidUtils.GetElapsedRealtime();
#endif
        }
        
        public static string GetLeftTimeString(TimeSpan leftTimeSpan,bool onlySecondsOnLess1Min)
        {
            int days = leftTimeSpan.Days;
            int hours = leftTimeSpan.Hours;
            int mins = leftTimeSpan.Minutes;
            int second = leftTimeSpan.Seconds;
            string hour = hours.ToString();
            if (days > 0)
            {
                return string.Format("{0}d {1}h", days, hour);
            }
            else
            {
                if (hours > 0)
                {
                    string min = leftTimeSpan.Minutes.ToString();
                    string secons = leftTimeSpan.Seconds.ToString();
                    return string.Format("{0}:{1}:{2}", hour.PadLeft(2, '0'), min.PadLeft(2, '0'),
                        secons.PadLeft(2, '0'));
                }
                else if(mins > 0 && second >= 0)
                {
                    string minStr = leftTimeSpan.Minutes.ToString();
                    string seconds = leftTimeSpan.Seconds.ToString();
                    return string.Format("{0}:{1}", minStr.PadLeft(2, '0'),
                        seconds.PadLeft(2, '0'));
                }
                else
                {
                    if (onlySecondsOnLess1Min)
                    {
                        return leftTimeSpan.Seconds.ToString();
                    }
                    else
                    {
                        string seconds = leftTimeSpan.Seconds.ToString();
                        return string.Format("00:{0}", seconds.PadLeft(2, '0'));
                    }

                }
            }
        }

        public static DateTime Max(DateTime a, DateTime b)
        {
            return a >= b ? a : b;
        }

        public static DateTime Min(DateTime a, DateTime b)
        {
            return a <= b ? a : b;
        }
    }
}