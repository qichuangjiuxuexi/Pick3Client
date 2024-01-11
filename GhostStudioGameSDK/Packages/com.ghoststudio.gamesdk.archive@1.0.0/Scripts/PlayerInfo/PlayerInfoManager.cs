using System;
using AppBase.Analytics;
using AppBase.Archive;
using AppBase.Config;
using AppBase.Debugging;
using AppBase.Event;
using AppBase.GetOrWait;
using AppBase.Module;
using AppBase.Timing;
using AppBase.Utils;
using UnityEngine;

namespace AppBase.PlayerInfo
{
    /// <summary>
    /// 用户信息管理器
    /// </summary>
    public class PlayerInfoManager : ModuleBase, IUpdateSecond
    {
        /// <summary>
        /// 用户信息
        /// </summary>
        public PlayerInfoRecord PlayerRecord { get; protected set; }

        /// <summary>
        /// 当APP升级时，记录上次APP版本号
        /// </summary>
        protected string lastClientVer;
        
        /// <summary>
        /// 记录最后登录的UTC日期
        /// </summary>
        protected DateTime lastLoginUtcTime;

        /// <summary>
        /// 本地夸天时间偏移 读表获取
        /// </summary>
        public int localNewDayOffest;
        
        /// <summary>
        /// 记录最后登录的本地日期
        /// </summary>
        protected DateTime lastLoginLocalTime;

        protected override void OnInit()
        {
            base.OnInit();
            PlayerRecord = AddModule<PlayerInfoRecord>();
            if (PlayerRecord.IsNewRecord)
            {
                OnNewRecord();
            }
            else
            {
                OnAppLaunch();
            }
            GameBase.Instance.GetModule<GetOrWaitManager>().SetData(SRDebugManager.user_id,PlayerRecord.ArchiveData.userId);
            LogAppLaunch();
            AddModule<PlayerInfoSROptions>();
        }

        protected override void OnAfterInit()
        {
            base.OnAfterInit();
            var offest = GameBase.Instance.GetModule<ConfigManager>()
                .GetConfigByKey<string, GlobalConfig>(AAConst.GlobalConfig, GlobalConfigKeys.LocalNewDay).Value;
            localNewDayOffest = string.IsNullOrEmpty(offest) ? 0 : int.Parse(offest);
            CheckAfterLogin();
            GameBase.Instance.GetModule<TimingManager>().SubscribeSecondUpdate(this);
            //加载完成，发通知打点和上传存档
            GameBase.Instance.GetModule<EventManager>().Broadcast(new EventOnLoadFinished());
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();
            GameBase.Instance.GetModule<TimingManager>().UnsubscribeSecondUpdate(this);
        }

        /// <summary>
        /// 创建新存档
        /// </summary>
        protected void OnNewRecord()
        {
            PlayerRecord.ArchiveData.visitorId = GameBase.Instance.GetModule<ArchiveManager>().VisitorId;
            PlayerRecord.ArchiveData.vidCreateUtcTime = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
            PlayerRecord.ArchiveData.userId = GameBase.Instance.GetModule<ArchiveManager>().UserId;
            lastClientVer = null;
            lastLoginUtcTime = DateTime.UtcNow;
            lastLoginLocalTime = DateTime.Now;
            PlayerRecord.IsNewRecord = false;
            PlayerRecord.SetDirty();
            //新用户注册打点
            if (!string.IsNullOrEmpty(PlayerRecord.ArchiveData.userId))
            {
                GameBase.Instance.GetModule<EventManager>().Broadcast(new AnalyticsEvent(AnalyticsThirdPartyConfigKeys.New_Register));
            }
        }

        /// <summary>
        /// APP启动更新存档信息
        /// </summary>
        protected void OnAppLaunch()
        {
            var data = PlayerRecord.ArchiveData;
            data.deviceId = AppUtil.DeviceId;
            data.deviceName = AppUtil.DeviceName;
            if (data.clientVer != Application.version)
            {
                lastClientVer = data.clientVer;
                data.clientVer = Application.version;
            }
            data.appLaunchUtcTime = DateTimeOffset.Now.ToUnixTimeMilliseconds();
            lastClientVer = null;
            lastLoginUtcTime = DateTimeOffset.FromUnixTimeMilliseconds(data.dlyOpenUtcTime).UtcDateTime;
            lastLoginLocalTime = DateTimeOffset.FromUnixTimeMilliseconds(data.dlyOpenLocalTime).LocalDateTime;
            PlayerRecord.SetDirty();
        }

