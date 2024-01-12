using API.V1.Game;
using AppBase;
using AppBase.Archive;
using AppBase.Network;
using AppBase.PlayerInfo;

namespace Protocol
{
    public class SocialLoginProtocol : ProtobufProtocol<LoginSocialRequest, LoginSocialResponse>
    {
        public override string service => "player";
        public override string action => "login-social";
        
        /// <summary>
        /// 社交账号登录
        /// </summary>
        public SocialLoginProtocol(AccountInfo accountInfo)
        {
            request = new LoginSocialRequest()
            {
                AccountInfo = accountInfo
            };
        }
        
        public override bool OnSend()
        {
            request.PlayerId = GameBase.Instance.GetModule<NetworkManager>().playerId;
            return true;
        }
    }
}