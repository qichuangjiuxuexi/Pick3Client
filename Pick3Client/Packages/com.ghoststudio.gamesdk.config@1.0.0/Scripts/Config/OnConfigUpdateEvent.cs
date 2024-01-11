using System.Collections.Generic;
using AppBase.Event;

namespace AppBase.Config
{
    /// <summary>
    /// 当配置发生更新时，发送此事件
    /// </summary>
    public class OnConfigUpdateEvent : IEvent
    {
        /// <summary>
        /// 发生更新的配置地址列表
        /// </summary>
        public HashSet<string> addresses;
        
        public OnConfigUpdateEvent(IEnumerable<string> addresses)
        {
            this.addresses = new HashSet<string>(addresses);
        }
    }
}