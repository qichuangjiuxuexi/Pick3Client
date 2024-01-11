/**********************************************

Copyright(c) 2020 by Me2zen
All right reserved

Author : Terrence Rao
Date : 2020-07-27 11:25:18
Ver:1.1.8
Description :
ChangeLog :
**********************************************/

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using WordGame.Utils;

namespace WordGame.Utils
{
    
    /// <summary>
    /// 
    /// </summary>
    public static class BezierTool
    {
        /// <summary>
        /// 获取贝塞尔曲线(如果少于3个点，或者生成的曲线少于3个点，则返回默认值)
        /// </summary>
        /// <param name="PointsPositions">贝塞尔曲线点</param>
        /// <param name="createPointCount">生成路径点的数量</param>
        /// <returns></returns>
        public static Vector3[] GetBezierPath(List<Vector3> PointsPositions, int createPointCount = 20)
        {
            if (PointsPositions != null && PointsPositions.Count >= 3 && createPointCount > 3)
            {
                Bezier bezierCurve = new Bezier(PointsPositions);

                Vector3[] pos = new Vector3[createPointCount];
                for (int i = 0; i < createPointCount; i++)
                {
                    float process = i / (float) (createPointCount - 1);
                    //Debug.LogError(string.Format("{0}    {1}    {2}", i, createPointCount - 1, process)); ;
                    pos[i] = bezierCurve.GetPoint(process);
                }

                return pos;
            }
            else if (PointsPositions != null && PointsPositions.Count > 0)
            {
                Vector3[] ret = new Vector3[PointsPositions.Count];
                for (int i = 0; i < PointsPositions.Count; i++)
                {
                    ret[i] = PointsPositions[i];
                }

                return ret;
            }

            //异常处理
            return new Vector3[0];
        }
    }
}
