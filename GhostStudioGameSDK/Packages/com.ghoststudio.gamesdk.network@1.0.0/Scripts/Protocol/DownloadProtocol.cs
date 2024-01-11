using API.V1.Game;
using AppBase.Archive;
using AppBase.PlayerInfo;

namespace AppBase.Network
{
    /// <summary>
    /// 下载存档协议
    /// </summary>
    public class DownloadProtocol : ProtobufProtocol<DownloadRequest, DownloadResponse>
    {
        public override string service => "player";
        public override string action => "download";
        
        private NetworkManager networkManager => GameBase.Instance.GetModule<NetworkManager>();
        private ArchiveManager archiveManager => GameBase.Instance.GetModule<ArchiveManager>();
        
        public override bool OnSend()
        {
            if (!networkManager.IsLogin || archiveManager == null) return false;
            request.PlayerId = networkManager.playerId;
            var playerInfo = archiveManager.GetAchiveData<PlayerInfoArchiveData>(nameof(PlayerInfoRecord));
            request.ServerVer = playerInfo?.serverVersion ?? 0;
            request.ClientVer = playerInfo?.levelVersion ?? 0;
            return true;
        }

        public override bool OnResponse()
        {
            if (response.PlayerData?.Data == null || response.PlayerData.Data.Count == 0)
            {
                //客户端存档版本和服务器一致，无需下载
                Debugger.Log(TAG, "No need to download");
                return true;
            }
            return archiveManager.SetArchivesFromJson(response.PlayerData.Data, response.PlayerId, response.PlayerData.ServerVer);
        }
    }
}