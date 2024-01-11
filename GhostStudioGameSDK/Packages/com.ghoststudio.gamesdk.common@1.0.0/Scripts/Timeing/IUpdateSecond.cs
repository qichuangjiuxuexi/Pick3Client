namespace AppBase.Timing
{
    /// <summary>
    /// 每秒更新接口
    /// </summary>
    public interface IUpdateSecond
    {
        /// <summary>
        /// 每秒更新
        /// </summary>
        void OnUpdateSecond();
    }
}