using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using AppBase.Utils;
using OfficeOpenXml;
using UnityEditor;
using UnityEngine;

public class UpdateConfigUtil
{
    public const string configScriptPath = "Assets/Project/AddressableRes/Configs/Scripts/";
    public const string configNamesPath = "Assets/Project/AddressableRes/Common/Scripts/Definition/ConfigNames.cs";
    public const string configAssetPath = "Assets/Project/AddressableRes/Configs/m_Data_dl/";
    private const string configAsmName = "HotfixAsm.Config";

    /// <summary>
    /// 一维数组分隔符
    /// </summary>
    private static readonly char[] ArraySeparatorChars = {';', ','};
    
    /// <summary>
    /// 二维数组分隔符
    /// </summary>
    private static readonly char[] Array2dSeparatorChars = {'|'};
    
    /// <summary>
    /// 字典分隔符
    /// </summary>
    private static readonly char[] DictSeparatorChars = { ':' };

    private static readonly Dictionary<string, TypeParser> typeParsers = new()
    {
        {"int", new TypeParser("int", typeof(int), s => ParseInt(s))},
        {"long", new TypeParser("long", typeof(long), s => ParseLong(s))},
        {"float", new TypeParser("float", typeof(float), s => ParseFloat(s))},
        {"double", new TypeParser("double", typeof(double), s => ParseDouble(s))},
        {"bool", new TypeParser("bool", typeof(bool), s => ParseBool(s))},
        {"string", new TypeParser("string", typeof(string), s => ParseString(s))},
        
        {"int[]", new TypeParser("List<int>", typeof(List<int>), s => ParseArray(s, ParseInt))},
        {"long[]", new TypeParser("List<long>", typeof(List<long>), s => ParseArray(s, ParseLong))},
        {"float[]", new TypeParser("List<float>", typeof(List<float>), s => ParseArray(s, ParseFloat))},
        {"bool[]", new TypeParser("List<bool>", typeof(List<bool>), s => ParseArray(s, ParseBool))},
        {"string[]", new TypeParser("List<string>", typeof(List<string>), s => ParseArray(s, ParseString))},
        
        {"int[][]", new TypeParser("List<ListObject<int>>", typeof(List<ListObject<int>>), s => ParseArray2d(s, ParseInt))},
        {"long[][]", new TypeParser("List<ListObject<long>>", typeof(List<ListObject<long>>), s => ParseArray2d(s, ParseLong))},
        {"float[][]", new TypeParser("List<ListObject<float>>", typeof(List<ListObject<float>>), s => ParseArray2d(s, ParseFloat))},
        {"bool[][]", new TypeParser("List<ListObject<bool>>", typeof(List<ListObject<bool>>), s => ParseArray2d(s, ParseBool))},
        {"string[][]", new TypeParser("List<ListObject<string>>", typeof(List<ListObject<string>>), s => ParseArray2d(s, ParseString))},

        {"dic[int][int]", new TypeParser("DictionaryObject<int, int>", typeof(DictionaryObject<int, int>), s => ParseDict(s, ParseInt, ParseInt))},
        {"dic[int][string]", new TypeParser("DictionaryObject<int, string>", typeof(DictionaryObject<int, string>), s => ParseDict(s, ParseInt, ParseString))},
        {"dic[string][string]", new TypeParser("DictionaryObject<string, string>", typeof(DictionaryObject<string, string>), s => ParseDict(s, ParseString, ParseString))},
        {"dic[string][int]", new TypeParser("DictionaryObject<string, int>", typeof(DictionaryObject<string, int>), s => ParseDict(s, ParseString, ParseInt))},
        
        {"int{}", new TypeParser("int", typeof(int), s => ParseInt(s))},
        {"string{}", new TypeParser("string", typeof(string), s => ParseString(s))},
    };
    
    public static string excelSourcePath
    {
        get
        {
            var dir = new DirectoryInfo(Path.GetDirectoryName(Environment.CurrentDirectory)!);
            var path = dir.EnumerateDirectories("*Config", SearchOption.TopDirectoryOnly).First().FullName;
            return path;
        }
    }

    /// <summary>
    /// 配置文件模板
    /// </summary>
    private const string templateConfig = @"using System;
using System.Collections.Generic;

/// <summary>
/// {1}
/// </summary>
[Serializable]
public class {0} : BaseConfig
{{{2}}}
";
    
