using System.Collections.Generic;
using System.Linq;

namespace AppBase.UserAssets
{
    /// <summary>
    /// 用户资产管理器：订单管理
    /// </summary>
    public partial class UserAssetManager
    {
        /// <summary>
        /// 资产订单列表
        /// </summary>
        public List<AssetOrderItem> OrderItems => UserAssetRecord.ArchiveData.orderItems;
        
        /// <summary>
        /// 记录订单
        /// </summary>
        /// <param name="orderItem">订单数据</param>
        public AssetOrderItem AddOrder(AssetOrderItem orderItem)
        {
            if (OrderItems.Contains(orderItem)) return orderItem;
            OrderItems.Add(orderItem);
            SetDirty();
            return orderItem;
        }
        
        /// <summary>
        /// 消耗订单 
        /// </summary>
        /// <param name="orderItem">订单数据</param>
        /// <returns>是否消耗成功</returns>
        public bool ConsumeOrder(AssetOrderItem orderItem)
        {
            if (orderItem == null) return false;
            foreach (var assetItem in orderItem.assetItems.Values)
            {
                //优先尝试调用自定义的消耗逻辑
                if (OnConsumeAsset(assetItem))
                {
                    //自定义消耗成功，不再调用默认的消耗逻辑
                    continue;
                }
                //调用默认的消耗逻辑
                if (assetItem.data != null)
                {
                    SetAssetData(assetItem.assetId, assetItem.data, orderItem.tag);
                }
                if (assetItem.assetNum != 0)
                {
                    AddAssetNum(assetItem.assetId, assetItem.assetNum, orderItem.tag);
                }
            }
            orderItem.assetItems.Clear();
            OrderItems.Remove(orderItem);
            SetDirty();
            return true;
        }
        
        /// <summary>
        /// 消耗所有订单
        /// </summary>
        public void ConsumeAllOrders()
        {
            if (OrderItems.Count == 0) return;
            var list = new List<AssetOrderItem>(OrderItems);
            foreach (var orderItem in list)
            {
                ConsumeOrder(orderItem);
            }
        }

        /// <summary>
        /// 删除订单
        /// </summary>
        /// <param name="orderItem">订单数据</param>
        /// <returns>删除成功</returns>
        public bool RemoveOrder(AssetOrderItem orderItem)
        {
            if (OrderItems.Remove(orderItem))
            {
                SetDirty();
                return true;
            }
            return false;
        }
        
        /// <summary>
        /// 根据来源标签查询订单
        /// </summary>
        /// <param name="tag">来源标签，打点使用</param>
        /// <returns>订单列表</returns>
        public List<AssetOrderItem> GetOrdersByTag(string tag)
        {
            return OrderItems.FindAll(order => order.tag == tag);
        }
        
        /// <summary>
        /// 根据来源标签消耗订单
        /// </summary>
        /// <param name="tag">来源标签，打点使用</param>
        /// <returns>订单列表</returns>
        public List<AssetOrderItem> ConsumeOrdersByTag(string tag)
        {
            var list = GetOrdersByTag(tag);
            foreach (var orderItem in list)
            {
                ConsumeOrder(orderItem);
            }
            return list;
        }
        
        /// <summary>
        /// 计算所有订单中资产的总数
        /// </summary>
        /// <param name="assetId">资源类型</param>
        /// <returns>资产总数</returns>
        public long GetOrdersAssetNum(int assetId)
        {
            return OrderItems.Sum(x => x.GetAssetNum(assetId));
        }
        
        /// <summary>
        /// 自动消耗订单
        /// </summary>
        public void AutoConsumeOrders()
        {
            if (OrderItems.Count == 0) return;
            var list = OrderItems.Where(x => x.autoConsumeType == (int)AutoConsumeType.AutoConsume).ToList();
            list.ForEach(x => x.ConsumeOrder());
        }
    }
}