using System;
using System.Collections.Generic;
using AppBase.OA.Behaviours;
using UnityEngine;

namespace AppBase.OA.Configs
{
    public enum PathType
    {
        // /// <summary>
        // /// 自动计算路径,曲线在始末点连线上方
        // /// </summary>
        // AutoAbove,
        //
        // /// <summary>
        // /// 自动计算路径，曲线在始末点连线下方
        // /// </summary>
        // AutoBelow,
        
        /// <summary>
        /// 自动模式，由autoPathWeights决定是从连线上方还是下方画弧。
        /// autoPathWeights中y > x则从上面，y=x 直线，否则从下方，如果始末点在x轴和y轴一致，则
        /// </summary>
        Auto,
        
        /// <summary>
        /// 由路径点生成的Bezier曲线的平滑路径
        /// </summary>
        BezierSmoothedWps,
        
        /// <summary>
        /// 严格的按照路标位置走(忽略config中的startValue和endValue)，不做任何处理，点与点之间走直线
        /// </summary>
        StrictWPs,
    }
    
    [OAConfigAttribute("移动-点成线模式",20)]
    [CreateAssetMenu(fileName = "WorldPositionMoveConfig",menuName = "OA动画/配置/移动/普通世界坐标移动配置")]
    public class WorldPositionMoveConfig : BaseObjectAnimConfig<Vector3>
    {
        /// <summary>
        /// 路径类型
        /// </summary>
        public PathType pathType;
        
        /// <summary>
        /// 当始末点连线不被视为垂直或者水平的线时，中间控制点的x、y坐标各自相对于起点的绝对值与全程距离的比值。
        /// </summary>
        public Vector2 autoPathWeights = Vector2.zero;

        /// <summary>
        /// 当始末点的x坐标一样时，该值的x为中间控制点与起点的连线和始末点连线的角度，往右为正，往左为负；y = （它的y坐标相对于起点y坐标的绝对值）/ （终点y坐标相对于起点y坐标的绝对值），一般取值【0，1】
        /// </summary>
        public Vector2 autoPathOffsetForVertical = new Vector2(30, 0.5f);
        
        /// <summary>
        /// 参考autoPathAngleOffsetForX的注释。
        /// </summary>
        public Vector2 autoPathOffsetForHorizt = new Vector2(30, 0.5f);

        /// <summary>
        /// 始末点连线与y轴成多少度以下时视为垂直向下的线
        /// </summary>
        public float maxAngleTreatAsVertical = 10;
        
        /// <summary>
        /// 始末点连线与x轴成多少度以下时视为垂直向下的线
        /// </summary>
        public float maxAngleTreatAsHorizontal = 10;
        
        
        /// <summary>
        /// 世界坐标下的路标坐标
        /// </summary>
        public List<Vector3> wps;

        public BaseWorldPostionMoveAutoPathStrategy autoPathStrategy;
        public BaseWorldPostionCorrectPathStrategy correctPathStrategy;
        /// <summary>
        /// pathType为StrictWPs时各段的曲线列表
        /// </summary>
        public List<AnimationCurve> strictWpsCurves;
        public override Type behaviourType
        {
            get
            {
                return typeof(WorldPostionMoveBehaviour);
            }
        }
    }
}