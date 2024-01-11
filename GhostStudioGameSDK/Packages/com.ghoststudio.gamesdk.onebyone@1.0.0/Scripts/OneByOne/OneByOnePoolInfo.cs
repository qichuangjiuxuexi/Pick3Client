using System;
using System.Collections.Generic;
using System.Linq;

namespace AppBase.UI.OneByOne
{
    /// <summary>
    /// 事件池定义
    /// </summary>
    public class OneByOnePoolInfo
    {
        private OneByOnePoolConfig poolConfig;
        private List<OneByOnePoolItem> eventList = new();
        private List<OneByOnePoolItem> highEventList = new();

        /// <summary>
        /// 事件执行类型
        /// </summary>
        private static class PoolType
        {
            /// <summary>
            /// 顺序执行
            /// </summary>
            public const int Sequence = 0;
            /// <summary>
            /// 并发执行
            /// </summary>
            public const int Parallel = 1;
            /// <summary>
            /// 随机执行
            /// </summary>
            public const int Random = 2;
            /// <summary>
            /// 单一执行
            /// </summary>
            public const int Single = 3;
            /// <summary>
            /// 反复执行
            /// </summary>
            public const int Repeat = 4;
        }
        
        public OneByOnePoolInfo(OneByOnePoolConfig poolConfig)
        {
            this.poolConfig = poolConfig;
            Init();
        }

        /// <summary>
        /// 触发下一事件
        /// </summary>
        public void TriggerEvent(OneByOneTriggerData triggerData)
        {
            switch (poolConfig.pool_type)
            {
                case PoolType.Sequence:
                    TriggerSequenceEvent(triggerData);
                    break;
                case PoolType.Parallel:
                    TriggerParallelEvent(triggerData);
                    break;
                case PoolType.Random:
                    TriggerRandomEvent(triggerData);
                    break;
                case PoolType.Single:
                    TriggerSinghtEvent(triggerData);
                    break;
                case PoolType.Repeat:
                    TriggerRepeatEvent(triggerData);
                    break;
                default:
                    Debugger.LogError("OneByOneManager", $"Unknown pool type {poolConfig.pool_type} used by pool {poolConfig.pool_id}");
                    triggerData.nextPoolIndex++;
                    triggerData.nextEventIndex = 0;
                    triggerData.waitingEvents = 0;
                    triggerData.isUnbreakable = false;
                    triggerData.TriggerNextEvent();
                    break;
            }
        }

        /// <summary>
        /// 触发下一顺序事件
        /// </summary>
        private void TriggerSequenceEvent(OneByOneTriggerData triggerData)
        {
            for (int i = triggerData.nextEventIndex; i < eventList.Count; i++)
            {
                if (eventList[i].CheckFilters(triggerData))
                {
                    triggerData.nextEventIndex = i + 1;
                    triggerData.waitingEvents = 0;
                    triggerData.isUnbreakable = eventList[i].IsUnbreakable;
                    eventList[i].TriggerEvent(triggerData);
                    return;
                }
            }
            //没有事件符合
            triggerData.nextPoolIndex++;
            triggerData.nextEventIndex = 0;
            triggerData.waitingEvents = 0;
            triggerData.isUnbreakable = false;
            triggerData.TriggerNextEvent();
        }

        /// <summary>
        /// 触发并发事件
        /// </summary>
        private void TriggerParallelEvent(OneByOneTriggerData triggerData)
        {
            //正在收集并发事件回调
            if (triggerData.waitingEvents > 0)
            {
                triggerData.nextEventIndex++;
                //全部并发回调都收集完毕，调用下一个事件池
                if (triggerData.nextEventIndex >= triggerData.waitingEvents)
                {
                    triggerData.nextPoolIndex++;
                    triggerData.nextEventIndex = 0;
                    triggerData.waitingEvents = 0;
                    triggerData.isUnbreakable = false;
                    triggerData.TriggerNextEvent();
                }
                return;
            }
            
            //并发执行
            var list = eventList.Where(e => e.CheckFilters(triggerData)).ToList();
            if (list.Count > 0)
            {
                triggerData.waitingEvents = list.Count;
                triggerData.isUnbreakable = list.Any(e => e.IsUnbreakable);
                triggerData.nextEventIndex = 0;
                list.ForEach(e => e.TriggerEvent(triggerData));
            }
            else
            {
                //没有事件符合
                triggerData.nextPoolIndex++;
                triggerData.nextEventIndex = 0;
                triggerData.waitingEvents = 0;
                triggerData.isUnbreakable = false;
                triggerData.TriggerNextEvent();
            }
        }

        /// <summary>
        /// 触发随机事件
        /// </summary>
        private void TriggerRandomEvent(OneByOneTriggerData triggerData)
        {
            //优先触发高优先级事件
            for (int i = 0; i < highEventList.Count; i++)
            {
                if (highEventList[i].CheckFilters(triggerData))
                {
                    triggerData.nextPoolIndex++;
                    triggerData.nextEventIndex = 0;
                    triggerData.waitingEvents = 0;
                    triggerData.isUnbreakable = highEventList[i].IsUnbreakable;
                    highEventList[i].TriggerEvent(triggerData);
                    return;
                }
            }
            
            //随机选取事件
            var list = eventList.Where(e => e.CheckFilters(triggerData)).ToList();
            var sum = list.Sum(e => e.weight);
            if (sum > 0)
            {
                int random = UnityEngine.Random.Range(0, sum);
                sum = 0;
                foreach (var evt in list)
                {
                    sum += evt.weight;
                    if (random < sum)
                    {
                        triggerData.nextPoolIndex++;
                        triggerData.nextEventIndex = 0;
                        triggerData.waitingEvents = 0;
                        triggerData.isUnbreakable = evt.IsUnbreakable;
                        evt.TriggerEvent(triggerData);
                        return;
                    }
                }
            }
            
            //没有事件符合
            triggerData.nextPoolIndex++;
            triggerData.nextEventIndex = 0;
            triggerData.waitingEvents = 0;
            triggerData.isUnbreakable = false;
            triggerData.TriggerNextEvent();
        }

