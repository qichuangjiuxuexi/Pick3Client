namespace AppBase.UserAssets
{
    /// <summary>
    /// 资产订单消耗类型
    /// </summary>
    public enum AutoConsumeType
    {
        /// <summary>
        /// 重启后自动加入资产中
        /// </summary>
        AutoConsume = 0,
        
        /// <summary>
        /// 重启后丢弃
        /// </summary>
        AutoDiscard = 1,
        
        /// <summary>
        /// 重启后保留，由业务系统重新取出发放
        /// </summary>
        Keep = 2,
    }
}