using System;
using System.Collections.Generic;
using System.Linq;
using AppBase.Config;
using AppBase.Module;
using AppBase.Timing;
using AppBase.Utils;

namespace AppBase.Activity
{
    /// <summary>
    /// 活动管理器
    /// </summary>
    public class ActivityManager : ModuleBase, IUpdateSecond
    {
        /// <summary>
        /// 注册的自定义活动类型
        /// </summary>
        protected Dictionary<string, Type> activityTypes = new();
        
        /// <summary>
        /// 活动字典
        /// </summary>
        protected Dictionary<int, ActivityModule> activityMap = new();
        
        /// <summary>
        /// 所有活动列表
        /// </summary>
        public List<ActivityModule> activityList = new();
        
        /// <summary>
        /// 活动分组字典
        /// </summary>
        protected Dictionary<string, ActivityGroup> activityGroups = new();
        
        /// <summary>
        /// 无分组活动列表
        /// </summary>
        protected List<ActivityModule> activityNoGroupList = new();
        
        protected override void OnInit()
        {
            base.OnInit();
            var configs = GameBase.Instance.GetModule<ConfigManager>().GetConfigList<ActivityConfig>(AAConst.ActivityConfig);
            configs.ForEach(InitActivity);
        }

        protected override void OnAfterInit()
        {
            base.OnAfterInit();
            UpdateActivities();
            GameBase.Instance.GetModule<TimingManager>().SubscribeSecondUpdate(this);
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();
            GameBase.Instance.GetModule<TimingManager>().UnsubscribeSecondUpdate(this);
        }

        /// <summary>
        /// 注册活动类型，如果不注册，则默认使用类型名字做活动名字
        /// </summary>
        /// <param name="activityName">活动名字</param>
        /// <param name="type">活动类型</param>
        public void RegisterActivityType(string activityName, Type type)
        {
            activityTypes[activityName] = type;
        }

        /// <summary>
        /// 初始化活动对象
        /// </summary>
        /// <param name="activityConfig">活动配置</param>
        protected void InitActivity(ActivityConfig activityConfig)
        {
            if (activityConfig == null) return;
            if (activityMap.ContainsKey(activityConfig.activity_id))
            {
                Debugger.LogError(TAG, $"Activity {activityConfig.activity_id} already exists");
                return;
            }
            
            //没有填写活动名字，使用默认的活动类型
            Type activityType;
            if (string.IsNullOrEmpty(activityConfig.activity_name))
            {
                activityType = typeof(ActivityModule);
            }
            //注册有活动名字，使用注册的活动类型，否则通过反射查找活动类型
            else if (!activityTypes.TryGetValue(activityConfig.activity_name, out activityType))
            {
                activityType = ReflectionUtil.GetType(ReflectionUtil.HotfixAsm, activityConfig.activity_name);
                if (activityType == null || !typeof(ModuleBase).IsAssignableFrom(activityType))
                {
                    Debugger.LogError(TAG, $"Activity type not found: {activityConfig.activity_name}");
                    return;
                }
            }
            
            //创建活动对象
            var activity = (ActivityModule)Activator.CreateInstance(activityType);
            if (activity == null) return;
            activityMap[activityConfig.activity_id] = activity;
            activityList.Add(activity);
            AddModule(activity, activityConfig);
            
            //注册分组
            if (activity.IsEnabled)
            {
                if (!string.IsNullOrEmpty(activity.Config.group_id))
                {
                    if (!activityGroups.TryGetValue(activity.Config.group_id, out var group))
                    {
                        group = new ActivityGroup();
                        activityGroups[activity.Config.group_id] = group;
                    }
                    group.AddActivity(activity);
                }
                else
                {
                    activityNoGroupList.Add(activity);
                }
            }
            
            Debugger.Log(TAG, $"InitActivity {activityConfig.activity_id} {activityConfig.activity_name}");
        }

        /// <summary>
        /// 根据活动ID取活动对象
        /// </summary>
        /// <param name="activityId">活动ID</param>
        /// <returns>活动对象</returns>
        public ActivityModule GetActivity(int activityId)
        {
            return activityMap.TryGetValue(activityId, out var activityModule) ? activityModule : null;
        }

        /// <summary>
        /// 根据活动ID取活动对象
        /// </summary>
        /// <param name="activityId">活动ID</param>
        /// <typeparam name="T">活动类型</typeparam>
        /// <returns>活动对象</returns>
        public T GetActivity<T>(int activityId) where T : ActivityModule
        {
            return GetActivity(activityId) as T;
        }
        
        /// <summary>
        /// 根据活动类型取活动对象
        /// </summary>
        /// <typeparam name="T">活动类型</typeparam>
        /// <returns>活动对象</returns>
        public T GetActivity<T>() where T : ActivityModule
        {
            return GetModule<T>();
        }

        /// <summary>
        /// 获取活动分组
        /// </summary>
        /// <param name="groupName">分组名称</param>
        /// <returns>活动分组</returns>
        public ActivityGroup GetActivityGroup(string groupName)
        {
            if (string.IsNullOrEmpty(groupName)) return null;
            return activityGroups.TryGetValue(groupName, out var group) ? group : null;
        }

        /// <summary>
        /// 获取所有开启的活动
        /// </summary>
        /// <param name="predicate">额外筛选条件</param>
        /// <returns>活动列表</returns>
        public List<ActivityModule> GetOpenActivities(Func<ActivityModule, bool> predicate = null)
        {
            var list = activityList.Where(a => a.IsOpen);
            if (predicate != null) list = list.Where(predicate);
            return list.ToList();
        }

        /// <summary>
        /// 每秒更新活动状态
        /// </summary>
        public void OnUpdateSecond()
        {
            UpdateActivities();
        }

        /// <summary>
        /// 更新活动状态
        /// </summary>
        public void UpdateActivities()
        {
            //更新无分组活动
            foreach (var activity in activityNoGroupList)
            {
                activity.UpdateActivity();
            }
            //更新分组活动
            foreach (var group in activityGroups.Values)
            {
                group.Update();
            }
        }
    }
}