using System;
using UnityEngine;
using UnityEngine.ResourceManagement.AsyncOperations;

namespace AppBase.Utils
{
    /// <summary>
    /// 带超时的等待下载的协程
    /// 超时判定机制：在指定时间内没有收到任何比特的数据更新，则判定为超时
    /// </summary>
    public class WaitForDownload : WaitForHandler
    {
        /// <summary>
        /// 开始下载的字节数
        /// </summary>
        private long startDownloadedBytes;
        
        /// <summary>
        /// 上一次下载的字节数
        /// </summary>
        private long lastDownloadedBytes;
        
        /// <summary>
        /// 超时时间
        /// </summary>
        private float timeout;
        
        /// <summary>
        /// 更新进度回调
        /// </summary>
        private Action<float> progress;
        
        /// <summary>
        /// 是否需要等待
        /// </summary>
        public override bool keepWaiting
        {
            get
            {
                if (handler.IsDone || isTimeout)
                {
                    progress?.Invoke(1);
                    return false;
                }
                var status = handler.GetDownloadStatus();
                if (status.DownloadedBytes > lastDownloadedBytes)
                {
                    lastDownloadedBytes = status.DownloadedBytes;
                    endTime = Time.realtimeSinceStartup + timeout;
                    progress?.Invoke((float)((double)(lastDownloadedBytes - startDownloadedBytes) / (status.TotalBytes - startDownloadedBytes)));
                }
                return true;
            }
        }
        
        /// <summary>
        /// 带超时的等待下载的协程
        /// 超时判定机制：在指定时间内没有收到任何比特的数据更新，则判定为超时
        /// </summary>
        /// <param name="handler">下载句柄</param>
        /// <param name="timeout">超时时间</param>
        /// <param name="progress">进度更新的回调</param>
        public WaitForDownload(AsyncOperationHandle handler, float timeout = 0, Action<float> progress = null) : base(handler, timeout)
        {
            startDownloadedBytes = lastDownloadedBytes = handler.GetDownloadStatus().DownloadedBytes;
            this.progress = progress;
            this.timeout = timeout;
        }
    }
}