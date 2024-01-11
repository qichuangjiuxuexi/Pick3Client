using System;
using System.Collections.Generic;
using AppBase.Analytics;
using AppBase.Event;
using AppBase.Utils;

namespace AppBase.UserAssets
{
    /// <summary>
    /// 用户资产管理器：资产管理
    /// </summary>
    public partial class UserAssetManager
    {
        /// <summary>
        /// 用户资产列表
        /// </summary>
        public Dictionary<int, UserAssetItem> AssetItems => UserAssetRecord.ArchiveData.assetItems;
        
        /// <summary>
        /// 获取资产数量
        /// </summary>
        /// <param name="assetId">资产类型</param>
        /// <param name="includeTemp">是否包含临时资产数据</param>
        /// <returns>资产数量</returns>
        public long GetAssetNum(int assetId, bool includeTemp = false)
        {
            var assetNum = Math.Max(0, GetAssetItem(assetId)?.assetNum ?? 0);
            if (includeTemp)
            {
                assetNum = assetNum.SafeIncrement(TempAssets.GetAssetNum(assetId));
            }
            return assetNum;
        }

        /// <summary>
        /// 设置正式资产数值
        /// </summary>
        /// <param name="assetId">资产类型</param>
        /// <param name="assetNum">资产数量</param>
        /// <param name="tag">来源标签，打点使用</param>
        public long SetAssetNum(int assetId, long assetNum, string tag)
        {
            var item = GetOrCreateAssetItem(assetId);
            var oldAssetNum = GetAssetNum(assetId);
            var oldAllAssetNum = GetAssetNum(assetId,true);
            item.assetNum = Math.Max(0, assetNum);
            SetDirty();
            GameBase.Instance.GetModule<EventManager>()
                .Broadcast(new EventUserAssetChange(assetId, oldAssetNum, item.assetNum, tag));
            var newAssetNum = GetAssetNum(assetId,true);
            if (newAssetNum != oldAllAssetNum)
                GameBase.Instance.GetModule<EventManager>()
                    .Broadcast(new EventAllUserAssetChange(assetId, oldAllAssetNum, newAssetNum, tag));
            return item.assetNum;
        }
        
        /// <summary>
        /// 增加正式资产数量
        /// </summary>
        /// <param name="assetId">资产类型</param>
        /// <param name="assetNum">资产数量</param>
        /// <param name="tag">来源标签，打点使用</param>
        /// <returns>新的资产数量</returns>
        public long AddAssetNum(int assetId, long assetNum, string tag)
        {
            var oldAssetNum = GetAssetNum(assetId);
            var newAssetNum = oldAssetNum.SafeIncrement(assetNum);
            SetAssetNum(assetId, newAssetNum, tag);
            return newAssetNum;
        }

        /// <summary>
        /// 扣减资产数量
        /// </summary>
        /// <param name="assetId">资产类型</param>
        /// <param name="assetNum">资产数量</param>
        /// <param name="tag">来源标签，打点使用</param>
        /// <param name="includeTemp">是否可以扣减临时资产，如果是则先扣减正式资产，不足再扣减临时资产</param>
        /// <returns>新的资产数量</returns>
        public long SubAssetNum(int assetId, long assetNum, string tag, bool includeTemp = false)
        {
            //不扣减临时资产
            if (!includeTemp)
            {
                return AddAssetNum(assetId, -assetNum, tag);
            }
            
            //正式资产足够扣减
            var assetItem = GetOrCreateAssetItem(assetId);
            if (assetItem.assetNum >= assetNum)
            {
                return AddAssetNum(assetId, -assetNum, tag);
            }
            
            //扣减正式和临时资产
            var oldAssetNum = GetAssetNum(assetId, true);

            assetNum -= assetItem.assetNum;
            assetItem.assetNum = 0;
            var tempItem = TempAssets.GetAssetItem(assetId);
            long newAssetNum = 0;
            if (tempItem != null)
            {
                tempItem.assetNum = tempItem.assetNum.SafeIncrement(-assetNum);
                newAssetNum = tempItem.assetNum;
            }

            GameBase.Instance.GetModule<EventManager>()
                .Broadcast(new EventUserAssetChange(assetId, oldAssetNum, newAssetNum, tag));
            long allAllNum = GetAssetNum(assetId, true);
            if (oldAssetNum != allAllNum)
            {
                GameBase.Instance.GetModule<EventManager>()
                    .Broadcast(new EventAllUserAssetChange(assetId, oldAssetNum, allAllNum, tag));
            }
            return newAssetNum;
        }

