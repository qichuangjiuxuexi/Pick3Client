using AppBase.Analytics;
using AppBase.Config;
using AppBase.Utils;
using ThinkingData.Analytics;
using ThinkingData.Analytics.Utils;

namespace AppBase.ThirdParty
{
    /// <summary>
    /// 数数打点管理器
    /// </summary>
    public class AnalyticsThinkingManager : UserAnalyticsManager
    {
        protected override void OnInit()
        {
            base.OnInit();
            InitSDK();
        }

        /// <summary>
        /// 初始化数数SDK
        /// </summary>
        public override void InitSDK()
        {
            var config = GameBase.Instance.GetModule<ConfigManager>().GetConfigMap<string, GlobalConfig>(AAConst.GlobalConfig);
            var appId = config.TryGetValue(AppUtil.IsDebug ? GlobalConfigKeys.ThinkingAnalaticsID_Debug : GlobalConfigKeys.ThinkingAnalaticsID, out var appIdConfig) ? appIdConfig.Value : null;
            var url = config.TryGetValue(GlobalConfigKeys.ThinkingAnalaticsURL, out var urlConfig) ? urlConfig.Value : null;
            if (string.IsNullOrEmpty(appId) || string.IsNullOrEmpty(url)) return;
            var settings = new TDConfig(appId, url)
            {
                timeZone = TDTimeZone.UTC,
            };
            TDAnalytics.Init(settings);
            TDAnalytics.EnableAutoTrack(TDAutoTrackEventType.All);
            TDAnalytics.EnableThirdPartySharing(TDThirdPartyType.ADJUST);
            isInit = true;
            TrackCachedEvents();
        }

        /// <summary>
        /// 登录
        /// </summary>
        /// <param name="userId">用户名</param>
        public void Login(string userId)
        {
            if (!isInit) return;
            TDAnalytics.Login(userId);
        }

        /// <summary>
        /// 打点
        /// </summary>
        /// <param name="event">打点数据</param>
        public override void TrackEvent(AnalyticsEvent evt)
        {
            if (!isInit) return;
            if (evt == null || string.IsNullOrEmpty(evt.eventName)) return;
            TDAnalytics.Track(evt.eventName, evt.eventData);
        }
        
        /// <summary>
        /// 设置用户属性
        /// </summary>
        /// <param name="userProperties">属性数据</param>
        public override void SetUserProperties(AnalyticsUserProperties userProperties)
        {
            if (!isInit) return;
            if (userProperties == null || userProperties.Count == 0) return;
            TDAnalytics.UserSet(userProperties);
        }

        /// <summary>
        /// 设置用户属性，如果已经存在则不设置
        /// </summary>
        /// <param name="userProperties">属性数据</param>
        public override void SetUserPropertiesOnce(AnalyticsUserProperties userProperties)
        {
            if (!isInit) return;
            if (userProperties == null || userProperties.Count == 0) return;
            TDAnalytics.UserSetOnce(userProperties);
        }
    }
}