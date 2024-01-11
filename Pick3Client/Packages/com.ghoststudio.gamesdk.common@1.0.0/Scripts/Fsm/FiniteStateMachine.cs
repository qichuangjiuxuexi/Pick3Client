using System;
using System.Collections.Generic;
using UnityEngine;

namespace AppBase.Fsm
{
    /// <summary>
    /// 有限状态机
    /// </summary>
    /// <typeparam name="T">状态名称枚举</typeparam>
    public class FiniteStateMachine<T> where T : Enum
    {
        /// <summary>
        /// 当前状态名称
        /// </summary>
        public T CurStateName => curStateName;
        protected T curStateName;
        
        /// <summary>
        /// 当前状态
        /// </summary>
        public IFsmState CurState => curState;
        protected IFsmState curState;
        protected Dictionary<T, IFsmState> allState = new ();
        
        /// <summary>
        /// 增加状态列表
        /// </summary>
        /// <param name="states">状态字典</param>
        public void AddState(Dictionary<T, IFsmState> states)
        {
            if (states != null)
            {
                foreach (var item in states)
                {
                    AddState(item.Key, item.Value);
                }
            }
        }
        
        /// <summary>
        /// 增加状态
        /// </summary>
        /// <param name="stateName">状态名称</param>
        /// <param name="state">状态</param>
        public void AddState(T stateName, IFsmState state)
        {
            if (state != null)
            {
                if (allState.ContainsKey(stateName))
                {
                    Debug.Log($"fsm already exists name {stateName}");
                    return;
                }
                allState.Add(stateName, state);
            }
        }
        
        /// <summary>
        /// 变更状态
        /// </summary>
        /// <param name="stateName">状态名称</param>
        /// <param name="param">参数</param>
        /// <returns>新状态</returns>
        public IFsmState Change(T stateName, object param = default)
        {
            allState.TryGetValue(stateName, out var state);
            if (curState != null)
            {
                if (curState == state) return curState;
                curState.OnExit();
                curState = null;
                curStateName = default;
            }
            if (state != null)
            {
                curStateName = stateName;
                curState = state;
                curState.OnEnter(param);
            }
            return curState;
        }
        
        /// <summary>
        /// 设置初始状态
        /// </summary>
        /// <param name="stateName">状态名称</param>
        /// <returns>状态</returns>
        public IFsmState Init(T stateName)
        {
            allState.TryGetValue(stateName, out var state);
            if (state != null)
            {
                curStateName = stateName;
                curState = state;
            }
            return curState;
        }
        
        /// <summary>
        /// 更新状态
        /// </summary>
        public void Update()
        {
            curState?.Update();
        }
    }
}
