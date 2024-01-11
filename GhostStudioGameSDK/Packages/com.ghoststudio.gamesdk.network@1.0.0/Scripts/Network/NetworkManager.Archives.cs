using System.Collections;
using AppBase.Timing;
using UnityEngine;

namespace AppBase.Network
{
    /// <summary>
    /// 网络管理器，自动上传存档逻辑
    /// </summary>
    public partial class NetworkManager : IUpdateSecond
    {
        /// <summary>
        /// 上传存档间隔时间
        /// </summary>
        private float uploadCdTime = 600;
        
        /// <summary>
        /// 上次上传存档时间
        /// </summary>
        private float lastUploadTime;

        /// <summary>
        /// 是否正在上传，避免重复上传
        /// </summary>
        private bool isUploading;
        
        /// <summary>
        /// 开始自动上传存档
        /// </summary>
        public void StartAutoUpload()
        {
            lastUploadTime = Time.realtimeSinceStartup;
            GameBase.Instance.GetModule<TimingManager>().SubscribeSecondUpdate(this);
        }
        
        /// <summary>
        /// 停止自动上传存档
        /// </summary>
        public void StopAutoUpload()
        {
            GameBase.Instance.GetModule<TimingManager>().UnsubscribeSecondUpdate(this);
        }

        /// <summary>
        /// 上传存档
        /// </summary>
        public void UploadArchives()
        {
            lastUploadTime = Time.realtimeSinceStartup;
            if (isUploading) return;
            isUploading = true;
            Send(new UploadProtocol(), b => isUploading = false);
        }

        /// <summary>
        /// 每秒检查是否到了自动上传存档时间
        /// </summary>
        public void OnUpdateSecond()
        {
            if (Time.realtimeSinceStartup > lastUploadTime + uploadCdTime)
            {
                UploadArchives();
            }
        }
        
        /// <summary>
        /// 游戏启动完成，开始自动上传存档
        /// </summary>
        public void OnLoadFinished(EventOnLoadFinished e)
        {
            UploadArchives();
            StartAutoUpload();
        }

        /// <summary>
        /// 切回游戏时，自动同步服务器时间
        /// </summary>
        public void OnGameFocus(EventOnGameFocus e)
        {
            Send(new HeartbeatProtocol());
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();
            StopAutoUpload();
        }
    }
}