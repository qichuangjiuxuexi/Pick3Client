using System;
using System.Collections.Generic;
using System.Linq;
using AppBase.Analytics;
using AppBase.Config;
using AppBase.Event;
using AppBase.Module;
using AppBase.Timing;
using AppBase.Utils;
using UnityEngine;
using UnityEngine.Purchasing;
using UnityEngine.Purchasing.Extension;
using UnityEngine.Purchasing.Security;

namespace AppBase.Payment
{
    /// <summary>
    /// 支付系统
    /// </summary>
    public class PaymentManager : ModuleBase, IDetailedStoreListener
    {
        /// <summary>
        /// IAP底层控制器
        /// </summary>
        protected IStoreController controller;
        
        /// <summary>
        /// 支付存档
        /// </summary>
        public PaymentRecord paymentRecord;
        
        /// <summary>
        /// 是否是模拟支付
        /// </summary>
        public bool isSimulateDebug;
        
        /// <summary>
        /// 配置字典：productId -> iapConfig
        /// </summary>
        protected Dictionary<string, IAPConfig> productIdConfigs;
        
        /// <summary>
        /// 初始化失败重试次数
        /// </summary>
        protected int initRetryCount;

        /// <summary>
        /// 未完成订单列表：productId -> PaymentOrderInfo
        /// </summary>
        protected Dictionary<string, PaymentOrderInfo> pendingOrders = new();
        
        /// <summary>
        /// 验证成功待发奖的订单列表：productId -> PaymentOrderInfo
        /// </summary>
        protected Dictionary<string, PaymentOrderInfo> verifiedOrders = new();

        protected override void OnInit()
        {
            base.OnInit();
            InitPurchasing();
            RegisterPaymentProperty();
        }

        /// <summary>
        /// 初始化支付系统
        /// </summary>
        public void InitPurchasing()
        {
            LogInfo("InitPurchasing");
            paymentRecord = AddModule<PaymentRecord>();
            productIdConfigs = GameBase.Instance.GetModule<ConfigManager>().GetConfigList<IAPConfig>(AAConst.IAPConfig).ToDictionary(PaymentExtension.GetProductId);
            var builder = ConfigurationBuilder.Instance(StandardPurchasingModule.Instance());
            builder.AddProducts(productIdConfigs.Select(c => new ProductDefinition(c.Key, (ProductType)c.Value.IAP_Type)));
            UnityPurchasing.Initialize(this, builder);
        }

        /// <summary>
        /// 获取商品IAP配置
        /// </summary>
        /// <param name="productKey"></param>
        /// <returns></returns>
        public IAPConfig GetIAPConfig(string productKey)
        {
            var iapConfig = GameBase.Instance.GetModule<ConfigManager>().GetConfigByKey<string, IAPConfig>(AAConst.IAPConfig, productKey);
            if (iapConfig == null)
            {
                Debugger.LogError(TAG, $"GetIAPConfig: productKey: {productKey} not found in IAPConfig");
            }
            return iapConfig;
        }
        
        /// <summary>
        /// 获取本地显示价格
        /// </summary>
        /// <param name="productId">商品Key</param>
        /// <returns>本地显示价格</returns>
        public string GetLocalizedPrice(string productKey)
        {
            return GetLocalizedPrice(GetIAPConfig(productKey));
        }

        /// <summary>
        /// 获取本地显示价格
        /// </summary>
        /// <param name="iapConfig">商品配置</param>
        /// <returns>本地显示价格</returns>
        public string GetLocalizedPrice(IAPConfig iapConfig)
        {
            if (iapConfig == null) return string.Empty;
            var product = controller?.products.WithID(iapConfig.GetProductId());
            var price = product?.metadata.localizedPriceString;
            return string.IsNullOrEmpty(price) ? iapConfig.DisplayPrice : price;
        }

        /// <summary>
        /// 启动支付
        /// </summary>
        /// <param name="productKey">商品Key</param>
        /// <param name="position">购买来源，打点使用</param>
        public bool Purchase(string productKey, string position)
        {
            return Purchase(GetIAPConfig(productKey), position);
        }

