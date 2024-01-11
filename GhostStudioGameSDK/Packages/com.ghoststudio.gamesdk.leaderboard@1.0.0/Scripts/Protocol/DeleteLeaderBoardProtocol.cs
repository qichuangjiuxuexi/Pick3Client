using System.Collections.Generic;
using System.Linq;
using API.V1.Game;
using AppBase.Network;

namespace AppBase.LeaderBoard
{
    /// <summary>
    /// 删除排行榜协议
    /// </summary>
    public class DeleteLeaderBoardProtocol : ProtobufProtocol<DeleteEntryRequest, DeleteResponse>
    {
        public override string service => "leaderboard";
        public override string action => "delete-entry";

        /// <summary>
        /// 批量删除排行榜
        /// </summary>
        /// <param name="ids">删除的排行榜列表</param>
        public DeleteLeaderBoardProtocol(IList<LeaderBoardID> ids)
        {
            request = new DeleteEntryRequest
            {
                IdentifierList = { ids.Select(id => id.ToProto()) }
            };
        }

        /// <summary>
        /// 单独删除排行榜
        /// </summary>
        /// <param name="entries">删除的排行榜列表</param>
        public DeleteLeaderBoardProtocol(LeaderBoardID id)
        {
            request = new DeleteEntryRequest
            {
                IdentifierList = { id.ToProto() }
            };
        }
        
        public override bool OnSend()
        {
            request.PlayerId = GameBase.Instance.GetModule<NetworkManager>().playerId;
            return true;
        }
    }
}