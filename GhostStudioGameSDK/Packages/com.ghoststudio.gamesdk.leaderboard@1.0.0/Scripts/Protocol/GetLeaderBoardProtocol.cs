using System.Collections.Generic;
using System.Linq;
using API.V1.Game;
using AppBase.Network;

namespace AppBase.LeaderBoard
{
    /// <summary>
    /// 获取排行榜协议
    /// </summary>
    public class GetLeaderBoardProtocol : ProtobufProtocol<GetEntriesRequest, GetEntriesResponse>
    {
        public override string service => "leaderboard";
        public override string action => "get-entries";
        
        /// <summary>
        /// 获取排行榜
        /// </summary>
        /// <param name="id">排行榜ID</param>
        /// <param name="start">分页开始位置</param>
        /// <param name="count">分页数量</param>
        public GetLeaderBoardProtocol(LeaderBoardID id, int start = 0, int count = 500)
        {
            request = new GetEntriesRequest
            {
                Identifier = id.ToProto(),
                Start = start,
                Count = count
            };
        }

        public override bool OnSend()
        {
            request.PlayerId = GameBase.Instance.GetModule<NetworkManager>().playerId;
            return true;
        }
        
        public List<LeaderBoardItem> GetItems()
        {
            if (response?.Entries == null) return new();
            return response.Entries.Select(entry => entry.ToItem()).ToList();
        }

        public LeaderBoardItem GetSelfItem()
        {
            if (response != null && response.SelfEntry != null)
            {
                return response.SelfEntry.ToItem();
            }

            return default;
        }
    }
}