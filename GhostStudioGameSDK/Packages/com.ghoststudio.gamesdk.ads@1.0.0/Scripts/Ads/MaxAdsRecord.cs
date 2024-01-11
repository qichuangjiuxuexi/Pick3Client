using System;
using System.Collections.Generic;
using AppBase.Archive;

namespace AppBase.Ads
{
    public class MaxAdsRecord:BaseRecord<MaxAdsArchiveData>
    {
        public void RewardRewardedAd()
        {
            if (ArchiveData.rewardCount == 0)
                ArchiveData.firstRewardTime = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
            ArchiveData.rewardCount++;
            SetDirty();
        }

        public void ShowInterstitial()
        {
            if (ArchiveData.interstitialCount == 0)
                ArchiveData.firstInterstitialTime = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
            ArchiveData.interstitialCount++;
            SetDirty();
        }
    }
    
    /// <summary>
    /// 用户充值信息
    /// </summary>
    [Serializable]
    public class MaxAdsArchiveData : BaseArchiveData
    {
        /// <summary>
        /// 首次看插屏时间
        /// </summary>
        public long firstInterstitialTime = 0 ;
        
        /// <summary>
        /// 首次看RV时间
        /// </summary>
        public long firstRewardTime = 0;
        
        /// <summary>
        /// 看插屏次数
        /// </summary>
        public int interstitialCount = 0;
        
        /// <summary>
        /// 看RV成功次数
        /// </summary>
        public int rewardCount = 0;
    }
}