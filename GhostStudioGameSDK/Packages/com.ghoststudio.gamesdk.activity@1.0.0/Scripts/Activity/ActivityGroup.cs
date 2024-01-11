using System.Collections.Generic;

namespace AppBase.Activity
{
    /// <summary>
    /// 活动分组
    /// </summary>
    public class ActivityGroup
    {
        /// <summary>
        /// 活动列表
        /// </summary>
        public List<ActivityModule> activities = new();

        /// <summary>
        /// 当前开启的活动
        /// </summary>
        public ActivityModule currentActivity;
        
        /// <summary>
        /// 添加活动
        /// </summary>
        public void AddActivity(ActivityModule activity)
        {
            if (activity == null || !activity.IsEnabled) return;
            activities.Add(activity);
            if (activity.IsOpen)
            {
                currentActivity = activity;
            }
            SortActivities();
        }

        /// <summary>
        /// 按活动优先级排序
        /// </summary>
        public void SortActivities()
        {
            activities.Sort((a, b) => a.Config.group_priority != b.Config.group_priority ? b.Config.group_priority - a.Config.group_priority : b.Id - a.Id);
        }

        /// <summary>
        /// 更新活动
        /// </summary>
        public void Update()
        {
            //如果当前有活动开启，则只更新当前活动
            if (currentActivity != null && currentActivity.IsOpen)
            {
                currentActivity.UpdateActivity();
            }
            //根据优先级轮询活动，找到第一个开启的活动
            if (currentActivity == null)
            {
                foreach (var activity in activities)
                {
                    activity.UpdateActivity();
                    if (activity.IsOpen)
                    {
                        currentActivity = activity;
                        break;
                    }
                }
            }
        }

        /// <summary>
        /// 设置当前开启的活动
        /// </summary>
        public void SetCurrentActivity(ActivityModule activity)
        {
            currentActivity = activity;
        }
        
        /// <summary>
        /// 取消设置当前开启的活动
        /// </summary>
        public void UnsetCurrentActivity(ActivityModule activity)
        {
            if (currentActivity == activity)
            {
                currentActivity = null;
            }
        }
    }
}