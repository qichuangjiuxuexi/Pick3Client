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
using System.Linq;
using System.Text;
using UnityEngine;

namespace WordGame.Utils
{
    public class ToolApplication
    {
        /// <summary>
        /// 根据安装版本, 判定玩家是否为新用户
        /// </summary>
        public static bool IsNewerVersionByInstall(int installVersion)
        {
            //TODO version
            //return AppService.InstallVersion >= installVersion;
            return false;
        }

        /// <summary>
        /// 根据安装版本, 判定是否属于这一版本区间
        /// </summary>
        /// <param name="versionCode">版本号: 193</param>
        /// <param name="versionRange">版本区间 eg:"n,196;210,220;280,n"</param>
        /// <returns></returns>
        public static bool IsVersionInRange(int versionCode, string versionRange)
        {
            bool result = false;

            string[] segmentArr = versionRange.Split(new string[] {";"}, System.StringSplitOptions.RemoveEmptyEntries);
            foreach (string segment in segmentArr)
            {
                string[] verStartAndEnd =
                    segment.Split(new string[] {","}, System.StringSplitOptions.RemoveEmptyEntries);
                if (verStartAndEnd.Length == 2)
                {
                    //解析失败时, 保持默认值0
                    int startVer = 0;
                    int.TryParse(verStartAndEnd[0], out startVer);
                    int endVer = 0;
                    int.TryParse(verStartAndEnd[1], out endVer);
                    if (endVer <= 0)
                    {
                        endVer = int.MaxValue;
                    }

                    //两头包含小, 包也包含大(方便配置, 可读取性高)
                    if (versionCode >= startVer && versionCode <= endVer)
                    {
                        result = true;
                        break;
                    }
                }
                else
                {
                    Debugger.LogDError("error here, fix it now");
                }
            }

            return result;
        }

        /// <summary>
        /// 判断能不能在应用内, 弹出评价
        /// </summary>
        /// <returns></returns>
        public static bool CheckReviewInApp()
        {
#if UNITY_IOS && !UNITY_EDITOR
        string strVersion = GetDeviceOSVerionAsString();
        return CompareVersion(strVersion, "10.3") >= 0;
#else
            return false;
#endif
        }


        /// <summary>
        /// 设备系统版本号
        /// iOS12.0.1:"12.0.1"
        /// 8.0.0 / API-26 (HONORPRA-AL00/343(C00)):"26"
        /// </summary>
        /// <returns></returns>
        public static string GetDeviceOSVerionAsString()
        {
            //return Me2zen.Device.DeviceFactory.DeviceService.OSVersion;
#if UNITY_IOS
            string systemOSInfo = SystemInfo.operatingSystem;
            string systemOSInfoVersion = new String(systemOSInfo.SkipWhile(c => !Char.IsDigit(c)).ToArray());
            return systemOSInfoVersion;
#elif UNITY_ANDROID
            string systemOSInfo = SystemInfo.operatingSystem;
            int index = systemOSInfo.IndexOf("api-", StringComparison.CurrentCultureIgnoreCase) + 4;
            string systemOSInfoVersion = systemOSInfo.Substring(index, 2);
            return systemOSInfoVersion;
#else
            string systemOSInfo = SystemInfo.operatingSystem;
            string systemOSInfoVersion = new String(systemOSInfo.SkipWhile(c => !Char.IsDigit(c)).ToArray());
            return systemOSInfoVersion;
#endif
        }

        /// <summary>
        /// 设备系统版本号, 比较
        /// iOS
        ///     "12.0.1" vs "11.2.9"
        /// Android
        ///     "21" vs "17"
        /// 
        /// </summary>
        /// <returns></returns>
        public static int CompareVersion(string curVersion, string baseVersion)
        {
            if (curVersion == null || curVersion.Length == 0 || baseVersion == null || baseVersion.Length == 0)
            {
                Debugger.LogDError("error in CompareVersion");
                return -1;
            }

            int index1 = 0;
            int index2 = 0;
            while (index1 < curVersion.Length && index2 < baseVersion.Length)
            {
                int[] number1 = GetVersionSubCode(curVersion, index1);
                int[] number2 = GetVersionSubCode(baseVersion, index2);

                if (number1[0] < number2[0]) return -1;
                else if (number1[0] > number2[0]) return 1;
                else
                {
                    index1 = number1[1] + 1;
                    index2 = number2[1] + 1;
                }
            }

            if (index1 == curVersion.Length && index2 == baseVersion.Length) return 0;
            if (index1 < curVersion.Length)
                return 1;
            else
                return -1;
        }

        /// <summary>
        /// 得到版本号中的, 某一段
        /// </summary>
        /// <param name="strVersion"></param>
        /// <param name="index"></param>
        /// <returns></returns>
        static int[] GetVersionSubCode(string strVersion, int index)
        {
            int[] value_index = new int[2]; //了版本号和索引点
            StringBuilder sb = new StringBuilder();
            while (index < strVersion.Length && strVersion[index] != '.')
            {
                sb.Append(strVersion[index]);
                index++;
            }

            value_index[0] = int.Parse(sb.ToString());
            value_index[1] = index;
            return value_index;
        }

        public static bool IsDebug
        {
            get
            {
#if DEBUG
                return true;
#endif
                return false;
            }
        }

        public static bool IsRelease
        {
            get
            { 
                return !IsDebug;
            }
        }
        
        public static bool IsEditor
        {
            get
            {
#if UNITY_EDITOR
                return true;
#endif
                return false;
            }
        }
        
        public static bool IsAndroid
        {
            get
            {
#if UNITY_ANDROID
                return true;
#endif
                return false;
            }
        }
        
        public static bool IsIOS
        {
            get
            {
#if UNITY_IOS
                return true;
#endif
                return false;
            }
        }
    }
}