        /// <summary>
        /// 启动支付
        /// </summary>
        /// <param name="iapConfig">商品配置</param>
        /// <param name="position">购买来源，打点使用</param>
        public bool Purchase(IAPConfig iapConfig, string position)
        {
            if (iapConfig == null) return false;
            var orderInfo = new PaymentOrderInfo(iapConfig.Key, iapConfig.GetProductId(), position);
            return Purchase(orderInfo);
        }
        
        /// <summary>
        /// 启动支付
        /// </summary>
        /// <param name="orderInfo">订单信息</param>
        public bool Purchase(PaymentOrderInfo orderInfo)
        {
            if (orderInfo == null || string.IsNullOrEmpty(orderInfo.productKey)) return false;

            //获取商品Id
            if (string.IsNullOrEmpty(orderInfo.productId))
            {
                var iapConfig = GetIAPConfig(orderInfo.productKey);
                if (iapConfig == null) return false;
                orderInfo.productId = iapConfig.GetProductId();
            }
            
            //模拟支付
            if (AppUtil.IsDebug && isSimulateDebug)
            {
                return SimulatePurchase(orderInfo);
            }

            //支付不可用
            if (controller == null)
            {
                OnPaymentFailed(orderInfo, PaymentState.RequestUnavaliable, "Payment is not available");
                return false;
            }
            
            //商品未找到
            var product = controller.products.WithID(orderInfo.productId);
            if (product == null)
            {
                OnPaymentFailed(orderInfo, PaymentState.RequestFailed, $"ProductId {orderInfo.productId} not found");
                return false;
            }
            
            //商品不能购买
            if (!product.availableToPurchase)
            {
                OnPaymentFailed(orderInfo, PaymentState.RequestFailed, $"Product {orderInfo.productId} is not available to purchase");
                return false;
            }
            
            //开始支付
            LogInfo($"Purchase begin: productKey: {orderInfo.productKey}, productId: {orderInfo.productId}");
            orderInfo.state = PaymentState.RequestInitiated;
            pendingOrders[orderInfo.productId] = orderInfo;
            LogPaymentEvent(orderInfo);
            GameBase.Instance.GetModule<TimingManager>().GlobalDelayCallFrame(1, () => controller.InitiatePurchase(product));
            return true;
        }

        /// <summary>
        /// 模拟支付
        /// </summary>
        protected bool SimulatePurchase(PaymentOrderInfo orderInfo)
        {
            //商品未找到
            if (!productIdConfigs.ContainsKey(orderInfo.productId))
            {
                OnPaymentFailed(orderInfo, PaymentState.RequestFailed, $"Product {orderInfo.productId} not found");
                return false;
            }
            //开始支付
            pendingOrders[orderInfo.productId] = orderInfo;
            orderInfo.transactionId = "Sim_" + Guid.NewGuid();
            orderInfo.state = PaymentState.RequestInitiated;
            LogPaymentEvent(orderInfo);
            orderInfo.state = PaymentState.RequestValidate;
            LogPaymentEvent(orderInfo);
            GameBase.Instance.GetModule<TimingManager>().GlobalDelayCallFrame(1, () => OnPaymentSuccess(orderInfo));
            return true;
        }

        /// <summary>
        /// 发奖完成，消耗订单
        /// </summary>
        public void Consume(PaymentOrderInfo orderInfo)
        {
            if (orderInfo == null || string.IsNullOrEmpty(orderInfo.productId)) return;
            RecordPaymentConsumed(orderInfo);
            
            //到商店消耗订单
            if (orderInfo.transactionId.StartsWith("Sim_"))
            {
                //模拟支付无需消单
                return;
            }
            if (controller == null)
            {
                //支付系统没有准备好
                LogError("ConsumePurchase: Payment is not available");
                return;
            }
            var product = controller.products.WithID(orderInfo.productId);
            if (product == null)
            {
                LogError($"ConsumePurchase: Product {orderInfo.productId} not found");
                return;
            }
            controller.ConfirmPendingPurchase(product);
        }
        
