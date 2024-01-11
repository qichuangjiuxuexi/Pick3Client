using System;
using System.Collections.Generic;
using System.Linq;
using AppBase.Config;
using AppBase.Event;
using AppBase.Module;
using AppBase.Network;
using AppBase.Timing;
using AppBase.Utils;
using UnityEngine;

namespace AppBase.LeaderBoard
{
    /// <summary>
    /// 排行榜管理器
    /// </summary>
    public class LeaderBoardManager : ModuleBase,IUpdateSecond
    {
        /// <summary>
        /// 排行榜列表缓存
        /// </summary>
        private Dictionary<LeaderBoardID, List<LeaderBoardItem>> cachedBoards = new();
        
        // /// <summary>
        // /// 每个排行榜自己的排行数据
        // /// </summary>
        // private Dictionary<LeaderBoardID, LeaderBoardItem> selfItemCache;
        
        /// <summary>
        /// 缓存获取过期时间
        /// </summary>
        private Dictionary<LeaderBoardID, float> cachedGetTimes = new();
        
        /// <summary>
        /// 排行榜更新缓存
        /// </summary>
        private Dictionary<LeaderBoardID, LeaderBoardItem> cachedUpdates = new();
        
        /// <summary>
        /// 需要更新，但是收频率限制暂时没更新的条目
        /// </summary>
        private Dictionary<LeaderBoardID,LeaderBoardItem> waitingUpload = new();
        
        /// <summary>
        /// 缓存更新过期时间
        /// </summary>
        private Dictionary<LeaderBoardID, float> cachedUpdateTimes = new();

        /// <summary>
        /// 用于每秒检查是否有需要自动上传的条目，避免每秒创建一个字典
        /// </summary>
        private Dictionary<LeaderBoardID, LeaderBoardItem> tmpCheckUpdateDict = new ();

        /// <summary>
        /// 标记上次上传的分数，避免统一分数多次上传，导致同一分数排名不稳定的情况出现
        /// </summary>
        private Dictionary<LeaderBoardID, double> lastUpdateScoreDict = new();

        private bool isUpdatingItem = false;
        private LeaderboardConfig GetConfig(LeaderBoardID id)
        {
            if (id == null)
            {
                if (AppUtil.IsDebug)
                {
                    Debugger .LogDError("leaderboard id 为空");
                    return null;
                }
            }
            return GameBase.Instance.GetModule<ConfigManager>().GetConfigByKey<int, LeaderboardConfig>(AAConst.LeaderboardConfig, id.featureId);
        }

        protected override void OnInternalInit()
        {
            base.OnInternalInit();
            GameBase.Instance.GetModule<EventManager>().Subscribe<EventOnGamePause>(OnPause);
            GameBase.Instance.GetModule<TimingManager>().SubscribeSecondUpdate(this);
        }

        /// <summary>
        /// 游戏切到后台，将未上传的个人积分同步给服务器
        /// </summary>
        /// <param name="paras"></param>
        void OnPause(EventOnGamePause paras)
        {
            if (waitingUpload == null || waitingUpload.Count == 0)
            {
                return;
            }

            var dic = new Dictionary<LeaderBoardID, LeaderBoardItem>();
            foreach (var item in waitingUpload)
            {
                //分数没变不上传
                if (lastUpdateScoreDict.ContainsKey(item.Key) &&
                    Mathf.Approximately((float)lastUpdateScoreDict[item.Key], (float)item.Value.score))
                {
                    continue;
                }
                dic.Add(item.Key, item.Value);
            }

            RequestUpdateLeaderBoard(dic,null);
        }

        /// <summary>
        /// 获取排行榜列表
        /// </summary>
        /// <param name="id">排行榜ID</param>
        /// <param name="callback">回调</param>
        /// <param name="useCached">是否使用缓存</param>
        public void GetLeaderBoard(LeaderBoardID id, Action<LeaderBoardID,List<LeaderBoardItem>> callback = null, bool useCached = true)
        {
            //读取缓存
            if (useCached &&
                cachedGetTimes.TryGetValue(id, out var time) && time < Time.realtimeSinceStartup &&
                cachedBoards.TryGetValue(id, out var items) && items.Count > 0)
            {
                callback?.Invoke(id,items);
            }
            //请求数据
            else
            {
                RequestGetLeaderBoard(id, callback);
            }
        }

