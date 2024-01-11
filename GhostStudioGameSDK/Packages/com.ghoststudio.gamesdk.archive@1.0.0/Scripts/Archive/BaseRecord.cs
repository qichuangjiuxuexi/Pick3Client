using AppBase.Event;
using AppBase.Module;

namespace AppBase.Archive
{
    /// <summary>
    /// 基础存档模块，存档作为控制器的一个子模块存在，属于业务层
    /// </summary>
    /// <typeparam name="T">原始存档类型</typeparam>
    public class BaseRecord<T> : ModuleBase where T : BaseArchiveData, new()
    {
        /// <summary>
        /// 存档名字
        /// </summary>
        public virtual string ArchiveName
        {
            get { return archiveName ?? GetType().Name; }
            protected set { archiveName = value; }
        }
        private string archiveName;
        
        /// <summary>
        /// 存档原始数据
        /// </summary>
        public T ArchiveData => archiveData;
        private T archiveData;

        /// <summary>
        /// 是否是新存档，用于判定是否需要初始化
        /// </summary>
        public bool IsNewRecord;

        protected sealed override void OnInternalInit()
        {
            base.OnInternalInit();
            archiveData = GameBase.Instance.GetModule<ArchiveManager>().GetAchiveData<T>(ArchiveName);
            if (archiveData == null)
            {
                archiveData = new T();
                IsNewRecord = true;
                OnNewRecord();
                GameBase.Instance.GetModule<ArchiveManager>().RecordArchiveData(ArchiveName, archiveData);
            }
            else
            {
                IsNewRecord = false;
                OnLoadRecord();
            }
            GameBase.Instance.GetModule<EventManager>().Subscribe<OnArchivesResetEvent>(OnArchivesReset);
        }

        protected override void OnInternalDestroy()
        {
            base.OnInternalDestroy();
            GameBase.Instance.GetModule<EventManager>().Unsubscribe<OnArchivesResetEvent>(OnArchivesReset);
        }

        /// <summary>
        /// 当创建新存档时调用，可以在这里做一些初始化工作
        /// </summary>
        protected virtual void OnNewRecord()
        {
        }
        
        /// <summary>
        /// 当存档加载完成时调用，可以在这里做一些升级工作
        /// </summary>
        protected virtual void OnLoadRecord()
        {
        }

        /// <summary>
        /// 标记存档已修改，需要保存
        /// </summary>
        /// <param name="saveImmediately">是否立即保存</param>
        public void SetDirty(bool saveImmediately = false)
        {
            GameBase.Instance.GetModule<ArchiveManager>().SetDirty(ArchiveName);
            if (saveImmediately)
            {
                GameBase.Instance.GetModule<ArchiveManager>().SaveAllDirty();
            }
        }

        /// <summary>
        /// 当存档被重置时，重新读取存档
        /// </summary>
        private void OnArchivesReset(OnArchivesResetEvent evt)
        {
            OnInternalInit();
        }
    }
}