using System;
using AppBase.OA.Behaviours;
using AppBase.OA.Behaviours.Config.Strategy;
using UnityEngine;

namespace AppBase.OA.Config
{
    [OAConfigAttribute("缩放-三轴合成",50)]
    [CreateAssetMenu(fileName = "CompositionRotateConfig",menuName = "OA动画/配置/缩放/三轴合成")]
    public class CompositionScaleConfig :  BaseObjectAnimConfig<float>
    {
        public AnimationCurve xAxisCurve;
        public AnimationCurve yAxisCurve;
        public AnimationCurve zAxisCurve;
        public float xBase = 1;
        public float yBase = 1;
        public float zBase = 1;
        public override Type behaviourType
        {
            get
            {
                return typeof(CompositionScaleBehaviour);
            }
        }
    }
}