        /// <summary>
        /// 更新单个排行榜数据
        /// </summary>
        /// <param name="id">排行榜ID</param>
        /// <param name="item">排行榜数据</param>
        /// <param name="callback">回调</param>
        /// <param name="forceUpdate">是否立即更新</param>
        public void UpdateLeaderBoard(LeaderBoardID id, LeaderBoardItem item, Action<bool> callback = null, bool forceUpdate = false)
        {
            //验证合法性
            var config = GetConfig(id);
            if (config == null)
            {
                callback?.Invoke(false);
                return;
            }
            //如果分数不变，不再上传
            if (cachedUpdates.ContainsKey(id) && lastUpdateScoreDict.ContainsKey(id) &&
                Mathf.Approximately((float)cachedUpdates[id].score, (float)lastUpdateScoreDict[id]))
            {
                cachedUpdates[id] = item;
                callback?.Invoke(false);
                return;
            }
            //缓存更新
            cachedUpdates[id] = item;
            //请求更新
            if (forceUpdate ||
                !cachedUpdateTimes.TryGetValue(id, out var time) ||
                time < Time.realtimeSinceStartup)
            {
                RequestUpdateLeaderBoard(new Dictionary<LeaderBoardID, LeaderBoardItem> { { id, item } }, callback);
            }
            else
            {
                //true或false如何界定？
                if (cachedBoards.ContainsKey(id))
                {
                    int oldRank = item.rank;
                    int newRank = GetNewRankWithOldRankList(cachedBoards[id],item);
                    for (int i = 0; i < cachedBoards[id].Count; i++)
                    {
                        if (cachedBoards[id][i].id == item.id)
                        {
                            cachedBoards[id][i].score = item.score;
                            break;
                        }
                    }
                    item.rank = newRank;
                    if (oldRank != newRank)
                    {
                        if (AppUtil.IsDebug)
                        {
                            Debugger.LogDWarning($"更新缓存排行榜缓存:{id.ToString()},new rank:{newRank},old rank:{oldRank}");
                        }
                        CorrectRankList(cachedBoards[id],item,oldRank);
                    }
                }
                callback?.Invoke(false);
                waitingUpload[id] = item;
            }
        }

        int GetNewRankWithOldRankList(List<LeaderBoardItem> targetList,LeaderBoardItem targetItem)
        {
            if (targetList.Count == 0)
            {
                return targetItem.rank;
            }

            int count = targetList.Count - 1;
            for (int i = count; i >= 0; i--)
            {
                if (Mathf.Approximately((float)targetList[i].score,(float)targetItem.score) || targetList[i].score > targetItem.score)
                {
                    return targetList[i].rank + 1;
                }
            }

            return 1;
        }

        /// <summary>
        /// 获取自己的排名信息(自己有可能在排行榜单外，在List中拿不到)
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public LeaderBoardItem GetSelfLeaderBoardData(LeaderBoardID id)
        {
            if (cachedUpdates != null && cachedUpdates.ContainsKey(id))
            {
                return cachedUpdates[id];
            }

            return null;
        }

        /// <summary>
        /// 请求获取排行榜数据
        /// </summary>
        private void RequestGetLeaderBoard(LeaderBoardID id, Action<LeaderBoardID,List<LeaderBoardItem>> callback)
        {
            var config = GetConfig(id);
            GameBase.Instance.GetModule<NetworkManager>().Send(new GetLeaderBoardProtocol(id, 0, config.limit),
                (success, protocol) =>
                {
                    if (!success)
                    {
                        cachedBoards.TryGetValue(id, out var result);
                        callback?.Invoke(id,result);
                        return;
                    }

                    var items = protocol.GetItems();
                    var config = GetConfig(id);
                    //缓存数据
                    if (config != null && config.get_interval_seconds > 0)
                    {
                        cachedBoards[id] = items;
                        cachedGetTimes[id] = Time.realtimeSinceStartup + config.get_interval_seconds;
                    }

                    //如果本地没有缓存，使用后端数据进行缓存，否则继续使用之前的缓存，避免此次得到的数据相比缓存更老，导致出现“回档”现象。
                    var serverSelf = protocol.GetSelfItem();
                    if (serverSelf != null)
                    {
                        if (!cachedUpdates.ContainsKey(id))
                        {
                            cachedUpdates[id] = serverSelf;
                        }
                        else
                        {
                            //比较服务器数据与缓存的数据，如果缓存的数据与服务器数据不一致：本地数据大，则使用本地数据，并重新排名。如果服务器数据大，则使用服务器数据覆盖本地数据
                            if (!Mathf.Approximately((float)serverSelf.score, (float)cachedUpdates[id].score))
                            {
                                if (serverSelf.score > cachedUpdates[id].score)
                                {
                                    cachedUpdates[id] = serverSelf;
                                    if (waitingUpload.ContainsKey(id))
                                    {
                                        waitingUpload.Remove(id);
                                    }
                                }
                                else
                                {
                                    for (int i = 0; i < items.Count; i++)
                                    {
                                        if (items[i].id == serverSelf.id)
                                        {
                                            items[i].score = cachedUpdates[id].score;
                                            items.Sort();
                                            break;
                                        }
                                    }
                                }
                            }
                        }
                    }
                    for (int i = 0; i < items.Count; i++)
                    {
                        items[i].rank = i + 1;
                        if (cachedUpdates.ContainsKey(id))
                        {
                            if (cachedUpdates[id].id == items[i].id)
                            {
                                cachedUpdates[id].rank = items[i].rank;
                            }
                        }
                    }
                    callback?.Invoke(id,items);
                });
        }

