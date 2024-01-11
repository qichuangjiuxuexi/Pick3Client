using System;
using System.Collections;
using API.V1.Game;
using AppBase.Config;
using AppBase.Event;
using AppBase.Module;
using AppBase.Timing;
using AppBase.Utils;
using UnityEngine;
using UnityEngine.Networking;

namespace AppBase.Network
{
    /// <summary>
    /// 网络管理器
    /// </summary>
    public partial class NetworkManager : ModuleBase
    {
        /// <summary>
        /// 服务器地址
        /// </summary>
        private string url;
        
        /// <summary>
        /// 客户端APP ID
        /// </summary>
        private string appId;
        
        /// <summary>
        /// 协议版本号
        /// </summary>
        private const string version = "v1";
        
        /// <summary>
        /// 认证信息
        /// </summary>
        internal string token;
        
        /// <summary>
        /// 当前登录的用户id
        /// </summary>
        public string playerId;

        /// <summary>
        /// 是否已登录
        /// </summary>
        public bool IsLogin => !string.IsNullOrEmpty(token) && !string.IsNullOrEmpty(playerId);

        protected override void OnInit()
        {
            base.OnInit();
            Debugger.SetLogEnable(TAG);
            var config = GameBase.Instance.GetModule<ConfigManager>().GetConfigMap<string, GlobalConfig>(AAConst.GlobalConfig);
            if (config.TryGetValue(AppUtil.IsDebug ? GlobalConfigKeys.ServerUrl_Debug : GlobalConfigKeys.ServerUrl, out var urlConfig))
                url = urlConfig.Value;
            if (config.TryGetValue(GlobalConfigKeys.ServerAppId, out var appIdConfig))
                appId = appIdConfig.Value;
            if (config.TryGetValue(GlobalConfigKeys.ServerUploadCD, out var uploadCdConfig))
                uploadCdTime = ParseUtil.ParseFloat(uploadCdConfig.Value, 600);
            AddModule<EventModule>().Subscribe<EventOnLoadFinished>(OnLoadFinished)
                .Subscribe<EventOnGameFocus>(OnGameFocus);
        }

        /// <summary>
        /// 发送网络请求
        /// </summary>
        /// <param name="request">请求协议</param>
        /// <param name="callback">结果回调</param>
        public void Send<T>(T request, Action<bool> callback = null) where T : NetworkProtocol
            => Send(request, (e, r) => callback?.Invoke(e));

        /// <summary>
        /// 发送网络请求
        /// </summary>
        /// <param name="request">请求协议</param>
        /// <param name="callback">结果回调</param>
        public void Send<T>(T request, Action<bool, T> callback) where T : NetworkProtocol
            => Send((NetworkProtocol)request, (e, r) => callback?.Invoke(e, (T)r));

        /// <summary>
        /// 发送网络请求
        /// </summary>
        /// <param name="request">请求协议</param>
        /// <param name="callback">结果回调</param>
        public void Send(NetworkProtocol request, Action<bool, NetworkProtocol> callback)
        {
            //检查请求合法性
            if (request == null)
            {
                HandleFail(ErrorReason.Failed, "Request is null", null, callback);
                return;
            }
            if (string.IsNullOrEmpty(url) || string.IsNullOrEmpty(appId))
            {
                HandleFail(ErrorReason.Failed, "Config url or appId is empty", request, callback);
                return;
            }
            if (string.IsNullOrEmpty(request.service) || string.IsNullOrEmpty(request.action) || string.IsNullOrEmpty(request.contentType))
            {
                HandleFail(ErrorReason.Failed, "Service or Action or ContentType is null", request, callback);
                return;
            }
            if (Application.internetReachability == NetworkReachability.NotReachable)
            {
                HandleFail(ErrorReason.Failed, "Network Not Reachable", request, callback);
                return;
            }
            //检查登录状态
            request.OnBeforeSend(loginSuccess =>
            {
                //检查登录状态
                if (!loginSuccess)
                {
                    HandleFail(ErrorReason.Failed, null, request, callback);
                    return;
                }
                //生成请求
                if (!request.OnSend())
                {
                    HandleFail(ErrorReason.Failed, null, request, callback);
                    return;
                }
                //准备请求数据
                var requestBytes = request.requestBytes;
                if (requestBytes == null)
                {
                    HandleFail(ErrorReason.Failed, "RequestBytes is null", request, callback);
                    return;
                }
                //发送请求
                request.OnInternalSend();
                GameBase.Instance.GetModule<TimingManager>().StartCoroutine(HandleSend(requestBytes, request, callback));
            });
        }

