namespace AppBase.OA.Editor
{
    public enum ObjectAnimationComponentEditorState
    {
        None,
        /// <summary>
        /// 正在预览播放动画中
        /// </summary>
        Preview,
    
        /// <summary>
        /// 编辑配置中
        /// </summary>
        EditConfig,
        PreviewFrame,
    }
    
    public interface IObjectAnimationEditor
    {
        ObjectAnimationComponentEditorState state { get; set; }
        void OnCustomSceneGUI();
        IObjectAnimationEditor hostEditor { get; set; }
        
        bool show { get; set; }

        IObjectAnimationEditor GetCurEditConfigEditor();
        void QuitEditMode();
        void EnterEditMode();

        bool GetIsUsePreviewStartEndValue();

        object GetPreviewStartValue();
        object GetPreviewEndValue();

        object GetPreviewBaseValue();

    }
}