        /// <summary>
        /// 支付失败
        /// </summary>
        protected void OnPaymentFailed(PaymentOrderInfo orderInfo, PaymentState paymentState, string errorMessage)
        {
            orderInfo.state = paymentState;
            if (paymentState == PaymentState.RequestCancelled)
            {
                LogInfo($"OnPaymentCancelled: {orderInfo.productId}");
            }
            else
            {
                LogError($"OnPaymentFailed: {orderInfo.productId} {paymentState} {errorMessage}");
            }
            LogPaymentEvent(orderInfo, errorMessage);
            GameBase.Instance.GetModule<EventManager>().Broadcast(new EventPaymentFailed(orderInfo));
        }
        
        /// <summary>
        /// 验证支付
        /// </summary>
        protected void OnPaymentValidate(PaymentOrderInfo orderInfo, PurchaseEventArgs receiptInfo)
        {
            //Editor不需要校验，直接返回校验成功
            if (AppUtil.IsEditor)
            {
                orderInfo.transactionId = receiptInfo.purchasedProduct.transactionID;
                OnPaymentSuccess(orderInfo);
                return;
            }
            
            //校验订单
            var receipt = receiptInfo.purchasedProduct.receipt;
            var validator = new CrossPlatformValidator(GooglePlayTangle.Data(), AppleTangle.Data(), Application.identifier);
            
            LogPaymentalidate(orderInfo.productKey, orderInfo.productId, orderInfo.transactionId, "start", receipt);
            
            IPurchaseReceipt[] result;
            try
            {
                result = validator.Validate(receipt);
            }
            catch (IAPSecurityException e)
            {
                LogPaymentalidate(orderInfo.productKey, orderInfo.productId, orderInfo.transactionId, "failed", receipt, e.Message);
                OnPaymentFailed(orderInfo, PaymentState.ValidateFailed, e.Message);
                return;
            }
            
            //商品ID不匹配
            var product = result?.FirstOrDefault(p => p.productID == orderInfo.productId);
            if (product == null)
            {
                string errorMessage = $"{orderInfo.productId} not match";
                LogPaymentalidate(orderInfo.productKey, orderInfo.productId, orderInfo.transactionId, "failed", receipt, errorMessage);
                OnPaymentFailed(orderInfo, PaymentState.ValidateFailed, errorMessage);
                return;
            }
            
            //检查订单状态
            
            orderInfo.transactionId = product.transactionID;
            if (product is GooglePlayReceipt googleProduct && googleProduct.purchaseState != GooglePurchaseState.Purchased)
            {
                LogError($"PaymentValidate Pending: {orderInfo.productId} purchaseState: {googleProduct.purchaseState}");
                return;
            }
            
            //已发过奖，直接消单
            if (paymentRecord.HasFinishedOrder(orderInfo))
            {
                Consume(orderInfo);
                return;
            }
            
            //校验成功 
            OnPaymentSuccess(orderInfo);
        }

        void LogPaymentalidate(string productKey, string productId, string order_id, string state, string receipt,
            string exception = "", string date = "")
        {
            GameBase.Instance.GetModule<EventManager>().Broadcast<AnalyticsEvent>(
                new AnalyticsEvent_iap_validate(productKey, productId, order_id, state, exception, receipt));
        }

        /// <summary>
        /// 支付成功
        /// </summary>
        protected void OnPaymentSuccess(PaymentOrderInfo orderInfo)
        {
            orderInfo.state = PaymentState.PaymentSucceed;
            LogPaymentEvent(orderInfo);
            Debugger.LogError("测试补单","支付成功");
            verifiedOrders[orderInfo.productId] = orderInfo;
            //不是补单的情况下通知业务层，补单的情况等检查补单的时机
            if (pendingOrders.ContainsKey(orderInfo.productId))
            {
                GameBase.Instance.GetModule<EventManager>().Broadcast(new EventPaymentSuccess(orderInfo));
            }
        }

