using System;
using UnityEngine;
using System.Collections.Generic;
using System.Globalization;

    /// <summary>
    /// Extension methods for UnityEngine.Vector3.
    /// </summary>
    public static class DateTimeExtension
    {
        
        /// <summary>
        /// Finds the position closest to the given one.
        /// </summary>
        /// <param name="position">World position.</param>
        /// <param name="otherPositions">Other world positions.</param>
        /// <returns>Closest position.</returns>
        public static string ToStringWithoutCulture(this DateTime dateTime)
        {
            return dateTime.ToString(CultureInfo.InvariantCulture);
        }

        public static bool TryParse(string dateTime,out DateTime time)
        {
            return DateTime.TryParse(dateTime, DateTimeFormatInfo.InvariantInfo, DateTimeStyles.None, out time);
        }
        
        public static DateTime Parse(string dateTime)
        {
            return DateTime.Parse(dateTime, DateTimeFormatInfo.InvariantInfo, DateTimeStyles.None);
        }

        /// <summary>
        /// 2020-04-05 12:26:50
        /// </summary>
        /// <param name="mailTime"></param>
        /// <returns></returns>
        public static bool TryParseServerTime(string serverTime,out DateTime time)
        {
            try
            {
                return DateTime.TryParseExact(serverTime, "yyyy-MM-dd HH:mm:ss", CultureInfo.GetCultureInfo("zh-cn"),
                    DateTimeStyles.None, out time);
            }
            catch (Exception e)
            {
                time = DateTime.Now;
                return false;
            }
            
        }
        
        public static DateTime ParseServerTime(string serverTime)
        {
            DateTime time = DateTime.Now;
            try
            {
                DateTime.TryParseExact(serverTime, "yyyy-MM-dd HH:mm:ss", CultureInfo.GetCultureInfo("zh-cn"),
                    DateTimeStyles.None, out time);
            }
            catch (Exception e)
            {
                return time;
            }

            return time;
        }
    }
