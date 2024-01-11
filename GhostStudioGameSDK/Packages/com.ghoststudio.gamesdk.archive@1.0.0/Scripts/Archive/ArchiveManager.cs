using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using AppBase.Analytics;
using AppBase.Debugging;
using AppBase.Event;
using AppBase.GetOrWait;
using AppBase.Module;
using AppBase.PlayerInfo;
using AppBase.Timing;
using AppBase.Utils;

namespace AppBase.Archive
{
    /// <summary>
    /// 存档数据控制器，这里控制所有存档数据，跟业务层无关
    /// </summary>
    public class ArchiveManager : ModuleBase, IUpdateFrame
    {
        /// <summary>
        /// 是否加密
        /// </summary>
#if UNITY_EDITOR
        private const bool IS_ENCRY = false;
        private const string datExtName = ".json";
#else
        private const bool IS_ENCRY = true;
        private const string datExtName = ".dat";
#endif
        public const string datDirPath = "ArchiveData/";

        private Dictionary<string, BaseArchiveData> archiveDict = new();
        private HashSet<string> dirtyArchiveNames = new();
        
        /// <summary>
        /// 缓存的访客ID
        /// </summary>
        private string visitorId;
        /// <summary>
        /// 缓存的用户ID
        /// </summary>
        private string userId;

        protected override void OnInit()
        {
            base.OnInit();
            AddModule<UpdateModule>().SubscribeFrameUpdate(this);
            AddModule<EventModule>().Subscribe<EventOnGamePause>(OnGamePause);
        }

        /// <summary>
        /// 标记有存档脏数据
        /// </summary>
        public void SetDirty(string archiveName)
        {
            if (string.IsNullOrEmpty(archiveName)) return;
            dirtyArchiveNames.Add(archiveName);
        }

        /// <summary>
        /// 保存所有存档脏数据
        /// </summary>
        public void SaveAllDirty()
        {
            if (dirtyArchiveNames.Count == 0) return;
            var dirtyNames = dirtyArchiveNames.ToArray();
            foreach (var archiveName in dirtyNames)
            {
                if (archiveDict.TryGetValue(archiveName, out var archiveData))
                {
                    SaveArchiveData(archiveName, archiveData);
                }
            }
        }

        /// <summary>
        /// 强制保存所有存档数据
        /// </summary>
        public void SaveAll()
        {
            var archiveNames = archiveDict.Keys.ToArray();
            foreach (var archiveName in archiveNames)
            {
                SaveArchiveData(archiveName, archiveDict[archiveName]);
            }
        }
        
        /// <summary>
        /// 加载所有存档数据
        /// </summary>
        public void LoadAllArchiveData()
        {
            archiveDict.Clear();
            dirtyArchiveNames.Clear();
            if (!ES3.DirectoryExists(datDirPath)) return;
            var fileNames = ES3.GetFiles(datDirPath);
            foreach (var fileName in fileNames)
            {
                if (!fileName.EndsWith(datExtName)) continue;
                var archiveName = Path.GetFileNameWithoutExtension(fileName);
                var archiveData = ReadArchive<BaseArchiveData>(archiveName);
                if (archiveData != null)
                {
                    archiveDict[archiveName] = archiveData;
                    Debugger.Log(TAG, $"Load archive success: {archiveName}");
                }
                else
                {
                    Debugger.LogError(TAG, $"Load archive failed: {archiveName}");
                }
            }
        }

        /// <summary>
        /// 读取存档数据
        /// </summary>
        /// <param name="archiveName">存档名字</param>
        /// <typeparam name="T">存档类型</typeparam>
        /// <returns>存档数据</returns>
        public T GetAchiveData<T>(string archiveName) where T : BaseArchiveData
        {
            if (archiveDict.TryGetValue(archiveName, out var value))
            {
                return value as T;
            }
            var archiveData = ReadArchive<T>(archiveName);
            if (archiveData != null)
            {
                archiveDict[archiveName] = archiveData;
                return archiveData;
            }
            return null;
        }

        private T ReadArchive<T>(string archiveName) where T : BaseArchiveData
        {
            try
            {
                var json = ReadFromEs3(archiveName);
                if (!string.IsNullOrEmpty(json))
                {
                    return JsonUtil.DeserializeObject<T>(json);
                }
                return null;
            }
            catch (Exception e)
            {
                Debugger.LogError(TAG, e.Message);
                return null;
            }
        }

        /// <summary>
        /// 记录存档，但不立即保存
        /// </summary>
        /// <param name="archiveName">存档名字</param>
        /// <param name="archiveData">存档数据</param>
        public void RecordArchiveData(string archiveName, BaseArchiveData archiveData)
        {
            if (archiveData == null || string.IsNullOrEmpty(archiveName)) return;
            archiveDict[archiveName] = archiveData;
        }

        /// <summary>
        /// 立即保存存档
        /// </summary>
        /// <param name="archiveName">存档名字</param>
        /// <param name="archiveData">存档数据</param>
        public void SaveArchiveData(string archiveName, BaseArchiveData archiveData)
        {
            if (archiveData == null || string.IsNullOrEmpty(archiveName)) return;
            try
            {
                if (dirtyArchiveNames.Contains(archiveName))
                {
                    archiveData.lastSaveTime = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
                }
                var json = JsonUtil.SerializeArchive(archiveData, typeof(BaseArchiveData));
                WriteToEs3(archiveName, json);
                archiveDict[archiveName] = archiveData;
            }
            catch (Exception e)
            {
                Debugger.LogError(TAG, e.Message);
            }
            finally
            {
                dirtyArchiveNames.Remove(archiveName);
            }
        }

