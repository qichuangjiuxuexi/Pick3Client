using System;

namespace AppBase.UI.Scene
{
    /// <summary>
    /// UI场景数据
    /// </summary>
    public class UISceneData : SceneData
    {
        public UISceneData(string address, object data = null, Action<SceneBase> switchCallback = null) : base(address, data, SceneType.UIScene, switchCallback)
        {
        }
    }
}