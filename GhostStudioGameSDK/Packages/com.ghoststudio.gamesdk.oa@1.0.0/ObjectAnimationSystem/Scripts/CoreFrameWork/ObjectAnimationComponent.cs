using System;
using System.Collections.Generic;
using AppBase.OA;
using UnityEngine;

/// <summary>
/// 物品移动、飞行的修改
/// </summary>
public class ObjectAnimationComponent : BaseObjectAnimationComponent
{
    public List<BaseObjectAnimConfig> modulesConfigs;
    private Dictionary<ObjectAnimUpdateType,List<BaseAnimBehaviour>> behavioursDict;
    private Dictionary<ObjectAnimUpdateType,float> elapsedTimeDict;
    public Action<bool> onFaceForwardChanged = null;
    public Action<float> onBaseDurationChanged;
    /// <summary>
    /// 当最后一个动画成分刷新之后的事件，比如某个动画按顺序有移动、旋转、缩放三个成分，那么该事件会在缩放Update完之后触发，参数是当前动画已经过去的时间
    /// </summary>
    public Action<float> onLastOneUpdated;
    public override void SimuUpdateAll(float time)
    {
        if (state == ObjectAnimState.Finished || state == ObjectAnimState.Stopped)
        {
            return;
        }
        if (totalElapsedTime < delayTime)
        {
            totalElapsedTime += time;
            return;
        }

        CheckInit();
        for (ObjectAnimUpdateType t = ObjectAnimUpdateType.NormalUpdate; t <= ObjectAnimUpdateType.ManualUpdate; t++)
        {
            if (behavioursDict.ContainsKey(t))
            {
                var list = behavioursDict[t];
                int count = list.Count;
                for (int i = 0; i < count; i++)
                {
                    if (list[i].config.enabled && list[i].state == ObjectAnimState.Running)
                    {
                        list[i].Update(time,elapsedTimeDict[t]);
                        if (i == count - 1)
                        {
                            onLastOneUpdated?.Invoke(totalElapsedTime);
                        }
                    }
                }
                elapsedTimeDict[t] += time * timeScale;
            }
        }
        
        bool unfinishedFound = false;
        foreach (var item in behavioursDict)
        {
            for (int i = 0; i < item.Value.Count; i++)
            {
                if (item.Value[i].config.enabled && item.Value[i].state != ObjectAnimState.Finished)
                {
                    unfinishedFound = true;
                }
            }
        }

        if (!unfinishedFound)
        {
            state = ObjectAnimState.Finished;
            var tmpAction = onfinished;
            onfinished = null;
            tmpAction?.Invoke();
        }
        totalElapsedTime += time;
    }
    
    public override void OnNormalUpdate(float deltaTime)
    {
        if (state != ObjectAnimState.Running)
        {
            return;
        }
        if (totalElapsedTime < delayTime)
        {
            totalElapsedTime += deltaTime;
            return;
        }

        if (onRealStart != null)
        {
            onRealStart.Invoke();
            onRealStart = null;
        }
        CheckInit();
        if (behavioursDict.ContainsKey(ObjectAnimUpdateType.NormalUpdate))
        {
            var list = behavioursDict[ObjectAnimUpdateType.NormalUpdate];
            int count = list.Count;
            for (int i = 0; i < count; i++)
            {
                if (list[i].config.enabled && list[i].state == ObjectAnimState.Running)
                {
                    list[i].Update(deltaTime,elapsedTimeDict[ObjectAnimUpdateType.NormalUpdate]);
                    if (i == count - 1)
                    {
                        onLastOneUpdated?.Invoke(totalElapsedTime);
                    }
                }
            }
        }

        if (elapsedTimeDict.ContainsKey(ObjectAnimUpdateType.NormalUpdate))
        {
            elapsedTimeDict[ObjectAnimUpdateType.NormalUpdate] += deltaTime * timeScale;
        }
        
        bool unfinishedFound = false;
        foreach (var item in behavioursDict)
        {
            for (int i = 0; i < item.Value.Count; i++)
            {
                if (item.Value[i].config.enabled && item.Value[i].state != ObjectAnimState.Finished)
                {
                    unfinishedFound = true;
                }
            }
        }
        totalElapsedTime += deltaTime;
        if (!unfinishedFound)
        {
            state = ObjectAnimState.Finished;
            var tmpAction = onfinished;
            onfinished = null;
            tmpAction?.Invoke();
        }
    }

    void CheckInit()
    {
        if (behavioursDict == null || behavioursDict.Count == 0 || elapsedTimeDict == null || elapsedTimeDict.Count == 0)
        {
            OnInit();
        }
    }

    private bool IsInDelay()
    {
        if (totalElapsedTime < delayTime)
        {
            return true;
        }

        return false;
    }

