using AppBase.Analytics;
using AppBase.Config;
using AppBase.Utils;
using Facebook.Unity;
using UnityEngine;

namespace AppBase.ThirdParty
{
    /// <summary>
    /// Facebook打点管理器
    /// </summary>
    public class AnalyticsFacebookManager : ThirdPartAnalyticsManager
    {
        private float lastTimeScale = 1;

        protected override void OnInit()
        {
            base.OnInit();
            InitSDK();
        }

        /// <summary>
        /// 初始化Facebook SDK
        /// </summary>
        public override void InitSDK()
        {
            if (AppUtil.IsDebug) return;
            var facebookId = GameBase.Instance.GetModule<ConfigManager>().GetConfigByKey<string, GlobalConfig>(AAConst.GlobalConfig, GlobalConfigKeys.FacebookId)?.Value;
            if (string.IsNullOrEmpty(facebookId)) return;
            if (!FB.IsInitialized)
            {
                FB.Init(InitCallback, OnHideUnity);
            }
            else
            {
                FB.ActivateApp();
                isInit = true;
                TrackCachedEvents();
            }
        }
        
        private void InitCallback()
        {
            if (FB.IsInitialized)
            {
                FB.ActivateApp();
                isInit = true;
                TrackCachedEvents();
            }
        }
        
        private void OnHideUnity(bool isGameShown)
        {
            if (!isGameShown)
            {
                lastTimeScale = Time.timeScale;
                Time.timeScale = 0;
            }
            else
            {
                Time.timeScale = lastTimeScale;
            }
        }

        /// <summary>
        /// 打点事件
        /// </summary>
        /// <param name="event">事件数据</param>
        public override void TrackEvent(AnalyticsEvent evt)
        {
            if (evt == null || string.IsNullOrEmpty(evt.eventName)) return;
            if (!isInit) return;
            var config = GameBase.Instance.GetModule<ConfigManager>().GetConfigByKey<string, AnalyticsThirdPartyConfig>(AAConst.AnalyticsThirdPartyConfig, evt.eventName);
            if (config == null || string.IsNullOrEmpty(config.facebook_name)) return;
            FB.LogAppEvent(config.facebook_name, null, evt.eventData);
        }
    }
}