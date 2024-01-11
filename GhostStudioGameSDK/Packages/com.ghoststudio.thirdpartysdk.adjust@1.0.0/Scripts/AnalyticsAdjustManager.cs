using System.Linq;
using AppBase.Analytics;
using AppBase.Config;
using AppBase.Utils;
using com.adjust.sdk;

namespace AppBase.ThirdParty
{
    /// <summary>
    /// Adjust打点管理器
    /// </summary>
    public class AnalyticsAdjustManager : ThirdPartAnalyticsManager
    {
        public string gaid;
        
        protected override void OnInit()
        {
            base.OnInit();
            ((AnalyticsManager)ParentModule).RegisterUserProperty(AnalyticsUserPropertiesConfigKeys.idfa, () => Adjust.getIdfa(), true);
            ((AnalyticsManager)ParentModule).RegisterUserProperty(AnalyticsUserPropertiesConfigKeys.idfv, () => Adjust.getIdfv(), true);
            ((AnalyticsManager)ParentModule).RegisterUserProperty(AnalyticsUserPropertiesConfigKeys.gaid, () => gaid, true);
            InitSDK();
        }

        /// <summary>
        /// 初始化Adjust SDK
        /// </summary>
        public override void InitSDK()
        {
            var config = GameBase.Instance.GetModule<ConfigManager>().GetConfigMap<string, GlobalConfig>(AAConst.GlobalConfig);
            var appId = config.TryGetValue(AppUtil.IsIOS ? GlobalConfigKeys.AdjustTokenApple : GlobalConfigKeys.AdjustTokenGoogle, out var appIdConfig) ? appIdConfig.Value : null;
            var secret = config.TryGetValue(AppUtil.IsIOS ? GlobalConfigKeys.AdjustSecretApple : GlobalConfigKeys.AdjustSecretGoogle, out var secretConfig) ? secretConfig.Value : null;
            var fbId = config.TryGetValue(GlobalConfigKeys.FacebookId, out var fbIdConfig) ? fbIdConfig.Value : null;
            if (string.IsNullOrEmpty(appId) || string.IsNullOrEmpty(secret)) return;
            var settings = new AdjustConfig(appId, AppUtil.IsDebug ? AdjustEnvironment.Sandbox : AdjustEnvironment.Production);
            settings.setLogLevel(AppUtil.IsDebug ? AdjustLogLevel.Verbose : AdjustLogLevel.Info);
            settings.setLaunchDeferredDeeplink(true);
            var secrets = secret!.Split(',').Select(s => long.Parse(s.Trim())).ToArray();
            if (secrets.Length == 5)
            {
                settings.setAppSecret(secrets[0], secrets[1], secrets[2], secrets[3], secrets[4]);
            }
            settings.setFbAppId(fbId);
            Adjust.start(settings);
            Adjust.getGoogleAdId(id => gaid = id);
            isInit = true;
            TrackCachedEvents();
        }
        
        /// <summary>
        /// 打点事件
        /// </summary>
        /// <param name="event">事件数据</param>
        public override void TrackEvent(AnalyticsEvent evt)
        {
            //检查合法性
            if (!isInit || evt == null) return;
            if (evt is AnalyticsAdRevenueEvent adRevenueEventData)
            {
                TrackAdRevenue(adRevenueEventData);
                return;
            }
            if (string.IsNullOrEmpty(evt.eventName)) return;
            
            //获取Adjust Token
            var config = GameBase.Instance.GetModule<ConfigManager>().GetConfigByKey<string, AnalyticsThirdPartyConfig>(AAConst.AnalyticsThirdPartyConfig, evt.eventName);
            if (config == null) return;
            var adjustToken = AppUtil.IsIOS ? config.adjust_token_apple : config.adjust_token_google;
            if (string.IsNullOrEmpty(adjustToken)) return;
            
            //打点参数
            var adjustEvent = new AdjustEvent(adjustToken);
            foreach (var param in evt.eventData)
            {
                adjustEvent.addCallbackParameter(param.Key, param.Value.ToString());
            }
            if (evt is AnalyticsRevenueEvent revenueEvent && revenueEvent.revenue > 0)
            {
                adjustEvent.setRevenue(revenueEvent.revenue, revenueEvent.currency);
            }
            Adjust.trackEvent(adjustEvent);
        }

        /// <summary>
        /// 打点广告收入
        /// </summary>
        /// <param name="source">广告源</param>
        /// <param name="event">事件数据</param>
        private void TrackAdRevenue(AnalyticsAdRevenueEvent evt)
        {
            var adjustEvent = new AdjustAdRevenue(AdjustConfig.AdjustAdRevenueSourceAppLovinMAX);
            if (evt.revenue > 0)
            {
                adjustEvent.setRevenue(evt.revenue, evt.currency);
            }
            if (!string.IsNullOrEmpty(evt.network))
            {
                adjustEvent.setAdRevenueNetwork(evt.network);
            }
            if (!string.IsNullOrEmpty(evt.unit))
            {
                adjustEvent.setAdRevenueUnit(evt.unit);
            }
            if (!string.IsNullOrEmpty(evt.placement))
            {
                adjustEvent.setAdRevenuePlacement(evt.placement);
            }
            foreach (var param in evt.eventData)
            {
                adjustEvent.addCallbackParameter(param.Key, param.Value.ToString());
            }
            Adjust.trackAdRevenue(adjustEvent);
        }
        
        /// <summary>
        /// 设置push tokens，用于Uninstall and reinstall tracking
        /// </summary>
        /// <param name="token"></param>
        public void SetFcmToken(string token)
        {
            Adjust.setDeviceToken(token);
        }
    }
}