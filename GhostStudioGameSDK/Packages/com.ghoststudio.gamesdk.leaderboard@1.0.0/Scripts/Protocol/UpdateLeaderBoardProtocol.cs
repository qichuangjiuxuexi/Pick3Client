using System.Collections.Generic;
using System.Linq;
using API.V1.Game;
using AppBase.Network;

namespace AppBase.LeaderBoard
{
    /// <summary>
    /// 更新排行榜协议
    /// </summary>
    public class UpdateLeaderBoardProtocol : ProtobufProtocol<UpdateEntryRequest, UpdateEntryResponse>
    {
        public override string service => "leaderboard";
        public override string action => "update-entry";
        
        /// <summary>
        /// 批量更新排行榜
        /// </summary>
        /// <param name="entries">更新的排行榜列表</param>
        public UpdateLeaderBoardProtocol(IDictionary<LeaderBoardID, LeaderBoardItem> entries)
        {
            request = new UpdateEntryRequest
            {
                IdentifierList = { },
                EntryList = { }
            };
            foreach (var entry in entries)
            {
                request.IdentifierList.Add(entry.Key.ToProto());
                request.EntryList.Add(entry.Value.ToProto());
            }
        }
        
        /// <summary>
        /// 单独更新排行榜
        /// </summary>
        /// <param name="id">更新的排行榜ID</param>
        /// <param name="item">更新的排行榜数据</param>
        public UpdateLeaderBoardProtocol(LeaderBoardID id, LeaderBoardItem item)
        {
            request = new UpdateEntryRequest
            {
                IdentifierList = { id.ToProto() },
                EntryList = { item.ToProto() }
            };
        }

        public override bool OnSend()
        {
            request.PlayerId = GameBase.Instance.GetModule<NetworkManager>().playerId;
            return true;
        }
        
        public List<int> GetUpdatedRanks()
        {
            if (response?.RankList == null) return new();
            return response.RankList.ToList();
        }
    }
}