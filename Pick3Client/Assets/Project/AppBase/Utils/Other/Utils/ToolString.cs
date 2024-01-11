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
using System.Text;
using Newtonsoft.Json.Linq;
using UnityEngine;
using WordGame.Utils.Timer;

namespace WordGame.Utils
{
    public class ToolString
    {
        public static readonly string[] SizeSuffixes = new string[]
        {
            "Bytes",
            "KB",
            "MB",
            "GB",
            "TB",
            "PB",
            "EB",
            "ZB",
            "YB"
        };

        /// <summary>
        /// 生成一个唯一ID
        /// eg: e14f1127-ef6a-49cd-966f-fb332bcb2479
        /// </summary>
        /// <returns></returns>
        public static string GetUuid()
        {
            return Guid.NewGuid().ToString();
        }

        /// <summary>
        /// 获取体积信息
        /// </summary>
        /// <param name="len"></param>
        /// <returns></returns>
        public static string GetSizeInfo(long len)
        {
            int number = 0;
            while (len > 1024)
            {
                len /= 1024;
                number++;
            }

            return len.ToString() + "  " + SizeSuffixes[number];
        }
        /// <summary>
        /// 分隔字符串为List<int>
        /// </summary>
        public static List<int> SplitStringToIntList(string strContent, string strSplit = ",")
        {
            string[] l_strSplitList;
            List<int> l_targetList = new List<int>();
            l_strSplitList =
                strContent.Split(new string[] {strSplit}, System.StringSplitOptions.RemoveEmptyEntries);
            foreach (string l_str in l_strSplitList)
            {
                l_targetList.Add(int.Parse(l_str));
            }

            return l_targetList;
        }

        /// <summary>
        /// 分隔字符串为List<float>
        /// </summary>
        public static List<float> SplitStringToFloatList(string strContent, string strSplit = ",")
        {
            string[] l_strSplitList;
            List<float> l_targetList = new List<float>();
            l_strSplitList =
                strContent.Split(new string[] {strSplit}, System.StringSplitOptions.RemoveEmptyEntries);
            foreach (string l_str in l_strSplitList)
            {
                l_targetList.Add(float.Parse(l_str,CultureInfo.InvariantCulture));
            }

            return l_targetList;
        }

        /// <summary>
        /// 分隔字符串为List<true>
        /// 1. "true, false, true"
        /// </summary>
        public static List<bool> SplitStringToBoolList(string strContent, string strSplit = ",")
        {
            string[] l_strSplitList;
            List<bool> l_targetList = new List<bool>();
            l_strSplitList =
                strContent.Split(new string[] {strSplit}, System.StringSplitOptions.RemoveEmptyEntries);
            foreach (string l_str in l_strSplitList)
            {
                l_targetList.Add(bool.Parse(l_str));
            }

            return l_targetList;
        }

        /// <summary>
        /// 分隔字符串为List<int>
        /// </summary>
        public static List<string> SplitStringToStringList(string strContent, string strSplit = ",")
        {
            string[] l_strSplitList;
            List<string> l_targetList = new List<string>();
            l_strSplitList =
                strContent.Split(new string[] {strSplit}, System.StringSplitOptions.RemoveEmptyEntries);
            foreach (string l_str in l_strSplitList)
            {
                l_targetList.Add(l_str);
            }

            return l_targetList;
        }

        /// <summary>
        /// 数据转成字符串
        /// </summary>
        /// <returns></returns>
        public static string JoinToString<T>(List<T> dataList, string strSplit = ",")
        {
            return string.Join(strSplit, dataList);
        }

        /// <summary>
        /// "驼峰格式"字符串, 转"下划线格式"字符串
        /// "HelloWord" -> "hello_world" 
        /// </summary>
        /// <param name="camelString"></param>
        /// <returns></returns>
        public static string ConverCamelStringToUnderLineString(string camelString)
        {
            StringBuilder strResult = new StringBuilder();
            bool needAddUnderLine = false;
            for (int i = 0; i < camelString.Length; i++)
            {
                if (char.IsUpper(camelString[i]))
                {
                    if (needAddUnderLine)
                    {
                        strResult.Append("_");
                        needAddUnderLine = false;
                    }
                    strResult.Append(char.ToLower(camelString[i]));
                }
                else
                {
                    strResult.Append(camelString[i]);
                    needAddUnderLine = true;
                }
            }

            return strResult.ToString();
        }
        
