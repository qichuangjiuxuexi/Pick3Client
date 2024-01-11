using System;
using UnityEngine.Serialization;

namespace AppBase.Archive
{
    /// <summary>
    /// 存档的纯数据定义，不要包含任何逻辑，存档数据可能会先于逻辑模块独立加载
    /// </summary>
    [Serializable]
    public class BaseArchiveData
    {
        /// <summary>
        /// 当前存档版本号
        /// </summary>
        public int version = 0;
        
        /// <summary>
        /// 当前存档最后修改时间，UTC，毫秒
        /// </summary>
        public long lastSaveTime = 0;
        
        /// <summary>
        /// 当前存档最后上传时间，UTC，毫秒
        /// </summary>
        public long lastUploadTime = 0;
    }
}
