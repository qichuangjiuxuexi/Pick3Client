using System;
using System.Collections.Generic;
using AppBase.Resource;
using UnityEngine;
using UnityEngine.Pool;

namespace AppBase.UI.Scene
{
    /// <summary>
    /// 场景基类
    /// 时序：OnLoad -> Awake -> OnAwake -> oldScene.OnPlayExitAnim -> OnPlayEnterAnim -> oldScene.OnBeforeDestroy -> oldScene.OnDestroy
    /// </summary>
    public class SceneBase : UIView
    {
        /// <summary>
        /// 场景数据
        /// </summary>
        public SceneData sceneData;
        
        /// <summary>
        /// 当加载场景完成时调用，用来加载场景其他资源，加载完成后，调用callback返回
        /// 注意此时场景还没有激活，IsActive = false
        /// </summary>
        /// <param name="callback">加载完成后，调用callback返回</param>
        public virtual void OnLoad(Action callback)
        {
            callback?.Invoke();
        }

        /// <summary>
        /// 当场景激活时调用，用来初始化场景，初始化完成后，调用callback返回
        /// </summary>
        /// <param name="callback">初始化完成后，调用callback返回</param>
        public virtual void OnAwake(Action callback)
        {
            callback?.Invoke();
        }
        
        /// <summary>
        /// 当进入场景时调用，用来播放入场动画，播放完成后，调用callback返回，才会将上一个场景销毁
        /// </summary>
        /// <param name="callback">播放完成后，调用callback返回</param>
        public virtual void OnPlayEnterAnim(Action callback)
        {
            callback?.Invoke();
        }

        /// <summary>
        /// 当退出场景时调用，用来播放出场动画，播放完成后，调用callback返回
        /// </summary>
        /// <param name="callback">播放完成后，调用callback返回</param>
        public virtual void OnPlayExitAnim(Action callback)
        {
            callback?.Invoke();
        }

        /// <summary>
        /// 在场景销毁前调用
        /// </summary>
        public virtual void OnBeforeDestroy()
        {
        }
        
        #region 场景对象池

        private Dictionary<string, ObjectPool<GameObject>> AllPool = new();
        private Dictionary<string, List<GameObject>> AllGameObject = new();

        public GameObject GetGameObjectForPool(string address, Transform parent)
        {
            if (!AllPool.ContainsKey(address))
            {
                AllPool.Add(address, new ObjectPool<GameObject>(() =>
                {
                    var handler = GameBase.Instance.GetModule<ResourceManager>().InstantGameObject(address, parent);
                    return handler.WaitForCompletion<GameObject>();
                }, actionOnDestroy: OnDestroyGameObject));
            }
            GameObject o = AllPool[address].Get();
            // AllPool[address].Release(o);
            o.transform.SetParent(parent);
            o.transform.localPosition = Vector3.zero;
            o.transform.localScale = Vector3.one;
            o.transform.localEulerAngles = Vector3.zero;
            o.SetActive(true);
            if (!AllGameObject.ContainsKey(address))
                AllGameObject.Add(address, new List<GameObject>());
            AllGameObject[address].Add(o);
            return o;
        }

        public void ReleaseGameObject(GameObject o)
        {
            string addres = RemoveGameobj(o);
            if (!string.IsNullOrEmpty(addres))
                AllPool[addres].Release(o);
        }

        private string RemoveGameobj(GameObject o)
        {
            foreach (var item in AllGameObject)
            {
                if (item.Value.Remove(o))
                {
                    o.SetActive(false);
                    o.transform.SetParent(transform);
                    return item.Key;
                }
            }
            return "";
        }

        private void OnDestroyGameObject(GameObject o)
        {
            RemoveGameobj(o);
            Destroy(o);
        }
        
        /// <summary>
        /// 内部使用
        /// </summary>
        internal void OnInternalDestroy()
        {
            foreach (var item in AllPool)
            {
                item.Value.Dispose();
            }
            foreach (var item in AllGameObject)
            {
                for (int i = item.Value.Count - 1; i >= 0; i--)
                {
                    Destroy(item.Value[i]);
                }
            }
        }

        #endregion
    }
}