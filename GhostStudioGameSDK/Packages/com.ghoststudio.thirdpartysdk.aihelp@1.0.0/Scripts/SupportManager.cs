using System;
using System.Collections;
using System.Collections.Generic;
using AIHelp;
using AppBase;
using AppBase.Analytics;
using AppBase.Archive;
using AppBase.Config;
using AppBase.Event;
using AppBase.GetOrWait;
using AppBase.Localization;
using AppBase.Module;
using AppBase.Utils;
using UnityEngine;

namespace AppBase.Support
{
    public partial class SupportManager : MonoModule
    {
        private string pushToken = "";
        private int unreadMsgCount = 0;
        
        private string appKey;
        private string domain;
        private string appID;
        private string language;
        private int pushType;
        private List<string> collectDataKeys;

        /// <summary>
        /// 对话界面的欢迎语多语言key，如果不设置则用默认的
        /// </summary>
        private string welcomeTextKey;

        void GetConfigData()
        {
            var configMgr = GameBase.Instance.GetModule<ConfigManager>();
            welcomeTextKey = configMgr.GetConfigByKey<string, SupportConfig>(AAConst.SupportConfig,SupportConfigKeys.Support_WelcomTextKey).Value;
            language = configMgr.GetConfigByKey<string, SupportConfig>(AAConst.SupportConfig,SupportConfigKeys.Support_Language).Value;
            var configStr = configMgr.GetConfigByKey<string, SupportConfig>(AAConst.SupportConfig,SupportConfigKeys.Support_PushPlatform).Value;
            int.TryParse(configStr, out pushType);
            if (AppUtil.IsAndroid)
            {
                appKey = configMgr.GetConfigByKey<string, SupportConfig>(AAConst.SupportConfig,SupportConfigKeys.Support_AppKey_Android).Value;
                domain = configMgr.GetConfigByKey<string, SupportConfig>(AAConst.SupportConfig,SupportConfigKeys.Support_Domain_Android).Value;
                appID = configMgr.GetConfigByKey<string, SupportConfig>(AAConst.SupportConfig,SupportConfigKeys.Support_AppID_Android).Value;
            }
            else if(AppUtil.IsIOS)
            {
                appKey = configMgr.GetConfigByKey<string, SupportConfig>(AAConst.SupportConfig,SupportConfigKeys.Support_AppKey_iOS).Value;
                domain = configMgr.GetConfigByKey<string, SupportConfig>(AAConst.SupportConfig,SupportConfigKeys.Support_Domain_iOS).Value;
                appID = configMgr.GetConfigByKey<string, SupportConfig>(AAConst.SupportConfig,SupportConfigKeys.Support_AppID_iOS).Value;
            }
            var config = configMgr.GetConfigByKey<string, SupportConfig>(AAConst.SupportConfig,SupportConfigKeys.Support_CollectDataKeys);
            if (config == null || string.IsNullOrEmpty(config.Value))
            {
                collectDataKeys = new List<string>(0);
            }
            else
            {
                collectDataKeys = config.Value.ParseListString();
            }
        }

        protected override void OnInit()
        {
            base.OnInit();
            GetConfigData();
            try
            {
                AIHelpSupport.Init(appKey, domain, appID);
                unreadMsgCount = 0;
                AIHelpSupport.SetOnAIHelpInitializedCallback(OnInitSucc);
            }
            catch (Exception e)
            {
                Debug.LogError(e);
            }
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();
            AIHelpSupport.SetOnAIHelpInitializedCallback(null);
            AIHelpSupport.StartUnreadMessageCountPolling(null);
        }

        void TryGetToken()
        {
            object token = null;
            switch (pushType)
            {
                case 1://Firebase
                    token = GameBase.Instance.GetModule<GetOrWaitManager>().GetOrWaitCallBack("firebase_token", OnGetToken);
                    break;
                case 2://苹果APns
                    token = GameBase.Instance.GetModule<GetOrWaitManager>().GetOrWaitCallBack("apns_token", OnGetToken);
                    break;
                default:
                    break;
            }

            if (token != null)
            {
                string strToken = (string)token;
                if (!string.IsNullOrEmpty(strToken))
                {
                    SetPushToken(strToken);
                }
            }
            
        }

        void OnGetToken(string key,object val)
        {
            if (val != null)
            {
                SetPushToken((string)val);
            }
        }

        void OnInitSucc(bool isSuccess, string message)
        {
            Debug.Log("AIHelp Init succ");
            AIHelpSupport.StartUnreadMessageCountPolling(OnMessageCountArrivedCallback);
            GameBase.Instance.GetModule<EventManager>().Broadcast<EventSupportInitFinished>(
                new EventSupportInitFinished()
                {
                    isSucc = isSuccess
                });
            TryGetToken();
        }

        void OnMessageCountArrivedCallback(int msgCount)
        {
            Debug.Log($"AIHELP:unread message msgCount:{msgCount}");
            unreadMsgCount = msgCount;
            OnReceiveUnreadMsg();
        }

