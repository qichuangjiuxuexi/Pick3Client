using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using UnityEditor;
using UnityEditor.AddressableAssets;
using UnityEditor.AddressableAssets.Settings;
using UnityEditor.AddressableAssets.Settings.GroupSchemas;
using UnityEngine;

public static class AddressableUtil
{
    public const string AddressableRootGroupPath = "Assets/Project/AddressableRes";
    public const string KeyAddressScriptPath = "Assets/Project/AddressableRes/Common/Scripts/Definition/AAConst.cs";
   
    public const string SinglePackPrefix = "s_";
    public const string MultiPackPrefix = "m_";
    public const string IgnoreBuildTag = "__i";
    public static readonly Regex DynamicLoadSuffix = new (@"_dl\b");
    public static readonly Regex RemoteLoadSuffix = new (@"_r\b");

    public const string TemplateAAConst = @"using System.Collections.Generic;
public class AAConst
{{
{0}
}}
";

    public static readonly HashSet<string> IgnoreExtentions = new ()
    {
        ".meta",
        ".cs",
        ".dll",
        ".DS_Store",
        ".keep",
    };

    [MenuItem("Tools/Addressable/Build Addressable Contents")]
    public static void BuildAddressableContents()
    {
        if (Application.isBatchMode)
        {
            Application.SetStackTraceLogType(LogType.Log, StackTraceLogType.None);
            Application.SetStackTraceLogType(LogType.Warning, StackTraceLogType.None);
        }
        try
        {
            if (!Application.isBatchMode) EditorUtility.DisplayProgressBar("AddressableUtil", "CheckAddressableSettings", 0);
            CheckAddressableSettings();
            if (!Application.isBatchMode) EditorUtility.DisplayProgressBar("AddressableUtil", "RemoveAllEntities", 0);
            RemoveAllEntities();
            if (!Application.isBatchMode) EditorUtility.DisplayProgressBar("AddressableUtil", "ReGroupAllEntities", 0);
            ReGroupAllEntities();
            if (!Application.isBatchMode) EditorUtility.DisplayProgressBar("AddressableUtil", "RemoveAllAtlasTextures", 0);
            SpriteAtlasUtil.RemoveAllAtlasTextures();
            if (!Application.isBatchMode) EditorUtility.DisplayProgressBar("AddressableUtil", "RemoveAllEmptyGroups", 1);
            RemoveAllEmptyGroups();
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            if (!Application.isBatchMode)
            {
                EditorUtility.ClearProgressBar();
                EditorUtility.DisplayDialog("更新Addressable配置完成", "", "OK");
            }
        }
        catch (Exception e)
        {
            if (!Application.isBatchMode)
            {
                Debug.LogError(e);
                EditorUtility.ClearProgressBar();
                EditorUtility.DisplayDialog("更新Addressable配置失败", e.Message, "OK");
            }
            throw;
        }
    }

    private static void CheckAddressableSettings()
    {
        var path = "Assets/AddressableAssetsData/AddressableAssetSettings.asset";
        var settings = AssetDatabase.LoadAssetAtPath<AddressableAssetSettings>(path);
        if (settings == null)
        {
            throw new Exception("Addressable settings not found: " + path);
        }
        AddressableAssetSettingsDefaultObject.Settings = settings;
    }

    /// <summary>
    /// 清空Groups内的所有Entities，但不要删掉Groups
    /// </summary>
    private static void RemoveAllEntities()
    {
        var settings = AddressableAssetSettingsDefaultObject.Settings;
        settings.groups.ToList().ForEach(group => RemoveEntitiesInGroup(group, settings));
        EditorUtility.SetDirty(settings);
    }

    /// <summary>
    /// 删除所有的空Groups，DefaultLocalGroup除外
    /// </summary>
    private static void RemoveAllEmptyGroups()
    {
        var settings = AddressableAssetSettingsDefaultObject.Settings;
        var groups = new List<AddressableAssetGroup>(settings.groups);
        for (int i = 0; i < groups.Count; i++)
        {
            var group = groups[i];
            if (group != null && group.name == "Built In Data") continue;
            if (group != null && group.Default) continue;
            try
            {
                if (group != null && group.entries != null && group.entries.Count > 0) continue;
                settings.RemoveGroup(group);
            }
            catch
            {
                settings.groups.Remove(group);
            }
            settings.SetDirty(AddressableAssetSettings.ModificationEvent.GroupRemoved, null, true, true);
        }
        EditorUtility.SetDirty(settings);
    }

