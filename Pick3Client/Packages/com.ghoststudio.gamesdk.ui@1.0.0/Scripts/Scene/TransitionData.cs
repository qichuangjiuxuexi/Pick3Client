using System;

namespace AppBase.UI.Scene
{
    /// <summary>
    /// 转场场景数据
    /// </summary>;
    public class TransitionData : SceneData
    {
        /// <summary>
        /// 上一个场景数据，自动获得
        /// </summary>
        public SceneData PreSceneData { get; internal set; }

        /// <summary>
        /// 下一个场景数据
        /// </summary>
        public SceneData NextSceneData { get; set; }
        
        /// <summary>
        /// 打开动画名称
        /// </summary>
        public string openAnimName = "Open";

        /// <summary>
        /// 关闭动画名称
        /// </summary>
        public string closeAnimName = "Close";

        public TransitionData()
        {
        }

        /// <summary>
        /// 转场场景数据
        /// </summary>
        /// <param name="address">转场场景地址</param>
        /// <param name="nextSceneData">下一个场景数据</param>
        /// <param name="sceneType">转场场景是否是UI场景</param>
        public TransitionData(string address, SceneData nextSceneData = null, SceneType sceneType = SceneType.UIScene) : base(address, nextSceneData, sceneType)
        {
            NextSceneData = nextSceneData;
        }
        
        #region 协程相关
        
        /// <summary>
        /// 协程等待下一个场景转场完成
        /// </summary>
        public override bool MoveNext() => NextSceneData?.MoveNext() ?? base.MoveNext();
        
        #endregion
    }
}