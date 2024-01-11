using System;
using System.Linq;
using UnityEngine;

namespace AppBase.Utils
{
    /// <summary>
    /// 跟包有关的工具类
    /// </summary>
    public static class AppUtil
    {
        /// <summary>
        /// 是否是测试包
        /// </summary>
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

        /// <summary>
        /// 是否是正式包
        /// </summary>
        public static bool IsRelease
        {
            get
            { 
                return !IsDebug;
            }
        }
        
        /// <summary>
        /// 是否是编辑器
        /// </summary>
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
        
        /// <summary>
        /// 是否是安卓包
        /// </summary>
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
    
        /// <summary>
        /// 设备系统版本号
        /// iOS12.0.1:"12.0.1"
        /// 8.0.0 / API-26 (HONORPRA-AL00/343(C00)):"26"
        /// </summary>
        public static string OSVersion()
        {
#if UNITY_ANDROID
            string systemOSInfo = SystemInfo.operatingSystem;
            int index = systemOSInfo.IndexOf("api-", StringComparison.CurrentCultureIgnoreCase) + 4;
            string systemOSInfoVersion = systemOSInfo.Substring(index, 2);
            return systemOSInfoVersion;
#else
            string systemOSInfo = SystemInfo.operatingSystem;
            string systemOSInfoVersion = new string(systemOSInfo.SkipWhile(c => !char.IsDigit(c)).ToArray());
            return systemOSInfoVersion;
#endif
        }
        
        /// <summary>
        /// 设备系统语言
        /// </summary>
        public static string SystemLanguage()
        {
            return Application.systemLanguage.ToString();
        }
        
        /// <summary>
        /// 设备时区偏移
        /// </summary>
        public static float TimeZoneOffest()
        {
            return TimeZoneInfo.Local.GetUtcOffset(DateTime.Now).Seconds/3600f;
        }


        /// <summary>
        /// 操作平台的名字
        /// </summary>
        public static string OSName()
        {
            if (IsAndroid)
                return "android";
            if (IsIOS)
                return "ios";
            return "other";
        }

        /// <summary>
        /// 是否是苹果包
        /// </summary>
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
        
        /// <summary>
        /// 设备Id
        /// IOS会存储在KeyChain
        /// </summary>
        public static string DeviceId
        {
            get
            {
                if (_deviceId != null) return _deviceId;
#if UNITY_IOS && !UNITY_EDITOR
                _deviceId = KeyChainUtil.Load(Application.identifier);
                if (string.IsNullOrEmpty(_deviceId))
                {
                    _deviceId = SystemInfo.deviceUniqueIdentifier;
                    KeyChainUtil.Save(Application.identifier, _deviceId);
                }
#else
                _deviceId = SystemInfo.deviceUniqueIdentifier;
#endif
                return _deviceId;
            }
        }
        private static string _deviceId;
        
        /// <summary>
        /// 设备型号
        /// </summary>
        public static string DeviceName
        {
            get
            {
                return SystemInfo.deviceName;
            }
        }
        
        /// <summary>
        /// Native版本号
        /// 如 1.0.0
        /// </summary>
        public static string ClientVersion
        {
            get
            {
                return Application.version;
            }
        }
    }
}