using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using AppBase.UI.Dialog;
using AppBase.UI.Scene;
using UnityEditor;
using UnityEngine;

namespace AppBase.UI.Editor
{
    public static class UIBindingEditor
    {
        private const string CodeTemplate1 = @"using AppBase.UI;
namespace {2}
{{
    public partial class {0}
    {{{1}
    }}
}}";
        private const string CodeTemplate2 = @"using AppBase.UI;
public partial class {0}
{{{1}
}}";
        private const string BindingTemplate1 = @"
        public {2} {0} => FindUIBinding<{2}>(""{1}"");";
        private const string BindingTemplate2 = @"
    public {2} {0} => FindUIBinding<{2}>(""{1}"");";
        private static readonly Regex regex = new(@"^[a-zA-Z_]\w*$");
        
        [MenuItem("Assets/生成Prefab的UI绑定", false, 0)]
        public static void GenerateUIBinding()
        {
            var prefabs = GetSelections();
            LastError = "";
            var list = new List<string>();
            //扫描所有选中的prefab
            foreach (var prefab in prefabs)
            {
                //扫描所有子节点
                var uiViews = prefab.GetComponentsInChildren<UIView>();
                foreach (var uiView in uiViews)
                {
                    var success = GenerateBindingCode(uiView);
                    if (success != null) list.Add(success);
                }
            }
            if (list.Count > 0)
            {
                EditorUtility.DisplayDialog($"成功生成{list.Count}个UI绑定", string.Join(", ", list), "确定");
            }
            else
            {
                EditorUtility.DisplayDialog("失败", $"生成UI绑定失败: {LastError}", "确定");
            }
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
        }

        private static string LastError;

        public static string GenerateBindingCode(UIView uiView)
        {
            //检查绑定类型
            var viewType = uiView.GetType();
            if (viewType == typeof(UIView) || viewType == typeof(UIDialog) || viewType == typeof(UIScene))
            {
                LastError ??= $"根结点需要绑定强类型: {uiView.name}";
                return null;
            }
            
            //取原始代码路径
            var originPath = AssetDatabase.GetAllAssetPaths().FirstOrDefault(p => p.EndsWith($"/{viewType.Name}.cs"));
            if (string.IsNullOrEmpty(originPath))
            {
                LastError = $"没有找到类型的原始代码: {viewType.Name}.cs";
                return null;
            }
            
            //扫描绑定
            var dict = new Dictionary<string, Tuple<string, string>>();
            ScanUIBindings(uiView.transform, dict);
            if (dict.Count == 0)
            {
                LastError = $"没有找到任何UIBinding的绑定: {uiView.name}";
                return null;
            }
            
            //修改原始代码为partial类
            var originCode = File.ReadAllText(originPath, Encoding.UTF8);
            if (originCode.IndexOf($"partial class {viewType.Name}", StringComparison.Ordinal) == -1)
            {
                originCode = originCode.Replace($"class {viewType.Name}", $"partial class {viewType.Name}");
                File.WriteAllText(originPath, originCode, Encoding.UTF8);
            }
            
            //生成binding代码
            var hasNs = !string.IsNullOrEmpty(viewType.Namespace);
            var bindingCode = string.Join("", dict.Select(kv => string.Format(hasNs?BindingTemplate1:BindingTemplate2, kv.Key, kv.Value.Item1, kv.Value.Item2)));
            var code = string.Format(hasNs?CodeTemplate1:CodeTemplate2, viewType.Name, bindingCode, viewType.Namespace);
            var bindingPath = Path.ChangeExtension(originPath, ".binding.cs");
            File.WriteAllText(bindingPath, code, Encoding.UTF8);
            
            Debug.Log($"生成UI绑定代码成功: {viewType.Name}");
            return viewType.Name;
        }

        private static void ScanUIBindings(Transform transform, Dictionary<string, Tuple<string, string>> dict)
        {
            var queue = new Queue<Transform>();
            queue.Enqueue(transform);
            while (queue.Count > 0)
            {
                var current = queue.Dequeue();
                for (int i = 0; i < current.childCount; i++)
                {
                    Transform child = current.GetChild(i);
                    if (child.TryGetComponent<UIBinding>(out var binding) && binding.IsBindingNameValid())
                    {
                        var type = binding.GetType();
                        var typeName = type == typeof(UIView) || type == typeof(UIBinding) ? type.Name : type.FullName;
                        dict.Add(GetBindingName(binding, dict), new Tuple<string, string>(GetNodePath(child), typeName));
                    }
                    //UIView不再继续遍历
                    if (child.TryGetComponent<UIView>(out var view) && view.GetType() != typeof(UIView)) continue;
                    queue.Enqueue(child);
                }
            }
        }

        private static string GetBindingName(UIBinding binding, Dictionary<string, Tuple<string, string>> dict)
        {
            var name = string.IsNullOrEmpty(binding.BindingName) ? binding.name : binding.BindingName;
            for (int i = 2; i < 1000; i++)
            {
                if (!dict.ContainsKey(name)) break;
                name = binding.name + i;
            }
            return name;
        }
        
        private static string GetNodePath(Transform node)
        {
            var stack = new Stack<string>();
            while (node != null && node.parent != null && node.parent.name != "Canvas (Environment)" && (stack.Count == 0 || node.GetComponent<UIView>() == null))
            {
                stack.Push(node.name);
                node = node.parent;
            }
            return string.Join('/', stack);
        }
        
        //获取选中的prefabs
        private static List<GameObject> GetSelections()
        {
            var selections = new List<GameObject>();
            foreach (var obj in Selection.objects)
            {
                var path = AssetDatabase.GetAssetPath(obj);
                //如果选中了文件夹，展开文件夹
                if (AssetDatabase.IsValidFolder(path))
                {
                    var objs = AssetDatabase.FindAssets("t:prefab", new[] { path })
                        .Select(AssetDatabase.GUIDToAssetPath)
                        .Where(p => !AssetDatabase.IsValidFolder(p))
                        .Select(AssetDatabase.LoadAssetAtPath<GameObject>);
                    selections.AddRange(objs);
                }
                else if (AssetDatabase.GetAssetPath(obj).EndsWith(".prefab"))
                {
                    selections.Add((GameObject)obj);
                }
            }
            return selections;
        }

        /// <summary>
        /// 检查UIBinding是否是合法变量名
        /// </summary>
        public static bool IsBindingNameValid(this UIBinding binding)
        {
            if (binding == null) return false;
            if (string.IsNullOrEmpty(binding.BindingName)) return true;
            return regex.IsMatch(binding.BindingName);
        }
    }
}
