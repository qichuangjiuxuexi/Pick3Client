using System;

namespace AppBase.UI.OneByOne
{
    /// <summary>
    /// 弹板池预制条件定义
    /// </summary>
    public class OneByOneConditionInfo
    {
        public int conditionId;
        public Func<string, OneByOneTriggerData, bool> conditionFunc;
    
        public OneByOneConditionInfo(int conditionId, Func<string, OneByOneTriggerData, bool> conditionFunc)
        {
            this.conditionId = conditionId;
            this.conditionFunc = conditionFunc;
        }
        
        /// <summary>
        /// 检查条件
        /// </summary>
        public bool CheckCondition(string conditionParam, OneByOneTriggerData triggerData, int conditionId)
        {
            if (conditionFunc == null)
            {
                Debugger.LogError("OneByOneManager", $"conditionId: {conditionId} not exist");
                return false;
            }
            return conditionId >= 0 ? conditionFunc.Invoke(conditionParam, triggerData) : !conditionFunc.Invoke(conditionParam, triggerData);
        }
    }
}