using System;
using UnityEditor;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;
using UnityEngine;

namespace AppBase.ThirdParty.Max.Editor
{
    public class MaxSettingsTools : IPreprocessBuildWithReport
    {
        public int callbackOrder => 0;
        public void OnPreprocessBuild(BuildReport report)
        {
            const string configPath = "Assets/Project/AddressableRes/Configs/m_Data_dl/GlobalConfig.asset";
            var config = AssetDatabase.LoadAssetAtPath<GlobalConfigList>(configPath).map;
            if (config == null) throw new Exception("GlobalConfig not found");
            Debug.Log("OnPreprocessBuild: AdMob Android AppID: " + config[GlobalConfigKeys.AdMobAndroidAppId].Value);
            Debug.Log("OnPreprocessBuild: AdMob IOS AppID: " + config[GlobalConfigKeys.AdMobIosAppId].Value);
            AppLovinSettings.Instance.AdMobAndroidAppId = config[GlobalConfigKeys.AdMobAndroidAppId].Value;
            AppLovinSettings.Instance.AdMobIosAppId = config[GlobalConfigKeys.AdMobIosAppId].Value;
        }
    }
}