    /// <summary>
    /// 配置属性模板
    /// </summary>
    private const string templateConfigProperty = @"
    /// <summary>
    /// {0}
    /// </summary>
    public {1} {2};
";

    /// <summary>
    /// 配置列表模板
    /// </summary>
    private const string templateConfigList = @"using System;

[Serializable]
public class {0}List : {1}
{{
}}
";

    /// <summary>
    /// 字典配置Keys模板
    /// </summary>
    private const string templateConfigKeys = @"
/// <summary>
/// {0}
/// </summary>
public static class {1}Keys
{{{2}}}
";
    
    /// <summary>
    /// 字典配置Keys文件属性模板
    /// </summary>
    private const string templateConfigKeysProperty = @"
    /// <summary>
    /// {0}
    /// </summary>
    public const string {1} = ""{2}"";
";
    
    /// <summary>
    /// ConfigNames文件模板
    /// </summary>
    private const string templateConfigNames = @"using System.Collections.Generic;
namespace AppBase.Config
{{
    public static class ConfigNames
    {{
        public static List<string> keyNames = new List<string>()
        {{
            ""{0}""
        }};
    }}
}}
";

    //当连续出现10行或10列空白时，扫描结束，避免无限扫描
    public const int skipEmptyCell = 10;
    private const string waitingForUpdateConfigData = "isWaitingForUpdateConfigData";
    private static readonly BinaryFormatter cloneFormatter = new BinaryFormatter();
    
    /// <summary>
    /// 解析配置定义和数据
    /// </summary>
    [MenuItem("Tools/Config/Update Configs")]
    public static void UpdateConfigs()
    {
        EditorPrefs.SetBool(waitingForUpdateConfigData, true);
        UpdateConfigScripts();
        if (!EditorApplication.isCompiling)
            WaitForCompile();
    }
    
    [UnityEditor.Callbacks.DidReloadScripts]
    private static void WaitForCompile()
    {
        if (!EditorPrefs.GetBool(waitingForUpdateConfigData)) return;
        EditorPrefs.DeleteKey(waitingForUpdateConfigData);
        UpdateConfigData();
    }
    
    /// <summary>
    /// 解析配置定义
    /// </summary>
    [MenuItem("Tools/Config/Steps/1. Update Config Scripts")]
    public static void UpdateConfigScripts()
    {
        if (Application.isBatchMode)
        {
            Application.SetStackTraceLogType(LogType.Log, StackTraceLogType.None);
            Application.SetStackTraceLogType(LogType.Warning, StackTraceLogType.None);
        }
        Debug.Log("UpdateConfigScripts");
        //获取文件列表
        var excelDir = new DirectoryInfo(Path.Combine(Environment.CurrentDirectory, excelSourcePath));
        if (!excelDir.Exists) throw new Exception($"Excel source path not found: {excelDir.FullName}");
        var excelFiles = excelDir.GetFiles("*.xlsx", SearchOption.AllDirectories);
        var list = new List<string>();
        //解析每一个文件
        try
        {
            for (var p = 0; p < excelFiles.Length; ++p)
            {
                //跳过隐藏文件
                var excelFile = excelFiles[p];
                if (IsNameInvalid(excelFile.Name) || excelFile.Attributes.HasFlag(FileAttributes.Hidden)) continue;
                //提示进度
                if (!Application.isBatchMode)
                    if (EditorUtility.DisplayCancelableProgressBar("更新配置脚本中", excelFile.Name, (float)p / excelFiles.Length))
                        throw new Exception("用户取消");
                //读取文件
                using var excel = new ExcelPackage(excelFile);
                var count = excel.Workbook.Worksheets.Count;
                //解析每一个工作表
                for (int i = 1; i <= count; i++)
                {
                    //跳过隐藏的工作表
                    var sheet = excel.Workbook.Worksheets[i];
                    if (IsNameInvalid(sheet.Name) || sheet.Hidden != eWorkSheetHidden.Visible) continue;
                    //提示进度
                    Debug.Log($"{excelFile.Name} {sheet.Name}");
                    if (!Application.isBatchMode)
                        if (EditorUtility.DisplayCancelableProgressBar("更新配置脚本中", $"{excelFile.Name} - {sheet.Name}", (float)p / excelFiles.Length + (float)(i+1) / count / excelFiles.Length))
                            throw new Exception("用户取消");
                    UpdateConfigScript(sheet);
                    var title = sheet.Cells[1, 2].Text?.Trim();
                    if (!string.IsNullOrEmpty(title)) list.Add(title);
                }
            }
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            if (!Application.isBatchMode)
            {
                EditorUtility.ClearProgressBar();
                if (!EditorPrefs.GetBool(waitingForUpdateConfigData))
                    EditorUtility.DisplayDialog("更新配置脚本完成", string.Join(", ", list), "OK");
            }
        }
        catch (Exception ex)
        {
            if (!Application.isBatchMode)
            {
                EditorUtility.ClearProgressBar();
                EditorUtility.DisplayDialog("更新配置脚本失败", ex.Message, "OK");
            }
            EditorPrefs.DeleteKey(waitingForUpdateConfigData);
            throw;
        }
    }

