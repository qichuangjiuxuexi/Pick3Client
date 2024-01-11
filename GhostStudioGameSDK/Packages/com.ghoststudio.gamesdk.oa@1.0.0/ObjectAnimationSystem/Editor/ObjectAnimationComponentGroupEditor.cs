using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using AppBase.OA;
using AppBase.OA.Editor;
using AppBase.OA.WorldPositionMoveModule.Editor;
using UnityEditor;
using UnityEngine;

namespace Project.AppBase.ObjectAnimationSystem.Scripts.Editor
{
    [CustomEditor(typeof(ObjectAnimationComponentGroup))]
    public class ObjectAnimationComponentGroupEditor : UnityEditor.Editor,IObjectAnimationEditor
    {
        public SerializedProperty animationName;
        private SerializedProperty targets;
        private SerializedProperty totalElapsedTime;
        private SerializedProperty delayCurve;
        private SerializedProperty useCurveToSetDelay;
        private SerializedProperty delayRange;
        
        private List<UnityEditor.Editor> editors;
        private int frameRate = 60;
        
        private bool isPreviewFrameByFrame = false;
        private ObjectAnimationComponentGroup curTarget;
        private double lastUpdateTime = 0;
        private int memberNum2Add = 1;
        private void OnEnable()
        {
            editors = new List<UnityEditor.Editor>(); 
            targets = serializedObject.FindProperty("targets");
            totalElapsedTime = serializedObject.FindProperty("totalElapsedTime");
            delayCurve = serializedObject.FindProperty("delayCurve");
            useCurveToSetDelay = serializedObject.FindProperty("useCurveToSetDelay");
            delayRange = serializedObject.FindProperty("delayRange");
            animationName = serializedObject.FindProperty("animationName");
            for (int i = 0; i < targets.arraySize; i++)
            {
                AddEditor(i);
            }
            EditorApplication.update += OnEditorUpdate;
        }

        void AddEditor(int index)
        {
            var editor = UnityEditor.Editor.CreateEditor(targets.GetArrayElementAtIndex(index).objectReferenceValue);
            editors.Add(editor);
            if (editor is IObjectAnimationEditor animEditor)
            {
                animEditor.show = false;
                animEditor.hostEditor = this;
            }
        }

        void OnEditorUpdate()
        {
            CheckDeletedMember();
            if (IsPreviewing())
            {
                if (EditorApplication.timeSinceStartup - lastUpdateTime > 1 / (float) frameRate)
                {
                    bool allFinished = true;
                    if (state == ObjectAnimationComponentEditorState.PreviewFrame)
                    {
                        if (isPreviewFrameByFrame)
                        {
                            totalElapsedTime.floatValue += (1 / (float) frameRate); 
                        }
                    }else if (state == ObjectAnimationComponentEditorState.Preview)
                    {
                        totalElapsedTime.floatValue += (1 / (float) frameRate); 
                    }
                    for (int i = 0; i < editors.Count; i++)
                    {
                        var cp = editors[i].serializedObject.targetObject as ObjectAnimationComponent;
                        if (state == ObjectAnimationComponentEditorState.PreviewFrame)
                        {
                            if (isPreviewFrameByFrame)
                            {
                                cp.SimuUpdateAll(1 / (float) frameRate);
                            }
                        }else if (state == ObjectAnimationComponentEditorState.Preview)
                        {
                            cp.SimuUpdateAll(1 / (float) frameRate);
                        }

                        allFinished &= (cp).state == ObjectAnimState.Finished;
                    }

                    if (state == ObjectAnimationComponentEditorState.PreviewFrame && isPreviewFrameByFrame)
                    {
                        isPreviewFrameByFrame = false;
                    }

                    if (allFinished)
                    {
                        (target as ObjectAnimationComponentGroup).onfinished?.Invoke();
                        state = ObjectAnimationComponentEditorState.None;
                    }

                    lastUpdateTime = EditorApplication.timeSinceStartup;
                    
                }
            }

            serializedObject.ApplyModifiedProperties();

        }

        void CheckDeletedMember()
        {
            for (int i = 0; i < targets.arraySize; i++)
            {
                var el = targets.GetArrayElementAtIndex(i);
                if (el == null || el.objectReferenceValue == null)
                {
                    targets.DeleteArrayElementAtIndex(i);
                    editors.RemoveAt(i);
                    i--;
                }
            }
        }

