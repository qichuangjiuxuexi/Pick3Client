using System;
using System.Collections.Generic;
using AppBase.OA;
using AppBase.OA.Configs;
using AppBase.OA.Editor;
using AppBase.OA.WorldPositionMoveModule.Editor;
using UnityEditor;
using UnityEngine;
using PathType = AppBase.OA.Configs.PathType;

[CustomEditor(typeof(WorldPositionMoveConfig))]
public class WorldPositionMoveConfigEditor : Editor,IObjectAnimationEditor
{
    private bool useConfigValue = true;
    //基类属性：
    private SerializedProperty enabled;
    SerializedProperty description;
    private SerializedProperty baseDuration;
    SerializedProperty behaviourType;
    SerializedProperty updateType;
    SerializedProperty duration;
    SerializedProperty speedCurve;
    SerializedProperty startValue;
    SerializedProperty endValue;
    SerializedProperty timeScale;
    SerializedProperty dynamicDurationCorrectStrategy;
    SerializedProperty delayTime;


    SerializedProperty pathType = null;
    SerializedProperty autoPathWeights = null;
    SerializedProperty autoPathOffsetForVertical = null;
    SerializedProperty autoPathOffsetForHorizt = null;
    SerializedProperty maxAngleTreatAsVertical = null;
    SerializedProperty maxAngleTreatAsHorizontal = null;
    SerializedProperty wps = null;
    SerializedProperty autoPathStrategy = null;
    private SerializedProperty correctPathStrategy = null;
    SerializedProperty strictWpsCurves = null;
    private bool areaHow2DealHorizontalOrVertical = false;
    private Vector3 basePosition;
    
    private GameObject editTarget = null;
    private bool isLeftCtrlKeep;
    private bool isLeftShiftKeep;
    private bool isLeftAltKeep;
    

    private List<AnimationPathPoint> points;

    private Vector3 addPostion;
    private double lastEditPointTime;
    private AnimationPathPoint removeTarget;
    private GUIStyle labelStyle;
    // private GUIStyle btnEnableStyle;
    // private GUIStyle btnDisableStyle;

    private bool showPath = true;
    private bool showLabel;
    // private static GUISkin skin;
    
    private Vector3 previewStartValue;
    private Vector3 previewEndValue;


    private void OnEnable()
    {
        enabled = serializedObject.FindProperty("enabled");
        description = serializedObject.FindProperty("description");
        baseDuration = serializedObject.FindProperty("baseDuration");
        pathType = serializedObject.FindProperty("pathType");
        autoPathWeights = serializedObject.FindProperty("autoPathWeights");
        autoPathOffsetForVertical = serializedObject.FindProperty("autoPathOffsetForVertical");
        autoPathOffsetForHorizt = serializedObject.FindProperty("autoPathOffsetForHorizt");
        maxAngleTreatAsVertical = serializedObject.FindProperty("maxAngleTreatAsVertical");
        maxAngleTreatAsHorizontal = serializedObject.FindProperty("maxAngleTreatAsHorizontal");
        wps = serializedObject.FindProperty("wps");
        autoPathStrategy = serializedObject.FindProperty("autoPathStrategy");

        startValue = serializedObject.FindProperty("startValue");
        endValue = serializedObject.FindProperty("endValue");

        // behaviourType = serializedObject.FindProperty("behaviourType");
        updateType = serializedObject.FindProperty("updateType");
        duration = serializedObject.FindProperty("duration");
        speedCurve = serializedObject.FindProperty("speedCurve");
        timeScale = serializedObject.FindProperty("timeScale");
        dynamicDurationCorrectStrategy = serializedObject.FindProperty("dynamicDurationCorrectStrategy");
        delayTime = serializedObject.FindProperty("delayTime");
        correctPathStrategy = serializedObject.FindProperty("correctPathStrategy");
        strictWpsCurves = serializedObject.FindProperty("strictWpsCurves");
        labelStyle = new GUIStyle();
        labelStyle.normal.textColor = Color.white;
        labelStyle.fontStyle = FontStyle.BoldAndItalic;
        // labelStyle.fontSize = 15;
        // btnEnableStyle = new GUIStyle();
        // btnEnableStyle.normal.background = Texture2D.whiteTexture;
        //
        // btnDisableStyle = new GUIStyle();
        // btnDisableStyle.normal.background = Texture2D.grayTexture;
        SceneView.duringSceneGui += OnSceneGUI;
        // if (skin == null)
        // {
        //     skin = AssetDatabase.LoadAssetAtPath<GUISkin>("Assets/Project/AppBase/ObjectAnimationSystem/Skin/ObjectAnimationSkin.guiskin");
        // }
    }

