using System;
using System.IO;
using UnityEditor;
using UnityEditor.Build.Reporting;
using UnityEngine;

public static partial class BuildNative
{
    [MenuItem("Tools/Build Tools/Build IOS Debug Ipa")]
    public static void BuildIOSDebug()
    {
        BuildIOSNative(true);
        if (!Application.isBatchMode)
        {
            EditorUtility.DisplayDialog("Build IOS Debug Ipa", "Success", "OK");
        }
    }

    [MenuItem("Tools/Build Tools/Build IOS Release Ipa")]
    public static void BuildIOSRelease()
    {
        BuildIOSNative(false);
        if (!Application.isBatchMode)
        {
            EditorUtility.DisplayDialog("Build IOS Release Ipa", "Success", "OK");
        }
    }

    /// <summary>
    /// 打包IOS
    /// </summary>
    public static void BuildIOSNative(bool isDebug)
    {
        Application.SetStackTraceLogType(LogType.Log, StackTraceLogType.None);
        Application.SetStackTraceLogType(LogType.Warning, StackTraceLogType.None);
        SetDebugSymbols(isDebug);
        PlayerSettings.SplashScreen.show = false;
        EditorUserBuildSettings.androidCreateSymbols = isDebug ? AndroidCreateSymbols.Debugging : AndroidCreateSymbols.Public;
        EditorUserBuildSettings.buildAppBundle = !isDebug;
        var dir = Path.Combine(Path.GetDirectoryName(Environment.CurrentDirectory)!, "Build");
        if (Directory.Exists(dir)) Directory.Delete(dir, true);
        Directory.CreateDirectory(dir);
        var path = Path.Combine(dir, "XcodeProject");
        OnBeforeBuildNative?.Invoke();
        var scenes = GetScenePathes();
        var report = BuildPipeline.BuildPlayer(scenes, path, BuildTarget.iOS, GetBuildOptions());
        if (report.summary.result == BuildResult.Failed)
        {
            PrintBuildError(report);
            EditorApplication.Exit(1);
            return;
        }
        OnAfterBuildNative?.Invoke();
    }
}