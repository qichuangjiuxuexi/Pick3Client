using AppBase.Event;

namespace AppBase.PlayerInfo
{
    /// <summary>
    /// 本地时区连续登录天数重置
    /// </summary>
    public class OnContinuePlayLocalDayReset : IEvent
    {
        public int lastContinuePlayLocalDay;
        
        public OnContinuePlayLocalDayReset(int lastContinuePlayLocalDay)
        {
            this.lastContinuePlayLocalDay = lastContinuePlayLocalDay;
        }
    }
}