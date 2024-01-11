using System.Collections.Generic;
using System.Linq;
using API.V1.Game;
using AppBase;
using AppBase.Network;

namespace Protocols
{
    public class UpdateMemberRoleProtocol : ProtobufProtocol<UpdateMemberRoleRequest, UpdateMemberRoleResponse>
    {
        public override string service => "club";
        public override string action => "update-member-role";
        
        public UpdateMemberRoleProtocol(string teamID,string memberPlayerID,int role)
        {
            request = new UpdateMemberRoleRequest
            {
                ClubId = teamID,
                MemberPlayerId = memberPlayerID,
                Role = role
            };
        } 
        

        public override bool OnSend()
        {
            request.PlayerId = GameBase.Instance.GetModule<NetworkManager>().playerId;
            return base.OnSend();
        }
        
    }
}