    /// <summary>
    /// 解析配置数据
    /// </summary>
    [MenuItem("Tools/Config/Steps/2. Update Config Data")]
    public static void UpdateConfigData()
    {
        if (Application.isBatchMode)
        {
            Application.SetStackTraceLogType(LogType.Log, StackTraceLogType.None);
            Application.SetStackTraceLogType(LogType.Warning, StackTraceLogType.None);
        }
        Debug.Log("UpdateConfigData");
        //获取文件列表
        var excelDir = new DirectoryInfo(Path.Combine(Environment.CurrentDirectory, excelSourcePath));
        if (!excelDir.Exists) throw new Exception($"Excel source path not found: {excelDir.FullName}");
        var excelFiles = excelDir.GetFiles("*.xlsx", SearchOption.AllDirectories);
        var list = new List<string>();
        //解析每一个文件
        try
        {
            for (var p = 0; p < excelFiles.Length; ++p)
            {
                //跳过隐藏文件
                var excelFile = excelFiles[p];
                if (IsNameInvalid(excelFile.Name) || excelFile.Attributes.HasFlag(FileAttributes.Hidden)) continue;
                //提示进度
                if (!Application.isBatchMode)
                    if (EditorUtility.DisplayCancelableProgressBar("更新配置数据中", excelFile.Name, (float)p / excelFiles.Length))
                        throw new Exception("用户取消");
                //读取文件
                using var excel = new ExcelPackage(excelFile);
                var count = excel.Workbook.Worksheets.Count;
                //解析每一个工作表
                for (int i = 1; i <= count; i++)
                {
                    //跳过隐藏的工作表
                    var sheet = excel.Workbook.Worksheets[i];
                    if (IsNameInvalid(sheet.Name) || sheet.Hidden != eWorkSheetHidden.Visible) continue;
                    //提示进度
                    Debug.Log($"{excelFile.Name} {sheet.Name}");
                    if (!Application.isBatchMode)
                        if (EditorUtility.DisplayCancelableProgressBar("更新配置数据中", $"{excelFile.Name} - {sheet.Name}", (float)p / excelFiles.Length + (float)(i+1) / count / excelFiles.Length))
                            throw new Exception("用户取消");
                    UpdateConfigData(sheet);
                    var title = sheet.Cells[1, 2].Text?.Trim();
                    if (!string.IsNullOrEmpty(title)) list.Add(title);
                }
            }
            //保存ConfigNames文件
            UpdateConfigNames(list);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            if (!Application.isBatchMode)
            {
                EditorUtility.ClearProgressBar();
                EditorUtility.DisplayDialog("更新配置数据完成", string.Join(", ", list), "OK");
            }
        }
        catch (Exception ex)
        {
            if (!Application.isBatchMode)
            {
                EditorUtility.ClearProgressBar();
                EditorUtility.DisplayDialog("更新配置数据失败", ex.Message, "OK");
            }
            throw;
        }
    }

