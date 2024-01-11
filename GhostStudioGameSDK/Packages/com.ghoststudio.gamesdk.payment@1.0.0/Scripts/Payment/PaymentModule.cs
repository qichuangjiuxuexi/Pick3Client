using System;
using System.Collections.Generic;
using System.Linq;
using AppBase.Event;
using AppBase.Module;
using AppBase.UI.Waiting;
using Cysharp.Threading.Tasks;

namespace AppBase.Payment
{
    /// <summary>
    /// 业务层的支付模块，用于处理数据层逻辑
    /// </summary>
    public class PaymentModule : ModuleBase
    {
        /// <summary>
        /// 监听的商品Key列表
        /// </summary>
        protected HashSet<string> productKeys = new();
        
        /// <summary>
        /// 是否自动展示Waiting遮罩，如果为null，则不展示
        /// </summary>
        public WaitingData AutoShowWaiting = new WaitingData(null, 20);
        
        /// <summary>
        /// 支付成功的数据层处理回调，可返回object回传给UI层
        /// </summary>
        public Func<PaymentOrderInfo, object> OnPaymentSuccessCallback;
        
        /// <summary>
        /// 支付补单的数据层处理回调，补单完毕后需要回调callback
        /// </summary>
        public Action<PaymentOrderInfo, Action> OnPaymentRestoredCallback;
        
        /// <summary>
        /// 支付失败的数据层处理回调，可返回object回传给UI层
        /// </summary>
        public Func<PaymentOrderInfo, object> OnPaymentFailedCallback;
        
        /// <summary>
        /// 记录本次等待的支付完成回调
        /// </summary>
        protected Action<bool, PaymentOrderInfo, object> waitingCallback;
        
        /// <summary>
        /// 记录本次等待的支付商品ID
        /// </summary>
        protected string waitingProductKey;

        protected sealed override void OnInternalInit()
        {
            base.OnInternalInit();
            AddModule<EventModule>()
                .Subscribe<EventPaymentSuccess>(HandlePaymentSuccess)
                .Subscribe<EventPaymentFailed>(HandlePaymentFailed)
                .Subscribe<EventPaymentRestored>(HandlePaymentRestored);

            //可以通过传参注册需要监听的商品ID
            switch (moduleData)
            {
                case string productKey:
                    Register(productKey);
                    break;
                case IAPConfig config:
                    Register(config);
                    break;
                case IEnumerable<string> productKeys:
                    Register(productKeys);
                    break;
                case IEnumerable<IAPConfig> configs:
                    Register(configs);
                    break;
            }
        }

        /// <summary>
        /// 注册需要监听的商品Key
        /// </summary>
        public PaymentModule Register(string productKey)
        {
            productKeys.Add(productKey);
            return this;
        }

        /// <summary>
        /// 注册需要监听的商品Key列表
        /// </summary>
        public PaymentModule Register(IEnumerable<string> productKeys)
        {
            this.productKeys.UnionWith(productKeys);
            return this;
        }

        /// <summary>
        /// 注册需要监听的商品配置
        /// </summary>
        public PaymentModule Register(IAPConfig iapConfig)
        {
            productKeys.Add(iapConfig.Key);
            return this;
        }
        
        /// <summary>
        /// 注册需要监听的商品配置
        /// </summary>
        public PaymentModule Register(IEnumerable<IAPConfig> iapConfigs)
        {
            productKeys.UnionWith(iapConfigs.Select(c => c.Key));
            return this;
        }
        
        /// <summary>
        /// 收到支付成功事件
        /// </summary>
        private void HandlePaymentSuccess(EventPaymentSuccess evt)
        {
            if (!productKeys.Contains(evt.orderInfo.productKey)) return;
            if (evt.orderInfo.state != PaymentState.PaymentSucceed) return;
            if (AutoShowWaiting != null)
            {
                GameBase.Instance.GetModule<WaitingManager>().HideWaiting();
            }
            var obj = OnPaymentSuccessCallback != null ? OnPaymentSuccessCallback.Invoke(evt.orderInfo) : OnPaymentSuccess(evt.orderInfo);
            if (waitingProductKey == evt.orderInfo.productKey && waitingCallback != null)
            {
                waitingCallback.Invoke(true, evt.orderInfo, obj);
                waitingCallback = null;
                waitingProductKey = null;
            }
        }
        
        /// <summary>
        /// 支付成功的数据层处理回调，可重载该函数实现自己的逻辑
        /// </summary>
        /// <returns>可返回object回传给UI层</returns>
        protected virtual object OnPaymentSuccess(PaymentOrderInfo orderInfo)
        {
            return null;
        }
        
