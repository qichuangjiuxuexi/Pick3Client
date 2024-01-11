using System;
using System.Collections.Generic;
using AppBase.UI.Dialog;

namespace AppBase.UI.OneByOne
{
    /// <summary>
    /// OneByOne的对话框上下文
    /// </summary>
    public class OneByOneDialogContext : DialogContext
    {
        public OneByOneTriggerData triggerData;
        public Action callback;
        
        public OneByOneDialogContext()
        {
        }
        
        public OneByOneDialogContext(OneByOneTriggerData triggerData, Action callback)
        {
            this.triggerData = triggerData;
            this.callback = callback;
        }

        public override void OnCloseCallback(UIDialog obj)
        {
            callback?.Invoke();
        }
    }
}