    /// <summary>
    /// 更新配置脚本
    /// </summary>
    private static void UpdateConfigScript(ExcelWorksheet sheet)
    {
        //获取文件名
        var configName = sheet.Cells[1, 2].Text?.Trim();
        if (string.IsNullOrEmpty(configName))
        {
            Debug.LogWarning($"Ignore Sheet: {sheet.Name}");
            return;
        }
        
        //获取类名
        var className = sheet.Cells[1, 4].Text?.Trim();
        if (string.IsNullOrEmpty(className))
        {
            className = configName;
        }

        //获取表格说明
        var sheetComment = sheet.Cells[1, 3].Text;
        if (string.IsNullOrEmpty(sheetComment))
        {
            sheetComment = sheet.Name;
        }

        //扫描每一列
        var properties = new StringBuilder();
        var emptyColumn = 0;
        for (int i = 2; i <= sheet.Dimension.End.Column; i++)
        {
            //获取列名
            var header = sheet.Cells[4, i].Text;
            //如果列名是分表列名，则忽略
            if (header.Contains(':')) continue;
            //获取列注释
            var comment = sheet.Cells[2, i].Text;
            if (string.IsNullOrEmpty(comment)) comment = header;
            //获取列类型
            var type = sheet.Cells[3, i].Text.Trim();
            if (string.IsNullOrEmpty(type) || type.ToLower() == "null")
            {
                if (++emptyColumn >= skipEmptyCell) break;
                continue;
            }
            emptyColumn = 0;
            //生成属性
            properties.AppendFormat(templateConfigProperty, ParseCommend(comment), ParseTypeName(type), header);
        }
        
        //生成配置代码
        var code = string.Format(templateConfig, className, ParseCommend(sheetComment), properties);
        var assetPath = configScriptPath + className + ".cs";
        var path = Path.Combine(Environment.CurrentDirectory, assetPath);
        File.WriteAllText(path, code, Encoding.UTF8);
        
        //对于字典配置，生成Keys文件
        var keyType = sheet.Cells[3, 2].Text.Trim();
        var listType = keyType.EndsWith("{}") ? $"BaseConfigDictionary<{ParseTypeName(keyType)}, {className}>" : $"BaseConfigList<{className}>";
        code = string.Format(templateConfigList, className, listType);
        assetPath = configScriptPath + className + "List.cs";
        path = Path.Combine(Environment.CurrentDirectory, assetPath);
        File.WriteAllText(path, code, Encoding.UTF8);
    }
    
