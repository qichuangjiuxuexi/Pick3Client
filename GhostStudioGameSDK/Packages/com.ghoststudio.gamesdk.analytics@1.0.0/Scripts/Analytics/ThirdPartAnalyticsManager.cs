using System.Collections.Generic;
using AppBase.Module;

namespace AppBase.Analytics
{
    /// <summary>
    /// 带缓存的打点管理器基类
    /// </summary>
    public abstract class ThirdPartAnalyticsManager : ModuleBase
    {
        /// <summary>
        /// 是否已初始化
        /// </summary>
        public bool isInit;
        
        /// <summary>
        /// 最大缓存数量
        /// </summary>
        protected int maxCacheCount = 100;

        /// <summary>
        /// 缓存队列
        /// </summary>
        protected Queue<AnalyticsEvent> cacheQueue = new();

        /// <summary>
        /// 初始化SDK
        /// </summary>
        public virtual void InitSDK()
        {
        }

        /// <summary>
        /// 直接打点
        /// </summary>
        /// <param name="evt">打点数据</param>
        public virtual void TrackEvent(AnalyticsEvent evt)
        {
        }

        /// <summary>
        /// 带缓存的打点
        /// </summary>
        /// <param name="evt">打点数据</param>
        public void TrackEventWithCache(AnalyticsEvent evt)
        {
            if (isInit)
            {
                TrackEvent(evt);
            }
            else
            {
                AddEventToCache(evt);
            }
        }
        
        /// <summary>
        /// 添加打点数据到缓存
        /// </summary>
        protected void AddEventToCache(AnalyticsEvent evt)
        {
            if (cacheQueue.Count >= maxCacheCount)
            {
                cacheQueue.Dequeue();
            }
            cacheQueue.Enqueue(evt);
        }
        
        /// <summary>
        /// 打点所有缓存的数据
        /// </summary>
        protected void TrackCachedEvents()
        {
            while (cacheQueue.Count > 0)
            {
                TrackEvent(cacheQueue.Dequeue());
            }
        }
    }
}