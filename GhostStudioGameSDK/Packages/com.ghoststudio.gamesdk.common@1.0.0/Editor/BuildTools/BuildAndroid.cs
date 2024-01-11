using System;
using System.IO;
using System.Linq;
using System.Text;
using UnityEditor;
using UnityEditor.Build.Reporting;
using UnityEngine;

public static partial class BuildNative
{
    public static event Action OnBeforeBuildNative;
    public static event Action OnAfterBuildNative;
    
    [MenuItem("Tools/Build Tools/Build Android Debug Apk")]
    public static void BuildAndroidDebug()
    {
        BuildAndroidNative(true, true);
        if (!Application.isBatchMode)
        {
            EditorUtility.DisplayDialog("Build Android Debug Apk", "Success", "OK");
        }
    }

    [MenuItem("Tools/Build Tools/Build Android Release Aab")]
    public static void BuildAndroidReleaseAab()
    {
        BuildAndroidNative(false, false);
        if (!Application.isBatchMode)
        {
            EditorUtility.DisplayDialog("Build Android Release Aab", "Success", "OK");
        }
    }

    [MenuItem("Tools/Build Tools/Build Android Release Apk")]
    public static void BuildAndroidReleaseApk()
    {
        BuildAndroidNative(false, true);
        if (!Application.isBatchMode)
        {
            EditorUtility.DisplayDialog("Build Android Release Aab", "Success", "OK");
        }
    }

    /// <summary>
    /// 打包Android
    /// </summary>
    public static void BuildAndroidNative(bool isDebug, bool isApk)
    {
        Application.SetStackTraceLogType(LogType.Log, StackTraceLogType.None);
        Application.SetStackTraceLogType(LogType.Warning, StackTraceLogType.None);
        ModifyAndroidManifestDebuggable(isDebug);
        SetDebugSymbols(isDebug);
        PlayerSettings.Android.useCustomKeystore = true;
        PlayerSettings.Android.keystoreName = "../Cer/keystore.keystore";
        PlayerSettings.Android.keystorePass = "me2zen";
        PlayerSettings.Android.keyaliasName = "me2zen";
        PlayerSettings.Android.keyaliasPass = "me2zen";
        PlayerSettings.SplashScreen.show = false;
        EditorUserBuildSettings.androidCreateSymbols = isDebug ? AndroidCreateSymbols.Debugging : AndroidCreateSymbols.Public;
        EditorUserBuildSettings.buildAppBundle = !isApk;
        var timestamp = DateTime.Now.ToString("yyyyMMddHHmmss");
        var debugStr = (isDebug ? "Debug" : "Release") + (isApk ? ".apk" : ".aab");
        var productName = PlayerSettings.productName.Replace(" ", "");
        var fileName = $"{productName}_v{PlayerSettings.bundleVersion}_{PlayerSettings.Android.bundleVersionCode}_{timestamp}_{debugStr}";
        var dir = Path.Combine(Path.GetDirectoryName(Environment.CurrentDirectory)!, "Build");
        if (Directory.Exists(dir)) Directory.Delete(dir, true);
        Directory.CreateDirectory(dir);
        var path = Path.Combine(dir, fileName);
        OnBeforeBuildNative?.Invoke();
        var scenes = GetScenePathes();
        var report = BuildPipeline.BuildPlayer(scenes, path, BuildTarget.Android, GetBuildOptions());
        if (report.summary.result == BuildResult.Failed)
        {
            PrintBuildError(report);
            EditorApplication.Exit(1);
            return;
        }
        OnAfterBuildNative?.Invoke();
    }
    
    /// <summary>
    /// 固定AOT启动场景
    /// </summary>
    public static string[] GetScenePathes()
    {
        return new[] { "Assets/Project/AOTRes/Scenes/Main.unity" };
    }
    
    /// <summary>
    /// 修改Android包Debug属性
    /// </summary>
    private static void ModifyAndroidManifestDebuggable(bool isDebug)
    {
        var relPath = "Plugins/Android/AndroidManifest.xml";
        var absPath = Path.Combine(Application.dataPath, relPath);
        if (!File.Exists(absPath)) return;
        var oldText = File.ReadAllText(absPath, Encoding.UTF8);
        var oldDebug = isDebug ? "false" : "true";
        var newDebug = isDebug ? "true" : "false";
        var newText = oldText.Replace($"android:debuggable=\"{oldDebug}\"", $"android:debuggable=\"{newDebug}\"");
        if (newText == oldText) return;
        File.WriteAllText(absPath, newText, Encoding.UTF8);
        AssetDatabase.ImportAsset("Assets/" + relPath, ImportAssetOptions.ForceSynchronousImport);
    }

    /// <summary>
    /// 设置DEBUG符号
    /// </summary>
    public static void SetDebugSymbols(bool isDebug)
    {
        var addSymbol = isDebug ? "DEBUG" : "RELEASE";
        var removeSymbol = isDebug ? "RELEASE" : "DEBUG";
        var target = BuildPipeline.GetBuildTargetGroup(EditorUserBuildSettings.activeBuildTarget);
        var symbols = PlayerSettings.GetScriptingDefineSymbolsForGroup(target);
        var list = symbols.Split(';').Select(p => p.Trim()).ToList();
        if (!string.IsNullOrEmpty(addSymbol) && !list.Contains(addSymbol))
        {
            list.Add(addSymbol);
        }
        if (!string.IsNullOrEmpty(removeSymbol))
        {
            list.Remove(removeSymbol);
        }
        var newSymbols = string.Join(";", list);
        if (symbols != newSymbols)
        {
            PlayerSettings.SetScriptingDefineSymbolsForGroup(target, newSymbols);
        }
    }
    
    /// <summary>
    /// 打印错误报告
    /// </summary>
    public static void PrintBuildError(BuildReport report)
    {
        Application.SetStackTraceLogType(LogType.Log, StackTraceLogType.None);
        Application.SetStackTraceLogType(LogType.Error, StackTraceLogType.None);
        Debug.Log("");
        Debug.Log("-------------------------打包错误-------------------------");
        Debug.Log("");
        foreach (var step in report.steps)
        {
            foreach (var message in step.messages)
            {
                if (message.type == LogType.Error)
                {
                    Debug.Log($"------------------------- 错误step: {step.name} -------------------------");
                    Debug.LogError(message.content);
                    Debug.Log("");
                }
            }
        }
        Application.SetStackTraceLogType(LogType.Error, StackTraceLogType.Full);
    }

    /// <summary>
    /// 生成BuildOptions
    /// </summary>
    public static BuildOptions GetBuildOptions()
    {
        var options = BuildOptions.CompressWithLz4HC;
        Environment.GetEnvironmentVariable("buildOptions")?.Split('|').ToList().ForEach(p =>
        {
            p = p.Trim();
            if (string.IsNullOrEmpty(p)) return;
            if (Enum.TryParse(p, out BuildOptions option))
            {
                Debug.Log($"BuildNative: Add BuildOption: {option}");
                options |= option;
            }
            else
            {
                Debug.LogError($"BuildNative: Unknown BuildOption: {p}");
            }
        });
        return options;
    }
}