        /// <summary>
        /// "驼峰格式"字符串, 转"下划线格式"字符串
        /// "hello_word" helloWorld
        /// </summary>
        /// <param name="camelString"></param>
        /// <returns></returns>
        public static string ConverUnderLineStringToCamelString(string underLineString)
        {
            StringBuilder strResult = new StringBuilder();
            bool needConvertToUpper = false;
            for (int i = 0; i < underLineString.Length; i++)
            {
                if (underLineString[i].Equals('_'))
                {
                    needConvertToUpper = true;
                }
                else
                {
                    if (needConvertToUpper)
                    {
                        strResult.Append(char.ToUpper(underLineString[i]));
                    }
                    else
                    {
                        strResult.Append(underLineString[i]);
                    }
                    needConvertToUpper = false;
                    
                }
            }

            return strResult.ToString();
        }

        /// <summary>
        /// 获取 csv 数据
        /// </summary>
        /// <param name="info"></param>
        /// <returns></returns>
        public static List<List<string>> GetCsvData(string info)
        {
            List<List<string>> data = new List<List<string>>();
            if (!string.IsNullOrEmpty(info))
            {
                string config = info.Replace("\r\n", "\n");
                List<string> rows = SplitStringToStringList(config, "\n");
                for (int i = 1; i < rows.Count; i++)
                {
                    if (!string.IsNullOrEmpty(rows[i].Trim()))
                    {
                        data.Add(SplitStringToStringList(rows[i], ","));
                    }
                }
            }

            return data;
        }

        /// <summary>
        /// 获取 csv 数据
        /// </summary>
        /// <param name="info"></param>
        /// <returns></returns>
        public static List<List<string>> GetCsvData(string info, int beginIndex)
        {
            List<List<string>> data = new List<List<string>>();
            if (!string.IsNullOrEmpty(info))
            {
                string config = info.Replace("\r\n", "\n");
                List<string> rows = SplitStringToStringList(config, "\n");
                for (int i = beginIndex; i < rows.Count; i++)
                {
                    if (!string.IsNullOrEmpty(rows[i].Trim()))
                    {
                        data.Add(SplitStringToStringList(rows[i], ","));
                    }
                }
            }

            return data;
        }

        private static string charsForRandom = "0123456789abcdefghijklmnopqrstuvwxyz";

        /// <summary>
        /// 随机字符方法一 遍历返回
        /// </summary>
        /// <param name="chars">随机字符串源</param>
        /// <param name="length">返回随机的字符串个数</param>
        /// <returns></returns>
        public static string GetRandString(int length)
        {
            StringBuilder stringBuilder = new StringBuilder();

            for (int i = 0; i < length; i++)
            {
                int index = ToolMath.GetRandom(0, charsForRandom.Length);
                stringBuilder.Append(charsForRandom[index]);
            }

            return stringBuilder.ToString();
        }

        /// <summary>
        /// 根据子串, 得到完成字符串
        /// </summary>
        /// <returns></returns>
        public static string GetStringBySubwords(List<string> subwords)
        {
            StringBuilder stringBuilder = new StringBuilder();
            foreach (string subWord in subwords)
            {
                stringBuilder.Append(subWord);
            }

            return stringBuilder.ToString();
        }

        public static string GetDailyRewardMarkString()
        {
            string strDate = TimeUtil.GetDateStringYear_Month_Day(DateTime.Now);
            string strResult = strDate + "/got";
            return strResult;
        }


        public static void ShowTimeStamp(string strTag)
        {
            //if (BuildSettings.IsDebug)        
            {
                Debugger.LogDWarning(strTag + " TimeStampInfo : " + DateTime.Now.ToLongTimeString() + " : " +
                                     Time.realtimeSinceStartup.ToString());
            }
        }

