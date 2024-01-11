using System;
using System.Linq;
using AppBase.Config;
using AppBase.Module;
using AppBase.Utils;
using Firebase;
using Firebase.Analytics;
using Firebase.Extensions;
using Firebase.RemoteConfig;
using UnityEngine;

namespace AppBase.ThirdParty
{
    /// <summary>
    /// Firebase远程配置管理器
    /// </summary>
    public class FirebaseRemoteConfigManager : ModuleBase
    {
        /// <summary>
        /// 是否加密
        /// </summary>
#if UNITY_EDITOR
        private const bool IS_ENCRY = false;
        private const string datPath = "RemoteConfig.json";
#else
        private const bool IS_ENCRY = true;
        private const string datPath = "RemoteConfig.dat";
#endif
        
        /// <summary>
        /// 请求超时，秒
        /// </summary>
        public const int timeout = 5;

        /// <summary>
        /// 缓存配置数据
        /// </summary>
        private FirebaseRemoteConfigSaveData configData = new();
        private bool isListened;
        
        protected override void OnInit()
        {
            base.OnInit();
            Debugger.SetLogEnable(TAG);
            LoadConfigData();
            GameBase.Instance.GetModule<ConfigManager>().UpdateConfigs(configData.configs);
        }

        /// <summary>
        /// 请求更新远程配置
        /// </summary>
        public void RequestRemoteConfig(Action<bool> callback = null)
        {
            //Editor模式、没联网不更新配置
            if (AppUtil.IsEditor || Application.internetReachability == NetworkReachability.NotReachable)
            {
                callback?.Invoke(false);
                return;
            }
            var flow = FlowUtil.Create();
            
            //初始化Firebase
            flow.Add(next => FirebaseApp.CheckAndFixDependenciesAsync().ContinueWithOnMainThread(task =>
            {
                if (task.Result == DependencyStatus.Available)
                {
                    //监听配置更新信号
                    if (!isListened)
                    {
                        isListened = true;
                        FirebaseRemoteConfig.DefaultInstance.OnConfigUpdateListener += OnRemoteConfigUpdate;
                    }
                    //配置用户信息
                    FirebaseAnalytics.SetUserProperty("udid", AppUtil.DeviceId);
                    FirebaseAnalytics.SetUserProperty("debug_build", AppUtil.IsDebug ? "true" : "false");
                    next?.Invoke();
                }
                else
                {
                    Debugger.LogError(TAG, $"Could not resolve all Firebase dependencies: {task.Result}");
                    callback?.Invoke(false);
                }
            }));
            
            //更改超时配置
            flow.Add(next =>
            {
                Debugger.Log(TAG, "SetConfigSettingsAsync begin");
                var configSettings = FirebaseRemoteConfig.DefaultInstance.ConfigSettings;
                configSettings.FetchTimeoutInMilliseconds = timeout * 1000;
                FirebaseRemoteConfig.DefaultInstance.SetConfigSettingsAsync(configSettings).ContinueWithOnMainThread(task =>
                {
                    next?.Invoke();
                });
            });
            
            //拉取配置
            flow.Add(next =>
            {
                Debugger.Log(TAG, "FetchAsync begin");
                FirebaseRemoteConfig.DefaultInstance.FetchAsync(TimeSpan.Zero).ContinueWithOnMainThread(fetchTask =>
                {
                    //超时
                    if (!fetchTask.IsCompleted)
                    {
                        Debugger.LogError(TAG, "FetchAsync timeout");
                        callback?.Invoke(false);
                        return;
                    }

                    //拉取失败
                    var configInfo = FirebaseRemoteConfig.DefaultInstance.Info;
                    if (configInfo.LastFetchStatus != LastFetchStatus.Success)
                    {
                        Debugger.LogError(TAG, $"FetchAsync failed: status: {configInfo.LastFetchStatus}, reason: {configInfo.LastFetchFailureReason}");
                        callback?.Invoke(false);
                        return;
                    }

                    next?.Invoke();
                });
            });
            
            //激活配置
            flow.Add(() =>
            {
                Debugger.Log(TAG, "FetchAsync success, ActivateAsync begin");
                FirebaseRemoteConfig.DefaultInstance.ActivateAsync().ContinueWithOnMainThread(activateTask =>
                {
                    //更新配置
                    Debugger.Log(TAG, "ActivateAsync finished");
                    UpdateConfigData();
                    callback?.Invoke(true);
                });
            });
            flow.Invoke();
        }

