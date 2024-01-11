using System;
using System.Collections;
using AppBase.Resource;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.ResourceManagement.ResourceProviders;
using UnityEngine.SceneManagement;
using Object = UnityEngine.Object;

namespace AppBase.Resource
{
    /// <summary>
    /// 资源加载器，可以用来缓存资源
    /// </summary>
    public class ResourceHandler : Retainable, IEnumerator
    {
        private const string TAG = "ResourceHandler";
        private event Action<ResourceHandler> callback;

        /// <summary>
        /// 资源地址
        /// </summary>
        private string address;
        public string Address => address;
        
        /// <summary>
        /// 资源加载句柄
        /// </summary>
        private AsyncOperationHandle handler;
        
        /// <summary>
        /// 是否加载成功
        /// </summary>
        public bool IsSuccess => handler.IsValid() && handler.Status == AsyncOperationStatus.Succeeded;
        private bool IsLoading;

        public ResourceHandler(string address)
        {
            this.address = address;
        }
        
        /// <summary>
        /// 加载资源
        /// </summary>
        /// <param name="callback">加载完成回调，无论成功失败都会回调，需要调用IsSuccess自行判定是否加载成功</param>
        public ResourceHandler LoadAsset<T>(Action<ResourceHandler> callback) where T : Object
        {
            if (IsSuccess)
            {
                callback?.Invoke(this);
                return this;
            }
            if (callback != null) this.callback += callback;
            if (!IsLoading)
            {
                IsLoading = true;
                handler = Addressables.LoadAssetAsync<T>(address);
                handler.Completed += OnLoadCompleted;
            }
            return this;
        }

        /// <summary>
        /// 加载资源并实例化
        /// </summary>
        /// <param name="instantParams">实例化参数</param>
        /// <param name="callback">加载完成回调，无论成功失败都会回调，需要调用IsSuccess自行判定是否加载成功</param>
        public ResourceHandler LoadInstantiation(InstantiationParameters instantParams, Action<ResourceHandler> callback)
        {
            if (IsSuccess)
            {
                callback?.Invoke(this);
                return this;
            }
            if (callback != null) this.callback += callback;
            if (!IsLoading)
            {
                IsLoading = true;
                handler = Addressables.InstantiateAsync(address, instantParams);
                handler.Completed += OnLoadCompleted;
            }
            return this;
        }

        private void OnLoadCompleted(AsyncOperationHandle inHandler)
        {
            IsLoading = false;
            inHandler.Completed -= OnLoadCompleted;
            if (inHandler.Status != AsyncOperationStatus.Succeeded)
            {
                Debugger.LogError(TAG, $"OnLoadAsset Failed: {address}");
            }
            callback?.Invoke(this);
            callback = null;
        }

        /// <summary>
        /// 释放资源
        /// </summary>
        protected override void OnDestroy()
        {
            Debugger.Log(TAG, $"OnDestroy: {address}");
            IsLoading = false;
            callback = null;
            if (handler.IsValid())
            {
                handler.Completed -= OnLoadCompleted;

                //卸载Unity场景
                if (handler.Result is SceneInstance scene && scene.Scene.IsValid())
                {
                    Addressables.UnloadSceneAsync(scene);
                }
                
                //卸载资源
                Addressables.Release(handler);
            }
            GameBase.Instance?.GetModule<ResourceManager>()?.OnHandlerDestroy(this);
        }

        /// <summary>
        /// 获取资源
        /// </summary>
        public Object GetAsset()
        {
            if (!IsSuccess) return null;
            return (Object)handler.Result;
        }

        /// <summary>
        /// 获取资源T
        /// </summary>
        public T GetAsset<T>() where T : Object
        {
            if (!IsSuccess) return null;
            return handler.Result as T;
        }

