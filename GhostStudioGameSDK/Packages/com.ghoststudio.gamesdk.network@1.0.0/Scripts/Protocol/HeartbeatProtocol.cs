using System;
using API.V1.Game;
using AppBase.Timing;

namespace AppBase.Network
{
    /// <summary>
    /// 心跳协议，用于同步服务器时间
    /// </summary>
    public class HeartbeatProtocol : ProtobufProtocol<HeartbeatRequest, HeartbeatResponse>
    {
        public override string service => "player";
        public override string action => "heartbeat";
        public override int timeout => 5;

        public override bool OnSend()
        {
            request.PlayerId = GameBase.Instance.GetModule<NetworkManager>().playerId;
            return true;
        }

        public override bool OnResponse()
        {
            //设置服务器时间
            var timingManager = GameBase.Instance.GetModule<TimingManager>();
            if (timingManager != null && response.ServerTime > 0)
            {
                timingManager.ServerTime = DateTimeOffset.FromUnixTimeMilliseconds(response.ServerTime).DateTime;
            }
            return true;
        }
    }
}
