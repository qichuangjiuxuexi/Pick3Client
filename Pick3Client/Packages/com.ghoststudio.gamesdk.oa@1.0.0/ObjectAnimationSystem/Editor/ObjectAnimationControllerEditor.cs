using System;
using System.Collections.Generic;
using AppBase.OA.Editor;
using AppBase.OA.WorldPositionMoveModule.Editor;
using UnityEditor;
using UnityEngine;

namespace Project.AppBase.ObjectAnimationSystem.Scripts.Editor
{
    [CustomEditor(typeof(ObjectAnimationController))]
    public class ObjectAnimationControllerEditor:BasePreviewableObjectAnimationEditor
    {
        private SerializedProperty defaultAnimation;
        private SerializedProperty autoPlayOnStart;
        private SerializedProperty alsoSearchInChildren;
        private int selectedIndex = 0;
        private string[] options;

        public override void InitProperties()
        {
            defaultAnimation = serializedObject.FindProperty("defaultAnimation");
            autoPlayOnStart = serializedObject.FindProperty("autoPlayOnStart");
            alsoSearchInChildren = serializedObject.FindProperty("alsoSearchInChildren");
            BaseObjectAnimationComponent[] cps = null;
            if (alsoSearchInChildren.boolValue)
            {
                cps = (target as ObjectAnimationController).GetComponentsInChildren<BaseObjectAnimationComponent>();
            }
            else
            {
                cps = (target as ObjectAnimationController).GetComponents<BaseObjectAnimationComponent>();
            }
            
            options = new string[cps.Length + 1];
            options[0] = "none";
            for (int i = 0; i < cps.Length; i++)
            {
                options[i + 1] = cps[i].animationName;
            }

            for (int i = 0; i < options.Length; i++)
            {
                if (defaultAnimation.stringValue == options[i])
                {
                    selectedIndex = i;
                }
            }
        }

        public override List<BaseObjectAnimationEditor> GetSubEditors()
        {
            return null;
        }

        public override void DrawCustomInspector()
        {
            serializedObject.Update();
            base.DrawCustomInspector();
            EditorGUILayout.PropertyField(autoPlayOnStart);
            EditorGUILayout.PropertyField(alsoSearchInChildren);
            EditorGUILayout.BeginHorizontal();
            selectedIndex = EditorGUILayout.Popup(EditorConstGUIContent.ConfigDefaultAnimation,selectedIndex, options);
            if (selectedIndex <= options.Length)
            {
                defaultAnimation.stringValue = selectedIndex == 0 ? "" : options[selectedIndex];
            }
            if (GUILayout.Button("刷新动画列表"))
            {
                InitProperties();
            }
            if (GUILayout.Button("播放所有动画"))
            {
                OnPlayAll();
            }
            EditorGUILayout.EndHorizontal();
            serializedObject.ApplyModifiedProperties(); 
        }

        public  void OnPlayAll()
        {
            state = ObjectAnimationComponentEditorState.Preview;
            var ctrl = (target as ObjectAnimationController);
            int finishedCount = 0;
            for (int i = 1; i < options.Length; i++)
            {
                ctrl.Play(options[i], () =>
                {
                    Debug.Log($"{defaultAnimation.stringValue} finished");
                    finishedCount++;
                    if (finishedCount == options.Length - 1)
                    {
                        state = ObjectAnimationComponentEditorState.None;
                    }
                    // state = ObjectAnimationComponentEditorState.None;
                });
            }
        }

        public override void OnPlay()
        {
            var ctrl = (target as ObjectAnimationController);
            ctrl.Play(ctrl.defaultAnimation, () =>
            {
                Debug.Log($"{defaultAnimation.stringValue} finished");
                state = ObjectAnimationComponentEditorState.None;
            });
        }

        public override void OnPlayFrame()
        {
            var ctrl = (target as ObjectAnimationController);
            ctrl.Play(ctrl.defaultAnimation, () =>
            {
                Debug.Log($"{defaultAnimation.stringValue} finished frame by frame");
                state = ObjectAnimationComponentEditorState.None;
            });
        }

        public override void SimulateUpdate(float deltaTime)
        {
            var ctrl = (target as ObjectAnimationController);
            var enumerator =  ctrl.GetAnimEnumrator();
            while (enumerator.MoveNext())
            {
                ((KeyValuePair<string,BaseObjectAnimationComponent>)enumerator.Current).Value.SimuUpdateAll(deltaTime);
            }
        }

        public override void DrawSubEditorTitle()
        {
        }

        public override void DrawSubEditorSplit()
        {
        }

        public override void DrawSelfTitle()
        {
        }
        
        
    }
}