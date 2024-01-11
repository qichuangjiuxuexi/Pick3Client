using AppBase.OA.Configs;
using AppBase.OA.Editor;
using AppBase.OA.WorldPositionMoveModule.Editor;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(CompositionAxisPostitionConfig))]
public class CompositionAxisPositionConfigEditor : BaseOAConfigEditor,IObjectAnimationEditor
{
    SerializedProperty useSeperateAxis;
    SerializedProperty postionType;
    SerializedProperty xCurve;
    SerializedProperty yCurve;
    SerializedProperty zCurve;
    SerializedProperty combinedCurve;
    SerializedProperty foceUserConfigPos;
    private Vector3 previewStartValue;
    private Vector3 previewEndValue;
    private Vector3 basePosition;
    protected override void OnEnable()
    {
        base.OnEnable();
        useSeperateAxis = serializedObject.FindProperty("useSeperateAxis");
        postionType = serializedObject.FindProperty("postionType");
        xCurve = serializedObject.FindProperty("xCurve");
        yCurve = serializedObject.FindProperty("yCurve");
        zCurve = serializedObject.FindProperty("zCurve");
        foceUserConfigPos = serializedObject.FindProperty("foceUserConfigPos");
        combinedCurve = serializedObject.FindProperty("combinedCurve");
    }

    

    public override void OnInspectorGUI()
    {
        serializedObject.Update();
        base.OnInspectorGUI();
        if (enabled.boolValue)
        {
            EditorGUILayout.PropertyField(postionType,EditorConstGUIContent.ConfigPositionType);
            EditorGUILayout.HelpBox("使用世界坐标还是使用局部坐标，还是使用UI上的anchoredPostition。UI上建议使用anchoredPostition，其他建议使用世界坐标",MessageType.Info);
            EditorGUILayout.PropertyField(useSeperateAxis,EditorConstGUIContent.ConfigUseSeperateAxis);
            if (useSeperateAxis.boolValue)
            {
                EditorGUILayout.PropertyField(xCurve,EditorConstGUIContent.ConfigXAxis);
                EditorGUILayout.PropertyField(yCurve,EditorConstGUIContent.ConfigYAxis);
                EditorGUILayout.PropertyField(zCurve,EditorConstGUIContent.ConfigZAxis);  
            }
            else
            {
                EditorGUILayout.PropertyField(combinedCurve,EditorConstGUIContent.ConfigConbinedAxis);
            }

            EditorGUILayout.PropertyField(foceUserConfigPos,EditorConstGUIContent.ConfigForceConfigPosition);
            DrawPreviewValues();
        }

        // }
        serializedObject.ApplyModifiedProperties();
    }

    void DrawPreviewValues()
    {
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
        switch ((PositionType)postionType.enumValueIndex)
        {
            case PositionType.WorldPosition:
                return trf.position;
                break;
            case PositionType.LocalPosition:
                return trf.localPosition;
                break;
            case PositionType.AnchoredPosition:
                return (trf as RectTransform).anchoredPosition;
                break;
            default:
                return trf.position;
        }
    }
    
    void SetPosition(Transform trf,Vector3 value)
    {
        switch ((PositionType)postionType.enumValueIndex)
        {
            case PositionType.WorldPosition:
                trf.position = value;
                break;
            case PositionType.LocalPosition:
                trf.localPosition = value;
                break;
            case PositionType.AnchoredPosition:
                (trf as RectTransform).anchoredPosition = value;
                break;
            default:
                break;
        }
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

    public ObjectAnimationComponentEditorState state { get; set; }
    public void OnCustomSceneGUI()
    {
        
    }

    public IObjectAnimationEditor hostEditor { get; set; }
    public bool show { get; set; }
    public IObjectAnimationEditor GetCurEditConfigEditor()
    {
        return null;
    }

    public void QuitEditMode()
    {
        
    }

    public void EnterEditMode()
    {
        
    }

    public override bool GetIsUsePreviewStartEndValue()
    {
        return true;
    }

    public override object GetPreviewStartValue()
    {
        return previewStartValue;
    }

    public override object GetPreviewEndValue()
    {
        return previewEndValue;
    }

    public override object GetPreviewBaseValue()
    {
        return basePosition;
    }
}