    private void OnDisable()
    {
        SceneView.duringSceneGui -= OnSceneGUI;
        DoClean();
    }

    public void OnDestroy()
    {
        DoClean(); 
    }

    void DoClean()
    {
        handPositions?.Clear();
        DestroyPoints();
        SetState(ObjectAnimationComponentEditorState.None,true);
    }
    

    public override void OnInspectorGUI()
    {
        serializedObject.Update();
        DrawPreviewValues();
        EditorGUILayout.PropertyField(description, EditorConstGUIContent.ConfigDes);
        EditorGUILayout.PropertyField(enabled,EditorConstGUIContent.ConfigEnabled);
        EditorGUILayout.PropertyField(baseDuration,EditorConstGUIContent.IsBaseDuration);
        showPath = EditorGUILayout.Toggle(EditorConstGUIContent.ConfigShowPath,showPath);
        showLabel = EditorGUILayout.Toggle(EditorConstGUIContent.ConfigShowWps,showLabel);
        if (enabled.boolValue)
        {
            EditorGUILayout.PropertyField(updateType);
            EditorGUILayout.Space();
            EditorGUILayout.PropertyField(pathType);
            EditorGUILayout.HelpBox(EditorConstGUIContent.ConfigPathType, true);
            EditorGUILayout.Space();
            // EditorGUILayout.PropertyField(behaviourType);
            EditorGUILayout.PropertyField(duration, EditorConstGUIContent.ConfigDuration);
            if (duration.floatValue <= 0)
            {
                EditorGUILayout.HelpBox("动画时长为0，请修正！",MessageType.Error);
            }
            EditorGUILayout.PropertyField(speedCurve, EditorConstGUIContent.ConfigCurve);
            // if (speedCurve.animationCurveValue.length == 0)
            // { 
            //     EditorGUILayout.HelpBox("缓动曲线设置错误，请修正！",MessageType.Error);
            // }
            EditorGUILayout.PropertyField(timeScale, EditorConstGUIContent.ConfigTimeScale);
            if (timeScale.floatValue <= 0)
            {
                EditorGUILayout.HelpBox("timeScale小于等于0，注意可能产生错误！",MessageType.Warning);
            }
            EditorGUILayout.PropertyField(dynamicDurationCorrectStrategy,EditorConstGUIContent.ConfigDurationCorrect );
            EditorGUILayout.PropertyField(delayTime, EditorConstGUIContent.ConfigDelay);
            basePosition = EditorGUILayout.Vector3Field("基础位置",basePosition);
            EditorGUILayout.HelpBox("基础位置全为0时，所有坐标相当于世界坐标，否则相当于相对坐标",MessageType.Info);
            if (pathType.enumValueIndex == (int) PathType.Auto)
            {
                EditorGUILayout.Space();
                EditorGUILayout.PropertyField(autoPathStrategy);
                EditorGUILayout.HelpBox(EditorConstGUIContent.ConfigHelpAutoStrategy);
                EditorGUILayout.Space();

                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.PropertyField(startValue, EditorConstGUIContent.ConfigStartVal);
                if (hostEditor != null && hostEditor is ObjectAnimationComponentEditor host && host.target is ObjectAnimationComponent comp)
                {
                    if (GUILayout.Button("<<",GUILayout.MaxWidth(30)))
                    {
                        startValue.vector3Value = comp.gameObject.transform.position;
                    }
                    if (GUILayout.Button(">>",GUILayout.MaxWidth(30)))
                    {
                        comp.gameObject.transform.position = startValue.vector3Value;
                    }
                }

                EditorGUILayout.EndHorizontal();
                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.PropertyField(endValue, EditorConstGUIContent.ConfigEndVal);
                if (hostEditor != null && hostEditor is ObjectAnimationComponentEditor host2 && host2.target is ObjectAnimationComponent comp2)
                {
                    if (GUILayout.Button("<<",GUILayout.MaxWidth(30)))
                    {
                        endValue.vector3Value = comp2.gameObject.transform.position;
                    }
                    if (GUILayout.Button(">>",GUILayout.MaxWidth(30)))
                    {
                        comp2.gameObject.transform.position = endValue.vector3Value;
                    }
                }
                EditorGUILayout.EndHorizontal();
                bool isVertical = false;
                bool isHor = false;
                if (autoPathStrategy.objectReferenceValue is BaseWorldPostionMoveAutoPathStrategy strategy)
                {
                    WorldPositionMoveConfig realConfig = target as WorldPositionMoveConfig;
                    isVertical = strategy.IsVertical(realConfig,realConfig.startValue,realConfig.endValue);
                    if (isVertical)
                    {
                        EditorGUILayout.HelpBox("当前始末点连线视为与y轴平行",MessageType.Warning);
                    }
                    else
                    {
                        isHor = strategy.IsHorizontal(realConfig,realConfig.startValue,realConfig.endValue);
                        if (isHor)
                        {
                            EditorGUILayout.HelpBox("当前始末点连线视为与x轴平行",MessageType.Warning);
                        }
                    }
                    DrawAutoPathHVArea(isVertical,isHor);
                }
                EditorGUILayout.Space();
                if (!isHor && !isVertical)
                {
                    EditorGUILayout.PropertyField(autoPathWeights, EditorConstGUIContent.ConfigAutoWeight);
                    EditorGUILayout.HelpBox(EditorConstGUIContent.ConfigHelpAutoWeight, true);
                    EditorGUILayout.Space();
                }
            }
            else
            {
                EditorGUILayout.PropertyField(wps);
                if (pathType.enumValueIndex == (int)PathType.StrictWPs)
                {
                    EditorGUILayout.HelpBox("StrictWPs模式下各段路径的缓动曲线,暂未实现功能",MessageType.Info);
                    EditorGUILayout.PropertyField(strictWpsCurves);
                }
                EditorGUILayout.PropertyField(correctPathStrategy);
                EditorGUILayout.HelpBox(EditorConstGUIContent.ConfigHelpAdaptStartEnd);
            }
            
            if (pathType.enumValueIndex != (int)PathType.Auto)
            {
                EditorGUILayout.BeginHorizontal();
                if (state == ObjectAnimationComponentEditorState.None)
                {
                    if (GUILayout.Button("清除路径（Wps）"))
                    {
                        wps.ClearArray();
                        points?.Clear();
                        handPositions?.Clear();
                    }
                    if (GUILayout.Button("编辑路径（Wps）"))
                    {
                        EnterEditMode();
                    }
                }

                if (state == ObjectAnimationComponentEditorState.EditConfig)
                {
                    EditorGUILayout.HelpBox("Mac电脑：增加点快捷键是：Command + Shift + 鼠标右击,删除点的快捷键是:Command + Alt +右击要删除的点\n" +
                                            "Windows电脑：增加点快捷键是：Ctrl + Shift + 鼠标右击,删除点的快捷键是:Ctrl + Alt +右击要删除的点",MessageType.Info);
                    if (GUILayout.Button("退出编辑"))
                    {
                        QuitEditMode();
                    }
                }
                EditorGUILayout.EndHorizontal();
            }
            else
            {
                if (state == ObjectAnimationComponentEditorState.EditConfig)
                {
                    if (GUILayout.Button("退出编辑"))
                    {
                        QuitEditMode();
                    }
                }
                else if(state == ObjectAnimationComponentEditorState.None)
                {
                    if (GUILayout.Button("编辑中间点"))
                    {
                        EnterEditMode();
                    }
                }
            }
        }

        // }
        serializedObject.ApplyModifiedProperties();
    }
    
