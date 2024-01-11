using System;
using System.Collections.Generic;
using AppBase.Archive;

namespace AppBase.Payment
{
    public class PaymentRecord : BaseRecord<PaymentArchiveData>
    {
        public bool HasFinishedOrder(PaymentOrderInfo orderInfo)
        {
            var oldOrder = ArchiveData.finishedOrders.Find(c => c.transactionId == orderInfo.transactionId);
            return oldOrder != null;
        }
        
        public void AddFinishedOrder(PaymentOrderInfo orderInfo)
        {
            var oldOrder = ArchiveData.finishedOrders.Find(c => c.transactionId == orderInfo.transactionId);
            if (oldOrder != null)
            {
                return;
            }
            ArchiveData.finishedOrders.Add(orderInfo);
            //限制存档记录100条支付成功的记录，避免存档过大
            if (ArchiveData.finishedOrders.Count > 100)
            {
                ArchiveData.finishedOrders.RemoveAt(0);
            }
            SetDirty();
        }
    }
    
    /// <summary>
    /// 用户充值信息
    /// </summary>
    [Serializable]
    public class PaymentArchiveData : BaseArchiveData
    {
        /// <summary>
        /// 支付完成、校验完成、并已发奖的订单
        /// </summary>
        public List<PaymentOrderInfo> finishedOrders = new();
        
        /// <summary>
        /// 首次充值时间
        /// </summary>
        public long firstPaymentTime = 0;
        
        /// <summary>
        /// 最近一次充值时间
        /// </summary>
        public long lastPaymentTime = 0;
        
        /// <summary>
        /// 充值总金额（美分）
        /// </summary>
        public int iapTotal = 0;
        
        /// <summary>
        /// 充值总次数
        /// </summary>
        public int iapCount = 0;
    }
    
    /// <summary>
    /// 订单信息
    /// </summary>
    [Serializable]
    public class PaymentOrderInfo
    {
        /// <summary>
        /// 商品Key
        /// </summary>
        public string productKey;
        
        /// <summary>
        /// IAP底层的商品Id
        /// </summary>
        public string productId;

        /// <summary>
        /// 购买来源，打点使用
        /// </summary>
        public string position;

        /// <summary>
        /// 支付状态
        /// </summary>
        public PaymentState state;
        
        /// <summary>
        /// 订单ID
        /// </summary>
        public string transactionId;
        
        /// <summary>
        /// 创建时间，本地UTC，毫秒
        /// </summary>
        public long createTime;
        
        /// <summary>
        /// 订单消单的时间，本地UTC，毫秒
        /// </summary>
        public long finishTime;

        /// <summary>
        /// 创建订单
        /// </summary>
        public PaymentOrderInfo()
        {
        }
        
        /// <summary>
        /// 创建订单
        /// </summary>
        /// <param name="productKey">商品Key</param>
        /// <param name="position">购买来源，打点使用</param>
        public PaymentOrderInfo(string productKey, string position)
        {
            this.productKey = productKey;
            this.position = position;
            createTime = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
        }
        
        /// <summary>
        /// 创建订单
        /// </summary>
        /// <param name="productKey">商品Key</param>
        /// <param name="productId">商品Id</param>
        /// <param name="position">购买来源，打点使用</param>
        public PaymentOrderInfo(string productKey, string productId, string position)
        {
            this.productKey = productKey;
            this.productId = productId;
            this.position = position;
            createTime = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
        }
    }
}