    private static void RemoveEntitiesInGroup(AddressableAssetGroup group, AddressableAssetSettings settings)
    {
        if (group == null) return;
        if (group?.name == "Built In Data") return;
        try
        {
            var entries = new List<AddressableAssetEntry>(group.entries);
            entries.ForEach(x =>
            {
                try
                {
                    group.RemoveAssetEntry(x);
                }
                catch
                {
                    group.entries.Remove(x);
                }
            });
            settings.SetDirty(AddressableAssetSettings.ModificationEvent.EntryRemoved, entries, true, true);
        }
        catch
        {
            try
            {
                settings.RemoveGroup(group);
            }
            catch
            {
                settings.groups.Remove(group);
            }
            settings.SetDirty(AddressableAssetSettings.ModificationEvent.GroupRemoved, null, true, true);
        }
    }

    private static void ReGroupAllEntities()
    {
        var keyAddressMap = new Dictionary<string, string>();
        var uiKeys = new List<string>();
        var settings = AddressableAssetSettingsDefaultObject.Settings;
        string rootDir = Path.Combine(Environment.CurrentDirectory, AddressableRootGroupPath);
        string[] directories = Directory.GetDirectories(rootDir, "?_*", SearchOption.AllDirectories);
        Array.Sort(directories);
        HashSet<string> tempSet = new HashSet<string>();
        for (int i = 0; i < directories.Length; i++)
        {
            string dirPath = directories[i];
            string dirName = Path.GetFileName(dirPath);
            if (!dirName.StartsWith(SinglePackPrefix) && !dirName.StartsWith(MultiPackPrefix) || dirName.Contains(IgnoreBuildTag))
            {
                continue;
            }
           
            if (!Application.isBatchMode)
            {
                float progress = i / ((float)directories.Length - 1);
                EditorUtility.DisplayProgressBar("分组中", $"正在分组{dirPath}，请稍后……", progress);
            }

            bool isSingle = dirName.StartsWith(SinglePackPrefix);
            bool isDynamic = DynamicLoadSuffix.IsMatch(dirName);
            bool isRemote = RemoteLoadSuffix.IsMatch(dirName);
            if (!tempSet.Add(dirName.ToLower()))
            {
                throw new Exception("repeat addressable asset group:"+ dirPath);
            }
            var group = isRemote ? CreateRemoteGroup(settings, dirName, isSingle) : CreateLocalGroup(settings, dirName, isSingle);

            string[] files = Directory.GetFiles(dirPath, "*", SearchOption.AllDirectories);
            Array.Sort(files);
            for (int j = 0; j < files.Length; j++)
            {
                if (files[j].Contains(IgnoreBuildTag)) continue;
                var fileExt = Path.GetExtension(files[j]);
                if (string.IsNullOrEmpty(fileExt) || IgnoreExtentions.Contains(fileExt)) continue;
                string assetPath = files[j].Substring(Environment.CurrentDirectory.Length + 1);
                
                //所属分组的文件夹下的祖先文件夹中有任何一个以_dl结尾，也算dynamic
                bool fileWithDynamic = isDynamic || IsAncestorDirDynamic(files[j],dirPath.Length);

                var guid = AssetDatabase.AssetPathToGUID(assetPath);
                string address = $"{Directory.GetParent(files[j]).Name}.{Path.GetFileName(files[j])}";
                AddressableAssetEntry entry = group.GetAssetEntry(guid);
                entry ??= settings.CreateOrMoveEntry(guid, group, false, false);

                if (entry == null)
                {
                    Debug.LogError(assetPath);
                    continue;
                }

                entry.SetAddress(address);
                if (fileWithDynamic)
                {
                    string key = Path.GetFileNameWithoutExtension(files[j]);
                    keyAddressMap[key] = address;
                    uiKeys.Add(key);
                }
            }
        }
        EditorUtility.SetDirty(settings);
        SaveToAAConst(keyAddressMap, uiKeys);
        if (!Application.isBatchMode)
        {
            EditorUtility.DisplayProgressBar("分组中", "保存地址映射关系……", 1);
            EditorUtility.ClearProgressBar();
        }
    }

    private static void SaveToAAConst(Dictionary<string, string> keyAddressMap, List<string> uiKeys)
    {
        uiKeys = uiKeys.Distinct().OrderBy(x => x).ToList();
        string destPath = Path.Combine(Environment.CurrentDirectory, KeyAddressScriptPath);
        StringBuilder builder = new StringBuilder();
        foreach (var item in uiKeys)
        {
            builder.Append("\t");
            builder.AppendFormat("public static string {0} = \"{1}\";\n", ParseKey(item), keyAddressMap[item]);
        }

        builder.Append("\n\t");
        builder.Append("public static Dictionary<string,string> keyAddressDict = new Dictionary<string,string>()\n");
        builder.Append("\t{\n");

        for (int i = 0; i < uiKeys.Count; i++)
        {
            builder.Append("\t\t{");
            builder.AppendFormat("\"{0}\" , {1}", uiKeys[i], ParseKey(uiKeys[i]));
            builder.Append("},\n");
        }
        builder.Append("\t};\n");

        builder.Append("\n\t");
        builder.Append("public static string GetAddress(string key)\n");
        builder.Append("\t{\n");
        builder.Append("\t\tif(keyAddressDict.TryGetValue(key, out var address))\n\t\t{\n\t\t\treturn address;\n\t\t}\n");
        builder.Append("\t\treturn \"\";\n");
        builder.Append("\t}\n");
        string finalContent = string.Format(TemplateAAConst, builder);
        File.WriteAllText(destPath, finalContent);
    }
    