        /// <summary>
        /// 记录消耗订单完成到存档
        /// </summary>
        protected void RecordPaymentConsumed(PaymentOrderInfo orderInfo)
        {
            if (orderInfo == null) return;
            orderInfo.finishTime = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
            orderInfo.state = PaymentState.RewardConsumed;
            if (pendingOrders.TryGetValue(orderInfo.productId, out var oldOrderInfo) && oldOrderInfo == orderInfo)
            {
                pendingOrders.Remove(orderInfo.productId);
            }
            if (verifiedOrders.TryGetValue(orderInfo.productId, out oldOrderInfo) && oldOrderInfo.transactionId == orderInfo.transactionId)
            {
                verifiedOrders.Remove(orderInfo.productId);
            }
            //如果已经Consume过了，不重复加
            if (paymentRecord.HasFinishedOrder(orderInfo)) return;
            if (paymentRecord.ArchiveData.firstPaymentTime == 0)
            {
                paymentRecord.ArchiveData.firstPaymentTime = orderInfo.finishTime;
            }
            paymentRecord.ArchiveData.lastPaymentTime = orderInfo.finishTime;
            paymentRecord.ArchiveData.iapCount++;
            if (productIdConfigs.TryGetValue(orderInfo.productId, out var config))
            {
                paymentRecord.ArchiveData.iapTotal += config.Price;
            }
            paymentRecord.AddFinishedOrder(orderInfo);
            paymentRecord.SetDirty();
            LogPaymentEvent(orderInfo);
            GameBase.Instance.GetModule<EventManager>().Broadcast(new EventPaymentConsumed(orderInfo));
        }
        
        /// <summary>
        /// 获得最大一笔充值金额
        /// </summary>
        /// <returns></returns>
        public int GetMaxIapRevenue()
        {
            int max = 0;
            paymentRecord.ArchiveData.finishedOrders.ForEach((e)=>
            {
                if (productIdConfigs.TryGetValue(e.productId, out IAPConfig cfg))
                {
                    if (cfg.Price > max)
                    {
                        max = cfg.Price;
                    }
                }
            });
            return max;
        }
        
        /// <summary>
        /// 获得平均单笔付费金额单位 美分
        /// </summary>
        /// <returns></returns>
        public int GetAverageIapRevenue()
        {
            int total = 0;
            int times = 0;
            paymentRecord.ArchiveData.finishedOrders.ForEach((e)=>
            {
                if (productIdConfigs.TryGetValue(e.productId, out IAPConfig cfg))
                {
                    total += cfg.Price;
                    times++;
                }
            });
            
            return times > 0 ? total/times : 0;
        }

        #region 补单

        /// <summary>
        /// 在检查点检查是否有补单，如果有则处理补单
        /// </summary>
        /// <param name="callback">补单全部处理完成的回调</param>
        /// <returns>是否有补单发生</returns>
        public bool CheckResotoredOrders(Action callback = null)
        {
            Debugger.LogError("测试补单","检查补单");
            if (verifiedOrders.Count == 0)
            {
                Debugger.LogError("测试补单","没有要补的订单");
                callback?.Invoke();
                return false;
            }
            var orders = verifiedOrders.Values.ToArray();
           
            void RestoreOrder(int i)
            {
                if (i >= orders.Length)
                {
                    callback?.Invoke();
                    return;
                }
                if (orders[i].finishTime > 0 || paymentRecord.HasFinishedOrder(orders[i]))
                {
                    Debugger.LogError("测试补单",$"删除订单 finishTime == {orders[i].finishTime}");
                    verifiedOrders.Remove(orders[i].productId);
                }
                else
                {
                    GameBase.Instance.GetModule<EventManager>().Broadcast(new EventPaymentRestored(orders[i]), () =>
                    {
                        Debugger.LogError("测试补单","补单完成  检查下一笔订单");
                        RestoreOrder(i + 1);
                    });
                }
            }
            RestoreOrder(0);
            return true;
        }

        #endregion
        
        #region 事件处理
        
        //初始化支付失败，每分钟尝试一次
        public void OnInitializeFailed(InitializationFailureReason error)
        {
            LogError($"OnInitializeFailed: {error} RetryCount: {initRetryCount}");
            GameBase.Instance.GetModule<TimingManager>().GlobalDelayCall(10 * Math.Min(++initRetryCount, 6), InitPurchasing);
        }

        //初始化支付失败，每分钟尝试一次
        public void OnInitializeFailed(InitializationFailureReason error, string message)
        {
            LogError($"OnInitializeFailed: {error} {message} RetryCount: {initRetryCount}");
            GameBase.Instance.GetModule<TimingManager>().GlobalDelayCall(10 * Math.Min(++initRetryCount, 6), InitPurchasing);
        }

