using System;
using System.Collections.Generic;
using System.Linq;
using AppBase.Config;
using AppBase.Event;
using AppBase.Module;
using AppBase.Timing;
using AppBase.UI.Dialog;
using AppBase.UI.OneByOne;
using AppBase.Utils;

namespace AppBase.Activity
{
    /// <summary>
    /// 活动基类
    /// </summary>
    public partial class ActivityModule : ModuleBase
    {
        /// <summary>
        /// 活动ID
        /// </summary>
        public int Id;
        
        /// <summary>
        /// 活动代码版本号
        /// </summary>
        public virtual int Version => 0;

        /// <summary>
        /// 活动配置
        /// </summary>
        public ActivityConfig Config;

        /// <summary>
        /// 活动数值配置
        /// </summary>
        public Dictionary<string, string> DataConfig;

        /// <summary>
        /// 活动存档
        /// </summary>
        public ActivityRecord Record;
        
        /// <summary>
        /// 活动开启时间（UTC）
        /// </summary>
        protected DateTime openTime;
        
        /// <summary>
        /// 活动结束时间（UTC）
        /// </summary>
        protected DateTime endTime;
        
        /// <summary>
        /// 活动是否启用
        /// </summary>
        public bool IsEnabled;

        /// <summary>
        /// 活动状态机
        /// </summary>
        public ActivityStateMachine StateMachine;

        protected sealed override void OnInternalInit()
        {
            base.OnInternalInit();
            
            //初始化活动配置
            Config ??= moduleData as ActivityConfig;
            if (Config == null)
            {
                Debugger.LogError(TAG, "ActivityConfig not found");
                IsEnabled = false;
                return;
            }
            
            //初始化活动数据
            Id = Config.activity_id;
            var isUtc = Config.timezone_type != (int)ActivityTimeZoneType.LocalTimeZone;
            openTime = TimeUtil.GetDateTimeByString(Config.open_time, !isUtc, defaultTime: DateTime.MinValue);
            endTime = TimeUtil.GetDateTimeByString(Config.end_time, !isUtc, defaultTime: DateTime.MaxValue);
            
            //初始化活动数值配置
            DataConfig = new();
            if (!string.IsNullOrEmpty(Config.data_config_name))
            {
                var dataConfigList = GameBase.Instance.GetModule<ConfigManager>().GetConfigList<ActivityDataConfig>(AAConst.ActivityDataConfig);
                foreach (var dc in dataConfigList)
                {
                    if (dc.data_config_name == Config.data_config_name)
                    {
                        DataConfig[dc.param_name] = dc.param_value;
                    }
                }
            }
            
            Record = AddModule<ActivityRecord>(Config.activity_id);
            //检查活动存档升级
            IsEnabled = Config.is_enabled && Version >= Config.min_version && CheckVersion();
            
            StateMachine = new ActivityStateMachine(this);
            StateMachine.Init((ActivityStateEnum)Record.ArchiveData.status);
            
            //检查活动状态
            if (!IsEnabled)
            {
                StateMachine.Change(ActivityStateEnum.Close);
            }
            
            //监听事件
            AddModule<EventModule>().Subscribe<OnConfigUpdateEvent>(OnConfigUpdate);
        }

        /// <summary>
        /// 检查过滤器是否满足
        /// </summary>
        protected bool CheckFilters()
        {
            if (Config.open_filters == null || Config.open_filters.Count == 0) return true;
            var oneByOneManager = GameBase.Instance.GetModule<OneByOneManager>();
            foreach (var filter in Config.open_filters)
            {
                if (filter == 0) continue;
                var filterInfo = oneByOneManager.GetFilterInfo(filter);
                if (filterInfo == null) return false;
                if (!filterInfo.CheckFilterWithId(null, filter)) return false;
            }
            return true;
        }

        /// <summary>
        /// 检查活动版本号，如果版本号不一致则升级
        /// </summary>
        protected bool CheckVersion()
        {
            //版本号一致
            if (Version == Record.ArchiveData.activityVersion) return true;
            //版本号比存档小，无法升级，活动不能开启
            if (Version < Record.ArchiveData.activityVersion) return false;
            //版本号比存档大，升级
            if (!OnVersionUpdate(Record.ArchiveData.activityVersion, Version)) return false;
            Record.ArchiveData.activityVersion = Version;
            Record.SetDirty();
            return true;
        }

        /// <summary>
        /// 活动是否开启中
        /// </summary>
        public bool IsOpen => StateMachine.CurStateName != ActivityStateEnum.Close;

        /// <summary>
        /// 每秒更新活动
        /// </summary>
        public void UpdateActivity()
        {
            StateMachine?.Update();
        }

        /// <summary>
        /// 当前轮剩余时间（秒）
        /// </summary>
        public long LeftTime
        {
            get
            {
                //计算当前轮结束时间
                var now = DateTime.UtcNow;
                //服务器时间
                if (Config.timezone_type == (int)ActivityTimeZoneType.ServerUtc)
                {
                    var serverTime = GameBase.Instance.GetModule<TimingManager>().ServerTime;
                    if (serverTime != null) now = serverTime.Value;
                }
                return Math.Max(0,
                    Math.Min((long)(Record.ArchiveData.roundEndTime - now).TotalSeconds,
                        (long)(endTime - now).TotalSeconds)
                );
            }
        }

