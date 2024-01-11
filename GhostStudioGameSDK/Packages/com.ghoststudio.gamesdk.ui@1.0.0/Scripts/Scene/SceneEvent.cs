using AppBase.Event;


namespace AppBase.UI.Scene
{
    /// <summary>
    /// 场景切换后的事件
    /// </summary>
    public struct AfterSwitchSceneEvent : IEvent
    {
        public readonly SceneData SceneData;

        public AfterSwitchSceneEvent(SceneData sceneData)
        {
            SceneData = sceneData;
        }
    }
    /// <summary>
    /// 销毁场景后的事件
    /// </summary>
    public struct DestroySceneEvent : IEvent
    {
        public readonly string Address;

        public DestroySceneEvent(string address)
        {
            Address = address;
        }
    }
}


