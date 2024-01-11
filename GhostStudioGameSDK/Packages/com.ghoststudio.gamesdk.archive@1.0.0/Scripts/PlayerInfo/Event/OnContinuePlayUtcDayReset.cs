using AppBase.Event;

namespace AppBase.PlayerInfo
{
    /// <summary>
    /// UTC连续登录天数重置
    /// </summary>
    public class OnContinuePlayUtcDayReset : IEvent
    {
        public int lastContinuePlayUtcDay;
        
        public OnContinuePlayUtcDayReset(int lastContinuePlayUtcDay)
        {
            this.lastContinuePlayUtcDay = lastContinuePlayUtcDay;
        }
    }
}