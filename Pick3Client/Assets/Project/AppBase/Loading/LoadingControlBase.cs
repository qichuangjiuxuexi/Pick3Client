using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using WordGame.Utils;

/// <summary>
/// Loading加载队列基类
/// </summary>
public class LoadingControlBase : MonoBehaviour
{
    protected List<LoadingProcess> processes;
    protected int processIndex;
    protected float currentWeight;
    protected float totalWeight;
    protected float lastProgress;
    
    protected void Start()
    {
        InitProcesses();
        ProcessAllProcesses();
    }

    protected void ProcessAllProcesses()
    {
        if (processes == null || processes.Count == 0) return;
        totalWeight = Math.Max(processes.Sum(p => p?.Weight ?? 0), 1);
        processIndex = 0;
        lastProgress = -1;
        StartCoroutine(ProcessNextProcess());
    }

    protected IEnumerator ProcessNextProcess()
    {
        for (processIndex = 0; processIndex < processes.Count; processIndex++)
        {
            var process = processes[processIndex];
            if (process == null) continue;
            yield return process.Process();
            currentWeight += process.Weight;
        }
        OnProgress(1);
        Destroy(gameObject);
    }

    /// <summary>
    /// 加载的队列
    /// </summary>
    protected virtual void InitProcesses()
    {
    }

    /// <summary>
    /// 当进度发生变化时触发
    /// </summary>
    /// <param name="progress">新进度</param>
    protected virtual void OnProgress(float progress)
    {
    }

    /// <summary>
    /// 当前的进度
    /// </summary>
    protected float Progress
    {
        get
        {
            var process = processes.TryGetValue(processIndex);
            if (process == null) return currentWeight / totalWeight;
            return (currentWeight + process.Weight * process.Progress) / totalWeight;
        }
    }

    protected void Update()
    {
        var progress = Progress;
        if (progress.floatEquals(lastProgress)) return;
        lastProgress = progress;
        OnProgress(progress);
    }
}
