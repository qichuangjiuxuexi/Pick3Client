using System;
using AppBase.Archive;
using AppBase.Utils;
using UnityEngine;

namespace AppBase.PlayerInfo
{
    public class PlayerInfoRecord : BaseRecord<PlayerInfoArchiveData>
    {
        protected override void OnNewRecord()
        {
            ArchiveData.visitorId = null;
            ArchiveData.deviceId = AppUtil.DeviceId;
            ArchiveData.deviceName = AppUtil.DeviceName;
            ArchiveData.clientVer = Application.version;
            ArchiveData.installVer = Application.version;
            ArchiveData.installUtcTime = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
            ArchiveData.appLaunchUtcTime = ArchiveData.installUtcTime;
            ArchiveData.dlyOpenUtcTime = ArchiveData.installUtcTime;
            ArchiveData.gameTime = ArchiveData.installUtcTime;
            ArchiveData.dlyOpenLocalTime = DateTimeOffset.Now.ToUnixTimeMilliseconds();
            ArchiveData.playLocalDay = 1;
            ArchiveData.playUtcDay = 1;
            ArchiveData.continuePlayLocalDay = 1;
            ArchiveData.continuePlayUtcDay = 1;
        }
    }
}
