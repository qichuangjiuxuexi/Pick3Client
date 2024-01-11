using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using DG.DemiEditor;
using AppBase.OA;
using AppBase.OA.Behaviours;
using AppBase.OA.Editor;
using AppBase.OA.WorldPositionMoveModule.Editor;
using OfficeOpenXml.FormulaParsing.Excel.Functions.Logical;
using Project.AppBase.ObjectAnimationSystem.Scripts.Editor;
using UnityEditor;
using UnityEngine;


[CanEditMultipleObjects]
[CustomEditor(typeof(ObjectAnimationComponent))]
public class ObjectAnimationComponentEditor : Editor,IObjectAnimationEditor
{
    private Camera camera;
    private ObjectAnimationClip clip;
    private ObjectAnimationClip tmp;
    
    private SerializedProperty modulesConfigs;
    private SerializedProperty timeScale;
    private SerializedProperty autoPlayOnStart;
    private SerializedProperty delayTime;
    private SerializedProperty totalElapsedTime;
    private SerializedProperty animationName;
    private SerializedProperty useUnscaledDeltaTime;
    /// <summary>
    /// 配置的inpector gui内容
    /// </summary>
    private List<Editor> configEditors;
    /// <summary>
    /// 当前选中的配置类型，点击添加配置将添加此类型的配置
    /// </summary>
    private int currentSelectedIndex;
    private ObjectAnimationComponent curTarget;
    private string[] allConfigTypes;
    private Type[] configTypes;
    // private string[] allConfigDes;
    private bool isPreviewFrameByFrame = false;
    private int frameRate = 60;
    private void OnEnable()
    {
        if (Application.isPlaying)
        {
            return;
        }
        modulesConfigs = serializedObject.FindProperty("modulesConfigs");
        timeScale = serializedObject.FindProperty("timeScale");
        autoPlayOnStart = serializedObject.FindProperty("autoPlayOnStart");
        delayTime = serializedObject.FindProperty("delayTime");
        totalElapsedTime = serializedObject.FindProperty("totalElapsedTime");
        animationName = serializedObject.FindProperty("animationName");
        useUnscaledDeltaTime = serializedObject.FindProperty("useUnscaledDeltaTime");
        for (int i = 0; i < modulesConfigs.arraySize; i++)
        {
            if (modulesConfigs.GetArrayElementAtIndex(i).objectReferenceValue == null)
            {
                modulesConfigs.DeleteArrayElementAtIndex(i);
                i--;
            }
        }
        serializedObject.ApplyModifiedProperties();
        InitConfigEditors();
        EditorApplication.update += OnEditorUpdate;
        LoadAllConfigTypes();
        curTarget = target as ObjectAnimationComponent;
        curTarget.ForceInit();
    }

    void LoadAllConfigTypes()
    {
        if (OAEditorSettings.instance == null)
        {
            OAEditorSettings.CheckSettings();
            currentSelectedIndex = 0;
            allConfigTypes = Array.Empty<string>();
            return;
        }

        var asms = OAEditorSettings.instance.assemblys;
        List<Type> configs = new List<Type>();
        for (int i = 0; i < asms.Count; i++)
        {
            var assembly = Assembly.Load(asms[i]);
            if (assembly == null)
            {
                continue;
            }
            var types = assembly.GetTypes();
            var tmpList = Array.FindAll(types, x => x.IsSubclassOf(typeof(BaseObjectAnimConfig)) && !x.IsAbstract).ToList();
            configs.AddRange(tmpList);
        }
        configs.Sort((x, y) =>
        {
            var attriX = x.GetCustomAttribute<OAConfigAttribute>();
            var attriY = y.GetCustomAttribute<OAConfigAttribute>();
            if (attriX != null && attriY != null)
            {
                return attriX.sort.CompareTo(attriY.sort);
            }
            return 0;
        });
        configTypes = configs.ToArray();
        allConfigTypes = new string[configTypes.Length];
        for (int i = 0; i < configTypes.Length; i++) 
        {
            var attri = configTypes[i].GetCustomAttribute<OAConfigAttribute>();
            if (attri != null)
            {
                allConfigTypes[i] = (attri.displayName);
            }
            else
            {
                allConfigTypes[i] = configTypes[i].FullName;
            }
        }
    }

