using System.Collections.Generic;
using System.Linq;
using API.V1.Game;
using AppBase;
using AppBase.Network;

namespace Protocols
{
    public class UpdateTeamBasicProtocol : ProtobufProtocol<UpdateClubBasicRequest, UpdateClubBasicResponse>
    {
        public override string service => "club";
        public override string action => "update-club-basic";
        
        public UpdateTeamBasicProtocol(string teamID,ClubBasic basicInfo)
        {
            request = new UpdateClubBasicRequest
            {
                ClubId = teamID,
                ClubBasic = basicInfo
            };
        } 
        

        public override bool OnSend()
        {
            request.PlayerId = GameBase.Instance.GetModule<NetworkManager>().playerId;
            return base.OnSend();
        }
        
    }
}