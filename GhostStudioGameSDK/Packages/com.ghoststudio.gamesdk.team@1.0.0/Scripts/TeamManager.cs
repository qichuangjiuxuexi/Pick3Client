using System;
using System.Collections;
using System.Collections.Generic;
using API.V1.Game;
using AppBase;
using AppBase.Archive;
using AppBase.Config;
using AppBase.Event;
using AppBase.Module;
using AppBase.Network;
using Protocols;
using UnityEngine;

public class ConstTeamRole
{
    public const int Leader = 1;
    public const int manager = 2;
    public const int Normal = 3;
}

public class TeamManager : ModuleBase
{
    private TeamBaseInfo myTeamInfo = new ();
    
    private List<ClubLite> lastRecommendTeams = new ();
    private double lastRecommandTime = 0;
    protected int configRecommendCount = 50;
    protected int configRecommendCD = 0;
    protected int configRecommenListExpireTime = 600;
    
    private List<ClubLite> lastSearchTeams = new ();
    private double lastSearchTime = 0;
    protected int configSearchCount = 50;
    protected int configSearchCD = 0;
    protected int configSearchListExpireTime = 600;
    
    private int configTeamMemberMaxCount = 50;

    private int configSameTeamDetailCacheExpire = 60;
    private Dictionary<string, ClubComplete> teamDetailCache = new ();
    private Dictionary<string, double> teamDetailExpiredTimePoint = new ();
    protected List<TeamBadgeConfig> teamBadgeConfigs;
    protected Dictionary<int,TeamBadgeConfig> teamBadgeMap;
    protected Dictionary<int, string> roleKey;
    protected override void OnInit()
    {
        base.OnInit();
        
        //推荐配置
        int.TryParse(
            GameBase.Instance.GetModule<ConfigManager>()
                .GetConfigByKey<string, TeamConfig>(AAConst.TeamConfig, TeamConfigKeys.Team_Recommend_Count)
                .Value, out configRecommendCount);
        int.TryParse(GameBase.Instance.GetModule<ConfigManager>()
            .GetConfigByKey<string, TeamConfig>(AAConst.TeamConfig, TeamConfigKeys.Team_Recommend_Expired)
            .Value, out configRecommenListExpireTime);
        int.TryParse(GameBase.Instance.GetModule<ConfigManager>()
            .GetConfigByKey<string, TeamConfig>(AAConst.TeamConfig, TeamConfigKeys.Team_Recommend_CD)
            .Value, out configRecommendCD);
        
        //搜索配置
        int.TryParse(
            GameBase.Instance.GetModule<ConfigManager>()
                .GetConfigByKey<string, TeamConfig>(AAConst.TeamConfig, TeamConfigKeys.Team_Search_Count)
                .Value, out configSearchCount);
        int.TryParse(GameBase.Instance.GetModule<ConfigManager>()
            .GetConfigByKey<string, TeamConfig>(AAConst.TeamConfig, TeamConfigKeys.Team_Search_Expired)
            .Value, out configSearchListExpireTime);
        int.TryParse(GameBase.Instance.GetModule<ConfigManager>()
            .GetConfigByKey<string, TeamConfig>(AAConst.TeamConfig, TeamConfigKeys.Team_Search_CD)
            .Value, out configSearchCD);
        //其他配置
        int.TryParse(GameBase.Instance.GetModule<ConfigManager>()
            .GetConfigByKey<string, TeamConfig>(AAConst.TeamConfig, TeamConfigKeys.Team_MaxMember_Count)
            .Value, out configTeamMemberMaxCount);
        
        int.TryParse(GameBase.Instance.GetModule<ConfigManager>()
            .GetConfigByKey<string, TeamConfig>(AAConst.TeamConfig, TeamConfigKeys.Team_Detail_Expired)
            .Value, out configSameTeamDetailCacheExpire);

        teamBadgeConfigs = GameBase.Instance.GetModule<ConfigManager>().GetConfigList<TeamBadgeConfig>(AAConst.TeamBadgeConfig);
        teamBadgeMap = new Dictionary<int, TeamBadgeConfig>();
        for (int i = 0; i < teamBadgeConfigs.Count; i++)
        {
            teamBadgeMap[teamBadgeConfigs[i].ID] = teamBadgeConfigs[i];
        }

        roleKey = new Dictionary<int, string>();
        roleKey[ConstTeamRole.Leader] = GameBase.Instance.GetModule<ConfigManager>()
            .GetConfigByKey<string, TeamConfig>(AAConst.TeamConfig, TeamConfigKeys.Team_Role_Leader_Key)
            .Value;
        roleKey[ConstTeamRole.manager] = GameBase.Instance.GetModule<ConfigManager>()
            .GetConfigByKey<string, TeamConfig>(AAConst.TeamConfig, TeamConfigKeys.Team_Role_Manager_Key)
            .Value;
        roleKey[ConstTeamRole.Normal] = GameBase.Instance.GetModule<ConfigManager>()
            .GetConfigByKey<string, TeamConfig>(AAConst.TeamConfig, TeamConfigKeys.Team_Role_Normal_Key)
            .Value;
    }

