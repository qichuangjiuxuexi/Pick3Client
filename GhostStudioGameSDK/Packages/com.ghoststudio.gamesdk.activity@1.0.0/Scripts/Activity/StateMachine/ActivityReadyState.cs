using System;
using AppBase.Event;
using AppBase.Fsm;
using AppBase.Timing;

namespace AppBase.Activity
{
    public partial class ActivityModule
    {
        /// <summary>
        /// 活动准备开启状态
        /// </summary>
        public class ActivityReadyState : IFsmState
        {
            protected ActivityModule activity;
            public ActivityReadyState(ActivityModule activity)
            {
                this.activity = activity;
            }

            public void OnEnter(object param)
            {
                activity.Record.ArchiveData.status = (int)ActivityStateEnum.Close; //存档里没有准备状态，只有关闭状态
                //下赛季已准备开始，需要获取当前赛季显示入口
                activity.Record.ArchiveData.roundThemeName = activity.ConfigThemeName;
                activity.Record.SetDirty();
                GameBase.Instance.GetModule<ActivityManager>().GetActivityGroup(activity.Config.group_id)?.SetCurrentActivity(activity);
                GameBase.Instance.GetModule<EventManager>().Broadcast(new OnActivityReadyEvent(activity));
                activity.OnActivityReady();
                if (activity.StateMachine.CurStateName == ActivityStateEnum.Ready && activity.Config.auto_open)
                {
                    activity.StateMachine.Change(ActivityStateEnum.Open);
                }
            }

            public void OnExit()
            {
            }

            public void Update()
            {
                //检查活动结束时间
                var now = DateTime.UtcNow;
                //服务器时间
                if (activity.Config.timezone_type == (int)ActivityTimeZoneType.ServerUtc)
                {
                    var serverTime = GameBase.Instance.GetModule<TimingManager>().ServerTime;
                    if (serverTime == null) return;
                    now = serverTime.Value;
                }
                //超过配置结束时间跳转关闭
                if (now >= activity.endTime)
                {
                    activity.StateMachine.Change(ActivityStateEnum.Close);
                }
            }
        }
    }
}