using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using AppBase;
using AppBase.Config;
using AppBase.Event;
using AppBase.Localization;
using AppBase.Module;
using AppBase.Timing;
using AppBase.Utils;
using NotificationSamples;
using UnityEngine;
#if UNITY_ANDROID
using Unity.Notifications.Android;
using UnityEngine.Android;
#elif UNITY_IOS
using Unity.Notifications.iOS;
#endif
public class SimpleNotification
{
    /// <summary>
    /// 推送channel
    /// </summary>
    public string channelID;
    
    /// <summary>
    /// 标题
    /// </summary>
    public string title;
    
    /// <summary>
    /// 次标题
    /// </summary>
    public string subTitle;
    
    /// <summary>
    /// 推送的内容
    /// </summary>
    public string content;
    
    /// <summary>
    /// 多少秒后推送
    /// </summary>
    public int waitSecondsToFire;
    
}

public class LocalNotificationManager : MonoModule
{
    private CustomGameNotificationsManager notificationsManager;
    private HashSet<Func<SimpleNotification>> dynamicNotificationDelegates;
    private GameNotificationChannel[] channels;

    private bool IsNotice = true;

    private Action<bool> OnRequestCallBack;

    protected override void OnInit()
    {
        base.OnInit();
        notificationsManager = GameObject.AddComponent<CustomGameNotificationsManager>();
        //2.2.2版本默认是QueueClearAndReschedule，不够灵活且对于具有动态时间的推送来讲实现起来比较复杂，因此统一设置为NoQueue，以保持最大的灵活性
        // notificationsManager.Mode = GameNotificationsManager.OperatingMode.ClearOnForegrounding;
        dynamicNotificationDelegates ??= new HashSet<Func<SimpleNotification>>();
    }

    private IEnumerator Initialize()
    {
        notificationsManager.onApplicationFocus += ScheduleAll;
        var config = GameBase.Instance.GetModule<ConfigManager>()
            .GetConfigList<LocalNotificationChannelConfig>(AAConst.LocalNotificationChannelConfig);
        channels = new GameNotificationChannel[config.Count];
        for (int i = 0; i < channels.Length; i++)
        {
            channels[i] = new GameNotificationChannel(config[i].ID, config[i].name, config[i].description,
                (GameNotificationChannel.NotificationStyle)config[i].style, config[i].showBadge,
                config[i].showLights,
                config[i].vibrates, config[i].highPriority, (GameNotificationChannel.PrivacyMode)config[i].privacy);
        }

        yield return notificationsManager.Initialize(channels);
        notificationsManager.LocalNotificationDelivered += OnReceiveNotification;
        notificationsManager.LocalNotificationExpired += OnNotificationExpired;
    }

    public void OnInitialize()
    {
        notificationsManager.StartCoroutine(Initialize());
    }


    void OnReceiveNotification(PendingNotification target)
    {
        if (AppUtil.IsDebug)
        {
            Debug.LogWarning("[LocalNotification] receive notification:" + target.Notification.Body);
        }
    }

    void OnNotificationExpired(PendingNotification target)
    {
        if (AppUtil.IsDebug)
        {
            Debug.LogWarning("[LocalNotification] notification expired:" + target.Notification.Body);
        }
    }

    public bool IsOpenAppFromNotification()
    {
#if UNITY_ANDROID
        AndroidNotificationIntentData intentData = AndroidNotificationCenter.GetLastNotificationIntent();
        if (intentData != null)
        {
            string strIntentData = intentData.Notification.IntentData;
            return true;
        }
#elif UNITY_IOS
        // int cause = _GetValueByNSUserDefaults("notificationLaunch");
        // return cause > 0;
        if (AppUtil.IsDebug)
        {
            Debugger.LogDWarning("iOS上暂未实现判断是否从推送打开的游戏，因为需要改MainAppcontroller.mm，如无必改的理由，最好别改");
        }
        return false;
#endif
        return false;
    }
// #if UNITY_IOS
//     [DllImport("__Internal")]
//     private static extern int _GetValueByNSUserDefaults(string message);
// #endif


    public void RegisterDynamicDelegats(Func<SimpleNotification> func)
    {
        dynamicNotificationDelegates?.Add(func);
    }

    public void UnRegisterDynamicDelegats(Func<SimpleNotification> func)
    {
        dynamicNotificationDelegates?.Remove(func);
    }

    void ScheduleAll(bool focus)
    {
        if (notificationsManager == null || !notificationsManager.Initialized)
        {
            return;
        }

        if (focus)
        {
            notificationsManager.CancelAllNotifications();
            return;
        }
        //增加notice判定 *
        if(!IsNotice) return;

        if (dynamicNotificationDelegates != null)
        {
            //动态推送，比如比赛要结束了，领奖期要结束了
            ScheduleDynamicNotification();
        }

        ScheduleNormalNotification();
    }

    /// <summary>
    /// 动态推送，比如比赛要结束了，领奖期要结束了等等
    /// </summary>
    void ScheduleDynamicNotification()
    {
        var iter = dynamicNotificationDelegates.GetEnumerator();
        while (iter.MoveNext())
        {
            SimpleNotification result = iter.Current?.Invoke();
            if (result != null)
            {
                if (notificationsManager)
                {
                    var notification = notificationsManager.CreateNotification();
                    notification.Title = result.title;
                    notification.Subtitle = result.subTitle;
                    notification.Body = result.content;
                    notification.Group = result.channelID;
                    notification.DeliveryTime = DateTime.Now.AddSeconds(result.waitSecondsToFire);
                    notification.Id =
                        Math.Abs(notification.DeliveryTime.Value.ToString("yyMMddHHmmssffffff").GetHashCode());
                    notificationsManager.ScheduleNotification(notification);
                }
            }
        }
    }

