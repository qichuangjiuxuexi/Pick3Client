//using UnityEngine;

//namespace BetaFramework.obb
//{
//    public class ObbDownload
//    {
//        private static AndroidJavaClass ObbDownloadClass = new AndroidJavaClass("com.beta.one.obbdownloadlib.UnityObbDownload");
//        private static AndroidObbDownloadListener m_DownloadListener;

//        /// <summary>
//        /// obb是否已经在本地
//        /// </summary>
//        /// <param name="mainVer">主扩展文件版本号</param>
//        /// <param name="patchVer">补丁扩展文件版本号，如果没有补丁文件使用默认值</param>
//        /// <returns></returns>
//        public static bool IsObbReady(int mainVer, int patchVer = -1)
//        {
//            var obbExist = ObbDownloadClass.CallStatic<bool>("isFileExist", true, mainVer, -1L);
//            if (obbExist && patchVer > 0)
//            {
//                obbExist = ObbDownloadClass.CallStatic<bool>("isFileExist", false, patchVer, -1L);
//            }

//            return obbExist;
//        }

//        /// <summary>
//        /// 开启下载obb流程
//        /// </summary>
//        /// <param name="obbDownloadListener">下载进度回调</param>
//        /// <param name="publicKey"></param>
//        /// <returns></returns>
//        public static bool StartObbDownload(ObbDownloadListener obbDownloadListener, string publicKey)
//        {
//            m_DownloadListener = new AndroidObbDownloadListener(obbDownloadListener);

//            byte[] salt = new byte[] {1, 43, 256 - 12, 256 - 1, 54, 98, 256 - 100, 256 - 12, 43, 2, 256 - 8, 256 - 4, 9, 5, 256 - 106, 256 - 108, 256 - 33, 45, 256 - 1, 84};
//            return ObbDownloadClass.CallStatic<bool>("startDownload",
//                m_DownloadListener, 
//                publicKey,  salt);
//        }

//        /// <summary>
//        /// 暂停下载
//        /// </summary>
//        public static void PauseDownload()
//        {
//            ObbDownloadClass.CallStatic("setDownloadState", true);
//        }
        
//        /// <summary>
//        /// 恢复下载
//        /// </summary>
//        public static void ResumeDownload()
//        {
//            ObbDownloadClass.CallStatic("setDownloadState", false);
//        }

//        /// <summary>
//        /// 启用使用移动网络下载（默认只能在WIFI环境下载）
//        /// </summary>
//        public static void EnableDownloadOverCellular()
//        {
//            ObbDownloadClass.CallStatic("enableDownloadOverCellular");
//        }

//        public static void RestartApplication(int delay)
//        {
//            ObbDownloadClass.CallStatic("restartApplication", delay);
//        }
//    }

//    public class AndroidObbDownloadListener : AndroidJavaProxy
//    {
//        private ObbDownloadListener _listener;
//        public AndroidObbDownloadListener(ObbDownloadListener listener) : base("com.beta.one.obbdownloadlib.ObbDownloadListener")
//        {
//            _listener = listener;
//        }

//        public void onDownloadStateChanged(int newState)
//        {
//            _listener?.onDownloadStateChanged(newState);
//        }

//        public void onDownloadProgress(long overallTotal, long overallProgress, long timeRemaining, float currentSpeed)
//        {
//            _listener?.onDownloadProgress(overallTotal, overallProgress, timeRemaining, currentSpeed);
//        }
//    }

//    public interface ObbDownloadListener
//    {
//        void onDownloadStateChanged(int newState);
//        void onDownloadProgress(long overallTotal, long overallProgress, long timeRemaining, float currentSpeed);
//    }

//    /// <summary>
//    /// ObbDownloadListener.onDownloadStateChanged参数newState对应值意义
//    /// </summary>
//    public enum ObbDownloadState
//    {
//         STATE_IDLE = 1,
//         STATE_FETCHING_URL = 2,
//         STATE_CONNECTING = 3,
//         STATE_DOWNLOADING = 4,
//         STATE_COMPLETED = 5,

//         STATE_PAUSED_NETWORK_UNAVAILABLE = 6,
//         STATE_PAUSED_BY_REQUEST = 7,

//        /**
//     * Both STATE_PAUSED_WIFI_DISABLED_NEED_CELLULAR_PERMISSION and
//     * STATE_PAUSED_NEED_CELLULAR_PERMISSION imply that Wi-Fi is unavailable and
//     * cellular permission will restart the service. Wi-Fi disabled means that
//     * the Wi-Fi manager is returning that Wi-Fi is not enabled, while in the
//     * other case Wi-Fi is enabled but not available.
//     */
//         STATE_PAUSED_WIFI_DISABLED_NEED_CELLULAR_PERMISSION = 8,

//         STATE_PAUSED_NEED_CELLULAR_PERMISSION = 9,

//        /**
//     * Both STATE_PAUSED_WIFI_DISABLED and STATE_PAUSED_NEED_WIFI imply that
//     * Wi-Fi is unavailable and cellular permission will NOT restart the
//     * service. Wi-Fi disabled means that the Wi-Fi manager is returning that
//     * Wi-Fi is not enabled, while in the other case Wi-Fi is enabled but not
//     * available.
//     * <p>
//     * The service does not return these values. We recommend that app
//     * developers with very large payloads do not allow these payloads to be
//     * downloaded over cellular connections.
//     */
//        STATE_PAUSED_WIFI_DISABLED = 10,
//        STATE_PAUSED_NEED_WIFI = 11,
//        STATE_PAUSED_ROAMING = 12,

//        /**
//     * Scary case. We were on a network that redirected us to another website
//     * that delivered us the wrong file.
//     */
//        STATE_PAUSED_NETWORK_SETUP_FAILURE = 13,
//        STATE_PAUSED_SDCARD_UNAVAILABLE = 14,
//        STATE_FAILED_UNLICENSED = 15,
//        STATE_FAILED_FETCHING_URL = 16,
//        STATE_FAILED_SDCARD_FULL = 17,
//        STATE_FAILED_CANCELED = 18,
//        STATE_FAILED = 19,
//    }
//}