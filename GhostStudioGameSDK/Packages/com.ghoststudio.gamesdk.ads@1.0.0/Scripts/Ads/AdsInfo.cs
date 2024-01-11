using System;

namespace AppBase.Ads
{
    /// <summary>
    /// 与广告平台无关的广告信息，避免直接引用MaxSdk.AdInfo
    /// </summary>
    [Serializable]
    public class AdsInfo
    {
        public string AdUnitIdentifier;
        public string AdFormat;
        public string NetworkName;
        public string NetworkPlacement;
        public string Placement;
        public string CreativeIdentifier;
        public double Revenue;
        public string RevenuePrecision;
        public string DspName;
    }
    
    internal static class AdsInfoExtention
    {
        internal static AdsInfo CreateAdsInfo(this MaxSdk.AdInfo maxAdInfo)
        {
            return new AdsInfo()
            {
                AdUnitIdentifier = maxAdInfo.AdUnitIdentifier,
                AdFormat = maxAdInfo.AdFormat,
                NetworkName = maxAdInfo.NetworkName,
                NetworkPlacement = maxAdInfo.NetworkPlacement,
                Placement = maxAdInfo.Placement,
                CreativeIdentifier = maxAdInfo.CreativeIdentifier,
                Revenue = maxAdInfo.Revenue,
                RevenuePrecision = maxAdInfo.RevenuePrecision,
                DspName = maxAdInfo.DspName,
            };
        }
    }
}
