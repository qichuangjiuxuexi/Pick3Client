using UnityEngine;

namespace AppBase.OA.Configs
{
    [CreateAssetMenu(menuName = "OA动画/策略/动画时长矫正策略")]
    /// <summary>
    /// 真正的时间与始末点距离成正比
    /// </summary>
    public class DurationCorrectStrategyVector3 : BaseObjectAnimDurationCorrectStrategy<Vector3>
    {
        public override float GetDynamicDuraion(BaseObjectAnimConfig config,Vector3 start,Vector3 end)
        {
            if (config == null)
            {
                return 0;
            }
            var realConfig = config as BaseObjectAnimConfig<Vector3>;
            if(Mathf.Approximately(realConfig.duration,0))
            {
                return realConfig.duration;
            }

            return realConfig.duration * (Vector3.Distance(start, end) /
                                          (Vector3.Distance(realConfig.startValue, realConfig.endValue)));
        }
    }
}