        /// <summary>
        /// 发送请求数据
        /// </summary>
        private IEnumerator HandleSend(byte[] requestBytes, NetworkProtocol request, Action<bool, NetworkProtocol> callback)
        {
            //准备请求头部
            using var webRequest = new UnityWebRequest($"{url}/{version}/{request.service}/{request.action}", "POST");
            webRequest.SetRequestHeader("Content-Type", request.contentType);
            webRequest.SetRequestHeader("App-ID", appId);
            var reqId = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds().ToString();
            webRequest.SetRequestHeader("Req-ID", reqId);
            if (IsLogin && (request.service != "player" || request.action != "login"))
            {
                webRequest.SetRequestHeader("Authorization", token);
            }
            
            //发送请求
            webRequest.timeout = request.timeout;
            webRequest.uploadHandler = new UploadHandlerRaw(requestBytes);
            webRequest.downloadHandler = new DownloadHandlerBuffer();
            yield return webRequest.SendWebRequest();
            
            //检查请求结果
            if (webRequest.result != UnityWebRequest.Result.Success)
            {
                HandleFail(ErrorReason.Failed, $"ResponseError: {webRequest.error}", request, callback);
                yield break;
            }
            if (webRequest.responseCode != 200)
            {
                HandleFail(ErrorReason.Failed, $"ResponseError: HttpCode = {webRequest.responseCode}", request, callback);
                yield break;
            }
            if (int.TryParse(webRequest.GetResponseHeader("Code"), out var errorCode) && errorCode != 0)
            {
                var errorReason = (ErrorReason) errorCode;
                HandleFail(errorReason, $"ResponseError: ErrorCode = {errorCode}, ErrorReason = {errorReason}", request, callback);
                yield break;
            }
            if (webRequest.GetResponseHeader("Content-Type") != request.contentType)
            {
                HandleFail(ErrorReason.Failed, $"ResponseError: Content-Type != {request.contentType}", request, callback);
                yield break;
            }
            var resId = webRequest.GetResponseHeader("Req-ID");
            if (resId != reqId)
            {
                HandleFail(ErrorReason.Failed, $"ResponseError: Req-Id {resId} != {reqId}", request, callback);
                yield break;
            }
            
            //解析处理结果
            byte[] responseData = webRequest.downloadHandler.data;
            HandleResponse(responseData, request, callback);
        }
        
        /// <summary>
        /// 处理请求成功
        /// </summary>
        private void HandleResponse(byte[] responseBytes, NetworkProtocol request, Action<bool, NetworkProtocol> callback)
        {
            if (responseBytes == null)
            {
                HandleFail(ErrorReason.Failed, "ResponseBytes is empty", request, callback);
                return;
            }
            try
            {
                request.responseBytes = responseBytes;
            }
            catch (Exception e)
            {
                HandleFail(ErrorReason.Failed, $"Parse ResponseBytes Error: {e.Message}", request, callback);
                return;
            }
            if (request.OnInternalResponse() && request.OnResponse())
            {
                request.errorCode = ErrorReason.Success;
                callback?.Invoke(true, request);
            }
            else
            {
                request.errorCode = ErrorReason.Failed;
                callback?.Invoke(false, request);
            }
        }

        /// <summary>
        /// 处理请求失败
        /// </summary>
        private void HandleFail(ErrorReason errorCode, string errorMsg, NetworkProtocol request, Action<bool, NetworkProtocol> callback)
        {
            if (errorMsg != null)
            {
                Debugger.LogError(TAG, errorMsg);
            }
            if (request != null)
            {
                request.errorCode = errorCode;
                request.OnFail();
            }
            callback?.Invoke(false, request);
        }
    }
}