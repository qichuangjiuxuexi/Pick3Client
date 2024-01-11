/**********************************************

Copyright(c) 2020 by com.me2zen
All right reserved

Author : Terrence Rao 
Date : 2020-07-18 19:30:13
Ver : 1.0.0
Description : 
ChangeLog :
**********************************************/


using System;
using System.Collections.Generic;
using System.Globalization;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Text;
using UnityEngine;

namespace WordGame.Utils
{
    /// <summary>
    /// 游戏逻辑内的时间工具.
    ///
    /// 通用的时间工具. 请使用 TimeUtil
    /// 
    /// </summary>
    public class ToolDateTime
    {
        private static DateTime dateTimeStartPointLocal = new DateTime(1970, 1, 1, 0, 0, 0, 0,DateTimeKind.Local);
        public static DateTime DateTimeStartPointLocal
        {
            get { return dateTimeStartPointLocal; }
        }
        
        
        /// <summary>
        /// 获取月天组合id
        /// 1.月份 * 100 + 天, eg 06-22 -> 622
        /// </summary>
        /// <param name="month">月</param>
        /// <param name="day">天</param>
        /// <returns></returns>
        public static int GetMonthDayId(int month, int day)
        {
            return month * 100 + day;
        }

        /// <summary>
        /// 月份ID, eg: 2018-07-25 -> 201807
        /// </summary>
        public static int GetMonthID(DateTime dateTime)
        {
            return 100 * dateTime.Year + dateTime.Month;
        }

        /// <summary>
        /// 月份ID, eg: 2018-07-25 -> 201807
        /// </summary>
        /// <param name="year">年</param>
        /// <param name="month">月</param>
        /// <returns>月份ID</returns>
        public static int GetMonthID(int year, int month)
        {
            return 100 * year + month;
        }

        /// <summary>
        /// 根据DateTime, 得到DayID
        /// </summary>
        /// <param name="dateTime"></param>
        /// <returns></returns>
        public static int GetDayID(DateTime dateTime)
        {
            return 10000 * dateTime.Year + 100 * dateTime.Month + dateTime.Day;
        }

        /// <summary>
        /// 根据年,月, 日得到DayID
        /// eg: 20201230
        /// </summary>
        /// <param name="year"></param>
        /// <param name="month"></param>
        /// <param name="day"></param>
        /// <returns></returns>
        public static int GetDayID(int year, int month, int day)
        {
            return 10000 * year + 100 * month + day;
        }

        /// <summary>
        /// eg: 202104181, 202104182, 202104183
        /// </summary>
        /// <param name="year"></param>
        /// <param name="month"></param>
        /// <param name="day"></param>
        /// <param name="levelIdInDay"></param>
        /// <returns></returns>
        public static int GetDailyChallengeLevelId(int year, int month, int day, int levelIdInDay)
        {
            return 10 * GetDayID(year, month, day) + levelIdInDay;
        }

        public static int GetMonthByMonthID(int monthID)
        {
            return monthID % 100;
        }

        /// <summary>
        /// 20191205 -> 5
        /// </summary>
        /// <param name="dayID"></param>
        /// <returns></returns>
        public static int GetDayByDayID(int dayID)
        {
            return dayID % 100;
        }

        /// <summary>
        /// 20191205 -> 2019
        /// </summary>
        /// <param name="dayID"></param>
        /// <returns></returns>
        public static int GetYeatByDayID(int dayID)
        {
            return dayID / 10000;
        }

        /// <summary>
        /// 20191205 -> 12
        /// </summary>
        /// <param name="dayID"></param>
        /// <returns></returns>
        public static int GetMonthByDayID(int dayID)
        {
            return dayID / 100 % 100;
        }

        /// <summary>
        /// 得到时间间隔 TimeSpan
        /// </summary>
        /// <param name="endTime"></param>
        /// <returns></returns>
        public static TimeSpan GetTimeSpanFromNow(DateTime endTime)
        {
            if (DateTime.Now < endTime)
            {
                return endTime - DateTime.Now;
            }
            else
            {
                return TimeSpan.Zero;
            }
        }
        
