using System.Collections.Generic;
using System.Linq;
using API.V1.Game;
using AppBase;
using AppBase.Network;

namespace Protocols
{
    public class GetTeamsProtocol : ProtobufProtocol<GetClubListRequest, GetClubListResponse>
    {
        public override string service => "club";
        public override string action => "get-club-list";
        
        public GetTeamsProtocol(int start,int count,int myLevel)
        {
            request = new GetClubListRequest
            {
                Start = start,
                Count = count,
                Level = myLevel
            };
        } 
        
        public GetTeamsProtocol(int start,int count,int myLevel,string searchPattern)
        {
            request = new GetClubListRequest
            {
                Start = start,
                Count = count,
                Name = searchPattern,
                Level = myLevel
            };
        }

        public override bool OnSend()
        {
            request.PlayerId = GameBase.Instance.GetModule<NetworkManager>().playerId;
            return base.OnSend();
        }
        
        public List<ClubLite> GetTeamsItems()
        {
            if (response?.ClubLiteList == null) return new();
            return response.ClubLiteList.ToList();
        } 
    }
}