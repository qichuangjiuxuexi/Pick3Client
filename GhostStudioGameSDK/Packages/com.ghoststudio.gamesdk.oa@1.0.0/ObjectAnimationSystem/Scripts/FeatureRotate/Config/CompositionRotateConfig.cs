using System;
using AppBase.OA.Behaviours;
using UnityEngine;

namespace AppBase.OA.Config
{
    [OAConfigAttribute("旋转-三轴合成",30)]
    [CreateAssetMenu(fileName = "CompositionRotateConfig",menuName = "OA动画/配置/旋转/三轴合成")]
    public class CompositionRotateConfig :  BaseObjectAnimConfig<float>
    {
        public bool isWorldRotation;
        public AnimationCurve xAxisCurve;
        public AnimationCurve yAxisCurve;
        public AnimationCurve zAxisCurve;
        public Vector3 xRange;
        public Vector3 yRange;
        public Vector3 zRange;
        public bool xPassZero = false;
        public bool yPassZero = false;
        public bool zPassZero = false;
        public override Type behaviourType
        {
            get
            {
                return typeof(CompositionRotateBehaviour);
            }
        }
    }
}