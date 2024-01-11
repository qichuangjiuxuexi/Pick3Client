using AppBase.Event;
using AppBase.Module;

namespace AppBase.UserAssets
{
    /// <summary>
    /// 用户资产管理器
    /// </summary>
    /// ---------------------
    /// 使用
    /// 用户获得资产
    ///     1.直接发放
    ///     2.添加到临时资产，回大厅统一发放
    ///     3.创建一个资产订单，等板子领取时发放
    /// 资产订单流程
    ///      创建->等到发放时->手动销毁订单
    ///         ->如果异常->重新登录->设置了AutoConsume的订单会自动发放
    ///                            未设置的需要业务层手动通过tag标签或者assetId或者data找到手动发放
    /// 
    public partial class UserAssetManager : ModuleBase
    {
        /// <summary>
        /// 资产存档
        /// </summary>
        protected UserAssetRecord UserAssetRecord;

        protected override void OnInit()
        {
            base.OnInit();
            UserAssetRecord = AddModule<UserAssetRecord>();
            TempAssets = AddModule<TempAssetManager>();
        }

        protected override void OnAfterInit()
        {
            base.OnAfterInit();
            AutoConsumeOrders();
        }

        /// <summary>
        /// 是否添加了初始资产
        /// </summary>
        /// <returns></returns>
        public bool HasAddInitAsset()
        {
            return UserAssetRecord.ArchiveData.hasAddedInitAsset;
        }
        
        /// <summary>
        /// 添加完成初始化资产调用
        /// </summary>
        public void AfterAddedInitAsset()
        {
            UserAssetRecord.ArchiveData.hasAddedInitAsset = true;
            UserAssetRecord.SetDirty();
        }

        /// <summary>
        /// 重新加载存档
        /// </summary>
        public void ReloadAssets()
        {
            RemoveAllModules();
            OnInit();
        }

        /// <summary>
        /// 当资产发生变化时，自动存档
        /// </summary>
        public void SetDirty()
        {
            UserAssetRecord.SetDirty();
        }

        /// <summary>
        /// 是否是新的资产存档
        /// </summary>
        public bool IsNewRecord
        {
            get => UserAssetRecord.IsNewRecord;
            set
            {
                UserAssetRecord.IsNewRecord = value;
                UserAssetRecord.SetDirty();
            }
        }

        /// <summary>
        /// 当消耗订单时调用，可自定义资产消耗逻辑
        /// </summary>
        /// <param name="orderItem">资产订单</param>
        /// <returns>是否不再调用默认的消耗逻辑</returns>
        public virtual bool OnConsumeAsset(UserAssetItem orderItem)
        {
            return false;
        }
    }
}
