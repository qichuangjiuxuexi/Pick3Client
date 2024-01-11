using System;
using System.Collections;
using System.Collections.Generic;
using AppBase.Config;
using AppBase.Event;
using AppBase.Module;
using AppBase.Timing;
using AppBase.Utils;
using UnityEngine;
using UnityEngine.Networking;

namespace AppBase.Network
{
    /// <summary>
    /// 网络远程配置管理器
    /// </summary>
    public class NetworkRemoteConfitManager : ModuleBase
    {
        public string ResourceServerUrl => GameBase.Instance.GetModule<ConfigManager>().GetConfigByKey<string, GlobalConfig>(AAConst.GlobalConfig, AppUtil.IsDebug ? GlobalConfigKeys.ResourceServerUrl_Debug : GlobalConfigKeys.ResourceServerUrl)?.Value;
        public string RemoteConfigUrl => string.IsNullOrEmpty(ResourceServerUrl) ? null : $"{ResourceServerUrl}/RemoteConfig";
        public string RemoteConfigCatalogUrl => string.IsNullOrEmpty(RemoteConfigUrl) ? null : $"{RemoteConfigUrl}/catalog.json";
        
        public const int timeout = 5;
        private bool IS_ENCRY => AppUtil.IsDebug;
        private string datPath => IS_ENCRY ? "RemoteConfig.dat" : "RemoteConfig.json";
        private NetworkRemoteConfigSaveData configData = new();
        
        protected override void OnInit()
        {
            base.OnInit();
            Debugger.SetLogEnable(TAG);
            LoadConfigData();
            GameBase.Instance.GetModule<ConfigManager>().UpdateConfigs(configData.configs);
            AddModule<EventModule>().Subscribe<EventOnGameFocus>(OnGameFocusEvent);
        }

        /// <summary>
        /// 切回前台时检查更新配置
        /// </summary>
        public void OnGameFocusEvent(EventOnGameFocus evt)
        {
            GameBase.Instance.GetModule<TimingManager>().StartCoroutine(RequestRemoteConfig());
        }

        public IEnumerator RequestRemoteConfig()
        {
            //Editor模式、没联网不更新配置
            if (AppUtil.IsEditor || Application.internetReachability == NetworkReachability.NotReachable || string.IsNullOrEmpty(ResourceServerUrl))
            {
                yield break;
            }
            NetworkRemoteConfigCatalog catalog = null;
            Debugger.Log(TAG, $"RequestRemoteConfigCatalog begin: {RemoteConfigCatalogUrl}");
            yield return RequestRemoteConfigCatalog(c => catalog = c);
            Debugger.Log(TAG, $"RequestRemoteConfigCatalog version: {catalog?.version}");
            if (catalog == null || catalog.version == configData.lastFetchTime)
            {
                yield break;
            }
            Dictionary<string, ScriptableObject> configs = new();
            foreach (var key in catalog.keys)
            {
                var address = AAConst.GetAddress(key);
                if (string.IsNullOrEmpty(address))
                {
                    Debugger.Log(TAG, $"address not found for key: {key}");
                    continue;
                }
                Debugger.Log(TAG, $"RequestRemoteConfigFile begin: {key}");
                yield return RequestRemoteConfigFile(key, address, configs);
            }
            Debugger.Log(TAG, $"RequestRemoteConfig finished: {string.Join(',', configData.configs.Keys)}");
            configData.configs = configs;
            configData.lastFetchTime = catalog.version;
            SaveConfigData();
            GameBase.Instance.GetModule<ConfigManager>().UpdateConfigs(configData.configs);
        }

        /// <summary>
        /// 请求catalog.json
        /// </summary>
        private IEnumerator RequestRemoteConfigCatalog(Action<NetworkRemoteConfigCatalog> callback)
        {
            using var webRequest = new UnityWebRequest(RemoteConfigCatalogUrl, "GET");
            webRequest.timeout = timeout;
            webRequest.downloadHandler = new DownloadHandlerBuffer();
            yield return webRequest.SendWebRequest();
            if (webRequest.result != UnityWebRequest.Result.Success) yield break;
            var json = webRequest.downloadHandler.text;
            if (string.IsNullOrEmpty(json)) yield break;
            var catalog = JsonUtil.DeserializeObject<NetworkRemoteConfigCatalog>(json);
            callback?.Invoke(catalog);
        }

        /// <summary>
        /// 请求配置json
        /// </summary>
        private IEnumerator RequestRemoteConfigFile(string key, string address, Dictionary<string, ScriptableObject> configs)
        {
            using var webRequest = new UnityWebRequest($"{RemoteConfigUrl}/{key}.json", "GET");
            webRequest.timeout = timeout;
            webRequest.downloadHandler = new DownloadHandlerBuffer();
            yield return webRequest.SendWebRequest();
            if (webRequest.result != UnityWebRequest.Result.Success)
            {
                Debugger.LogError(TAG, $"RequestRemoteConfigFile error: {webRequest.error}");
                yield break;
            }
            var json = webRequest.downloadHandler.text;
            if (string.IsNullOrEmpty(json)) yield break;
            var config = JsonUtil.DeserializeObject<ScriptableObject>(json);
            if (config == null) yield break;
            configs[address] = config;
        }

        /// <summary>
        /// 读取本地缓存的远程配置
        /// </summary>
        private void LoadConfigData()
        {
            configData.configs.Clear();
            configData.lastFetchTime = 0;
            if (!ES3.FileExists(datPath)) return;
            var json = IS_ENCRY ? EncryptUtil.DecryptString(ES3.LoadRawBytes(datPath)) : ES3.LoadRawString(datPath);
            if (string.IsNullOrEmpty(json)) return;
            var data = JsonUtil.DeserializeObject<NetworkRemoteConfigSaveData>(json);
            if (data != null) configData = data;
        }

        /// <summary>
        /// 保存远程配置到本地
        /// </summary>
        private void SaveConfigData()
        {
            var json = JsonUtil.SerializeArchive(configData, typeof(NetworkRemoteConfigSaveData));
            if (IS_ENCRY)
            {
                ES3.SaveRaw(EncryptUtil.EncryptString(json), datPath);
            }
            else
            {
                ES3.SaveRaw(json, datPath);
            }
        }
    }
}