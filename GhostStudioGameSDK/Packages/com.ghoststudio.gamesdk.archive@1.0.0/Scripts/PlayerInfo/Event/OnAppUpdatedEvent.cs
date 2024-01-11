using AppBase.Event;

namespace AppBase.PlayerInfo
{
    /// <summary>
    /// 当APP升级时触发
    /// </summary>
    public class OnAppUpdatedEvent : IEvent
    {
        public string curClientVer;
        public string lastClientVer;

        public OnAppUpdatedEvent(string curClientVer, string lastClientVer)
        {
            this.curClientVer = curClientVer;
            this.lastClientVer = lastClientVer;
        }
    }
}