using System;
using System.Collections.Generic;
using AppBase.Archive;
using UnityEngine.Serialization;

namespace AppBase.PlayerInfo
{
    /// <summary>
    /// 与业务无关的用户信息
    /// </summary>
    [Serializable]
    public class PlayerInfoArchiveData : BaseArchiveData
    {
        /// <summary>
        /// VisitorID，客户端创建存档时生成的GUID
        /// </summary>
        public string visitorId;
        
        /// <summary>
        /// 创建该visitorId时的本地utc时间
        /// </summary>
        public long vidCreateUtcTime;
        
        /// <summary>
        /// 用户ID，当成功登录时，由服务器返回，没有登录时为空
        /// </summary>
        public string userId;

        /// <summary>
        /// 首次拿到userId（视为userID的创建时间）
        /// </summary>
        public long uidCreateUtcTime;

        /// <summary>
        /// 服务器存档版本号
        /// </summary>
        public long serverVersion;

        /// <summary>
        /// 客户端存档版本号，由游戏逻辑控制，可存经验值等自增的数据
        /// </summary>
        public long levelVersion;

        /// <summary>
        /// 绑定的社交账号列表
        /// </summary>
        public List<SocialAccountInfo> socialAccounts = new();
        
        /// <summary>
        /// 设备ID
        /// </summary>
        public string deviceId;
        
        /// <summary>
        /// 设备型号
        /// </summary>
        public string deviceName;


        /// <summary>
        /// 当前客户端版本号
        /// </summary>
        public string clientVer;
        
        /// <summary>
        /// 安装时客户端版本号
        /// </summary>
        public string installVer;
        
        /// <summary>
        /// 注册时间，本地UTC时间，毫秒
        /// </summary>
        public long installUtcTime;

        /// <summary>
        /// 游戏最大时间，毫秒，表示玩家从注册开始到现在的最大时间
        /// </summary>
        public long gameTime;

        /// <summary>
        /// 最后启动时间，本地UTC时间，毫秒
        /// </summary>
        public long appLaunchUtcTime;

        /// <summary>
        /// 每日登录时间，本地UTC时间，毫秒
        /// </summary>
        public long dlyOpenUtcTime;

        /// <summary>
        /// 每日登录时间，本地时区时间，毫秒
        /// </summary>
        public long dlyOpenLocalTime;

        /// <summary>
        /// 打开过游戏的天数，UTC时间
        /// </summary>
        public int playUtcDay;
        
        /// <summary>
        /// 连续登录天数，UTC时间
        /// </summary>
        public int continuePlayUtcDay;

        /// <summary>
        /// 打开过游戏的天数，本地时间
        /// </summary>
        public int playLocalDay;
        
        /// <summary>
        /// 连续登录天数，本地时间
        /// </summary>
        public int continuePlayLocalDay;

        /// <summary>
        /// 与业务有关的其他用户信息
        /// </summary>
        public Dictionary<string, object> playerInfo = new();
    }
}
