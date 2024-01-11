using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace WordGame.Utils
{
    /// <summary>
    /// Transform 扩展类
    /// </summary>
    public static class UnityExtensions
    {
        #region Transform 位置扩展

        /// <summary>
        /// x位置扩展
        /// </summary>
        /// <param name="t"></param>
        /// <param name="newX"></param>
        public static void SetLocalPositionX(this Transform t, float newX)
        {
            t.localPosition = new Vector3(newX, t.localPosition.y, t.localPosition.z);
        }

        /// <summary>
        /// y位置扩展
        /// </summary>
        /// <param name="t"></param>
        /// <param name="newY"></param>
        public static void SetLocalPositionY(this Transform t, float newY)
        {
            t.localPosition = new Vector3(t.localPosition.x, newY, t.localPosition.z);
        }

        /// <summary>
        /// z位置扩展
        /// </summary>
        /// <param name="t"></param>
        /// <param name="newZ"></param>
        public static void SetLocalPositionZ(this Transform t, float newZ)
        {
            t.localPosition = new Vector3(t.localPosition.x, t.localPosition.y, newZ);
        }

        /// <summary>
        /// x,y位置扩展
        /// </summary>
        /// <param name="t"></param>
        /// <param name="newX"></param>
        /// <param name="newY"></param>
        public static void SetLocalPositionXY(this Transform t, float newX, float newY)
        {
            t.localPosition = new Vector3(newX, newY, t.localPosition.z);
        }

        /// <summary>
        /// x,z位置扩展
        /// </summary>
        /// <param name="t"></param>
        /// <param name="newX"></param>
        /// <param name="newZ"></param>
        public static void SetLocalPositionXZ(this Transform t, float newX, float newZ)
        {
            t.localPosition = new Vector3(newX, t.localPosition.y, newZ);
        }

        /// <summary>
        /// y,z位置扩展
        /// </summary>
        /// <param name="t"></param>
        /// <param name="newY"></param>
        /// <param name="newZ"></param>
        public static void SetLocalPositionYZ(this Transform t, float newY, float newZ)
        {
            t.localPosition = new Vector3(t.localPosition.x, newY, newZ);
        }

        #endregion

        #region 旋转扩展

        /// <summary>
        /// x旋转扩展
        /// </summary>
        /// <param name="t"></param>
        /// <param name="newX"></param>
        public static void SetLocalRotateX(this Transform t, float newX)
        {
            t.localEulerAngles = new Vector3(newX, t.localEulerAngles.y, t.localEulerAngles.z);
        }

        /// <summary>
        /// y旋转扩展
        /// </summary>
        /// <param name="t"></param>
        /// <param name="newY"></param>
        public static void SetLocalRotateY(this Transform t, float newY)
        {
            t.localEulerAngles = new Vector3(t.localEulerAngles.x, newY, t.localEulerAngles.z);
        }

        /// <summary>
        /// z旋转扩展
        /// </summary>
        /// <param name="t"></param>
        /// <param name="newZ"></param>
        public static void SetLocalRotateZ(this Transform t, float newZ)
        {
            t.localEulerAngles = new Vector3(t.localEulerAngles.x, t.localEulerAngles.y, newZ);
        }

        /// <summary>
        /// x,y旋转扩展
        /// </summary>
        /// <param name="t"></param>
        /// <param name="newX"></param>
        /// <param name="newY"></param>
        public static void SetLocalRotateXY(this Transform t, float newX, float newY)
        {
            t.localEulerAngles = new Vector3(newX, newY, t.localEulerAngles.z);
        }

        /// <summary>
        /// x,z旋转扩展
        /// </summary>
        /// <param name="t"></param>
        /// <param name="newX"></param>
        /// <param name="newZ"></param>
        public static void SetLocalRotateXZ(this Transform t, float newX, float newZ)
        {
            t.localEulerAngles = new Vector3(newX, t.localEulerAngles.y, newZ);
        }

        /// <summary>
        /// y,z旋转扩展
        /// </summary>
        /// <param name="t"></param>
        /// <param name="newY"></param>
        /// <param name="newZ"></param>
        public static void SetLocalRotateYZ(this Transform t, float newY, float newZ)
        {
            t.localEulerAngles = new Vector3(t.localEulerAngles.x, newY, newZ);
        }

        #endregion

        #region 缩放扩展

        /// <summary>
        /// x缩放扩展
        /// </summary>
        /// <param name="t"></param>
        /// <param name="newX"></param>
        public static void SetLocalScaleX(this Transform t, float newX)
        {
            t.localScale = new Vector3(newX, t.localScale.y, t.localScale.z);
        }

        /// <summary>
        /// y缩放扩展
        /// </summary>
        /// <param name="t"></param>
        /// <param name="newY"></param>
        public static void SetLocalScaleY(this Transform t, float newY)
        {
            t.localScale = new Vector3(t.localScale.x, newY, t.localScale.z);
        }

        /// <summary>
        /// z缩放扩展
        /// </summary>
        /// <param name="t"></param>
        /// <param name="newZ"></param>
        public static void SetLocalScaleZ(this Transform t, float newZ)
        {
            t.localScale = new Vector3(t.localScale.x, t.localScale.y, newZ);
        }

        /// <summary>
        /// x,y缩放扩展
        /// </summary>
        /// <param name="t"></param>
        /// <param name="newX"></param>
        /// <param name="newY"></param>
        public static void SetLocalScaleXY(this Transform t, float newX, float newY)
        {
            t.localScale = new Vector3(newX, newY, t.localScale.z);
        }

        /// <summary>
        /// x,z缩放扩展
        /// </summary>
        /// <param name="t"></param>
        /// <param name="newX"></param>
        /// <param name="newZ"></param>
        public static void SetLocalScaleXZ(this Transform t, float newX, float newZ)
        {
            t.localScale = new Vector3(newX, t.localScale.y, newZ);
        }

        /// <summary>
        /// y,z缩放扩展
        /// </summary>
        /// <param name="t"></param>
        /// <param name="newY"></param>
        /// <param name="newZ"></param>
        public static void SetLocalScaleYZ(this Transform t, float newY, float newZ)
        {
            t.localScale = new Vector3(t.localScale.x, newY, newZ);
        }

        #endregion


        #region RectTransform.

        /// <summary>
        /// 转换为 RectTransform
        /// </summary>
        /// <param name="t"></param>
        /// <returns></returns>
        public static RectTransform GetRecttransform(this Transform t)
        {
            if (t != null)
            {
                return t as RectTransform;
            }

            return null;
        }
        
        public static RectTransform AddFullScreenRectTransform(this GameObject obj)
        {
            if (obj == null) return null;
            var rect = obj.GetOrAddComponent<RectTransform>();
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.offsetMin = rect.offsetMax = Vector2.zero;
            return rect;
        }

        #endregion
        
        #region GameObject物件扩展

        public static void SetAct(this GameObject t, bool activeState)
        {
            if (t != null)
            {
                if (t.activeSelf != activeState)
                {
                    t.SetActive(activeState);
                }
            }
        }
        
        public static void SetActive(this Component component, bool isActive)
        {
            if (component != null && component.gameObject.activeSelf != isActive)
            {
                component.gameObject.SetActive(isActive);
            }
        }
        
        /// <summary>
        /// 播放动画，如果Inactive状态则在Active时开始播放
        /// </summary>
        public static void PlayAnimator(this Component component, string animName, float offset = 0)
        {
            var animator = component?.GetComponent<Animator>();
            if (animator == null || string.IsNullOrEmpty(animName)) return;
            animator.keepAnimatorStateOnDisable = true;
            animator.PlayInFixedTime(animName, 0, offset);
        }
        
        /// <summary>
        /// 播放动画并立即更新
        /// </summary>
        public static float PlayAnimatorUpdate(this Component component, string animName, float offset = 0)
        {
            var animator = component?.GetComponent<Animator>();
            if (animator == null || string.IsNullOrEmpty(animName)) return 0;
            animator.keepAnimatorStateOnDisable = true;
            animator.PlayInFixedTime(animName, 0, offset);
            animator.Update(0);
            var duration = animator.GetCurrentAnimatorStateInfo(0).length - offset;
            return duration;
        }
        
        /// <summary>
        /// 播放动画并，播放完成后回调
        /// </summary>
        public static void PlayAnimCallback(this Component component, string animName, Action callback)
        {
            if (component == null || !component.gameObject.activeInHierarchy)
            {
                return;
            }
            var animator = component?.GetComponent<Animator>();
            if (animator == null || string.IsNullOrEmpty(animName)) return;
            animator.PlayInFixedTime(animName, 0, 0);
            animator.Update(0);
            var animatorMono = animator.gameObject.AddComponent<AnimatorCoroutineMono>();
            animatorMono.DelayAni(animName,callback);
        }

        /// <summary>
        /// 移除某个动画的回调
        /// </summary>
        public static void RemoveCallback(this Animator animator, string animName)
        {
            if (animator == null || string.IsNullOrEmpty(animName)) return;
            var animatorMonos = animator.gameObject.GetComponents<AnimatorCoroutineMono>();
            foreach (var animatorMono in animatorMonos)
            {
                if (animatorMono.AnimName == animName)
                {
                    GameObject.Destroy(animatorMono);
                    break;
                }
            }
        }

        public static void RemoveAllCallback(this Animator animator)
        {
            var animatorMonos = animator.gameObject.GetComponents<AnimatorCoroutineMono>();
            foreach (var animatorMono in animatorMonos)
            {
                GameObject.Destroy(animatorMono);
            }
        }

        public static void AddListener(this Button button, UnityAction action)
        {
            if (button == null || action == null) return;
            button.onClick.RemoveAllListeners();
            button.onClick.AddListener(action);
        }

        public static GameObject AddGameObject(this GameObject gameObject, string subObjName)
        {
            if (gameObject == null) return null;
            var subObject = new GameObject(subObjName);
            subObject.transform.SetParent(gameObject.transform);
            subObject.transform.localPosition = Vector3.zero;
            subObject.transform.localScale = Vector3.one;
            subObject.layer = gameObject.layer;
            return subObject;
        }
        
        
        public static GameObject AddInstantiate(this GameObject gameObject, GameObject template)
        {
            if (gameObject == null || template == null) return null;
            var subObject = GameObject.Instantiate(template, gameObject.transform, false);
            subObject.transform.localPosition = Vector3.zero;
            subObject.transform.localScale = Vector3.one;
            return subObject;
        }
        
        public static Transform Finds(this Transform transform, params string[] paths)
        {
            if (transform == null || paths.Length == 0) return null;
            foreach (var name in paths)
            {
                var t = transform.Find(name);
                if (t != null) return t;
            }
            return null;
        }

        #endregion

        #region 事件扩展
        /// <summary>
        /// 绑定点击事件
        /// </summary>
        /// <param name="eventTrigger"></param>
        /// <param name="type"></param>
        /// <param name="call"></param>
        /// <param name="uiSound"></param>
        public static void AddListener(this EventTrigger eventTrigger, EventTriggerType type,
            UnityAction<BaseEventData> call)
        {
            EventTrigger.Entry entry = eventTrigger.getEntry(type);
            entry.callback.AddListener(call);
        }

        /// <summary>
        /// 解绑事件
        /// </summary>
        /// <param name="eventTrigger"></param>
        /// <param name="type"></param>
        public static void RemoveListener(this EventTrigger eventTrigger, EventTriggerType type)
        {
            EventTrigger.Entry entry = eventTrigger.getEntry(type);
            entry.callback.RemoveAllListeners();
        }


        /// <summary>
        /// 寻找Entry
        /// </summary>
        /// <param name="eventTrigger"></param>
        /// <param name="type"></param>
        /// <param name="uiSound"></param>
        /// <returns></returns>
        private static EventTrigger.Entry getEntry(this EventTrigger eventTrigger, EventTriggerType type)
        {
            EventTrigger.Entry entry = null;
            for (int i = 0; i < eventTrigger.triggers.Count; i++)
            {
                if (eventTrigger.triggers[i].eventID == type)
                {
                    entry = eventTrigger.triggers[i];
                    break;
                }
            }

            if (entry == null)
            {
                entry = new EventTrigger.Entry();
                entry.eventID = type;
                eventTrigger.triggers.Add(entry);
            }

            return entry;
        }
        
        /// <summary>
        /// 延迟回调，如果对话框关闭，则不会回调
        /// </summary>
        public static IEnumerator DelayCall(this MonoBehaviour obj, float delay, Action callBack,bool isIgnoreTimeScale = true)
        {
            if (obj == null || callBack == null) return null;
            IEnumerator iEnumerator = _delayCallBack(delay, callBack);
            obj.StartCoroutine(iEnumerator);
            return iEnumerator;
        }
        
        public static void StopDelayCall(this MonoBehaviour obj, IEnumerator iEnumerator)
        {
            if (obj == null || iEnumerator == null) return;
            obj.StopCoroutine(iEnumerator);
        }
    
        private static IEnumerator _delayCallBack(float delay, Action callBack,bool isIgnoreTimeScale = true)
        {
            if (isIgnoreTimeScale)
            {
                yield return new WaitForSecondsRealtime(delay);
            }
            else
            {
                yield return new WaitForSeconds(delay);
            }
            callBack?.Invoke();
        }
        
        public static IEnumerator DelayCallFrame(this MonoBehaviour obj, int delay, Action callBack)
        {
            if (obj == null || callBack == null) return null;
            IEnumerator iEnumerator = _delayCallBackFrame(delay, callBack);
            obj.StartCoroutine(iEnumerator);
            return iEnumerator;
        }
        
        private static IEnumerator _delayCallBackFrame(int delay, Action callBack)
        {
            while (delay > 0)
            {
                delay--;
                yield return null;
            }
            callBack?.Invoke();
        }

        #endregion

        #region 组件拓展
        /// <summary>
        /// 获取或者添加组件
        /// </summary>
        /// <typeparam name="T">组件的类型</typeparam>
        /// <param name="transform">添加的组件对象</param>
        /// <returns>获取或者添加的组件</returns>
        public static T GetOrAddComponent<T>(this Component transform) where T : Component
        {
            if (transform == null)
            {
                return null;
            }
            T behaviour = transform.GetComponent<T>();
            if (behaviour == null)
            {
                behaviour = transform.gameObject.AddComponent<T>();
            }
            return behaviour;
        }

        /// <summary>
        /// 获取或者添加组件
        /// </summary>
        /// <typeparam name="T">组件的类型</typeparam>
        /// <param name="gameObject">添加的组件对象</param>
        /// <returns>获取或添加的组件</returns>
        public static T GetOrAddComponent<T>(this GameObject gameObject) where T : Component
        {
            if (gameObject == null)
            {
                return null;
            }
            if (!gameObject.TryGetComponent(out T behaviour))
            {
                behaviour = gameObject.AddComponent<T>();
            }
            return behaviour;
        }
        
        /// <summary>
        /// 获取或者添加组件
        /// </summary>
        /// <param name="gameObject">添加的组件对象</param>
        /// <param name="type">组件的类型</param>
        /// <returns>获取或添加的组件</returns>
        public static Component GetOrAddComponent(this GameObject gameObject,Type type) 
        {
            if (gameObject == null)
            {
                return null;
            }
            if (!gameObject.TryGetComponent(type ,out Component behaviour))
            {
                behaviour = gameObject.AddComponent(type);
            }
            return behaviour;
        }
        #endregion

        #region 图片扩展
        
        public static void SetAlpha(this Graphic @this, float value)
        {
            if (@this != null)
            {
                var c = @this.color;
                c.a = value;
                @this.color = c;
            }
        }

        #endregion
    }
}