        /// <summary>
        /// 登录完成后检查
        /// </summary>
        public void CheckAfterLogin()
        {
            //检查APP升级事件
            if (lastClientVer != null)
            {
                GameBase.Instance.GetModule<EventManager>().Broadcast(new OnAppUpdatedEvent(PlayerRecord.ArchiveData.clientVer, lastClientVer));
                lastClientVer = null;
            }
            
            //检查连续登录重置
            var lastUtcTime = DateTimeOffset.FromUnixTimeMilliseconds(PlayerRecord.ArchiveData.dlyOpenUtcTime).UtcDateTime;
            if (DateTime.UtcNow.Date - lastUtcTime.Date > TimeSpan.FromDays(1))
            {
                var lastContinuePlayUtcDay = PlayerRecord.ArchiveData.continuePlayUtcDay;
                PlayerRecord.ArchiveData.continuePlayUtcDay = 0;
                PlayerRecord.SetDirty();
                GameBase.Instance.GetModule<EventManager>().Broadcast(new OnContinuePlayUtcDayReset(lastContinuePlayUtcDay));
            }
            var lastLocalTime = DateTimeOffset.FromUnixTimeMilliseconds(PlayerRecord.ArchiveData.dlyOpenLocalTime).LocalDateTime;
            if (DateTime.Now.Date - lastLocalTime.Date > TimeSpan.FromDays(1))
            {
                var lastContinuePlayLocalDay = PlayerRecord.ArchiveData.continuePlayLocalDay;
                PlayerRecord.ArchiveData.continuePlayLocalDay = 0;
                PlayerRecord.SetDirty();
                GameBase.Instance.GetModule<EventManager>().Broadcast(new OnContinuePlayLocalDayReset(lastContinuePlayLocalDay));
            }
            
            //检查跨天
            OnUpdateSecond();
        }

        public DateTime GetUtcNewDayDateTime => lastLoginUtcTime.Date.AddDays(1);
        
        public DateTime GetLocalNewDayDateTime => lastLoginLocalTime.Date.AddDays(1);
       

        /// <summary>
        /// 游戏运行中每秒检查
        /// </summary>
        public void OnUpdateSecond()
        {
            var nowUtcTime = DateTime.UtcNow;
            var nowLocalTime = DateTime.Now;

            DateTime? triggerNewUtcDay = null;
            DateTime? triggerNewLocalDay = null;
            //检查触发UTC跨天
            if (nowUtcTime.Date > lastLoginUtcTime.Date)
            {
                triggerNewUtcDay = DateTimeOffset.FromUnixTimeMilliseconds(PlayerRecord.ArchiveData.dlyOpenUtcTime).UtcDateTime;
                PlayerRecord.ArchiveData.dlyOpenUtcTime = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
                PlayerRecord.ArchiveData.playUtcDay++;
                PlayerRecord.ArchiveData.continuePlayUtcDay++;
                lastLoginUtcTime = nowUtcTime;
                PlayerRecord.SetDirty();
                LogPlayLocalDay();
            }
            //检查触发本地时区跨天
            if (nowLocalTime.Date.AddHours(localNewDayOffest) > lastLoginLocalTime.Date.AddHours(localNewDayOffest))
            {
                triggerNewLocalDay = DateTimeOffset.FromUnixTimeMilliseconds(PlayerRecord.ArchiveData.dlyOpenLocalTime).LocalDateTime;
                PlayerRecord.ArchiveData.dlyOpenLocalTime = DateTimeOffset.Now.ToUnixTimeMilliseconds();
                PlayerRecord.ArchiveData.playLocalDay++;
                PlayerRecord.ArchiveData.continuePlayLocalDay++;
                lastLoginLocalTime = nowLocalTime;
                PlayerRecord.SetDirty();
            }
            
            //发送跨天消息
            if (triggerNewUtcDay != null)
            {
                GameBase.Instance.GetModule<EventManager>().Broadcast(new OnNewUtcDayEvent(triggerNewUtcDay.Value, nowUtcTime));
            }
            if (triggerNewLocalDay != null)
            {
                GameBase.Instance.GetModule<EventManager>().Broadcast(new OnNewLocalDayEvent(triggerNewLocalDay.Value, nowLocalTime));
            }
        }

