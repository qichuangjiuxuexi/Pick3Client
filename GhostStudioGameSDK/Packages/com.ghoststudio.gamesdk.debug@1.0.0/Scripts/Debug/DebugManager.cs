using AppBase.Module;

namespace AppBase.Debugging
{
    /// <summary>
    /// Debug工具管理器
    /// </summary>
    public class DebugManager : ModuleBase
    {
        public SRDebugManager SRDebug { get; private set; }
        public RemoteDebugManager RemoteDebug { get; private set; }
        
        protected override void OnInit()
        {
            base.OnInit();
            SRDebug = AddModule<SRDebugManager>();
            RemoteDebug = AddModule<RemoteDebugManager>();
        }
    }
}
