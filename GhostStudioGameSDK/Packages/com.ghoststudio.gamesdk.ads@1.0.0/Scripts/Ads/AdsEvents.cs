using AppBase.Event;

namespace AppBase.Ads
{
    
    /// <summary>
    /// Banner广告Ready状态改变
    /// </summary>
    public class OnAdsBannerReadyChanged : IEvent
    {
        public bool isBannerReady;
        public OnAdsBannerReadyChanged(bool inIsBannerReady) => isBannerReady = inIsBannerReady;
    }

    /// <summary>
    /// RewardVideo广告Ready状态改变
    /// </summary>
    public class OnAdsRewardVideoReadyChanged : IEvent
    {
        public bool isRewardVideoReady;
        public OnAdsRewardVideoReadyChanged(bool inIsRewardVideoReady) => isRewardVideoReady = inIsRewardVideoReady;
    }

    /// <summary>
    /// 观看RV广告成功
    /// </summary>
    public class OnAdsRewardVideoWatchSucceed : IEvent
    {
    }
    
    /// <summary>
    /// 获得广告收入，打点记录
    /// </summary>
    public class OnAdsRevenuePaid : IEvent
    {
        public AdsInfo adsInfo;
        //插屏 rv Banner
        public OnAdsType postion;

        public OnAdsRevenuePaid(AdsInfo info, OnAdsType pos)
        {
            adsInfo = info;
            postion = pos;
        }
    }
}