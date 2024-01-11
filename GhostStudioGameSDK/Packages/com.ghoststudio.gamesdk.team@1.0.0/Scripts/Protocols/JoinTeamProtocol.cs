using System.Collections.Generic;
using System.Linq;
using API.V1.Game;
using AppBase;
using AppBase.Network;

namespace Protocols
{
    public class JoinTeamProtocol : ProtobufProtocol<JoinClubRequest, JoinClubResponse>
    {
        public override string service => "club";
        public override string action => "join-club";
        
        public JoinTeamProtocol(string teamID, ClubMember selfInfo,int level)
        {
            request = new JoinClubRequest
            {
                ClubId = teamID,
                ClubMember = selfInfo,
                Level = level
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