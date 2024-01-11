using UnityEditor;
using UnityEditor.Callbacks;

public static class BuildConfigUtil
{
    public const string configPath = "Assets/Project/AddressableRes/Configs/m_Data_dl/GlobalConfig.asset";
    
    [DidReloadScripts]
    public static void RegisterBuildConfigUtil()
    {
        BuildNative.OnBeforeBuildNative -= OnBeforeBuildNative;
        BuildNative.OnBeforeBuildNative += OnBeforeBuildNative;
    }
    
    /// <summary>
    /// 打包时，读取配置写入包体
    /// </summary>
    public static void OnBeforeBuildNative()
    {
        var config = AssetDatabase.LoadAssetAtPath<GlobalConfigList>(configPath).map;
        var isIos = EditorUserBuildSettings.activeBuildTarget == BuildTarget.iOS;
        PlayerSettings.bundleVersion = config[isIos ? GlobalConfigKeys.VersionApple : GlobalConfigKeys.VersionGoogle].Value;
        if (isIos)
        {
            PlayerSettings.iOS.buildNumber = config[GlobalConfigKeys.VersionCodeApple].Value;
        }
        else
        {
            PlayerSettings.Android.bundleVersionCode = int.Parse(config[GlobalConfigKeys.VersionCodeGoogle].Value);
        }
        PlayerSettings.applicationIdentifier = config[isIos ? GlobalConfigKeys.BundleIdApple : GlobalConfigKeys.BundleIdGoogle].Value;
        PlayerSettings.productName = config[GlobalConfigKeys.AppDisplayName].Value;
        PlayerSettings.companyName = config[GlobalConfigKeys.CompanyName].Value;
    }
}