    /// <summary>
    /// 公会信息是否确认过（网络联通状态下，myTeamInfo是否有效）
    /// </summary>
    /// <returns></returns>
    public bool IsTeamInfoValid()
    {
        return myTeamInfo.isValid;
    }

    public bool IsHaveJoinTeam()
    {
        if (myTeamInfo.isValid && myTeamInfo.detailInfo != null)
        {
            return true;
        }

        return false;
    }

    public int TeamMemberMaxCount
    {
        get
        {
            return configTeamMemberMaxCount;
        }
    }

    public void GetSelfTeamInfo(Action<ErrorReason> callBack)
    {
        //请求更新
        GameBase.Instance.GetModule<NetworkManager>().Send<GetSelfTeamInfoProtocol>(new GetSelfTeamInfoProtocol(), (success, protocol) =>
        {
            if (!success)
            {
                callBack?.Invoke(protocol.errorCode);
                return;
            }

            myTeamInfo.detailInfo = protocol.response.ClubComplete;
            myTeamInfo.isValid = true;
            callBack?.Invoke(protocol.errorCode);
            GameBase.Instance.GetModule<EventManager>().Broadcast<EventOnConfirmSelfTeamInfo>();
        });
    }
    
    
    /// <summary>
    /// 
    /// </summary>
    /// <param name="start"></param>
    /// <param name="count"></param>
    /// <param name="level"></param>
    /// <param name="callBack">参数代表是否请求到了新的列表</param>
    public void GetRecommendTeams(int start,int count,int level,Action<bool,ErrorReason> callBack = null)
    {
        if (lastRecommendTeams.Count > 0)
        {
            if (Time.realtimeSinceStartup - lastRecommandTime < configRecommendCD)
            {
                callBack?.Invoke(false,ErrorReason.Success);
                return;
            }
        }
        //请求更新
        GameBase.Instance.GetModule<NetworkManager>().Send<GetTeamsProtocol>(new GetTeamsProtocol(start,count,level), (success, protocol) =>
        {
            if (!success)
            {
                callBack?.Invoke(false,protocol.errorCode);
                return;
            }

            lastRecommandTime = Time.realtimeSinceStartup;
            lastRecommendTeams = protocol.GetTeamsItems();
            callBack?.Invoke(true,protocol.errorCode);
        });
    }

    public int GetRecommendTotalCount()
    {
        return lastRecommendTeams.Count;
    }

    public ClubLite GetRecommendTeamByIndex(int index)
    {
        if (index < 0 || index > lastRecommendTeams.Count)
        {
            return null;
        }

        return lastRecommendTeams[index];
    }
    
    /// <summary>
    /// 获取推荐列表
    /// </summary>
    /// <param name="start"></param>
    /// <param name="count"></param>
    /// <param name="level"></param>
    /// <param name="searchPattern"></param>
    /// <param name="callBack">第一个参数表示是否成功从服务器获取了新的列表。第二个参数表示，如果不是从本地缓存拿的，而是通过网络请求的，请求的结果状态码是什么</param>
    public void SearchTeams(int start,int count,int level,string searchPattern,Action<bool,ErrorReason> callBack = null)
    {
        if (lastSearchTeams.Count > 0)
        {
            if (Time.realtimeSinceStartup - lastSearchTime < configSearchCD)
            {
                callBack?.Invoke(false,ErrorReason.Success);
                return;
            }
        }
        GameBase.Instance.GetModule<NetworkManager>().Send<GetTeamsProtocol>(new GetTeamsProtocol(start,count,level,searchPattern), (success, protocol) =>
        {
            if (!success)
            {
                callBack?.Invoke(false,protocol.errorCode);
                return;
            }

            lastSearchTime = Time.realtimeSinceStartup;
            lastSearchTeams = protocol.GetTeamsItems();
            callBack?.Invoke(true,ErrorReason.Success);
        });
    }

