using System;
using System.Collections;
using System.Collections.Generic;
using AppBase.PlayerInfo;
using UnityEngine;
#if UNITY_ANDROID
using GooglePlayGames;
using GooglePlayGames.BasicApi;
#endif

namespace AppBase.SocialAccount
{
    public class SocialAccountGoogle : SocialAccountBase
    {
        // 社交平台类型
        public override SocialAccountType Type => SocialAccountType.Google;

        public override void OnInit()
        {
#if UNITY_ANDROID
            //PlayGamesPlatform.DebugLogEnabled = true;
            PlayGamesPlatform.Activate();
#endif
        }

        private Action<SocialAccountInfo> successCallback;

        private Action failedCallback;
        
        public override void Login(Action<SocialAccountInfo> successCallback, Action failedCallback)
        {
            this.successCallback = successCallback;
            this.failedCallback = failedCallback;
#if UNITY_EDITOR
            SocialAccountInfo info = new SocialAccountInfo();
            info.id = "GOOGLE001";
            info.name = "TEST_GOOGLE_001";
            info.type = (int)Type;
            info.email = "test_dev@test.com";
            successCallback?.Invoke(info);
#elif UNITY_ANDROID
            Debug.Log("[SocialAccount] Login with google");
            PlayGamesPlatform.Instance.ManuallyAuthenticate(ProcessAuthentication);
#endif
        }
#if UNITY_ANDROID
        void ProcessAuthentication(SignInStatus status) {
            if (status == SignInStatus.Success) {
                Debug.Log("[SocialAccount] Google UserInfo===>id:"+Social.localUser.id);
                Debug.Log("[SocialAccount] Google UserInfo===>userName:"+Social.localUser.userName);
                // Continue with Play Games Services
                SocialAccountInfo data = new SocialAccountInfo();
                data.id = Social.localUser.id;
                data.name = Social.localUser.userName;
                data.type = (int)Type;
                successCallback?.Invoke(data);
            } else {
                // Disable your integration with Play Games Services or show a login button
                // to ask users to sign-in. Clicking it should call
                // PlayGamesPlatform.Instance.ManuallyAuthenticate(ProcessAuthentication).
                Debug.Log("[SocialAccount] Login with google failed : " + status);
                failedCallback?.Invoke();
            }
        }
#endif
        public override void Logout(Action successCallback, Action failedCallback)
        {
            successCallback?.Invoke();
        }
    }
}
