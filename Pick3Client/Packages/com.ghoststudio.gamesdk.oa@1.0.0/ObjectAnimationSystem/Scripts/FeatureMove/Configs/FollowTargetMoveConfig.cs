using System;
using AppBase.OA.Behaviours;
using UnityEngine;

namespace AppBase.OA.Configs
{
    /// <summary>
    /// 所谓球坐标跟随模式，就是以目标点为球坐标原点，以参数r、θ、φ表示本物体相对于目标点的位置，球坐标系解释可详见
    /// https://baike.baidu.com/item/%E7%90%83%E5%9D%90%E6%A0%87%E7%B3%BB
    /// r、θ、φ的数值存依次在StartValue或EndValue中的x、y、z分量中。r∈[0,+∞)，θ∈[0, 180]， φ∈[0,360]
    /// </summary>
    [OAConfigAttribute("移动-球坐标跟随模式",30)]
    [CreateAssetMenu(fileName = "FollowTargetMoveConfig",menuName = "OA动画/配置/移动/球坐标跟随模式移动配置")]
    public class FollowTargetMoveConfig : BaseObjectAnimConfig<Vector3>
    {
        public AnimationCurve rCurve;
        public AnimationCurve thetaCurve;
        public AnimationCurve faiCurve;
        public bool freezeTheta;
        public bool freezeFai;
        /// <summary>
        /// 为了预览方便加进去的
        /// </summary>
        private Transform followTarget;

        public Transform GetFollowTarget()
        {
            return followTarget;
        }

        public void SetFollowTarget(Transform trf)
        {
            followTarget = trf;
        }
        public override Type behaviourType
        {
            get
            {
                return typeof(FollowTargetMoveBehaviour);
            }
        }
    }
}