using AppBase.Event;

namespace AppBase.UI.OneByOne
{
    /// <summary>
    /// 弹板池调用事件，需要使用带Callback的回调函数来接收
    /// </summary>
    public class OneByOneEvent : IEvent
    {
        public string eventName;
        public string extraData;
        public OneByOneTriggerData triggerData;

        public OneByOneEvent(string eventName, string extraData, OneByOneTriggerData triggerData)
        {
            this.eventName = eventName;
            this.extraData = extraData;
            this.triggerData = triggerData;
        }
    }
    
    /// <summary>
    /// 弹板池并发事件，并发执行，并等待所有并发回调回来
    /// </summary>
    public class OneByOneParallelEvent : OneByOneEvent, IParallelEvent
    {
        public OneByOneParallelEvent(string eventName, string extraData, OneByOneTriggerData triggerData) : base(eventName, extraData, triggerData)
        {
        }
    }
}
