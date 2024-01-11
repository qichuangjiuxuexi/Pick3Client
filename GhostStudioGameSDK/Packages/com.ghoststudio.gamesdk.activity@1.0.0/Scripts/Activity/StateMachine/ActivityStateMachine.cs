using System.Collections.Generic;
using AppBase.Fsm;

namespace AppBase.Activity
{
    public partial class ActivityModule
    {
        /// <summary>
        /// 活动状态机
        /// </summary>
        public class ActivityStateMachine : FiniteStateMachine<ActivityStateEnum>
        {
            public ActivityStateMachine(ActivityModule activity)
            {
                AddState(new Dictionary<ActivityStateEnum, IFsmState>
                {
                    { ActivityStateEnum.Close, new ActivityCloseState(activity) },
                    { ActivityStateEnum.Ready, new ActivityReadyState(activity) },
                    { ActivityStateEnum.Open, new ActivityOpenState(activity) },
                    { ActivityStateEnum.Finish, new ActivityFinishState(activity) },
                });
            }
        }
    }
}
