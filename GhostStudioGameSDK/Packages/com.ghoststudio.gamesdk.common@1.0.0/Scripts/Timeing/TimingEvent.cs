using AppBase.Event;

namespace AppBase.Timing
{
    /// <summary>
    /// 游戏切换到前台时触发
    /// </summary>
    public struct EventOnGameFocus : IEvent
    {
    }
    
    /// <summary>
    /// 游戏切换到后台时触发
    /// </summary>
    public struct EventOnGamePause : IEvent
    {
    }
    
    /// <summary>
    /// 游戏退出时触发
    /// </summary>
    public struct EventOnGameQuit : IEvent
    {
    }
    
    /// <summary>
    /// 游戏加载完成时触发
    /// </summary>
    public struct EventOnLoadFinished : IEvent
    {
    }
    
    /// <summary>
    /// Android 按 back 键时触发
    /// </summary>
    public struct EventOnAndroidBack : IEvent
    {
    }
}
