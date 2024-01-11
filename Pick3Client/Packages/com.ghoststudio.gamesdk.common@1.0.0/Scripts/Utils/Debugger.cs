using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// 日志打印工具，根据Tag过滤日志
/// </summary>
public static class Debugger
{
    /// <summary>
    /// 是否允许打印普通日志
    /// </summary>
    public static bool IsLogEnabled
    {
        get => isLogEnabled;
        set
        {
            isLogEnabled = value;
            PlayerPrefs.SetInt(logEnableKey, isLogEnabled ? 1 : 0);
            PlayerPrefs.Save();
        }
    }

    private static bool isLogEnabled;

    /// <summary>
    /// 允许打印的普通日志Tag
    /// </summary>
    private static HashSet<string> logEnableTags;

    /// <summary>
    /// 默认的普通日志Tag
    /// </summary>
    public const string logTagDefault = "default";

    private const string logTagsKey = "EnableLogTags";
    private const string logEnableKey = "IsEnableLog";

    static Debugger()
    {
        ReadDebugConfig();
    }

    /// <summary>
    /// 设置允许打印的普通日志Tag
    /// </summary>
    public static void SetLogEnable(string logTag, bool isEnable = true)
    {
        var changed = isEnable ? logEnableTags.Add(logTag) : logEnableTags.Remove(logTag);
        if (changed) WriteDebugConfig();
    }

    public static bool GetLogEnable(string logTag)
    {
        return logEnableTags.Contains("all") || logEnableTags.Contains(logTag);
    }

    public static void SetLogEnable(string[] logTags, bool isEnable)
    {
        if (isEnable)
        {
            logEnableTags.UnionWith(logTags);
        }
        else
        {
            logEnableTags.ExceptWith(logTags);
        }
        WriteDebugConfig();
    }

    public static void LogD(object message)
    {
        Log(logTagDefault, message);
    }

    public static void LogD(object message, Object context)
    {
        Log(logTagDefault, message, context);
    }

    public static void LogDFormat(string format, params object[] args)
    {
        LogFormat(logTagDefault, format, args);
    }

    public static void LogDFormat(Object context, string format, params object[] args)
    {
        LogFormat(logTagDefault, context, format, args);
    }

    public static void LogDError(object message)
    {
        LogError(logTagDefault, message);
    }

    public static void LogDError(object message, Object context)
    {
        LogError(logTagDefault, message, context);
    }

    public static void LogDErrorFormat(string format, params object[] args)
    {
        LogErrorFormat(logTagDefault, format, args);
    }

    public static void LogDErrorFormat(Object context, string format, params object[] args)
    {
        LogErrorFormat(logTagDefault, context, format, args);
    }

    public static void LogDWarning(object message)
    {
        LogWarning(logTagDefault, message);
    }

    public static void LogDWarning(object message, Object context)
    {
        LogWarning(logTagDefault, message, context);
    }

    public static void LogDWarningFormat(string format, params object[] args)
    {
        LogWarningFormat(logTagDefault, format, args);
    }

    public static void LogDWarningFormat(Object context, string format, params object[] args)
    {
        LogWarningFormat(logTagDefault, context, format, args);
    }

    public static void Log(string logTag, object message)
    {
        Log(logTag, message, (Object)null);
    }

    public static void Log(string logTag, object message, Object context)
    {
        if (!isLogEnabled) return;
        if (!GetLogEnable(logTag)) return;
        Debug.Log((object)string.Format("[{0}]: {1}", (object)logTag, message), context);
    }

    public static void LogFormat(string logTag, string format, params object[] args)
    {
        Log(logTag, (object)string.Format(format, args));
    }

    public static void LogFormat(
        string logTag,
        Object context,
        string format,
        params object[] args)
    {
        Log(logTag, (object)string.Format(format, args), context);
    }

    public static void LogError(string logTag, object message)
    {
        LogError(logTag, message, (Object)null);
    }

    public static void LogError(string logTag, object message, Object context)
    {
        Debug.LogError((object)string.Format("[{0}]: {1}", (object)logTag, message), context);
    }

    public static void LogErrorFormat(string logTag, string format, params object[] args)
    {
        LogError(logTag, (object)string.Format(format, args));
    }

    public static void LogErrorFormat(
        string logTag,
        Object context,
        string format,
        params object[] args)
    {
        LogError(logTag, (object)string.Format(format, args), context);
    }

    public static void LogWarning(string logTag, object message)
    {
        LogWarning(logTag, message, (Object)null);
    }

    public static void LogWarning(string logTag, object message, Object context)
    {
        Debug.LogWarning((object)string.Format("[{0}]: {1}", (object)logTag, message), context);
    }

    public static void LogWarningFormat(string logTag, string format, params object[] args)
    {
        LogWarning(logTag, (object)string.Format(format, args));
    }

    public static void LogWarningFormat(
        string logTag,
        Object context,
        string format,
        params object[] args)
    {
        LogWarning(logTag, (object)string.Format(format, args), context);
    }

    private static void ReadDebugConfig()
    {
        var config = PlayerPrefs.GetString(logTagsKey);
        logEnableTags = !string.IsNullOrEmpty(config) ? new HashSet<string>(config.Split(',')) : new HashSet<string>();
#if DEBUG
        isLogEnabled = PlayerPrefs.GetInt(logEnableKey, 1) == 1;
#else
        isLogEnabled = PlayerPrefs.GetInt(logEnableKey, 0) == 1;
#endif
    }

    private static void WriteDebugConfig()
    {
        PlayerPrefs.SetString(logTagsKey, string.Join(",", logEnableTags));
        PlayerPrefs.Save();
    }
}