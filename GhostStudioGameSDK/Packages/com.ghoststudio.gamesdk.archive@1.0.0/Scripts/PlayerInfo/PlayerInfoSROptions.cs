
using AppBase.Debugging;
namespace AppBase.PlayerInfo
{
    public class PlayerInfoSROptions : SROptionsModule
    {
        protected override string CatalogName => "用户和设备相关";
        protected override SROptionsPriority CatalogPriority => SROptionsPriority.User;

        [DisplayName("deviceId")]
        public string DeviceId
        {
            get => ((PlayerInfoManager)ParentModule).PlayerRecord.ArchiveData.deviceId;
            set  {}
        }
        
        [DisplayName("visitorId")]
        public string VisitorId
        {
            get => ((PlayerInfoManager)ParentModule).PlayerRecord.ArchiveData.visitorId;
            set  {}
        }
        
        [DisplayName("userId")]
        public string UserId
        {
            get => ((PlayerInfoManager)ParentModule).PlayerRecord.ArchiveData.userId;
            set  {}
        }
    }
}