using System;
using AppBase.Config;
using AppBase.Event;
using AppBase.UI.Dialog;
using AppBase.Utils;

namespace AppBase.UI.OneByOne
{
    /// <summary>
    /// OneByOne事件定义
    /// </summary>
    public class OneByOneEventInfo
    {
        /// <summary>
        /// 事件ID
        /// </summary>
        public int eventId;

        /// <summary>
        /// 触发函数
        /// </summary>
        public Action<OneByOneTriggerData, Action> triggerAction;
        
        /// <summary>
        /// 过滤器函数
        /// </summary>
        public Func<OneByOneTriggerData, bool> filterFunc;

        /// <summary>
        /// 事件配置
        /// </summary>
        public OneByOneEventConfig eventConfig => _eventConfig ??= GameBase.Instance.GetModule<ConfigManager>().GetConfigByKey<int, OneByOneEventConfig>(AAConst.OneByOneEventConfig, eventId);
        protected OneByOneEventConfig _eventConfig;
        
        /// <summary>
        /// 使用代码注册事件
        /// </summary>
        /// <param name="eventId">事件id</param>
        /// <param name="triggerAction">事件触发回调</param>
        /// <param name="filterFunc">自带过滤器，可留空</param>
        public OneByOneEventInfo(int eventId, Action<OneByOneTriggerData, Action> triggerAction, Func<OneByOneTriggerData, bool> filterFunc = null)
        {
            this.eventId = eventId;
            this.triggerAction = triggerAction;
            this.filterFunc = filterFunc;
        }

        /// <summary>
        /// 使用配置定义事件
        /// </summary>
        public OneByOneEventInfo(OneByOneEventConfig eventConfig)
        {
            eventId = eventConfig.event_id;
            triggerAction = TriggerByConfig;
            filterFunc = FilterByConfig;
        }

        /// <summary>
        /// 触发事件
        /// </summary>
        public void TriggerEvent(OneByOneTriggerData triggerData)
        {
            if (triggerAction != null)
            {
                bool isInvoked = false;
                triggerAction.Invoke(triggerData, () =>
                {
                    //防止回调被多次调用
                    if (isInvoked)
                    {
                        Debugger.LogError(nameof(OneByOneManager), $"callback twice: triggerId = {triggerData.triggerId}, eventId = {eventId}, action = {triggerAction}");
                        return;
                    }
                    isInvoked = true;
                    triggerData.TriggerNextEvent();
                });
            }
            else
            {
                triggerData.TriggerNextEvent();
            }
        }

        /// <summary>
        /// 事件类型
        /// </summary>
        private static class EventType
        {
            /// <summary>
            /// 代码注册的自定义事件
            /// </summary>
            public const int Custom = 0;
            /// <summary>
            /// 弹出弹板
            /// </summary>
            public const int Dialog = 1;
            /// <summary>
            /// 发送串行事件
            /// </summary>
            public const int Event = 2;
            /// <summary>
            /// 发送并发事件
            /// </summary>
            public const int ParallelEvent = 3;
        }

        /// <summary>
        /// 触发使用配置定义的事件
        /// </summary>
        private void TriggerByConfig(OneByOneTriggerData triggerData, Action callback)
        {
            switch (eventConfig?.event_type)
            {
                case EventType.Dialog:
                    var address = AAConst.GetAddress(eventConfig.event_name);
                    if (string.IsNullOrEmpty(address))
                    {
                        Debugger.LogError("OneByOneManager", $"Dialog address {eventConfig.event_name} not found!");
                        callback?.Invoke();
                        return;
                    }
                    var extraData = string.IsNullOrEmpty(eventConfig.extra_data) ? null : eventConfig.extra_data;
                    var dialogData = new DialogData(address, extraData);
                    dialogData.context.Add(new OneByOneDialogContext(triggerData, callback));
                    GameBase.Instance.GetModule<DialogManager>().PopupDialog(dialogData);
                    return;
                case EventType.Event:
                    extraData = string.IsNullOrEmpty(eventConfig.extra_data) ? null : eventConfig.extra_data;
                    var eventType = ReflectionUtil.GetType(ReflectionUtil.HotfixAsm, eventConfig.event_name) ?? typeof(OneByOneEvent);
                    var evt = (IEvent)Activator.CreateInstance(eventType, eventConfig.event_name, extraData, triggerData);
                    GameBase.Instance.GetModule<EventManager>().Broadcast(evt, callback);
                    return;
                case EventType.ParallelEvent:
                    extraData = string.IsNullOrEmpty(eventConfig.extra_data) ? null : eventConfig.extra_data;
                    eventType = ReflectionUtil.GetType(ReflectionUtil.HotfixAsm, eventConfig.event_name) ?? typeof(OneByOneParallelEvent);
                    evt = (IEvent)Activator.CreateInstance(eventType, eventConfig.event_name, extraData, triggerData);
                    GameBase.Instance.GetModule<EventManager>().Broadcast(evt, callback);
                    return;
            }
            Debugger.LogError("OneByOneManager", $"Custom events {eventId} not implemented!");
            callback?.Invoke();
        }

        private bool FilterByConfig(OneByOneTriggerData triggerData)
        {
            if (eventConfig == null) return false;
            if (eventConfig.filters == null || eventConfig.filters.Count == 0) return true;
            foreach (var filterId in eventConfig.filters)
            {
                if (filterId == 0) continue;
                var filterInfo = GameBase.Instance.GetModule<OneByOneManager>().GetFilterInfo(filterId);
                if (filterInfo == null) return false;
                if (!filterInfo.CheckFilterWithId(triggerData, filterId)) return false;
            }
            return true;
        }

        /// <summary>
        /// 检查是否满足可触发条件
        /// </summary>
        /// <param name="triggerData">触发器数据</param>
        /// <returns>否有可触发</returns>
        public bool CheckFilter(OneByOneTriggerData triggerData)
        {
            if (filterFunc != null && !filterFunc.Invoke(triggerData)) return false;
            return true;
        }
        
        /// <summary>
        /// 是否标记不可中断
        /// </summary>
        public bool IsUnbreakable => eventConfig?.is_unbreakable == true;
        
        /// <summary>
        /// 检查事件是否不可中断
        /// </summary>
        /// <param name="triggerData">触发器数据</param>
        /// <returns>是否不可中断</returns>
        public bool CheckUnbreakable(OneByOneTriggerData triggerData)
        {
            return IsUnbreakable && CheckFilter(triggerData);
        }
    }
}