    void DrawPreviewValues()
    {
        useConfigValue = EditorGUILayout.Toggle("是否强制使用配置中的位置",useConfigValue);
        if (useConfigValue)
        { 
            return;
        }
        EditorGUILayout.LabelField("设置预览用的始末位置、原点位置：");
        EditorGUILayout.BeginHorizontal();
        previewStartValue = EditorGUILayout.Vector3Field("预览用的起始位置", previewStartValue);
        DrawApplyCurPositionToTargetStart();
        DrawApplyCurPositionToPreviewValueStart();
        EditorGUILayout.EndHorizontal();
        EditorGUILayout.BeginHorizontal();
        previewEndValue = EditorGUILayout.Vector3Field("预览用的最终位置", previewEndValue);
        DrawApplyCurPositionToTargetEnd();
        DrawApplyCurPositionToPreviewValueEnd();
        EditorGUILayout.EndHorizontal();
        EditorGUILayout.BeginHorizontal();
        basePosition = EditorGUILayout.Vector3Field("基础位置",basePosition);
        if (GUILayout.Button("归0",GUILayout.MaxWidth(30)))
        {
            basePosition = Vector3.zero;
        }
        EditorGUILayout.EndHorizontal();
    }
    
    void DrawApplyCurPositionToTargetStart()
    {
        if (GUILayout.Button(">>",GUILayout.MaxWidth(30)))
        {
            if (hostEditor != null)
            {
                var editor = hostEditor as Editor;
                var mono = editor.target as MonoBehaviour;
                SetPosition(mono.transform,previewStartValue);
            }
        }
    }
    