        public void DrawSelfInspector()
        {
            EditorGUILayout.PropertyField(animationName,EditorConstGUIContent.ConfigAnimationName);
            EditorGUILayout.PropertyField(useCurveToSetDelay,EditorConstGUIContent.ConfigUseDelayCurve);
            EditorGUILayout.HelpBox(EditorConstGUIContent.ConfigHelpDelay);
            if (useCurveToSetDelay.boolValue)
            {
                EditorGUILayout.PropertyField(delayCurve,EditorConstGUIContent.ConfigDelayCurve);
                EditorGUILayout.PropertyField(delayRange,EditorConstGUIContent.ConfigDelayRange);
            }
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();
            DrawSelfInspector();
            DrawPlayArea();
            EditorGUILayout.Space(50);
            DrawAnimMembers();
            serializedObject.ApplyModifiedProperties();
            for (int i = 0; i < editors.Count; i++)
            {
                editors[i].serializedObject.ApplyModifiedProperties();
            }
            DrawPlayArea();
        }

        void DrawPlayArea()
        {
            EditorGUILayout.BeginHorizontal();
            frameRate = EditorGUILayout.IntField("模拟帧率", frameRate);
            if (GUILayout.Button("播放"))
            {
                QuitEditMode();
                if (!IsPreviewing())
                {
                    curTarget = serializedObject.targetObject as ObjectAnimationComponentGroup;
                    curTarget.onfinished = () =>
                    {
                        state = ObjectAnimationComponentEditorState.None;
                        isPreviewFrameByFrame = false;
                        curTarget.onfinished = null;
                        Debug.Log("组动画播放完毕！");
                    };
                    curTarget.Init();
                    curTarget.SimulatePlay();
                    state = ObjectAnimationComponentEditorState.Preview;
                }
            }

            if (GUILayout.Button("逐帧播放"))
            {
                if (!IsPreviewing())
                {
                    curTarget = serializedObject.targetObject as ObjectAnimationComponentGroup;
                    curTarget.onfinished = () =>
                    {
                        state = ObjectAnimationComponentEditorState.None;
                        isPreviewFrameByFrame = false;
                        curTarget.onfinished = null;
                        Debug.Log("组动画播放完毕！");
                    };
                    curTarget.SimulatePlay();
                    state = ObjectAnimationComponentEditorState.PreviewFrame;
                }
            }


            if (IsPreviewing())
            {
                if (GUILayout.Button("停止播放"))
                {
                    state = ObjectAnimationComponentEditorState.None;
                    isPreviewFrameByFrame = false;
                }
            }

            if (state == ObjectAnimationComponentEditorState.PreviewFrame)
            {
                if (GUILayout.Button("下一帧"))
                {
                    isPreviewFrameByFrame = true;
                }
            }

            EditorGUILayout.EndHorizontal();
            if (state == ObjectAnimationComponentEditorState.Preview ||
                state == ObjectAnimationComponentEditorState.PreviewFrame)
            {
                EditorGUILayout.HelpBox($"已播放时长:{totalElapsedTime.floatValue}", MessageType.Info);
            }

            EditorGUILayout.Space();
        }

        void DrawAnimMembers()
        {
            CheckDeletedMember();
            EditorGUILayout.LabelField($"******动画成员列表 （共{targets.arraySize}个）******",
                EditorStyleConst.SplitLineStyle);
            EditorGUILayout.Space();

            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button("展开所有"))
            {
                for (int i = 0; i < editors.Count; i++)
                {
                    if (editors[i] is IObjectAnimationEditor tmp)
                    {
                        tmp.show = true;
                    }
                }
            }
            
            if (GUILayout.Button("折叠所有"))
            {
                for (int i = 0; i < editors.Count; i++)
                {
                    if (editors[i] is IObjectAnimationEditor tmp)
                    {
                        tmp.show = false;
                    }
                }
            }
                        
            if (GUILayout.Button("获取已有子组件"))
            {
                curTarget = target as ObjectAnimationComponentGroup;
                curTarget.targets = curTarget.GetComponentsInChildren<ObjectAnimationComponent>(true).ToList();
            }
            
