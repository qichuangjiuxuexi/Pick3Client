using System;
using System.Collections.Generic;
using API.V1.Game;
using AppBase.Archive;
using AppBase.Event;
using AppBase.Module;
using AppBase.Network;
using AppBase.PlayerInfo;
using AppBase.UI.Dialog;
using AppBase.Utils;
using Protocol;
using UnityEngine;

namespace AppBase.SocialAccount
{
    [Serializable]
    public class LocalSocialAccountData
    {
        public List<SocialAccountInfo> DataList = new List<SocialAccountInfo>();
    }
    /// <summary>
    /// 第三方账号绑定管理器
    /// </summary>
    public class SocialAccountManager : ModuleBase
    {
        private Dictionary<SocialAccountType, SocialAccountBase> mSocialBaseDic =
            new Dictionary<SocialAccountType, SocialAccountBase>();

        private LocalSocialAccountData mLocalData;
        private const string LOCAL_SOCIAL_ACCOUNT_SAVEKEY = "LOCAL_SOCIAL_ACCOUNT_SAVEKEY";

        private SocialAccountSettings mSettings;
        
        private ArchiveManager archiveManager => GameBase.Instance.GetModule<ArchiveManager>();
        private PlayerInfoArchiveData playerInfo => archiveManager?.GetAchiveData<PlayerInfoArchiveData>(nameof(PlayerInfoRecord));

        protected override void OnInit()   
        {
            LoadLocalData();
            if (moduleData is SocialAccountSettings settings)
            {
                SetSocialData(settings);
            }

            AddModule<EventModule>().Subscribe<UploadRequestFailedEvent>(UploadRecordFailedEvent);
        }

        protected override void OnDestroy()
        {
            mLocalData = null;
        }

        /// <summary>
        /// 设置社交账号信息
        /// </summary>
        public void SetSocialData(SocialAccountSettings settins)
        {
            mSettings = settins;
            for (int i = 0; i < settins.types.Count; i++)
            {
                switch (settins.types[i])
                {
                    case SocialAccountType.Facebook:
                        mSocialBaseDic.Add(SocialAccountType.Facebook, new SocialAccountFacebook());
                        break;
                    case SocialAccountType.Google:
                        mSocialBaseDic.Add(SocialAccountType.Google, new SocialAccountGoogle());
                        break;
                    case SocialAccountType.Apple:
                        mSocialBaseDic.Add(SocialAccountType.Apple, new SocialAccountApple());
                        break;
                }
            }
            //初始化
            foreach (var socialBase in mSocialBaseDic.Values)
            {
                socialBase.OnInit();
            }
        }

        /// <summary>
        /// 注册自定义登录渠道
        /// </summary>
        /// <param name="type"></param>
        /// <param name="socialBase"></param>
        public void RegisterSocialBase(SocialAccountType type, SocialAccountBase socialBase)
        {
            mSocialBaseDic[type] = socialBase;
        }

        /// <summary>
        /// 获取所有本地保存的社交账号信息
        /// </summary>
        /// <returns></returns>
        public List<SocialAccountInfo> GetAllBindAccountInfos()
        {
            if (mLocalData == null)
            {
                LoadLocalData();
            }
            return mLocalData.DataList;
        }
        
        /// <summary>
        /// 根据类型获取绑定过社交账号
        /// </summary>
        /// <param name="type">类型</param>
        /// <returns>社交账号信息</returns>
        public SocialAccountInfo GetBindedAccountByType(SocialAccountType type)
        {
            if (mLocalData == null)
            {
                LoadLocalData();
            }

            foreach (var social in mLocalData.DataList)
            {
                if (social.type == (int)type)
                {
                    return social;
                }
            }
            return null;
        }

        void UploadRecordFailedEvent(UploadRequestFailedEvent evt)
        {
            if (evt.errorReason == ErrorReason.PlayerUploadInvalidServerVer)
            {
                OthersLogin();
            }
        }

        /// <summary>
        /// 其他人登录绑定账号
        /// </summary>
        void OthersLogin()
        {
            if (mLocalData != null && mLocalData.DataList.Count > 0)
            {
                var socialInfo = mLocalData.DataList[0];
                GameBase.Instance.GetModule<EventManager>().Broadcast(new SocialAccountOthersLoginEvent(socialInfo));
                if (!string.IsNullOrEmpty(mSettings.otherLoginUIAddress))
                {
                    GameBase.Instance.GetModule<DialogManager>().PopupDialog(mSettings.otherLoginUIAddress, new SocialAccountDialogData()
                    {
                        accountInfo = socialInfo,
                        isNeedChange = true,
                        changeCallback = ChangeAccountToRestart,
                        restartCallback = () =>
                        {
                            GameBase.Instance.GetModule<DialogManager>().DestroyAllDialog();
                            GameBase.Instance.Restart();
                        }
                    });
                }
            }
        }

