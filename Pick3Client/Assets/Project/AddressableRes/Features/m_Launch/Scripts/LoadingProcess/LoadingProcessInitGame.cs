using System.Collections;
using System.Globalization;
using DG.Tweening;
using UnityEngine;

/// <summary>
/// 初始化游戏，设置基本参数，初始化Game
/// </summary>
public class LoadingProcessInitGame : LoadingProcess
{
    public LoadingProcessInitGame(float weight) : base(weight)
    {
    }

    public override IEnumerator Process()
    {
        //Log类型不需要堆栈跟踪
        Application.SetStackTraceLogType(LogType.Log, StackTraceLogType.None);
        Debugger.LogD("LoadingProcessInitGame");
        //DoTween只输出错误类型日志
        DOTween.logBehaviour = LogBehaviour.ErrorsOnly;
        CultureInfo.DefaultThreadCurrentCulture = CultureInfo.InvariantCulture;
        CultureInfo.DefaultThreadCurrentUICulture = CultureInfo.InvariantCulture;
        //禁用多点触控
        Input.multiTouchEnabled = false;
        //屏幕永不休眠
        Screen.sleepTimeout = SleepTimeout.NeverSleep;
        //帧率
        Application.targetFrameRate = 60;
        Game.Start();
        yield break;
    }
}
