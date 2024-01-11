using AppBase.Event;
using UnityEngine;

namespace AppBase.Timing
{
    /// <summary>
    /// 游戏时机管理器运行时组件
    /// </summary>
    public class TimingRuntimeComponent : MonoBehaviour
    {
        public TimingManager timingManager;
        
        public void Init(TimingManager timingManager)
        {
            this.timingManager = timingManager;
        }

        protected void Update()
        {
            timingManager?.Update();
            if (Input.GetKeyDown(KeyCode.Escape))
            {
                GameBase.Instance.GetModule<EventManager>().Broadcast(new EventOnAndroidBack());
            }
        }
        
        protected void OnApplicationPause(bool pauseStatus)
        {
            timingManager?.OnApplicationPause(pauseStatus);
        }
        
        protected void OnApplicationQuit()
        {
            timingManager?.OnApplicationQuit();
        }
    }
}