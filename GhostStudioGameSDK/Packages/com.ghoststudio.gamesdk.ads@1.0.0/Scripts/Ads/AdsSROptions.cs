using System;
using AppBase.Analytics;
using AppBase.Debugging;
using AppBase.Utils;

namespace AppBase.Ads
{
    public class AdsSROptions : SROptionsModule
    {
        protected override string CatalogName => "广告相关";
        protected override SROptionsPriority CatalogPriority => SROptionsPriority.Ads;

        [DisplayName("启动广告检查器")]
        public void OpenAdInspector()
        {
            MaxSdk.ShowMediationDebugger();
        }
        [DisplayName("谷歌GAID/苹果IDFA")]
        public string GetGoogleGaidOrIosIdfa
        {
            get  {
                try
                {
                    if (AppUtil.IsAndroid)
                    {
                        return GameBase.Instance.GetModule<AnalyticsManager>().GetPropertyValue
                            (AnalyticsUserPropertiesConfigKeys.gaid)?.ToString();
                    }

                    if (AppUtil.IsIOS)
                    {
                        return GameBase.Instance.GetModule<AnalyticsManager>().GetPropertyValue
                            (AnalyticsUserPropertiesConfigKeys.idfa)?.ToString();
                    }
                }
                catch (Exception e)
                {
                    return "初始化中";
                }
               
                return "不是Android和IOS设备没有";
            }
            set  {}
        }
    }
}