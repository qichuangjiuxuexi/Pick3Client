using System.Collections.Generic;
using System.Linq;
using API.V1.Game;
using AppBase;
using AppBase.Network;

namespace Protocols
{
    public class UpdateMemberBasicProtocol : ProtobufProtocol<UpdateMemberBasicRequest, UpdateMemberBasicResponse>
    {
        public override string service => "club";
        public override string action => "update-member-basic";
        
        public UpdateMemberBasicProtocol(string teamID,string nickName,string avartar)
        {
            request = new UpdateMemberBasicRequest
            {
                ClubId = teamID,
                Nickname = nickName,
                Avatar = avartar
            };
        } 
        

        public override bool OnSend()
        {
            request.PlayerId = GameBase.Instance.GetModule<NetworkManager>().playerId;
            return base.OnSend();
        }
        
    }
}