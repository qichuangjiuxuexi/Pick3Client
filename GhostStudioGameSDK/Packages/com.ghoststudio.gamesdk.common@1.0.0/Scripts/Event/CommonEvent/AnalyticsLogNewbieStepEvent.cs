namespace AppBase.Event
{
    /// <summary>
    /// 新手漏斗打点事件
    /// </summary>
    public class AnalyticsLogNewbieStepEvent:IEvent
    {
        public readonly string eventName;

        public AnalyticsLogNewbieStepEvent( string eventName)
        {
            this.eventName = eventName;
        }
    }
}