namespace AppBase.Event
{
    /// <summary>
    /// 事件类型基类，继承可以用class也可以用struct，建议用struct继承以避免GC
    /// </summary>
    public interface IEvent
    {
    }
    
    /// <summary>
    /// 并发的事件类型，同时调用所有监听者，并等待所有回调返回
    /// </summary>
    public interface IParallelEvent : IEvent
    {
    }
    
    /// <summary>
    /// 可取消的事件类型，监听者可以通过设置IsCanceled来取消事件传播
    /// </summary>
    public class CancelableEvent : IEvent
    {
        /// <summary>
        /// 标记事件是否被取消，不要再向后传播
        /// </summary>
        public bool IsCanceled;
    }
}
