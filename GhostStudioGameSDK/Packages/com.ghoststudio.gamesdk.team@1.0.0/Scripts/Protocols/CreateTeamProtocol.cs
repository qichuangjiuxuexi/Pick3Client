using System.Collections.Generic;
using System.Linq;
using API.V1.Game;
using AppBase;
using AppBase.Network;

namespace Protocols
{
    public class CreateTeamProtocol : ProtobufProtocol<CreateClubRequest, CreateClubResponse>
    {
        public override string service => "club";
        public override string action => "create-club";
        
        public CreateTeamProtocol(ClubBasic baseInfo,ClubMember leaderInfo)
        {
            request = new CreateClubRequest
            {
                ClubBasic = baseInfo,
                ClubMember = leaderInfo
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