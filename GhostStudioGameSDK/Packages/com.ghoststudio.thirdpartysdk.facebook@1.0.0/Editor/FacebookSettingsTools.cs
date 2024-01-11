using System;
using System.Collections.Generic;
using Facebook.Unity.Editor;
using Facebook.Unity.Settings;
using UnityEditor;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;
using UnityEngine;

namespace AppBase.Analytics.Editor
{
    public class FacebookSettingsTools : IPreprocessBuildWithReport
    {
        public int callbackOrder => 0;
        public void OnPreprocessBuild(BuildReport report)
        {
            const string configPath = "Assets/Project/AddressableRes/Configs/m_Data_dl/GlobalConfig.asset";
            var config = AssetDatabase.LoadAssetAtPath<GlobalConfigList>(configPath).map;
            if (config == null) throw new Exception("GlobalConfig not found");
            Debug.Log("OnPreprocessBuild: Facebook ID: " + config[GlobalConfigKeys.FacebookId].Value);
            FacebookSettings.AppLabels = new List<string> { config[GlobalConfigKeys.AppDisplayName].Value };
            FacebookSettings.AppIds = new List<string> { config[GlobalConfigKeys.FacebookId].Value };
            FacebookSettings.ClientTokens = new List<string> { config[GlobalConfigKeys.FacebookToken].Value };
            ManifestMod.GenerateManifest();
        }
    }
}