using System.Collections.Generic;
using System.Linq;
using API.V1.Game;
using AppBase;
using AppBase.Network;

namespace Protocols
{
    public class LeaveTeamProtocol : ProtobufProtocol<ExitClubRequest, ExitClubResponse>
    {
        public override string service => "club";
        public override string action => "exit-club";
        
        public LeaveTeamProtocol(string teamID)
        {
            request = new ExitClubRequest
            {
                ClubId = teamID,
            };
        } 
        

        public override bool OnSend()
        {
            request.PlayerId = GameBase.Instance.GetModule<NetworkManager>().playerId;
            return base.OnSend();
        }
        
    }
}