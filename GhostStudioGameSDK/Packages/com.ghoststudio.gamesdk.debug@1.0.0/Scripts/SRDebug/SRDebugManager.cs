using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Text;
using AppBase.GetOrWait;
using AppBase.Module;
using AppBase.Utils;
using SRDebugger.Services;
using SRF.Service;

namespace AppBase.Debugging
{
    /// <summary>
    /// SRDebugger管理器
    /// </summary>
    public class SRDebugManager : ModuleBase
    {
        private float _uiScale = 1f;
        private bool _isTriggerEnabled = true;
        private List<SROptionsModule> _options = new();
        
        /// <summary>
        /// 是否启用了SRDebugger
        /// </summary>
        public bool IsEnabled = SRDebug.IsInitialized;

        /// <summary>
        /// 隐藏SROption面板
        /// </summary>
        public void HideDebugPanel()
        {
            if (SRDebug.IsInitialized)
            {
                SRDebug.Instance.HideDebugPanel();
            }
        }

        /// <summary>
        /// 显示SROption面板
        /// </summary>
        public void ShowDebugPanel()
        {
            if (SRDebug.IsInitialized)
            {
                SRDebug.Instance.ShowDebugPanel();
            }
        }

        /// <summary>
        /// 设置是否启用SROption小标签
        /// </summary>
        public bool IsTriggerEnabled
        {
            get => SRDebug.IsInitialized ? SRDebug.Instance.IsTriggerEnabled : _isTriggerEnabled;
            set
            {
                _isTriggerEnabled = value;
                if (SRDebug.IsInitialized)
                {
                    SRDebug.Instance.IsTriggerEnabled = _isTriggerEnabled;
                }
            }
        }

        /// <summary>
        /// 设置SROption面板缩放参数
        /// </summary>
        public float UIScale
        {
            get => SRDebug.IsInitialized ? SRDebug.Instance.Settings.UIScale : _uiScale;
            set
            {
                _uiScale = value;
                if (SRDebug.IsInitialized)
                {
                    SRDebug.Instance.Settings.UIScale = _uiScale;
                }
            }
        }

        public const string user_id = "user_id";
        private  int[]  DcynamicEntryCode()
        {
            object obj = GameBase.Instance.GetModule<GetOrWaitManager>().GetOrWaitCallBack(user_id,null);
            if (obj == null)
                return null;
            string input = obj.ToString();
            if (string.IsNullOrEmpty(input))
                return null;
            input = $"Player{input}";
            byte[] inputBytes = System.Text.Encoding.ASCII.GetBytes(input);

            MD5 md5 = MD5.Create();
            byte[] hashBytes = md5.ComputeHash(inputBytes);
            StringBuilder stringBuilder = new StringBuilder();
            int[] entryCode = new int[4];
            for (int i = 0; i < 16; i++)
            {
                stringBuilder.Append(hashBytes[i]);
                if (i%4 ==3)
                {
                    stringBuilder.Replace('-', '"');
                    entryCode[i / 4] = (int)(Convert.ToInt64(stringBuilder.ToString()) % 10);
                    stringBuilder = new StringBuilder();
                }
            }
           
            return entryCode;
        }

        public void OpenSRDebug()
        {
            int[] entryCode = null;
            if (AppUtil.IsRelease)
            {
                entryCode = DcynamicEntryCode();
                if (entryCode == null)
                    return;   
                SRDebug.Instance.Settings.RequireCode = true;
                SRDebug.Instance.Settings.EntryCode = entryCode;
            }
            if (SRDebugger.Settings.Instance.RequireCode)
            {
                SRServiceManager.GetService<IPinEntryService>().ShowPinEntry(entryCode,
                    "please enter password below!", (isValid) =>
                    {
                        if (isValid)
                            InitSRDebug();
                    }, true);
            }
            else
            {
                InitSRDebug();
            }
        }

        protected override void OnInit()
        {
            base.OnInit();
            if (AppUtil.IsDebug)
                OpenSRDebug();
        }

        protected override void OnAfterInit()
        {
            base.OnAfterInit();
            AddModule<CommonSROptions>();
        }

        /// <summary>
        /// 初始化SRDebugger
        /// </summary>
        public void InitSRDebug()
        {
            if (SRDebug.IsInitialized) return;
            SRDebug.Init();
            SRDebug.Instance.Settings.UIScale = _uiScale;
            SRDebug.Instance.IsTriggerEnabled = _isTriggerEnabled;
            foreach (var option in _options)
            {
                SRDebug.Instance.AddOptionContainer(option);
            }
        }
        
        /// <summary>
        /// 注册SROptionModule
        /// </summary>
        public void RegisterSROptions(SROptionsModule module)
        {
            _options.Add(module);
            if (SRDebug.IsInitialized)
            {
                SRDebug.Instance.AddOptionContainer(module);
            }
        }
        
        /// <summary>
        /// 解注册SROptionModule
        /// </summary>
        public void UnregisterSROptions(SROptionsModule module)
        {
            _options.Remove(module);
            if (SRDebug.IsInitialized)
            {
                SRDebug.Instance.RemoveOptionContainer(module);
            }
        }
    }
}