        /// <summary>
        /// 请求更新排行榜数据
        /// </summary>
        private void RequestUpdateLeaderBoard(IDictionary<LeaderBoardID, LeaderBoardItem> entries, Action<bool> callback)
        {
            if (entries == null || entries.Count == 0)
            {
                callback?.Invoke(false);
                return;
            }
            //记录更新时间，避免反复更新
            foreach (var id in entries.Keys)
            {
                var config = GetConfig(id);
                if (config != null && config.upload_interval_seconds > 0)
                {
                    cachedUpdateTimes[id] = Time.realtimeSinceStartup + config.upload_interval_seconds;
                }
                lastUpdateScoreDict[id] = entries[id].score;
                if (waitingUpload != null && waitingUpload.ContainsKey(id))
                {
                    waitingUpload.Remove(id);
                }
            }
            
            //请求更新
            GameBase.Instance.GetModule<NetworkManager>().Send<UpdateLeaderBoardProtocol>(new UpdateLeaderBoardProtocol(entries), (success, protocol) =>
            {
                if (!success)
                {
                    callback?.Invoke(false);
                    return;
                }
                //验证更新结果
                var ranks = protocol.GetUpdatedRanks();
                if (ranks.Count != entries.Count)
                {
                    Debugger.LogError(TAG, $"Update failed, ranks count {ranks.Count} != entries count {entries.Count}");
                    callback?.Invoke(false);
                    return;
                }
                //更新排名缓存
                int index = 0;
                foreach (var pair in entries)
                {
                    int oldRank = pair.Value.rank;
                    pair.Value.rank = ranks[index++];
                    if (cachedBoards.TryGetValue(pair.Key, out var items) && items.Count > 0)
                    {
                        //todo
                        if (oldRank != pair.Value.rank)
                        {
                            CorrectRankList(items,pair.Value,oldRank);
                        }
                        
                        //刷新榜单中自己的分数
                        for (int i = 0; i < items.Count; i++)
                        {
                            if (items[i].id == pair.Value.id)
                            {
                                items[i].score = pair.Value.score;
                            }
                        }
                        
                    }
                }
                callback?.Invoke(true);
            });
        }

        void CorrectRankList(List<LeaderBoardItem> targetList,LeaderBoardItem targetItem,int oldRank)
        {
            int newRank = targetItem.rank;
            
            //情形1：玩家原来不在榜，现在依然不在榜(不用处理)
            if (newRank > targetList.Count && oldRank > targetList.Count)
            {
                return;
            }
            
            //情形2：玩家原来未上榜，现在上榜了，在List中等于最新排名的条目及其后面的条目排名都掉一个名次，List数量多了一个
            if (oldRank > targetList.Count && newRank <= targetList.Count)
            {
                DealEnterList(targetList,targetItem);
                return;
            }
            //情形3：玩家原来上榜的，现在落榜了，原排名以下的往上提一个名次，List数量少一个，此时是否考虑重新拉取一下榜单？
            if (oldRank <= targetList.Count && newRank > targetList.Count)
            {
                DealQuitList(targetList,targetItem,oldRank);
                return;
            }

            //情形4：玩家排名上升，老排名(不含)与新排名（含）之间的都往上的都下降一个名次
            if (newRank < oldRank)
            {
                DealRankUp(targetList,newRank,oldRank);
                return;
            }
            //情形5：玩家排名下降，最新排名上面的都提一个名次
            if (newRank > oldRank)
            {
                DealRankDown(targetList,newRank,oldRank);
                return;
            }
        }
        

