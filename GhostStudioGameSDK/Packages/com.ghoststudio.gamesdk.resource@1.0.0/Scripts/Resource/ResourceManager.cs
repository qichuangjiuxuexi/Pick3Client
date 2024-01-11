using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AppBase.Module;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.ResourceProviders;
using UnityEngine.SceneManagement;
using Object = UnityEngine.Object;

namespace AppBase.Resource
{
    /// <summary>
    /// 资源管理器
    /// </summary>
    public class ResourceManager : MonoModule
    {
        /// <summary>
        /// 资源缓存池
        /// </summary>
        protected Dictionary<string, ResourceHandler> assetsPool = new();

        protected override void OnInit()
        {
            base.OnInit();
            Addressables.InitializeAsync().WaitForCompletion();
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();
            assetsPool.Clear();
        }

        /// <summary>
        /// 加载资源
        /// </summary>
        /// <param name="address">地址</param>
        /// <param name="reference">标记生命周期的引用，当引用被释放时，资源也一起被释放</param>
        /// <param name="successCallback">加载成功时的回调</param>
        /// <param name="failureCallback">加载失败时的回调</param>
        /// <typeparam name="T">资源类型</typeparam>
        /// <returns>加载器</returns>
        public ResourceHandler LoadAsset<T>(string address, IResourceReference reference, Action<T> successCallback = null, Action failureCallback = null) where T : Object
        {
            if (!assetsPool.TryGetValue(address, out var handler))
            {
                handler = new ResourceHandler(address);
                assetsPool[address] = handler;
            }
            handler.LoadAsset<T>(h =>
            {
                if (h.IsSuccess && reference.IsValid())
                {
                    var obj = h.GetAsset<T>();
                    if (obj != null)
                    {
                        reference?.AddHandler(h);
                        successCallback?.Invoke(obj);
                        return;
                    }
                }
                h.CheckRetainCount();
                failureCallback?.Invoke();
            });
            return handler;
        }

        /// <summary>
        /// 批量加载资源
        /// </summary>
        /// <param name="addresses">地址列表</param>
        /// <param name="reference">标记生命周期的引用，当引用被释放时，资源也一起被释放</param>
        /// <param name="successCallback">加载一个成功时的回调</param>
        /// <param name="failureCallback">加载一个失败时的回调</param>
        /// <param name="finishedCallback">全部加载完成时回调</param>
        /// <typeparam name="T">资源类型</typeparam>
        /// <returns>加载器列表</returns>
        public List<ResourceHandler> LoadAssetes<T>(IList<string> addresses, IResourceReference reference, Action<string, T> successCallback = null, Action<string> failureCallback = null, Action<Dictionary<string, T>> finishedCallback = null) where T : Object
        {
            var list = new List<ResourceHandler>();
            var dict = finishedCallback != null ? new Dictionary<string, T>() : null;
            int count = 0;
            foreach (var address in addresses)
            {
                var handler = LoadAsset<T>(address, reference, obj =>
                {
                    successCallback?.Invoke(address, obj);
                    if (finishedCallback != null)
                    {
                        dict[address] = obj;
                        if (++count >= addresses.Count)
                        {
                            finishedCallback.Invoke(dict);
                        }
                    }
                }, () =>
                {
                    failureCallback?.Invoke(address);
                    if (finishedCallback != null && ++count >= addresses.Count)
                    {
                        finishedCallback.Invoke(dict);
                    }
                });
                list.Add(handler);
            }
            return list;
        }
        
        /// <summary>
        /// 加载资源handler，注意需要手动调用Release来管理生命周期，否则会造成内存泄露
        /// </summary>
        /// <param name="address">地址</param>
        /// <param name="successCallback">加载成功时的回调</param>
        /// <param name="failureCallback">加载失败时的回调</param>
        /// <typeparam name="T">资源类型</typeparam>
        /// <returns>加载器</returns>
        public ResourceHandler LoadAssetHandler<T>(string address, Action<ResourceHandler> successCallback = null, Action failureCallback = null) where T : Object
        {
            if (!assetsPool.TryGetValue(address, out var handler))
            {
                handler = new ResourceHandler(address);
                assetsPool[address] = handler;
            }
            handler.LoadAsset<T>(h =>
            {
                if (h.IsSuccess)
                {
                    h.Retain();
                    successCallback?.Invoke(h);
                }
                else
                {
                    h.CheckRetainCount();
                    failureCallback?.Invoke();
                }
            });
            return handler;
        }
        