        //获取到支付结果，开始验证
        public PurchaseProcessingResult ProcessPurchase(PurchaseEventArgs purchaseEvent)
        {
            var productId = purchaseEvent.purchasedProduct.definition.id;
            LogInfo($"ProcessPurchase: productId: {productId}");
            
            //补单
            pendingOrders.TryGetValue(productId, out var orderInfo);
            if (orderInfo == null)
            {
                LogInfo($"ProcessPurchase: restore order: {productId}");
                if (!productIdConfigs.TryGetValue(productId, out var iapConfig))
                {
                    LogError($"ProcessPurchase restore order failed: productId: {productId} not found in IAPConfig");
                    return PurchaseProcessingResult.Pending;
                }
                orderInfo = new PaymentOrderInfo(iapConfig.Key, productId, "restore");
            }
            
            OnPaymentValidate(orderInfo, purchaseEvent);
            return PurchaseProcessingResult.Pending;
        }

        //初始化成功
        public void OnInitialized(IStoreController controller, IExtensionProvider extensions)
        {
            LogInfo("OnInitialized");
            this.controller = controller;
            GameBase.Instance.GetModule<EventManager>().Broadcast(new EventPaymentInitiated());
        }

        //支付失败
        public void OnPurchaseFailed(Product product, PurchaseFailureReason failureReason)
        {
            OnPurchaseFailed(product, new PurchaseFailureDescription(product.definition.id, failureReason, failureReason.ToString()));
        }

        //支付失败
        public void OnPurchaseFailed(Product product, PurchaseFailureDescription failureDescription)
        {
            LogError($"OnPurchaseFailed: productId: {product.definition.id}, reason: {failureDescription.reason}, message: {failureDescription.message}");
            
            //找不到待处理订单，可能是补单
            if (!pendingOrders.TryGetValue(product.definition.id, out var orderInfo))
            {
                if (!productIdConfigs.TryGetValue(product.definition.id, out var iapConfig)) return;
                orderInfo = new PaymentOrderInfo(iapConfig.Key, product.definition.id, "restore");
            }
            
            var paymentState = failureDescription.reason == PurchaseFailureReason.UserCancelled ? PaymentState.RequestCancelled : PaymentState.RequestFailed;
            OnPaymentFailed(orderInfo, paymentState, failureDescription.message);
        }
        
        #endregion
        
        #region 刷新商品
        
        /// <summary>
        /// 重新拉取所有商品列表
        /// </summary>
        public void RefreshProductList(int retryCount = 0)
        {
            var productList = new HashSet<ProductDefinition>();
            productList.UnionWith(productIdConfigs.Select(c => new ProductDefinition(c.Key, (ProductType)c.Value.IAP_Type)));
            LogInfo($"RefreshProductList: count: {productList.Count}");
            controller.FetchAdditionalProducts(productList, () =>
            {
                //刷新成功
                LogInfo($"RefreshProductList: success count: {controller.products.all.Length}");
                GameBase.Instance.GetModule<EventManager>().Broadcast(new EventPaymentInitiated());
            }, (reason, message) =>
            {
                //拉取失败，重新尝试三次
                LogError($"RefreshProductList: error: {reason}, message: {message}, retryCount: {retryCount}");
                if (retryCount++ < 3)
                {
                    GameBase.Instance.GetModule<TimingManager>().GlobalDelayCall(retryCount * 10, () => RefreshProductList(retryCount));
                }
            });
        }
        
        #endregion

        #region 日志记录

        //输出日志
        private void LogInfo(string message)
        {
            Debugger.Log(TAG, message);
        }

        //输出错误
        private void LogError(string message)
        {
            Debugger.LogError(TAG, message);
            // GameBase.Instance.GetModule<AnalyticsManager>()?.TrackError(message);
        }
        