            EditorGUILayout.EndHorizontal();
            EditorGUILayout.Space(20);
            
            
            if (editors == null || editors.Count == 0)
            {
                DrawWhenNoMember();
                return;
            }
            for (int i = 0; i < editors.Count; i++)
            {
                if (editors[i] == null)
                {
                    continue;
                }
                GUI.backgroundColor = i % 2 == 0 ? EditorStyleConst.MemberBGColor2 : EditorStyleConst.MemberBGColor1; 
                EditorGUILayout.Space(20);
                EditorGUILayout.ObjectField($"组动画成员{i + 1}:",editors[i].serializedObject.targetObject,typeof(ObjectAnimationComponent));
                if (editors[i] is IObjectAnimationEditor animEditor)
                {
                    animEditor.show = EditorGUILayout.Toggle("是否展开以编辑",animEditor.show);
                }
                if (editors[i] is IObjectAnimationEditor animEditor2)
                {
                    if (animEditor2.show)
                    {
                        editors[i].OnInspectorGUI();
                    } 
                }
                else
                {
                    editors[i].OnInspectorGUI();
                }
                EditorGUILayout.Space(10);

                EditorGUILayout.BeginHorizontal();
                if (GUILayout.Button("删除此成员动画"))
                {
                    if (EditorUtility.DisplayDialog("注意", "确认要删除此成员动画吗?", "确认", "取消"))
                    {
                        DestroyImmediate((editors[i].target as ObjectAnimationComponent).gameObject);
                        DestroyImmediate(editors[i]);
                        targets.DeleteArrayElementAtIndex(i);
                        editors.RemoveAt(i);
                        i--;
                    }
                }
                
                if (GUILayout.Button("复制此成员动画"))
                {
                    var go = (editors[i].target as ObjectAnimationComponent).gameObject;
                    var newOne = Instantiate(go);
                    newOne.name = go.name;
                    CreateMemberFromPrefabTemplate(newOne);
                }

                if (editors[i] is IObjectAnimationEditor editor)
                {
                    if (editor.show)
                    {
                        if (GUILayout.Button("折叠"))
                        {
                            editor.show = false;
                        }
                    }
                    else
                    {
                        if (GUILayout.Button("展开"))
                        {
                            editor.show = true; 
                        }
                    }

                }

                 
                EditorGUILayout.EndHorizontal();
                EditorGUILayout.LabelField(
                    "********************************************************************************************************************************************************",
                    EditorStyleConst.SplitLineStyle);
            }
        }
        
        void DrawWhenNoMember()
        {
            EditorGUILayout.LabelField("无");
            EditorGUILayout.BeginHorizontal();
            memberNum2Add = EditorGUILayout.IntField("输入创建的数量:", memberNum2Add);
            if (GUILayout.Button("创建空配置类型"))
            {
                AddEmptyMember();
            }
            if (GUILayout.Button("选择配置模版创建"))
            {
                AddFromTemplateConfig();
            }
            
            if (GUILayout.Button("选择预制件模版创建"))
            {
                AddFromTemplatePrefab();
            }
            EditorGUILayout.EndHorizontal();
        }

        void AddEmptyMember()
        {
            if (memberNum2Add > 0)
            {
                for (int i = 0; i < memberNum2Add; i++)
                {
                    GameObject go = new GameObject($"anim_{i + 1}");
                    go.transform.SetParent((target as ObjectAnimationComponentGroup).transform);
                    go.transform.localPosition = Vector3.one;
                    var oa = go.AddComponent<ObjectAnimationComponent>();
                    oa.modulesConfigs = new List<BaseObjectAnimConfig>(){};
                    targets.InsertArrayElementAtIndex(targets.arraySize);
                    targets.GetArrayElementAtIndex(targets.arraySize - 1).objectReferenceValue = oa;
                    AddEditor(targets.arraySize - 1);
                }
            }
        }

        void AddFromTemplateConfig()
        {
            if (memberNum2Add > 0)
            {
                string[] filter = { "Config files", "asset", "All files", "*" };
                string lastSelectDir = EditorPrefs.GetString(EditorPrefsKeyConst.lastSelectConfigPath, "");
                if (!Directory.Exists(lastSelectDir))
                {
                    lastSelectDir = Application.dataPath;
                }
                string path = EditorUtility.OpenFilePanelWithFilters("选择模版文件",lastSelectDir,filter);
                if (!string.IsNullOrEmpty(path))
                {
                    string newPath = path.Replace(Application.dataPath, "Assets");
                    var obj = AssetDatabase.LoadAssetAtPath<BaseObjectAnimConfig>(newPath);
                    EditorPrefs.SetString(EditorPrefsKeyConst.lastSelectConfigPath,Directory.GetParent(path).FullName);
                    for (int i = 0; i < memberNum2Add; i++)
                    {
                        var instance = ScriptableObject.Instantiate(obj);
                        CreateMemberFromConfigTemplate(instance,i + 1);
                    }
                }
            }

        }
        
