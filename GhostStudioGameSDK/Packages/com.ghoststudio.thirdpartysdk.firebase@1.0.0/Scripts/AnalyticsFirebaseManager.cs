using System;
using System.Collections.Generic;
using System.Threading;
using AppBase.Analytics;
using AppBase.Archive;
using AppBase.Config;
using AppBase.GetOrWait;
using AppBase.Utils;
using Cysharp.Threading.Tasks;
using Firebase;
using Firebase.Analytics;
using Firebase.Crashlytics;
using Firebase.Extensions;
using UnityEngine;

namespace AppBase.ThirdParty
{
    /// <summary>
    /// Firebase打点管理器
    /// </summary>
    public class AnalyticsFirebaseManager : ThirdPartAnalyticsManager
    {
        public string installationId;
        public string fcmToken;
        
        private FirebaseApp app;
        private ArchiveManager archiveManager => GameBase.Instance.GetModule<ArchiveManager>();

        protected override void OnInit()
        {
            base.OnInit();
            ((AnalyticsManager)ParentModule).RegisterUserProperty(AnalyticsUserPropertiesConfigKeys.firebase_id, () => installationId, true);
            ((AnalyticsManager)ParentModule).RegisterUserProperty(AnalyticsUserPropertiesConfigKeys.firebase_fcm_token, () => fcmToken, true);
            InitSDK();
        }

        /// <summary>
        /// 初始化SDK
        /// </summary>
        public override void InitSDK()
        {
            if (AppUtil.IsDebug) return;
            FirebaseApp.CheckAndFixDependenciesAsync().ContinueWithOnMainThread(task =>
            {
                var dependencyStatus = task.Result;
                if (dependencyStatus == DependencyStatus.Available)
                {
                    app = FirebaseApp.DefaultInstance;
                    isInit = true;
                    FirebaseAnalytics.SetUserProperty("udid", AppUtil.DeviceId);
                    Crashlytics.SetCustomKey("udid", AppUtil.DeviceId);
                    var visitorId = archiveManager?.UserId ?? archiveManager?.VisitorId;
                    if (!string.IsNullOrEmpty(visitorId))
                    {
                        FirebaseAnalytics.SetUserId(visitorId);
                        Crashlytics.SetUserId(visitorId);
                        FirebaseAnalytics.SetUserProperty("pid", visitorId);
                    }
                    if (AppUtil.IsDebug)
                    {
                        FirebaseAnalytics.SetUserProperty("debug_build", "true");
                        Crashlytics.SetCustomKey("debug_build", "true");
                    }
                    FirebaseAnalytics.GetAnalyticsInstanceIdAsync().ContinueWithOnMainThread(id => OnGetInstanceId(id.Result));
                    //经测试，有时这种方式拿不到
                    Firebase.Messaging.FirebaseMessaging.GetTokenAsync().ContinueWithOnMainThread(token => OnGetFcmToken(token.Result));
                    //注册个回调，接收token
                    Firebase.Messaging.FirebaseMessaging.TokenReceived += OnTokenReceived;
                    TrackCachedEvents();
                }
                else
                {
                    isInit = false;
                    Debugger.LogError(TAG, $"Could not resolve all Firebase dependencies: {dependencyStatus}");
                }
            });
        }

        /// <summary>
        /// 打点事件
        /// </summary>
        /// <param name="event">事件数据</param>
        public override void TrackEvent(AnalyticsEvent evt)
        {
            if (evt == null || string.IsNullOrEmpty(evt.eventName)) return;
            if (!isInit) return;
            var config = GameBase.Instance.GetModule<ConfigManager>().GetConfigByKey<string, AnalyticsThirdPartyConfig>(AAConst.AnalyticsThirdPartyConfig, evt.eventName);
            if (config == null || string.IsNullOrEmpty(config.firebase_name)) return;
            if (evt.eventData != null && evt.eventData.Count > 0)
            {
                var paramList = new List<Parameter>();
                foreach (var param in evt.eventData)
                {
                    switch (param.Value)
                    {
                        case float:
                        case double:
                            paramList.Add(new Parameter(param.Key, Convert.ToDouble(param.Value)));
                            break;
                        case int:
                        case long:
                            paramList.Add(new Parameter(param.Key, Convert.ToInt64(param.Value)));
                            break;
                        default:
                            paramList.Add(new Parameter(param.Key, param.Value.ToString()));
                            break;
                    }
                }
                FirebaseAnalytics.LogEvent(evt.eventName, paramList.ToArray());
            }
            else
            {
                FirebaseAnalytics.LogEvent(evt.eventName);
            }
        }

        /// <summary>
        /// 记录错误到Crashlytics
        /// </summary>
        /// <param name="error">错误信息</param>
        public void TrackError(string error)
        {
            if (!isInit || string.IsNullOrEmpty(error)) return;
            Crashlytics.Log(error);
        }

        private void OnGetInstanceId(string id)
        {
            Debugger.Log(TAG, $"Firebase installationId: {id}");
            if (string.IsNullOrEmpty(id)) return;
            installationId = id;
        }
        
        private void OnGetFcmToken(string token)
        {
            UnityEngine.Debug.Log("Received Registration Token(OnGetFcmToken): " + token);
            if (string.IsNullOrEmpty(token)) return;
            fcmToken = token;
            GameBase.Instance.GetModule<GetOrWaitManager>().SetData("firebase_token",fcmToken);
        }
        
        public void OnTokenReceived(object sender, Firebase.Messaging.TokenReceivedEventArgs token)
        {
            Firebase.Messaging.FirebaseMessaging.GetTokenAsync().ContinueWithOnMainThread(token => OnGetFcmToken(token.Result));
        }
    }
}