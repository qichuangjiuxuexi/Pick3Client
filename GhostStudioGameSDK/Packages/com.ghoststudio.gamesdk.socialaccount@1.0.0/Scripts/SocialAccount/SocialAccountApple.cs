using System;
using System.Collections.Generic;
using AppBase.PlayerInfo;
using AppBase.Timing;
#if UNITY_IOS
using AppleAuth;
using AppleAuth.Enums;
using AppleAuth.Extensions;
using AppleAuth.Interfaces;
using AppleAuth.Native;
#endif
using UnityEngine;

namespace AppBase.SocialAccount
{
    public class SocialAccountApple : SocialAccountBase, IUpdateFrame
    {
        // 社交平台类型
        public override SocialAccountType Type => SocialAccountType.Apple;
#if UNITY_IOS
        private IAppleAuthManager appleAuthManager;
#endif

        public override void OnInit()
        {
#if UNITY_IOS
            // If the current platform is supported
            if (AppleAuthManager.IsCurrentPlatformSupported)
            {
                // Creates a default JSON deserializer, to transform JSON Native responses to C# instances
                var deserializer = new PayloadDeserializer();
                // Creates an Apple Authentication manager with the deserializer
                this.appleAuthManager = new AppleAuthManager(deserializer);

                GameBase.Instance.GetModule<TimingManager>().SubscribeFrameUpdate(this);
            }
#endif
        }

        public void Update()
        {
#if UNITY_IOS
            if (this.appleAuthManager != null)
            {
                this.appleAuthManager.Update();
            }
#endif
        }

        public override void Login(Action<SocialAccountInfo> successCallback, Action failedCallback)
        {
#if UNITY_EDITOR
            SocialAccountInfo info = new SocialAccountInfo();
            info.id = "APPLE001";
            info.name = "TEST_APPLE_001";
            info.type = (int)Type;
            info.email = "test_dev@test.com";
            successCallback?.Invoke(info);
            return;
#endif
#if UNITY_IOS
            var loginArgs = new AppleAuthLoginArgs(LoginOptions.IncludeEmail | LoginOptions.IncludeFullName);
            this.appleAuthManager.LoginWithAppleId(loginArgs, credential =>
                {
                    // Obtained credential, cast it to IAppleIDCredential
                    var appleIdCredential = credential as IAppleIDCredential;
                    if (appleIdCredential != null)
                    {
                        // Apple User ID
                        // You should save the user ID somewhere in the device
                        var userId = appleIdCredential.User;
                        // Full name (Received ONLY in the first login)
                        var fullName = appleIdCredential.FullName;

                        SocialAccountInfo data = new SocialAccountInfo();
                        data.id = userId;
                        data.name = fullName == null ? "" : fullName.Nickname;
                        if (data.name == null)
                        {
                            data.name = "";
                        }
                        data.type = (int)Type;
                        data.email = appleIdCredential.Email;
                        // And now you have all the information to create/login a user in your system
                        successCallback?.Invoke(data);
                    }
                    else
                    {
                        Debug.LogError("[AppleAuth] credential is null");
                        failedCallback?.Invoke();
                    }
                },
                errorInfo =>
                {
                    var authorizationErrorCode = errorInfo.GetAuthorizationErrorCode();
                    Debug.LogError("[AppleAuth]" + errorInfo.LocalizedDescription + "..." +
                                   authorizationErrorCode.ToString());
                    failedCallback?.Invoke();
                });
#else
            failedCallback?.Invoke();
#endif
        }

        public override void Logout(Action successCallback, Action failedCallback)
        {
            successCallback?.Invoke();
        }
    }
}