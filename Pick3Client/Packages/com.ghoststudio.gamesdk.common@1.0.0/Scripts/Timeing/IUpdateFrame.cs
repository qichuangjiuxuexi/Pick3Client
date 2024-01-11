namespace AppBase.Timing
{
    /// <summary>
    /// 每帧更新接口
    /// </summary>
    public interface IUpdateFrame
    {
        /// <summary>
        /// 每帧更新
        /// </summary>
        void Update();
    }
}