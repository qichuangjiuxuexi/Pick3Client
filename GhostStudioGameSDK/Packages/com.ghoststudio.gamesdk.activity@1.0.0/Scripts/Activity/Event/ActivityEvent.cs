using AppBase.Event;

namespace AppBase.Activity
{
    /// <summary>
    /// 活动可开启事件
    /// </summary>
    public class OnActivityReadyEvent : IEvent
    {
        public ActivityModule activity;
        public OnActivityReadyEvent(ActivityModule activity)
        {
            this.activity = activity;
        }
    }
    
    /// <summary>
    /// 活动开启事件
    /// </summary>
    public class OnActivityOpenEvent : IEvent
    {
        public ActivityModule activity;
        public OnActivityOpenEvent(ActivityModule activity)
        {
            this.activity = activity;
        }
    }
    
    /// <summary>
    /// 活动完成事件
    /// </summary>
    public class OnActivityFinishEvent : IEvent
    {
        public ActivityModule activity;
        public OnActivityFinishEvent(ActivityModule activity)
        {
            this.activity = activity;
        }
    }
    
    /// <summary>
    /// 活动结束事件
    /// </summary>
    public class OnActivityCloseEvent : IEvent
    {
        public ActivityModule activity;
        public OnActivityCloseEvent(ActivityModule activity)
        {
            this.activity = activity;
        }
    }
}