    void DrawApplyCurPositionToPreviewValueStart()
    {
        if (GUILayout.Button("<<",GUILayout.MaxWidth(30)))
        {
            if (hostEditor != null)
            {
                var editor = hostEditor as Editor;
                var mono = editor.target as MonoBehaviour;
                previewStartValue = GetPosition(mono.transform);
            }
        }
    }

    Vector3 GetPosition(Transform trf)
    {
        return trf.position;
    }
    
    void SetPosition(Transform trf,Vector3 value)
    {
        trf.position = value;
    }
    
    void DrawApplyCurPositionToTargetEnd()
    {
        if (GUILayout.Button(">>",GUILayout.MaxWidth(30)))
        {
            if (hostEditor != null)
            {
                var editor = hostEditor as Editor;
                var mono = editor.target as MonoBehaviour;
                SetPosition(mono.transform,previewEndValue);
            }
        }
    }

    void DrawApplyCurPositionToPreviewValueEnd()
    {
        if (GUILayout.Button("<<",GUILayout.MaxWidth(30)))
        {
            if (hostEditor != null)
            {
                var editor = hostEditor as Editor;
                var mono = editor.target as MonoBehaviour;
                previewEndValue = GetPosition(mono.transform);
            }
        }
    }

    void DrawAutoPathHVArea(bool isVertical,bool isHor)
    {
        if (isVertical || isHor)
        {
            areaHow2DealHorizontalOrVertical =
                EditorGUILayout.BeginFoldoutHeaderGroup(areaHow2DealHorizontalOrVertical, EditorConstGUIContent.ConfigHVStrategy);
        }
        if (areaHow2DealHorizontalOrVertical)
        {
            if (isVertical)
            {
                EditorGUILayout.PropertyField(maxAngleTreatAsVertical);
                EditorGUILayout.HelpBox(EditorConstGUIContent.ConfigHelpAsVertical);
                EditorGUILayout.Space();
                EditorGUILayout.PropertyField(autoPathOffsetForVertical);
                EditorGUILayout.HelpBox(EditorConstGUIContent.ConfigHelpVerticalStrategy);
                EditorGUILayout.Space();
            }

            if (isHor)
            {
                EditorGUILayout.PropertyField(maxAngleTreatAsHorizontal);
                EditorGUILayout.HelpBox(EditorConstGUIContent.ConfigHelpAsHor);
                EditorGUILayout.Space();
                EditorGUILayout.PropertyField(autoPathOffsetForHorizt);
                EditorGUILayout.HelpBox(EditorConstGUIContent.ConfigHelpHorStrategy);
            }
        }

        EditorGUILayout.EndFoldoutHeaderGroup();
    }

    void InitWps()
    {
        if (wps.arraySize == 0)
        {
            wps.InsertArrayElementAtIndex(0);
            if (hostEditor != null && hostEditor is ObjectAnimationComponentEditor host)
            {
                wps.GetArrayElementAtIndex(0).vector3Value =  (host.target as ObjectAnimationComponent).gameObject.transform
                    .position; 
            }
        }
    }
    
    void InitSME()
    {

    }

    void SetState(ObjectAnimationComponentEditorState state,bool syncHost = false)
    {
        this.state = state;
        if (syncHost && hostEditor != null)
        {
            hostEditor.state = state;
        }
    }

    void PrepareEditEnv()
    {
        ShowPoints();
    }

    GameObject GetEditTarget()
    {
        const string editTargetName = "DELETE ME AFTER USING";
        editTarget = GameObject.Find(editTargetName);
        if (editTarget == null)
        {
            editTarget = new GameObject(editTargetName);
            editTarget.hideFlags = GetHideFlags();
        }

        return editTarget;
    }

