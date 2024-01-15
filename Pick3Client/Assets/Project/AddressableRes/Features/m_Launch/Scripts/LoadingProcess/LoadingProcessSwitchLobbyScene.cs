using System.Collections;
using AppBase.UI.Scene;
using UnityEngine;

public class LoadingProcessSwitchLobbyScene : LoadingProcess
{
    public LoadingProcessSwitchLobbyScene(float weight) : base(weight)
    {
    }
    
    public override IEnumerator Process()
    {
        UISceneData data;
        yield return null;
        // yield return Game.Scene.SwitchScene(new UISceneData(AAConst.LaunchScene));
    }
}