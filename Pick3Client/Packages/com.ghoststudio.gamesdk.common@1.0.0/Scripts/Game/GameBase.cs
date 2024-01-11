using System.Collections;
using System.Collections.Generic;
using AppBase.Event;
using AppBase.Module;
using AppBase.Timing;
using AppBase.Utils;

namespace AppBase
{
    /// <summary>
    /// Game基类
    /// </summary>
    public abstract class GameBase : ModuleBase
    {
        /// <summary>
        /// 游戏根对象
        /// </summary>
        public static GameBase Instance { get; private set; }

        protected sealed override void OnInternalInit()
        {
            Instance = this;
        }

        protected sealed override void OnInternalDestroy()
        {
            Instance = null;
        }
        
        /// <summary>
        /// 这里初始化依赖配置的模块
        /// </summary>
        public virtual IEnumerator InitAfterConfig()
        {
            yield break;
        }
        
        /// <summary>
        /// 这里初始化依赖存档的模块
        /// </summary>
        public virtual IEnumerator InitAfterLogin()
        {
            yield break;
        }

        #region 依赖存档的模块管理

        /// <summary>
        /// 是否已加载存档
        /// </summary>
        public bool IsAfterLogin;
        
        /// <summary>
        /// 所有依赖存档的模块列表
        /// </summary>
        public List<ModuleBase> AfterLoginModules = new();
        
        protected sealed override void OnAddModule(ModuleBase module)
        {
            if (IsAfterLogin && !AfterLoginModules.Contains(module))
            {
                AfterLoginModules.Add(module);
            }
        }
        
        protected sealed override void OnRemoveModule(ModuleBase module)
        {
            if (IsAfterLogin)
            {
                AfterLoginModules.Remove(module);
            }
        }

        /// <summary>
        /// 卸载所有依赖存档的模块
        /// </summary>
        public void RemoveAfterLoginModules()
        {
            for (int i = AfterLoginModules.Count - 1; i >= 0; i--)
            {
                RemoveModule(AfterLoginModules[i]);
            }
        }

        #endregion

        /// <summary>
        /// 热重启游戏
        /// </summary>
        public void Restart()
        {
            GetModule<EventManager>().Broadcast(new OnGameRestartEvent(), () =>
            {
                GetModule<TimingManager>().GlobalDelayCallFrame(1, () =>
                {
                    Dispose();
                    ReflectionUtil.InvokeStaticMethod("AOTAsm", "AOTLaunch", "Create");
                });
            });
        }
    }
}