using System;
using System.Collections.Generic;
using System.IO;
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
            
            const string packagesFolderPath = "../GhostStudioGameSDK/Packages/com.ghoststudio.thirdpartysdk.facebook@1.0.0/Plugins/FacebookSDK";
            const string assetsFolderPath = "Assets/FacebookSDK";
            const string linkFile = "link.xml";
            
            Debug.Log("Facebook copy link file");
            
            if (AssetDatabase.IsValidFolder(assetsFolderPath) &&
                File.Exists(Path.Combine(assetsFolderPath, linkFile)))
            {
                Debug.Log("FB link file exists in Assets folder.");
                return;
            }
            
            if (!File.Exists(Path.Combine(packagesFolderPath, linkFile)))
            {
                Debug.Log("FB link file exists in Packages folder.");
                StopBuildWithMessage("FB packages folder not found.");
                return;
            }

            if (!Directory.Exists(assetsFolderPath))
            {
                Directory.CreateDirectory(assetsFolderPath);
            }
            
            File.Copy(Path.Combine(packagesFolderPath, linkFile),
                Path.Combine(assetsFolderPath, linkFile));
            
            Debug.Log("Copied FB link file from Packages to Assets folder.");
        }
    
        private void StopBuildWithMessage(string message)
        {
            var prefix = "[FB] ";
            throw new BuildPlayerWindow.BuildMethodException(prefix + message);
        }
    }
}