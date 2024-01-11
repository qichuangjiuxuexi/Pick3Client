using System;
using System.Globalization;

namespace AppBase.Utils
{
    internal class TimeUtil
    {
        /// <summary>
        /// 时间字符串转dateTime
        /// </summary>
        /// <param name="timestr">时间字符串</param>
        /// <param name="convertUtc">字符串是本地时间，需要转换成UTC时间</param>
        /// <param name="format">时间格式</param>
        /// <param name="defaultTime">默认时间</param>
        /// <returns>时间对象</returns>
        internal static DateTime GetDateTimeByString(string timestr, bool convertUtc = false, string format = "yyyy-MM-dd HH:mm:ss", DateTime defaultTime = default)
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
    }
}