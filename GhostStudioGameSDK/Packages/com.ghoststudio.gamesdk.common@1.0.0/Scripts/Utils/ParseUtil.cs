using System;
using System.Collections.Generic;
using System.Globalization;

namespace AppBase.Utils
{
    public static class ParseUtil
    {
        public static int ParseInt(string str, int defaultValue = default)
        {
            return int.TryParse(str, out var value) ? value : defaultValue;
        }
        
        public static bool ParseBool(string str)
        {
            return str?.ToLower() is "1" or "true";
        }
        
        public static long ParseLong(string str, long defaultValue = default)
        {
            return long.TryParse(str, out var value) ? value : defaultValue;
        }
        
        public static float ParseFloat(string str, float defaultValue = default)
        {
            return float.TryParse(str, NumberStyles.Float, CultureInfo.InvariantCulture, out float val) ? val : defaultValue;
        }
        
        public static double ParseDouble(string str, double defaultValue = default)
        {
            return double.TryParse(str, NumberStyles.Float, CultureInfo.InvariantCulture, out double val) ? val : defaultValue;
        }
        
        public static DateTime ParseDateTime(string str, DateTime defaultValue = default)
        {
            return DateTime.TryParseExact(str, "yyyy-MM-dd HH:mm:ss", CultureInfo.InvariantCulture, DateTimeStyles.None, out var val) ? val : defaultValue;
        }

        public static T Parse<T>(string str, T defaultValue = default)
        {
            return Type.GetTypeCode(typeof(T)) switch
            {
                TypeCode.String => (T)(object)str,
                TypeCode.Int32 => (T)(object)ParseInt(str, Convert.ToInt32(defaultValue)),
                TypeCode.Int64 => (T)(object)ParseLong(str, Convert.ToInt64(defaultValue)),
                TypeCode.Boolean => (T)(object)ParseBool(str),
                TypeCode.Single => (T)(object)ParseFloat(str, Convert.ToSingle(defaultValue)),
                TypeCode.Double => (T)(object)ParseDouble(str, Convert.ToDouble(defaultValue)),
                TypeCode.DateTime => (T)(object)ParseDateTime(str, Convert.ToDateTime(defaultValue)),
                _ => defaultValue
            };
        }

        
        /// <summary>
        /// 将str解析为List<string>
        /// </summary>
        /// <param name="str"></param>
        /// <param name="strSplitChar">指定分割符</param>
        /// <param name="autoTrimEle">是否将分割后的部分在添加进list中前去除两端的空白字符</param>
        /// <param name="splitOption">切割选项</param>
        /// <returns></returns>
        public static List<string> ParseListString(this string str, char strSplitChar = ',', bool autoTrimEle = true,
            StringSplitOptions splitOption = StringSplitOptions.RemoveEmptyEntries)
        {
            if (str == null)
            {
                return new List<string>(0);
            }

            var strArr = str.Split(strSplitChar, splitOption);
            var list = new List<string>(strArr.Length);
            for (int i = 0; i < strArr.Length; i++)
            {
                list.Add(autoTrimEle ? strArr[i].Trim() : strArr[i]);
            }

            return list;
        }

        /// <summary>
        /// 将字符串解析为List<int>
        /// </summary>
        /// <param name="str"></param>
        /// <param name="strSplitChar">指定分割符</param>
        /// <param name="ignoreFailed">如果某一部分解析成int失败了，是否忽略此部分，并继续解析下一个。false的话会报错，并返回一个空List<int>。</param>
        /// <returns></returns>
        public static List<int> ParseListInt(this string str, char strSplitChar = ',', bool ignoreFailed = false)
        {
            if (str == null)
            {
                return new List<int>(0);
            }

            var strArr = str.Split(strSplitChar, StringSplitOptions.RemoveEmptyEntries);
            var list = new List<int>(strArr.Length);
            for (int i = 0; i < strArr.Length; i++)
            {
                if (int.TryParse(str, out var value))
                {
                    list.Add(value);
                    continue;
                }
                if (!ignoreFailed)
                {
                    Debugger.LogDError("input str is invalid");
                    return new List<int>(0);
                }
            }

            return list;
        }
    }
}