    static bool IsAncestorDirDynamic(string file, int length)
    {
        var parent = Directory.GetParent(file);
        while (parent != null)
        {
            if (parent.FullName.Length >= length)
            {
                if (DynamicLoadSuffix.IsMatch(parent.Name)) 
                {
                    return true;
                }
            }
            else
            {
                return false;
            }

            parent = parent.Parent;
        }

        return false;
    }

    /// <summary>
    /// 解析属性名
    /// </summary>
    private static string ParseKey(string str)
    {
        str = str.Trim().Replace(" ", "_").Replace(".", "_").Replace("-", "_");
        if (str.Length > 0 && char.IsDigit(str[0])) str = '_' + str;
        return str;
    }

    public static AddressableAssetGroup CreateLocalGroup(AddressableAssetSettings settings, string groupName, bool isSingle)
    {
        var group = settings.FindGroup(groupName) ?? settings.CreateGroup(groupName, false, false, false, 
            new List<AddressableAssetGroupSchema>(), typeof(BundledAssetGroupSchema), typeof(ContentUpdateGroupSchema));
        var schema = group.GetSchema<BundledAssetGroupSchema>() ?? group.AddSchema<BundledAssetGroupSchema>();
        schema.BundleMode = isSingle ? BundledAssetGroupSchema.BundlePackingMode.PackSeparately : BundledAssetGroupSchema.BundlePackingMode.PackTogether;
        schema.InternalIdNamingMode = BundledAssetGroupSchema.AssetNamingMode.GUID;
        
        //local settings:
        schema.BuildPath.SetVariableByName(settings, AddressableAssetSettings.kLocalBuildPath);
        schema.LoadPath.SetVariableByName(settings, AddressableAssetSettings.kLocalLoadPath);
        schema.BundleNaming = BundledAssetGroupSchema.BundleNamingStyle.NoHash;
        schema.AssetBundledCacheClearBehavior = BundledAssetGroupSchema.CacheClearBehavior.ClearWhenSpaceIsNeededInCache;
        schema.UseAssetBundleCrc = false;
        schema.UseAssetBundleCrcForCachedBundles = false;
        schema.IncludeInBuild = true;
        
        var updateSchema = group.GetSchema<ContentUpdateGroupSchema>() ?? group.AddSchema<ContentUpdateGroupSchema>();
        updateSchema.StaticContent = true;
        return group;
    }

    public static AddressableAssetGroup CreateRemoteGroup(AddressableAssetSettings settings, string groupName, bool isSingle)
    {
        var group = settings.FindGroup(groupName) ?? settings.CreateGroup(groupName, false, false, false, 
            new List<AddressableAssetGroupSchema>(), typeof(BundledAssetGroupSchema), typeof(ContentUpdateGroupSchema));
        var schema = group.GetSchema<BundledAssetGroupSchema>() ?? group.AddSchema<BundledAssetGroupSchema>();
        schema.BundleMode = isSingle ? BundledAssetGroupSchema.BundlePackingMode.PackSeparately : BundledAssetGroupSchema.BundlePackingMode.PackTogether;
        schema.InternalIdNamingMode = BundledAssetGroupSchema.AssetNamingMode.GUID;
        
        //remote settings:
        schema.BuildPath.SetVariableByName(settings, AddressableAssetSettings.kRemoteBuildPath);
        schema.LoadPath.SetVariableByName(settings, AddressableAssetSettings.kRemoteLoadPath);
        schema.BundleNaming = BundledAssetGroupSchema.BundleNamingStyle.AppendHash;
        schema.AssetBundledCacheClearBehavior = BundledAssetGroupSchema.CacheClearBehavior.ClearWhenWhenNewVersionLoaded;
        schema.UseAssetBundleCrc = true;
        schema.UseAssetBundleCrcForCachedBundles = true;
        schema.Compression = BundledAssetGroupSchema.BundleCompressionMode.LZMA;
        schema.Timeout = 20;
        schema.RetryCount = 3;
        schema.IncludeInBuild = false;
        
        var updateSchema = group.GetSchema<ContentUpdateGroupSchema>() ?? group.AddSchema<ContentUpdateGroupSchema>();
        updateSchema.StaticContent = false;
        return group;
    }
}