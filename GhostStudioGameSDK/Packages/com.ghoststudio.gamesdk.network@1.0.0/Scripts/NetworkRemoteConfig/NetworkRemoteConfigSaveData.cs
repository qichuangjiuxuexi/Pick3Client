using System;
using System.Collections.Generic;
using UnityEngine;

namespace AppBase.Network
{
    /// <summary>
    /// 缓存配置数据
    /// </summary>
    [Serializable]
    public class NetworkRemoteConfigSaveData
    {
        /// <summary>
        /// 缓存的远程配置，键是映射的address
        /// </summary>
        public Dictionary<string, ScriptableObject> configs = new();
        
        /// <summary>
        /// 获取时间戳，毫秒
        /// </summary>
        public long lastFetchTime;
    }
}