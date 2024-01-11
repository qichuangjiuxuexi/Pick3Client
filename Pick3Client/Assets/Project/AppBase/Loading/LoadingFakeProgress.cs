using System;
using UnityEngine;

/// <summary>
/// 假进度
/// </summary>
public class LoadingFakeProgress : MonoBehaviour
{
    /// <summary>
    /// 假进度
    /// </summary>
    public float fakeProgress { get; protected set; }

    /// <summary>
    /// 真进度
    /// </summary>
    public float realProgress;
    
    /// <summary>
    /// 假进度时间
    /// </summary>
    public float fakeTotalTime = 10;
    
    /// <summary>
    /// 假进度百分比
    /// </summary>
    public float fakeTotalPercent = 0.8f;

    protected float startLoadTime;
    protected float lastFakeProgress;

    protected void Start()
    {
        startLoadTime = Time.realtimeSinceStartup;
        lastFakeProgress = -1;
    }

    protected void Update()
    {
        var loadTime = Math.Min(Time.realtimeSinceStartup - startLoadTime, fakeTotalTime);
        var fakeProgress = loadTime / fakeTotalTime * fakeTotalPercent;
        var progress = realProgress;
        //超过10s，显示真实进度叠加假进度
        if (loadTime >= fakeTotalTime)
        {
            if (lastFakeProgress < 0)
            {
                lastFakeProgress = progress;
            }
            //真实进度比假进度慢，后70%为真实进度
            if (lastFakeProgress < fakeProgress)
            {
                progress = fakeProgress + (progress - lastFakeProgress) / (1 - lastFakeProgress) * (1 - fakeProgress);
            }
        }
        //10s内，谁跑得快显示谁
        else
        {
            progress = Math.Max(progress, fakeProgress);
        }
        fakeProgress = progress;
    }
}
