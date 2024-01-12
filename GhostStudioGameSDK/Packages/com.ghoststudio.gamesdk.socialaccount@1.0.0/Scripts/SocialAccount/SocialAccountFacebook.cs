using System;
using System.Collections;
using System.Collections.Generic;
using AppBase.PlayerInfo;
using AppBase.Timing;
using AppBase.Utils;
using Facebook.Unity;
using UnityEngine;
using UnityEngine.Networking;

namespace AppBase.SocialAccount
{
    public class FacebookIconJson
    {
        public int width = 0;
        public int height = 0;
        public bool is_silhouette = false;
        public string url = "";
    }
    
    public class FacebookIconJsonWrapper
    {
        public FacebookIconJson data = null;
    }
    
    public class FacebookLoginData
    {
        public string id = "";
        public string name = "";
        public string email = "";
        public FacebookIconJsonWrapper picture = null;
    }
    
    public class SocialAccountFacebook : SocialAccountBase
    {
        private const int FACEBOOK_ICON_SIZE = 256;
        
        // 社交平台类型
        public override SocialAccountType Type => SocialAccountType.Facebook;

        public override void Login(Action<SocialAccountInfo> successCallback, Action failedCallback)
        {
#if UNITY_EDITOR
            SocialAccountInfo info = new SocialAccountInfo();
            info.id = "FB002";
            info.name = "TEST_FB_001";
            info.type = (int)Type;
            info.email = "test_dev@test.com";
            successCallback?.Invoke(info);
            return;
#endif
            if (FB.IsInitialized)
            {
                FB.LogInWithReadPermissions(new List<string>{"public_profile, email"}, result =>
                {
                    if (FB.IsLoggedIn) 
                    {
                        var accessToken = AccessToken.CurrentAccessToken;
                        if (accessToken == null)
                        {
                            Debug.LogError("[SocialAccount] 登陆Facebook获取Token失败...");
                            failedCallback?.Invoke();
                            return;
                        }
                        Debug.Log("[SocialAccount] 获取登陆信息: UserId:" + accessToken.UserId + " TokenString:" + accessToken.TokenString);
                        foreach (var data in accessToken.Permissions)
                        {
                            Debug.Log($"[SocialAccount] Permissions数据：{data}");
                        }

                        string graphApiLink = BuildFacebookGraphApiLink(accessToken.UserId, accessToken.TokenString);
                        Debug.Log($"[SocialAccount] 头像获取链接地址：{graphApiLink}");
                        FB.API("me?fields=name", HttpMethod.GET, result =>
                        {
                            if (string.IsNullOrEmpty(result.Error))
                            {
                                failedCallback?.Invoke();
                            }
                            else
                            {
                                Debug.Log($"[SocialAccount] 获取名字返回：{result.RawResult}");
                            }
                        });
                        GameBase.Instance.GetModule<TimingManager>().StartCoroutine(GetFacebookInfoByLink(graphApiLink, successCallback, failedCallback));
                    }
                    else
                    {
                        failedCallback?.Invoke();
                    }
                });
            }
            else
            {
                Debug.Log("[SocialAccount] FB未被初始化成功");
                failedCallback?.Invoke();
            }
        }

        public override void Logout(Action successCallback, Action failedCallback)
        {
            successCallback?.Invoke();
        }
        
        // 生成获取facebook信息的Get请求链接
        private string BuildFacebookGraphApiLink(string facebookUserId, string accessToken)
        {
            if (string.IsNullOrEmpty(facebookUserId) || string.IsNullOrEmpty(accessToken))
            {
                return "";
            }
            
            var graphApiUrl = $"https://graph.facebook.com/{facebookUserId}?fields=id,name,picture.width({FACEBOOK_ICON_SIZE}).height({FACEBOOK_ICON_SIZE})&access_token={accessToken}";
            return graphApiUrl;
        }
        
        // 由GraphApiUrl获取Facebook信息 => id，名字，头像链接
        private IEnumerator GetFacebookInfoByLink(string url, Action<SocialAccountInfo> successCallback, Action failedCallback)
        {
            Debug.Log("[SocialAccount] Facebook请求Graph Link:" + url);
            UnityWebRequest request = UnityWebRequest.Get(url);
            yield return request.SendWebRequest();
            if(request.isHttpError || request.isNetworkError)
            {
                Debug.LogError("[SocialAccount] 请求Facebook信息出错: " + request.error);
                failedCallback?.Invoke();
            }
            else
            {
                string receiveContent = request.downloadHandler.text;
                Debug.Log("[SocialAccount] 请求Facebook信息返回: " + receiveContent);

                if (!string.IsNullOrEmpty(receiveContent))
                {
                    var facebookLoginData = JsonUtil.DeserializeObject<FacebookLoginData>(receiveContent);
                    SocialAccountInfo saInfo = new SocialAccountInfo();
                    saInfo.id = facebookLoginData.id;
                    saInfo.name = facebookLoginData.name;
                    saInfo.email = facebookLoginData.email;
                    saInfo.avatar = facebookLoginData.picture.data.url;
                    saInfo.type = (int)Type;
                    successCallback?.Invoke(saInfo);
                }
                else
                {
                    failedCallback?.Invoke();
                }
            }
        }
    }
}