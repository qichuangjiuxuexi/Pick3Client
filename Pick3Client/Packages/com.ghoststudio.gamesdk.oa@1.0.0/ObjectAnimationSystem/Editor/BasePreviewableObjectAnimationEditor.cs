using UnityEditor;
using UnityEngine;

namespace AppBase.OA.Editor
{
    public abstract class BasePreviewableObjectAnimationEditor : BaseObjectAnimationEditor
    {
        private bool isPreviewFrameByFrame = false;
        private int frameRate = 60;
        private BaseObjectAnimationComponent curTarget;
        private SerializedProperty totalElapsedTime;
        private float elapsedTime;
        private double lastUpdateTime = 0;
        public override void OnEnable()
        {
            base.OnEnable();
            totalElapsedTime = serializedObject.FindProperty("totalElapsedTime");
        }

        public override void DrawCustomInspector()
        {
            DrawPlayDashboard();
        }

        public virtual void DrawPlayDashboard()
        {
            EditorGUILayout.BeginHorizontal();
            frameRate = EditorGUILayout.IntField("模拟帧率", frameRate);
            if (state == ObjectAnimationComponentEditorState.None)
            {
                if (GUILayout.Button("播放"))
                {
                    if (!IsInPreview())
                    {
                        state = ObjectAnimationComponentEditorState.Preview;
                        OnPlay();
                    }
                }
            
                if (GUILayout.Button("逐帧播放"))
                {
                    if (!IsInPreview())
                    {
                        state = ObjectAnimationComponentEditorState.PreviewFrame;
                        OnPlayFrame();
                    }
                }
            }
            
            if (IsInPreview())
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
                if (totalElapsedTime != null)
                {
                    EditorGUILayout.HelpBox($"已播放时长:{totalElapsedTime.floatValue}", MessageType.Info);
                }
                else
                {
                    EditorGUILayout.FloatField("已播放时长：", elapsedTime);
                }
            }
            
            EditorGUILayout.Space();
        }
        public abstract void OnPlay();
        public abstract void OnPlayFrame();

        public bool IsInPreview()
        {
            return state == ObjectAnimationComponentEditorState.Preview ||
                           state == ObjectAnimationComponentEditorState.PreviewFrame;
        }
        
        public override void OnEditorUpdate()
        {
            base.OnEditorUpdate();
            if (IsInPreview()  && hostEditor == null)
            {
                if (EditorApplication.timeSinceStartup - lastUpdateTime > 1 / (float)frameRate)
                {
                    if (state == ObjectAnimationComponentEditorState.PreviewFrame)
                    {
                        if (isPreviewFrameByFrame)
                        {
                            SimulateUpdate(1 / (float)frameRate);
                            isPreviewFrameByFrame = false;
                            AddTime();
                        }
                    }
                    else if(state == ObjectAnimationComponentEditorState.Preview)
                    {
                        SimulateUpdate(1 / (float)frameRate);
                        AddTime();
                    }
                    lastUpdateTime = EditorApplication.timeSinceStartup;
                }
                
            }
        }

        public abstract void SimulateUpdate(float deltaTime);

        void AddTime()
        {
            if (totalElapsedTime != null)
            {
                totalElapsedTime.floatValue += (1 / (float) frameRate);
            }
            elapsedTime += (1 / (float) frameRate);
        }
    }
}