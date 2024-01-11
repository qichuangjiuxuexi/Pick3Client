using System;
using AppBase.Analytics;
using AppBase.Config;
using AppBase.Event;
using AppBase.Module;
using AppBase.Timing;
using AppBase.Utils;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace AppBase.Ads
{
    /// <summary>
    /// Applovin Max 广告管理器，与业务无关
    /// </summary>
    public class MaxAdsManager : ModuleBase
    {
        protected string adsSdkId => GameBase.Instance.GetModule<ConfigManager>().GetConfigByKey<string, GlobalConfig>(AAConst.GlobalConfig, AppUtil.IsIOS ? GlobalConfigKeys.AdsSdkIdApple : GlobalConfigKeys.AdsSdkIdGoogle)?.Value;
        protected string adsRewardVideoId => GameBase.Instance.GetModule<ConfigManager>().GetConfigByKey<string, GlobalConfig>(AAConst.GlobalConfig, AppUtil.IsIOS ? GlobalConfigKeys.AdsRewardVideoIdApple : GlobalConfigKeys.AdsRewardVideoIdGoogle)?.Value;
        protected string adsInterstitialId => GameBase.Instance.GetModule<ConfigManager>().GetConfigByKey<string, GlobalConfig>(AAConst.GlobalConfig, AppUtil.IsIOS ? GlobalConfigKeys.AdsInterstitialIdApple : GlobalConfigKeys.AdsInterstitialIdGoogle)?.Value;
        protected string adsBannerId => GameBase.Instance.GetModule<ConfigManager>().GetConfigByKey<string, GlobalConfig>(AAConst.GlobalConfig, AppUtil.IsIOS ? GlobalConfigKeys.AdsBannerIdApple : GlobalConfigKeys.AdsBannerIdGoogle)?.Value;
        public bool IsSimulateDebug;
        
        /// <summary>
        /// 广告存档
        /// </summary>
        public MaxAdsRecord AdsRecord;
        
        public void InitSDK()
        {
            Debugger.SetLogEnable(TAG);
            AddModule<AdsSROptions>(); 
            if (string.IsNullOrEmpty(adsSdkId))
            {
                LogInfo("Initialize", "adsSdkId is empty");
                return;
            }
            if (MaxSdk.IsInitialized())
            {
                LogInfo("Initialize", "Max already Initialized");
                return;
            }
            LogInfo("Initialize", adsSdkId);
            MaxSdkCallbacks.OnSdkInitializedEvent += OnInitialized;
            MaxSdk.SetSdkKey(adsSdkId);
            MaxSdk.SetUserId(AppUtil.DeviceId);
            MaxSdk.InitializeSdk();
            AdsRecord = AddModule<MaxAdsRecord>();
            RegisterPaymentProperty();
            
        }
        
        #region 注册广告相关用户属性

        private void RegisterPaymentProperty()
        {
            
            var Analytics = GameBase.Instance.GetModule<AnalyticsManager>();
            
            Analytics.RegisterUserProperty(AnalyticsUserPropertiesConfigKeys.user_interstitial_video_times,
                () => AdsRecord.ArchiveData.interstitialCount, false);
            Analytics.RegisterUserProperty(AnalyticsUserPropertiesConfigKeys.first_interstitial_video_time,
                () => TimeUtil.ConvertUnixTimestampToDateTimeString(AdsRecord.ArchiveData.firstInterstitialTime), false);
            Analytics.RegisterUserProperty(AnalyticsUserPropertiesConfigKeys.first_reward_video_time,
                () => TimeUtil.ConvertUnixTimestampToDateTimeString(AdsRecord.ArchiveData.firstRewardTime), false);
            Analytics.RegisterUserProperty(AnalyticsUserPropertiesConfigKeys.user_reward_video_times,
                () => AdsRecord.ArchiveData.rewardCount, false);
            
        }

        #endregion
        
        #region Cmp

        public bool IsGDPR()
        {
            return MaxSdk.GetSdkConfiguration()?.ConsentFlowUserGeography == 
                   MaxSdkBase.ConsentFlowUserGeography.Gdpr;
        }

        public async UniTask<bool> LoadAndShowCmpFlow()
        {
            var error = await UniTaskUtil.InvokeAsync<MaxCmpError>(MaxSdk.CmpService.ShowCmpForExistingUser);
            if (null != error)
            {
                /*
                 *   Code = GetCode(MaxSdkUtils.GetIntFromDictionary(error, "code")),
                   Message = MaxSdkUtils.GetStringFromDictionary(error, "message"),
                   CmpCode = MaxSdkUtils.GetIntFromDictionary(error, "cmpCode", -1),
                   CmpMessage = MaxSdkUtils.GetStringFromDictionary(error, "cmpMessage")
                 */
                Debugger.LogError(TAG, $"LoadAndShowCmpFlow error Code== {error.Code}  Code== {error.CmpCode}  Message =={error.Message}  CmpCode == {error.CmpCode} CmpMessage == {error.CmpMessage}");
            }
            return error == null;
        }
        
        #endregion
        
        protected void OnInitialized(MaxSdkBase.SdkConfiguration sdkConfiguration)
        {
            LogInfo("OnInitialized", sdkConfiguration.CountryCode);
            if (IsGDPR())
            {
                if (PlayerPrefs.GetInt("Match3d_MaxGdpr",0)==0)
                {
                    LoadAndShowCmpFlow().ContinueWith(isSuccess =>
                    {
                        if (isSuccess)
                        {  
                            PlayerPrefs.SetInt("Match3d_MaxGdpr", 1);
                            PlayerPrefs.Save();
                        }
                    }).Forget();
                }
            }
            InitializeInterstitialAds();
            InitializeRewardedAds();
            InitializeBannerAds();
        }

        #region 插屏广告打点

        public void Logads_interstitial(string network, string position, string status)
        {
            AnalyticsEvent evt = new AnalyticsEvent_ads_interstitial(network, position, status, "normal");
            GameBase.Instance.GetModule<EventManager>().Broadcast(evt);

        }

        #endregion
        
        #region 插屏广告
        
        int retryAttemptIv;

        private void InitializeInterstitialAds()
        {
            if (string.IsNullOrEmpty(adsInterstitialId))
            {
                LogInfo("InitializeInterstitialAds", "adsInterstitialId is empty");
                return;
            }
            
            // Attach callback
            MaxSdkCallbacks.Interstitial.OnAdLoadedEvent += OnInterstitialLoadedEvent;
            MaxSdkCallbacks.Interstitial.OnAdLoadFailedEvent += OnInterstitialLoadFailedEvent;
            MaxSdkCallbacks.Interstitial.OnAdDisplayedEvent += OnInterstitialDisplayedEvent;
            MaxSdkCallbacks.Interstitial.OnAdClickedEvent += OnInterstitialClickedEvent;
            MaxSdkCallbacks.Interstitial.OnAdHiddenEvent += OnInterstitialHiddenEvent;
            MaxSdkCallbacks.Interstitial.OnAdDisplayFailedEvent += OnInterstitialAdFailedToDisplayEvent;
            MaxSdkCallbacks.Interstitial.OnAdRevenuePaidEvent += OnInterstitialAdRevenuePaidEvent;
        
            // Load the first interstitial
            LoadInterstitial("load_initialize");
        }

        private void DestroyInterstitialAds()
        {
            MaxSdkCallbacks.Interstitial.OnAdLoadedEvent -= OnInterstitialLoadedEvent;
            MaxSdkCallbacks.Interstitial.OnAdLoadFailedEvent -= OnInterstitialLoadFailedEvent;
            MaxSdkCallbacks.Interstitial.OnAdDisplayedEvent -= OnInterstitialDisplayedEvent;
            MaxSdkCallbacks.Interstitial.OnAdClickedEvent -= OnInterstitialClickedEvent;
            MaxSdkCallbacks.Interstitial.OnAdHiddenEvent -= OnInterstitialHiddenEvent;
            MaxSdkCallbacks.Interstitial.OnAdDisplayFailedEvent -= OnInterstitialAdFailedToDisplayEvent;
            MaxSdkCallbacks.Interstitial.OnAdRevenuePaidEvent -= OnInterstitialAdRevenuePaidEvent;
        }

        private void LoadInterstitial(string pos)
        {
            LogInfo("LoadInterstitial", adsInterstitialId);
            if (!string.IsNullOrEmpty(pos))
                Logads_interstitial("", pos, ADSStatus.load.ToString());
            MaxSdk.LoadInterstitial(adsInterstitialId);
        }

        private void OnInterstitialLoadedEvent(string adUnitId, MaxSdkBase.AdInfo adInfo)
        {
            // Interstitial ad is ready for you to show. MaxSdk.IsInterstitialReady(adUnitId) now returns 'true'
            LogInfo("OnInterstitialLoadedEvent", adInfo.NetworkName);
            Logads_interstitial(adInfo.NetworkName, "", ADSStatus.load_success.ToString());
            // Reset retry attempt
            retryAttemptIv = 0;
        }

        private void OnInterstitialLoadFailedEvent(string adUnitId, MaxSdkBase.ErrorInfo errorInfo)
        {
            // Interstitial ad failed to load 
            // AppLovin recommends that you retry with exponentially higher delays, up to a maximum delay (in this case 64 seconds)
            LogInfo("OnInterstitialLoadFailedEvent", errorInfo.Message);

            retryAttemptIv++;
            int retryDelay = (int)(Math.Pow(2, Math.Min(6, retryAttemptIv)) * 1000);
            GameBase.Instance.GetModule<TimingManager>().GlobalDelayCall(retryDelay, ()=>LoadInterstitial(""));
        }

        private void OnInterstitialDisplayedEvent(string adUnitId, MaxSdkBase.AdInfo adInfo)
        {
            LogInfo("OnInterstitialDisplayedEvent", adInfo.NetworkName);
            Logads_interstitial(adInfo.NetworkName,"",ADSStatus.show_success.ToString());
        }

        private void OnInterstitialAdFailedToDisplayEvent(string adUnitId, MaxSdkBase.ErrorInfo errorInfo, MaxSdkBase.AdInfo adInfo)
        {
            LogError("OnInterstitialAdFailedToDisplayEvent", $"{adInfo.NetworkName} {errorInfo.Message}");
            Logads_interstitial(adInfo.NetworkName,"",ADSStatus.show_fail.ToString());
            // Interstitial ad failed to display. AppLovin recommends that you load the next ad.
            LoadInterstitial("load_failed");
        }

        private void OnInterstitialClickedEvent(string adUnitId, MaxSdkBase.AdInfo adInfo)
        {
            LogInfo("OnInterstitialClickedEvent", adInfo.NetworkName);
            Logads_interstitial(adInfo.NetworkName,"",ADSStatus.click.ToString());
        }

        private void OnInterstitialHiddenEvent(string adUnitId, MaxSdkBase.AdInfo adInfo)
        {
            LogInfo("OnInterstitialHiddenEvent", adInfo.NetworkName);
            Logads_interstitial(adInfo.NetworkName,"",ADSStatus.close.ToString());
            // Interstitial ad is hidden. Pre-load the next ad.
            LoadInterstitial("load_next");
        }

        private void OnInterstitialAdRevenuePaidEvent(string adUnitId, MaxSdkBase.AdInfo adInfo)
        {
            LogInfo("OnInterstitialAdRevenuePaidEvent", $"{adInfo.NetworkName} ${adInfo.Revenue}");
            Logads_interstitial(adInfo.NetworkName,"",ADSStatus.pay.ToString());
            // Ad revenue paid. Use this callback to track user revenue.
            GameBase.Instance.GetModule<EventManager>().Broadcast(new OnAdsRevenuePaid(adInfo.CreateAdsInfo(), OnAdsType.Interstitial));
        }

        /// <summary>
        /// 插屏广告是否准备好
        /// </summary>
        public bool IsInterstitialReady
        {
            get
            {
                if (string.IsNullOrEmpty(adsInterstitialId)) return false;
                if (AppUtil.IsDebug && IsSimulateDebug) return true;
                return MaxSdk.IsInterstitialReady(adsInterstitialId);
            }
        }

        /// <summary>
        /// 展示插屏广告
        /// </summary>
        public bool ShowInterstitial(string postion)
        {
            LogInfo("ShowInterstitial", $"IsInterstitialReady {IsInterstitialReady}");
            if (!IsInterstitialReady) return false;
            Logads_interstitial("",postion,ADSStatus.show.ToString());
            MaxSdk.ShowInterstitial(adsInterstitialId);
            AdsRecord.ShowInterstitial();
            return true;
        }
        
        #endregion

        #region RV视频广告打点

        void Logads_rewarded(string network, string position, string status)
        {
            AnalyticsEvent evt = new AnalyticsEvent_ads_rv(network, position, status, "normal");
            GameBase.Instance.GetModule<EventManager>().Broadcast(evt);
        }

        #endregion

        #region 视频广告

        int retryAttemptRv;
        Action<bool> onRewardedAdCompleted;

        private void InitializeRewardedAds()
        {
            if (string.IsNullOrEmpty(adsRewardVideoId))
            {
                LogInfo("InitializeRewardedAds", "adsRewardVideoId is empty");
                return;
            }
            
            // Attach callback
            MaxSdkCallbacks.Rewarded.OnAdLoadedEvent += OnRewardedAdLoadedEvent;
            MaxSdkCallbacks.Rewarded.OnAdLoadFailedEvent += OnRewardedAdLoadFailedEvent;
            MaxSdkCallbacks.Rewarded.OnAdDisplayedEvent += OnRewardedAdDisplayedEvent;
            MaxSdkCallbacks.Rewarded.OnAdClickedEvent += OnRewardedAdClickedEvent;
            MaxSdkCallbacks.Rewarded.OnAdRevenuePaidEvent += OnRewardedAdRevenuePaidEvent;
            MaxSdkCallbacks.Rewarded.OnAdHiddenEvent += OnRewardedAdHiddenEvent;
            MaxSdkCallbacks.Rewarded.OnAdDisplayFailedEvent += OnRewardedAdFailedToDisplayEvent;
            MaxSdkCallbacks.Rewarded.OnAdReceivedRewardEvent += OnRewardedAdReceivedRewardEvent;
                    
            // Load the first rewarded ad
            LoadRewardedAd("load_initialize");
        }

        private void DestroyRewardedAds()
        {
            MaxSdkCallbacks.Rewarded.OnAdLoadedEvent -= OnRewardedAdLoadedEvent;
            MaxSdkCallbacks.Rewarded.OnAdLoadFailedEvent -= OnRewardedAdLoadFailedEvent;
            MaxSdkCallbacks.Rewarded.OnAdDisplayedEvent -= OnRewardedAdDisplayedEvent;
            MaxSdkCallbacks.Rewarded.OnAdClickedEvent -= OnRewardedAdClickedEvent;
            MaxSdkCallbacks.Rewarded.OnAdRevenuePaidEvent -= OnRewardedAdRevenuePaidEvent;
            MaxSdkCallbacks.Rewarded.OnAdHiddenEvent -= OnRewardedAdHiddenEvent;
            MaxSdkCallbacks.Rewarded.OnAdDisplayFailedEvent -= OnRewardedAdFailedToDisplayEvent;
            MaxSdkCallbacks.Rewarded.OnAdReceivedRewardEvent -= OnRewardedAdReceivedRewardEvent;
        }

        private void LoadRewardedAd(string pos)
        {
            LogInfo("LoadRewardedAd", adsRewardVideoId);
            if (!string.IsNullOrEmpty(pos))
                Logads_rewarded("", pos, ADSStatus.load.ToString());
            MaxSdk.LoadRewardedAd(adsRewardVideoId);
        }

        private void OnRewardedAdLoadedEvent(string adUnitId, MaxSdkBase.AdInfo adInfo)
        {
            // Rewarded ad is ready for you to show. MaxSdk.IsRewardedAdReady(adUnitId) now returns 'true'.
            LogInfo("OnRewardedAdLoadedEvent", adInfo.NetworkName);
            Logads_rewarded(adInfo.NetworkName, "", ADSStatus.load_success.ToString());
            // Reset retry attempt
            retryAttemptRv = 0;
            GameBase.Instance.GetModule<EventManager>().Broadcast(new OnAdsRewardVideoReadyChanged(true));
        }

        private void OnRewardedAdLoadFailedEvent(string adUnitId, MaxSdkBase.ErrorInfo errorInfo)
        {
            // Rewarded ad failed to load 
            // AppLovin recommends that you retry with exponentially higher delays, up to a maximum delay (in this case 64 seconds).
            LogInfo("OnRewardedAdLoadFailedEvent", errorInfo.Message);
            
            retryAttemptRv++;
            int retryDelay = (int)(Math.Pow(2, Math.Min(6, retryAttemptRv)) * 1000);
            GameBase.Instance.GetModule<TimingManager>().GlobalDelayCall(retryDelay, ()=>LoadRewardedAd(""));
        }

        private void OnRewardedAdDisplayedEvent(string adUnitId, MaxSdkBase.AdInfo adInfo) {
            LogInfo("OnRewardedAdClickedEvent", adInfo.NetworkName);
            Logads_rewarded(adInfo.NetworkName, "", ADSStatus.show_success.ToString());
            GameBase.Instance.GetModule<EventManager>().Broadcast(new OnAdsRewardVideoReadyChanged(false));
        }

        private void OnRewardedAdFailedToDisplayEvent(string adUnitId, MaxSdkBase.ErrorInfo errorInfo, MaxSdkBase.AdInfo adInfo)
        {
            LogError("OnRewardedAdFailedToDisplayEvent", $"{adInfo.NetworkName} {errorInfo.Message}");
            GameBase.Instance.GetModule<EventManager>().Broadcast(new OnAdsRewardVideoReadyChanged(false));
            // Rewarded ad failed to display. AppLovin recommends that you load the next ad.
            Logads_rewarded(adInfo.NetworkName, "", ADSStatus.show.ToString());
            LoadRewardedAd("load_failed");
            onRewardedAdCompleted?.Invoke(false);
            onRewardedAdCompleted = null;
        }

        private void OnRewardedAdClickedEvent(string adUnitId, MaxSdkBase.AdInfo adInfo)
        {
            LogInfo("OnRewardedAdClickedEvent", adInfo.NetworkName);
            Logads_rewarded(adInfo.NetworkName, "", ADSStatus.click.ToString());
        }

        private void OnRewardedAdHiddenEvent(string adUnitId, MaxSdkBase.AdInfo adInfo)
        {
            LogInfo("OnRewardedAdHiddenEvent", adInfo.NetworkName);
            // Rewarded ad is hidden. Pre-load the next ad
            Logads_rewarded(adInfo.NetworkName, "", ADSStatus.close.ToString());
            LoadRewardedAd("load_next");
            onRewardedAdCompleted?.Invoke(false);
            onRewardedAdCompleted = null;
        }

        private void OnRewardedAdReceivedRewardEvent(string adUnitId, MaxSdk.Reward reward, MaxSdkBase.AdInfo adInfo)
        {
            LogInfo("OnRewardedAdReceivedRewardEvent", $"{adInfo.NetworkName} {reward.Amount}");
            Logads_rewarded(adInfo.NetworkName, "", ADSStatus.reward.ToString());
            // The rewarded ad displayed and the user should receive the reward.
            GameBase.Instance.GetModule<EventManager>().Broadcast<OnAdsRewardVideoWatchSucceed>();
            AdsRecord.RewardRewardedAd();
            onRewardedAdCompleted?.Invoke(true);
            onRewardedAdCompleted = null;
        }

        private void OnRewardedAdRevenuePaidEvent(string adUnitId, MaxSdkBase.AdInfo adInfo)
        {
            LogInfo("OnRewardedAdReceivedRewardEvent", $"{adInfo.NetworkName} ${adInfo.Revenue}");
            Logads_rewarded(adInfo.NetworkName, "", ADSStatus.pay.ToString());
            // Ad revenue paid. Use this callback to track user revenue.
            GameBase.Instance.GetModule<EventManager>().Broadcast(new OnAdsRevenuePaid(adInfo.CreateAdsInfo(),OnAdsType.reward));
        }

        /// <summary>
        /// 视频广告是否准备好
        /// </summary>
        public bool IsRewardedAdReady
        {
            get
            {
                if (string.IsNullOrEmpty(adsRewardVideoId)) return false;
                if (AppUtil.IsDebug && IsSimulateDebug) return true;
                return MaxSdk.IsRewardedAdReady(adsRewardVideoId);
            }
        }

        /// <summary>
        /// 展示视频广告
        /// </summary>
        public bool ShowRewardedAd(Action<bool> callback,string position)
        {
            LogInfo("ShowRewardedAd", $"IsRewardedAdReady {IsRewardedAdReady}");
            if (!IsRewardedAdReady) return false;
            Logads_rewarded("",position,ADSStatus.show.ToString());
            onRewardedAdCompleted = callback; 
            MaxSdk.ShowRewardedAd(adsRewardVideoId);
            return true;
        }

        #endregion

        #region 横幅广告

        private bool isLoadingBanner;
        private bool isBannerReady;
        private bool isBannerShowing;
        private int retryAttemptBanner;
        public bool IsBannerReady => isBannerReady;

        private void InitializeBannerAds()
        {
            if (string.IsNullOrEmpty(adsBannerId))
            {
                LogInfo("InitializeBannerAds", "adsBannerId is empty");
                return;
            }
            MaxSdkCallbacks.Banner.OnAdLoadedEvent      += OnBannerAdLoadedEvent;
            MaxSdkCallbacks.Banner.OnAdLoadFailedEvent  += OnBannerAdLoadFailedEvent;
            MaxSdkCallbacks.Banner.OnAdClickedEvent     += OnBannerAdClickedEvent;
            MaxSdkCallbacks.Banner.OnAdRevenuePaidEvent += OnBannerAdRevenuePaidEvent;
        }

        private void DestroyBannerAds()
        {
            MaxSdkCallbacks.Banner.OnAdLoadedEvent      -= OnBannerAdLoadedEvent;
            MaxSdkCallbacks.Banner.OnAdLoadFailedEvent  -= OnBannerAdLoadFailedEvent;
            MaxSdkCallbacks.Banner.OnAdClickedEvent     -= OnBannerAdClickedEvent;
            MaxSdkCallbacks.Banner.OnAdRevenuePaidEvent -= OnBannerAdRevenuePaidEvent;
        }

        private void OnBannerAdLoadedEvent(string adUnitId, MaxSdkBase.AdInfo adInfo)
        {
            LogInfo("OnBannerAdLoadedEvent", adInfo.NetworkName);
            isLoadingBanner = false;
            isBannerReady = true;
            isBannerShowing = false;
            retryAttemptBanner = 0;
            GameBase.Instance.GetModule<EventManager>().Broadcast(new OnAdsBannerReadyChanged(isBannerReady));
        }

        private void OnBannerAdLoadFailedEvent(string adUnitId, MaxSdkBase.ErrorInfo errorInfo)
        {
            LogInfo("OnBannerAdLoadFailedEvent", errorInfo.Message);
            isLoadingBanner = false;
            
            retryAttemptBanner++;
            int retryDelay = (int)(Math.Pow(2, Math.Min(6, retryAttemptBanner)) * 1000);
            GameBase.Instance.GetModule<TimingManager>().GlobalDelayCall(retryDelay, () => LoadBannerAd());
        }

        private void OnBannerAdClickedEvent(string adUnitId, MaxSdkBase.AdInfo adInfo)
        {
            LogInfo("OnBannerAdClickedEvent", adInfo.NetworkName);
        }

        private void OnBannerAdRevenuePaidEvent(string adUnitId, MaxSdkBase.AdInfo adInfo)
        {
            LogInfo("OnBannerAdRevenuePaidEvent", $"{adInfo.NetworkName} ${adInfo.Revenue}");
            GameBase.Instance.GetModule<EventManager>().Broadcast(new OnAdsRevenuePaid(adInfo.CreateAdsInfo(),OnAdsType.Banner));
        }

        /// <summary>
        /// 加载Banner
        /// </summary>
        public bool LoadBannerAd()
        {
            if (string.IsNullOrEmpty(adsBannerId))
            {
                LogInfo("ShowBannerAd", "adsBannerId is empty");
                return false;
            }
            if (isLoadingBanner)
            {
                LogInfo("LoadBanner", "Already loading banner");
                return false;
            }
            LogInfo("LoadBanner", adsBannerId);
            isLoadingBanner = true;
            
            // Banners are automatically sized to 320×50 on phones and 728×90 on tablets
            // You may call the utility method MaxSdkUtils.isTablet() to help with view sizing adjustments
            MaxSdk.CreateBanner(adsBannerId, MaxSdkBase.BannerPosition.BottomCenter);

            // Set background or background color for banners to be fully functional
            MaxSdk.SetBannerBackgroundColor(adsBannerId, Color.white);
            return true;
        }

        /// <summary>
        /// 展示Banner
        /// </summary>
        public bool ShowBannerAd()
        {
            if (string.IsNullOrEmpty(adsBannerId))
            {
                LogInfo("ShowBannerAd", "adsBannerId is empty");
                return false;
            }
            if (isBannerShowing)
            {
                LogInfo("ShowBannerAd", "Banner already showing");
                return false;
            }
            LogInfo("ShowBannerAd", adsBannerId);
            MaxSdk.ShowBanner(adsBannerId);
            isBannerShowing = true;
            return true;
        }

        /// <summary>
        /// 隐藏Banner
        /// </summary>
        public bool HideBannerAd()
        {
            if (string.IsNullOrEmpty(adsBannerId))
            {
                LogInfo("ShowBannerAd", "adsBannerId is empty");
                return false;
            }
            if (!isBannerShowing)
            {
                LogInfo("HideBannerAd", "Banner not showing");
                return false;
            }
            LogInfo("HideBannerAd", adsBannerId);
            MaxSdk.HideBanner(adsBannerId);
            isBannerShowing = false;
            return true;
        }
        
        #endregion
    
        private void LogInfo(string tag, string message)
        {
            message = $"{tag}: {message}";
            Debugger.Log(TAG, message);
        }

        private void LogError(string tag, string message)
        {
            message = $"{tag} Error: {message}";
            Debugger.LogError(TAG, message);
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();
            MaxSdkCallbacks.OnSdkInitializedEvent -= OnInitialized;
            DestroyInterstitialAds();
            DestroyRewardedAds();
            DestroyBannerAds();
        }
    }
}