    public int GetSearchTeamTotalCount()
    {
        return lastSearchTeams.Count;
    }
    
    public ClubLite GetSearchTeamByIndex(int index)
    {
        if (index < 0 || index > lastSearchTeams.Count)
        {
            return null;
        }

        return lastSearchTeams[index];
    }
    
    public void GetTeamDetail(string teamID,Action<ErrorReason,ClubComplete> callBack)
    {
        if (teamDetailCache.ContainsKey(teamID) && teamDetailExpiredTimePoint.ContainsKey(teamID))
        {
            if (teamDetailExpiredTimePoint[teamID] > Time.realtimeSinceStartup)
            {
                callBack?.Invoke(ErrorReason.Success,teamDetailCache[teamID]);
                return;
            }
        }
        //请求更新
        GameBase.Instance.GetModule<NetworkManager>().Send<GetTeamsDetailProtocol>(new GetTeamsDetailProtocol(teamID), (success, protocol) =>
        {
            if (!success)
            {
                callBack?.Invoke(protocol.errorCode,null);
                return;
            }
            var detail = protocol.GetClubDetail();
            teamDetailCache[teamID] = detail;
            teamDetailExpiredTimePoint[teamID] = Time.realtimeSinceStartup + configSameTeamDetailCacheExpire;
            callBack?.Invoke(protocol.errorCode,protocol.GetClubDetail());
        });
    }

    public void JointTeam(string teamID,ClubMember leaderInfo,int level,Action<ErrorReason> callBack)
    {
        GameBase.Instance.GetModule<NetworkManager>().Send<JoinTeamProtocol>(new JoinTeamProtocol(teamID,leaderInfo,level), (success, protocol) =>
        {
            if (!success)
            {
                callBack?.Invoke(protocol.errorCode);
                return;
            }
            OnJoinNewTeam(protocol.GetClubDetail());
            callBack?.Invoke(protocol.errorCode);
        });
    }

    public void CreateTeam(ClubBasic baseInfo,ClubMember leaderInfo,Action<ErrorReason,ClubComplete> callBack)
    {
        GameBase.Instance.GetModule<NetworkManager>().Send<CreateTeamProtocol>(new CreateTeamProtocol(baseInfo,leaderInfo), (success, protocol) =>
        {
            if (!success)
            {
                callBack?.Invoke(protocol.errorCode,null);
                return;
            }

            OnJoinNewTeam(protocol.GetClubDetail());
            callBack?.Invoke(protocol.errorCode,myTeamInfo.detailInfo);
        });
    }

    private void OnJoinNewTeam(ClubComplete detaiInfo)
    {
        lastRecommendTeams?.Clear();
        lastSearchTeams?.Clear();
        myTeamInfo.isValid = true;
        myTeamInfo.detailInfo = detaiInfo;
        myTeamInfo.messages = new List<Message>();
    }

    public void LeaveTeam(string teamID,Action<ErrorReason> callBAck)
    {
        GameBase.Instance.GetModule<NetworkManager>().Send<LeaveTeamProtocol>(new LeaveTeamProtocol(teamID), (success, protocol) =>
        {
            callBAck?.Invoke(protocol.errorCode);
            OnLeaveTeam();
        });
    }

    private void OnLeaveTeam()
    {
        myTeamInfo.isValid = false;
        myTeamInfo.messages?.Clear();
        myTeamInfo.detailInfo = null;
        lastRecommendTeams?.Clear();
        lastSearchTeams?.Clear();
    }

    public void UpdateMemberBasic(string nickName,string avatar)
    {
        var protocol = new UpdateMemberBasicProtocol(myTeamInfo.detailInfo.Basic.Name,nickName,avatar);
        GameBase.Instance.GetModule<NetworkManager>().Send<UpdateMemberBasicProtocol>(protocol, (success, protocol) =>
        {
            var myId = GameBase.Instance.GetModule<ArchiveManager>().UserId;
            for (int i = 0; i < myTeamInfo.detailInfo.MemberList.Count; i++)
            {
                var member = myTeamInfo.detailInfo.MemberList[i];
                if (member.PlayerId == myId)
                {
                    member.Avatar = avatar;
                    member.Nickname = nickName;
                    break;
                }
            }
        });
    }
    