        /// <summary>
        /// 数据转化成字符串
        /// </summary>
        /// <param name="list"></param>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public static string GetPrettyFormatStringOfList<T>(List<T> list)
        {
            StringBuilder strBuilder = new StringBuilder();
            strBuilder.Append("[");
            foreach (T value in list)
            {
                strBuilder.Append(value.ToString() + ", ");
            }

            strBuilder.Remove(strBuilder.Length - 2, 2);
            strBuilder.Append("]");
            return strBuilder.ToString();
        }

        public static string UTF8ByteArrayToString(byte[] characters)
        {
            UTF8Encoding encoding = new UTF8Encoding();
            string constructedString = encoding.GetString(characters);
            return (constructedString);
        }

        public static byte[] StringToUTF8ByteArray(String pXmlString)
        {
            UTF8Encoding encoding = new UTF8Encoding();
            byte[] byteArray = encoding.GetBytes(pXmlString);
            return byteArray;
        }

        /// <summary>
        /// string转为int
        /// </summary>
        /// <param name="str"></param>
        /// <param name="defult"></param>
        /// <returns></returns>
        public static int GetInt(string str, int defult = 0)
        {
            int value = defult;
            if (!int.TryParse(str, out value))
            {
                value = defult;
            }

            return value;
        }

        /// <summary>
        /// string转为floot
        /// </summary>
        /// <param name="str"></param>
        /// <param name="defult"></param>
        /// <returns></returns>
        public static float GetFloat(string str, float defult = 0)
        {
            float value = defult;
            if (!float.TryParse(str, out value))
            {
                value = defult;
            }

            return value;
        }

        /// <summary>
        /// string转为floot
        /// </summary>
        /// <param name="str"></param>
        /// <param name="defult"></param>
        /// <returns></returns>
        public static double GetDouble(string str, double defult = 0)
        {
            double value = defult;
            if (!double.TryParse(str, out value))
            {
                value = defult;
            }

            return value;
        }

        /// <summary>
        /// string转为bool
        /// </summary>
        /// <param name="str"></param>
        /// <param name="defult"></param>
        /// <returns></returns>
        public static bool GetBool(string str, bool defult = false)
        {
            bool value = defult;
            if (!bool.TryParse(str, out value))
            {
                value = defult;
            }

            return value;
        }


        /// <summary>
        /// 根据卡牌ID 获取小写字符串
        /// </summary>
        /// <param name="UnitId"></param>
        /// <returns></returns>
        public static string GetASCIIStringLower(int UnitId)
        {
            byte[] array = new byte[1];
            array[0] = (byte) (Convert.ToInt32(96 + UnitId)); //ASCII码强制转换二进制
            string str = Convert.ToString(Encoding.ASCII.GetString(array));

            return str;
        }


        /// <summary>
        /// string转为DateTime
        /// </summary>
        /// <param name="str"></param>
        /// <returns></returns>
        public static DateTime GetDateTime(string str)
        {
            DateTime temp = DateTime.Now;

            if (!string.IsNullOrEmpty(str))
            {
                DateTime.TryParse(str, out temp);
            }

            return temp;
        }


        /// <summary>
        /// 获得utf8字符串
        /// </summary>
        /// <param name="info"></param>
        /// <returns></returns>
        public static string GetStringFromUTF8Bytes(byte[] bytes)
        {
            string info = Encoding.UTF8.GetString(bytes);

            return info;
        }

        // 去掉字符中的换行，空格，tab
        public static string RemoveSpace(string strContent)
        {
            strContent.Replace("\t", "")
                .Replace("\n", "")
                .Replace("\r", "");

            return strContent;
        }

