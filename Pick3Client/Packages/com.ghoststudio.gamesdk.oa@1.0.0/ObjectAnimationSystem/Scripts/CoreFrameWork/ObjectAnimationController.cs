using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ObjectAnimationController : MonoBehaviour
{
    private Dictionary<string, BaseObjectAnimationComponent> animationMap;
    public string defaultAnimation;
    public bool autoPlayOnStart = false;
    public bool alsoSearchInChildren = false;
    private bool inited = false;
    private void Awake()
    {
        Init();
    }

    private void Start()
    {
        if (autoPlayOnStart && !string.IsNullOrEmpty(defaultAnimation))
        {
            Play(defaultAnimation);
        }
    }

    private void OnValidate()
    {
        inited = false;
        Init();
    }


    void Init()
    {
        if (inited)
        {
            return;
        }
        animationMap = new Dictionary<string, BaseObjectAnimationComponent>();
        BaseObjectAnimationComponent[] list = null;
        if (alsoSearchInChildren)
        {
            list = GetComponentsInChildren<BaseObjectAnimationComponent>(true);
        }
        else
        {
            list = GetComponents<BaseObjectAnimationComponent>();
        }

        if (list != null)
        {
            for (int i = 0; i < list.Length; i++)
            {
                string name = list[i].animationName;
                list[i].autoPlayOnStart = false;
                if (animationMap.ContainsKey(name))
                {
                    Debug.LogError($"重复的动画名字:{name}");
                }
                else
                {
                    animationMap[name] = list[i];
                }
            }
        }
    }

    public BaseObjectAnimationComponent Play(string name,Action onFinished = null,float speedScale = 1,float delay = 0)
    {
        if (!inited)
        {
            Init();
        }
        if (animationMap.ContainsKey(name))
        {
            var anim = animationMap[name];
            anim.onfinished = onFinished;
            anim.timeScale = speedScale;
            anim.delayTime = delay;
            anim.Play();
            return anim;
        }
        Debug.LogWarning($"OA动画未找到:{name}");
        return null;
    }

    public IEnumerator GetAnimEnumrator()
    {
        return animationMap.GetEnumerator();
    }
}
