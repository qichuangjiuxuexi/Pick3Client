using System;
using UnityEngine;
#if UNITY_IOS
using AppleAuth;
using AppleAuth.Enums;
using AppleAuth.Extensions;
using AppleAuth.Interfaces;
using AppleAuth.Native;
#endif


namespace AppleAuth
{
    public class AppleAuthMonoManager
    {
#if UNITY_IOS
        private IAppleAuthManager appleAuthManager;

        void Start()
        {
            // If the current platform is supported
            if (AppleAuthManager.IsCurrentPlatformSupported)
            {
                // Creates a default JSON deserializer, to transform JSON Native responses to C# instances
                var deserializer = new PayloadDeserializer();
                // Creates an Apple Authentication manager with the deserializer
                this.appleAuthManager = new AppleAuthManager(deserializer);
            }
        }

        // Update is called once per frame
        void Update()
        {
            // Updates the AppleAuthManager instance to execute
            // pending callbacks inside Unity's execution loop
            if (this.appleAuthManager != null)
            {
                this.appleAuthManager.Update();
            }
        }

        public void Login(Action<IAppleIDCredential> successCallback, Action<string> failedCallback)
        {
// #if UNITY_EDITOR
//             successCallback?.Invoke(new AppleAuthLoginData(){id = ConfigParam.Instance.appleUserId, name = ConfigParam.Instance.appleUserId});
//             return;
// #endif
            var loginArgs = new AppleAuthLoginArgs(LoginOptions.IncludeEmail | LoginOptions.IncludeFullName);
            this.appleAuthManager.LoginWithAppleId(loginArgs, credential =>
                {
                    // Obtained credential, cast it to IAppleIDCredential
                    var appleIdCredential = credential as IAppleIDCredential;
                    if (appleIdCredential != null)
                    {
                        // // Apple User ID
                        // // You should save the user ID somewhere in the device
                        // var userId = appleIdCredential.User;
                        // // Full name (Received ONLY in the first login)
                        // var fullName = appleIdCredential.FullName;
                        //
                        // AppleAuthLoginData data = new AppleAuthLoginData();
                        // data.id = userId;
                        // data.name = fullName == null ? "" : fullName.Nickname;
                        // if (data.name == null)
                        // {
                        //     data.name = "";
                        // }
                        // And now you have all the information to create/login a user in your system
                        successCallback?.Invoke(appleIdCredential);
                    }
                    else
                    {
                        failedCallback?.Invoke("[AppleAuth] credential is null");
                    }
                },
                errorInfo =>
                {
                    var authorizationErrorCode = errorInfo.GetAuthorizationErrorCode();
                    failedCallback?.Invoke("[AppleAuth]" + errorInfo.LocalizedDescription + "..." +
                                           authorizationErrorCode.ToString());
                });
        }
#endif
    }
}