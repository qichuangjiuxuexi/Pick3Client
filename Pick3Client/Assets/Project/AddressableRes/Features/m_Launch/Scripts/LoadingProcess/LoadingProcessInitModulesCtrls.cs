using System.Collections;
using AppBase.Timing;
using UnityEngine;

/// <summary>
/// 加载存档文件
/// </summary>
public class LoadingProcessInitModulesCtrls : LoadingProcess
{
    public LoadingProcessInitModulesCtrls(float weight) : base(weight)
    {
    }
    
    public override IEnumerator Process()
    {
        Game.Module.IsAfterLogin = true;
        yield return Game.Module.InitAfterLogin();
        Game.Module.AfterInit();
    }
}
