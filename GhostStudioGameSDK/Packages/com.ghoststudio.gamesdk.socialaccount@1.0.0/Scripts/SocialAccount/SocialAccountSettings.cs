using System.Collections.Generic;

namespace AppBase.SocialAccount
{
    public class SocialAccountSettings
    {
        // 接入的社交账号类型
        public List<SocialAccountType> types;
        // 绑定成功弹板地址
        public string bindSucUIAddress;
        // 绑定成功需要切换账号弹板地址
        public string bindSucChangeUIAddress;
        // 绑定失败弹板地址
        public string bindFailUIAddress;
        // 解绑成功弹板地址
        public string unbindSucUIAddress;
        // 解绑成功需要切换账号弹板地址
        public string unbindSucChangeUIAddress;
        // 顶号提示弹板地址
        public string otherLoginUIAddress;

        /// <summary>
        /// 初始化第三方账号绑定设置面板
        /// </summary>
        /// <param name="types">接入的渠道列表</param>
        /// <param name="bindSucAddress">绑定成功弹板地址</param>
        /// <param name="bindSucChangeAddress">绑定成功需要切换账号弹板地址</param>
        /// <param name="bindFailAddress">绑定失败弹板地址</param>
        /// <param name="unbindTipsAddress">解绑成功弹板地址</param>
        /// <param name="unbindChangeTipsAddress">解绑成功需要切换账号弹板地址</param>
        /// <param name="otherLoginTipsAddress">顶号提示弹板地址</param>
        public SocialAccountSettings(List<SocialAccountType> types, string bindSucAddress = ""
            , string bindSucChangeAddress = "", string bindFailAddress = "", string unbindTipsAddress = ""
            , string unbindChangeTipsAddress = "", string otherLoginTipsAddress = "")
        {
            this.types = types;
            bindSucUIAddress = bindSucAddress;
            bindSucChangeUIAddress = bindSucChangeAddress;
            bindFailUIAddress = bindFailAddress;
            unbindSucUIAddress = unbindTipsAddress;
            unbindSucChangeUIAddress = unbindChangeTipsAddress;
            otherLoginUIAddress = otherLoginTipsAddress;
        }
    }
}