    /// <summary>
    /// 更新配置数据
    /// </summary>
    private static void UpdateConfigData(ExcelWorksheet sheet)
    {
        //获取配置文件名
        var configName = sheet.Cells[1, 2].Text?.Trim();
        if (string.IsNullOrEmpty(configName))
        {
            Debug.LogWarning($"Ignore Sheet: {sheet.Name}");
            return;
        }
        
        //获取配置类名
        var className = sheet.Cells[1, 4].Text?.Trim();
        if (string.IsNullOrEmpty(className))
        {
            className = configName;
        }

        //获取配置类型
        var configType = ReflectionUtil.GetType(configAsmName, className);
        if (configType == null) throw new Exception($"{configName}表里的类型：{className} 在所有配置的程序集中找不到！");
        var assetType = ReflectionUtil.GetType(configAsmName, className + "List");
        if (assetType == null) throw new Exception($"{configName}表里的类型：{className}List 在所有配置的程序集中找不到！");
        var keyTypeName = sheet.Cells[3, 2].Text.Trim().ToLower();
        var isKeyMapType = keyTypeName.EndsWith("{}");
        var listType = typeof(List<>).MakeGenericType(configType);
        var listObj = Activator.CreateInstance(listType) as IList;
        if (listObj == null) throw new Exception(configName);
        var splitDict = new Dictionary<string, IList>();
        
        //获取字典配置类型
        var keysType = isKeyMapType ? typeof(List<>).MakeGenericType(ParseType(keyTypeName)) : null;
        var keysObj = isKeyMapType ? Activator.CreateInstance(keysType) as IList : null;
        if (isKeyMapType && keysObj == null) throw new Exception(configName);
        var keysComments = isKeyMapType ? new List<string>() : null;

        //扫描每一行
        var emptyRow = 0;
        for (int r = 5; r <= sheet.Dimension.End.Row; r++)
        {
            //忽略空行 和 值为 0 的情况
            if (string.IsNullOrWhiteSpace(sheet.Cells[r, 2].Text) || sheet.Cells[r, 2].Text == "0")
            {
                if (++emptyRow >= skipEmptyCell) break;
                continue;
            }
            
            //创建配置对象
            var rowObj = Activator.CreateInstance(configType);
            if (rowObj == null) throw new Exception(configName);
            
            //扫描每一列
            emptyRow = 0;
            var emptyColumn = 0;
            for (int c = 2; c <= sheet.Dimension.End.Column; c++)
            {
                //获取列类型
                var typeName = sheet.Cells[3, c].Text?.Trim();
                if (string.IsNullOrEmpty(typeName) || typeName.ToLower() == "null")
                {
                    if (++emptyColumn >= skipEmptyCell) break;
                    continue;
                }
                //获取列名
                emptyColumn = 0;
                var columnName = sheet.Cells[4, c].Text;
                //获取列数据
                var columnData = sheet.Cells[r, c].Text ?? "";
                
                //如果是分表列，则进行分表处理
                var columnNameSplits = columnName.Split(':');
                var setRowObj = rowObj;
                if (columnNameSplits.Length > 1)
                {
                    var splitConfigName = columnNameSplits[0].Trim();
                    columnName = columnNameSplits[1].Trim();
                    if (!splitDict.TryGetValue(splitConfigName, out IList splitList))
                    {
                        //新建一张分表
                        splitList = (IList)Activator.CreateInstance(listType);
                        splitDict.Add(splitConfigName, splitList);
                    }
                    if (splitList.Count > listObj.Count)
                    {
                        //使用已有的分表行数据
                        setRowObj = splitList[^1];
                    }
                    else
                    {
                        //克隆一份新的分表行数据
                        using var ms = new MemoryStream();
                        cloneFormatter.Serialize(ms, rowObj);
                        ms.Seek(0, SeekOrigin.Begin);
                        setRowObj = cloneFormatter.Deserialize(ms);
                        splitList.Add(setRowObj);
                    }
                }
                
                //解析列数据
                try
                {
                    ParseField(setRowObj, columnName, typeName, columnData);
                }
                catch (Exception ex)
                {
                    throw new Exception($"解析错误：{configName} 第{r}行 第{c}列 {columnName} {typeName} {columnData}", ex);
                }
                
                //保存主键
                if (isKeyMapType && c == 2)
                {
                    //如果有重复的主键，报错
                    var key = ParseValue(keyTypeName, columnData);
                    if (keysObj.Contains(key))
                    {
                        throw new Exception($"configName == {configName} 第{r}行 不允许有相同的主键 {key}");
                    }
                    
                    keysObj.Add(key);
                    keysComments.Add(ParseCommend(sheet.Cells[r, 1].Text));
                }
            }
            listObj.Add(rowObj);
        }

        //保存配置文件
        void SaveConfigAsset(string configName, IList listObj)
        {
            var path = configAssetPath + configName + ".asset";
            bool isNew = false;
            var assetObj = AssetDatabase.LoadAssetAtPath(path, assetType);
            if (assetObj == null)
            {
                assetObj = ScriptableObject.CreateInstance(assetType);
                isNew = true;
            }
            if (assetObj == null) throw new Exception(configName);
            ((IConfigList)assetObj).values = listObj;
            if (isKeyMapType) ((IConfigDictionary)assetObj).keys = keysObj;
            EditorUtility.SetDirty(assetObj);
            if (isNew)
            {
                AssetDatabase.CreateAsset(assetObj, path);
            }
        }
        SaveConfigAsset(configName, listObj);
        
        //保存分表的配置文件
        foreach (var (splitName, splitList) in splitDict)
        {
            SaveConfigAsset(splitName, splitList);
        }
        
        //保存主键文件
        if (isKeyMapType && keyTypeName == "string{}")
        {
            //获取表格说明
            var sheetComment = sheet.Cells[1, 3].Text;
            if (string.IsNullOrEmpty(sheetComment)) sheetComment = sheet.Name;
            //生成主键代码
            var keyProperties = keysObj.Cast<string>()
                .Where(s => !int.TryParse(s.Trim(), out _)) //纯数字的主键不用生成
                .Select((s, i) => string.Format(templateConfigKeysProperty, keysComments[i] ?? ParseCommend(s), ParseKey(s), s))
                .ToArray();
            //保存主键文件
            if (keyProperties.Length > 0)
            {
                var keysCode = string.Format(templateConfigKeys, ParseCommend(sheetComment), configName, string.Join("", keyProperties));
                var path = configScriptPath + configName + "Keys.cs";
                File.WriteAllText(path, keysCode, Encoding.UTF8);
            }
        }
    }
    
    /// <summary>
    /// 生成ConfigNames文件
    /// </summary>
    private static void UpdateConfigNames(List<string> list)
    {
        list.Sort();
        var code = string.Format(templateConfigNames, string.Join("\",\n            \"", list));
        var path = Path.Combine(Environment.CurrentDirectory, configNamesPath);
        File.WriteAllText(path, code, Encoding.UTF8);
    }

