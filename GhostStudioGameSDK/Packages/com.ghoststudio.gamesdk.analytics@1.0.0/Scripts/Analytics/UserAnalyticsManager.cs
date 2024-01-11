namespace AppBase.Analytics
{
    /// <summary>
    /// 带用户属性的打点管理器
    /// </summary>
    public class UserAnalyticsManager : ThirdPartAnalyticsManager
    {
        /// <summary>
        /// 设置用户属性
        /// </summary>
        public virtual void SetUserProperties(AnalyticsUserProperties userProperties)
        {
        }
        
        /// <summary>
        /// 设置用户属性（仅一次）
        /// </summary>
        public virtual void SetUserPropertiesOnce(AnalyticsUserProperties userProperties)
        {
        }
    }
}