    HideFlags GetHideFlags() 
    {
        return HideFlags.DontSaveInEditor | HideFlags.HideInInspector | HideFlags.NotEditable | HideFlags.HideInHierarchy;
        // return HideFlags.None;
    }
    
    void DestroyEditEnv()
    {
        DoClean();
    }
    
    void RemovePoint()
    {
        for (int i = 0; i < points.Count; i++) 
        {
            if (points[i] == removeTarget)
            {
                DestroyImmediate(points[i].gameObject);
                points.RemoveAt(i);
                wps.DeleteArrayElementAtIndex(i);
                if (handPositions != null && handPositions.Count > i)
                {
                    handPositions.RemoveAt(i);
                }
                break;
            }
        }
        removeTarget = null;
        RefreshPoints();
        serializedObject.ApplyModifiedProperties();
    }

    private bool CanEdit()
    {
        return EditorApplication.timeSinceStartup - lastEditPointTime > 0.2;
    }

    private void OnSceneGUI(SceneView view)
    {
        CheckEditOp(view);
        DrawDebugThings();
    }

    void CheckEditOp(SceneView view)
    {
        if (state != ObjectAnimationComponentEditorState.EditConfig || IsAutoPath())
        {
            return;
        } 
        Event e = Event.current;
        if (e != null)
        {
            if (IsAdd(e) && IsMouseOpValid(e))
            {
                if (CanEdit())
                {
                    addPostion = e.mousePosition;
                    var screenPoint = HandleUtility.GUIPointToScreenPixelCoordinate(addPostion);
                    var worldPos = view.camera.ScreenToWorldPoint(screenPoint);
                    AddPoint(worldPos);
                    lastEditPointTime = EditorApplication.timeSinceStartup;
                }

            }
            else if (IsRemove(e) && IsMouseOpValid(e))
            {
                if (CanEdit())
                {
                    Ray ray = HandleUtility.GUIPointToWorldRay(e.mousePosition);
                    RaycastHit hit;
                    if (Physics.Raycast(ray, out hit))
                    {
                        if (hit.collider != null)
                        {
                            removeTarget = hit.collider.GetComponent<AnimationPathPoint>();
                            lastEditPointTime = EditorApplication.timeSinceStartup;
                            RemovePoint();
                        }
                    }
                }
            }
        }
    }

    void DrawLineAndPoint()
    {
        try
        {
            if (AllowShowPath())
            {
                serializedObject?.Update();
                if (pathType.enumValueIndex == (int) PathType.Auto)
                {
                    if (autoPathStrategy.objectReferenceValue is BaseWorldPostionMoveAutoPathStrategy strategy)
                    {
                        var realConfig = target as WorldPositionMoveConfig;
                        List<Vector3> points = strategy.GetAutoPathCtrlPoints(realConfig,realConfig.startValue,realConfig.endValue);
                        for (int i = 0; i < points.Count - 1; i++)
                        {
                            Handles.DrawLine(points[i],points[i + 1],0);
                        }
                    }
                }
                else
                {
                    for (int i = 0; i < wps.arraySize - 1; i++)
                    {
                        Handles.DrawLine(wps.GetArrayElementAtIndex(i).vector3Value,wps.GetArrayElementAtIndex(i + 1).vector3Value,0);
                    }
                }
            }
        }
        catch (Exception e)
        {

        }
    }

    bool AllowShowPath()
    {
        return showPath || state == ObjectAnimationComponentEditorState.EditConfig;
    }

    bool AllowShowPathPointInfo()
    {
        return showLabel || state == ObjectAnimationComponentEditorState.EditConfig;
    }
    GameObject GetNewPoints()
    {
        var go = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        SphereCollider collider = go.GetComponent<SphereCollider>();
        MeshRenderer render = go.GetComponent<MeshRenderer>();
        render.sharedMaterial.SetColor("_BaseColor",Color.red);
        collider.radius = 0.5f;
        go.hideFlags = GetHideFlags();
        go.AddComponent<AnimationPathPoint>();
        for (int i = 0; i < go.transform.childCount; i++)
        {
            go.transform.GetChild(i).gameObject.hideFlags = GetHideFlags();
        }
        return go;
    }

