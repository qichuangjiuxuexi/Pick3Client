using API.V1.Game;

namespace AppBase.Network
{
    /// <summary>
    /// 删档协议
    /// </summary>
    public class DeleteProtocol : ProtobufProtocol<DeleteRequest, DeleteResponse>
    {
        public override string service => "player";
        public override string action => "delete";

        public override bool OnSend()
        {
            request.PlayerId = GameBase.Instance.GetModule<NetworkManager>().playerId;
            return true;
        }
    }
}