using System;
using AppBase.Event;
using AppBase.Fsm;
using AppBase.Timing;

namespace AppBase.Activity
{
    public partial class ActivityModule
    {
        /// <summary>
        /// 活动关闭状态
        /// </summary>
        public class ActivityCloseState : IFsmState
        {
            protected ActivityModule activity;
            public ActivityCloseState(ActivityModule activity)
            {
                this.activity = activity;
            }
            
            public void OnEnter(object param)
            {
                GameBase.Instance.GetModule<EventManager>().Broadcast(new OnActivityCloseEvent(activity));
                activity.Record.ArchiveData.status = (int)ActivityStateEnum.Close;
                activity.Record.SetDirty();
                GameBase.Instance.GetModule<ActivityManager>().GetActivityGroup(activity.Config.group_id)?.UnsetCurrentActivity(activity);
                activity.OnActivityClose();
            }

            public void OnExit()
            {
            }

            public void Update()
            {
                //检查活动开关
                if (!activity.IsEnabled) return;
                //检查活动轮数
                if (activity.Config.round_limit > 0 && activity.Record.ArchiveData.round > activity.Config.round_limit) return;
                //检查活动开启和结束时间
                var now = DateTime.UtcNow;
                //服务器时间
                if (activity.Config.timezone_type == (int)ActivityTimeZoneType.ServerUtc)
                {
                    var serverTime = GameBase.Instance.GetModule<TimingManager>().ServerTime;
                    if (serverTime == null) return;
                    now = serverTime.Value;
                }
                if (now < activity.openTime || now >= activity.endTime) return;
                //检查活动冷却时间
                if (activity.Config.round_duration_hours > 0)
                {
                    if (activity.Config.round_type == 2)//动态周期活动
                    {
                        var delta = (long)(now - activity.Record.ArchiveData.roundOpenTime).TotalSeconds;
                        var total = (long)((activity.Config.round_duration_hours + activity.Config.round_cd_hours) * 3600);
                        if (delta < total) return;
                    }
                    else
                    {
                        var delta = (long)(now - activity.openTime).TotalSeconds;
                        var total = (long)((activity.Config.round_duration_hours + activity.Config.round_cd_hours) * 3600);
                        var remain = delta % total;
                        if (remain >= activity.Config.round_duration_hours * 3600) return;
                    }
                }
                //检查活动过滤器
                if (!activity.CheckFilters()) return;
                activity.StateMachine.Change(ActivityStateEnum.Ready);
            }
        }
    }
}