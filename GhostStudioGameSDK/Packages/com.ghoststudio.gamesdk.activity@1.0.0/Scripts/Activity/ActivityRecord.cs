using System;
using AppBase.Archive;

namespace AppBase.Activity
{
    public class ActivityRecord : BaseRecord<ActivityArchiveData>
    {
        public override string ArchiveName => $"ActivityRecord_{moduleData}";

        /// <summary>
        /// 设置活动数据
        /// </summary>
        /// <param name="key"></param>
        /// <param name="value"></param>
        public void SetActivityData(string key, object value)
        {
            if (string.IsNullOrEmpty(key)) return;
            if (!ArchiveData.activityData.TryGetValue(key, out var oldValue) || oldValue == null || !oldValue.Equals(value))
            {
                ArchiveData.activityData[key] = value;
                SetDirty();
            }
        }
        
        /// <summary>
        /// 取活动数据
        /// </summary>
        /// <param name="key">键</param>
        /// <param name="defaultValue">默认值</param>
        /// <typeparam name="T">类型</typeparam>
        /// <returns>值</returns>
        public T GetActivityData<T>(string key, T defaultValue = default)
        {
            if (ArchiveData.activityData.TryGetValue(key, out var value))
            {
                try
                {
                    return (T)Convert.ChangeType(value, typeof(T));
                }
                catch
                {
                }
            }
            return defaultValue;
        }
        
        /// <summary>
        /// 取或创建轮次数据
        /// </summary>
        /// <param name="key">键</param>
        /// <param name="defaultValue">默认值</param>
        /// <typeparam name="T">类型</typeparam>
        /// <returns>值</returns>
        public T GetOrCreateActivityData<T>(string key, T defaultValue = default)
        {
            if (ArchiveData.activityData.TryGetValue(key, out var value))
            {
                try
                {
                    return (T)Convert.ChangeType(value, typeof(T));
                }
                catch
                {
                }
            }
            if (defaultValue == null)
            {
                return Activator.CreateInstance<T>();
            }
            return defaultValue;
        }
        
        /// <summary>
        /// 是否包含活动数据
        /// </summary>
        /// <param name="key">键</param>
        /// <returns>是否包含</returns>
        public bool ContainsActivityData(string key)
        {
            return ArchiveData.activityData.ContainsKey(key);
        }
        
        
        /// <summary>
        /// 设置轮次数据
        /// </summary>
        /// <param name="key"></param>
        /// <param name="value"></param>
        public void SetRoundData(string key, object value)
        {
            if (string.IsNullOrEmpty(key)) return;
            if (!ArchiveData.roundData.TryGetValue(key, out var oldValue) || oldValue == null || !oldValue.Equals(value))
            {
                ArchiveData.roundData[key] = value;
                SetDirty();
            }
        }
        
        /// <summary>
        /// 取轮次数据
        /// </summary>
        /// <param name="key">键</param>
        /// <param name="defaultValue">默认值</param>
        /// <typeparam name="T">类型</typeparam>
        /// <returns>值</returns>
        public T GetRoundData<T>(string key, T defaultValue = default)
        {
            if (ArchiveData.roundData.TryGetValue(key, out var value))
            {
                try
                {
                    return (T)Convert.ChangeType(value, typeof(T));
                }
                catch
                {
                }
            }
            return defaultValue;
        }
        
        /// <summary>
        /// 取或创建轮次数据
        /// </summary>
        /// <param name="key">键</param>
        /// <param name="defaultValue">默认值</param>
        /// <typeparam name="T">类型</typeparam>
        /// <returns>值</returns>
        public T GetOrCreateRoundData<T>(string key, T defaultValue = default)
        {
            if (ArchiveData.roundData.TryGetValue(key, out var value))
            {
                try
                {
                    return (T)Convert.ChangeType(value, typeof(T));
                }
                catch
                {
                }
            }
            if (defaultValue == null)
            {
                return Activator.CreateInstance<T>();
            }
            return defaultValue;
        }
        
        /// <summary>
        /// 是否包含轮次数据
        /// </summary>
        /// <param name="key">键</param>
        /// <returns>是否包含</returns>
        public bool ContainsRoundData(string key)
        {
            return ArchiveData.roundData.ContainsKey(key);
        }
    }
}