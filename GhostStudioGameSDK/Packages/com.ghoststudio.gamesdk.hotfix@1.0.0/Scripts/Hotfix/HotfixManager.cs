using System;
using System.Collections;
using AppBase.Config;
using AppBase.Module;
using AppBase.Utils;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;

namespace AppBase.Hotfix
{
    /// <summary>
    /// 热更新管理器
    /// </summary>
    public class HotfixManager : ModuleBase
    {
        /// <summary>
        /// 资源服务器地址
        /// </summary>
        public string ResourceServerUrl => GameBase.Instance.GetModule<ConfigManager>().GetConfigByKey<string, GlobalConfig>(AAConst.GlobalConfig, AppUtil.IsDebug ? GlobalConfigKeys.ResourceServerUrl_Debug : GlobalConfigKeys.ResourceServerUrl)?.Value;
        
        /// <summary>
        /// 资源目录版本
        /// </summary>
        public string ResourceCatalogVersion => GameBase.Instance.GetModule<ConfigManager>().GetConfigByKey<string, GlobalConfig>(AAConst.GlobalConfig, AppUtil.IsDebug ? GlobalConfigKeys.ResourceCatalogVersion_Debug : GlobalConfigKeys.ResourceCatalogVersion)?.Value;
        
        /// <summary>
        /// 超时时间（秒）
        /// </summary>
        public const int timeout = 5;

        protected override void OnInit()
        {
            base.OnInit();
            Debugger.SetLogEnable(TAG);
        }
        
        /// <summary>
        /// 初始化Addressable热更新
        /// </summary>
        public IEnumerator InitHotfix()
        {
            yield return Addressables.InitializeAsync();
        }

        /// <summary>
        /// 请求热更新
        /// </summary>
        public IEnumerator RequestHotfixUpdate(Action<float> progress = null, Action<bool> callback = null)
        {
            //检查Catalog是否有更新
            Debugger.Log(TAG, "CheckForCatalogUpdates");
            var checkCatalog = Addressables.CheckForCatalogUpdates(false);
            yield return new WaitForHandler(checkCatalog, timeout);
            var isSuccess = checkCatalog.Status == AsyncOperationStatus.Succeeded && checkCatalog.Result.Count > 0;
            Addressables.Release(checkCatalog);
            if (!isSuccess)
            {
                callback?.Invoke(false);
                yield break;
            }
            progress?.Invoke(0.05f);
            //更新Catalog
            Debugger.Log(TAG, "UpdateCatalogs");
            var updateHandler = Addressables.UpdateCatalogs(false);
            yield return new WaitForHandler(checkCatalog, timeout);
            isSuccess = updateHandler.Status == AsyncOperationStatus.Succeeded;
            Addressables.Release(updateHandler);
            if (!isSuccess)
            {
                callback?.Invoke(false);
                yield break;
            }
            progress?.Invoke(0.1f);
            //更新资源
            Debugger.Log(TAG, "DownloadDependenciesAsync");
            var hotfixHandler = Addressables.DownloadDependenciesAsync("Hotfix");
            yield return new WaitForDownload(hotfixHandler, timeout, p => progress?.Invoke(p * 0.9f + 0.1f));
            isSuccess = hotfixHandler.Status == AsyncOperationStatus.Succeeded;
            callback?.Invoke(isSuccess);
        }
    }
}
