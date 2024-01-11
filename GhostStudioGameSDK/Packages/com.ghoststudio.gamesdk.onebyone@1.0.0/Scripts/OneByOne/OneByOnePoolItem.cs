using System.Collections.Generic;
using System.Linq;

namespace AppBase.UI.OneByOne
{
    /// <summary>
    /// 事件池中的一个事件定义
    /// </summary>
    public class OneByOnePoolItem
    {
        /// <summary>
        /// 事件顺序
        /// </summary>
        public int eventIndex;
        
        /// <summary>
        /// 事件ID
        /// </summary>
        public int eventId;
        
        /// <summary>
        /// 过滤器表
        /// </summary>
        public List<int> filterIds;
        
        /// <summary>
        /// 权重
        /// </summary>
        public int weight;
        
        /// <summary>
        /// 事件定义
        /// </summary>
        public OneByOneEventInfo eventInfo => GameBase.Instance.GetModule<OneByOneManager>().GetEventInfo(eventId);
        
        public OneByOnePoolItem(int index, int eventId, List<int> filterIds, int weight)
        {
            this.eventIndex = index;
            this.eventId = eventId;
            this.filterIds = filterIds;
            this.weight = weight;
        }
        
        /// <summary>
        /// 检查过滤器
        /// </summary>
        public bool CheckFilters(OneByOneTriggerData triggerData)
        {
            if (eventId == 0 || weight < 0) return false;
            if (eventInfo == null) return false;
            if (eventInfo.filterFunc != null && !eventInfo.filterFunc.Invoke(triggerData)) return false;
            if (filterIds == null || filterIds.Count == 0) return true;
            return filterIds.All(f => f == 0 || GameBase.Instance.GetModule<OneByOneManager>().GetFilterInfo(f)?.CheckFilterWithId(triggerData, f) == true);
        }

        /// <summary>
        /// 高优先级事件，当weight >= 10000时，随机优先取该事件
        /// </summary>
        public bool IsHighWeight => weight >= 10000;

        /// <summary>
        /// 触发事件
        /// </summary>
        public void TriggerEvent(OneByOneTriggerData triggerData)
        {
            if (eventInfo != null)
            {
                eventInfo.TriggerEvent(triggerData);
            }
            else
            {
                triggerData.TriggerNextEvent();
            }
        }

        /// <summary>
        /// 是否标记不可中断
        /// </summary>
        public bool IsUnbreakable => eventInfo?.IsUnbreakable == true;
        
        /// <summary>
        /// 检查事件是否不可中断
        /// </summary>
        /// <param name="triggerData">触发器数据</param>
        /// <returns>是否不可中断</returns>
        public bool CheckUnbreakable(OneByOneTriggerData triggerData)
        {
            return IsUnbreakable && CheckFilters(triggerData);
        }
    }
}