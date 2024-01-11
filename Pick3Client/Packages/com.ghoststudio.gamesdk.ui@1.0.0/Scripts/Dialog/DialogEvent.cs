using AppBase.Event;

namespace AppBase.UI.Dialog
{
    /// <summary>
    /// 弹板弹出之前的事件
    /// </summary>
    public struct BeforeDialogPopEvent : IEvent
    {
        public readonly DialogData DialogData;

        public BeforeDialogPopEvent(DialogData dialogData)
        {
            DialogData = dialogData;
        }
    }
    /// <summary>
    /// 弹板弹出之后的事件
    /// </summary>
    public struct AfterDialogPopEvent : IEvent
    {
        public readonly DialogData DialogData;

        public AfterDialogPopEvent(DialogData dialogData)
        {
            DialogData = dialogData;
        }
    }
    /// <summary>
    /// 对话框销毁的事件
    /// </summary>
    public struct OnDialogDestroyEvent : IEvent
    {
        public readonly string Address;
        public OnDialogDestroyEvent(string address)
        {
            Address = address;
        }
    }
}