        /// <summary>
        /// 更新远程配置
        /// </summary>
        private void UpdateConfigData()
        {
            var remoteConfig = FirebaseRemoteConfig.DefaultInstance;
            configData.configs.Clear();
            foreach (var key in remoteConfig.Keys)
            {
                var address = AAConst.GetAddress(key);
                if (string.IsNullOrEmpty(address))
                {
                    Debugger.LogError(TAG, $"UpdateConfigData: address not found for key: {key}");
                    continue;
                }
                var value = remoteConfig.GetValue(key);
                //配置默认值时，使用包体内的配置
                if (value.Source != ValueSource.RemoteValue) continue;
                var json = value.StringValue;
                if (string.IsNullOrEmpty(json) || json == "{}") continue;
                //解析配置
                var config = JsonUtil.DeserializeObject<ScriptableObject>(json);
                if (config == null) continue;
                configData.configs[address] = config;
            }
            configData.lastFetchTime = ((DateTimeOffset)remoteConfig.Info.FetchTime).ToUnixTimeMilliseconds();
            SaveConfigData();
            GameBase.Instance.GetModule<ConfigManager>().UpdateConfigs(configData.configs);
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
            var data = JsonUtil.DeserializeObject<FirebaseRemoteConfigSaveData>(json);
            if (data != null) configData = data;
        }

        /// <summary>
        /// 保存远程配置到本地
        /// </summary>
        private void SaveConfigData()
        {
            var json = JsonUtil.SerializeArchive(configData, typeof(FirebaseRemoteConfigSaveData));
            if (IS_ENCRY)
            {
                ES3.SaveRaw(EncryptUtil.EncryptString(json), datPath);
            }
            else
            {
                ES3.SaveRaw(json, datPath);
            }
        }
        
        /// <summary>
        /// 删除所有远程配置
        /// </summary>
        public void ClearConfigData()
        {
            configData.configs.Clear();
            configData.lastFetchTime = 0;
            ES3.DeleteFile(datPath);
        }

        /// <summary>
        /// 收到远程配置更新信号
        /// </summary>
        private void OnRemoteConfigUpdate(object sender, ConfigUpdateEventArgs args)
        {
            if (args.Error != RemoteConfigError.None)
            {
                Debugger.LogError(TAG, $"Error occurred while listening: {args.Error}");
                return;
            }
            
            //打印更新的配置key
            var keys = args.UpdatedKeys.ToArray();
            if (keys.Length > 0)
            {
                Debugger.Log(TAG, $"OnRemoteConfigUpdate: keys = [{string.Join(',', keys)}], ActivateAsync begin");
            }
            else
            {
                Debugger.Log(TAG, $"OnRemoteConfigUpdate: keys = [], nothing to update");
                return;
            }
            
            //激活配置
            FirebaseRemoteConfig.DefaultInstance.ActivateAsync().ContinueWithOnMainThread(activateTask =>
            {
                //更新配置
                Debugger.Log(TAG, "OnRemoteConfigUpdate: ActivateAsync finished");
                UpdateConfigData();
            });
        }

        protected sealed override void OnInternalDestroy()
        {
            base.OnInternalDestroy();
            if (isListened)
            {
                FirebaseRemoteConfig.DefaultInstance.OnConfigUpdateListener -= OnRemoteConfigUpdate;
            }
        }
    }
}