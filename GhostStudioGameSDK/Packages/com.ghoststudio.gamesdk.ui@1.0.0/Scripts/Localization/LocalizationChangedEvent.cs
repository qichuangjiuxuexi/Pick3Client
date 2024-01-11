using AppBase.Event;

namespace AppBase.Localization
{
    /// <summary>
    /// 当语言切换时触发
    /// </summary>
    public struct LocalizationChangedEvent : IEvent
    {
        public string languageAddress;
        public LocalizationChangedEvent(string languageAddress) => this.languageAddress = languageAddress;
    }
}