        /// <summary>
        /// 触发单一事件
        /// </summary>
        private void TriggerSinghtEvent(OneByOneTriggerData triggerData)
        {
            for (int i = 0; i < eventList.Count; i++)
            {
                if (eventList[i].CheckFilters(triggerData))
                {
                    triggerData.nextPoolIndex++;
                    triggerData.nextEventIndex = 0;
                    triggerData.waitingEvents = 0;
                    triggerData.isUnbreakable = eventList[i].IsUnbreakable;
                    eventList[i].TriggerEvent(triggerData);
                    return;
                }
            }
            //没有事件符合
            triggerData.nextPoolIndex++;
            triggerData.nextEventIndex = 0;
            triggerData.waitingEvents = 0;
            triggerData.isUnbreakable = false;
            triggerData.TriggerNextEvent();
        }

        /// <summary>
        /// 触发反复事件
        /// </summary>
        private void TriggerRepeatEvent(OneByOneTriggerData triggerData)
        {
            //最多反复执行50次，避免死循环
            if (triggerData.nextEventIndex < 50)
            {
                //选取一个可执行的事件
                for (int i = 0; i < eventList.Count; i++)
                {
                    if (eventList[i].CheckFilters(triggerData))
                    {
                        triggerData.nextEventIndex++;
                        triggerData.isUnbreakable = eventList[i].IsUnbreakable;
                        eventList[i].TriggerEvent(triggerData);
                        return;
                    }
                }
            }
            //没有事件符合
            triggerData.nextPoolIndex++;
            triggerData.nextEventIndex = 0;
            triggerData.waitingEvents = 0;
            triggerData.isUnbreakable = false;
            triggerData.TriggerNextEvent();
        }

        private void Init()
        {
            for (int i = 0; i < MaxEventCount; i++)
            {
                var eventId = GetEventId(i);
                var weight = GetWeight(i);
                if (eventId != 0 && weight >= 0)
                {
                    var eventInfo = new OneByOnePoolItem(i, eventId, GetFilters(i), weight);
                    if (poolConfig.pool_type == PoolType.Random && eventInfo.IsHighWeight)
                    {
                        highEventList.Add(eventInfo);
                    }
                    else
                    {
                        eventList.Add(eventInfo);
                    }
                }
            }
            eventList.Sort((a, b) => a.weight != b.weight ? b.weight - a.weight : a.eventIndex - b.eventIndex);
        }
        
        private const int MaxEventCount = 10;
        private int GetEventId(int index)
        {
            return index switch
            {
                0 => poolConfig.event_1,
                1 => poolConfig.event_2,
                2 => poolConfig.event_3,
                3 => poolConfig.event_4,
                4 => poolConfig.event_5,
                5 => poolConfig.event_6,
                6 => poolConfig.event_7,
                7 => poolConfig.event_8,
                8 => poolConfig.event_9,
                9 => poolConfig.event_10,
                _ => 0
            };
        }

        private int GetWeight(int index)
        {
            return index switch
            {
                0 => poolConfig.weight_1,
                1 => poolConfig.weight_2,
                2 => poolConfig.weight_3,
                3 => poolConfig.weight_4,
                4 => poolConfig.weight_5,
                5 => poolConfig.weight_6,
                6 => poolConfig.weight_7,
                7 => poolConfig.weight_8,
                8 => poolConfig.weight_9,
                9 => poolConfig.weight_10,
                _ => 0
            };
        }

        private List<int> GetFilters(int index)
        {
            return index switch
            {
                0 => poolConfig.filters_1,
                1 => poolConfig.filters_2,
                2 => poolConfig.filters_3,
                3 => poolConfig.filters_4,
                4 => poolConfig.filters_5,
                5 => poolConfig.filters_6,
                6 => poolConfig.filters_7,
                7 => poolConfig.filters_8,
                8 => poolConfig.filters_9,
                9 => poolConfig.filters_10,
                _ => null
            };
        }

        /// <summary>
        /// 检查是否有事件满足可触发条件
        /// </summary>
        /// <param name="triggerData">触发器数据</param>
        /// <returns>否有有事件可触发</returns>
        public bool CheckFilters(OneByOneTriggerData triggerData)
        {
            return highEventList.Any(e => e.CheckFilters(triggerData)) || eventList.Any(e => e.CheckFilters(triggerData));
        }
        
        /// <summary>
        /// 检查是否有不可中断的事件还未执行完
        /// </summary>
        /// <param name="triggerData">触发器数据</param>
        /// <returns>是否有还未执行完的不可中断事件</returns>
        public bool CheckUnbreakable(OneByOneTriggerData triggerData)
        {
            return highEventList.Any(e => e.CheckUnbreakable(triggerData)) || eventList.Any(e => e.CheckUnbreakable(triggerData));
        }
    }
}
