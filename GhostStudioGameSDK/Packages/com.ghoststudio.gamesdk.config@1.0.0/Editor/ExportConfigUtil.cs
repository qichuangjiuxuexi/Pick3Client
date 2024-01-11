using System;
using System.IO;
using System.Linq;
using System.Text;
using AppBase.Utils;
using Newtonsoft.Json;
using UnityEditor;
using UnityEngine;

public static class ExportConfigUtil
{
    [MenuItem("Tools/Config/Export Configs")]
    public static void ExportConfigs()
    {
        var dirPath = Path.Combine(Path.GetDirectoryName(Environment.CurrentDirectory)!, "Exports", "Configs");
        if (Directory.Exists(dirPath)) Directory.Delete(dirPath, true);
        Directory.CreateDirectory(dirPath);
        var utf8 = new UTF8Encoding(false);
        var configs = AssetDatabase.FindAssets("t:scriptableobject", new[] { UpdateConfigUtil.configAssetPath })
            .Select(AssetDatabase.GUIDToAssetPath)
            .Select(AssetDatabase.LoadAssetAtPath<ScriptableObject>);
        JsonSerializerSettings settings = new()
        {
            TypeNameHandling = TypeNameHandling.Auto,
            Formatting = Formatting.Indented
        };
        foreach (var config in configs)
        {
            var json = JsonConvert.SerializeObject(config, typeof(ScriptableObject), settings);
            var filePath = Path.Combine(dirPath, $"{config.name}.json");
            File.WriteAllText(filePath, json, utf8);
            Debug.Log($"ExportConfigs: {filePath}");
        }
        if (!Application.isBatchMode)
        {
            EditorUtility.RevealInFinder(dirPath);
        }
    }
}