        public void ShowConversation(Dictionary<string, object> metaData, string entranceID)
        {
            ShowEntrance(metaData, entranceID);
        }
        
        public int GetUnreadMsgCount()
        {
            return unreadMsgCount;
        }

        public void ShowFAQs(Dictionary<string, object> metaData, string entranceID)
        {
            ShowEntrance(metaData, entranceID);
        }

        public void Login()
        {
            UpdateUserInfo(null);
        }
        
        public void LogOut()
        {
            AIHelpSupport.ResetUserInfo();
        }

        public void RequestUnreadMsg()
        {
            Debug.LogError("未实现的接口");
        }
        

        void UpdateUserInfo(Dictionary<string, object> metaData)
        {
            try
            {
                Dictionary<string, object> customData = CollectData();
                if (metaData != null)
                {
                    if (customData == null)
                    {
                        customData = new Dictionary<string, object>();
                    }

                    foreach (var item in metaData)
                    {
                        customData[item.Key] = item.Value;
                    }
                }

                if (customData == null)
                {
                    customData = new Dictionary<string, object>(0);
                }

                var builder = new UserConfig.Builder();
                string uid = GameBase.Instance.GetModule<ArchiveManager>().UserId;
                if (string.IsNullOrEmpty(uid))
                {
                    uid = GameBase.Instance.GetModule<ArchiveManager>().VisitorId;
                }
                builder.SetUserId(uid);
                var analyticsMgr = GameBase.Instance.GetModule<AnalyticsManager>();
                var userName = analyticsMgr.GetPropertyValue(AnalyticsUserPropertiesConfigKeys.user_nickname,"");
                builder.SetUserName((string)userName);
                builder.SetCustomData(JsonUtil.SerializeObject(customData))
                    .build();
                UserConfig userConfig = builder.build();

                AIHelpSupport.UpdateUserInfo(userConfig);
            }
            catch (Exception e)
            {
                Debug.LogError(e);
            }
        }

        Dictionary<string, object> CollectData()
        {
            var dataMap = new Dictionary<string, object>(collectDataKeys.Count);
            var analyticsMgr = GameBase.Instance.GetModule<AnalyticsManager>();
            for (int i = 0; i < collectDataKeys.Count; i++)
            {
                dataMap[collectDataKeys[i]] = analyticsMgr.GetPropertyValue(collectDataKeys[i]);
            }

            return dataMap;
        }

        void OnReceiveUnreadMsg()
        {
            Debug.LogError($"OnReceiveUnreadMsg：{unreadMsgCount}");
            GameBase.Instance.GetModule<EventManager>()
                .Broadcast<EventOnSupportMsgCountChanged>(new EventOnSupportMsgCountChanged() { count = unreadMsgCount });
        }

        void SetPushToken(string pushToken)
        {
            this.pushToken = pushToken;
            Debugger.LogDWarning($"set push token:{pushToken}");
            switch (pushType)
            {
                case 1:
                    AIHelpSupport.SetPushTokenAndPlatform(pushToken, PushPlatform.FIREBASE);
                    break;
                case 2:
                    AIHelpSupport.SetPushTokenAndPlatform(pushToken, PushPlatform.APNS);
                    break;
                default:
                    Debug.LogWarning("push token type not found");
                    break;
            }
        }

        void ShowEntrance(Dictionary<string, object> metaData, string entranceID)
        {
            unreadMsgCount = 0;
            UpdateUserInfo(metaData);
            var builder = new ApiConfig.Builder();
            builder.SetEntranceId(entranceID);
            builder.SetWelcomeMessage(GetWelcomeText());
            ApiConfig apiConfig = builder.build();
            AIHelpSupport.Show(apiConfig);
            GameBase.Instance.GetModule<EventManager>()
                .Broadcast<EventOnSupportMsgCountChanged>(new EventOnSupportMsgCountChanged() { count = unreadMsgCount });
        }

        string GetWelcomeText()
        {
            string welcomeText = "";
            if (!string.IsNullOrEmpty(welcomeTextKey)) //如果明确指明了
            {
                welcomeText = GameBase.Instance.GetModule<LocalizationManager>().GetText(welcomeTextKey, "");
                if (string.IsNullOrEmpty(welcomeText))
                {
                    return TryGetDefaultText();
                }
            }

            return TryGetDefaultText();
        }

        string TryGetDefaultText()
        {
            var configMgr = GameBase.Instance.GetModule<ConfigManager>();
            var config = configMgr.GetConfigByKey<string, SupportConfig>(AAConst.SupportConfig, language);
            if (!string.IsNullOrEmpty(language) && config != null)
            {
                return config.Value;
            }

            string lang = Application.systemLanguage.ToString();
            config = configMgr.GetConfigByKey<string, SupportConfig>(AAConst.SupportConfig, lang);
            if (config != null)
            {
                return config.Value;
            }

            config = configMgr.GetConfigByKey<string, SupportConfig>(AAConst.SupportConfig, SystemLanguage.English.ToString());
            if (config != null)
            {
                return config.Value;
            }

            return "Hi, how can we help you?";
        }
    }

}
