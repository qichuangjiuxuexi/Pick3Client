using System.Collections;
using System.Collections.Generic;
using AppBase.OA.Configs;
using AppBase.OA.Editor;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(FollowTargetMoveConfig))]
public class FollowTargetMoveBehaviourEditor : BaseOAConfigEditor,IObjectAnimationEditor
{
    private Transform followTarget;
    public SerializedProperty rCurve;
    public SerializedProperty thetaCurve;
    public SerializedProperty faiCurve;
    public SerializedProperty freezeTheta;
    public SerializedProperty freezeFai;

    protected override void OnEnable()
    {
        base.OnEnable();
        rCurve = serializedObject.FindProperty("rCurve");
        thetaCurve = serializedObject.FindProperty("thetaCurve");
        faiCurve = serializedObject.FindProperty("faiCurve");
        freezeTheta = serializedObject.FindProperty("freezeTheta");
        freezeFai = serializedObject.FindProperty("freezeFai");
    }

    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();
        this.followTarget = EditorGUILayout.ObjectField(this.followTarget,typeof(Transform),true) as Transform;
        if (target != null && target is FollowTargetMoveConfig followTarget)
        {
            followTarget.SetFollowTarget(this.followTarget);
        }

        EditorGUILayout.PropertyField(rCurve);
        EditorGUILayout.PropertyField(thetaCurve);
        EditorGUILayout.PropertyField(faiCurve);
        EditorGUILayout.PropertyField(freezeTheta);
        EditorGUILayout.PropertyField(freezeFai);
    }
}
