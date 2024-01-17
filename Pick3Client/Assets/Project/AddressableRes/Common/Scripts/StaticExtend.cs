using System;
using System.Collections.Generic;
using System.Linq;
using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public static class StaticExtend
{
    /// <summary>
    /// 获取Transform下指定路径下的组件
    /// </summary>
    /// <typeparam name="T">Component</typeparam>
    /// <param name="t"></param>
    /// <param name="path">指定路径</param>
    /// <param name="forceCreate">是否强制创建</param>
    /// <param name="withoutErr">如果找不到，不提示错误</param>
    /// <returns></returns>
    public static T Bind<T>(this Transform t, string path, bool forceCreate = false, bool withoutErr = false) where T : Component
    {
        if (t == null)
        {
            if (!withoutErr)
            {
                Debug.LogError("t is Null");
            }
            return null;
        }

        var temp = t.Find(path);
        if (temp == null)
        {
            if (!withoutErr)
            {
                Debug.LogError(string.Format("Find {0}/{1} Failed", t.name, path));
            }
            return null;
        }

        T component = temp.GetComponent<T>();
        if (component == null)
        {
            if (forceCreate)
            {
                component = temp.gameObject.AddComponent<T>();
            }
            else if (!withoutErr)
            {
                Debug.LogError(string.Format("Transform {0}/{1} can't find Button", t.name, path));
            }
        }

        return component;
    }

    /// <summary>
    /// 获取Transform下指定路径下的组件
    /// </summary>
    /// <param name="t">Transform</param>
    /// <param name="forceCreate">如果找不到组件，是否创建一个</param>
    /// <param name="withoutErr">如果找不到组件，是否报错</param>
    /// <typeparam name="T">组件类型</typeparam>
    /// <returns></returns>
    public static T Bind<T>(this Transform t, bool forceCreate = false, bool withoutErr = false) where T : Component
    {
        if (t == null)
        {
            if (!withoutErr)
            {
                Debug.LogError("t is Null");
            }
            return null;
        }
        
        T component = t.GetComponent<T>();
        if (component == null)
        {
            if (forceCreate)
            {
                component = t.gameObject.AddComponent<T>();
            }
            else if (!withoutErr)
            {
                Debug.LogError(string.Format("Transform {0} can't find Button", t.name));
            }
        }
        return component;
    }
    
    /// <summary>
    /// 绑定按钮和处理函数
    /// </summary>
    /// <param name="t">Transform</param>
    /// <param name="path">组件路径</param>
    /// <param name="clickAction">绑定函数</param>
    /// <param name="forceCreate">如果找不到Button组件，是否创建一个</param>
    /// <param name="withoutErr">如果找不到组件，是否报错</param>
    /// <returns></returns>
    public static Button BindButton(this Transform t, string path, UnityAction clickAction, bool forceCreate = false, bool withoutErr = false)
    {
        var button = Bind<Button>(t, path, forceCreate, withoutErr);
        if (button != null)
        {
            button.onClick.RemoveAllListeners();
            button.onClick.AddListener(clickAction);
        }
        return button;
    }
    
    /// <summary>
    /// 绑定按钮和处理函数（处理函数增加参数绑定）
    /// </summary>
    /// <param name="t">Transform</param>
    /// <param name="path">组件路径</param>
    /// <param name="clickAction">绑定函数（带一个参数）</param>
    /// <param name="arg">绑定参数</param>
    /// <param name="forceCreate">如果找不到Button组件，是否创建一个</param>
    /// <param name="withoutErr">如果找不到组件，是否报错</param>
    /// <typeparam name="T">参数类型</typeparam>
    /// <returns></returns>
    public static Button BindButton<T>(this Transform t, string path, Action<T> clickAction, T arg, bool forceCreate = false, bool withoutErr = false)
    {
        var button = Bind<Button>(t, path, forceCreate, withoutErr);
        if (button != null)
        {
            button.onClick.RemoveAllListeners();
            button.onClick.AddListener(() => clickAction?.Invoke(arg));
        }
        return button;
    }

    /// <summary>
    /// 播放动画，从0开始播，播完之后可以回调，并且可以自动保存状态
    /// </summary>
    /// <param name="component"></param>
    /// <param name="animName"></param>
    /// <param name="callback">播完之后回调</param>
    /// <param name="isOnce">如果正在播放该动画，则不重新播放</param>
    /// <param name="offset">offset in second</param>
    public static void PlayAnimator(this Component component, string animName, Action callback = null, bool isOnce = false, float offset = 0)
    {
        if (component == null || string.IsNullOrEmpty(animName)) return;
        var animator = component.GetComponent<Animator>();
        PlayAnimator(animator, animName, callback, isOnce, offset);
    }

    /// <summary>
    /// 播放动画，从0开始播，播完之后可以回调，并且可以自动保存状态
    /// </summary>
    /// <param name="animator">animator</param>
    /// <param name="animName"></param>
    /// <param name="callback">播完之后回调</param>
    /// <param name="isOnce">如果正在播放该动画，则不重新播放</param>
    /// <param name="offset">offset in second</param>
    public static void PlayAnimator(this Animator animator, string animName, Action callback = null, bool isOnce = false, float offset = 0)
    {
        if (animator == null) return;
        if (isOnce && animator.GetCurrentAnimatorStateInfo(0).IsName(animName)) return;
        animator.keepAnimatorStateOnDisable = true;
        animator.PlayInFixedTime(animName, 0, offset);
        if (callback != null)
        {
            animator.Update(0);
            var duration = animator.GetCurrentAnimatorStateInfo(0).length;
            animator.transform.DOKill();
            animator.transform.DOLocalMoveZ(0, duration).OnComplete(() => callback());
        }
    }

    /// <summary>
    /// 设置transform.x
    /// </summary>
    public static void SetLocalPositionX(this Transform transform, float x)
    {
        if (transform == null) return;
        var pos = transform.localPosition;
        pos.x = x;
        transform.localPosition = pos;
    }

    /// <summary>
    /// 设置transform.y
    /// </summary>
    public static void SetLocalPositionY(this Transform transform, float y)
    {
        if (transform == null) return;
        var pos = transform.localPosition;
        pos.y = y;
        transform.localPosition = pos;
    }

    /// <summary>
    /// 设置transform.y
    /// </summary>
    public static void SetLocalPositionYScale(this Transform transform, float scale)
    {
        if (transform == null) return;
        var pos = transform.localPosition;
        pos.y *= scale;
        transform.localPosition = pos;
    }

    /// <summary>
    /// 设置transform.z
    /// </summary>
    public static void SetLocalPositionZ(this Transform transform, float z)
    {
        if (transform == null) return;
        var pos = transform.localPosition;
        pos.z = z;
        transform.localPosition = pos;
    }

    /// <summary>
    /// 设置transform.scale
    /// </summary>
    public static void SetLocalScale(this Transform transform, float x)
    {
        if (transform == null) return;
        transform.localScale = new Vector3(x, x, 1);
    }

    /// <summary>
    /// 设置transform.scaleX
    /// </summary>
    public static void SetLocalScaleX(this Transform transform, float x)
    {
        if (transform == null) return;
        var pos = transform.localScale;
        pos.x = x;
        transform.localScale = pos;
    }

    /// <summary>
    /// 设置transform.scaleY
    /// </summary>
    public static void SetLocalScaleY(this Transform transform, float y)
    {
        if (transform == null) return;
        var pos = transform.localScale;
        pos.y = y;
        transform.localScale = pos;
    }

    /// <summary>
    /// 安全设置text
    /// </summary>
    public static void SetTextSafe(this TMP_Text text, string str)
    {
        if (text != null)
        {
            text.text = str ?? "";
        }
    }

    /// <summary>
    /// 安全设置color.a
    /// </summary>
    public static void SetColorAlpha(this Graphic img, float a)
    {
        if (img == null) return;
        var color = img.color;
        color.a = a;
        img.color = color;
    }

    /// <summary>
    /// 安全设置旋转角度localEulerAngles.z
    /// </summary>
    public static void SetLocalRotation(this Transform transform, float z)
    {
        if (transform == null) return;
        var euler = transform.localEulerAngles;
        euler.z = z;
        transform.localEulerAngles = euler;
    }

    /// <summary>
    /// 安全设置sizeDelta.x
    /// </summary>
    public static void SetSizeDeltaX(this Component component, float w)
    {
        var rect = component?.GetComponent<RectTransform>();
        if (rect == null) return;
        var size = rect.sizeDelta;
        size.x = w;
        rect.sizeDelta = size;
    }

    /// <summary>
    /// 安全设置sizeDelta.y
    /// </summary>
    public static void SetSizeDeltaY(this Component component, float h)
    {
        var rect = component?.GetComponent<RectTransform>();
        if (rect == null) return;
        var size = rect.sizeDelta;
        size.y = h;
        rect.sizeDelta = size;
    }

    /// <summary>
    /// 安全设置sizeDelta
    /// </summary>
    public static void SetSizeDelta(this Component component, Vector2 size)
    {
        var rect = component?.GetComponent<RectTransform>();
        if (rect == null) return;
        rect.sizeDelta = size;
    }

    /// <summary>
    /// 安全设置onClick.AddListener
    /// </summary>
    public static void AddListener(this Button button, UnityAction action)
    {
        if (button != null)
        {
            button.onClick.RemoveAllListeners();
            if (action != null)
            {
                button.onClick.AddListener(action);
            }
        }
    }

    /// <summary>
    /// 安全设置button.interactable
    /// </summary>
    public static void SetInteractable(this Button button, bool interactable)
    {
        if (button != null)
        {
            button.interactable = interactable;
        }
    }

    /// <summary>
    /// 安全设置image.raycastTarget
    /// </summary>
    public static void SetRaycastTarget(this Component component, bool isEnabled)
    {
        if (component == null) return;
        var image = component.GetComponent<Image>();
        if (image == null) return;
        image.raycastTarget = isEnabled;
    }

    /// <summary>
    /// 安全设置gameObject.setActive
    /// </summary>
    public static void SetActive(this Component component, bool isActive)
    {
        if (component != null && component.gameObject.activeSelf != isActive)
        {
            component.gameObject.SetActive(isActive);
        }
    }

    /// <summary>
    /// 安全设置 for gameObject.setActive
    /// </summary>
    public static void SetActive<T>(this IEnumerable<T> components, bool isActive) where T : Component
    {
        if (components != null)
        {
            foreach (var component in components)
            {
                SetActive(component, isActive);
            }
        }
    }

    /// <summary>
    /// 对每个Children执行操作，单层的
    /// </summary>
    public static void EachChild(this Transform transform, Action<Transform> callback)
    {
        if (transform == null) return;
        for (int i = 0; i < transform.childCount; i++)
        {
            callback?.Invoke(transform.GetChild(i));
        }
    }
    /// <summary>
    /// 对每个Children执行操作，单层的（带参数i）
    /// </summary>
    public static void EachChild(this Transform transform, Action<Transform, int> callback)
    {
        if (transform == null) return;
        for (int i = 0; i < transform.childCount; i++)
        {
            callback?.Invoke(transform.GetChild(i), i);
        }
    }

    /// <summary>
    /// 移除所有子节点
    /// </summary>
    public static void RemoveAllChildren(this Transform transform)
    {
        if (transform == null) return;
        for (int i = transform.childCount - 1; i >= 0; i--)
        {
            GameObject.Destroy(transform.GetChild(i).gameObject);
        }
    }
    
    /// <summary>
    /// 移除组件，如果是Transform组件则移除对象
    /// </summary>
    public static void DestroyObject(this Component obj)
    {
        if (obj == null) return;
        if (obj is Transform)
        {
            GameObject.Destroy(obj.gameObject);
        }
        else
        {
            GameObject.Destroy(obj);
        }
    }
    /// <summary>
    /// 移除对象
    /// </summary>
    public static void DestroyObject(this GameObject obj)
    {
        if (obj == null) return;
        GameObject.Destroy(obj);
    }

    /// <summary>
    /// 取所有子结点，单层的
    /// <param name="transform">结点</param>
    /// <param name="predicate">满足条件限制，可为空</param>
    /// </summary>
    public static List<Transform> GetAllChildren(this Transform transform, Func<Transform, int, bool> predicate = null)
    {
        var result = new List<Transform>();
        if (transform == null) return result;
        for (int i = 0; i < transform.childCount; i++)
        {
            var child = transform.GetChild(i);
            if (predicate == null || predicate(child, i))
                result.Add(child);
        }
        return result;
    }


    /// <summary>
    /// 将一个值通过映射转换为另一个值
    /// <param name="key">原值</param>
    /// <param name="map">映射表，形式如(a,A),(b,B)</param>
    /// </summary>
    public static R MapValue<T, R>(this T key, IDictionary<T,R> map, R defaultValue = default)
    {
        if (map != null && map.ContainsKey(key))
            return map[key];
        return defaultValue;
    }

    /// <summary>
    /// String转int
    /// </summary>
    public static int TryParseInt(this string str, int defaultValue = 0)
    {
        return int.TryParse(str, out int value) ? value : defaultValue;
    }

    /// <summary>
    /// String转long
    /// </summary>
    public static long TryParseLong(this string str, long defaultValue = 0)
    {
        return long.TryParse(str, out long value) ? value : defaultValue;
    }
    
    public static int TryCastInt(this object obj, int defaultValue = 0)
    {
        switch (obj)
        {
            case int val:
                return val;
            case long val:
                return (int)val;
            case float val:
                return (int)val;
            case double val:
                return (int)val;
            case byte val:
                return (int)val;
            case decimal val:
                return (int)val;
            default:
                return defaultValue;
        }
    }
        
    /// <summary>
    /// list.Contains()的安全版本
    /// </summary>
    public static bool IsOneOf<T>(this T value, params T[] list)
    {
        return list != null && Array.IndexOf(list, value) != -1;
    }
        
    /// <summary>
    /// v ?? newV 的安全版本
    /// </summary>
    public static T SafeValue<T>(this T value, T safeValue, T unsafeValue = default)
    {
        return value.Equals(unsafeValue) ? safeValue : value;
    }

    /// <summary>
    /// map.TryGetValue()的安全版本
    /// </summary>
    public static T2 TryGetValue<T1, T2>(this IDictionary<T1, T2> map, T1 key, T2 defaultValue = default)
    {
        if (map != null && map.ContainsKey(key))
        {
            return map[key];
        }
        return defaultValue;
    }

    /// <summary>
    /// list[i]的安全版本
    /// </summary>
    public static T TryGetValue<T>(this IList<T> list, int index, T defaultValue = default)
    {
        if (list != null && index >= 0 && index < list.Count)
        {
            return list[index];
        }
        return defaultValue;
    }

    /// <summary>
    /// IsNullOrEmpty的安全版本
    /// </summary>
    public static bool IsNullOrEmpty(this string str)
    {
        return string.IsNullOrEmpty(str);
    }

    /// <summary>
    /// AnimatorStateInfo.IsName多参数版本
    /// </summary>
    public static bool IsName(this AnimatorStateInfo stateInfo, params string[] names)
    {
        return names.Any(stateInfo.IsName);
    }
}