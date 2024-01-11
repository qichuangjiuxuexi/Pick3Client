using System;
using System.Collections.Generic;
using System.Linq;
using AppBase.Config;
using AppBase.Event;
using AppBase.Module;
using AppBase.Timing;
using AppBase.Utils;
using UnityEngine;

namespace AppBase.Analytics
{
    /// <summary>
    /// 打点管理器
    /// 不包含三方SDK，需要自己AddModule添加进来
    /// </summary>
    public class AnalyticsManager : ModuleBase
    {
        private Dictionary<string, Func<object>> PropertyGetters = new();
        private Dictionary<string, object> PropertyCache = new();
        private Dictionary<string, bool> PropertyEnableCache = new();

        private ConfigManager configManager => GameBase.Instance.GetModule<ConfigManager>();
        private Dictionary<int, AnalyticsUserAssetConfig> userAssetConfigs => configManager.GetConfigMap<int, AnalyticsUserAssetConfig>(AAConst.AnalyticsUserAssetConfig);
        
        protected override void OnInit()
        {
            base.OnInit();
            AddModule<EventModule>()
                .Subscribe<EventOnLoadFinished>(OnLoadFinishedEvent)
                .Subscribe<AnalyticsEvent>(TrackEvent)
                .Subscribe<AnalyticsLogNewbieStepEvent>(e => LogNewbieStep(e.eventName))
                .Subscribe<EventAllUserAssetChange>(TrackUserAssetEvent);
            LogNewbieStep(AnalyticsUserStepConfigKeys.log);
        }

        private void TrackUserAssetEvent(EventAllUserAssetChange evt)
        {
            if (userAssetConfigs.TryGetValue(evt.assetId, out var config))
            {
                long offest = evt.newAssetNum - evt.oldAssetNum;
                if (offest > 0)
                {
                    //增加
                    if (config.got_analysis)
                    {
                        TrackEvent(new AnalyticsEvent_resource_got(evt.assetId, evt.tag, offest, evt.newAssetNum));
                    }
                }
                else if (offest < 0)
                {
                    //减少
                    if (config.use_analysis)
                    {
                        TrackEvent(new AnalyticsEvent_resource_use(evt.assetId, evt.tag, -offest, evt.newAssetNum));
                    }
                }
            }
        }

        /// <summary>
        /// 打点统计事件（自动）
        /// </summary>
        /// <param name="event">事件数据</param>
        public void TrackEvent(AnalyticsEvent evt)
        {
            if (evt == null || string.IsNullOrEmpty(evt.eventName)) return;
            if (!TrackThirdPartyEvent(evt))
            {
                TrackUserEvent(evt);
            }
        }

        /// <summary>
        /// 新手漏斗
        /// </summary>
        public void LogNewbieStep(string key)
        {
            int newbieStep = PlayerPrefs.GetInt($"LogNewbieStep_{key}", 0);
            if (newbieStep == 0)
            {
                var config = configManager.GetConfigByKey<string, AnalyticsUserStepConfig>(AAConst.AnalyticsUserStepConfig, key);
                if (config != null)
                {
                    PlayerPrefs.SetInt($"LogNewbieStep_{key}", 1);
                    PlayerPrefs.SetInt($"LogNewbieStep_Cur", config.step_id);
                    PlayerPrefs.Save();
                    var evt = new AnalyticsEvent("guide");
                    evt.eventData["step"] = config.step_id;
                    TrackUserEvent(evt);
                }
            }
        }

        /// <summary>
        /// 设置用户属性（数数）
        /// </summary>
        /// <param name="userProperties">用户属性</param>
        public void SetUserProperties(AnalyticsUserProperties userProperties)
        {
            if (userProperties == null || userProperties.Count == 0) return;
            AnalyticsUserProperties userPropertiesOnce = null;
            var config = configManager.GetConfigMap<string, AnalyticsUserPropertiesConfig>(AAConst.AnalyticsUserPropertiesConfig);
            if (config != null)
            {
                foreach (var pair in userProperties)
                {
                    var property = config[pair.Key];
                    if (property == null) continue;
                    if (property.is_once)
                    {
                        userPropertiesOnce ??= new AnalyticsUserProperties();
                        userPropertiesOnce[pair.Key] = pair.Value;
                    }
                }
                if (userPropertiesOnce != null)
                {
                    foreach (var pair in userPropertiesOnce)
                    {
                        userProperties.Remove(pair.Key);
                    }
                }
            }
            foreach (var module in moduleList)
            {
                if (module is UserAnalyticsManager manager)
                {
                    manager.SetUserProperties(userProperties);
                    manager.SetUserPropertiesOnce(userPropertiesOnce);
                }
            }
        }