        /// <summary>
        /// 收到支付失败事件
        /// </summary>
        private void HandlePaymentFailed(EventPaymentFailed evt)
        {
            if (!productKeys.Contains(evt.orderInfo.productKey)) return;
            if (AutoShowWaiting != null)
            {
                GameBase.Instance.GetModule<WaitingManager>().HideWaiting();
            }
            var obj = OnPaymentFailedCallback != null ? OnPaymentFailedCallback.Invoke(evt.orderInfo) : OnPaymentFailed(evt.orderInfo);
            if (waitingProductKey == evt.orderInfo.productKey && waitingCallback != null)
            {
                waitingCallback.Invoke(false, evt.orderInfo, obj);
                waitingCallback = null;
                waitingProductKey = null;
            }
        }
        
        /// <summary>
        /// 支付失败的数据层处理回调，可重载该函数实现自己的逻辑
        /// </summary>
        /// <returns>可返回object回传给UI层</returns>
        protected virtual object OnPaymentFailed(PaymentOrderInfo orderInfo)
        {
            return null;
        }
        
        /// <summary>
        /// 收到支付补单事件
        /// </summary>
        private async UniTask HandlePaymentRestored(EventPaymentRestored evt)
        {
            if (!productKeys.Contains(evt.orderInfo.productKey)) return;
            if (evt.orderInfo.state != PaymentState.PaymentSucceed) return;
            if (AutoShowWaiting != null)
            {
                GameBase.Instance.GetModule<WaitingManager>().HideWaiting();
            }
            if (OnPaymentRestoredCallback != null)
            {
                await OnPaymentRestoredCallback.InvokeAsync(evt.orderInfo);
            }
            else
            {
                await UniTaskUtil.InvokeAsync(callback => OnPaymentRestored(evt.orderInfo, callback));
            }
        }
        
        /// <summary>
        /// 支付补单的数据层处理回调，可重载该函数实现自己的逻辑
        /// </summary>
        /// <param name="orderInfo">订单</param>
        /// <param name="callback">补单完毕后需要回调callback</param>
        protected virtual void OnPaymentRestored(PaymentOrderInfo orderInfo, Action callback)
        {
            callback?.Invoke();
        }

        /// <summary>
        /// 发起支付
        /// </summary>
        /// <param name="productKey">商品Key</param>
        /// <param name="position">支付来源，打点使用</param>
        /// <param name="callback">支付完成回调，只回调一次（是否支付成功，订单数据，额外数据）</param>
        /// <returns>是否启动支付，如果true则需要设置UI按钮禁用</returns>
        public bool Purchase(string productKey, string position, Action<bool, PaymentOrderInfo, object> callback)
        {
            if (string.IsNullOrEmpty(productKey)) return false;
            return Purchase(new PaymentOrderInfo(productKey, position), callback);
        }

        /// <summary>
        /// 发起支付
        /// </summary>
        /// <param name="iapConfig">商品配置</param>
        /// <param name="position">支付来源，打点使用</param>
        /// <param name="callback">支付完成回调，只回调一次（是否支付成功，订单数据，额外数据）</param>
        /// <returns>是否启动支付，如果true则需要设置UI按钮禁用</returns>
        public bool Purchase(IAPConfig iapConfig, string position, Action<bool, PaymentOrderInfo, object> callback)
        {
            if (iapConfig == null) return false;
            return Purchase(new PaymentOrderInfo(iapConfig.Key, iapConfig.GetProductId(), position), callback);
        }

        /// <summary>
        /// 发起支付
        /// </summary>
        /// <param name="iapConfig">发起订单</param>
        /// <param name="callback">支付完成回调，只回调一次（是否支付成功，订单数据，额外数据）</param>
        /// <returns>是否启动支付，如果true则需要设置UI按钮禁用</returns>
        public bool Purchase(PaymentOrderInfo orderInfo, Action<bool, PaymentOrderInfo, object> callback)
        {
            if (orderInfo == null) return false;
            var productKey = orderInfo.productKey;
            if (!productKeys.Contains(productKey))
            {
                Debugger.LogError(TAG, $"Purchase failed, productKey: {productKey} not registered in PaymentModule");
                return false;
            }
            if (GameBase.Instance.GetModule<PaymentManager>().Purchase(orderInfo))
            {
                waitingCallback = callback;
                waitingProductKey = productKey;
                //自动显示Waiting遮罩
                if (AutoShowWaiting != null)
                {
                    GameBase.Instance.GetModule<WaitingManager>().ShowWaiting(AutoShowWaiting);
                }
                return true;
            }
            return false;
        }

        /// <summary>
        /// 发奖完成后，发起消单
        /// </summary>
        /// <param name="orderInfo">订单信息</param>
        public void Consume(PaymentOrderInfo orderInfo)
        {
            GameBase.Instance.GetModule<PaymentManager>().Consume(orderInfo);
        }
        
        /// <summary>
        /// 获取本地显示价格
        /// </summary>
        public string GetLocalizedPrice(string productKey)
        {
            return GameBase.Instance.GetModule<PaymentManager>().GetLocalizedPrice(productKey);
        }
        
        /// <summary>
        /// 获取本地显示价格
        /// </summary>
        public string GetLocalizedPrice(IAPConfig config)
        {
            return GameBase.Instance.GetModule<PaymentManager>().GetLocalizedPrice(config);
        }
    }
}
