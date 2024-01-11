using System;
using System.Collections.Generic;
using TMPro;
using TMPro.EditorUtilities;
using UnityEditor;
using UnityEngine;

/// <summary>
/// 字体绑定脚本
/// </summary>
[CustomEditor(typeof(LocalizationTextBinder))]
public class TextMeshBindEditor : Editor
{
    private SerializedProperty useBindKey;
    private SerializedProperty key;
    private bool oldBindKey;
    private string oldKey = "";
    private static SystemLanguage oldLanguage = SystemLanguage.English;
    
    private static Dictionary<SystemLanguage, TextConfigList> dataMap;
    private static Dictionary<SystemLanguage, TMP_FontAsset> fontMap;

    private static string fontPath = "Assets/Project/AddressableRes/m_Font/TextMeshPro/";
    private static string fontName = "SourceHanSansHW_Bold_SDF_";


    private void OnEnable()
    {
        key = serializedObject.FindProperty("key");
        useBindKey = serializedObject.FindProperty("useBindKey");
        string str = PlayerPrefs.GetString("Editor_TextMeshProFontPath");
        if (!string.IsNullOrEmpty(str))
            fontPath = str;
        str = PlayerPrefs.GetString("Editor_TextMeshProFontName");
        if (!string.IsNullOrEmpty(str))
            fontName = str;
    }

    public static void ResetLanguage(LocalizationTextBinder binder)
    {
        binder.curLanguage = SystemLanguage.English;
        if (binder.useBindKey)
        {
            var find  = dataMap.TryGetValue(binder.curLanguage, out TextConfigList config);
            if (find)
            {
                if(config.map.ContainsKey(binder.key))
                {
                    binder.SetText(config.map[binder.key].Value);
                }
                else
                {
                    binder.SetText($"{binder.key} not found!");
                }
                EditorUtility.SetDirty(binder);
            }
        }
    }

    /// <summary>
    /// 监视板初始化
    /// </summary>
    public override void OnInspectorGUI()
    {
        serializedObject.Update();
        if (target != null)
        {
            var temp = target as LocalizationTextBinder;
            if (temp == null) return;
            dataMap ??= new Dictionary<SystemLanguage, TextConfigList>();
            fontMap ??= new();
            oldKey = temp.key;
            oldLanguage = temp.curLanguage;
            oldBindKey = temp.useBindKey;
            temp.useBindKey = EditorGUILayout.Toggle("使用绑定Key", temp.useBindKey);
            temp.key = EditorGUILayout.TextField("Key", temp.key);
            temp.curLanguage = (SystemLanguage) EditorGUILayout.EnumPopup("当前语种：", temp.curLanguage);
            
            EditorGUILayout.LabelField("加载中日韩字体的父文件夹(fontPath/Japanese_dl/{fontName}");
            string fp = EditorGUILayout.TextField("中日韩字体父文件夹:", fontPath);
            if (!fp.Trim().Equals(fontPath.Trim()))
            {
                fontPath = fp.Trim();
                PlayerPrefs.SetString("Editor_TextMeshProFontPath", fontPath);
            }
            
            EditorGUILayout.LabelField("中日韩字体的文件前缀(fontName_Japanese)");
            string fn = EditorGUILayout.TextField("中日韩字体前缀:", fontName);
            if (!fn.Trim().Equals(fontName.Trim()))
            {
                fontName = fn.Trim();
                PlayerPrefs.SetString("Editor_TextMeshProFontName", fontName);
            }
            
            var curLanguage = temp.curLanguage;
            if (oldKey != temp.key || oldLanguage != temp.curLanguage)
            {
                var config = GetConfig(temp.curLanguage);
                if (config != null)
                {
                    SetText(temp, config);
                }
                else
                {
                    temp.SetTextEditor("== no language pck found! ==");
                    Debug.LogError($"未找到多语言包：{curLanguage}");
                }
                ChangeFontInEditor(temp, curLanguage);
                oldKey = temp.key;
                oldLanguage = curLanguage;
                EditorUtility.SetDirty(target);
            }
            else
            {
                if (temp.useBindKey)
                {
                    var config = GetConfig(temp.curLanguage);
                    var text = temp.GetComponent<TMP_Text>();
                    config.map.TryGetValue(temp.key, out var txtcfg);
                    if (text && txtcfg != null)
                    {
                        if (text.text != txtcfg.Value)
                        {
                            SetText(temp, config);
                        }
                    }
                }
            }
            

            if (oldBindKey != temp.useBindKey)
            {
                oldBindKey = temp.useBindKey;
                EditorUtility.SetDirty(target);
            }
            useBindKey.boolValue = temp.useBindKey;
            key.stringValue = temp.key;
            serializedObject.ApplyModifiedProperties();
        }
    }
    
    public static void SetText(LocalizationTextBinder temp, TextConfigList config)
    {
        if (config.map.ContainsKey(temp.key))
        {
            temp.SetTextEditor(config.map[temp.key].Value);
        }
        else
        {
            temp.SetTextEditor($"{temp.key} not found!");
        }
            
        EditorUtility.SetDirty(temp);
    }

    public static TextConfigList GetConfig(SystemLanguage lang)
    {
        if (dataMap.ContainsKey(lang))
        {
            return dataMap[lang];
        }
        var config = AssetDatabase.LoadAssetAtPath<TextConfigList>($"Assets/Project/AddressableRes/Configs/m_Data_dl/{lang.ToString()}.asset");
        if (config != null)
        {
            dataMap[lang] = config;
        }
        return config;
    }
    
    public static void ChangeFontInEditor(LocalizationTextBinder target, SystemLanguage lan)
    {
        var font = GetFont(lan);
        if (target!=null)
        {
            var text = target.GetComponent<TMP_Text>();
            var mat = GetMaterial(text, font);
            if(font!=null)
                target.SetFontInEditor(font, mat);
        }
    }
    
    public static Material GetMaterial(TMP_Text text, TMP_FontAsset newFont)
    {
        string fontName = text.font.name;
        string matName = text.fontMaterial.name.Split(" ")[0];
        if (matName.Length <= fontName.Length)
        {
            var materials = TMP_EditorUtility.FindMaterialReferences(newFont);
            return materials[0];
        }
        string endName = matName.Substring(fontName.Length);
        var allMaterials = TMP_EditorUtility.FindMaterialReferences(newFont);
        foreach (var mt in allMaterials)
        {
            if (mt.name.Equals(newFont.name + endName))
            {
                return mt;
            }
        }

        return null;
    }
    
    public static TMP_FontAsset GetFont(SystemLanguage lang)
    {
        if (fontMap.ContainsKey(lang))
        {
            return fontMap[lang];
        }
        TMP_FontAsset font = null;
        if (lang == SystemLanguage.English)
        {
            font = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>($"{fontPath}English/neo_sans_black_SDF.asset");
        }
        else
        {
            font = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>($"{fontPath}{lang.ToString()}_dl/{fontName}{lang.ToString()}.asset");
        }
        if (font != null)
        {
            fontMap[lang] = font;
        }

        return font;
    }
}