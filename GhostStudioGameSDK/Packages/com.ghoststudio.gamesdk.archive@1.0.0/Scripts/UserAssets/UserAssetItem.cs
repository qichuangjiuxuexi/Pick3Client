using System;

namespace AppBase.UserAssets
{
    /// <summary>
    /// 资产数据
    /// </summary>
    [Serializable]
    public class UserAssetItem
    {
        /// <summary>
        /// 资产编号
        /// </summary>
        public int assetId;

        /// <summary>
        /// 资产数量
        /// </summary>
        public long assetNum;
        
        /// <summary>
        /// 其它资产数据
        /// </summary>
        public object data;
        
        public UserAssetItem()
        {
        }

        public UserAssetItem(int assetId, long assetNum, object data = null)
        {
            this.assetId = assetId;
            this.assetNum = assetNum;
            this.data = data;
        }
    }
}