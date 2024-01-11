using AppBase.Event;

namespace AppBase.Payment
{
    /// <summary>
    /// 支付成功通知，等待发奖
    /// </summary>
    public struct EventPaymentSuccess : IEvent
    {
        public PaymentOrderInfo orderInfo;
        public EventPaymentSuccess(PaymentOrderInfo orderInfo)
        {
            this.orderInfo = orderInfo;
        }
    }
    
    /// <summary>
    /// 支付失败通知
    /// </summary>
    public struct EventPaymentFailed : IEvent
    {
        public PaymentOrderInfo orderInfo;
        public EventPaymentFailed(PaymentOrderInfo orderInfo)
        {
            this.orderInfo = orderInfo;
        }
    }
    
    /// <summary>
    /// 发生补单，等待发奖
    /// </summary>
    public struct EventPaymentRestored : IEvent
    {
        public PaymentOrderInfo orderInfo;
        public EventPaymentRestored(PaymentOrderInfo orderInfo)
        {
            this.orderInfo = orderInfo;
        }
    }
    
    /// <summary>
    /// 当订单发奖完成，IAP信息发生更新
    /// </summary>
    public struct EventPaymentConsumed : IEvent
    {
        public PaymentOrderInfo orderInfo;
        public EventPaymentConsumed(PaymentOrderInfo orderInfo)
        {
            this.orderInfo = orderInfo;
        }
    }

    /// <summary>
    /// 支付系统初始化完成，可以刷新本地价格
    /// </summary>
    public struct EventPaymentInitiated : IEvent
    {
    }
}
