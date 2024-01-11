namespace AppBase.UI.Scene
{
    /// <summary>
    /// 场景类型
    /// </summary>
    public enum SceneType
    {
        /// <summary>
        /// 普通场景，挂载在Scenes下
        /// </summary>
        NormalScene,
        
        /// <summary>
        /// UI场景，挂载在Canvas/Scenes下
        /// </summary>
        UIScene,
        
        /// <summary>
        /// Unity场景，会切换整个场景
        /// </summary>
        SingleUnityScene,
        
        /// <summary>
        /// Unity场景，会叠加场景
        /// </summary>
        AdditiveUnityScene,
    }
}
