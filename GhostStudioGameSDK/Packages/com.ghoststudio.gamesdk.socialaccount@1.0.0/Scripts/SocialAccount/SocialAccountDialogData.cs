using System;
using AppBase.PlayerInfo;

namespace AppBase.SocialAccount
{
    public class SocialAccountDialogData
    {
        public SocialAccountInfo accountInfo;   // 社交账号信息
        public bool isNeedChange;               // 是否需要切换账号
        public Action<SocialAccountInfo> changeCallback;     // 切换账号方法回调
        public Action<SocialAccountInfo> unBindCallback;     // 登出账号方法回调
        public Action restartCallback;          // 当前账号重新登录方法
    }
}