        /// <summary>
        /// 取活动数值配置
        /// </summary>
        /// <param name="key">配置Key</param>
        /// <param name="defaultValue">默认值</param>
        /// <typeparam name="T">数值类型</typeparam>
        /// <returns>配置值</returns>
        public T GetDataConfig<T>(string key, T defaultValue = default)
        {
            if (DataConfig.TryGetValue(key, out var str))
            {
                return ParseUtil.Parse<T>(str, defaultValue);
            }
            else
            {
                Debugger.LogError(TAG, $"GetDataConfig key not found: {key}");
                return defaultValue;
            }
        }

        /// <summary>
        /// 当活动准备好时调用
        /// </summary>
        protected virtual void OnActivityReady()
        {
        }

        /// <summary>
        /// 当活动开启时调用
        /// </summary>
        protected virtual void OnActivityOpen()
        {
        }

        /// <summary>
        /// 当活动完成，等待关闭时调用
        /// </summary>
        protected virtual void OnActivityFinish()
        {
        }

        /// <summary>
        /// 当活动关闭时调用
        /// </summary>
        protected virtual void OnActivityClose()
        {
        }

        /// <summary>
        /// 当活动存档版本号升级时调用
        /// </summary>
        /// <param name="oldVersion">旧活动</param>
        /// <param name="newVersion">新活动</param>
        /// <returns>是否升级成功，如果升级失败则活动开不起来</returns>
        protected virtual bool OnVersionUpdate(int oldVersion, int newVersion)
        {
            return true;
        }

        #region 皮肤相关

        /// <summary>
        /// 取配置中的活动皮肤名称，可重载这里实现自定义的皮肤逻辑
        /// </summary>
        public virtual string ConfigThemeName => !string.IsNullOrEmpty(Config.theme_name) ? Config.theme_name : GetType().Name;

        /// <summary>
        /// 取活动皮肤地址
        /// </summary>
        /// <param name="suffix">地址后缀</param>
        /// <returns>地址</returns>
        public virtual string GetThemeAddress(string suffix)
        {
            var key = Record.ArchiveData.roundThemeName + suffix;
            var address = AAConst.GetAddress(key);
            return address;
        }

        /// <summary>
        /// 入口地址
        /// </summary>
        public virtual string EntranceAddress => GetThemeAddress("Entrance");

        /// <summary>
        /// 主界面地址
        /// </summary>
        public virtual string MainDialogAddress => GetThemeAddress("Dialog");

        /// <summary>
        /// 根据皮肤配置弹出活动对话框
        /// </summary>
        /// <param name="suffix">地址后缀</param>
        /// <param name="data">对话框数据</param>
        /// <param name="loadedCallback">加载完成后，弹出之前的回调</param>
        /// <returns>对话框数据</returns>
        public virtual DialogData PopupDialog(string suffix, object data = null, Action<UIDialog> closeCallback = null, Action<UIDialog> loadedCallback = null)
        {
            var address = GetThemeAddress(suffix);
            if (string.IsNullOrEmpty(address))
            {
                Debugger.LogError(TAG, $"OpenDialog Address not found: {suffix}");
                return null;
            }
            var dialogData = new DialogData(address, data, loadedCallback,closeCallback);
            return GameBase.Instance.GetModule<DialogManager>().PopupDialog(dialogData);
        }

        /// <summary>
        /// 弹出活动主界面
        /// </summary>
        /// <param name="data">对话框数据</param>
        /// <param name="loadedCallback">加载完成后，弹出之前的回调</param>
        /// <returns>对话框数据</returns>
        public virtual DialogData PopupMainDialog(object data = null , Action<UIDialog> closeCallback = null, Action<UIDialog> loadedCallback = null)
        {
            return PopupDialog("Dialog", data,closeCallback, loadedCallback);
        }

        #endregion

        /// <summary>
        /// 当活动配置发生更新
        /// </summary>
        private void OnConfigUpdate(OnConfigUpdateEvent evt)
        {
            //更新活动配置
            bool needUpdate = false;
            if (evt.addresses.Contains(AAConst.ActivityConfig))
            {
                var config = GameBase.Instance.GetModule<ConfigManager>().GetConfigByKey<int, ActivityConfig>(AAConst.ActivityConfig, Id);
                if (config != null)
                {
                    Config = config;
                    var isUtc = Config.timezone_type != (int)ActivityTimeZoneType.LocalTimeZone;
                    openTime = TimeUtil.GetDateTimeByString(Config.open_time, !isUtc, defaultTime: DateTime.MinValue);
                    endTime = TimeUtil.GetDateTimeByString(Config.end_time, !isUtc, defaultTime: DateTime.MaxValue);
                    IsEnabled = Config.is_enabled && Version >= Config.min_version && CheckVersion();
                    needUpdate = true;
                }
            }
            
            //更新活动数值配置
            if (evt.addresses.Contains(AAConst.ActivityDataConfig))
            {
                DataConfig.Clear();
                if (!string.IsNullOrEmpty(Config.data_config_name))
                {
                    var dataConfigList = GameBase.Instance.GetModule<ConfigManager>().GetConfigList<ActivityDataConfig>(AAConst.ActivityDataConfig);
                    foreach (var dc in dataConfigList)
                    {
                        if (dc.data_config_name == Config.data_config_name)
                        {
                            DataConfig[dc.param_name] = dc.param_value;
                        }
                    }
                }
                needUpdate = true;
            }

            //更新活动状态机
            if (needUpdate)
            {
                if (IsEnabled)
                {
                    StateMachine.Update();
                }
                else
                {
                    StateMachine.Change(ActivityStateEnum.Close);
                }
            }
        }
    }
}