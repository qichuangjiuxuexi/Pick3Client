using System;
using AppBase.Config;

namespace AppBase.UI.OneByOne
{
    /// <summary>
    /// 过滤器定义
    /// </summary>
    public class OneByOneFilterInfo
    {
        /// <summary>
        /// 过滤器ID
        /// </summary>
        public int filterId;
        
        /// <summary>
        /// 过滤器函数
        /// </summary>
        public Func<OneByOneTriggerData, bool> filterFunc;
        
        /// <summary>
        /// 代码注册过滤器
        /// </summary>
        public OneByOneFilterInfo(int filterId, Func<OneByOneTriggerData, bool> filterFunc)
        {
            this.filterId = filterId;
            this.filterFunc = filterFunc;
        }
        
        /// <summary>
        /// 使用预制条件配置过滤器
        /// </summary>
        public OneByOneFilterInfo(OneByOneFilterConfig config)
        {
            filterId = config.filter_id;
            filterFunc = FilterByConfig;
        }
        
        /// <summary>
        /// 检查过滤器
        /// </summary>
        public bool CheckFilter(OneByOneTriggerData triggerData)
        {
            if (filterFunc == null)
            {
                Debugger.LogError("OneByOneManager", $"filterId: {filterId} not exist");
                return false;
            }
            return filterFunc.Invoke(triggerData);
        }
        
        /// <summary>
        /// 检查过滤器,如果filterId 传负数里面直接取反
        /// </summary>
        /// <param name="triggerData"></param>
        /// <param name="filterId"></param>
        /// <returns></returns>
        public bool CheckFilterWithId(OneByOneTriggerData triggerData, int filterId)
        {
            return filterId >= 0 ? CheckFilter(triggerData) : !CheckFilter(triggerData);
        }

        /// <summary>
        /// 使用预制条件配置过滤器
        /// </summary>
        private bool FilterByConfig(OneByOneTriggerData triggerData)
        {
            var config = GameBase.Instance.GetModule<ConfigManager>().GetConfigByKey<int, OneByOneFilterConfig>(AAConst.OneByOneFilterConfig, filterId);
            if (config == null) return false;
            if (config.filter_id == 0)
            {
                Debugger.LogError("OneByOneManager", $"Custom filter {filterId} not implemented!");
                return false;
            }
            var conditionInfo = GameBase.Instance.GetModule<OneByOneManager>().GetConditionInfo(config.condition_id);
            if (conditionInfo == null)
            {
                Debugger.LogError("OneByOneManager", $"Condition {config.condition_id} used by filter {filterId} is not implemented!");
                return false;
            }
            return conditionInfo.CheckCondition(config.condition_param, triggerData, config.condition_id);
        }
    }
}