    void InitConfigEditors()
    {
        configEditors = new List<Editor>();
        for (int i = 0; i < modulesConfigs.arraySize; i++)
        {
            var obj = modulesConfigs.GetArrayElementAtIndex(i).objectReferenceValue;
            var editor = Editor.CreateEditor(obj);
            configEditors.Add(editor);
            if (editor is IObjectAnimationEditor cur)
            {
                cur.hostEditor = this;
            }
        }
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
        if (configEditors != null)
        {
            for (int i = 0; i < configEditors.Count; i++)
            {
                if (configEditors[i])
                {
                    DestroyImmediate(configEditors[i]);
                }
            }
            configEditors.Clear();
        }
    }

    private double lastUpdateTime = 0;

    void OnEditorUpdate()
    {
        if (IsPreviewing() && curTarget && hostEditor == null)
        {
            if (EditorApplication.timeSinceStartup - lastUpdateTime > 1 / (float)frameRate)
            {
                if (state == ObjectAnimationComponentEditorState.PreviewFrame)
                {
                    if (isPreviewFrameByFrame)
                    {
                        curTarget.SimuUpdateAll(1 / (float)frameRate);
                        isPreviewFrameByFrame = false;
                    }
                }
                else if(state == ObjectAnimationComponentEditorState.Preview)
                {
                    curTarget.SimuUpdateAll(1 / (float)frameRate);
                }

                lastUpdateTime = EditorApplication.timeSinceStartup;
            }
        }
    }

    bool IsPreviewing()
    {
        return state == ObjectAnimationComponentEditorState.Preview || state == ObjectAnimationComponentEditorState.PreviewFrame;
    }

    void DrawExportClip()
    {
        if (GUILayout.Button("导出全新的OAClip并编辑"))
        {
            if (curTarget == null || curTarget.modulesConfigs == null || curTarget.modulesConfigs.Count == 0)
            {
                EditorUtility.DisplayDialog("注意！", "没有有效的动画配置！", "OK");
            }
            else
            {
                serializedObject.ApplyModifiedProperties();
                var clip = ScriptableObject.CreateInstance<ObjectAnimationClip>();
                clip.animationConfigs = new List<BaseObjectAnimConfig>();
                clip.clipName = animationName.stringValue;
                for (int i = 0; i < modulesConfigs.arraySize; i++)
                {
                    var obj = modulesConfigs.GetArrayElementAtIndex(i).objectReferenceValue as BaseObjectAnimConfig;
                    string lastSavePath = EditorPrefs.GetString(EditorPrefsKeyConst.lastSaveConfigPath);
                    string path =
                        EditorUtility.SaveFilePanelInProject($"保存子动画配置({i + 1}/{modulesConfigs.arraySize}步)", $"{obj.GetType().Name}", "asset", "选择保存路径",lastSavePath);
                    if (!string.IsNullOrEmpty(path))
                    {
                        EditorPrefs.SetString(EditorPrefsKeyConst.lastSaveConfigPath,path);
                        var newObj = Instantiate(obj);
                        modulesConfigs.GetArrayElementAtIndex(i).objectReferenceValue = newObj;
                        var editor = Editor.CreateEditor(newObj);
                        if (editor is IObjectAnimationEditor tmp)
                        {
                            tmp.hostEditor = this;
                        }

                        if (configEditors[i] != null)
                        {
                            DestroyImmediate(configEditors[i]);
                        }
                        configEditors[i] = editor;
                        AssetDatabase.CreateAsset(newObj,path);
                        clip.animationConfigs.Add(newObj);
                    }
                    else
                    {
                        //一旦取消，不继续保存任何文件
                        return;
                    }
                }
                string lastSavePathNew = EditorPrefs.GetString(EditorPrefsKeyConst.lastSaveConfigPath);
                string pathNew =
                    EditorUtility.SaveFilePanelInProject("最后一步：保存Clip文件配置", "NewObjectAnimationCLip", "asset", "选择保存路径",lastSavePathNew);
                if (!string.IsNullOrEmpty(pathNew))
                {
                    EditorPrefs.SetString(EditorPrefsKeyConst.lastSaveConfigPath,pathNew);
                    AssetDatabase.CreateAsset(clip,pathNew);
                }

                for (int i = 0; i < configEditors.Count; i++)
                {
                    configEditors[i].serializedObject.Update();
                }
            }
        }
    }
    
