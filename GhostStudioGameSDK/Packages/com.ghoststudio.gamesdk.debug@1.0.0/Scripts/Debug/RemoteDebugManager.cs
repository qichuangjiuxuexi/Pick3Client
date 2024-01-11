using System;
using System.Collections.Generic;
using System.Net.WebSockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using AppBase.Module;
using AppBase.Timing;
using AppBase.Utils;
using UnityEngine;

namespace AppBase.Debugging
{
    /// <summary>
    /// 远程调试工具
    /// </summary>
    public class RemoteDebugManager : ModuleBase
    {
        private const string remoteUrl = "ws://39.106.57.54:42451";
        private const int logCount = 1000;
        
        private ClientWebSocket socket;
        private Queue<LogMessage> sendQueue = new ();
        private ArraySegment<byte> receiveBuff = new (new byte[1024]);
        private string UserID;
        private bool isSending;
        private bool isConnected => isEnableConnect && socket is { State: WebSocketState.Open or WebSocketState.Connecting };
        private bool isEnableConnect;
        private bool isEnableSend;
        
        protected override void OnInit()
        {
            base.OnInit();
            UserID = AppUtil.DeviceId;
            isEnableConnect = PlayerPrefs.GetInt("RemoteDebugConnect", AppUtil.IsDebug ? 1 : 0) != 0;
            if (isEnableConnect)
            {
                Application.logMessageReceivedThreaded -= OnLogCallback;
                Application.logMessageReceivedThreaded += OnLogCallback;
                Connect();
            }
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();
            Application.logMessageReceivedThreaded -= OnLogCallback;
            isEnableConnect = false;
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
            lock (sendQueue)
            {
                if (sendQueue.Count >= logCount) sendQueue.Dequeue();
                sendQueue.Enqueue(message);
            }
            SendNextMessage();
        }

        /// <summary>
        /// 开始连接
        /// </summary>
        private void Connect()
        {
            if (!isEnableConnect || isConnected) return;
            try
            {
                socket = new ClientWebSocket();
                socket.ConnectAsync(new Uri(remoteUrl), CancellationToken.None).ContinueWith(OnConnected);
            }
            catch (Exception ex)
            {
                Debugger.LogWarning(TAG, $"Connect Exception: {ex}");
                GameBase.Instance.GetModule<TimingManager>().GlobalDelayCall(10, Connect);
            }
        }

        /// <summary>
        /// 连接成功
        /// </summary>
        private void OnConnected(Task task)
        {
            if (!task.IsCompletedSuccessfully)
            {
                Debugger.LogWarning(TAG, $"Connect failed: {task.Exception}");
                GameBase.Instance.GetModule<TimingManager>().GlobalDelayCall(10, Connect);
                return;
            }
            Debugger.Log(TAG, "Connected");
            ReceiveNextCommand();
            //客户端连接成功
            var msg = new LogMessage("conn");
            SendMessage(msg);
        }

        /// <summary>
        /// 开始发送日志
        /// </summary>
        private void SendNextMessage()
        {
            lock (this) if (!isEnableSend || isSending) return;
            if (!isConnected) return;
            LogMessage message;
            lock (sendQueue)
            {
                if (sendQueue.Count == 0) return;
                message = sendQueue.Dequeue();
            }
            SendMessage(message, SendNextMessage);
        }

        /// <summary>
        /// 发送一条日志
        /// </summary>
        private void SendMessage(LogMessage message, Action callback = null)
        {
            lock (this) isSending = true;
            message.pid = UserID;
            var json = JsonUtil.SerializeObject(message);
            var buff = new ArraySegment<byte>(Encoding.UTF8.GetBytes(json));
            try
            {
                socket.SendAsync(buff, WebSocketMessageType.Text, true, CancellationToken.None).ContinueWith(task =>
                {
                    lock (this) isSending = false;
                    callback?.Invoke();
                });
            }
            catch (Exception ex)
            {
                Debugger.LogWarning(TAG, $"Send failed: {ex}");
                if (callback != null)
                {
                    GameBase.Instance.GetModule<TimingManager>().GlobalDelayCall(10, callback);
                }
            }
        }
        
        /// <summary>
        /// 接收指令
        /// </summary>
        private void ReceiveNextCommand()
        {
            if (!isConnected) return;
            try
            {
                socket.ReceiveAsync(receiveBuff, CancellationToken.None).ContinueWith(OnReceiveCommand);
            }
            catch (Exception ex)
            {
                Debugger.LogWarning(TAG, $"Receive failed: {ex}");
                GameBase.Instance.GetModule<TimingManager>().GlobalDelayCall(10, ReceiveNextCommand);
            }
        }
        
        /// <summary>
        /// 接收到指令
        /// </summary>
        private void OnReceiveCommand(Task<WebSocketReceiveResult> task)
        {
            var result = task.Result;
            if (task.IsCanceled || !task.IsCompletedSuccessfully || result?.MessageType == WebSocketMessageType.Close)
            {
                Debugger.LogWarning(TAG, $"Disconnected from server {task.Exception}");
                GameBase.Instance.GetModule<TimingManager>().GlobalDelayCall(10, ReceiveNextCommand);
                return;
            }
            if (result == null || result.Count <= 0)
            {
                ReceiveNextCommand();
                return;
            }
            var json = Encoding.UTF8.GetString(receiveBuff.Array!, receiveBuff.Offset, result.Count);
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
            ReceiveNextCommand();
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
                    lock (this) isEnableSend = true;
                    Debugger.Log(TAG, $"Server connected: {command.cmd}");
                    SendNextMessage();
                    break;
                //服务器连接断开
                case "disconnect":
                    lock (this) isEnableSend = true;
                    Debugger.Log(TAG, $"Server disconnected: {command.cmd}");
                    break;
                //服务器指令
                case "cmd":
                    Debugger.Log(TAG, $"OnReceiveCommand: {command.cmd}");
                    RunDebugCommand(command.cmd);
                    break;
            }
        }

        /// <summary>
        /// 发送指令事件
        /// </summary>
        private void RunDebugCommand(string cmd)
        {
            // await UniTask.SwitchToMainThread();
            // if (cmd.Contains('('))
            // {
            //     var paramStr = cmd.Substring(cmd.IndexOf('(') + 1);
            //     paramStr = "[" + paramStr.Substring(0, paramStr.LastIndexOf(')')) + "]";
            //     var param = ToolJson.DeserializeObject<object[]>(paramStr);
            //     cmd = cmd.Substring(0, cmd.IndexOf('('));
            //     var obj = ReflectionUtil.InvokeMethodByPath(cmd.Trim(), param);
            //     var json = ToolJson.SerializeUnityObject(obj);
            //     InsertAndSendMessage(new LogMessage(json, "", LogType.Log));
            // }
            // else
            // {
            //     var obj = ReflectionUtil.GetValueByPath(cmd.Trim());
            //     var json = ToolJson.SerializeUnityObject(obj);
            //     InsertAndSendMessage(new LogMessage(json, "", LogType.Log));
            // }
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