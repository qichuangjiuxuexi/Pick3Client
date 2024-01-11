using UnityEngine;

namespace AppBase.OA.Behaviours.Config.Strategy
{
    [CreateAssetMenu(fileName = "BaseAxisModifierStrategy" ,menuName = "OA动画/策略/默认的变旋转轴策略")]
    public class BaseAxisModifierStrategy : BaseObjectAnimStrategy
    {
        public Vector3 GetAxis(BaseObjectAnimConfig config,float progress)
        {
            return (config as RotateConfig).rotateAxis;
        }
    }
}