        /// <summary>
        /// 绑定社交账号
        /// </summary>
        /// <param name="type">渠道</param>
        /// <param name="sucAction">成功回调,是否需要切换账号</param>
        /// <param name="failAction">失败回调</param>
        public void Login(SocialAccountType type, Action<bool> sucAction = null, Action failAction = null)
        {
            if (!mSocialBaseDic.ContainsKey(type))
            {
                Debug.LogError("[SocialAccount] 要登录的社交平台没有接入");
                failAction?.Invoke();
                if (!string.IsNullOrEmpty(mSettings.bindFailUIAddress))
                {
                    GameBase.Instance.GetModule<DialogManager>().PopupDialog(mSettings.bindFailUIAddress, new SocialAccountDialogData()
                    {
                        accountInfo = new SocialAccountInfo(){type = (int) type},
                    });
                }
                return;
            }
            
            mSocialBaseDic[type].Login(socialInfo =>
            {
                Debug.Log($"[SocialAccount] 登录{socialInfo.type.ToString()}成功 id:{socialInfo.id} name:{socialInfo.name}");
                var account = new AccountInfo()
                {
                    Type = (AccountInfo.Types.AccountType)socialInfo.type,
                    Id = socialInfo.id,
                    Name = socialInfo.name,
                    Email = socialInfo.email??""
                };
                GameBase.Instance.GetModule<NetworkManager>().Send(new SocialLoginProtocol(account),
                    (success, protocol) =>
                    {
                        if (!success)
                        {
                            Debug.Log($"[SocialAccount] 绑定{socialInfo.type.ToString()}失败");
                            failAction?.Invoke();
                            if (!string.IsNullOrEmpty(mSettings.bindFailUIAddress))
                            {
                                GameBase.Instance.GetModule<DialogManager>().PopupDialog(mSettings.bindFailUIAddress, new SocialAccountDialogData()
                                {
                                    accountInfo = socialInfo,
                                });
                            }
                            return;
                        }

                        switch (protocol.response.Result)
                        {
                            case LoginSocialResponse.Types.LoginSocialResultType.LoginSocialResultSuccess:
                            case LoginSocialResponse.Types.LoginSocialResultType.LoginSocialResultBound:
                                sucAction?.Invoke(false);
                                if (!string.IsNullOrEmpty(mSettings.bindSucUIAddress))
                                {
                                    GameBase.Instance.GetModule<DialogManager>().PopupDialog(mSettings.bindSucUIAddress, new SocialAccountDialogData()
                                    {
                                        accountInfo = socialInfo,
                                        isNeedChange = false,
                                    });
                                }
                                AddNewSocialAccount(socialInfo);
                                break;
                            case LoginSocialResponse.Types.LoginSocialResultType.LoginSocialResultOtherPlayer:
                            case LoginSocialResponse.Types.LoginSocialResultType.LoginSocialResultOtherAccount:
                                sucAction?.Invoke(true);
                                if (!string.IsNullOrEmpty(mSettings.bindSucChangeUIAddress))
                                {
                                    GameBase.Instance.GetModule<DialogManager>().PopupDialog(mSettings.bindSucChangeUIAddress, new SocialAccountDialogData()
                                    {
                                        accountInfo = socialInfo,
                                        isNeedChange = true,
                                        changeCallback = ChangeAccountToRestart
                                    });
                                }
                                break;
                        }
                    });
            }, () =>
            {
                Debug.Log($"[SocialAccount] 登录{type.ToString()}失败");
                failAction?.Invoke();
                if (!string.IsNullOrEmpty(mSettings.bindFailUIAddress))
                {
                    GameBase.Instance.GetModule<DialogManager>().PopupDialog(mSettings.bindFailUIAddress, new SocialAccountDialogData()
                    {
                        accountInfo = new SocialAccountInfo(){type = (int) type},
                    });
                }
            });
        }