        void AddFromTemplatePrefab()
        {
            if (memberNum2Add > 0)
            {
                string[] filter = { "Prefab files", "prefab", "All files", "*" };
                string lastSelectDir = EditorPrefs.GetString(EditorPrefsKeyConst.lastSelectPrefabPath, "");
                if (!Directory.Exists(lastSelectDir))
                {
                    lastSelectDir = Application.dataPath;
                }
                string path = EditorUtility.OpenFilePanelWithFilters("选择模版文件",lastSelectDir,filter);
                if (!string.IsNullOrEmpty(path))
                {
                    string newPath = path.Replace(Application.dataPath, "Assets");
                    var obj = AssetDatabase.LoadAssetAtPath<GameObject>(newPath);
                    EditorPrefs.SetString(EditorPrefsKeyConst.lastSelectPrefabPath,Directory.GetParent(path).FullName);
                    for (int i = 0; i < memberNum2Add; i++)
                    {
                        var instance = ScriptableObject.Instantiate(obj);
                        CreateMemberFromPrefabTemplate(instance);
                    }
                }
            }

        }

        void CreateMemberFromConfigTemplate(BaseObjectAnimConfig config,int index)
        {
            GameObject go = new GameObject($"anim_{index}");
            go.transform.SetParent((target as ObjectAnimationComponentGroup).transform);
            go.transform.localPosition = Vector3.one;
            var oa = go.AddComponent<ObjectAnimationComponent>();
            oa.modulesConfigs = new List<BaseObjectAnimConfig>(){config};
            targets.InsertArrayElementAtIndex(targets.arraySize);
            targets.GetArrayElementAtIndex(targets.arraySize - 1).objectReferenceValue = oa;
            AddEditor(targets.arraySize - 1);
        }
        
        void CreateMemberFromPrefabTemplate(GameObject go)
        {
            go.transform.SetParent((target as ObjectAnimationComponentGroup).transform);
            go.transform.localPosition = Vector3.one;
            var oa = go.GetComponent<ObjectAnimationComponent>();
            if (!oa)
            {
                oa = go.AddComponent<ObjectAnimationComponent>();
            }
            InstenciateOAConfigs(oa);
            targets.InsertArrayElementAtIndex(targets.arraySize);
            targets.GetArrayElementAtIndex(targets.arraySize - 1).objectReferenceValue = oa;
            AddEditor(targets.arraySize - 1);
        }

        /// <summary>
        /// 替换配置文件，使与原对象的配置不是同一个
        /// </summary>
        void InstenciateOAConfigs(ObjectAnimationComponent oa)
        {
            if (oa != null)
            {
                for (int i = 0; i < oa.modulesConfigs.Count; i++)
                {
                    oa.modulesConfigs[i] = Instantiate(oa.modulesConfigs[i]);
                }
            }
        }
        

        public ObjectAnimationComponentEditorState state { get; set; }
        public void OnCustomSceneGUI()
        {
            
        }
        
        public void OnSceneGUI()
        {
            if (editors != null)
            {
                for (int i = 0; i < editors.Count; i++)
                {
                    if (editors[i] is IObjectAnimationEditor editor)
                    {
                        editor.OnCustomSceneGUI();
                    }
                }
            }
        }
        
        public IObjectAnimationEditor hostEditor { get; set; }
        public bool show { get; set; }

        bool IsPreviewing()
        {
            return state == ObjectAnimationComponentEditorState.Preview || state == ObjectAnimationComponentEditorState.PreviewFrame;
        }
        
        private void OnDisable()
        {
            DoClean();
        }

        private void OnDestroy()
        {
            DoClean();
        }

        void DoClean()
        {
            EditorApplication.update -= OnEditorUpdate;
            if (editors != null)
            {
                for (int i = 0; i < editors.Count; i++)
                {
                    if (editors[i])
                    {
                        DestroyImmediate(editors[i]);
                    }
                }
                editors.Clear();
            }
        }
        
        public IObjectAnimationEditor GetCurEditConfigEditor()
        {
            if (editors != null)
            {
                for (int i = 0; i < editors.Count; i++)
                {
                    if (editors[i] is IObjectAnimationEditor editor)
                    {
                        if (editor.state == ObjectAnimationComponentEditorState.EditConfig)
                        {
                            return editor;
                        }
                    }
                }
            }

            return null;
        }

        public void QuitEditMode()
        {
            for (int i = 0; i < editors.Count; i++)
            {
                if (editors[i] is IObjectAnimationEditor editor)
                {
                    editor.QuitEditMode();
                }
            }
        }

        public void EnterEditMode()
        {
            for (int i = 0; i < editors.Count; i++)
            {
                if (editors[i] is IObjectAnimationEditor editor)
                {
                    editor.EnterEditMode();
                }
            }
        }

        public bool GetIsUsePreviewStartEndValue()
        {
            return false;
        }

        public object GetPreviewStartValue()
        {
            return null;
        }

        public object GetPreviewEndValue()
        {
            return null;
        }

        public object GetPreviewBaseValue()
        {
            return null;
        }
    }
}