        /// <summary>
        /// 加载Unity场景，注意需要手动调用Release来管理生命周期，否则会造成内存泄露
        /// </summary>
        /// <param name="address">地址</param>
        /// <param name="loadMode">模式</param>
        /// <param name="activateOnLoad">是否自动激活</param>
        /// <param name="successCallback">加载成功回调</param>
        /// <param name="failureCallback">加载失败回调</param>
        /// <returns>加载器</returns>
        public ResourceHandler LoadUnitySceneHandler(string address, LoadSceneMode loadMode = LoadSceneMode.Single, bool activateOnLoad = false, Action<ResourceHandler> successCallback = null, Action failureCallback = null)
        {
            var handler = new ResourceHandler(address);
            handler.LoadUnityScene(loadMode, activateOnLoad, h =>
            {
                if (h.IsSuccess)
                {
                    h.Retain();
                    successCallback?.Invoke(h);
                }
                else
                {
                    h.CheckRetainCount();
                    failureCallback?.Invoke();
                }
            });
            return handler;
        }
        
        /// <summary>
        /// 实例化游戏对象，资源生命周期跟随实例化的游戏对象
        /// </summary>
        /// <param name="address">地址</param>
        /// <param name="instantParams">实例化参数</param>
        /// <param name="successCallback">加载成功时的回调</param>
        /// <param name="failureCallback">加载失败时的回调</param>
        /// <typeparam name="T">资源类型</typeparam>
        /// <returns>加载器</returns>
        public ResourceHandler InstantGameObject(string address, InstantiationParameters instantParams, Action<GameObject> successCallback = null, Action failureCallback = null)
        {
            var handler = new ResourceHandler(address);
            handler.LoadInstantiation(instantParams, h =>
            {
                if (h.IsSuccess)
                {
                    var obj = h.GetAsset<GameObject>();
                    if (obj != null)
                    {
                        obj.name = obj.name.Replace("(Clone)", "");
                        obj.GetResourceReference().AddHandler(h);
                        successCallback?.Invoke(obj);
                        return;
                    }
                }
                h.CheckRetainCount();
                failureCallback?.Invoke();
            });
            return handler;
        }

        /// <summary>
        /// 实例化游戏对象，资源生命周期跟随实例化的游戏对象
        /// </summary>
        /// <param name="address">地址</param>
        /// <param name="parent">实例化父节点</param>
        /// <param name="successCallback">加载成功时的回调</param>
        /// <param name="failureCallback">加载失败时的回调</param>
        /// <typeparam name="T">资源类型</typeparam>
        /// <returns>加载器</returns>
        public ResourceHandler InstantGameObject(string address, Transform parent, Action<GameObject> successCallback = null, Action failureCallback = null)
        {
            return InstantGameObject(address, new InstantiationParameters(parent , false), successCallback, failureCallback);
        }

        /// <summary>
        /// 当加载器引用计数为0时，自动释放资源
        /// </summary>
        public bool OnHandlerDestroy(ResourceHandler handler)
        {
            if (handler != null && handler.RetainCount <= 0 && assetsPool.TryGetValue(handler.Address, out var oldHandler) && oldHandler == handler)
            {
                return assetsPool.Remove(handler.Address);
            }
            return false;
        }

        /// <summary>
        /// 检查地址是否存在
        /// </summary>
        /// <param name="address">地址</param>
        /// <typeparam name="T">资源类型</typeparam>
        /// <returns>地址是否存在</returns>
        public bool IsAddressExist<T>(string address) where T : Object
        {
            return Addressables.ResourceLocators.Any(r => r.Locate(address, typeof(T), out _));
        }
    }
}