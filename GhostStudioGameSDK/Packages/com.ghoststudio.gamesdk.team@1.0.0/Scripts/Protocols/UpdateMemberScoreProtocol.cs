using System.Collections.Generic;
using System.Linq;
using API.V1.Game;
using AppBase;
using AppBase.Network;

namespace Protocols
{
    public class UpdateMemberScoreProtocol : ProtobufProtocol<UpdateMemberScoreRequest, UpdateMemberScoreResponse>
    {
        public override string service => "club";
        public override string action => "update-member-score";
        
        public UpdateMemberScoreProtocol(string teamID,double score,string scoreKeyName)
        {
            request = new UpdateMemberScoreRequest
            {
                ClubId = teamID,
                Score = score,
                ScoreKeyName = scoreKeyName
            };
        } 
        

        public override bool OnSend()
        {
            request.PlayerId = GameBase.Instance.GetModule<NetworkManager>().playerId;
            return base.OnSend();
        }
        
    }
}