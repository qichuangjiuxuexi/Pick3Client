using System;
using System.Collections.Generic;
using AppBase.Config;
using AppBase.Module;

namespace AppBase.UI.OneByOne
{
    /// <summary>
    /// 弹板池管理器
    /// </summary>
    public class OneByOneManager : ModuleBase
    {
        protected Dictionary<string, OneByOneTriggerInfo> triggerInfos = new();
        protected Dictionary<int, OneByOnePoolInfo> poolInfos = new();
        protected Dictionary<int, OneByOneEventInfo> eventInfos = new();
        protected Dictionary<int, OneByOneFilterInfo> filterInfos = new();
        protected Dictionary<int, OneByOneConditionInfo> conditionInfos = new();

        protected override void OnInit()
        {
            base.OnInit();
            var triggerConfigs = GameBase.Instance.GetModule<ConfigManager>().GetConfigList<OneByOneTriggerConfig>(AAConst.OneByOneTriggerConfig);
            foreach (var triggerConfig in triggerConfigs)
            {
                triggerInfos[triggerConfig.trigger_name] = new OneByOneTriggerInfo(triggerConfig);
            }
            var poolConfigs = GameBase.Instance.GetModule<ConfigManager>().GetConfigList<OneByOnePoolConfig>(AAConst.OneByOnePoolConfig);
            foreach (var poolConfig in poolConfigs)
            {
                poolInfos[poolConfig.pool_id] = new OneByOnePoolInfo(poolConfig);
            }
            var eventConfigs = GameBase.Instance.GetModule<ConfigManager>().GetConfigList<OneByOneEventConfig>(AAConst.OneByOneEventConfig);
            foreach (var eventConfig in eventConfigs)
            {
                eventInfos[eventConfig.event_id] = new OneByOneEventInfo(eventConfig);
            }
            var filterConfigs = GameBase.Instance.GetModule<ConfigManager>().GetConfigList<OneByOneFilterConfig>(AAConst.OneByOneFilterConfig);
            foreach (var filterConfig in filterConfigs)
            {
                filterInfos[filterConfig.filter_id] = new OneByOneFilterInfo(filterConfig);
            }
        }

        /// <summary>
        /// 触发OneByOne
        /// </summary>
        /// <param name="triggerData">触发器数据</param>
        public void TriggerEvent(OneByOneTriggerData triggerData)
        {
            if (triggerData == null || string.IsNullOrEmpty(triggerData.triggerId)) return;
            if (triggerData.isInterupted) return;
            if (!triggerInfos.TryGetValue(triggerData.triggerId, out var triggerInfo)) return;
            triggerInfo.TriggerEvent(triggerData);
        }

        /// <summary>
        /// 取事件池定义
        /// </summary>
        public OneByOnePoolInfo GetPoolInfo(int poolId)
        {
            return poolInfos.TryGetValue(poolId, out var poolInfo) ? poolInfo : null;
        }

        /// <summary>
        /// 注册事件定义
        /// </summary>
        public void RegisterEventInfo(OneByOneEventInfo eventInfo)
        {
            if (eventInfo == null || eventInfo.eventId == 0) return;
            eventInfos[eventInfo.eventId] = eventInfo;
        }
        
        /// <summary>
        /// 注册过滤器定义
        /// </summary>
        public void RegisterFilterInfo(OneByOneFilterInfo filterInfo)
        {
            if (filterInfo == null || filterInfo.filterId == 0) return;
            filterInfos[filterInfo.filterId] = filterInfo;
        }

        /// <summary>
        /// 取事件定义
        /// </summary>
        public OneByOneEventInfo GetEventInfo(int eventId)
        {
            return eventInfos.TryGetValue(eventId, out var eventInfo) ? eventInfo : null;
        }
        
        /// <summary>
        /// 取过滤器定义
        /// </summary>
        public OneByOneFilterInfo GetFilterInfo(int filterId)
        {
            return filterInfos.TryGetValue(Math.Abs(filterId), out var filterInfo) ? filterInfo : null;
        }
        
        /// <summary>
        /// 注册预制条件定义
        /// </summary>
        /// <param name="conditionInfo"></param>
        public void RegisterConditionInfo(OneByOneConditionInfo conditionInfo)
        {
            if (conditionInfo == null || conditionInfo.conditionId == 0) return;
            conditionInfos[conditionInfo.conditionId] = conditionInfo;
        }
        
        /// <summary>
        /// 取预制条件定义
        /// </summary>
        public OneByOneConditionInfo GetConditionInfo(int conditionId)
        {
            return conditionInfos.TryGetValue(Math.Abs(conditionId), out var info) ? info : null;
        }

        /// <summary>
        /// 检查是否有不可中断的事件还未执行完
        /// </summary>
        /// <param name="triggerData">触发器数据</param>
        /// <returns>是否有还未执行完的不可中断事件</returns>
        public bool CheckUnbreakable(OneByOneTriggerData triggerData)
        {
            if (triggerData == null || string.IsNullOrEmpty(triggerData.triggerId)) return false;
            if (triggerData.isInterupted) return false;
            if (triggerData.isUnbreakable) return true;
            if (!triggerInfos.TryGetValue(triggerData.triggerId, out var triggerInfo)) return false;
            return triggerInfo.CheckUnbreakable(triggerData);
        }
    }
}