    void ShowPoints()
    {
        points ??= new List<AnimationPathPoint>();
        if (IsAutoPath())
        {
            if (autoPathStrategy.objectReferenceValue is BaseWorldPostionMoveAutoPathStrategy strategy)
            {
                var realConfig = target as WorldPositionMoveConfig;
                var pointsPos = strategy.GetAutoPathCtrlPoints(realConfig,realConfig.startValue,realConfig.endValue);
                for (int i = 0; i < pointsPos.Count; i++)
                {
                    var go = GetNewPoints();
                    go.transform.position = pointsPos[i];
                    go.transform.SetParent(GetEditTarget().transform,true);
                    var goPath = go.GetComponent<AnimationPathPoint>();
                    points.Add(goPath);
                }
            }
        }
        else
        {
            for (int i = 0; i < wps.arraySize; i++)
            {
                var point = wps.GetArrayElementAtIndex(i);
                var go = GetNewPoints();
                go.transform.position = point.vector3Value;
                go.transform.SetParent(GetEditTarget().transform,true);
                var pathPoint = go.GetComponent<AnimationPathPoint>();
                points.Add(pathPoint);
            }
        }

    }
    
    bool IsAutoPath()
    {
        return pathType.enumValueIndex == (int) PathType.Auto;
    }
    
    void RefreshPoints()
    {
        if (IsAutoPath())
        {
            return;
        }
        for (int i = 0; i < wps.arraySize; i++)
        {
            var point = wps.GetArrayElementAtIndex(i);
            if (i >= points.Count)
            {
                return;
            }
            var go = points[i];
            go.transform.position = point.vector3Value;
            go.transform.SetParent(GetEditTarget().transform,true);
        }
    }

    void AddPoint(Vector3 point)
    {
        if (IsAutoPath())
        {
            return;
        }
        float z = GetEditTarget().transform.position.z;
        if (points.Count > 0)
        {
            z = points[^1].transform.position.z;
        }

        point.z = z;
        wps.InsertArrayElementAtIndex(wps.arraySize);
        wps.GetArrayElementAtIndex(wps.arraySize - 1).vector3Value = point;
        points ??= new List<AnimationPathPoint>();
        var pointGo = GetNewPoints();
        var pointPath = pointGo.GetComponent<AnimationPathPoint>();
        pointGo.transform.SetParent(GetEditTarget().transform);
        pointGo.transform.position = point;
        points.Add(pointPath);
        serializedObject.ApplyModifiedProperties();
    }

    void DestroyPoints()
    {
        points?.Clear();
        if (GetEditTarget())
        {
            DestroyImmediate(GetEditTarget());
        }
    }
    

    bool IsAdd(Event e)
        {
#if UNITY_EDITOR_OSX
            return e.command && e.shift;
#else
        return e.control && e.shift;
#endif
        }

        bool IsRemove(Event e)
        {
#if UNITY_EDITOR_OSX
            return e.command && e.alt;
#else
        return e.control && e.alt;
#endif
        }

        bool IsMouseOpValid(Event e)
        {
            return e.isMouse && e.type == EventType.MouseDown && e.button == 1;
        }

    public ObjectAnimationComponentEditorState state { get; set; }
    
    private List<Vector3> handPositions;
    public void OnCustomSceneGUI()
    {
       
    }

    void InitHandlePosition()
    {
        if (IsAutoPath())
        {
            if (handPositions == null || handPositions.Count != 3)
            {
                handPositions = new List<Vector3>(3);
                for (int i = 0; i < points.Count; i++)
                {
                    handPositions.Add(points[i].transform.position);
                }
            }
        }
        else
        {
            if (handPositions == null || handPositions.Count != wps.arraySize)
            {
                handPositions = new List<Vector3>(wps.arraySize);
                for (int i = 0; i < wps.arraySize; i++)
                {
                    handPositions.Add(wps.GetArrayElementAtIndex(i).vector3Value);
                }
            }
        }
    }

