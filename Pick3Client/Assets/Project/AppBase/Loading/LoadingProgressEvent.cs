using AppBase.Event;

public struct LoadingProgressEvent : IEvent
{
    public float progress;
    
    public LoadingProgressEvent(float progress)
    {
        this.progress = progress;
    }
}
