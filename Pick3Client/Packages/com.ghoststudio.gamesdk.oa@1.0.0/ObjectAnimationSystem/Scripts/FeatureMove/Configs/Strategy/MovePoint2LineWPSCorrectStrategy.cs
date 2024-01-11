using System.Collections.Generic;
using UnityEngine;

namespace AppBase.OA.Configs
{
    [CreateAssetMenu(fileName = "DefaultMovePoint2LineWPSCorrectStrategy",menuName = "OA动画/策略/点成线非Auto模式的路径自适应策略")]
    /// <summary>
    /// 策略是：对于点线模式的移动，路径模式为非Auto时，如果改变了起点和终点，中间点的自适应策略：从第二个点开始，其偏移量逐渐由起点的偏移量过渡到终点的偏移量。特征就是如果起点和终点的偏移量一致，那么就相当于曲线整体偏移
    /// </summary>
    public class MovePoint2LineWPSCorrectStrategy : BaseWorldPostionCorrectPathStrategy
    {
        public override List<Vector3> GetAutoPathCtrlPoints(BaseObjectAnimConfig config, Vector3 startVal, Vector3 endVal)
        {
            WorldPositionMoveConfig realConfig = config as WorldPositionMoveConfig;
            if (realConfig == null || realConfig.wps.Count < 3)
            {
                return new List<Vector3>(0);
            }
            //判断是否左右翻转翻转
            // if (IsHorizontalFlip(realConfig,startVal,endVal))
            // {
            //     return CorrectHorizontalFlip(realConfig, startVal, endVal);
            // }else if (IsVerticalFlip(realConfig,startVal,endVal))
            // {
            //     return CorrectVerticalFlip(realConfig, startVal, endVal);
            // }
            //判断是否上下翻转
            //默认偏移的形式
            return CorrectDefault(realConfig,startVal,endVal);
        }

        List<Vector3> CorrectDefault(WorldPositionMoveConfig realConfig, Vector3 startVal, Vector3 endVal)
        {
            List<Vector3> result = new List<Vector3>(realConfig.wps.Count);
            Vector3 startOffset = startVal - realConfig.wps[0];
            Vector3 endOffset = endVal - realConfig.wps[^1];
            result.Add(startVal);
            for (int i = 1; i < realConfig.wps.Count - 1; i++)
            {
                result.Add(realConfig.wps[i] +
                           Vector3.Lerp(startOffset, endOffset, i / (float) (realConfig.wps.Count - 2)));
            }
            result.Add(endVal);
            return result;
        }

        bool IsHorizontalFlip(WorldPositionMoveConfig realConfig, Vector3 startVal, Vector3 endVal)
        {
            float oldDelta = realConfig.endValue.x - realConfig.startValue.x;
            float newDelta = endVal.x - endVal.x;
            if (oldDelta * newDelta < 0)
            {
                return true;
            }
            return false;
        }
        
        List<Vector3> CorrectHorizontalFlip(WorldPositionMoveConfig realConfig, Vector3 startVal, Vector3 endVal)
        {
            List<Vector3> result = new List<Vector3>(realConfig.wps.Count);
            Vector3[] newWps = new Vector3[realConfig.wps.Count];
            for (int i = 0; i < newWps.Length; i++)
            {
                newWps[i] = new Vector3(realConfig.wps[i].x * -1, realConfig.wps[i].y, realConfig.wps[i].z);
            }
            Vector3 startOffset = startVal - newWps[0];
            Vector3 endOffset = endVal - newWps[^1];
            result.Add(startVal);
            for (int i = 1; i < newWps.Length - 1; i++)
            {
                result.Add(newWps[i] +
                           Vector3.Lerp(startOffset, endOffset, i / (float) (realConfig.wps.Count - 2)));
            }
            result.Add(endVal);
            return result;
        }
        
        bool IsVerticalFlip(WorldPositionMoveConfig realConfig, Vector3 startVal, Vector3 endVal)
        {
            return false;
        }
        
        List<Vector3> CorrectVerticalFlip(WorldPositionMoveConfig realConfig, Vector3 startVal, Vector3 endVal)
        {
            List<Vector3> result = new List<Vector3>(realConfig.wps.Count);
            Vector3[] newWps = new Vector3[realConfig.wps.Count];
            for (int i = 0; i < newWps.Length; i++)
            {
                newWps[i] = new Vector3(realConfig.wps[i].x, realConfig.wps[i].y * -1, realConfig.wps[i].z);
            }
            Vector3 startOffset = startVal - newWps[0];
            Vector3 endOffset = endVal - newWps[^1];
            result.Add(startVal);
            for (int i = 1; i < newWps.Length - 1; i++)
            {
                result.Add(newWps[i] +
                           Vector3.Lerp(startOffset, endOffset, i / (float) (realConfig.wps.Count - 2)));
            }
            result.Add(endVal);
            return result;
        }
    }
}