        /// <summary>
        /// 打点用户事件（数数）
        /// </summary>
        /// <param name="evt">事件数据</param>
        public void TrackUserEvent(AnalyticsEvent evt)
        {
            if (evt == null || string.IsNullOrEmpty(evt.eventName)) return;
            SetCommonEventProperties(evt);
            foreach (var module in moduleList)
            {
                if (module is UserAnalyticsManager manager)
                {
                    manager.TrackEventWithCache(evt);
                }
            }
        }
        
        /// <summary>
        /// 打点市场事件（Adjust、Firebase、Facebook等）
        /// </summary>
        /// <param name="evt">事件数据</param>
        public bool TrackThirdPartyEvent(AnalyticsEvent evt)
        {
            if (evt == null || string.IsNullOrEmpty(evt.eventName)) return false;
            var config = configManager.GetConfigByKey<string, AnalyticsThirdPartyConfig>(AAConst.AnalyticsThirdPartyConfig, evt.eventName);
            if (config == null) return false;
            foreach (var module in moduleList)
            {
                if (module is ThirdPartAnalyticsManager manager && module is not UserAnalyticsManager)
                {
                    manager.TrackEventWithCache(evt);
                }
            }
            return true;
        }

        /// <summary>
        /// 设置通用事件属性
        /// </summary>
        private void SetCommonEventProperties(AnalyticsEvent evt)
        {
            var list = configManager.GetConfigList<AnalyticsEventPropertiesConfig>(AAConst.AnalyticsEventPropertiesConfig);
            foreach (var config in list)
            {
                var value = GetPropertyValue(config.property_name);
                if (value != null)
                {
                    evt.eventData[config.property_name] = value;
                }
            }
        }
        
        /// <summary>
        /// 注册用户属性获取器
        /// </summary>
        /// <param name="propertyName">属性名称</param>
        /// <param name="getter">获取器回调</param>
        /// <param name="isEnableCache">是否缓存</param>
        public void RegisterUserProperty(string propertyName, Func<object> getter,bool isEnableCache)
        {
            if (string.IsNullOrEmpty(propertyName) || getter == null) return; 
            PropertyGetters[propertyName] = getter;
            PropertyEnableCache[propertyName] = isEnableCache;
        }
        
        /// <summary>
        /// 登录时收集用户属性
        /// </summary>
        public void CollectUserPropertiesOnLogin()
        {
            var config = configManager.GetConfigList<AnalyticsUserPropertiesConfig>(AAConst.AnalyticsUserPropertiesConfig);
            if (config == null) return;
            AnalyticsUserProperties userProperties = new();
            AnalyticsUserProperties userPropertiesOnce = new();
            for (int i = 0; i < config.Count; ++i)
            {
                var property = config[i];
                // if (!property.is_login_collect) continue;
                var value = GetPropertyValue(property.property_name);
                if (value == null) continue;
                if (property.is_once)
                {
                    userPropertiesOnce[property.property_name] = value;
                }
                else
                {
                    userProperties[property.property_name] = value;
                }
            }
            foreach (var module in moduleList)
            {
                if (module is UserAnalyticsManager manager)
                {
                    manager.SetUserProperties(userProperties);
                    manager.SetUserPropertiesOnce(userPropertiesOnce);
                }
            }
        }

        /// <summary>
        /// 获取用户属性值和公共事件属性
        /// </summary>
        /// <param name="propertyName">属性名</param>
        /// <param name="defaultValue">默认值</param>
        /// <returns>属性值</returns>
        public object GetPropertyValue(string propertyName, object defaultValue = null)
        {
            //取缓存值
            if (PropertyCache.TryGetValue(propertyName, out var value) && value != null) return value;
            //取getter值
            if (PropertyGetters.TryGetValue(propertyName, out var getter) && getter != null)
            {
                value = getter.Invoke();
                //缓存值
                if (value != null && PropertyEnableCache.TryGetValue(propertyName, out var isEnableCache) && isEnableCache)
                {
                    PropertyCache[propertyName] = value;
                }
                return value;
            }
            //通用事件属性取不到值时，不报错
            if (configManager.GetConfigByKey<string, AnalyticsEventPropertiesConfig>(AAConst.AnalyticsEventPropertiesConfig, propertyName) == null)
            {
                Debugger.LogError(TAG, $"PropertyGetters is not registered propertyName: {propertyName}");
            }
            return defaultValue;
        }

        /// <summary>
        /// 游戏加载完成，打点用户属性
        /// </summary>
        /// <param name="e"></param>
        private void OnLoadFinishedEvent(EventOnLoadFinished e)
        {
            CollectUserPropertiesOnLogin();
        }
    }
}