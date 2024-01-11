namespace AppBase.Activity
{
    /// <summary>
    /// 活动状态
    /// </summary>
    public enum ActivityStateEnum
    {
        /// <summary>
        /// 活动关闭
        /// </summary>
        Close = 0,
        
        /// <summary>
        /// 活动可开启，等待开启
        /// </summary>
        Ready = 1,
        
        /// <summary>
        /// 活动开启中
        /// </summary>
        Open = 2,
        
        /// <summary>
        /// 活动已完成，等待关闭
        /// </summary>
        Finish = 3,
    }
}