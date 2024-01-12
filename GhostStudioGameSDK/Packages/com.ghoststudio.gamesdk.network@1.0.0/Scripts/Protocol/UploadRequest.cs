using System.Collections.Generic;
using AppBase.Archive;
using AppBase.PlayerInfo;
using AppBase.Utils;
using API.V1.Game;
using AppBase.Event;
using AppBase.Timing;

namespace AppBase.Network
{
    /// <summary>
    /// 上传存档协议
    /// </summary>
    public class UploadProtocol : ProtobufProtocol<UploadRequest, UploadResponse>
    {
        public override string service => "player";
        public override string action => "upload";
        private ArchiveManager archiveManager => GameBase.Instance.GetModule<ArchiveManager>();
        private PlayerInfoArchiveData playerInfo => archiveManager.GetAchiveData<PlayerInfoArchiveData>(nameof(PlayerInfoRecord));
        
        //保存上传的时间戳
        private Dictionary<string, long> uploadArchives = new();

        public override bool OnSend()
        {
            if (archiveManager?.ArchiveDict == null || archiveManager.ArchiveDict.Count == 0 || string.IsNullOrEmpty(playerInfo?.userId)) return false;
            archiveManager.SaveAllDirty();
            request.PlayerId = playerInfo.userId;
            request.PlayerData = new PlayerData
            {
                ServerVer = playerInfo.serverVersion,
                ClientVer = playerInfo.levelVersion,
                Data = {  }
            };
            
            //检查是否有存档需要上传
            uploadArchives.Clear();
            foreach (var archive in archiveManager.ArchiveDict)
            {
                if (archive.Value.lastUploadTime != archive.Value.lastSaveTime || playerInfo.serverVersion <= 0)
                {
                    var json = JsonUtil.SerializeArchive(archive.Value, typeof(BaseArchiveData));
                    if (!string.IsNullOrEmpty(json))
                    {
                        uploadArchives.Add(archive.Key, archive.Value.lastSaveTime);
                        request.PlayerData.Data.Add(archive.Key, json);
                    }
                }
            }
            if (uploadArchives.Count == 0)
            {
                //没有存档需要上传
                Debugger.Log(TAG, "No archive need to upload");
                return false;
            }
            uploadArchives.TryAdd(nameof(PlayerInfoRecord), playerInfo.lastSaveTime);
            return true;
        }

        public override bool OnResponse()
        {
            playerInfo.serverVersion = response.ServerVer;
            foreach (var uploadTime in uploadArchives)
            {
                var archive = archiveManager.GetAchiveData<BaseArchiveData>(uploadTime.Key);
                if (archive != null)
                {
                    archive.lastUploadTime = uploadTime.Value;
                    archiveManager.SaveArchiveData(uploadTime.Key, archive);
                }
            }
            return true;
        }

        public override void OnFail()
        {
            GameBase.Instance.GetModule<EventManager>().Broadcast(new UploadRequestFailedEvent(errorCode));
        }
    }
}