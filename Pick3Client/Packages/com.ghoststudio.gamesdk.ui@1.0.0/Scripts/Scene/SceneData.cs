using System;
using System.Collections;
using AppBase.Resource;

namespace AppBase.UI.Scene
{
    /// <summary>
    /// 场景数据
    /// </summary>
    public class SceneData : IEnumerator
    {
        /// <summary>
        /// 地址
        /// </summary>
        public string address;

        /// <summary>
        /// 用户数据
        /// </summary>
        public object data;

        /// <summary>
        /// 场景类型
        /// </summary>
        public SceneType sceneType;
        
        /// <summary>
        /// 当场景资源加载完成后回调
        /// </summary>
        public event Action<SceneBase> loadedCallback;
        public void OnLoadedCallback(SceneBase obj)
        {
            loadedCallback?.Invoke(obj);
            loadedCallback = null;
        }
        
        /// <summary>
        /// 当场景切换完成后回调
        /// </summary>
        public event Action<SceneBase> switchCallback;
        public void OnSwitchCallback(SceneBase obj)
        {
            switchCallback?.Invoke(obj);
            switchCallback = null;
            //场景切换完成，释放协程
            handler = null;
        }

        public SceneData()
        {
        }

        /// <summary>
        /// 场景数据
        /// </summary>
        /// <param name="address">对话框地址</param>
        /// <param name="data">用户数据</param>
        /// <param name="sceneType">场景类型</param>
        /// <param name="switchCallback">当场景切换完成后回调</param>
        public SceneData(string address, object data = null, SceneType sceneType = SceneType.NormalScene, Action<SceneBase> switchCallback = null)
        {
            this.address = address;
            this.data = data;
            this.sceneType = sceneType;
            if (switchCallback != null)
            {
                this.switchCallback += switchCallback;
            }
        }

        #region 协程相关

        /// <summary>
        /// 资源加载器，切换场景完成后会置空
        /// </summary>
        public ResourceHandler handler { get; internal set; }

        /// <summary>
        /// 协程等待直到场景切换完成
        /// </summary>
        public virtual bool MoveNext() => handler != null;

        public object Current => handler?.Current;
        public void Reset() {}
        
        #endregion
    }
}