        /// <summary>
        /// 版本号比较
        /// 支持两拉版本的版本号比较. (1.0, 1.2)
        /// 1: 代表ver1 > ver2
        /// -1: 代表ver1 < ver2
        /// 0: 两个版本号一样
        /// </summary>
        /// <param name="value0"></param>
        /// <param name="value1"></param>
        /// <returns></returns>
        public static int CompareVersion(string value0, string value1)
        {
            int state = 0;

            string[] valueArray0 = value0.Split('.');
            string[] valueArray1 = value1.Split('.');

            int value00 = int.Parse(valueArray0[0]);
            int value10 = int.Parse(valueArray1[0]);

            if (value00 > value10)
            {
                state = 1;
            }
            else if (value00 < value10)
            {
                state = -1;
            }
            else
            {
                if (valueArray0.Length == 1)
                {
                    state = -1;
                }
                else if (valueArray1.Length == 1)
                {
                    state = 1;
                }
                else
                {
                    int value01 = int.Parse(valueArray0[1]);
                    int value11 = int.Parse(valueArray1[1]);

                    if (value01 > value11)
                    {
                        state = 1;
                    }
                    else if (value01 < value11)
                    {
                        state = -1;
                    }
                    else
                    {
                        state = 0;
                    }
                }
            }

            return state;
        }

        /// <summary>
        /// 判断是否为回文字符串
        /// </summary>
        /// <param name="strContent"></param>
        /// <returns></returns>
        public static bool IsPalindromeString(string strContent)
        {
            StringBuilder str = new StringBuilder();
            for (int i = strContent.Length - 1; i >= 0; i--)
            {
                str.Append(strContent[i]);
            }

            return strContent.Equals(str.ToString());
        }

        /// <summary>
        /// TextMeshPro 中使用图集字的格式
        /// </summary>
        /// <param name="strContent"></param>
        /// <returns></returns>
        public static string GetTextWithSprite(string strContent)
        {
            return string.Format("<sprite name=\"{0}\">", strContent);
        }

        /// <summary>
        /// 根据设备Id分组
        /// clientId为十六进制字符
        /// </summary>
        /// <param name="clientId"></param>
        /// <returns></returns>
        public static int GetGroupIdByClientId(string clientId)
        {
            int groupId = 1;
            clientId = clientId.ToLower();
            if (!string.IsNullOrEmpty(clientId))
            {
                char lastChar = clientId.ToCharArray()[clientId.Length - 1];
                if (lastChar >= '0' && lastChar <= '3')
                {
                    groupId = 1;
                }
                else if (lastChar >= '4' && lastChar <= '7')
                {
                    groupId = 2;
                }
                else if ((lastChar >= '8' && lastChar <= '9') || (lastChar>='a' && lastChar<='b'))
                {
                    groupId = 3;
                }else if (lastChar >= 'c' && lastChar <= 'f')
                {
                    groupId = 4;
                }
                else
                {
                    Debugger.LogDError("error in GetGroupIdByClientId not hex string");
                    groupId = (lastChar - 'g') % 4 + 1;
                }

            }
            else
            {
                Debugger.LogDError("error in GetGroupIdByClientId null");
            }

            return groupId;
        }
        
        
        /// <summary>
        /// 将byte数组转换为 无符号整形数组
        /// </summary>
        /// <param name="data"></param>
        /// <param name="includeLength"></param>
        /// <returns></returns>
        public static uint[] Byte2Uint(byte[] data, bool includeLength)
        {
            if (data == null)
            {
                return null;
            }

            int dataLen = data.Length; //原始数据长度
            int saveLen = ((dataLen & 3) != 0) ? ((dataLen >> 2) + 1) : (dataLen >> 2); //四分之一长度(能整除就4分之1，不能就扩一位，存储余数)
            uint[] array;
            if (includeLength) //如果需要写入长度，就再次扩容，然后将原始数据长度写在最后一位
            {
                array = new uint[saveLen + 1];
                array[saveLen] = (uint)dataLen;
            }
            else
            {
                array = new uint[saveLen];
            }

            for (int i = 0; i < dataLen; i++) //填充数据
            {
                array[i >> 2] |= (uint)data[i] << ((i & 3) << 3);
            }

            return array;
        }

