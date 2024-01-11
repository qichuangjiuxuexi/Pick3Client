using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;

namespace AppBase.Utils
{
    /// <summary>
    /// IOS钥匙串工具类
    /// </summary>
    public static class KeyChainUtil
    {
#if UNITY_IOS && !UNITY_EDITOR
        [DllImport("__Internal")]
        private static extern void _SaveToKeychain(string service, string data);

        [DllImport("__Internal")]
        private static extern IntPtr _LoadFromKeychain(string service);

        [DllImport("__Internal")]
        private static extern void _RemoveFromKeychain(string service);
#endif
        
        private static Dictionary<string, string> keyChainCache;

        /// <summary>
        /// 保存KeyChain
        /// </summary>
        public static void Save(string service, string data)
        {
#if UNITY_IOS && !UNITY_EDITOR
            if (string.IsNullOrEmpty(service)) return;
            if (data == null)
            {
                Remove(service);
                return;
            }
            _SaveToKeychain(service, data);
            keyChainCache ??= new();
            keyChainCache[service] = data;
#endif
        }

        /// <summary>
        /// 读取KeyChain
        /// </summary>
        public static string Load(string service)
        {
#if UNITY_IOS && !UNITY_EDITOR
            if (keyChainCache != null && keyChainCache.TryGetValue(service, out string cacheData))
            {
                return cacheData;
            }
            if (string.IsNullOrEmpty(service)) return null;
            IntPtr dataPtr = _LoadFromKeychain(service);
            if (dataPtr != IntPtr.Zero)
            {
                string data = Marshal.PtrToStringAnsi(dataPtr);
                Marshal.FreeHGlobal(dataPtr);
                if (data != null)
                {
                    keyChainCache ??= new();
                    keyChainCache[service] = data;
                }
                return data;
            }
#endif
            return null;
        }
        
        /// <summary>
        /// 删除KeyChain
        /// </summary>
        public static void Remove(string service)
        {
#if UNITY_IOS && !UNITY_EDITOR
            if (string.IsNullOrEmpty(service)) return;
            _RemoveFromKeychain(service);
            keyChainCache?.Remove(service);
#endif
        }
    }
}