        /// <summary>
        /// 是否存在资产数据，可用来初始化资产
        /// </summary>
        /// <param name="assetId">资产类型</param>
        /// <returns>是否存在资产数据</returns>
        public bool HasAsset(int assetId)
        {
            return GetAssetItem(assetId) != null;
        }

        /// <summary>
        /// 获取数值资产数据
        /// </summary>
        /// <param name="assetId">资产类型</param>
        /// <returns>数值资产数据</returns>
        public UserAssetItem GetAssetItem(int assetId)
        {
            return AssetItems.TryGetValue(assetId, out var item) ? item : null;
        }

        /// <summary>
        /// 获取或创建资产
        /// </summary>
        /// <param name="assetId">资产类型</param>
        /// <param name="defaultValue">创建时的初始值</param>
        /// <param name="defaultData">创建时的其他初始数据</param>
        /// <returns>资产数据</returns>
        public UserAssetItem GetOrCreateAssetItem(int assetId, long defaultValue = 0, object defaultData = null)
        {
            var item = GetAssetItem(assetId);
            if (item == null)
            {
                item = new UserAssetItem(assetId, defaultValue, defaultData);
                AssetItems[assetId] = item;
                SetDirty();
            }
            return item;
        }
        
        /// <summary>
        /// 获取资产其他数据
        /// </summary>
        /// <param name="assetId">资产类型</param>
        /// <returns>资产其他数据</returns>
        public object GetAssetData(int assetId)
        {
            return GetAssetItem(assetId)?.data;
        }
        
        /// <summary>
        /// 获取资产其他数据
        /// </summary>
        /// <param name="assetId">资产类型</param>
        /// <param name="defaultValue">找不到资产时的默认值</param>
        /// <returns>资产其他数据</returns>
        public T GetAssetData<T>(int assetId, T defaultValue = default)
        {
            var item = GetAssetItem(assetId);
            if (item != null)
            {
                try
                {
                    return (T)Convert.ChangeType(item.data, typeof(T));
                }
                catch
                {
                }
            }
            return defaultValue;
        }

        /// <summary>
        /// 设置资产其他数据
        /// </summary>
        /// <param name="assetId">资产类型</param>
        /// <param name="assetData">资产其他数据</param>
        /// <param name="tag">来源标签，打点使用</param>
        public void SetAssetData(int assetId, object assetData, string tag)
        {
            var item = GetOrCreateAssetItem(assetId);
            item.data = assetData;
            SetDirty();
        }
        
        /// <summary>
        /// 设置资产数值和其他数据
        /// </summary>
        /// <param name="assetId">资产类型</param>
        /// <param name="assetData">资产其他数据</param>
        /// <param name="tag">来源标签，打点使用</param>
        public void SetAssetData(int assetId, long assetNum, object assetData, string tag)
        {
            var item = GetOrCreateAssetItem(assetId);
            var oldAssetNum = GetAssetNum(assetId);
            var oldAllNum =  GetAssetNum(assetId,true);
            item.assetNum = Math.Max(0, assetNum);
            item.data = assetData;
            SetDirty();
            GameBase.Instance.GetModule<EventManager>()
                .Broadcast(new EventUserAssetChange(assetId, oldAssetNum, item.assetNum, tag));
            long newAllNum =  GetAssetNum(assetId,true);
            if (newAllNum != oldAllNum)
                GameBase.Instance.GetModule<EventManager>()
                    .Broadcast(new EventAllUserAssetChange(assetId, oldAllNum, newAllNum, tag));
        }
        
        /// <summary>
        /// 清除资产
        /// </summary>
        /// <param name="assetId">资产类型</param>
        /// <param name="tag">来源标签，打点使用</param>
        /// <returns>清除前的资产数值</returns>
        public long ClearAsset(int assetId, string tag)
        {
            var item = GetAssetItem(assetId);
            if (item == null) return 0;
            var oldNum = GetAssetNum(assetId);
            long oldAllAssetNum = GetAssetNum(assetId, true);
            item.assetNum = 0;
            item.data = null;
            AssetItems.Remove(assetId);
            SetDirty();
            GameBase.Instance.GetModule<EventManager>().Broadcast(new EventUserAssetChange(assetId, oldNum, 0, tag));
            long newNum = GetAssetNum(assetId, true);
            if (newNum != oldAllAssetNum)
                GameBase.Instance.GetModule<EventManager>().Broadcast(new EventAllUserAssetChange(assetId, oldNum, newNum, tag));
            return oldNum;
        }
    }
}