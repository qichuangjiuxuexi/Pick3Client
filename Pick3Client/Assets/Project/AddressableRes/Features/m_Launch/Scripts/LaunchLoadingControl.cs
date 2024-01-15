using System.Collections.Generic;
using UnityEngine;

public class LaunchLoadingControl : LoadingControlBase
{
    public static void Create()
    {
        var go = new GameObject("LaunchLoadingControl");
        DontDestroyOnLoad(go);
        go.AddComponent<LaunchLoadingControl>();
    }
    
    protected override void InitProcesses()
    {
        processes = new List<LoadingProcess>
        {
            //初始化游戏
            new LoadingProcessInitGame(1),
            //切换到Loading场景
            new LoadingProcessSwitchLoadScene(1),
            //初始化依赖存档的模块
            new LoadingProcessInitModulesCtrls(50),
            //加载完成等待
            new LoadingProgressLoadJustWait(1),
            //切换到大厅
            new LoadingProcessSwitchLobbyScene(1),
        };
    }

    protected override void OnProgress(float progress)
    {
        Game.Event.Broadcast(new LoadingProgressEvent(progress));
    }
}