        /// <summary>
        /// 无符号整形 转 byte数组
        /// </summary>
        /// <param name="data"></param>
        /// <param name="includeLength"></param>
        /// <returns></returns>
        public static byte[] Uint2Byte(uint[] data, bool includeLength)
        {
            if (data == null)
            {
                return null;
            }

            int dataLen = data.Length << 2; //数据长度为存储长度的4的整数倍
            if (includeLength) //如果存储了长度，最后一位是长度
            {
                int lenInfo = (int)data[data.Length - 1]; //存储的原始数据长度
                dataLen -= 4;
                if (lenInfo < dataLen - 3 || lenInfo > dataLen) //过小就说明存储有问题
                {
                    return null;
                }

                dataLen = lenInfo; //将存档中记录的 数据长度 覆盖计算得到的 数据长度
            }

            byte[] array = new byte[dataLen];
            for (int i = 0; i < dataLen; i++) //填充数据
            {
                array[i] = (byte)(data[i >> 2] >> ((i & 3) << 3));
            }

            return array;
        }
        
        private static bool SetValueListDataByString<T>(string strContent,ref List<T> targetList,string splitChar = ",")
        {
            bool anyNoSupportType = false;
            List<string> strList = ToolString.SplitStringToStringList(strContent,splitChar);
            if (strList != null && strList.Count > 0)
            {
                for (int i = 0; i < strList.Count; i++)
                {
                    //分类型处理string
                    if (typeof(T) == typeof(string))
                    {
                        targetList.Add((T) (object) strList[i]);
                    }
                    //int
                    else if (typeof(T) == typeof(int))
                    {
                        targetList.Add((T) (object) int.Parse(strList[i]));
                    }
                    //float
                    else if (typeof(T) == typeof(float))
                    {
                        targetList.Add((T) (object) float.Parse(strList[i],CultureInfo.InvariantCulture));
                    }
                    //jsonnode
                    else if (typeof(T) == typeof(JObject))
                    {
                        targetList.Add((T) (object) strList[i]);
                    }
                    //other 暂不支持
                    else
                    {
                        anyNoSupportType = true;
                    }
                }
            }

            return anyNoSupportType;
        }
        
