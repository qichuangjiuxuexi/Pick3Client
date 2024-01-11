using AppBase.Event;
using AppBase.Fsm;

namespace AppBase.Activity
{
    public partial class ActivityModule
    {
        /// <summary>
        /// 活动完成，等待关闭
        /// </summary>
        public class ActivityFinishState : IFsmState
        {
            protected ActivityModule activity;
            public ActivityFinishState(ActivityModule activity)
            {
                this.activity = activity;
            }

            public void OnEnter(object param)
            {
                activity.Record.ArchiveData.status = (int)ActivityStateEnum.Finish;
                activity.Record.SetDirty();
                GameBase.Instance.GetModule<ActivityManager>().GetActivityGroup(activity.Config.group_id)?.SetCurrentActivity(activity);
                GameBase.Instance.GetModule<EventManager>().Broadcast(new OnActivityFinishEvent(activity));
                activity.OnActivityFinish();
            }

            public void OnExit()
            {
            }

            public void Update()
            {
            }
        }
    }
}