    public void DrawDebugThings()
    {
        bool allowOp = state == ObjectAnimationComponentEditorState.EditConfig;
        if (allowOp)
        {
            serializedObject.Update();
            if (points == null || points.Count == 0)
            {
                return;
            }
            InitHandlePosition();
            EditorGUI.BeginChangeCheck();
            for (int i = 0; i < handPositions.Count; i++)
            {
                float distance = 0;
                if (i > 0)
                {
                    distance = Vector3.Distance(handPositions[i], handPositions[i - 1]);
                }

                if (AllowShowPathPointInfo())
                {
                    Handles.Label(handPositions[i] + Vector3.up * 0.8f,$"{i}:{ distance }",labelStyle);
                }
                handPositions[i] = Handles.PositionHandle(handPositions[i], Quaternion.identity);
            }

            if (EditorGUI.EndChangeCheck())
            {
                if (!IsAutoPath())
                {
                    for (int i = 0; i < handPositions.Count; i++)
                    {
                        wps.GetArrayElementAtIndex(i).vector3Value = handPositions[i];
                        points[i].transform.position = handPositions[i];
                    }
                }
                else
                {
                    points[0].transform.position = handPositions[0];
                    points[1].transform.position = handPositions[1];
                    points[2].transform.position = handPositions[2];
                    startValue.vector3Value = handPositions[0];
                    endValue.vector3Value = handPositions[2];
                    var strategy = (autoPathStrategy.objectReferenceValue as BaseWorldPostionMoveAutoPathStrategy);
                    var realConfig = target as WorldPositionMoveConfig;
                    if (strategy.IsVertical(realConfig,realConfig.startValue,realConfig.endValue))
                    {
                        autoPathOffsetForVertical.vector2Value = strategy.GetAutoPathMiddlePointsWeight(target as BaseObjectAnimConfig, handPositions);
                    }else if (strategy.IsHorizontal(realConfig,realConfig.startValue,realConfig.endValue))
                    {
                        autoPathOffsetForHorizt.vector2Value = strategy.GetAutoPathMiddlePointsWeight(target as BaseObjectAnimConfig, handPositions);
                    }
                    else
                    {
                        autoPathWeights.vector2Value = strategy.GetAutoPathMiddlePointsWeight(target as BaseObjectAnimConfig, handPositions);
                    }
                }
            }
            serializedObject.ApplyModifiedProperties();
            SyncPositions();
        }
        DrawLineAndPoint();
    }

    /// <summary>
    /// Undo时同步一下
    /// </summary>
    void SyncPositions()
    {
        if (!IsAutoPath())
        {
            if (handPositions == null || handPositions.Count < wps.arraySize)
            {
                return;
            }
            
            if (points == null || points.Count < wps.arraySize)
            {
                return;
            }
            for (int i = 0; i < wps.arraySize; i++)
            {
                Vector3 pos = wps.GetArrayElementAtIndex(i).vector3Value;
                handPositions[i] = pos;
                if (points[i])
                {
                    points[i].transform.position = pos;
                }
                
            }
        }
        else
        {
            if (handPositions == null || handPositions.Count < 3)
            {
                return;
            }
            
            if (points == null || points.Count < 3)
            {
                return;
            }
            
            handPositions[0] = startValue.vector3Value;
            if (autoPathStrategy.objectReferenceValue is BaseWorldPostionMoveAutoPathStrategy strategy)
            {
                var realConfig = target as WorldPositionMoveConfig;
                
                var points = strategy.GetAutoPathCtrlPoints(realConfig,realConfig.startValue,realConfig.endValue);
                handPositions[1] = points[1];
            }

            handPositions[2] = endValue.vector3Value;
            if (points[0])
            {
                points[0].transform.position = handPositions[0];
            }

            if (points[1])
            {
                points[1].transform.position = handPositions[1];
            }

            if (points[2])
            {
                points[2].transform.position = handPositions[2];
            }
        }
    }
    

    public IObjectAnimationEditor hostEditor { get; set; }
    public bool show { get; set; }
    public IObjectAnimationEditor GetCurEditConfigEditor()
    {
        return null;
    }
    
    public void QuitEditMode()
    {
        SetState(ObjectAnimationComponentEditorState.None,true);
        DestroyEditEnv();
    }

    public void EnterEditMode()
    {
        if (!IsAutoPath())
        {
            SetState(ObjectAnimationComponentEditorState.EditConfig,true);
            InitWps();
            PrepareEditEnv();
            return;
        }
        SetState(ObjectAnimationComponentEditorState.EditConfig, true);
        InitSME();
        PrepareEditEnv();
    }

    public bool GetIsUsePreviewStartEndValue()
    {
        return !useConfigValue;
    }

    public object GetPreviewStartValue()
    {
        return previewStartValue;
    }

    public object GetPreviewEndValue()
    {
        return previewEndValue;
    }

    public object GetPreviewBaseValue()
    {
        return basePosition;
    }
}

