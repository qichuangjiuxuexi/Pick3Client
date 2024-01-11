namespace AppBase.Activity
{
    /// <summary>
    /// 活动时间类型
    /// </summary>
    public enum ActivityTimeZoneType
    {
        /// <summary>
        /// 本地Utc时间
        /// </summary>
        LocalUtc = 0,
        /// <summary>
        /// 手机本地时区
        /// </summary>
        LocalTimeZone = 1,
        /// <summary>
        /// 服务器Utc时间
        /// </summary>
        ServerUtc = 2,
    }
}