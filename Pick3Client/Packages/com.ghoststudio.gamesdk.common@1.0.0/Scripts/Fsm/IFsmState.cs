namespace AppBase.Fsm
{
    /// <summary>
    /// 状态接口
    /// </summary>
    public interface IFsmState
    {
        /// <summary>
        /// 当状态激活时
        /// </summary>
        /// <param name="param"></param>
        public void OnEnter(object param);
        
        /// <summary>
        /// 当状态退出时
        /// </summary>
        public void OnExit();
        
        /// <summary>
        /// 更新状态
        /// </summary>
        public void Update();
    }
}
