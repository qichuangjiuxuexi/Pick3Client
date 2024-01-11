using System.Collections.Generic;
using UnityEngine;

namespace AppBase.OA.Configs
{
    public abstract class BaseWorldPostionMoveAutoPathStrategy : BaseObjectAnimStrategy
    {
        /// <summary>
        /// 使否需要考虑水平与竖直连线的问题
        /// </summary>
        public bool isNeedConsiderHV;
        public bool isDebug;
        public abstract List<Vector3> GetAutoPathCtrlPoints(BaseObjectAnimConfig config,Vector3 newStartValue,Vector3 newEndValue);
        public abstract Vector2 GetAutoPathMiddlePointsWeight(BaseObjectAnimConfig config,List<Vector3> points);

        public abstract bool IsVertical(BaseObjectAnimConfig config,Vector3 startValue,Vector3 endValue);
        public abstract bool IsHorizontal(BaseObjectAnimConfig config,Vector3 startValue,Vector3 endValue);
        public float LerpEx(float start, float end, float t)
        {
            return start + (end - start) * t;
        }
        
    }
}