    public void ScheduleNewNotification(string title, string subTitle, string content, string groupID,
        DateTime fireTime)
    {
        if (notificationsManager)
        {
            var notification = notificationsManager.CreateNotification();
            notification.Title = title;
            notification.Subtitle = subTitle;
            notification.Body = content;
            notification.Group = groupID;
            notification.DeliveryTime = fireTime;
            notification.Id = Math.Abs(fireTime.ToString("yyMMddHHmmssffffff").GetHashCode());
            notificationsManager.ScheduleNotification(notification);
        }
    }

    /// <summary>
    /// 设定日常推送
    /// </summary>
    void ScheduleNormalNotification()
    {
        var configs = GameBase.Instance.GetModule<ConfigManager>()?
            .GetConfigList<LocalNotificationConfig>(AAConst.LocalNotificationConfig);
        for (int i = 0; i < configs.Count; i++)
        {
            ScheduleOneLine(configs[i]);
        }
    }

    void ScheduleOneLine(LocalNotificationConfig config)
    {
        if (config.contentList.Count == 0 || config.fireDays.Count == 0)
        {
            return;
        }

        var date = DateTime.Now.Date;
        var textManger = GameBase.Instance.GetModule<LocalizationManager>();
        var listDays = config.fireDays;
        var firstfireTime = date.AddHours(config.hour).AddMinutes(config.min).AddSeconds(config.second);
        for (int j = 0; j < listDays.Count; j++)
        {
            var notification = notificationsManager.CreateNotification();
            notification.Group = config.group;
            int index = 0;
            switch (config.selectionMode)
            {
                case 0:
                    index = UnityEngine.Random.Range(0, config.contentList.Count);
                    break;
                case 1:
                    index = j % config.contentList.Count;
                    break;
            }

            if (index < config.titleList.Count)
            {
                notification.Title = textManger.GetText(config.titleList[index]);
            }

            if (index < config.subTitleList.Count)
            {
                notification.Subtitle = textManger.GetText(config.subTitleList[index]);
            }

            string key = config.contentList[index];
            notification.Body = textManger.GetText(key);
            var fireTime = firstfireTime.AddDays(listDays[j]);
            if (fireTime < DateTime.Now)
            {
                continue;
            }

            notification.DeliveryTime = fireTime;
            notification.Id = Math.Abs(fireTime.ToString("yyMMddHHmmssffffff").GetHashCode());
            notificationsManager.ScheduleNotification(notification);
            if (AppUtil.IsDebug)
            {
                Debug.LogWarningFormat("[LocalNotification]设定了静态推送,ID:{0},时间：{1}", notification.Id,
                    fireTime.ToString("yyyy-MM-dd HH:mm:ss"));
            }
        }
    }

    public void SetOnRequestCallBack(Action<bool> callBack)
    {
        OnRequestCallBack = callBack;
    }

    /// <summary>
    /// 判定权限
    /// </summary>
    /// <returns></returns>
    public bool IsCanNotice()
    {
#if UNITY_ANDROID
        var status = AndroidNotificationCenter.UserPermissionToPost;
        switch (status)
        {
            case PermissionStatus.NotRequested:
            case PermissionStatus.Denied:
            case PermissionStatus.DeniedDontAskAgain:
                return false;
        }
#elif UNITY_IOS
            var oldStatus = iOSNotificationCenter.GetNotificationSettings().AuthorizationStatus;
            return oldStatus is not AuthorizationStatus.NotDetermined or AuthorizationStatus.Denied;
#endif
        return true;
    }

    /// <summary>
    /// 是否允许通知
    /// </summary>
    /// <returns></returns>
    public bool IsShowNotification()
    {
        return IsNotice && IsCanNotice();
    }

    /// <summary>
    /// 设置权限
    /// </summary>
    private IEnumerator SetPermission()
    {
        if (!notificationsManager.Initialized)
        {
            yield return Initialize();
        }

        yield return notificationsManager.Platform.RequestNotificationPermission();
#if UNITY_ANDROID || UNITY_IOS
        OnRequestCallBack?.Invoke(IsCanNotice());
#endif
    }

    /// <summary>
    /// 外部调用
    /// </summary>
    /// <param name="isNotice"></param>
    public void SetIsNotice(bool isNotice)
    {
        IsNotice = isNotice;
    }

    /// <summary>
    /// 切换Notice状态
    /// </summary>
    public void ChangeNoticeState()
    {
        if (IsShowNotification())
        {
            IsNotice = false;
        }
        else
        {
            notificationsManager.StartCoroutine(SetPermission());
        }
    }

    protected override void OnDestroy()
    {
        base.OnDestroy();
        notificationsManager.LocalNotificationDelivered -= OnReceiveNotification;
        notificationsManager.LocalNotificationExpired -= OnNotificationExpired;
        notificationsManager.onApplicationFocus -= ScheduleAll;
    }
}