        /// <summary>
        /// 取PlayerInfo信息
        /// </summary>
        /// <param name="key">键</param>
        /// <param name="defaultValue">默认值</param>
        /// <typeparam name="T">类型</typeparam>
        /// <returns>值</returns>
        public T GetPlayerInfo<T>(string key, T defaultValue = default)
        {
            if (PlayerRecord.ArchiveData.playerInfo.TryGetValue(key, out var value))
            {
                try
                {
                    return (T)Convert.ChangeType(value, typeof(T));
                }
                catch
                {
                    return defaultValue;
                }
            }
            return defaultValue;
        }
        
        /// <summary>
        /// 设置PlayerInfo信息
        /// </summary>
        /// <param name="key">键</param>
        /// <param name="value">值</param>
        public void SetPlayerInfo(string key, object value)
        {
            if (string.IsNullOrEmpty(key)) return;
            if (!PlayerRecord.ArchiveData.playerInfo.TryGetValue(key, out var oldValue) || oldValue == null || !oldValue.Equals(value))
            {
                PlayerRecord.ArchiveData.playerInfo[key] = value;
                PlayerRecord.SetDirty();
            }
        }

        /// <summary>
        /// 是否包含PlayerInfo信息
        /// </summary>
        /// <param name="key">键</param>
        /// <returns>是否包含</returns>
        public bool ContainsPlayerInfo(string key)
        {
            return PlayerRecord.ArchiveData.playerInfo.ContainsKey(key);
        }

        #region 打点

        /// <summary>
        /// 每日登录打点
        /// </summary>
        protected void LogAppLaunch()
        {
            GameBase.Instance.GetModule<EventManager>()
                .Broadcast(new AnalyticsLogNewbieStepEvent(AnalyticsUserStepConfigKeys.first_launch));
            GameBase.Instance.GetModule<EventManager>()
                .Broadcast((AnalyticsEvent)new AnalyticsEvent_app_launch("normal", "normal"));
            GameBase.Instance.GetModule<EventManager>()
                .Broadcast(new AnalyticsEvent(AnalyticsThirdPartyConfigKeys.Login));
            
        }
        /// <summary>
        /// 连续登录打点事件上报
        /// </summary>
        void LogPlayLocalDay()
        {
            GameBase.Instance.GetModule<EventManager>()
                .Broadcast((AnalyticsEvent)new AnalyticsEvent_user_login(PlayerRecord.ArchiveData.continuePlayUtcDay, PlayerRecord.ArchiveData.playUtcDay));
            switch (PlayerRecord.ArchiveData.playUtcDay)
            {
                case 2:
                    GameBase.Instance.GetModule<EventManager>()
                        .Broadcast(new AnalyticsEvent(AnalyticsThirdPartyConfigKeys.Login_Day_2));
                    break;
                case 3:
                    GameBase.Instance.GetModule<EventManager>()
                        .Broadcast(new AnalyticsEvent(AnalyticsThirdPartyConfigKeys.Login_Day_3));
                    break;
                case 4:
                    GameBase.Instance.GetModule<EventManager>()
                        .Broadcast(new AnalyticsEvent(AnalyticsThirdPartyConfigKeys.Login_Day_4));
                    break;
                case 5:
                    GameBase.Instance.GetModule<EventManager>()
                        .Broadcast(new AnalyticsEvent(AnalyticsThirdPartyConfigKeys.Login_Day_5));
                    break;
                case 6:
                    GameBase.Instance.GetModule<EventManager>()
                        .Broadcast(new AnalyticsEvent(AnalyticsThirdPartyConfigKeys.Login_Day_6));
                    break;
                case 7:
                    GameBase.Instance.GetModule<EventManager>()
                        .Broadcast(new AnalyticsEvent(AnalyticsThirdPartyConfigKeys.Login_Day_7));
                    break;
                case 14:
                    GameBase.Instance.GetModule<EventManager>()
                        .Broadcast(new AnalyticsEvent(AnalyticsThirdPartyConfigKeys.Login_Day_14));
                    break;
                case 30:
                    GameBase.Instance.GetModule<EventManager>()
                        .Broadcast(new AnalyticsEvent(AnalyticsThirdPartyConfigKeys.Login_Day_30));
                    break;
            }
        }
        

        #endregion
    }
}
