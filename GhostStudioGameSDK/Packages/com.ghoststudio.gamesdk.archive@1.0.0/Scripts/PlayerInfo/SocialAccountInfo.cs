using System;

namespace AppBase.PlayerInfo
{
    /// <summary>
    /// 社交账号信息
    /// </summary>
    [Serializable]
    public class SocialAccountInfo
    {
        /// <summary>
        /// 社交账号类型
        /// 1：Facebook
        /// 2：Google
        /// 3：Appleso
        /// </summary>
        public int type;

        /// <summary>
        /// 社交账号ID
        /// </summary>
        public string id;

        /// <summary>
        /// 社交账号名称
        /// </summary>
        public string name;

        /// <summary>
        /// 社交账号Email
        /// </summary>
        public string email;

        /// <summary>
        /// 社交账号头像
        /// </summary>
        public string avatar;
    }
}