        /// <summary>
        /// 得到倒计时字符串 带天
        /// </summary>
        public static string GetTimeCountDownStringWithDay(TimeSpan timeSpan)
        {
            if (timeSpan.TotalDays > 1f)
            {
                if (timeSpan.Hours == 0)
                {
                    return $"{timeSpan.Days}d";
                }
                return $"{timeSpan.Days}d {timeSpan.Hours}h";
            }
            else
            {
                return GetTimeCountDownString(timeSpan);
            }
        }

        /// <summary>
        /// 得到倒计时字符串
        /// </summary>
        public static string GetTimeCountDownString(TimeSpan timeSpan)
        {
            if (timeSpan.TotalSeconds >= 3600f)
            {
                return timeSpan.ToString(@"hh\:mm\:ss");
            }
            else
            {
                return timeSpan.ToString(@"mm\:ss");
            }
        }

        /// <summary>
        /// 得到星期几
        /// </summary>
        /// <param name="dayInWeek">0~6</param>
        /// <returns></returns>
        public static String GetWeekDayShortName(int dayInWeek)
        {
            
            string[] weekDaysName = new string[]
            {
                "SUN",
                "MON",
                "TUES",
                "WED",
                "THUR",
                "FRI",
                "SAT"
            };
            
            if (dayInWeek >= 0 && dayInWeek <= 6)
            {
                return weekDaysName[dayInWeek ];

                //改用在语言包中读取
                //string key = string.Format("Common_WeekDay_{0}", dayInWeek);
                //return TextConfigManager.Instance.GetText(0);
            }
            else
            {
                Debugger.LogDError("error in GetWeekDayShortName");
                return "Unknow";
            }
        }

        /// <summary>
        /// 得到月份简称
        /// </summary>
        /// <param name="dayInWeek">1~12</param>
        /// <returns></returns>
        public static String GetMonthShortName(int month)
        {
            
            string[] monthNames = new string[]
            {
                "JAN",
                "FEB",
                "MAR",
                "APR",
                "MAY",
                "JUN",
                "JUL",
                "AUG",
                "SEPT", //SEP
                "OCT",
                "NOV",
                "DEC"
            };
            if (month >= 1 && month <= 12)
            {
                return monthNames[month - 1];
                //改用在语言包中读取
                //string key = string.Format("Common_MonthShort_{0}", month);
                //string key = string.Format("Common_MonthShort_FirstUpper_{0}", month);
                //return TextConfigManager.Instance.GetText(0);
            }
            else
            {
                Debugger.LogDError("error in GetWeekDayShortName");
                return "Unknow";
            }
        }

        /// <summary>
        /// 得到月份简称
        /// </summary>
        /// <param name="dayInWeek">1~12</param>
        /// <returns></returns>
        public static String GetMonthFullName(int month)
        {
            
            string[] monthNames = new string[]
            {
                "January",
                "February",
                "March",
                "April",
                "May",
                "June",
                "July",
                "August",
                "September",
                "October",
                "November",
                "December"
            };
            
            if (month >= 1 && month <= 12)
            {
                return monthNames[month - 1];
                //string key = string.Format("Common_Month_{0}", month);
                //return TextConfigManager.Instance.GetText(0);
            }
            else
            {
                Debugger.LogDError("error in GetWeekDayShortName");
                return "Unknow";
            }
        }
        
        /// <summary>
        /// 获取当前月有多少天
        /// </summary>
        /// <param name="year">年</param>
        /// <param name="month">月</param>
        /// <returns>当前月的天数</returns>
        public static int GetMonthDaysCount(int year, int month)
        {
            int ret = 0;
            switch (month)
            {
                case 1:
                case 3:
                case 5:
                case 7:
                case 8:
                case 10:
                case 12:
                    ret = 31;
                    break;
                case 4:
                case 6:
                case 9:
                case 11:
                    ret = 30;
                    break;
                case 2:
                    if (year % 400 == 0)
                    {
                        ret = 29;
                    }
                    else if (year % 100 != 0
                             && year % 4 == 0)
                    {
                        ret = 29;
                    }
                    else
                    {
                        ret = 28;
                    }

                    break;
            }

            return ret;
        }

    }
}