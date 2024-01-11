using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditor.AddressableAssets;
using UnityEditor.AddressableAssets.Settings;
using UnityEditor.U2D;
using UnityEngine.U2D;

public class SpriteAtlasUtil
{
    /// <summary>
    /// 在Groups中移除所有Atlas的Texture
    /// </summary>
    public static void RemoveAllAtlasTextures()
    {
        var settings = AddressableAssetSettingsDefaultObject.Settings;
        var entries = settings.groups
            .SelectMany(ScanAtlasTextures)
            .Distinct()
            .Select(AssetDatabase.AssetPathToGUID)
            .Select(settings.FindAssetEntry)
            .ToList();
        entries.ForEach(e => settings.RemoveAssetEntry(e.guid));
        settings.SetDirty(AddressableAssetSettings.ModificationEvent.EntryRemoved, entries, true, true);
        EditorUtility.SetDirty(settings);
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
    }

    private static List<string> ScanAtlasTextures(AddressableAssetGroup group)
    {
        if (group == null || group.entries == null) return new();
        return group.entries
            .Where(e => e.AssetPath.EndsWith(".spriteatlas"))
            .Select(e => e.AssetPath)
            .SelectMany(GetAtlasTextures)
            .ToList();
    }
    
    public static List<string> GetAtlasTextures(string atlasPath)
    {
        return AssetDatabase.LoadAssetAtPath<SpriteAtlas>(atlasPath)
            .GetPackables()
            .Select(AssetDatabase.GetAssetPath)
            .SelectMany(p =>
                AssetDatabase.IsValidFolder(p) ?
                    AssetDatabase.FindAssets("t:texture2d", new[] { p }).Select(AssetDatabase.GUIDToAssetPath) :
                    new[] { p }
            )
            .ToList();
    }
}