        //打点记录支付事件
        private void LogPaymentEvent(PaymentOrderInfo orderInfo, string errorMessage = null)
        {
            if (orderInfo == null || string.IsNullOrEmpty(orderInfo.productKey)) return;
            var price = productIdConfigs.TryGetValue(orderInfo.productId, out var config) ? config.Price : 0;
            var product = controller?.products.WithID(orderInfo.productId);
            var priceDisplay = product?.metadata.localizedPriceString ?? "";
            var analaticsManager = GameBase.Instance.GetModule<AnalyticsManager>();
            
            switch (orderInfo.state)
            {
                case PaymentState.RequestInitiated:
                    analaticsManager.TrackEvent(new AnalyticsEvent_iap_start(orderInfo.productKey, orderInfo.productId, price));
                    return;
                case PaymentState.RequestCancelled:
                    analaticsManager.TrackEvent(new AnalyticsEvent_iap_failure(orderInfo.productKey, orderInfo.productId, price, errorMessage));
                    var canceled = new AnalyticsEvent(AnalyticsThirdPartyConfigKeys.Purchased_Canceled);
                    canceled.eventData.Add("product_id", orderInfo.productId);
                    analaticsManager.TrackThirdPartyEvent(canceled);
                    return;
                case PaymentState.RequestUnavaliable:
                case PaymentState.RequestFailed:
                   analaticsManager.TrackEvent(new AnalyticsEvent_iap_failure(orderInfo.productKey, orderInfo.productId, price, errorMessage));
                    return;
                case PaymentState.PaymentSucceed:
                   analaticsManager.TrackEvent(new AnalyticsEvent_iap_success(orderInfo.productKey,orderInfo.productId, orderInfo.transactionId, price,"normal"));
                   if (paymentRecord.ArchiveData.firstPaymentTime == 0)
                       analaticsManager.TrackThirdPartyEvent(
                           new AnalyticsEvent(AnalyticsThirdPartyConfigKeys.Purchased_First_Time));
                   return;
                case PaymentState.RewardConsumed:
                    var purchased = new AnalyticsRevenueEvent(AnalyticsThirdPartyConfigKeys.Purchased, price);
                    purchased.eventData.Add("iapCount", paymentRecord.ArchiveData.iapCount);
                    analaticsManager.TrackThirdPartyEvent(purchased);
                   // analaticsManager.TrackThirdPartyEvent(new AnalyticsEvent(AnalyticsThirdPartyConfigKeys.PurchasedCount));
                    return;
            }
        }

        #endregion

        #region 注册支付相关用户属性

        private void RegisterPaymentProperty()
        {
            
            var Analytics = GameBase.Instance.GetModule<AnalyticsManager>();
            
            Analytics.RegisterUserProperty(AnalyticsUserPropertiesConfigKeys.user_iap_total_cent,
                () => paymentRecord.ArchiveData.iapTotal, false);
            Analytics.RegisterUserProperty(AnalyticsUserPropertiesConfigKeys.user_iap_times,
                () => paymentRecord.ArchiveData.iapCount, false);
            Analytics.RegisterUserProperty(AnalyticsUserPropertiesConfigKeys.first_iap_time,
                () => TimeUtil.ConvertUnixTimestampToDateTimeString(paymentRecord.ArchiveData.firstPaymentTime), false);
            Analytics.RegisterUserProperty(AnalyticsUserPropertiesConfigKeys.last_iap_time,
                () => TimeUtil.ConvertUnixTimestampToDateTimeString(paymentRecord.ArchiveData.lastPaymentTime), false);
            Analytics.RegisterUserProperty(AnalyticsUserPropertiesConfigKeys.max_iap_revenue,
                () => GetMaxIapRevenue(), false);
            
        }

        #endregion
    }

    public static class PaymentExtension
    {
        /// <summary>
        /// 通过商品配置获取当前平台的商品ID
        /// </summary>
        /// <param name="iapConfig">商品配置</param>
        /// <returns>当前平台的商品ID</returns>
        public static string GetProductId(this IAPConfig iapConfig)
        {
            return AppUtil.IsIOS ? iapConfig.AppleProductId : iapConfig.GoogleProductId;
        }

        /// <summary>
        /// 发奖完成，消耗订单
        /// </summary>
        public static void Consume(this PaymentOrderInfo orderInfo)
        {
            GameBase.Instance.GetModule<PaymentManager>().Consume(orderInfo);
        }
    }
}