        /// <summary>
        /// 拿到数据列表
        /// </summary>
        public static List<List<T>> GetListValueListByKey<T>(string str,string layer0SplitChar = ";",string layer1SplitChar = ",")
        {
            List<List<T>> targetList = new List<List<T>>();
            if (!string.IsNullOrEmpty(str))
            {
                List<string> splits = ToolString.SplitStringToStringList(str,layer0SplitChar);
                
                
                if (splits != null && splits.Count > 0)
                {
                    for (int i = 0; i < splits.Count; i++)
                    {
                        List<T> tmp = new List<T>();
                        bool anyNoSupportType = SetValueListDataByString(splits[i],ref tmp,layer1SplitChar);
                        if (anyNoSupportType)
                        {
                            Debugger.LogDErrorFormat(
                                "ToolString.GetListValueListByKey error only type of \"string\" \"int\" \"float\" is support {0}",
                                str);
                        }
                        targetList.Add(tmp);
                    }
                }
            }


            if (targetList.Count <= 0)
            {
                Debugger.LogDWarningFormat(
                    "error in ToolString.GetListValueListByKey At least one must be supported name:{0}",
                    str);
            }

            return targetList;
        }

        
        /// <summary>
        /// 将版本号转换成数字.比如1.2.3转换成220102003
        /// 再比如1.02.30转换成220102030;
        /// 前两位暂定必须是22(2022年)，后面7位才是真正的版本号。7位中的前2位是主本版号，往后两位是次版本号，最后3位是再次版本号
        /// </summary>
        /// <param name="v1"></param>
        /// <param name="v2"></param>
        /// <returns></returns>
        public static int GetVersionCodeFromVersionStr(string versionStr)
        {
            string[] parts = versionStr.Split(new char[] {'.'}, StringSplitOptions.RemoveEmptyEntries);
            if (parts.Length != 3)
            {
                Debugger.LogDErrorFormat("错误的版本号格式,长度有误：{0}", versionStr);
            }

            if (parts[0].Length > 2 || parts[0].Length == 0)
            {
                Debugger.LogDErrorFormat("错误的版本号格式,第一部分长度有误：{0}", parts[0]);
                return 0;
            }
            
            if (parts[1].Length > 2 || parts[1].Length == 0)
            {
                Debugger.LogDErrorFormat("错误的版本号格式,第二部分长度有误：{0}", parts[1]);
                return 0;
            }
            
            if (parts[2].Length > 3 || parts[2].Length == 0)
            {
                Debugger.LogDErrorFormat("错误的版本号格式,第三部分长度有误：{0}", parts[2]);
                return 0;
            }
            
            int baseNum = 230000000;
            int firstPart = 0;
            if (!int.TryParse(parts[0], out firstPart))
            {
                Debugger.LogDErrorFormat("错误的版本号格式：第一部分有误:{0}", parts[0]);
                return 0;
            }
            firstPart *= 100000;
            
            int secondPart = 0;
            if (!int.TryParse(parts[1], out secondPart))
            {
                Debugger.LogDErrorFormat("错误的版本号格式：第二部分有误:{0}", parts[1]);
            }

            secondPart *= 1000;
            
            int thirdPart = 0;
            if (!int.TryParse(parts[2], out thirdPart))
            {
                Debugger.LogDErrorFormat("错误的版本号格式：第三部分有误:{0}", parts[2]);
            }

            thirdPart *= 1;

            return baseNum + firstPart + secondPart + thirdPart;
        }
        
        
        /// <summary>
        /// 将版本号转换成数字.比如1.2.3转换成220102003
        /// 再比如1.02.30转换成220102030;
        /// 前两位暂定必须是22(2022年)，后面7位才是真正的版本号。7位中的前2位是主本版号，往后两位是次版本号，最后3位是再次版本号
        /// </summary>
        /// <param name="v1"></param>
        /// <param name="v2"></param>
        /// <returns></returns>
        public static string GetVersionStringFromVersionCode(int versionCode)
        {
            string str = versionCode.ToString();
            if (str.Length != 9)
            {
                Debugger.LogDErrorFormat("错误的版本号格式,长度有误：{0}", versionCode);
                return "error version";
            }

            string p1 = int.Parse(str.Substring(2, 2)).ToString();
            string p2 = int.Parse(str.Substring(4, 2)).ToString();
            string p3 = int.Parse(str.Substring(6, 3)).ToString();
            return string.Format("{0}.{1}.{2}", p1, p2, p3);
        }

        /// <summary>
        /// 任意进制转换，可用于base36转换方法
        /// https://cloud.tencent.com/developer/article/1730078
        /// </summary>
        /// <param name="decimalNumber"></param>
        /// <param name="radix"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentException"></exception>
        public static string DecimalToArbitrarySystem(long decimalNumber, int radix)
        {
            const int BitsInLong = 64;
            const string Digits = "0123456789ABCDEFGHIJKLMNOPQRSTUVWXYZ";

            if (radix < 2 || radix > Digits.Length)
                Debug.LogError("The radix must be >= 2 and <= " +
                               Digits.Length);

            if (decimalNumber == 0)
                return "0";

            int index = BitsInLong - 1;
            long currentNumber = Math.Abs(decimalNumber);
            char[] charArray = new char[BitsInLong];

            while (currentNumber != 0)
            {
                int remainder = (int)(currentNumber % radix);
                charArray[index--] = Digits[remainder];
                currentNumber = currentNumber / radix;
            }

            string result = new String(charArray, index + 1, BitsInLong - index - 1);
            if (decimalNumber < 0)
            {
                result = "-" + result;
            }

            return result;
        }

        /// <summary>
        /// 对长字符串做处理
        /// </summary>
        /// <param name="originString"></param>
        /// <returns></returns>
        public static string BestFixStringWithEllipsis(string originString,int count = 10)
        {
            if (originString.Length > count)
            {
                return String.Concat(originString.Substring(0, count), "...");
            }

            return originString;
        }

    }
}