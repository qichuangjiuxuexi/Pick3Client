using System.Collections.Generic;
using UnityEngine;

namespace AppBase.OA.Configs
{
    [CreateAssetMenu(fileName = "MovePoint2LineAutoModeLineCenterStrategy",menuName = "OA动画/策略/移动-点成线-Auto模式下的中间点固定连线中间位置")]
    public class MovePoint2LineAutoModeLineCenterStrategy : BaseWorldPostionMoveAutoPathStrategy
    {
        public override List<Vector3>  GetAutoPathCtrlPoints(BaseObjectAnimConfig config,Vector3 newStartValue,Vector3 newEndValue)
        {
            WorldPositionMoveConfig realConfig = config as WorldPositionMoveConfig;
            Vector3 startValue = newStartValue;
            Vector3 endValue = newEndValue;
            Vector2 weights = realConfig.autoPathWeights;
            Vector3 middlePoint = Vector3.Lerp(startValue,endValue,0.5f);
            float distance = Vector3.Distance(startValue, endValue);
            middlePoint = middlePoint + (Vector3)(distance * weights);
            return new List<Vector3>() {startValue, middlePoint, endValue};
        }

        public override Vector2 GetAutoPathMiddlePointsWeight(BaseObjectAnimConfig config, List<Vector3> points)
        {
            return (config as WorldPositionMoveConfig).autoPathWeights;
        }

        public override bool IsHorizontal(BaseObjectAnimConfig config,Vector3 startValue,Vector3 endValue)
        {
            return false;
        }

        public override bool IsVertical(BaseObjectAnimConfig config,Vector3 startValue,Vector3 endValue)
        {
            return false;
        }

    }
}