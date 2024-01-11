using System;
using AppBase.Event;

namespace AppBase.PlayerInfo
{
    /// <summary>
    /// 当本地时间新的一天时触发
    /// </summary>
    public class OnNewLocalDayEvent : IEvent
    {
        public DateTime lastLoginLocalTime;
        public DateTime curLoginLocalTime;
        
        public OnNewLocalDayEvent(DateTime lastLoginLocalTime, DateTime curLoginLocalTime)
        {
            this.lastLoginLocalTime = lastLoginLocalTime;
            this.curLoginLocalTime = curLoginLocalTime;
        }
    }
}