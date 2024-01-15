using System.Collections;
using UnityEngine;

/// <summary>
/// 加载配置文件
/// </summary>
public class LoadingProgressLoadJustWait : LoadingProcess
{
    public LoadingProgressLoadJustWait(float weight) : base(weight)
    {
    }

    public override IEnumerator Process()
    {
        Game.Event.Broadcast(new LoadingProgressEvent(){progress = 0.99f});
        yield return new WaitForSeconds(0.5f);
    }
}
