using System;
using System.Collections.Generic;
using AppBase.OA.Behaviours;
using UnityEngine;

namespace AppBase.OA.Configs
{

    public enum PositionType
    {
        WorldPosition,
        LocalPosition,
        AnchoredPosition,
    }
    
    [OAConfigAttribute("移动-三轴合成",10)]
    [CreateAssetMenu(fileName = "CompositionAxisPostitionConfig",menuName = "OA动画/配置/移动/三轴合成")]
    public class CompositionAxisPostitionConfig : BaseObjectAnimConfig<Vector3>
    {
        public bool useSeperateAxis;
        public PositionType postionType;
        public AnimationCurve xCurve;
        public AnimationCurve yCurve;
        public AnimationCurve zCurve;
        public AnimationCurve combinedCurve;
        public bool foceUserConfigPos = false;
        public override Type behaviourType
        {
            get
            {
                return typeof(CompositionAxisPostitionBehaviour);
            }
        }
    }
}