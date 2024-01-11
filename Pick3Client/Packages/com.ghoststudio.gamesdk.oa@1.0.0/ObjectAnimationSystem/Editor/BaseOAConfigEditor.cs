using AppBase.OA.Editor;
using AppBase.OA.WorldPositionMoveModule.Editor;
using UnityEditor;
using UnityEngine;

public abstract class BaseOAConfigEditor : Editor,IObjectAnimationEditor
{
    //基类属性：
   protected SerializedProperty enabled;
   protected SerializedProperty description;
   protected SerializedProperty baseDuration;
   protected SerializedProperty behaviourType;
   protected SerializedProperty updateType;
   protected SerializedProperty duration;
   protected SerializedProperty speedCurve;
   protected SerializedProperty startValue;
   protected SerializedProperty endValue;
   protected SerializedProperty timeScale;
   protected SerializedProperty dynamicDurationCorrectStrategy;
   protected SerializedProperty delayTime;

    protected GUIStyle labelStyle;
    // private GUIStyle btnEnableStyle;
    // private GUIStyle btnDisableStyle;

    protected bool showPath = true;
    protected bool showLabel;
    protected static GUISkin skin;


    protected virtual void OnEnable()
    {
        enabled = serializedObject.FindProperty("enabled");
        description = serializedObject.FindProperty("description");
        baseDuration = serializedObject.FindProperty("baseDuration");

        startValue = serializedObject.FindProperty("startValue");
        endValue = serializedObject.FindProperty("endValue");

        // behaviourType = serializedObject.FindProperty("behaviourType");
        updateType = serializedObject.FindProperty("updateType");
        duration = serializedObject.FindProperty("duration");
        speedCurve = serializedObject.FindProperty("speedCurve");
        timeScale = serializedObject.FindProperty("timeScale");
        dynamicDurationCorrectStrategy = serializedObject.FindProperty("dynamicDurationCorrectStrategy");
        delayTime = serializedObject.FindProperty("delayTime");
        

        labelStyle = new GUIStyle();
        labelStyle.normal.textColor = Color.blue;
        labelStyle.fontStyle = FontStyle.Italic;
        if (skin == null)
        {
            skin = AssetDatabase.LoadAssetAtPath<GUISkin>("Assets/Project/AppBase/ObjectAnimationSystem/Skin/ObjectAnimationSkin.guiskin");
        }
    }


    public override void OnInspectorGUI()
    {
        serializedObject.Update();
        EditorGUILayout.PropertyField(description, EditorConstGUIContent.ConfigDes);
        EditorGUILayout.PropertyField(enabled,EditorConstGUIContent.ConfigEnabled);
        EditorGUILayout.PropertyField(baseDuration,EditorConstGUIContent.IsBaseDuration);
        showPath = EditorGUILayout.Toggle(EditorConstGUIContent.ConfigShowPath,showPath);
        showLabel = EditorGUILayout.Toggle(EditorConstGUIContent.ConfigShowWps,showLabel);
        if (enabled.boolValue)
        {
            EditorGUILayout.PropertyField(updateType);
            EditorGUILayout.Space();
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
            EditorGUILayout.PropertyField(startValue,EditorConstGUIContent.ConfigStartVal );
            EditorGUILayout.PropertyField(endValue,EditorConstGUIContent.ConfigEndVal );
        }

        // }
        serializedObject.ApplyModifiedProperties();
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

    public virtual bool GetIsUsePreviewStartEndValue()
    {
        return false;
    }

    public virtual object GetPreviewStartValue()
    {
        return null;
    }

    public virtual object GetPreviewEndValue()
    {
        return null;
    }

    public virtual object GetPreviewBaseValue()
    {
        return null;
    }
}

