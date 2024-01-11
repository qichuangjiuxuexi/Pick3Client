
namespace AppBase.OA
{
    /// <summary>
    /// 有些动画时长需要根据实际情况进行调整，定义其调整策略
    /// </summary>
    public abstract class BaseObjectAnimDurationCorrectStrategy<T> : BaseObjectAnimStrategy
    {
        public abstract float GetDynamicDuraion(BaseObjectAnimConfig config,T startValue,T endValue);
    }
}