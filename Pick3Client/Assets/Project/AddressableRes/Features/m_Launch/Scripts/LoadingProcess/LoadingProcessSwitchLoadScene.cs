using System.Collections;
using AppBase.UI.Scene;

/// <summary>
/// 加载并显示Loading界面
/// </summary>
public class LoadingProcessSwitchLoadScene : LoadingProcess
{
    public LoadingProcessSwitchLoadScene(float wight) : base(wight)
    {
    }

    public override IEnumerator Process()
    {
        yield return Game.Scene.SwitchScene(new UISceneData(AAConst.LaunchScene));
    }
}