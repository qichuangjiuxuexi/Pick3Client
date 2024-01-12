using System;
using System.Buffers;
using System.Collections.Concurrent;
using System.Text;
using AppBase.Event;
using AppBase.GetOrWait;
using AppBase.Module;
using AppBase.Network.WebSocket;
using AppBase.Timing;
using AppBase.Utils;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace AppBase.Debugging
{
    /// <summary>
    /// 远程调试工具
    /// </summary>
    public class RemoteDebugManager : ModuleBase
    {
        private const string remoteUrl = "wss://ali-slots-res.me2zengame.com:42451";
        private const int logCount = 1000;

        private UnityWebSocket socket;
        private ConcurrentQueue<LogMessage> sendQueue = new();
        private volatile string userId;

        /// <summary>
        /// 是否开启远程调试功能
        /// </summary>
        private bool isEnable
        {
            get => PlayerPrefs.GetInt("RemoteDebugConnect", AppUtil.IsDebug ? 1 : 0) != 0;
            set
            {
                PlayerPrefs.SetInt("RemoteDebugConnect", value ? 1 : 0);
                PlayerPrefs.Save();
                UpdateState();
            }
        }

        /// <summary>
        /// 是否允许发送日志，如果没有人监听，则不发送
        /// </summary>
        private volatile bool isEnableSend;

        protected override void OnInit()
        {
            base.OnInit();
            Debugger.SetLogEnable(TAG);
            UpdateState();
            AddModule<EventModule>().Subscribe<EventOnLoadFinished>(OnUserIdChanged)
                .Subscribe<EventOnGameQuit>(OnGameQuitEvent);
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();
            Disconnect();
        }

        /// <summary>
        /// 开始运行
        /// </summary>
        private void UpdateState()
        {
            Debugger.Log(TAG, $"Start: {isEnable}");
            if (isEnable)
            {
                if (socket != null) return;
                Application.logMessageReceivedThreaded -= OnLogCallback;
                Application.logMessageReceivedThreaded += OnLogCallback;
                GameBase.Instance.GetModule<GetOrWaitManager>().GetOrWait<string>(SRDebugManager.user_id, uid =>
                {
                    userId = uid;
                    Connect();
                });
            }
            else
            {
                Disconnect();
            }
        }

        /// <summary>
        /// 断开连接
        /// </summary>
        private void Disconnect()
        {
            Debugger.Log(TAG, $"Disconnect {socket != null}");
            Application.logMessageReceivedThreaded -= OnLogCallback;
            if (socket != null)
            {
                socket.StopAsync().Forget();
                socket = null;
            }
            isEnableSend = false;
        }
        
        /// <summary>
        /// UserId变化
        /// </summary>
        private void OnUserIdChanged(EventOnLoadFinished evt)
        {
            GameBase.Instance.GetModule<GetOrWaitManager>().GetOrWait<string>(SRDebugManager.user_id, uid =>
            {
                userId = uid;
            });
        }

        private void OnGameQuitEvent(EventOnGameQuit obj)
        {
            Disconnect();
        }
        
        /// <summary>
        /// 记录日志
        /// </summary>
        private void OnLogCallback(string condition, string stackTrace, LogType type)
        {
            var msg = new LogMessage(condition, stackTrace, type);
            InsertAndSendMessage(msg);
        }

        private void InsertAndSendMessage(LogMessage message)
        {
            if (sendQueue.Count >= logCount) sendQueue.TryDequeue(out _);
            sendQueue.Enqueue(message);
            SendNextMessage().Forget();
        }

        /// <summary>
        /// 开始连接
        /// </summary>
        private void Connect()
        {
            Debugger.Log(TAG, $"Connect: {socket != null}");
            if (socket != null) return;
            socket = new UnityWebSocket(remoteUrl);
            socket.OnConnected += OnConnected;
            socket.OnReceived += OnReceiveCommand;
            socket.SetAutomaticReconnect().SetKeepAlive().StartAsync().Forget();
        }

        /// <summary>
        /// 开始发送日志
        /// </summary>
        private async UniTask SendNextMessage()
        {
            if (!isEnableSend) return;
            while (sendQueue.TryDequeue(out var message))
            {
                await SendMessage(message);
            }
        }

        /// <summary>
        /// 发送一条日志
        /// </summary>
        private async UniTask SendMessage(LogMessage message)
        {
            message.pid = userId;
            var json = JsonUtil.SerializeObject(message);
            var buff = new ArraySegment<byte>(Encoding.UTF8.GetBytes(json));
            await socket.SendAsync(buff);
        }
        
        /// <summary>
        /// 连接成功
        /// </summary>
        private async UniTask OnConnected()
        {
            var msg = new LogMessage("conn");
            await SendMessage(msg);
        }
        
        /// <summary>
        /// 接收到指令
        /// </summary>
        private UniTask OnReceiveCommand(ReadOnlySequence<byte> result)
        {
            var json = Encoding.UTF8.GetString(result.ToArray());
            Debugger.Log(TAG, $"OnReceiveCommand: {json}");
            CommandMessage command = default;
            try
            {
                command = JsonUtil.DeserializeObject<CommandMessage>(json);
            }
            catch (Exception ex)
            {
                Debugger.LogWarning(TAG, $"OnReceiveCommand Deserialize failed: {ex}");
            }
            ExecuteCommand(command);
            return UniTask.CompletedTask;
        }

        /// <summary>
        /// 处理指令
        /// </summary>
        private void ExecuteCommand(CommandMessage command)
        {
            switch (command.type)
            {
                //服务器连接成功
                case "server":
                    isEnableSend = true;
                    Debugger.Log(TAG, $"Server connected: {command.cmd}");
                    SendNextMessage().Forget();
                    break;
                //服务器连接断开
                case "disconnect":
                    isEnableSend = false;
                    Debugger.Log(TAG, $"Server disconnected: {command.cmd}");
                    break;
                //服务器指令
                case "cmd":
                    Debugger.Log(TAG, $"OnReceiveCommand: {command.cmd}");
                    RunDebugCommand(command.cmd).Forget();
                    break;
            }
        }

        /// <summary>
        /// 发送指令事件
        /// </summary>
        private async UniTask RunDebugCommand(string cmd)
        {
            await UniTask.SwitchToMainThread();
            //todo
        }

        /// <summary>
        /// 消息对象
        /// </summary>
        private struct LogMessage
        {
            public string app;
            public string pid;
            public string type;
            public string[] args;

            public LogMessage(string type)
            {
                app = "new_arch";
                pid = null;
                this.type = type;
                args = null;
            }

            public LogMessage(string condition, string stackTrace, LogType type)
            {
                app = "new_arch";
                pid = null;
                this.type = type switch
                {
                    LogType.Log => "info",
                    LogType.Warning => "warn",
                    _ => "error"
                };
                args = new[] { condition, stackTrace };
            }
        }
        
        /// <summary>
        /// 指令对象
        /// </summary>
        private struct CommandMessage
        {
            public string type;
            public string cmd;
        }
    }
}