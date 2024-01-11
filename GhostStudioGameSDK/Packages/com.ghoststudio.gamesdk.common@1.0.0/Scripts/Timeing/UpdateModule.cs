using AppBase.Event;
using AppBase.Module;

namespace AppBase.Timing
{
    /// <summary>
    /// 每帧更新的子模块
    /// </summary>
    public class UpdateModule : ModuleBase
    {
        private IUpdateFrame frameModule;
        private IUpdateSecond secondModule;
        
        /// <summary>
        /// 注册每帧更新
        /// </summary>
        public UpdateModule SubscribeFrameUpdate(IUpdateFrame parentModule)
        {
            frameModule = parentModule;
            GameBase.Instance.GetModule<TimingManager>().SubscribeFrameUpdate(frameModule);
            return this;
        }
        
        /// <summary>
        /// 注册每秒更新
        /// </summary>
        public UpdateModule SubscribeSecondUpdate(IUpdateSecond parentModule)
        {
            secondModule = parentModule;
            GameBase.Instance.GetModule<TimingManager>().SubscribeSecondUpdate(secondModule);
            return this;
        }
        
        protected override void OnDestroy()
        {
            if (frameModule != null)
            {
                GameBase.Instance.GetModule<TimingManager>().UnsubscribeFrameUpdate(frameModule);
                frameModule = null;
            }
            if (secondModule != null)
            {
                GameBase.Instance.GetModule<TimingManager>().UnsubscribeSecondUpdate(secondModule);
                secondModule = null;
            }
        }
    }
}