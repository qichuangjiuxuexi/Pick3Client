using System;
using System.Collections.Generic;
using UnityEditor;

namespace AppBase.OA.Editor
{
   
    public abstract class BaseObjectAnimationEditor : UnityEditor.Editor,IObjectAnimationEditor
    {
        public List<BaseObjectAnimationEditor> subEditors;
        public virtual void OnEnable()
        {
            EditorApplication.update += OnEditorUpdate;
            if (subEditors != null)
            {
                subEditors.AddRange(GetSubEditors());
            }
            InitProperties();
        }

        public abstract void InitProperties();
        public abstract List<BaseObjectAnimationEditor> GetSubEditors();

        public abstract void DrawCustomInspector();
        public abstract void DrawSubEditorTitle();
        public abstract void DrawSubEditorSplit();
        public abstract void DrawSelfTitle();
        
        public void DrawSubEditors()
        {
            serializedObject.Update();
            DrawSubEditorTitle();
            if (subEditors != null)
            {
                for (int i = 0; i < subEditors.Count; i++)
                {
                    subEditors[i].DrawSelfTitle();
                    DrawIsShow(subEditors[i]);
                    if (subEditors[i].show)
                    {
                        subEditors[i].OnInspectorGUI();
                    }
                    DrawSubEditorSplit();
                }
            }
            DrawSubEditorEnd();
            serializedObject.ApplyModifiedProperties();
        }
        
        public virtual void DrawIsShow(BaseObjectAnimationEditor editor)
        {
            editor.show = EditorGUILayout.Toggle("是否展开:", editor.show);
        }

        public void DrawSubEditorEnd()
        {
            
        }

        public override void OnInspectorGUI()
        {
            DrawCustomInspector();
            DrawSubEditors();
        }

        public virtual void OnDisable()
        {
            EditorApplication.update -= OnEditorUpdate;
            if (subEditors != null)
            {
                for (int i = 0; i < subEditors.Count; i++)
                {
                    DestroyImmediate(subEditors[i]);
                }
                subEditors.Clear();
            }
        }

        public virtual void OnEditorUpdate()
        {

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

        public object GetPreviewBaseValue()
        {
            return null;
        }

        public bool IsInPreview()
        {
            return state == ObjectAnimationComponentEditorState.Preview ||
                           state == ObjectAnimationComponentEditorState.PreviewFrame;
        }
    }
}