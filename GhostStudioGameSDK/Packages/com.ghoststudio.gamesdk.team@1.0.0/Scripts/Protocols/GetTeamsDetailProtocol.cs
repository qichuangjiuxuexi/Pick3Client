using System.Collections.Generic;
using System.Linq;
using API.V1.Game;
using AppBase;
using AppBase.Network;

namespace Protocols
{
    public class GetTeamsDetailProtocol : ProtobufProtocol<GetClubRequest, GetClubResponse>
    {
        public override string service => "club";
        public override string action => "get-club";
        
        public GetTeamsDetailProtocol(string clubID)
        {
            request = new GetClubRequest
            {
                ClubId = clubID,
            };
        } 
        

        public override bool OnSend()
        {
            request.PlayerId = GameBase.Instance.GetModule<NetworkManager>().playerId;
            return base.OnSend();
        }

        public ClubComplete GetClubDetail()
        {
            return response.ClubComplete;
        }
    }
}