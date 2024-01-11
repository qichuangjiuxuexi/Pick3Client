namespace AppBase.Ads
{
    public enum OnAdsType
    {
        Interstitial,
        reward,
        Banner,
    }
    
    /// <summary>
    /// 广告状态
    /// </summary>
    public enum ADSStatus
    {
        load,//: 请求广告
        load_success,//: 广告加载完成
        load_fail,//: 广告加载失败

        show,//: 广告开始展示
        show_success,//: 展示完成
        reward,//: 可以给用户发奖励了
        show_fail,//: 展示失败
        click,//: 点击广告内容
        close,//: 关闭

        pay,//: 广告给我们付钱了
    }
}