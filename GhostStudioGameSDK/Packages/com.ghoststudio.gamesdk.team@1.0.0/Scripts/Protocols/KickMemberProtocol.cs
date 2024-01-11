using System.Collections.Generic;
using System.Linq;
using API.V1.Game;
using AppBase;
using AppBase.Network;

namespace Protocols
{
    public class KickMemberProtocol : ProtobufProtocol<KickMemberRequest, KickMemberResponse>
    {
        public override string service => "club";
        public override string action => "kick-member";
        
        public KickMemberProtocol(string teamID,string targetPlayerID)
        {
            request = new KickMemberRequest
            {
                ClubId = teamID,
                MemberPlayerId = targetPlayerID,
            };
        } 
        

        public override bool OnSend()
        {
            request.PlayerId = GameBase.Instance.GetModule<NetworkManager>().playerId;
            return base.OnSend();
        }
        
    }
}