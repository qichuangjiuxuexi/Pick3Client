using AppBase.Event;
using AppBase.PlayerInfo;

namespace AppBase.SocialAccount
{
    /// <summary>
    /// 绑定事件
    /// </summary>
    public struct SocialAccountBindEvent : IEvent
    {
        public SocialAccountInfo info;

        public SocialAccountBindEvent(SocialAccountInfo info)
        {
            this.info = info;
        }
    }

    /// <summary>
    /// 解绑事件
    /// </summary>
    public struct SocialAccountUnbindEvent : IEvent
    {
        public SocialAccountType type;

        public SocialAccountUnbindEvent(SocialAccountType type)
        {
            this.type = type;
        }
    }
    
    /// <summary>
    /// 其他人顶号事件
    /// </summary>
    public struct SocialAccountOthersLoginEvent : IEvent
    {
        public SocialAccountInfo info;

        public SocialAccountOthersLoginEvent(SocialAccountInfo info)
        {
            this.info = info;
        }
    }
}