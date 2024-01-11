using System;
using AppBase.Event;
using AppBase.Fsm;
using AppBase.Timing;

namespace AppBase.Activity
{
    public partial class ActivityModule
    {
        /// <summary>
        /// 活动开启状态
        /// </summary>
        public class ActivityOpenState : IFsmState
        {
            protected ActivityModule activity;
            public ActivityOpenState(ActivityModule activity)
            {
                this.activity = activity;
            }

            public void OnEnter(object param)
            {
                //计算当前轮结束时间
                var now = DateTime.UtcNow;
                //服务器时间
                if (activity.Config.timezone_type == (int)ActivityTimeZoneType.ServerUtc)
                {
                    var serverTime = GameBase.Instance.GetModule<TimingManager>().ServerTime;
                    if (serverTime != null) now = serverTime.Value;
                }
                switch (activity.Config.round_type)
                {
                    case 0: //固定周期活动
                        if (activity.Config.round_duration_hours > 0)
                        {
                            var delta = (long)(now - activity.openTime).TotalSeconds;
                            var total = (long)((activity.Config.round_duration_hours + activity.Config.round_cd_hours) * 3600);
                            var lastDuration = activity.Config.round_duration_hours * 3600 - delta % total;
                            activity.Record.ArchiveData.roundEndTime = now.AddSeconds(lastDuration);
                        }
                        else
                        {
                            activity.Record.ArchiveData.roundEndTime = activity.endTime; //常驻活动
                        }
                        break;
                    case 1: //每月活动
                        if (activity.Config.timezone_type == (int)ActivityTimeZoneType.LocalTimeZone)
                        {
                            //本地时间1号
                            var localNow = DateTime.Now;
                            var day1 = localNow.Date.AddDays(-(localNow.Day - 1)).ToUniversalTime();
                            activity.Record.ArchiveData.roundEndTime = day1.AddMonths(1);
                        }
                        else
                        {
                            //utc时间1号
                            var day1 = now.Date.AddDays(-(now.Day - 1));
                            activity.Record.ArchiveData.roundEndTime = day1.AddMonths(1);
                        }
                        break;
                    case 2: //动态周期
                        activity.Record.ArchiveData.roundEndTime = activity.Config.round_duration_hours > 0 ?
                            now.AddHours(activity.Config.round_duration_hours) : //有轮次时长
                            activity.endTime; //常驻活动
                        break;
                }
                if (activity.Record.ArchiveData.roundEndTime > activity.endTime)
                {
                    activity.Record.ArchiveData.roundEndTime = activity.endTime;
                }

                activity.Record.ArchiveData.roundOpenTime = now;
                //设置轮次数据
                activity.Record.ArchiveData.status = (int)ActivityStateEnum.Open;
                activity.Record.ArchiveData.round++;
                activity.Record.ArchiveData.roundData.Clear();
                activity.Record.SetDirty();
                GameBase.Instance.GetModule<ActivityManager>().GetActivityGroup(activity.Config.group_id)?.SetCurrentActivity(activity);
                activity.OnActivityOpen();
                GameBase.Instance.GetModule<EventManager>().Broadcast(new OnActivityOpenEvent(activity));
            }

            public void OnExit()
            {
            }

            public void Update()
            {
                //检查活动轮次结束
                var now = DateTime.UtcNow;
                if (now >= activity.Record.ArchiveData.roundEndTime || now >= activity.endTime)
                {
                    activity.StateMachine.Change(ActivityStateEnum.Finish);
                }
            }
        }
    }
}