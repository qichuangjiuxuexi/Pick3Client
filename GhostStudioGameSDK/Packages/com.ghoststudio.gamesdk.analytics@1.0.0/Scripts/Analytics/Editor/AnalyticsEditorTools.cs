using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using OfficeOpenXml;
using UnityEditor;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;
using UnityEngine;

namespace AppBase.Analytics.Editor
{
    public class AnalyticsEditorTools
    {
        private const string analyticsDir = "Assets/Project/AddressableRes/Features/Analytics/Scripts";
        
        private const string templateScript = @"namespace AppBase.Analytics
{{
    /// <summary>
    /// {2}
    /// </summary>
    public class AnalyticsEvent_{0} : AnalyticsEvent
    {{{3}
        /// <summary>
        /// {2}
        /// </summary>{5}
        public AnalyticsEvent_{0}({4}) : base(""{1}"")
        {{{6}
        }}
    }}
}}";

        private const string templateDefaultInit = @"
        /// <summary>
        /// {2}
        /// </summary>
        public AnalyticsEvent_{0}() : base(""{1}"")
        {{
        }}
";

        private const string templateParamDef = @"
        /// <summary>
        /// {2}
        /// </summary>
        public {1} {0}
        {{
            get => eventData.TryGetValue(""{0}"", out var value) ? ({1})value : default;
            set => eventData[""{0}""] = value;
        }}
";
        
        private const string templateParamInit = @"{2}{1} {0}";

        private const string templateParamSet = @"
            this.{0} = {0};";

        private const string templateParamDesc = @"
        /// <param name=""{0}"">{1}</param>";
        
        [MenuItem("Tools/Analytics/Update Analytics Events")]
        public static void UpdateAnalyticsEvents()
        {
            var sheet = GetAnalyticsConfig("AnalyticsEventConfig", out var excel);
            if (sheet == null) throw new Exception("AnalyticsEventConfig not found");
            UpdateAnalyticsEventScripts(sheet);
            excel.Dispose();
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            if (!Application.isBatchMode)
                EditorUtility.DisplayDialog("生成打点配置完成", "", "OK");
        }
        
        private static void UpdateAnalyticsEventScripts(ExcelWorksheet sheet)
        {
            var emptyRow = 0;
            string oldEventName = "";
            string oldEventDesc = "";
            string oldEventType = "";
            StringBuilder paramDef = new StringBuilder();
            StringBuilder paramInit = new StringBuilder();
            StringBuilder paramSet = new StringBuilder();
            StringBuilder paramDesc = new StringBuilder();
            Action WriteAllText = () =>
            {
                if (!string.IsNullOrEmpty(oldEventName))
                {
                    if (paramInit.Length > 0)
                    {
                        paramDef.Append(string.Format(templateDefaultInit, oldEventType, oldEventName, oldEventDesc));
                    }

                    var script = string.Format(templateScript, oldEventType, oldEventName, oldEventDesc, paramDef,
                        paramInit, paramDesc, paramSet);
                    if (!Directory.Exists(analyticsDir)) Directory.CreateDirectory(analyticsDir);
                    var scriptPath = Path.Combine(analyticsDir, $"AnalyticsEvent_{oldEventType}.cs");
                    File.WriteAllText(scriptPath, script, Encoding.UTF8);
                }
                oldEventName = "";
                oldEventType = "";
                oldEventDesc = "";
            };
            for (int r = 5; r <= sheet.Dimension.End.Row +1; r++)
            {
                var eventName = sheet.Cells[r, 2].Text?.Trim();
                var eventDesc = sheet.Cells[r, 1].Text?.Trim();
                eventDesc = string.IsNullOrEmpty(eventDesc) ? eventName : eventDesc;
                if (string.IsNullOrEmpty(eventName))
                {
                    WriteAllText();
                    if (++emptyRow >= UpdateConfigUtil.skipEmptyCell) break;
                    continue;
                }
                var eventType = eventName.Replace(" ",  "_");
               
                emptyRow = 0;
                var emptyColumn = 0;
                if (oldEventName != eventName)
                {
                    WriteAllText();
                    oldEventName = eventName;
                    oldEventType = eventType;
                    oldEventDesc = eventDesc;
                    paramDef = new StringBuilder();
                    paramInit = new StringBuilder();
                    paramSet = new StringBuilder();
                    paramDesc = new StringBuilder();
                }
                
                var paramName = sheet.Cells[r, 4].Text?.Trim().Replace(" ", "_");
                if (string.IsNullOrEmpty(paramName))
                {
                    if (++emptyColumn >= UpdateConfigUtil.skipEmptyCell) break;
                    continue;
                }
                var paramType = UpdateConfigUtil.ParseTypeName(sheet.Cells[r, 3].Text);
                emptyColumn = 0;
                var desc =  sheet.Cells[r, 5].Text ?? paramName;
                paramDef.Append(string.Format(templateParamDef, paramName, paramType, desc));
                paramInit.Append(string.Format(templateParamInit, paramName, paramType, paramInit.Length > 0 ? ", " : ""));
                paramSet.Append(string.Format(templateParamSet, paramName));
                paramDesc.Append(string.Format(templateParamDesc, paramName, desc));
            }
        }

        private static ExcelWorksheet GetAnalyticsConfig(string configName, out ExcelPackage excel)
        {
            var excelSourcePath = UpdateConfigUtil.excelSourcePath;
            var excelDir = new DirectoryInfo(Path.Combine(Environment.CurrentDirectory, excelSourcePath));
            if (!excelDir.Exists) throw new Exception($"Excel source path not found: {excelDir.FullName}");
            var excelFiles = excelDir.GetFiles("*.xlsx", SearchOption.AllDirectories);
            foreach (var excelFile in excelFiles)
            {
                if (UpdateConfigUtil.IsNameInvalid(excelFile.Name) || excelFile.Attributes.HasFlag(FileAttributes.Hidden)) continue;
                excel = new ExcelPackage(excelFile);
                var count = excel.Workbook.Worksheets.Count;
                for (int i = 1; i <= count; i++)
                {
                    var sheet = excel.Workbook.Worksheets[i];
                    Debug.Log($"{excelFile.Name} {sheet.Name}");
                    if (UpdateConfigUtil.IsNameInvalid(sheet.Name) || sheet.Hidden != eWorkSheetHidden.Visible) continue;
                    var title = sheet.Cells[1, 2].Text?.Trim();
                    if (title == configName) return sheet;
                }
                excel.Dispose();
            }
            excel = null;
            return null;
        }
    }
}