        /// <summary>
        /// 处理之前没上榜现在上榜的情形
        /// </summary>
        /// <param name="targetList"></param>
        /// <param name="targetItem"></param>
        void DealEnterList(List<LeaderBoardItem> targetList,LeaderBoardItem targetItem)
        {
            int myIndex = -1;
            for (int i = 0; i < targetList.Count; i++)
            {
                if (targetList[i].id == targetItem.id)
                {
                    myIndex = i;
                    break;
                }
            }

            if (AppUtil.IsDebug)
            {
                Debugger.LogDWarning($"玩家进榜了，之前在榜单中的索引：{myIndex}");
            }
            int newRank = targetItem.rank;
            for (int i = newRank - 1; i < targetList.Count; i++)
            {
                targetList[i].rank++;
            }
            targetList.Insert(newRank - 1,targetItem);
        }
        
        /// <summary>
        /// 处理之前上榜了现在落榜了的情形
        /// </summary>
        /// <param name="targetList"></param>
        /// <param name="targetItem"></param>
        void DealQuitList(List<LeaderBoardItem> targetList,LeaderBoardItem targetItem,int oldRank)
        {
            int newRank = targetItem.rank;
            for (int i = oldRank; i < targetList.Count; i++)
            {
                targetList[i].rank--;
            }
            targetList.RemoveAt(oldRank - 1);
        }
        
        /// <summary>
        /// 处理已上榜情况下排名上升的情形
        /// </summary>
        /// <param name="targetList"></param>
        /// <param name="newRank"></param>
        /// <param name="oldRank"></param>
        void DealRankUp(List<LeaderBoardItem> targetList,int newRank,int oldRank)
        {
            for (int i = newRank - 1; i < oldRank - 1; i++)
            {
                targetList[i].rank++;
            }
            //将List中的老排名对应的Item移动到新排名的索引位置
            var itemUsed = targetList[oldRank - 1];
            itemUsed.rank = newRank;
            targetList.RemoveAt(oldRank - 1);
            targetList.Insert(newRank - 1,itemUsed);
        }
        
        /// <summary>
        /// 处理已上榜情况下排名下降的情形
        /// </summary>
        /// <param name="targetList"></param>
        /// <param name="newRank"></param>
        /// <param name="oldRank"></param>
        void DealRankDown(List<LeaderBoardItem> targetList,int newRank,int oldRank)
        {
            for (int i = oldRank; i < newRank; i++)
            {
                targetList[i].rank--;
            }
            //将List中的老排名对应的Item移动到新排名的索引位置
            var itemUsed = targetList[oldRank - 1];
            targetList.RemoveAt(oldRank - 1);
            itemUsed.rank = newRank;
            targetList.Insert(newRank - 1,itemUsed);
        }

        /// <summary>
        /// 排名查找器
        /// </summary>
        private readonly Comparer<LeaderBoardItem> rankComparer = Comparer<LeaderBoardItem>.Create((a, b) => a.rank.CompareTo(b.rank));

        public void OnUpdateSecond()
        {
            //如果上一次的检查有了更新服务器数据的实际操作，暂时停止检查，等上次操作完成后再继续检查
            if (isUpdatingItem)
            {
                return;
            }
            tmpCheckUpdateDict?.Clear();
            foreach (var item in waitingUpload)
            {
                if (!cachedUpdateTimes.ContainsKey(item.Key))
                {
                    if (lastUpdateScoreDict.ContainsKey(item.Key))
                    {
                        //如果分数没变，不上传
                        if (Mathf.Approximately((float)lastUpdateScoreDict[item.Key], (float)item.Value.score))
                        {
                            continue;
                        }
                    }

                    tmpCheckUpdateDict[item.Key] = waitingUpload[item.Key];
                    continue;
                }

                float lastTime = cachedUpdateTimes[item.Key];
                if (Time.realtimeSinceStartup > lastTime)
                {
                    //如果分数没变，不上传
                    if (lastUpdateScoreDict.ContainsKey(item.Key))
                    {
                        if (Mathf.Approximately((float)lastUpdateScoreDict[item.Key], (float)item.Value.score))
                        {
                            continue;
                        }
                    }

                    //否则上传
                    tmpCheckUpdateDict[item.Key] = waitingUpload[item.Key];
                }
            }

            if (tmpCheckUpdateDict.Count > 0)
            {
                isUpdatingItem = true;
                if (AppUtil.IsDebug)
                {
                    Debug.Log($"时间到了，开始上传个人数据：{string.Join(" | ",tmpCheckUpdateDict.Values.ToList())}");
                }
                RequestUpdateLeaderBoard(tmpCheckUpdateDict, (result) =>
                {
                    //不管result是true还是false，都将isUpdatingItem置为false，继续检测
                    isUpdatingItem = false;
                });
            }
        }
    }
}
