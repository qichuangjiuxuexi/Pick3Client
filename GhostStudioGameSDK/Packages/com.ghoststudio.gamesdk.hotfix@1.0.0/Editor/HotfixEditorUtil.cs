using System;
using System.IO;
using UnityEditor;
using UnityEditor.AddressableAssets;
using UnityEditor.AddressableAssets.Build;
using UnityEditor.AddressableAssets.Settings;
using UnityEditor.Build.Pipeline.Utilities;
using UnityEditor.Callbacks;
using UnityEngine;

public static class HotfixEditorUtil
{
    private const string HotfixLabel = "Hotfix";
    private const string HotfixGroupSuffix = "_hotfix";
    private const string ContentUpdateGroupName = "m_ContentUpdate" + HotfixGroupSuffix;

    /// <summary>
    /// 生成ContentStateData，为热更做准备
    /// </summary>
    [MenuItem("Tools/Hotfix/1. Build Content State Data")]
    public static void BuildContentStateData()
    {
        UpdateCatalogConfigs();
        var contentStateDataPath = ContentUpdateScript.GetContentStateDataPath(false);
        if (File.Exists(contentStateDataPath)) File.Delete(contentStateDataPath);
        var settings = AddressableAssetSettingsDefaultObject.Settings;
        var remoteBuildPath = settings.RemoteCatalogBuildPath.GetValue(settings);
        if (Directory.Exists(remoteBuildPath)) Directory.Delete(remoteBuildPath, true);
        AddressableAssetSettings.BuildPlayerContent();
        Debug.Log($"BuildPlayerContent end, contentStateDataPath: {contentStateDataPath}");
        if (!Application.isBatchMode)
        {
            var buildPath = Path.Combine(Path.GetDirectoryName(Environment.CurrentDirectory)!, "Build");
            EditorUtility.RevealInFinder(buildPath);
        }
    }
    
    /// <summary>
    /// 生成Hotfix分组
    /// </summary>
    [MenuItem("Tools/Hotfix/2. Build Hotfix Groups")]
    public static void BuildHotfix()
    {
        //读取ContentStateData
        UpdateCatalogConfigs();
        var contentStateDataPath = ContentUpdateScript.GetContentStateDataPath(false);;
        if (!File.Exists(contentStateDataPath) && !Application.isBatchMode)
        {
            contentStateDataPath = EditorUtility.OpenFilePanel("Select ContentStateData", Environment.CurrentDirectory, "bin");
        }
        if (!File.Exists(contentStateDataPath)) throw new FileNotFoundException(contentStateDataPath);
        //清理缓存
        AddressableAssetSettings.CleanPlayerContent();
        BuildCache.PurgeCache(false);
        UnityEditor.U2D.SpriteAtlasUtility.PackAllAtlases(EditorUserBuildSettings.activeBuildTarget, false);
        var settings = AddressableAssetSettingsDefaultObject.Settings;
        var serverDataPath = settings.RemoteCatalogBuildPath.GetValue(settings);
        if (Directory.Exists(serverDataPath))
        {
            Directory.Delete(serverDataPath, true);
        }
        //增加Hotfix标签
        settings.AddLabel(HotfixLabel);
        //收集修改过的资源
        var entries = ContentUpdateScript.GatherModifiedEntries(settings, contentStateDataPath);
        //移动修改过的资源到Hotfix分组
        foreach (var entry in entries)
        {
            var groupName = entry.parentGroup == null ? ContentUpdateGroupName : entry.parentGroup.name + HotfixGroupSuffix;
            var group = AddressableUtil.CreateRemoteGroup(settings, groupName, false);
            entry.labels.Add(HotfixLabel);
            settings.CreateOrMoveEntry(entry.guid, group);
        }
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        //编译Hotfix分组
        ContentUpdateScript.BuildContentUpdate(settings, contentStateDataPath);
    }

    [DidReloadScripts]
    public static void RegisterHotfixEditorUtil()
    {
        BuildNative.OnBeforeBuildNative -= UpdateCatalogConfigs;
        BuildNative.OnBeforeBuildNative += UpdateCatalogConfigs;
    }

    /// <summary>
    /// 设置包体热更配置信息
    /// </summary>
    public static void UpdateCatalogConfigs()
    {
        var config = AssetDatabase.LoadAssetAtPath<GlobalConfigList>(BuildConfigUtil.configPath).DataMap;
        var settings = AddressableAssetSettingsDefaultObject.Settings;
#if DEBUG
        var catalogVersion = config[GlobalConfigKeys.ResourceCatalogVersion_Debug].Value;
#else
        var catalogVersion = config[GlobalConfigKeys.ResourceCatalogVersion].Value;
#endif

        //设置Remote路径信息
        settings.profileSettings.SetValue(settings.activeProfileId, AddressableAssetSettings.kRemoteBuildPath, "../Build/ServerData");
        settings.profileSettings.SetValue(settings.activeProfileId, AddressableAssetSettings.kRemoteLoadPath, "{Game.Hotfix.ResourceServerUrl}/[BuildTarget]/HotfixResources/v{Game.Hotfix.ResourceCatalogVersion}");
        
        //设置StateData信息
        settings.ContentStateBuildPath = "../Build";
        
        //设置Catalog信息
        settings.OverridePlayerVersion = catalogVersion;
        settings.BuildRemoteCatalog = true;
        settings.RemoteCatalogBuildPath.SetVariableById(settings, settings.profileSettings.GetProfileDataByName(AddressableAssetSettings.kRemoteBuildPath).Id);
        settings.RemoteCatalogLoadPath.SetVariableById(settings, settings.profileSettings.GetProfileDataByName(AddressableAssetSettings.kRemoteLoadPath).Id);
        settings.DisableCatalogUpdateOnStartup = true;
        settings.MaxConcurrentWebRequests = 10;
        settings.CatalogRequestsTimeout = 5;
        settings.OptimizeCatalogSize = true;
        EditorUtility.SetDirty(settings);
        AssetDatabase.SaveAssets();
    }
}