        /// <summary>
        /// 解绑社交账号
        /// </summary>
        /// <param name="type">渠道</param>
        /// <param name="sucAction">成功回调</param>
        /// <param name="failAction">失败回调</param>
        public void Logout(SocialAccountType type, Action<bool> sucAction = null, Action failAction = null)
        {
            if (!mSocialBaseDic.ContainsKey(type))
            {
                Debug.LogError("[SocialAccount] 要登录的社交平台没有接入");
                failAction?.Invoke();
                return;
            }
            
            mSocialBaseDic[type].Logout(() =>
            {
                int index = mLocalData.DataList.FindIndex(item => item.type == (int) type);
                if (index != -1)
                {
                    var socialInfo = mLocalData.DataList[index];
                    mLocalData.DataList.RemoveAt(index);
                    SaveLocalData();
                    GameBase.Instance.GetModule<EventManager>().Broadcast(new SocialAccountUnbindEvent(type));
                    if (mLocalData.DataList.Count > 0)
                    {
                        Debug.Log($"[SocialAccount] 解绑{type.ToString()}成功，还有绑定账号信息，不需要重新登录");
                        sucAction?.Invoke(false);
                        if (!string.IsNullOrEmpty(mSettings.unbindSucUIAddress))
                        {
                            GameBase.Instance.GetModule<DialogManager>().PopupDialog(mSettings.unbindSucUIAddress, new SocialAccountDialogData()
                            {
                                accountInfo = socialInfo,
                                isNeedChange = false,
                            });
                        }
                    }
                    else
                    {
                        Debug.Log($"[SocialAccount] 解绑{type.ToString()}成功，无其他绑定账号信息，需要游客账号登录");
                        sucAction?.Invoke(true);
                        if (!string.IsNullOrEmpty(mSettings.unbindSucChangeUIAddress))
                        {
                            GameBase.Instance.GetModule<DialogManager>().PopupDialog(mSettings.unbindSucChangeUIAddress, new SocialAccountDialogData()
                            {
                                accountInfo = socialInfo,
                                isNeedChange = true,
                                changeCallback = ChangeAccountToRestart,
                            });
                        }
                    }
                }
                else
                {
                    Debug.Log($"[SocialAccount] 解绑{type.ToString()}失败, 当前没有绑定该账号");
                    failAction?.Invoke();
                }
            }, ()=>
            {
                Debug.Log($"[SocialAccount] 解绑{type.ToString()}失败");
                failAction?.Invoke();
            });
        }

        /// <summary>
        /// 添加新渠道到本地存档
        /// </summary>
        /// <param name="accountInfo">账号信息</param>
        void AddNewSocialAccount(SocialAccountInfo accountInfo)
        {
            int index = mLocalData.DataList.FindIndex(item => item.type == accountInfo.type);
            if (index == -1)
            {
                mLocalData.DataList.Add(accountInfo);
            }
            else
            {
                mLocalData.DataList[index] = accountInfo;
            }
            playerInfo?.socialAccounts?.Clear();
            for (int i = 0; i < mLocalData.DataList.Count; i++)
            {
                playerInfo?.socialAccounts?.Add(mLocalData.DataList[i]);
            }
            archiveManager?.SetDirty(nameof(PlayerInfoRecord));
            SaveLocalData();
            GameBase.Instance.GetModule<EventManager>().Broadcast(new SocialAccountBindEvent(accountInfo));
        }
        
        /// <summary>
        /// 切换账号重新登录
        /// </summary>
        /// <param name="accountInfo">账号信息</param>
        public void ChangeAccountToRestart(SocialAccountInfo accountInfo)
        {
            mLocalData.DataList.Clear();
            if (accountInfo != null)
            {
                mLocalData.DataList.Add(accountInfo);
            }
            SaveLocalData();
            GameBase.Instance.GetModule<NetworkManager>().UploadArchives();
            archiveManager?.ClearArchive();
            GameBase.Instance.GetModule<DialogManager>().DestroyAllDialog();
            GameBase.Instance.Restart();
        }

        /// <summary>
        /// 加载本地绑定存档
        /// </summary>
        void LoadLocalData()
        {
            string localData = PlayerPrefs.GetString(LOCAL_SOCIAL_ACCOUNT_SAVEKEY, "");
            if (string.IsNullOrEmpty(localData))
            {
                mLocalData = new LocalSocialAccountData();
            }
            else
            {
                mLocalData = JsonUtil.DeserializeObject<LocalSocialAccountData>(localData);
            }
        }

        /// <summary>
        /// 保存绑定信息到本地
        /// </summary>
        void SaveLocalData()
        {
            string localData = JsonUtil.SerializeObject(mLocalData);
            PlayerPrefs.SetString(LOCAL_SOCIAL_ACCOUNT_SAVEKEY, localData);
            if (mLocalData.DataList.Count > 0)
            {
                var accountInfo = mLocalData.DataList[0];
                string localLoginData = JsonUtil.SerializeObject(accountInfo);
                PlayerPrefs.SetString(LoginProtocol.LOGIN_SOCIAL_ACCOUNT_SAVEKEY, localLoginData);
            }
            else
            {
                PlayerPrefs.SetString(LoginProtocol.LOGIN_SOCIAL_ACCOUNT_SAVEKEY, "");
            }
        }

        public Texture GetSocialAvatar()
        {
            Texture texture = null;
            
            return texture;
        }
    }
}
