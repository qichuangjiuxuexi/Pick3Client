using System;
using System.Collections.Generic;
using API.V1.Game;
using AppBase.Utils;

namespace AppBase.LeaderBoard
{
    /// <summary>
    /// 排行榜条目
    /// </summary>
    public class LeaderBoardItem : IComparable<LeaderBoardItem>
    {
        /// <summary>
        /// 排行榜条目id
        /// </summary>
        public string id;

        /// <summary>
        /// 分数
        /// </summary>
        public double score;

        /// <summary>
        /// 名次
        /// </summary>
        public int rank;

        /// <summary>
        /// 显示名称
        /// </summary>
        public string nickname;

        /// <summary>
        /// 头像
        /// </summary>
        public string avatar;

        /// <summary>
        /// 额外信息
        /// </summary>
        public Dictionary<string, object> extra;

        public LeaderBoardItem()
        {
        }

        public LeaderBoardItem(string id, double score, string nickname, string avatar, Dictionary<string, object> extra = null)
        {
            this.id = id;
            this.score = score;
            this.nickname = nickname;
            this.avatar = avatar;
            this.extra = extra;
        }

        /// <summary>
        /// 设置额外信息
        /// </summary>
        public void SetExtra(string key, object value)
        {
            extra ??= new Dictionary<string, object>();
            extra[key] = value;
        }

        /// <summary>
        /// 获取额外信息
        /// </summary>
        public T GetExtra<T>(string key, T defaultValue = default)
        {
            if (extra != null && extra.TryGetValue(key, out var value))
            {
                try
                {
                    return (T)Convert.ChangeType(value, typeof(T));
                }
                catch
                {
                }
            }
            return defaultValue;
        }

        public int CompareTo(LeaderBoardItem other)
        {
            if (ReferenceEquals(this, other)) return 0;
            if (ReferenceEquals(null, other)) return -1;
            return -score.CompareTo(other.score);
        }
    }

    /// <summary>
    /// 将排行榜条目转换为protobuf
    /// </summary>
    internal static class LeaderBoardItemExtention
    {
        internal static LeaderBoardItem ToItem(this LeaderboardEntry entry)
        {
            var item = new LeaderBoardItem
            {
                id = entry.Id ?? "",
                score = entry.Score,
                rank = entry.Rank,
                nickname = entry.Nickname ?? "",
                avatar = entry.Avatar ?? ""
            };
            if (!string.IsNullOrEmpty(entry.Extra))
            {
                item.extra = JsonUtil.DeserializeObject<Dictionary<string, object>>(entry.Extra) ?? new Dictionary<string, object>();
            }
            return item;
        }
        
        internal static LeaderboardEntry ToProto(this LeaderBoardItem item)
        {
            var entry = new LeaderboardEntry
            {
                Id = item.id,
                Score = item.score
            };
            if (!string.IsNullOrEmpty(item.nickname))
            {
                entry.Nickname = item.nickname;
            }
            if (!string.IsNullOrEmpty(item.avatar))
            {
                entry.Avatar = item.avatar;
            }
            if (item.extra != null && item.extra.Count > 0)
            {
                entry.Extra = JsonUtil.SerializeArchive(item.extra, typeof(Dictionary<string, object>));
            }
            return entry;
        }
    }
}