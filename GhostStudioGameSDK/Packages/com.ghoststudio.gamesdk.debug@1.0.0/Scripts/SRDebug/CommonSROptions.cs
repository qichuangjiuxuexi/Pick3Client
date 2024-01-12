using AppBase.GetOrWait;
using AppBase.Utils;
using UnityEngine;

namespace AppBase.Debugging
{
    /// <summary>
    /// 通用SROptions设置，与业务无关
    /// </summary>
    public class CommonSROptions : SROptionsModule
    {
        protected override string CatalogName => "通用";
        protected override SROptionsPriority CatalogPriority => SROptionsPriority.Common;
        
        protected override void OnInit()
        {
            base.OnInit();
            OnSRDebugScale();
            OnShowFPS();
        }

        [DisplayName("Debug面板缩放"), Increment(0.1), NumberRange(0.5f, 3f)]
        public float SRDebugScale
        {
            get
            {
                return PlayerPrefs.GetFloat("SRDebugScale", 1f);
            }
            set
            {
                PlayerPrefs.SetFloat("SRDebugScale", value);
                OnSRDebugScale();
            }
        }

        private void OnSRDebugScale()
        {
            GameBase.Instance.GetModule<DebugManager>().SRDebug.UIScale = PlayerPrefs.GetFloat("SRDebugScale", 1f);
        }
        
        [DisplayName("TimeScale"), Increment(0.1), NumberRange(0f, 10f)]
        public float TimeScale
        {
            get => Time.timeScale;
            set => Time.timeScale = value;
        }

        private const string ShowFpsKey = "ShowFPS";
        [DisplayName("ShowFPS")]
        public bool ShowFPS
        {
            get => PlayerPrefs.GetInt(ShowFpsKey, 0) != 0;
            set
            {
                PlayerPrefs.SetInt(ShowFpsKey, value ? 1 : 0);
                PlayerPrefs.Save();
                OnShowFPS();
            }
        }

        private void OnShowFPS()
        {
            var isShow = PlayerPrefs.GetInt(ShowFpsKey, 0) != 0;
            var camera = GameBase.Instance.GetModule<CameraManager>().UICamera;
            if (camera == null) return;
            if (isShow)
            {
                if (!camera.gameObject.GetComponent<FPSDisplay>())
                    camera.gameObject.AddComponent<FPSDisplay>();
            }
            else Object.Destroy(camera.GetComponent<FPSDisplay>());
        }

        [DisplayName("帧率设置")]
        public int GMFrame
        {
            get => Application.targetFrameRate;
            set => Application.targetFrameRate = value;
        }

        [DisplayName("热重启")]
        public void RestartGame()
        {
            GameBase.Instance.Restart();
        }
        
        [DisplayName("设置Firebase Token")]
        public string SetFirebaseToken
        {
            get => GameBase.Instance.GetModule<GetOrWaitManager>()?.GetOrWait<string>("firebase_token") ?? "";
            set => GameBase.Instance.GetModule<GetOrWaitManager>()?.SetData("firebase_token", value);
        }
    }
}
