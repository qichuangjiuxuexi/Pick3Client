using System;
using AppBase.OA;
using UnityEngine;

/// <summary>
/// 物品移动、飞行的修改
/// </summary>
public abstract class BaseObjectAnimationComponent : MonoBehaviour
{
    public string animationName;
    public float timeScale = 1;
    public float delayTime = 0;
    public bool autoPlayOnStart = false;
    public Action onfinished = null;
    /// <summary>
    /// 动画不受时间缩放影响
    /// </summary>
    public bool useUnscaledDeltaTime = false;
    /// <summary>
    /// 延迟过后真正开始执行动作
    /// </summary>
    public Action onRealStart = null;
    public ObjectAnimState state { get; protected set; }
    public float totalElapsedTime = 0;
    protected bool inited = false;

    private void Awake()
    {
        Init();
    }

    public void Start()
    {
        if (autoPlayOnStart)
        {
            Play();
        }
    }

    public abstract void SimuUpdateAll(float deltaTime);
    
    public void Update()
    {
        NormalUpdate(useUnscaledDeltaTime ? Time.unscaledDeltaTime : Time.deltaTime);
    }

    public virtual void NormalUpdate(float deltaTime)
    {
        if (totalElapsedTime < delayTime)
        {
            totalElapsedTime += deltaTime;
            return;
        }

        OnNormalUpdate(deltaTime);
    }

    public abstract void OnNormalUpdate(float deltaTime);
    public abstract void OnLateUpdate(float deltaTime);
    public abstract void OnFixedUpdate(float deltaTime);
    public abstract void UpdateManual(float deltaTime);

    private bool IsInDelay()
    {
        if (totalElapsedTime < delayTime)
        {
            return true;
        }
        return false;
    }

    void LateUpdate()
    {
        OnLateUpdate(useUnscaledDeltaTime ? Time.unscaledDeltaTime : Time.deltaTime);
    }

    public virtual void FixedUpdate()
    {
        OnFixedUpdate(useUnscaledDeltaTime ? Time.fixedUnscaledDeltaTime : Time.fixedDeltaTime);
    }

    public void Init()
    {
        if (!inited)
        {
            OnInit();
            inited = true;
        }
    }
    public abstract void OnInit();
    public abstract void Play();
    public abstract void ResetState();
    public abstract void Restart();
    public abstract void Stop();

}
