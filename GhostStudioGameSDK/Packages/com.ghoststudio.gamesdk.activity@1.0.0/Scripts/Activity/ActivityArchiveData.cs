using System;
using System.Collections.Generic;
using AppBase.Archive;

namespace AppBase.Activity
{
    [Serializable]
    public class ActivityArchiveData : BaseArchiveData
    {
        /// <summary>
        /// 活动ID
        /// </summary>
        public int activityId;

        /// <summary>
        /// 活动版本号
        /// </summary>
        public int activityVersion;

        /// <summary>
        /// 活动状态
        /// </summary>
        public int status;

        /// <summary>
        /// 当前轮数
        /// </summary>
        public int round;
        
        /// <summary>
        /// 当前轮开启时间（UTC）
        /// </summary>
        public DateTime roundOpenTime;

        /// <summary>
        /// 当前轮结束时间（UTC）
        /// </summary>
        public DateTime roundEndTime;

        /// <summary>
        /// 当前轮主题
        /// </summary>
        public string roundThemeName;

        /// <summary>
        /// 活动数据（常驻）
        /// </summary>
        public Dictionary<string, object> activityData = new();

        /// <summary>
        /// 活动数据（轮）
        /// </summary>
        public Dictionary<string, object> roundData = new();
    }
}