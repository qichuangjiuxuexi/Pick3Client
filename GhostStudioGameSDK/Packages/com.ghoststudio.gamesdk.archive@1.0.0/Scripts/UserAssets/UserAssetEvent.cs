
namespace AppBase.Event
{
    /// <summary>
    /// 正式资产数变化事件
    /// </summary>
    public struct EventUserAssetChange  : IEvent
    {
        /// <summary>
        /// 资产类型
        /// </summary>
        public int assetId;
        
        /// <summary>
        /// 旧的资产数值
        /// </summary>
        public long oldAssetNum;
        
        /// <summary>
        /// 新的资产数值
        /// </summary>
        public long newAssetNum;
        
        /// <summary>
        /// 来源标签
        /// </summary>
        public string tag;

        public EventUserAssetChange(int assetId, long oldAssetNum, long newAssetNum, string tag)
        {
            this.assetId = assetId;
            this.oldAssetNum = oldAssetNum;
            this.newAssetNum = newAssetNum;
            this.tag = tag;
        }
    }
}