    void DrawSetByClip()
    {
        if (curTarget != null && (curTarget.modulesConfigs == null || curTarget.modulesConfigs.Count == 0))
        {                             
            if (GUILayout.Button("从已有OAClip克隆并编辑"))
            {
                string[] filter = { "Config files", "asset", "All files", "*" };
                string lastSelectDir = EditorPrefs.GetString(EditorPrefsKeyConst.lastSelectConfigPath, "");
                if (!Directory.Exists(lastSelectDir))
                {
                    lastSelectDir = Application.dataPath;
                }
                string path = EditorUtility.OpenFilePanelWithFilters("选择OAClip文件",lastSelectDir,filter);
                if (!string.IsNullOrEmpty(path))
                {
                    string newPath = path.Replace(Application.dataPath, "Assets");
                    var obj = AssetDatabase.LoadAssetAtPath<ObjectAnimationClip>(newPath);
                    if (obj != null)
                    {
                        EditorPrefs.SetString(EditorPrefsKeyConst.lastSelectConfigPath,Directory.GetParent(path).FullName);
                        var instance = ScriptableObject.Instantiate(obj);
                        for (int i = 0; i < instance.animationConfigs.Count; i++)
                        {
                            if (instance.animationConfigs[i] != null)
                            {
                                var config = ScriptableObject.Instantiate(instance.animationConfigs[i]);
                                modulesConfigs.InsertArrayElementAtIndex(modulesConfigs.arraySize);
                                var editor = Editor.CreateEditor(config);
                                if (editor is IObjectAnimationEditor tmp)
                                {
                                    tmp.hostEditor = this;
                                }
                                modulesConfigs.GetArrayElementAtIndex(modulesConfigs.arraySize - 1).objectReferenceValue = config;
                                configEditors.Add(editor);
                            }
                        }
                        serializedObject.ApplyModifiedProperties();
                    }
                    curTarget.ForceInit();
                }
            }
            
            if (GUILayout.Button("打开已有OAClip进行编辑"))
            {
                string[] filter = { "Config files", "asset", "All files", "*" };
                string lastSelectDir = EditorPrefs.GetString(EditorPrefsKeyConst.lastSelectConfigPath, "");
                if (!Directory.Exists(lastSelectDir))
                {
                    lastSelectDir = Application.dataPath;
                }
                string path = EditorUtility.OpenFilePanelWithFilters("选择OAClip文件",lastSelectDir,filter);
                if (!string.IsNullOrEmpty(path))
                {
                    string newPath = path.Replace(Application.dataPath, "Assets");
                    var obj = AssetDatabase.LoadAssetAtPath<ObjectAnimationClip>(newPath);
                    if (obj != null)
                    {
                        EditorPrefs.SetString(EditorPrefsKeyConst.lastSelectConfigPath,Directory.GetParent(path).FullName);
                        for (int i = 0; i < obj.animationConfigs.Count; i++)
                        {
                            if (obj.animationConfigs[i] != null)
                            {
                                var config = obj.animationConfigs[i];
                                modulesConfigs.InsertArrayElementAtIndex(modulesConfigs.arraySize);
                                var editor = Editor.CreateEditor(config);
                                if (editor is IObjectAnimationEditor tmp)
                                {
                                    tmp.hostEditor = this;
                                }
                                modulesConfigs.GetArrayElementAtIndex(modulesConfigs.arraySize - 1).objectReferenceValue = config;
                                configEditors.Add(editor);
                            }
                        }
                        serializedObject.ApplyModifiedProperties();
                    }
                    curTarget.ForceInit();
                }
            }
        }
        
    }
    public override void OnInspectorGUI()
    {
        if (Application.isPlaying)
        {
            base.DrawDefaultInspector();
            return;
        }
        serializedObject.Update();
        EditorGUILayout.PropertyField(animationName,EditorConstGUIContent.ConfigAnimationName);
        EditorGUILayout.PropertyField(modulesConfigs, EditorConstGUIContent.ConfigAllParts,true);
        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.LabelField("设置要编辑或预览的动画配置列表");
        tmp = (ObjectAnimationClip)EditorGUILayout.ObjectField(tmp, typeof(ObjectAnimationClip));
        EditorGUILayout.EndHorizontal();
        if (clip != tmp && tmp != null)
        {
            clip = tmp;
            modulesConfigs.ClearArray();
            modulesConfigs.arraySize = clip.animationConfigs.Count;
            for (int i = 0; i < clip.animationConfigs.Count; i++)
            {
                modulesConfigs.GetArrayElementAtIndex(i).objectReferenceValue = clip.animationConfigs[i];
            }
            ClearConfigEditors();
            InitConfigEditors();
        }
        EditorGUILayout.BeginHorizontal();
        DrawExportClip();
        DrawSetByClip();
        EditorGUILayout.EndHorizontal();
        DrawDynamicObjectsSlots();
        EditorGUILayout.PropertyField(timeScale, EditorConstGUIContent.ConfigTimeScaleTimes, true);
        EditorGUILayout.PropertyField(autoPlayOnStart, EditorConstGUIContent.ConfigAutoPlay, true);
        EditorGUILayout.PropertyField(delayTime, EditorConstGUIContent.ConfigDelaySeconds, true);
        EditorGUILayout.Space(20);
        EditorGUILayout.BeginHorizontal();
        frameRate = EditorGUILayout.IntField("模拟帧率",frameRate);
        if (state == ObjectAnimationComponentEditorState.None)
        {
            if (GUILayout.Button("播放"))
            {
                if (!IsPreviewing())
                {
                    curTarget = serializedObject.targetObject as ObjectAnimationComponent;
                    curTarget.onfinished = () =>
                    {
                        state = ObjectAnimationComponentEditorState.None;
                        isPreviewFrameByFrame = false;
                        curTarget.onfinished = null;
                        Debug.Log("播放完毕！");
                    };
                    curTarget.ForceInit();
                    CheckCorrectPreviewStartEndValue();
                    curTarget.Play();
                    state = ObjectAnimationComponentEditorState.Preview;
                }
            }
            
            if (GUILayout.Button("逐帧播放"))
            {
                if (!IsPreviewing())
                {
                    curTarget = serializedObject.targetObject as ObjectAnimationComponent;
                    curTarget.onfinished = () =>
                    {
                        state = ObjectAnimationComponentEditorState.None;
                        isPreviewFrameByFrame = false;
                        curTarget.onfinished = null;
                        Debug.Log("播放完毕！");
                    };
                    curTarget.ForceInit();
                    CheckCorrectPreviewStartEndValue();
                    curTarget.Play();
                    state = ObjectAnimationComponentEditorState.PreviewFrame;
                }
            }
        }
        if (IsPreviewing())
        {
            if (GUILayout.Button("停止播放"))
            {
                state = ObjectAnimationComponentEditorState.None;
                isPreviewFrameByFrame = false;
                curTarget.onfinished = null;
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
            EditorGUILayout.HelpBox($"已播放时长:{totalElapsedTime.floatValue}",MessageType.Info);
        }
        EditorGUILayout.Space();
        EditorGUILayout.PropertyField(useUnscaledDeltaTime,EditorConstGUIContent.ConfigUseUnscaledDeltaTime); 
        EditorGUILayout.LabelField("以下是动画配置列表：",EditorStyleConst.BoldFontStyle);
        if (SyncConfigAndEditor())
        {
            EditorUtility.DisplayDialog("注意", "配置发生了变化，注意维护clip文件的正确性！", "OK");
        }
        DrawDurationCorrectPart();
        for (int i = 0; i < configEditors.Count; i++)
        {
            // EditorGUILayout.BeginVertical(splitStyle);
            EditorGUILayout.LabelField(
                "——————————————————————————————————————————————————————————————————————————————————————————————————————",
                EditorStyleConst.SplitLineStyle);
            EditorGUILayout.Space();
            configEditors[i].OnInspectorGUI();
            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button("删除此动画配置"))
            {
                if (EditorUtility.DisplayDialog("注意!!", "确定要删除吗，如果未导出保存过此配置，将丢失且不能找回!", "确定", "取消"))
                {
                    var target = configEditors[i];
                    configEditors.RemoveAt(i); 
                    modulesConfigs.DeleteArrayElementAtIndex(i);
                    if (curTarget)
                    {
                        curTarget.ForceInit();
                    }
                    i--; 
                    DestroyImmediate(target);
                    serializedObject.ApplyModifiedProperties();
                }
            }
            
            if (GUILayout.Button("保存"))
            {
                var obj = modulesConfigs.GetArrayElementAtIndex(i).objectReferenceValue;
                string lastSavePath = EditorPrefs.GetString(EditorPrefsKeyConst.lastSaveConfigPath);
                string path =
                    EditorUtility.SaveFilePanelInProject("保存配置", "NewObjectAnimConfig", "asset", "选择保存路径",lastSavePath);
                if (!string.IsNullOrEmpty(path))
                {
                    EditorPrefs.SetString(EditorPrefsKeyConst.lastSaveConfigPath,path);
                    AssetDatabase.CreateAsset(obj,path);
                }
            }
            EditorGUILayout.EndHorizontal(); 
            // EditorGUILayout.EndVertical();
        }
        EditorGUILayout.LabelField(
            "——————————————————————————————————————————————————————————————————————————————————————————————————————",
            EditorStyleConst.SplitLineStyle);
        EditorGUILayout.Space(60);
        currentSelectedIndex = EditorGUILayout.Popup(currentSelectedIndex,allConfigTypes);
        if (OAEditorSettings.instance != null && GUILayout.Button("添加上面选择的动画配置"))
        {
            Type type = configTypes[currentSelectedIndex];
            for (int i = 0; i < configEditors.Count; i++)
            {
                if (type == configEditors[i].serializedObject.targetObject.GetType())
                {
                    EditorUtility.DisplayDialog("错误", "已经挂载了此动画组件", "OK");
                    return;
                }
            }
            var instance = ScriptableObject.CreateInstance(configTypes[currentSelectedIndex]);
            modulesConfigs.InsertArrayElementAtIndex(modulesConfigs.arraySize);
            var editor = Editor.CreateEditor(instance);
            if (editor is IObjectAnimationEditor tmp)
            {
                tmp.hostEditor = this;
            }
            modulesConfigs.GetArrayElementAtIndex(modulesConfigs.arraySize - 1).objectReferenceValue = instance;
            configEditors.Add(editor);
            serializedObject.ApplyModifiedProperties();
            curTarget.ForceInit();
        }
        
        if (GUILayout.Button("选择配置模版添加动画配置"))
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
                var instance = ScriptableObject.Instantiate(obj);
                modulesConfigs.InsertArrayElementAtIndex(modulesConfigs.arraySize);
                var editor = Editor.CreateEditor(instance);
                if (editor is IObjectAnimationEditor tmp)
                {
                    tmp.hostEditor = this;
                }
                modulesConfigs.GetArrayElementAtIndex(modulesConfigs.arraySize - 1).objectReferenceValue = instance;
                configEditors.Add(editor);
                serializedObject.ApplyModifiedProperties();
                curTarget.ForceInit();
            }
        }

        for (int i = 0; i < configEditors.Count; i++)
        {
            configEditors[i].serializedObject.ApplyModifiedProperties();
        }

        serializedObject.ApplyModifiedProperties();
    }
    
    bool SyncConfigAndEditor()
    {
        if(configEditors.Count != modulesConfigs.arraySize)
        {
            OnConfigChanged();
            return true;
        }

        bool changed = false;
        for (int i = 0; i < modulesConfigs.arraySize; i++)
        {
            if (configEditors[i].target.GetInstanceID() !=
                modulesConfigs.GetArrayElementAtIndex(i).objectReferenceValue.GetInstanceID())
            {
                changed = true;
                Debug.Log("配置表发生了变化");
                break;
            }
        }

        if (changed)
        {
            OnConfigChanged();
        }

        return changed;
    }

    void OnConfigChanged()
    {
        ClearConfigEditors();
        InitConfigEditors();
        clip = null;
        tmp = null;
    }

    void ClearConfigEditors()
    {
        for (int i = configEditors.Count - 1; i >= 0 ; i--)
        {
            DestroyImmediate(configEditors[i]);
            configEditors.RemoveAt(i);
        }  
    }

    void CheckCorrectPreviewStartEndValue()
    {
        for (int i = 0; i < configEditors.Count; i++)
        {
            var config = configEditors[i].target as BaseObjectAnimConfig;
            if (configEditors[i] is IObjectAnimationEditor editor)
            {
                var bh = curTarget.GetBehaviour(config.behaviourType);
                bool usePreview = editor.GetIsUsePreviewStartEndValue();
                if (usePreview)
                {
                    CorrectPreviewValue(bh,editor);
                }
            }
        }
    }

    void CorrectPreviewValue(BaseAnimBehaviour bh,IObjectAnimationEditor editor)
    {
        if (bh is BaseAnimBehaviour<Vector3> bhVector3)
        {
            bhVector3.CorrectStart((Vector3)editor.GetPreviewStartValue());
            bhVector3.CorrectEnd((Vector3)editor.GetPreviewEndValue());
            bhVector3.CorrectBase((Vector3)editor.GetPreviewBaseValue());
        }else if (bh is  BaseAnimBehaviour<float> bhFloat)
        {
            bhFloat.CorrectStart((float)editor.GetPreviewStartValue());
            bhFloat.CorrectEnd((float)editor.GetPreviewEndValue());
            bhFloat.CorrectBase((float)editor.GetPreviewBaseValue());
        }else if (bh is  BaseAnimBehaviour<int> bhInt)
        {
            bhInt.CorrectStart((int)editor.GetPreviewStartValue());
            bhInt.CorrectEnd((int)editor.GetPreviewEndValue());
            bhInt.CorrectBase((int)editor.GetPreviewBaseValue());
        }else if (bh is  BaseAnimBehaviour<string> bhString)
        {
            bhString.CorrectStart((string)editor.GetPreviewStartValue());
            bhString.CorrectEnd((string)editor.GetPreviewEndValue());
            bhString.CorrectBase((string)editor.GetPreviewBaseValue());
        }
        else if (bh is  BaseAnimBehaviour<bool> bhBool)
        {
            bhBool.CorrectStart((bool)editor.GetPreviewStartValue());
            bhBool.CorrectEnd((bool)editor.GetPreviewEndValue());
            bhBool.CorrectBase((bool)editor.GetPreviewBaseValue());
        }
    }

    void DrawDurationCorrectPart()
    {
        if (curTarget == null || curTarget.modulesConfigs == null || curTarget.modulesConfigs.Count == 0)
        {
            return;
        }
        int baseDurationNum = 0;
        if (GUILayout.Button("与基准duration动画同步"))
        {
            float baseDuration = -1;
            for (int i = 0; i < curTarget.modulesConfigs.Count; i++)
            {
                if (curTarget.modulesConfigs[i].baseDuration)
                {
                    if (baseDuration < 0)
                    {
                        baseDuration = curTarget.modulesConfigs[i].duration;
                        break;
                    }
                }
            }

            if (baseDuration > 0)
            {
                for (int i = 0; i < curTarget.modulesConfigs.Count; i++)
                {
                    curTarget.modulesConfigs[i].duration = baseDuration;
                }
            }
            else
            {
                EditorUtility.DisplayDialog("注意！", "未指定基准时长动画或基准时长动画配置错误！","OK");
            }
            serializedObject.Update();
        }
        for (int i = 0; i < curTarget.modulesConfigs.Count; i++)
        {
            if (curTarget.modulesConfigs[i].baseDuration)
            {
                baseDurationNum ++;
            }
        }

        if (baseDurationNum > 1)
        {
            EditorGUILayout.HelpBox("发现多个基准时长动画！",MessageType.Error);
        }
        else if(baseDurationNum < 1)
        {
            EditorGUILayout.HelpBox("暂未指定基准时长动画！(如果指定了时长矫正策略，可能会产生问题)",MessageType.Warning);
        }
    }

    void DrawDynamicObjectsSlots()
    {
        var bh = curTarget.GetBehaviour<CameraSizeBehaviour>();
        if (bh != null)
        {
            camera = (Camera)EditorGUILayout.ObjectField(EditorConstGUIContent.ConfigSimuCamera, camera, typeof(Camera), true);
            bh.setSize = (size,progress)=>SetCameraSize(size,Vector3.zero, progress);
        }
    }

    void SetCameraSize(float size,Vector3 worldPos,float progress)
    {
        if (camera.orthographic)
        {
            camera.orthographicSize = size;
        }
        else
        {
            camera.fieldOfView = size;
        }
    }

    ObjectAnimationComponentEditorState _state;

    public ObjectAnimationComponentEditorState state
    {
        get { return _state;}
        set
        {
            _state = value;
            if (hostEditor != null)
            {
                hostEditor.state = value;
            }
        }
    }

    public void OnCustomSceneGUI()
    {
        
    }

    public IObjectAnimationEditor hostEditor { get; set; }
    public bool show { get; set; }
    public IObjectAnimationEditor GetCurEditConfigEditor()
    {
        if (configEditors != null)
        {
            for (int i = 0; i < configEditors.Count; i++)
            {
                if (configEditors[i] is IObjectAnimationEditor editor)
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
        for (int i = 0; i < configEditors.Count; i++)
        {
            if (configEditors[i] is IObjectAnimationEditor editor)
            {
                editor.QuitEditMode();
            }
        }
    }

    public void EnterEditMode()
    {
        for (int i = 0; i < configEditors.Count; i++)
        {
            if (configEditors[i] is IObjectAnimationEditor editor)
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

    public void OnSceneGUI()
    {
        if (configEditors != null)
        {
            for (int i = 0; i < configEditors.Count; i++)
            {
                if (configEditors[i] is IObjectAnimationEditor editor)
                {
                    editor.OnCustomSceneGUI();
                }
            }
        }
    }
}
