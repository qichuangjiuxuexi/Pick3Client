using System;
using API.V1.Game;

namespace AppBase.LeaderBoard
{
    /// <summary>
    /// 排行榜标识，用来标识一个排行榜单
    /// </summary>
    public class LeaderBoardID
    {
        /// <summary>
        /// 排行榜ID
        /// </summary>
        public int featureId;
        
        /// <summary>
        /// 分组ID/区域ID
        /// </summary>
        public string groupId;

        /// <summary>
        /// 赛季ID
        /// </summary>
        public int seasonId;
        
        public LeaderBoardID()
        {
        }
        
        public LeaderBoardID(int featureId, string groupId = null, int seasonId = 0)
        {
            this.featureId = featureId;
            this.groupId = groupId;
            this.seasonId = seasonId;
        }
        
        public LeaderBoardID(int featureId, int seasonId)
        {
            this.featureId = featureId;
            this.seasonId = seasonId;
        }

        /// <summary>
        /// 生成排行榜标识字符串
        /// </summary>
        public override string ToString()
        {
            return $"{featureId}:{(string.IsNullOrEmpty(groupId) ? "1" : groupId)}:{seasonId}";
        }
        
        public override bool Equals(object obj)
        {
            if (obj is LeaderBoardID id)
            {
                return featureId == id.featureId && groupId == id.groupId && seasonId == id.seasonId;
            }
            return false;
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(featureId, groupId, seasonId);
        }
    }

    /// <summary>
    /// 将排行榜标识转换为protobuf
    /// </summary>
    internal static class LeaderBoardIDExtention
    {
        internal static LeaderboardIdentifier ToProto(this LeaderBoardID id)
        {
            return new LeaderboardIdentifier
            {
                FeatureId = id.featureId.ToString(),
                GroupId = id.ToString()
            };
        }
    }
}
