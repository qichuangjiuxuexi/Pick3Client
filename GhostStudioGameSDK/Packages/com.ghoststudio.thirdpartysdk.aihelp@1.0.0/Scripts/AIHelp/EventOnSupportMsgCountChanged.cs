using AppBase.Event;

namespace AIHelp
{
    public struct EventOnSupportMsgCountChanged : IEvent
    {
        public int count;
    }
    public struct EventSupportInitFinished : IEvent
    {
        public bool isSucc;
    }
}