    public override void OnLateUpdate(float deltaTime)
    {
        if (state != ObjectAnimState.Running || IsInDelay())
        {
            return;
        }
        if (behavioursDict.ContainsKey(ObjectAnimUpdateType.LateUpdate))
        {
            var list = behavioursDict[ObjectAnimUpdateType.LateUpdate];
            int count = list.Count;
            for (int i = 0; i < list.Count; i++)
            {
                if (list[i].config.enabled && list[i].state == ObjectAnimState.Running)
                {
                    list[i].Update(deltaTime,elapsedTimeDict[ObjectAnimUpdateType.LateUpdate]);
                    if (i == count - 1)
                    {
                        onLastOneUpdated?.Invoke(totalElapsedTime);
                    }
                }
            }
        }

        if (elapsedTimeDict.ContainsKey(ObjectAnimUpdateType.LateUpdate))
        {
            elapsedTimeDict[ObjectAnimUpdateType.LateUpdate] += deltaTime * timeScale;
        }
    }

    public override void OnFixedUpdate(float deltaTime)
    {
        if (state != ObjectAnimState.Running || IsInDelay())
        {
            return;
        }
        if (behavioursDict.ContainsKey(ObjectAnimUpdateType.FixedUpdate))
        {
            var list = behavioursDict[ObjectAnimUpdateType.FixedUpdate];
            int count = list.Count;
            for (int i = 0; i < list.Count; i++)
            {
                if (list[i].config.enabled && list[i].state == ObjectAnimState.Running)
                {
                    list[i].Update(deltaTime,elapsedTimeDict[ObjectAnimUpdateType.FixedUpdate]);
                    if (i == count - 1)
                    {
                        onLastOneUpdated?.Invoke(totalElapsedTime);
                    }
                }
            }
        }

        if (elapsedTimeDict.ContainsKey(ObjectAnimUpdateType.FixedUpdate))
        {
            elapsedTimeDict[ObjectAnimUpdateType.FixedUpdate] += deltaTime * timeScale;
        }
    }

    public override void UpdateManual(float deltaTime)
    {
        if (state != ObjectAnimState.Running || IsInDelay())
        {
            return;
        }
        elapsedTimeDict[ObjectAnimUpdateType.FixedUpdate] += deltaTime;
        if (behavioursDict.ContainsKey(ObjectAnimUpdateType.ManualUpdate))
        {
            var list = behavioursDict[ObjectAnimUpdateType.ManualUpdate];
            int count = list.Count;
            for (int i = 0; i < list.Count; i++)
            {
                if (list[i].config.enabled && list[i].state == ObjectAnimState.Running)
                {
                    list[i].Update(deltaTime,elapsedTimeDict[ObjectAnimUpdateType.ManualUpdate]);
                    if (i == count - 1)
                    {
                        onLastOneUpdated?.Invoke(totalElapsedTime);
                    }
                }
            }
        }

        if (elapsedTimeDict.ContainsKey(ObjectAnimUpdateType.ManualUpdate))
        {
            elapsedTimeDict[ObjectAnimUpdateType.ManualUpdate] += deltaTime * timeScale;
        }
    }
    public override void OnInit()
    {
        modulesConfigs ??= new List<BaseObjectAnimConfig>();
        behavioursDict = new Dictionary<ObjectAnimUpdateType, List<BaseAnimBehaviour>>();
        elapsedTimeDict = new Dictionary<ObjectAnimUpdateType, float>();
        for (int i = 0; i < modulesConfigs.Count; i++)
        {
            if (modulesConfigs[i] == null)
            {
                modulesConfigs.RemoveAt(i);
                i--;
                continue;
            }
            var type = modulesConfigs[i].updateType;
            if (!behavioursDict.ContainsKey(type))
            {
                behavioursDict[type] = new List<BaseAnimBehaviour>();
            }
            var behaviour = Activator.CreateInstance(modulesConfigs[i].behaviourType) as BaseAnimBehaviour;
            behaviour.Init(modulesConfigs[i],gameObject);
            behaviour.onFaceForwardChanged = onFaceForwardChanged;
            behaviour.onBaseDurationChanged = OnBaseDurationChanged;
            if (modulesConfigs[i].baseDuration)
            {
                behaviour.UpdateDuration();
            }
            behavioursDict[type].Add(behaviour);
        }

        elapsedTimeDict[ObjectAnimUpdateType.NormalUpdate] = 0;
        elapsedTimeDict[ObjectAnimUpdateType.LateUpdate] = 0;
        elapsedTimeDict[ObjectAnimUpdateType.FixedUpdate] = 0;
        elapsedTimeDict[ObjectAnimUpdateType.ManualUpdate] = 0;
        inited = true;
    }

    public void ForceInit()
    {
        OnInit();
    }

