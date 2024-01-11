namespace AppBase.UI.OneByOne
{
    /// <summary>
    /// 触发器定义
    /// </summary>
    public class OneByOneTriggerInfo
    {
        private OneByOneTriggerConfig triggerConfig;
        
        public OneByOneTriggerInfo(OneByOneTriggerConfig triggerConfig)
        {
            this.triggerConfig = triggerConfig;
        }
        
        /// <summary>
        /// 触发下一个池子
        /// </summary>
        public void TriggerEvent(OneByOneTriggerData triggerData)
        {
            for (int i = triggerData.nextPoolIndex; i < triggerConfig.pool_ids.Count; i++)
            {
                var poolInfo = GameBase.Instance.GetModule<OneByOneManager>().GetPoolInfo(triggerConfig.pool_ids[i]);
                if (poolInfo != null)
                {
                    triggerData.nextPoolIndex = i;
                    poolInfo.TriggerEvent(triggerData);
                    return;
                }
            }
            //没有事件可触发，触发完成回调
            triggerData.waitingEvents = 0;
            triggerData.isUnbreakable = false;
            triggerData.OnFinishCallback();
        }
        
        /// <summary>
        /// 检查是否有事件满足可触发条件
        /// </summary>
        /// <param name="triggerData">触发器数据</param>
        /// <returns>否有有事件可触发</returns>
        public bool CheckFilters(OneByOneTriggerData triggerData)
        {
            var mgr = GameBase.Instance.GetModule<OneByOneManager>();
            for (int i = 0; i < triggerConfig.pool_ids.Count; i++)
            {
                var poolInfo = mgr.GetPoolInfo(triggerConfig.pool_ids[i]);
                if (poolInfo != null && poolInfo.CheckFilters(triggerData)) return true;
            }
            return false;
        }
        
        /// <summary>
        /// 检查是否有不可中断的事件还未执行完
        /// </summary>
        /// <param name="triggerData">触发器数据</param>
        /// <returns>是否有还未执行完的不可中断事件</returns>
        public bool CheckUnbreakable(OneByOneTriggerData triggerData)
        {
            for (int i = triggerData.nextPoolIndex; i < triggerConfig.pool_ids.Count; i++)
            {
                var poolInfo = GameBase.Instance.GetModule<OneByOneManager>().GetPoolInfo(triggerConfig.pool_ids[i]);
                if (poolInfo.CheckUnbreakable(triggerData)) return true;
            }
            return false;
        }
    }
}