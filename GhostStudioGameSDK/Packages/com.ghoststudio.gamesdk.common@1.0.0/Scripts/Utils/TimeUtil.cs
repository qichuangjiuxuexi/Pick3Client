using System;

namespace AppBase.Utils
{
    public static class TimeUtil
    {
        public static DateTime ConvertUnixTimestampToDateTime(long unixTimestamp)
        {
            DateTimeOffset dateTimeOffset = DateTimeOffset.FromUnixTimeMilliseconds(unixTimestamp);
            DateTime dateTime = dateTimeOffset.DateTime;
            return dateTime;
        }
        
        public static string ConvertUnixTimestampToDateTimeString(long unixTimestamp)
        {
            return ConvertUnixTimestampToDateTime(unixTimestamp).ToString("yyyy-MM-dd HH:mm:ss");
        }
    }
}