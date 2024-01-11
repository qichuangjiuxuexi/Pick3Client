using System;
using System.Collections.Generic;
using AppBase.Archive;

namespace AppBase.UserAssets
{
    /// <summary>
    /// 资产存档
    /// </summary>
    public class UserAssetRecord : BaseRecord<UserAssetArchiveData>
    {
    }
    
    /// <summary>
    /// 资产存档数据
    /// </summary>
    [Serializable]
    public class UserAssetArchiveData : BaseArchiveData
    {
        /// <summary>
        /// 用户资产
        /// </summary>
        public Dictionary<int, UserAssetItem> assetItems = new();
        
        /// <summary>
        /// 订单列表
        /// </summary>
        public List<AssetOrderItem> orderItems = new();
        
        /// <summary>
        /// 用户临时资产
        /// </summary>
        public Dictionary<int, UserAssetItem> tempAssets = new();

        /// <summary>
        /// 是否添加过初始资源
        /// </summary>
        public bool hasAddedInitAsset;
    }
}
