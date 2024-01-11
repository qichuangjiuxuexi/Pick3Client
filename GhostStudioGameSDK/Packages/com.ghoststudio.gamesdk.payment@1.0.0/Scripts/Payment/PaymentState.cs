namespace AppBase.Payment
{
    /// <summary>
    /// 支付状态
    /// </summary>
    public enum PaymentState
    {
        /// <summary>
        /// 刚创建订单，未发起支付
        /// </summary>
        Invalid = 0,
        
        /// <summary>
        /// 发起了请求，点击某物品购买按钮
        /// </summary>
        RequestInitiated = 100,
        
        /// <summary>
        /// 无法发起请求，当前不可支付
        /// </summary>
        RequestUnavaliable = 200,
        
        /// <summary>
        /// 玩家点击取消
        /// </summary>
        RequestCancelled = 300,
        
        /// <summary>
        /// 支付出错
        /// </summary>
        RequestFailed = 400,
        
        /// <summary>
        /// 支付回调，等待验证
        /// </summary>
        RequestValidate = 500,
        
        /// <summary>
        /// 验证失败
        /// </summary>
        ValidateFailed = 600,
        
        /// <summary>
        /// 支付成功
        /// </summary>
        PaymentSucceed = 700,
        
        /// <summary>
        /// 已发奖消单
        /// </summary>
        RewardConsumed = 800,
    }
    
    public static class PaymentStateExtention
    {
        /// <summary>
        /// 是否是支付失败状态
        /// </summary>
        public static bool IsFailed(this PaymentState state)
        {
            return state > PaymentState.RequestInitiated && state < PaymentState.PaymentSucceed;
        }
        
        /// <summary>
        /// 是否是支付成功状态
        /// </summary>
        public static bool IsSuccess(this PaymentState state)
        {
            return state == PaymentState.PaymentSucceed || state == PaymentState.RewardConsumed;
        }
    }
}