    void OnBaseDurationChanged(float newDuration)
    {
        onBaseDurationChanged?.Invoke(newDuration);
        for (ObjectAnimUpdateType i = ObjectAnimUpdateType.NormalUpdate; i <= ObjectAnimUpdateType.ManualUpdate; i++)
        {
            if (!behavioursDict.ContainsKey(i))
            {
                continue;
            }
            var list = behavioursDict[i];
            for (int j = 0; j < list.Count; j++)
            {
                list[j].OnBaseDurationChanged(newDuration);
            }
        }
    }

    [ContextMenu("play")]
    public override void Play()
    {
        ResetState();
        for (ObjectAnimUpdateType i = ObjectAnimUpdateType.NormalUpdate; i <= ObjectAnimUpdateType.ManualUpdate; i++)
        {
            if (!behavioursDict.ContainsKey(i))
            {
                continue;
            }
            var list = behavioursDict[i];
            for (int j = 0; j < list.Count; j++)
            {
                list[j].Start();
            }
        }

        state = ObjectAnimState.Running;
    }

    public override void ResetState()
    {
        state = ObjectAnimState.None;
        CheckInit();
        for (ObjectAnimUpdateType i = ObjectAnimUpdateType.NormalUpdate; i <= ObjectAnimUpdateType.ManualUpdate; i++)
        {
            elapsedTimeDict[i] = 0;
        }

        totalElapsedTime = 0;
    }

    [ContextMenu("replay")]
    public override void Restart()
    {
        ResetState();
        for (ObjectAnimUpdateType i = ObjectAnimUpdateType.NormalUpdate; i <= ObjectAnimUpdateType.ManualUpdate; i++)
        {
            if (!behavioursDict.ContainsKey(i))
            {
                continue;
            }
            var list = behavioursDict[i];
            for (int j = 0; j < list.Count; j++)
            {
                list[j].Start();
            }
        }

        state = ObjectAnimState.Running;
    }

    [ContextMenu("stop")]
    public override void Stop()
    {
        for (ObjectAnimUpdateType i = ObjectAnimUpdateType.NormalUpdate; i <= ObjectAnimUpdateType.ManualUpdate; i++)
        {
            if (!behavioursDict.ContainsKey(i))
            {
                continue;
            }
            var list = behavioursDict[i];
            for (int j = 0; j < list.Count; j++)
            {
                list[j].Stop();
            }
        }
    }

    // public void CorrectStartValue(object startVal,Type targetConfigType)
    // {
    //     CheckInit();
    //     foreach (var bh in behavioursDict)
    //     {
    //         var list = bh.Value;
    //         for (int i = 0; i < list.Count; i++)
    //         {
    //             if (list[i].GetType() == targetConfigType)
    //             {
    //                 list[i].CorrectStart(startVal);
    //             }
    //         }
    //     }
    // }
    //
    // public void CorrectEndValue<T>(T startVal,Type targetConfigType)
    // {
    //     CheckInit();
    //     foreach (var bh in behavioursDict)
    //     {
    //         var list = bh.Value;
    //         for (int i = 0; i < list.Count; i++)
    //         {
    //             if (list[i].GetType() == targetConfigType)
    //             {
    //                 list[i].CorrectEnd(startVal);
    //             }
    //         }
    //     }
    // }

    public void SetOAClips(ObjectAnimationClip clip,bool isClone = false)
    {
        if (clip == null || clip.animationConfigs == null)
        {
            return;
        }

        animationName = clip.clipName;
        if (!isClone)
        {
            modulesConfigs = clip.animationConfigs;
        }
        else
        {
            modulesConfigs = new List<BaseObjectAnimConfig>();
            for (int i = 0; i < clip.animationConfigs.Count; i++)
            {
                modulesConfigs.Add(Instantiate(clip.animationConfigs[i]));
            }
        }
        behavioursDict = null;
        ForceInit();
    }

    public T GetBehaviour<T>() where T: BaseAnimBehaviour
    {
        CheckInit();
        foreach (var item in behavioursDict)
        {
            foreach (var bh in item.Value)
            {
                if (bh is T)
                {
                    return bh as T;
                }
            }
        }

        return null;
    }
    
    public BaseAnimBehaviour GetBehaviour(string type)
    {
        CheckInit();
        Type tp = Type.GetType(type);
        foreach (var item in behavioursDict)
        {
            foreach (var bh in item.Value)
            {
                if (bh.GetType() == tp)
                {
                    return bh;
                }
            }
        }

        return null;
    }
    
    public BaseAnimBehaviour GetBehaviour(Type type)
    {
        CheckInit();
        foreach (var item in behavioursDict)
        {
            foreach (var bh in item.Value)
            {
                if (bh.GetType() == type)
                {
                    return bh;
                }
            }
        }

        return null;
    }
}