        /// <summary>
        /// 读存档文件
        /// </summary>
        private string ReadFromEs3(string key, string defaultContent = null)
        {
            var path = datDirPath + key + datExtName;
            
            //检查文件是否存在，不存在则尝试恢复备份
            if (!ES3.FileExists(path) && !ES3.RestoreBackup(path))
            {
                return defaultContent;
            }
            
            //读取文件
            string ReadFile(string p)
            {
                if (IS_ENCRY)
                {
                    var bytes = ES3.LoadRawBytes(p);
                    return EncryptUtil.DecryptString(bytes);
                }
                else
                {
                    return ES3.LoadRawString(p);
                }
            }
            string ret = ReadFile(path);
            
            //如果文件读取失败，则尝试读取备份
            if (string.IsNullOrEmpty(ret) && ES3.RestoreBackup(path))
            {
                ret = ReadFile(path);
            }
            
            return string.IsNullOrEmpty(ret) ? defaultContent : ret;
        }

        /// <summary>
        /// 写存档文件
        /// </summary>
        private void WriteToEs3(string key, string content)
        {
            var path = datDirPath + key + datExtName;
            //备份文件
            ES3.CreateBackup(path);
            if (IS_ENCRY)
            {
                var bytes = EncryptUtil.EncryptString(content);
                ES3.SaveRaw(bytes, path);
            }
            else
            {
                ES3.SaveRaw(content, path);
            }
        }

        /// <summary>
        /// 切到后台，强制保存所有存档
        /// </summary>
        private void OnGamePause(EventOnGamePause evt)
        {
            SaveAllDirty();
        }

        /// <summary>
        /// 每帧自动保存所有脏数据
        /// </summary>
        public void Update()
        {
            SaveAllDirty();
        }

        /// <summary>
        /// 删除所有存档
        /// </summary>
        public void ClearArchive()
        {
            visitorId = null;
            archiveDict.Clear();
            dirtyArchiveNames.Clear();
            ES3.DeleteDirectory(datDirPath);
        }

        /// <summary>
        /// 获取所有存档
        /// </summary>
        public Dictionary<string, BaseArchiveData> ArchiveDict => archiveDict;

        /// <summary>
        /// 从Json中恢复所有存档
        /// </summary>
        public bool SetArchivesFromJson(IDictionary<string, string> jsons, string playerId, long serverVersion)
        {
            if (jsons == null || jsons.Count == 0) return false;
            var archives = new Dictionary<string, BaseArchiveData>();
            foreach (var json in jsons)
            {
                if (string.IsNullOrEmpty(json.Value) || json.Value == "{}") continue;
                var archive = JsonUtil.DeserializeObject<BaseArchiveData>(json.Value);
                if (archive == null) continue;
                archive.lastUploadTime = archive.lastSaveTime;
                archives.Add(json.Key, archive);
                if (archive is PlayerInfoArchiveData playerInfo)
                {
                    playerInfo.userId = playerId;
                    playerInfo.serverVersion = serverVersion;
                }
            }
            if (archives.Count == 0) return false;
            ClearArchive();
            archiveDict = archives;
            SaveAll();
            GameBase.Instance.GetModule<EventManager>().Broadcast(new OnArchivesResetEvent());
            Debugger.Log(TAG, $"SetArchivesFromJson success, count: {archives.Count}");
            return true;
        }

        /// <summary>
        /// 缓存访客ID
        /// </summary>
        public string VisitorId
        {
            get
            {
                var playerInfo = GetAchiveData<PlayerInfoArchiveData>(nameof(PlayerInfoRecord));
                if (playerInfo != null && !string.IsNullOrEmpty(playerInfo.visitorId))
                {
                    return visitorId = playerInfo.visitorId;
                }
                return visitorId ??= Guid.NewGuid().ToString();
            }
        }

        /// <summary>
        /// 缓存用户ID
        /// </summary>
        public string UserId
        {
            get
            {
                var playerInfo = GetAchiveData<PlayerInfoArchiveData>(nameof(PlayerInfoRecord));
                if (playerInfo != null && !string.IsNullOrEmpty(playerInfo.userId))
                {
                    return playerInfo.userId;
                }
                return userId;
            }
            set
            {
                if (string.IsNullOrEmpty(value)) return;
                userId = value;
                var playerInfo = GetAchiveData<PlayerInfoArchiveData>(nameof(PlayerInfoRecord));
                if (playerInfo != null && playerInfo.userId != value)
                {
                    playerInfo.userId = value;
                    playerInfo.uidCreateUtcTime = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
                    SaveArchiveData(nameof(PlayerInfoRecord), playerInfo);
                    //新用户注册打点
                    GameBase.Instance.GetModule<EventManager>().Broadcast(new AnalyticsEvent(AnalyticsThirdPartyConfigKeys.New_Register));
                }
                GameBase.Instance.GetModule<GetOrWaitManager>().SetData(SRDebugManager.user_id,userId);
            }
        }
    }
}