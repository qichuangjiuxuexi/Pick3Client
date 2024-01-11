using System.Collections;

/// <summary>
/// Loading流程基类
/// </summary>
public abstract class LoadingProcess
{
    /// <summary>
    /// 权重，用于计算总进度
    /// </summary>
    public float Weight { get; private set; }
    /// <summary>
    /// 当前流程进度
    /// </summary>
    public float Progress { get; protected set; }
    
    /// <summary>
    /// 流程处理逻辑
    /// </summary>
    public virtual IEnumerator Process()
    {
        yield break;
    }

    protected LoadingProcess(float weight)
    {
        Weight = weight;
    }
}
