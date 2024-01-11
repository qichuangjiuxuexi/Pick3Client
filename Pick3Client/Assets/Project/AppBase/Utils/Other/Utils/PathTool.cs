using System.Text;
using UnityEngine;

public class PathTool
{
#if UNITY_WEBGL
    /// <summary>
    /// 获取加载URL
    /// </summary>
    /// <param name="relativelyPath">相对路径</param>
    /// <returns></returns>
    public static string GetLoadURL(string relativelyPath)
    {
#if UNITY_EDITOR
        return "file://" + Application.streamingAssetsPath + "/" + relativelyPath;
#else
        return Application.absoluteURL + "StreamingAssets/" + relativelyPath;
#endif
    }
#endif

    //获取相对路径
    public static string GetRelativelyPath(string path, string fileName, string expandName)
    {
        StringBuilder builder = new StringBuilder();
        builder.Append(path);
        builder.Append("/");
        builder.Append(fileName);
        builder.Append(".");
        builder.Append(expandName);

        return builder.ToString();
    }

    /// <summary>
    /// 获取某个目录下的相对路径
    /// </summary>
    /// <param name="FullPath">完整路径</param>
    /// <param name="DirectoryPath">目标目录</param>
    public static string GetDirectoryRelativePath(string DirectoryPath, string FullPath)
    {
        DirectoryPath = DirectoryPath.Replace(@"\", "/");
        FullPath = FullPath.Replace(@"\", "/");

        FullPath = FullPath.Replace(DirectoryPath, "");

        return FullPath;
    }
    
    /// <summary>
    /// 获取编辑器下的路径
    /// </summary>
    /// <param name="directoryName">目录名</param>
    /// <param name="fileName">文件名</param>
    /// <param name="expandName">拓展名</param>
    /// <returns></returns>
    public static string GetEditorPath(string directoryName, string fileName, string expandName)
    {
        StringBuilder builder = new StringBuilder();
        builder.Append(Application.dataPath);
        builder.Append("/Editor");
        builder.Append(directoryName);
        builder.Append("/");
        builder.Append(fileName);
        builder.Append(".");
        builder.Append(expandName);

        return builder.ToString();
    }
    
}
