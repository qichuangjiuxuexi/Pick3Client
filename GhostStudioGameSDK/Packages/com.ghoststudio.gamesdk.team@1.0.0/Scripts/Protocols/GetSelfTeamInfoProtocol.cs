using System.Collections.Generic;
using System.Linq;
using API.V1.Game;
using AppBase;
using AppBase.Network;

namespace Protocols
{
    public class GetSelfTeamInfoProtocol : ProtobufProtocol<GetMyClubRequest, GetMyClubResponse>
    {
        public override string service => "club";
        public override string action => "get-my-club";
        
        public GetSelfTeamInfoProtocol()
        {
            request = new GetMyClubRequest();
        } 
        
        public override bool OnSend()
        {
            request.PlayerId = GameBase.Instance.GetModule<NetworkManager>().playerId;
            return base.OnSend();
        }
    }
}