        /// <summary>
        /// 实例化资源
        /// </summary>
        public GameObject GetInstantiation(InstantiationParameters instantParams)
        {
            if (!IsSuccess) return null;
            var newGo = handler.Result as GameObject;
            if (newGo == null) return null;
            return instantParams.SetPositionRotation ?
                GameObject.Instantiate(newGo, instantParams.Position, instantParams.Rotation, instantParams.Parent) :
                GameObject.Instantiate(newGo, instantParams.Parent, instantParams.InstantiateInWorldPosition);
        }

        /// <summary>
        /// 同步等待资源加载完成
        /// </summary>
        /// <returns>返回资源</returns>
        internal object WaitForCompletionInternal()
        {
            var obj = handler.WaitForCompletion();
            if (callback != null)
            {
                OnLoadCompleted(handler);
            }
            return obj;
        }

        #region UnityScene相关

        /// <summary>
        /// 加载Unity场景
        /// </summary>
        /// <param name="loadMode">模式</param>
        /// <param name="activateOnLoad">是否自动激活</param>
        /// <param name="callback">加载完成回调，无论成功失败都会回调，需要调用IsSuccess自行判定是否加载成功</param>
        public ResourceHandler LoadUnityScene(LoadSceneMode loadMode, bool activateOnLoad, Action<ResourceHandler> callback)
        {
            if (IsSuccess)
            {
                callback?.Invoke(this);
                return this;
            }
            if (callback != null) this.callback += callback;
            if (!IsLoading)
            {
                IsLoading = true;
                handler = Addressables.LoadSceneAsync(address, loadMode, activateOnLoad);
                handler.Completed += OnLoadCompleted;
            }
            return this;
        }

        /// <summary>
        /// 卸载Unity场景
        /// </summary>
        public IEnumerator UnloadUnityScene(Action<SceneInstance> callback = null)
        {
            if (!IsSuccess || IsLoading) return null;
            var sceneInstance = GetSceneInstance();
            if (sceneInstance.Scene.IsValid())
            {
                var handle = Addressables.UnloadSceneAsync(sceneInstance);
                if (callback != null)
                {
                    handle.Completed += h => callback.Invoke(h.Result);
                }
                return handle;
            }
            return null;
        }

        /// <summary>
        /// 获取UnitySceneInstance
        /// </summary>
        public SceneInstance GetSceneInstance()
        {
            if (!IsSuccess || handler.Result is not SceneInstance scene) return default;
            return scene;
        }
        
        /// <summary>
        /// 激活Unity场景
        /// </summary>
        public AsyncOperation ActivateUnityScene(Action<Scene> callback)
        {
            if (IsSuccess)
            {
                var sceneInstance = GetSceneInstance();
                if (sceneInstance.Scene.IsValid())
                {
                    var operation = sceneInstance.ActivateAsync();
                    if (callback != null)
                    {
                        operation.completed += h => callback.Invoke(sceneInstance.Scene);
                    }
                    return operation;
                }
            }
            callback?.Invoke(default);
            return null;
        }

        #endregion

        #region 协程相关
        public bool MoveNext()
        {
            if (handler.IsValid() && handler.IsDone && callback != null)
            {
                OnLoadCompleted(handler);
            }
            return !handler.IsDone;
        }

        public object Current => handler.Result;
        public void Reset() {}
        #endregion
    }
}

/// <summary>
/// 资源加载器扩展方法，防止Handler为空导致空指针异常
/// </summary>
public static class ResourceHandlerExtension
{
    /// <summary>
    /// 同步等待资源加载完成
    /// </summary>
    /// <returns>返回资源</returns>
    public static object WaitForCompletion(this ResourceHandler handler)
    {
        return handler?.WaitForCompletionInternal();
    }

    /// <summary>
    /// 同步等待资源加载完成
    /// </summary>
    /// <returns>返回资源</returns>
    public static T WaitForCompletion<T>(this ResourceHandler handler) where T : Object
    {
        return handler?.WaitForCompletionInternal() as T;
    }
}