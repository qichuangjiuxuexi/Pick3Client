using System;
using System.Linq;
using AppBase.PlayerInfo;
using AppBase.Utils;
using API.V1.Game;
using AppBase.Analytics;
using AppBase.Archive;
using AppBase.Event;
using AppBase.Timing;

namespace AppBase.Network
{
    /// <summary>
    /// 登录协议
    /// </summary>
    public class LoginProtocol : ProtobufProtocol<LoginRequest, LoginResponse>
    {
        public override string service => "player";
        public override string action => "login";
        
        private ArchiveManager archiveManager => GameBase.Instance.GetModule<ArchiveManager>();
        private PlayerInfoArchiveData playerInfo => archiveManager?.GetAchiveData<PlayerInfoArchiveData>(nameof(PlayerInfoRecord));

        public override void OnBeforeSend(Action<bool> callback)
        {
            callback?.Invoke(true);
        }

        public override bool OnSend()
        {
            //通过社交账号登录
            var socialInfo = playerInfo?.socialAccounts?.FirstOrDefault();
            if (socialInfo == null)
            {
                request.AccountInfo = new LoginRequest.Types.AccountInfo
                {
                    Type = LoginRequest.Types.AccountInfo.Types.AccountType.AccountGuest,
                    Id = AppUtil.DeviceId,
                    Name = ""
                };
            }
            else
            {
                request.AccountInfo = new LoginRequest.Types.AccountInfo
                {
                    Type = (LoginRequest.Types.AccountInfo.Types.AccountType)socialInfo.type,
                    Id = socialInfo.id,
                    Name = socialInfo.name,
                };
            }
            
            request.DeviceId = AppUtil.DeviceId;
            request.PlatformType = AppUtil.IsIOS ? LoginRequest.Types.PlatformType.PlatformIos : LoginRequest.Types.PlatformType.PlatformAndroid;
            request.ClientVersion = AppUtil.ClientVersion;
            
            //拉取存档
            request.DownloadReq = new DownloadRequest
            {
                PlayerId = playerInfo?.userId ?? "",
                ClientVer = playerInfo?.levelVersion ?? 0,
                ServerVer = playerInfo?.serverVersion ?? 0,
            };
            return true;
        }

        public override bool OnResponse()
        {
            var manager = GameBase.Instance.GetModule<NetworkManager>();
            manager.token = response.Token;
            manager.playerId = response.PlayerId;
            
            //设置服务器时间
            var timingManager = GameBase.Instance.GetModule<TimingManager>();
            if (timingManager != null && response.ServerTime > 0)
            {
                timingManager.ServerTime = DateTimeOffset.FromUnixTimeMilliseconds(response.ServerTime).DateTime;
            }
            
            //拉取存档
            if (response.PlayerData?.Data?.Count > 0 &&
                (playerInfo == null || response.PlayerData.ServerVer > playerInfo.serverVersion || response.PlayerData.ClientVer > playerInfo.levelVersion))
            {
                archiveManager.SetArchivesFromJson(response.PlayerData.Data, response.PlayerId, response.PlayerData.ServerVer);
            }

            //设置用户ID
            archiveManager.UserId = response.PlayerId;
            return true;
        }
    }
}
