namespace AppBase.UI.Scene
{
    /// <summary>
    /// UnityScene场景基类
    /// Single时序：oldScene.OnPlayExitAnim -> oldScene.OnBeforeDestroy -> oldScene.OnDestroy -> Awake -> OnLoad -> OnAwake -> OnPlayEnterAnim
    /// Additive时序：Awake -> OnLoad -> OnAwake -> oldScene.OnPlayExitAnim -> OnPlayEnterAnim -> oldScene.OnBeforeDestroy -> oldScene.OnDestroy 
    /// </summary>
    public class UnityScene : SceneBase
    {
        public UnityEngine.SceneManagement.Scene unityScene;
    }
}
