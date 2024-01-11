using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using AppBase.Event;
using UnityEngine;
using AppBase.Module;
using AppBase.Resource;

namespace AppBase.Config
{
    /// <summary>
    /// 配置文件获取 控制器
    /// </summary>
    public class ConfigManager : ModuleBase
    {
        /// <summary>
        /// Scriptable文件数据
        /// </summary>
        private Dictionary<string, ScriptableObject> configAssets = new ();

        /// <summary>
        /// 异步加载所有配置
        /// </summary>
        public IEnumerator LoadAllConfigs(Action<float> progress = null)
        {
            var addresses = ConfigNames.keyNames.Select(AAConst.GetAddress).Where(k => !string.IsNullOrEmpty(k)).ToArray();
            var handlers = GameBase.Instance.GetModule<ResourceManager>().LoadAssetes<ScriptableObject>(addresses, this.GetResourceReference(), finishedCallback: dict =>
            {
                configAssets = dict;
            });
            for (int i = 0; i < handlers.Count; i++)
            {
                yield return handlers[i];
                progress?.Invoke((i + 1) / (float)handlers.Count);
            }
        }

        /// <summary>
        /// 同步加载配置
        /// </summary>
        /// <param name="address">配置文件路径</param>
        protected T GetConfigScriptableObject<T>(string address) where T : class, IConfigList
        {
            if (string.IsNullOrEmpty(address)) return null;
            if (!configAssets.TryGetValue(address, out var asset))
            {
                Debugger.LogWarning(TAG, $"config {address} should use after ConfigManager loaded");
                var handler = GameBase.Instance.GetModule<ResourceManager>().LoadAsset<ScriptableObject>(address, this.GetResourceReference());
                asset = handler?.WaitForCompletion<ScriptableObject>();
                if (asset == null) return null;
                configAssets[address] = asset;
            }
            var config = asset as T;
            if (config == null)
            {
                Debugger.LogError(TAG, $"config {address} is not type {typeof(T)}");
            }
            return config;
        }

        /// <summary>
        /// 获取配置文件信息（数组）
        /// </summary>
        /// <typeparam name="T">对象类型</typeparam>
        /// <param name="address">配置文件路径</param>
        /// <returns>配置数组</returns>
        public List<T> GetConfigList<T>(string address) where T : BaseConfig
            => GetConfigScriptableObject<IConfigList<T>>(address)?.values ?? new();

        /// <summary>
        /// 获取配置文件信息（字典）
        /// </summary>
        /// <typeparam name="T">对象类型</typeparam>
        /// <typeparam name="K">Key类型</typeparam>
        /// <param name="address">配置文件路径</param>
        /// <returns>配置字典</returns>
        public Dictionary<K, T> GetConfigMap<K, T>(string address) where T : BaseConfig
            => GetConfigScriptableObject<BaseConfigDictionary<K, T>>(address)?.map ?? new();

        /// <summary>
        /// 根据字典Key查询配置条目
        /// </summary>
        /// <param name="address">配置文件路径</param>
        /// <param name="key">字典Key</param>
        /// <typeparam name="T">对象类型</typeparam>
        /// <typeparam name="K">Key类型</typeparam>
        /// <returns>配置条目</returns>
        public T GetConfigByKey<K, T>(string address, K key) where T : BaseConfig
        {
            var map = GetConfigMap<K, T>(address);
            if (!map.TryGetValue(key, out T result)) return default;
            return result;
        }

        /// <summary>
        /// 根据List的Index查询配置条目
        /// </summary>
        /// <param name="address">配置文件路径</param>
        /// <param name="index">List的Index，从0开始</param>
        /// <typeparam name="T">对象类型</typeparam>
        /// <returns>配置条目</returns>
        public T GetConfigByIndex<T>(string address, int index) where T : BaseConfig
        {
            var list = GetConfigList<T>(address);
            if (index < 0 || index >= list.Count) return default;
            return list[index];
        }
        
        /// <summary>
        /// 更新远程配置
        /// </summary>
        public void UpdateConfigs(Dictionary<string, ScriptableObject> configs)
        {
            if (configs == null || configs.Count == 0) return;
            foreach (var pair in configs)
            {
                configAssets[pair.Key] = pair.Value;
            }
            GameBase.Instance.GetModule<EventManager>().Broadcast(new OnConfigUpdateEvent(configs.Keys));
        }
    }
}
