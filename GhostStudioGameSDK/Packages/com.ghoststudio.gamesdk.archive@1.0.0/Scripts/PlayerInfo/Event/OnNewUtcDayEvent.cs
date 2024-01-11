using System;
using AppBase.Event;

namespace AppBase.PlayerInfo
{
    /// <summary>
    /// 当UTC新的一天时触发
    /// </summary>
    public class OnNewUtcDayEvent : IEvent
    {
        public DateTime lastLoginUtcTime;
        public DateTime curLoginUtcTime;
        
        public OnNewUtcDayEvent(DateTime lastLoginUtcTime, DateTime curLoginUtcTime)
        {
            this.lastLoginUtcTime = lastLoginUtcTime;
            this.curLoginUtcTime = curLoginUtcTime;
        }
    }
}