    /// <summary>
    /// 更新分数
    /// </summary>
    /// <param name="score">分数值</param>
    /// <param name="scoreIndex">更新哪个分数字段</param>
    public void UpdateMemberScore(double score,int scoreIndex = 0)
    {
        string scoreKeyName = "score";
        if (scoreIndex > 0)
        {
            scoreKeyName = "score" + scoreIndex;
        }
        UpdateMemberScoreProtocol protocol = new UpdateMemberScoreProtocol(myTeamInfo.detailInfo.Basic.Name,score,scoreKeyName);
        GameBase.Instance.GetModule<NetworkManager>().Send<UpdateMemberScoreProtocol>(protocol, (success, protocol) =>
        {
            var myId = GameBase.Instance.GetModule<ArchiveManager>().UserId;
            for (int i = 0; i < myTeamInfo.detailInfo.MemberList.Count; i++)
            {
                var member = myTeamInfo.detailInfo.MemberList[i];
                if (member.PlayerId == myId)
                {
                    member.Score = score;
                    break;
                }
            }
        });
    }

    public void UpdateTeamBasic(string teamName,int bagde,string des,int requireLevel,Action<ErrorReason> callBack)
    {
        myTeamInfo.detailInfo.Basic.Name = teamName;
        myTeamInfo.detailInfo.Basic.Badge = bagde;
        myTeamInfo.detailInfo.Basic.Desc = des;
        myTeamInfo.detailInfo.Basic.RequireLevel = requireLevel;
        UpdateTeamBasicProtocol protocol =
            new UpdateTeamBasicProtocol(myTeamInfo.detailInfo.Basic.Name, myTeamInfo.detailInfo.Basic);
        GameBase.Instance.GetModule<NetworkManager>().Send<UpdateTeamBasicProtocol>(protocol, (success, protocol) =>
        {
            callBack?.Invoke(protocol.errorCode);
            if (success)
            {
                GameBase.Instance.GetModule<EventManager>().Broadcast<EventTeamBasicChanged>();
            }
        });
    }

    public void KickMember(string teamID,string targetMemberID)
    {
        var protocol = new KickMemberProtocol(teamID,targetMemberID);
        GameBase.Instance.GetModule<NetworkManager>().Send<KickMemberProtocol>(protocol, (success, protocol) =>
        {
            if (success)
            {
                GameBase.Instance.GetModule<EventManager>().Broadcast<EventTeamMemberUpdate>();
            }
        });
    }

    public void UpdateMemberRole(string targetMemberID,int roleID)
    {
        var protocol = new UpdateMemberRoleProtocol(myTeamInfo.detailInfo.Basic.Name,targetMemberID,roleID);
        GameBase.Instance.GetModule<NetworkManager>().Send<UpdateMemberRoleProtocol>(protocol, (success, protocol) =>
        {
            if (success)
            {
                var members = myTeamInfo.detailInfo.MemberList;
                for (int i = 0; i < members.Count; i++)
                {
                    if (members[i].PlayerId == targetMemberID)
                    {
                        members[i].Role = roleID;
                        break;
                    }
                }
                GameBase.Instance.GetModule<EventManager>().Broadcast<EventTeamMemberUpdate>();
            }
        });
    }
    public List<TeamBadgeConfig> GetTeamBadgeList()
    {
        return teamBadgeConfigs;
    }
    public int GetTotalTeamBadgeCount()
    {
        return teamBadgeConfigs.Count;
    }
    
    public TeamBadgeConfig GetTeamBadgeConfig(int badgeID)
    {
        if (teamBadgeMap.ContainsKey(badgeID))
        {
            return teamBadgeMap[badgeID];
        }

        return null;
    }

    public string GetTeamRoleKey(int roleID)
    {
        string key = "";
        roleKey.TryGetValue(roleID, out key);
        return key;
    }

    public ClubComplete GetSelfTeamDetail()
    {
        if (!myTeamInfo.isValid)
        {
            return null;
        }
        return myTeamInfo.detailInfo;
    }
    
}
