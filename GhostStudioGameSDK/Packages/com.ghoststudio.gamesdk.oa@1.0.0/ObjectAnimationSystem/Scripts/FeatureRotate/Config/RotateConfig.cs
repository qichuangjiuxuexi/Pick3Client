using System;
using AppBase.OA.Behaviours.Config.Strategy;
using UnityEngine;

namespace AppBase.OA.Behaviours.Config
{
    [OAConfigAttribute("旋转-单轴增量旋转",40)]
    [CreateAssetMenu(fileName = "RotateConfig",menuName = "OA动画/配置/旋转/单轴增量旋转")]
    public class RotateConfig :  BaseObjectAnimConfig<float>
    {
        public bool isWorldRotation = false;
        public BaseAxisModifierStrategy axisStrategy;
        public Vector3 rotateAxis;
        public override Type behaviourType
        {
            get
            {
                return typeof(RotateBehaviour);
            }
        }
    }
}