using System;
using System.Collections.Generic;
using AppBase.Event;
using AppBase.Module;
using AppBase.Utils;

namespace AppBase.UserAssets
{
    /// <summary>
    /// 用户资产管理器：临时资产管理
    /// </summary>
    public partial class UserAssetManager
    {
        /// <summary>
        /// 临时资产
        /// </summary>
        public TempAssetManager TempAssets { get; private set; }

        /// <summary>
        /// 临时资产管理器
        /// </summary>
        public class TempAssetManager : ModuleBase
        {
            protected UserAssetManager manager => (UserAssetManager)ParentModule;
            
            /// <summary>
            /// 临时资产列表
            /// </summary>
            public Dictionary<int, UserAssetItem> AssetItems => manager.UserAssetRecord.ArchiveData.tempAssets;

            protected override void OnAfterInit()
            {
                base.OnAfterInit();
                ConsumeAllAssets();
            }

            public void SetDirty() => manager.SetDirty();

            /// <summary>
            /// 获取资产数量
            /// </summary>
            /// <param name="assetId">资产类型</param>
            /// <param name="includeOrder">是否包含未消耗订单的数据</param>
            /// <returns>资产数量</returns>
            public long GetAssetNum(int assetId)
            {
                return Math.Max(0, GetAssetItem(assetId)?.assetNum ?? 0);
            }

            /// <summary>
            /// 设置资产数值
            /// </summary>
            /// <param name="assetId">资产类型</param>
            /// <param name="assetNum">资产数量</param>
            /// <param name="tag">来源标签，打点使用</param>
            public long SetAssetNum(int assetId, long assetNum, string tag)
            {
                var item = GetOrCreateAssetItem(assetId);
                var oldAssetNum = manager.GetAssetNum(assetId,true);
                item.assetNum = Math.Max(0, assetNum);
                SetDirty();
                long newAssetNum = manager.GetAssetNum(assetId,true);
                if (newAssetNum != oldAssetNum)
                    GameBase.Instance.GetModule<EventManager>()
                        .Broadcast(new EventAllUserAssetChange(assetId, oldAssetNum, newAssetNum, tag));
                return item.assetNum;
            }
            
            /// <summary>
            /// 增加资产数量
            /// </summary>
            /// <param name="assetId">资产类型</param>
            /// <param name="assetNum">资产数量</param>
            /// <param name="tag">来源标签，打点使用</param>
            /// <returns>新的资产数量</returns>
            public long AddAssetNum(int assetId, long assetNum, string tag)
            {
                var oldAssetNum = GetAssetNum(assetId);
                var newAssetNum = oldAssetNum.SafeIncrement(assetNum);
                SetAssetNum(assetId, newAssetNum, tag);
                return newAssetNum;
            }
            
            /// <summary>
            /// 直接减少资产数量
            /// </summary>
            /// <param name="assetId">资产类型</param>
            /// <param name="assetNum">资产数量</param>
            /// <param name="tag">来源标签，打点使用</param>
            /// <returns>新的资产数量</returns>
            public long SubAssetNum(int assetId, long assetNum, string tag)
            {
                return AddAssetNum(assetId, -assetNum, tag);
            }
            
            /// <summary>
            /// 是否存在资产数据，可用来初始化资产
            /// </summary>
            /// <param name="assetId">资产类型</param>
            /// <returns>是否存在资产数据</returns>
            public bool HasAsset(int assetId)
            {
                return GetAssetItem(assetId) != null;
            }

            /// <summary>
            /// 获取数值资产数据
            /// </summary>
            /// <param name="assetId">资产类型</param>
            /// <returns>数值资产数据</returns>
            public UserAssetItem GetAssetItem(int assetId)
            {
                return AssetItems.TryGetValue(assetId, out var item) ? item : null;
            }

            /// <summary>
            /// 获取或创建资产
            /// </summary>
            /// <param name="assetId">资产类型</param>
            /// <param name="defaultValue">创建时的初始值</param>
            /// <param name="defaultData">创建时的其他初始数据</param>
            /// <returns>资产数据</returns>
            public UserAssetItem GetOrCreateAssetItem(int assetId, long defaultValue = 0, object defaultData = null)
            {
                var item = GetAssetItem(assetId);
                if (item == null)
                {
                    item = new UserAssetItem(assetId, defaultValue, defaultData);
                    AssetItems[assetId] = item;
                    SetDirty();
                }
                return item;
            }
            
            /// <summary>
            /// 获取资产其他数据
            /// </summary>
            /// <param name="assetId">资产类型</param>
            /// <returns>资产其他数据</returns>
            public object GetAssetData(int assetId)
            {
                return GetAssetItem(assetId)?.data;
            }
            
