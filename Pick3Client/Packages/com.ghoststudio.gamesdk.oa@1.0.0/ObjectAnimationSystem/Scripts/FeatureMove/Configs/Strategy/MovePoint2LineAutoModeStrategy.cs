using System.Collections.Generic;
using UnityEngine;

namespace AppBase.OA.Configs
{
    [CreateAssetMenu(fileName = "DefaultMovePoint2LineAutoModeStrategy",menuName = "OA动画/策略/移动-点成线-Auto模式下的中间点自适应策略")]
    public class MovePoint2LineAutoModeStrategy : BaseWorldPostionMoveAutoPathStrategy
    {
        public override List<Vector3>  GetAutoPathCtrlPoints(BaseObjectAnimConfig config,Vector3 newStartValue,Vector3 newEndValue)
        {
            WorldPositionMoveConfig realConfig = config as WorldPositionMoveConfig;
            Vector3 startValue = newStartValue;
            Vector3 endValue = newEndValue;
            Vector2 weights = realConfig.autoPathWeights;
            Vector3 middlePoint = Vector3.Lerp(startValue,endValue,0.5f);
            if (isNeedConsiderHV)
            {
                bool isVertical = IsVertical(realConfig,startValue,endValue);
                if (isVertical)
                {
                    weights = realConfig.autoPathOffsetForVertical;
                    if (endValue.y >= startValue.y)
                    {
                        middlePoint = new Vector3(startValue.x + (endValue.y - startValue.y) * weights.x,
                            LerpEx(startValue.y, endValue.y,weights.y), LerpEx(startValue.z, endValue.z,0.5f));
                    }
                    else
                    {
                        middlePoint = new Vector3(startValue.x + (startValue.y - endValue.y) * weights.x,
                            LerpEx(startValue.y, endValue.y,weights.y), LerpEx(startValue.z, endValue.z,0.5f));
                    }
                }
                else
                {
                        bool isHorizontal = IsHorizontal(realConfig,startValue,endValue);
                        if (isHorizontal)
                        {
                            weights = realConfig.autoPathOffsetForHorizt; 
                            if (endValue.x >= startValue.x)
                            {
                                middlePoint = new Vector3(LerpEx(startValue.x, endValue.x,weights.x),
                                    startValue.y + (endValue.x - startValue.x) * weights.y,
                                    LerpEx(startValue.z, endValue.z,0.5f));
                            }
                            else
                            {
                                middlePoint = new Vector3(LerpEx(startValue.x, endValue.x,weights.x),
                                    startValue.y + (startValue.x - endValue.x) * weights.y,
                                    LerpEx(startValue.z, endValue.z,0.5f));
                            }
                        }
                        else
                        {
                            middlePoint = GetAutoPathMiddlePointDefault(startValue, endValue, weights);
                        }
                }
            }
            else
            {
                middlePoint = GetAutoPathMiddlePointDefault(startValue, endValue, weights);
            }
            return new List<Vector3>() {startValue, middlePoint, endValue};
        }

        Vector3 GetAutoPathMiddlePointDefault(Vector3 startValue, Vector3 endValue, Vector2 weights)
        {
            Vector3 pointM1 = new Vector3(LerpEx(startValue.x, endValue.x, weights.x),
                LerpEx(startValue.y, endValue.y, weights.x), LerpEx(startValue.z, endValue.z, weights.x));
            //1表示逆时针旋转，-1表示顺时针
            float dir = Vector3.Dot(endValue - startValue, Vector3.right)  >= 0 && weights.x >= 0 ? 1 : -1;
            Quaternion q = Quaternion.Euler(0, 0, dir * 90);
            Vector3 rotated = q * (pointM1 - startValue).normalized * weights.y *
                              (Vector3.Distance(startValue, endValue));
            if (isDebug)
            {
                Debug.DrawLine(pointM1, pointM1 + rotated);
                Debug.DrawLine(startValue, endValue);
            }

            Vector3 middlePoint = pointM1 + rotated;
            return middlePoint;
        }

        public override Vector2 GetAutoPathMiddlePointsWeight(BaseObjectAnimConfig config, List<Vector3> points)
        {
            WorldPositionMoveConfig realConfig = config as WorldPositionMoveConfig;
            Vector2 weights = Vector3.one * 0.5f;
            if (points.Count < 3)
            {
                return weights;
            }

            Vector3 line1 = (realConfig.endValue - realConfig.startValue);
            Vector3 line2 = (points[1] - realConfig.startValue);
            float l1Mg = line1.sqrMagnitude;
            float x = Vector3.Dot(line1, line2) / l1Mg;
            weights.x = x;
            Vector3 pointM1 = new Vector3(LerpEx(realConfig.startValue.x, realConfig.endValue.x, x),
                LerpEx(realConfig.startValue.y, realConfig.endValue.y, x),
                LerpEx(realConfig.startValue.z, realConfig.endValue.z, x));
            weights.y = (points[1] - pointM1).magnitude / (realConfig.endValue - realConfig.startValue).magnitude;
            weights.y *= Vector3.Dot(points[1] - pointM1, Vector3.up) > 0 ? 1 : -1;

            return weights;
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