using System;
using System.Collections;
using System.Collections.Generic;
using AppBase.PlayerInfo;

namespace AppBase.SocialAccount
{
    public class SocialAccountBase
    {
        // 社交平台类型
        public virtual SocialAccountType Type => SocialAccountType.Facebook;

        public virtual void OnInit()
        {
            
        }

        public virtual void Login(Action<SocialAccountInfo> successCallback, Action failedCallback)
        {
            failedCallback?.Invoke();
        }

        public virtual void Logout(Action successCallback, Action failedCallback)
        {
            successCallback?.Invoke();
        }
    }
}