            /// <summary>
            /// 获取资产其他数据
            /// </summary>
            /// <param name="assetId">资产类型</param>
            /// <param name="defaultValue">找不到资产时的默认值</param>
            /// <returns>资产其他数据</returns>
            public T GetAssetData<T>(int assetId, T defaultValue = default)
            {
                var item = GetAssetItem(assetId);
                if (item != null)
                {
                    try
                    {
                        return (T)Convert.ChangeType(item.data, typeof(T));
                    }
                    catch
                    {
                    }
                }
                return defaultValue;
            }

            /// <summary>
            /// 设置资产其他数据
            /// </summary>
            /// <param name="assetId">资产类型</param>
            /// <param name="assetData">资产其他数据</param>
            /// <param name="tag">来源标签，打点使用</param>
            public void SetAssetData(int assetId, object assetData, string tag)
            {
                var item = GetOrCreateAssetItem(assetId);
                item.data = assetData;
                SetDirty();
            }
            
            /// <summary>
            /// 设置资产数值和其他数据
            /// </summary>
            /// <param name="assetId">资产类型</param>
            /// <param name="assetData">资产其他数据</param>
            /// <param name="tag">来源标签，打点使用</param>
            public void SetAssetData(int assetId, long assetNum, object assetData, string tag)
            {
                var item = GetOrCreateAssetItem(assetId);
                var oldAssetNum = manager.GetAssetNum(assetId,true);
                item.assetNum = Math.Max(0, assetNum);
                item.data = assetData;
                SetDirty();
                long newAssetNUm = manager.GetAssetNum(assetId,true);
                if (newAssetNUm != oldAssetNum)
                    GameBase.Instance.GetModule<EventManager>()
                        .Broadcast(new EventAllUserAssetChange(assetId, oldAssetNum, newAssetNUm, tag));
            }

            /// <summary>
            /// 清除资产
            /// </summary>
            /// <param name="assetId">资产类型</param>
            /// <param name="tag">来源标签，打点使用</param>
            /// <returns>清除前的资产数值</returns>
            public long ClearAsset(int assetId, string tag)
            {
                var item = GetAssetItem(assetId);
                if (item == null) return 0;
                var oldNum = manager.GetAssetNum(assetId,true);
                item.assetNum = 0;
                item.data = null;
                AssetItems.Remove(assetId);
                SetDirty();
                long newNum = manager.GetAssetNum(assetId,true);
                if (newNum != oldNum)
                    GameBase.Instance.GetModule<EventManager>().Broadcast(
                        new EventAllUserAssetChange(assetId, oldNum, newNum, tag));
                return oldNum;
            }
            
            /// <summary>
            /// 将某个临时资产转换为正式资产
            /// </summary>
            /// <param name="assetId">资产类型</param>
            /// <returns>转换的临时资产数量</returns>
            public long ConsumeAsset(int assetId)
            {
                var tempAsset = GetAssetItem(assetId);
                if (tempAsset == null) return 0;
             
                
                //优先尝试调用自定义的消耗逻辑
                if (manager.OnConsumeAsset(tempAsset))
                {
                    //自定义消耗成功，不再调用默认的消耗逻辑
                    AssetItems.Remove(assetId);
                    return tempAsset.assetNum;
                }
                //调用默认的消耗逻辑
                var assetItem = manager.GetOrCreateAssetItem(assetId);
                long oldNum = assetItem.assetNum;
                if (tempAsset.data != null)
                {
                    assetItem.data = tempAsset.data;
                }
                if (tempAsset.assetNum != 0)
                {
                    assetItem.assetNum = assetItem.assetNum.SafeIncrement(tempAsset.assetNum);
                }
                AssetItems.Remove(assetId);
                GameBase.Instance.GetModule<EventManager>().Broadcast(
                    new EventUserAssetChange(assetId, oldNum, assetItem.assetNum, "temp"));
                SetDirty();
                return tempAsset.assetNum;
            }

            /// <summary>
            /// 将所有临时资产转换为正式资产
            /// </summary>
            public void ConsumeAllAssets()
            {
                if (AssetItems.Count == 0) return;
                foreach (var tempAsset in AssetItems.Values)
                {
                    var assetItem = manager.GetOrCreateAssetItem(tempAsset.assetId);
                
                    //优先尝试调用自定义的消耗逻辑
                    if (manager.OnConsumeAsset(tempAsset))
                    {
                        //自定义消耗成功，不再调用默认的消耗逻辑
                        continue;
                    }
                    //调用默认的消耗逻辑
                    if (tempAsset.data != null)
                    {
                        assetItem.data = tempAsset.data;
                    }
                    if (tempAsset.assetNum != 0)
                    {
                        assetItem.assetNum = assetItem.assetNum.SafeIncrement(tempAsset.assetNum);
                    }
                }
                AssetItems.Clear();
                SetDirty();
            }
        }
    }
}