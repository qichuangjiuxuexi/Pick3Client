using System;
using System.Collections.Generic;
using AppBase.Utils;

namespace AppBase.UserAssets
{
    /// <summary>
    /// 资产订单
    /// </summary>
    [Serializable]
    public class AssetOrderItem
    {
        /// <summary>
        /// 资产的变化数量
        /// </summary>
        public Dictionary<int, UserAssetItem> assetItems = new();
        
        /// <summary>
        /// 订单来源标签
        /// </summary>
        public string tag;
        
        /// <summary>
        /// 异常重启后，是否自动加到资产中
        /// </summary>
        public int autoConsumeType;

        public AssetOrderItem()
        {
        }

        /// <summary>
        /// 创建资产订单
        /// </summary>
        /// <param name="tag">订单来源，打点用</param>
        /// <param name="autoConsumeType">异常重启后，是否自动加到资产中</param>
        public AssetOrderItem(string tag, AutoConsumeType autoConsumeType = AutoConsumeType.AutoConsume)
        {
            this.tag = tag;
            this.autoConsumeType = (int)autoConsumeType;
        }
        
        /// <summary>
        /// 创建资产订单，并往里添加一个资产
        /// </summary>
        /// <param name="assetId">资产类型</param>
        /// <param name="assetNum">资产数值</param>
        /// <param name="tag">订单来源，打点用</param>
        /// <param name="autoConsumeType">异常重启后，是否自动加到资产中</param>
        public AssetOrderItem(int assetId, long assetNum, string tag, AutoConsumeType autoConsumeType = AutoConsumeType.AutoConsume)
        {
            this.tag = tag;
            this.autoConsumeType = (int)autoConsumeType;
            SetAssetNum(assetId, assetNum);
        }
        
        /// <summary>
        /// 创建资产订单，并往里添加多个资产
        /// </summary>
        /// <param name="assetItems">资产字典</param>
        /// <param name="tag">订单来源，打点用</param>
        /// <param name="autoConsumeType">异常重启后，是否自动加到资产中</param>
        public AssetOrderItem(Dictionary<int, UserAssetItem> assetItems, string tag, AutoConsumeType autoConsumeType = AutoConsumeType.AutoConsume)
        {
            this.tag = tag;
            this.assetItems = assetItems;
            this.autoConsumeType = (int)autoConsumeType;
        }

        /// <summary>
        /// 消耗订单，将订单资产加到用户资产中
        /// </summary>
        public void ConsumeOrder()
        {
            GameBase.Instance.GetModule<UserAssetManager>().ConsumeOrder(this);
        }

        /// <summary>
        /// 丢弃订单，订单资产不会加到用户资产中
        /// </summary>
        public bool RemoveOrder()
        {
            return GameBase.Instance.GetModule<UserAssetManager>().RemoveOrder(this);
        }

        public void SetDirty()
        {
            GameBase.Instance.GetModule<UserAssetManager>().SetDirty();
        }

        #region 订单资产管理

        /// <summary>
        /// 获取资产数量
        /// </summary>
        /// <param name="assetId">资产类型</param>
        /// <param name="includeOrder">是否包含未消耗订单的数据</param>
        /// <returns>资产数量</returns>
        public long GetAssetNum(int assetId)
        {
            return Math.Max(0, GetAssetItem(assetId)?.assetNum ?? 0);
        }

        /// <summary>
        /// 设置资产数值
        /// </summary>
        /// <param name="assetId">资产类型</param>
        /// <param name="assetNum">资产数量</param>
        public long SetAssetNum(int assetId, long assetNum)
        {
            var item = GetOrCreateAssetItem(assetId);
            item.assetNum = Math.Max(0, assetNum);
            SetDirty();
            return item.assetNum;
        }
        
        /// <summary>
        /// 增加资产数量
        /// </summary>
        /// <param name="assetId">资产类型</param>
        /// <param name="assetNum">资产数量</param>
        /// <returns>新的资产数量</returns>
        public long AddAssetNum(int assetId, long assetNum)
        {
            var oldAssetNum = GetAssetNum(assetId);
            var newAssetNum = oldAssetNum.SafeIncrement(assetNum);
            SetAssetNum(assetId, newAssetNum);
            return newAssetNum;
        }
        
        /// <summary>
        /// 直接减少资产数量
        /// </summary>
        /// <param name="assetId">资产类型</param>
        /// <param name="assetNum">资产数量</param>
        /// <returns>新的资产数量</returns>
        public long SubAssetNum(int assetId, long assetNum)
        {
            return AddAssetNum(assetId, -assetNum);
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
            return assetItems.TryGetValue(assetId, out var item) ? item : null;
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
                assetItems[assetId] = item;
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
            if (item != null && item.data is T data)
            {
                return data;
            }
            return defaultValue;
        }

        /// <summary>
        /// 设置资产其他数据
        /// </summary>
        /// <param name="assetId">资产类型</param>
        /// <param name="assetData">资产其他数据</param>
        public void SetAssetData(int assetId, object assetData)
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
        public void SetAssetData(int assetId, long assetNum, object assetData)
        {
            var item = GetOrCreateAssetItem(assetId);
            item.assetNum = Math.Min(0, assetNum);
            item.data = assetData;
            SetDirty();
        }

        /// <summary>
        /// 清除资产
        /// </summary>
        /// <param name="assetId">资产类型</param>
        /// <returns>清除前的资产数值</returns>
        public long ClearAsset(int assetId)
        {
            var item = GetAssetItem(assetId);
            if (item == null) return 0;
            var oldNum = item.assetNum;
            item.assetNum = 0;
            item.data = null;
            assetItems.Remove(assetId);
            SetDirty();
            return oldNum;
        }

        #endregion
    }
}