    /// <summary>
    /// 解析类型名
    /// </summary>
    public static string ParseTypeName(string typeName)
    {
        typeName = typeName.Trim().ToLower();
        if (!typeParsers.TryGetValue(typeName, out var parser)) throw new Exception(typeName);
        return parser.name;
    }

    /// <summary>
    /// 解析类型
    /// </summary>
    private static Type ParseType(string typeName)
    {
        typeName = typeName.Trim().ToLower();
        if (!typeParsers.TryGetValue(typeName, out var parser)) throw new Exception(typeName);
        return parser.type;
    }

    /// <summary>
    /// 解析值
    /// </summary>
    private static object ParseValue(string typeName, string str)
    {
        typeName = typeName.Trim().ToLower();
        if (!typeParsers.TryGetValue(typeName, out var parser)) throw new Exception(typeName);
        return parser.Parse(str);
    }

    /// <summary>
    /// 将值解析到对象的字段中
    /// </summary>
    private static void ParseField(object obj, string fieldName, string typeName, string str)
    {
        var fieldInfo = obj.GetType().GetField(fieldName);
        if (fieldInfo == null) throw new Exception(fieldName);
        fieldInfo.SetValue(obj, ParseValue(typeName, str));
    }
   
    /// <summary>
    /// 解析注释
    /// </summary>
    private static string ParseCommend(string commend)
    {
        return commend.Replace("\\n", "\n").Replace("\n", "\n    /// ");
    }

    private static int ParseInt(string str)
    {
        return !int.TryParse(str.Trim(), out int result) ? 0 : result;
    }

    private static long ParseLong(string str)
    {
        return !long.TryParse(str.Trim(), out long result) ? 0 : result;
    }
    
    private static double ParseDouble(string str)
    {
        return !double.TryParse(str.Trim(), NumberStyles.Float, CultureInfo.InvariantCulture, out double result) ? 0 : result;
    }
    
    private static float ParseFloat(string str)
    {
        return !float.TryParse(str.Trim(), NumberStyles.Float, CultureInfo.InvariantCulture, out float result) ? 0 : result;
    }
    
    private static bool ParseBool(string str)
    {
        return str.Trim().ToLower() is "1" or "true";
    }

    private static string ParseString(string str)
    {
        return str.Trim().Replace("\\n", "\n");
    }

    /// <summary>
    /// 解析一维数组
    /// </summary>
    private static List<T> ParseArray<T>(string str, Func<string, T> selector)
    {
        if (string.IsNullOrWhiteSpace(str)) return new List<T>();
        return str.Trim().Split(ArraySeparatorChars).Select(selector).ToList();
    }

    /// <summary>
    /// 解析二维数组
    /// </summary>
    private static List<ListObject<T>> ParseArray2d<T>(string str, Func<string, T> selector)
    {
        if (string.IsNullOrWhiteSpace(str)) return new List<ListObject<T>>();
        return str.Trim().Split(Array2dSeparatorChars).Select(x => new ListObject<T>(ParseArray(x, selector))).ToList();
    }

    /// <summary>
    /// 解析字典
    /// </summary>
    private static DictionaryObject<K,V> ParseDict<K,V>(string str, Func<string, K> selectorK, Func<string, V> selectorV)
    {
        if (string.IsNullOrWhiteSpace(str)) return new DictionaryObject<K, V>(new Dictionary<K, V>());
        return new DictionaryObject<K, V>(ParseArray(str, ParseString)
            .Select(p => p.Split(DictSeparatorChars))
            .Select(p => new KeyValuePair<K, V>(selectorK(p[0]), selectorV(p[1])))
            .ToList());
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

    /// <summary>
    /// 判定文件名是否要被忽略
    /// </summary>
    public static bool IsNameInvalid(string name)
    {
        return string.IsNullOrEmpty(name) || name.StartsWith('~') || name.StartsWith('$') || name.StartsWith('.');
    }
    
    private class TypeParser
    {
        public string name;
        public Type type;
        public Func<string, object> Parse;
        public TypeParser(string name, Type type, Func<string, object> parser)
        {
            this.name = name;
            this.type = type;
            this.Parse = parser;
        }
    }
}