/**********************************************

Copyright(c) 2020 by Me2zen
All right reserved

Author : Terrence Rao
Date : 2020-07-29 16:36:21
Ver : 1.0.0
Description :
ChangeLog :
**********************************************/

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace WordGame.Utils
{
    /// <summary>
    /// 
    /// </summary>
    public class DPHelper : MonoBehaviour
    {
        public static float DpToPx(int dp)
        {
            //.75 on ldpi(120 dpi)
            //1.0 on mdpi(160 dpi; baseline)
            //1.5 on hdpi(240 dpi)
            //2.0 on xhdpi(320 dpi)
            //3.0 on xxhdpi(480 dpi)
            //4.0 on xxxhdpi(640 dpi)

            float px = 0;
            float deviceDPI = Screen.dpi;
            //Debug.Log("deviceDPI:" + deviceDPI);
            if (Screen.dpi >= 0 && Screen.dpi < 120)
            {
                px = dp * 0.75f;
            }
            else if (Screen.dpi >= 120 && Screen.dpi < 160)
            {
                px = dp * 1.0f;
            }
            else if (Screen.dpi >= 160 && Screen.dpi < 240)
            {
                px = dp * 1.5f;
            }
            else if (Screen.dpi >= 240 && Screen.dpi < 320)
            {
                px = dp * 2.0f;
            }
            else if (Screen.dpi >= 320 && Screen.dpi < 480)
            {
                px = dp * 3f;
            }
            else if (Screen.dpi >= 480 && Screen.dpi < 640)
            {
                px = dp * 4f;
            }
            else
            {
                px = dp * 4f;
            }

            return px;
        }

        /// <summary>
        /// 获取Banner广告DP高度
        /// 广告320*50 dp
        /// </summary>
        /// <returns></returns>
        public static float GetBannerHeightPixel()
        {
#if UNITY_EDITOR
            return 150;
#endif

#if UNITY_IOS
            //iOS使用新Banner高度
            return GetBannerHeightPixelNew();
#endif

#if UNITY_ANDROID
            //Android, 只有pad使用新banner高度
            if (IsPad())
            {
                return GetBannerHeightPixelNew();
            }
            else
            {
                return GetBannerHeightPixelOriginal();
            }

#endif
        }

        public static float GetBannerHeightPixelOriginal()
        {
            float x = 0;
            if (IsPad())
            {
                x = DpToPx(90);
            }
            else
            {
                x = DpToPx(50);
            }

            //if (Application.platform == RuntimePlatform.WindowsEditor)
            //{
            //    x = 150;
            //}

            //float x = 150;

            //if (Screen.width > 750f && Screen.height > 1334f)
            //{
            float w = (float) Screen.width / 750f;
            float h = (float) Screen.height / 1334f;

            if (w > h)
            {
                x = x / h;
            }
            else
            {
                x = x / w;
            }
            //}

            //底部的安全区, 已经在SafeAdapt中处理
            if (Application.platform == RuntimePlatform.IPhonePlayer)
            {
                //float safeHeigth = screenAdaptValue * Screen.safeArea.yMin;
                //x += safeHeigth;
            }

            return x;
        }


        /// <summary>
        /// 参考WordTown 调Banner
        /// </summary>
        public static float GetBannerHeightPixelNew()
        {
            const float DefaultBannerHeightForPhone = 105; // 150/1920*1334
            float DefaultBannerHeightForPad = 126; // 180/1920*1334


            //bannerHeight
            float bannerHeight = DefaultBannerHeightForPhone;

            //Pad
            if (IsPad())
            {
                bannerHeight = DefaultBannerHeightForPad;
            }
            //Phone
            else
            {
                bannerHeight = DefaultBannerHeightForPhone;

#if UNITY_IOS
                if (IsScreenMoreThinThanDesign())
                {
                    //bannerHeight = 2 * BannerHeightInDP_Pad; //banner在顶部，特殊处理 较长设备如:iphoneX设备. 特殊处理为最高banner

                    //在iphoneX等设备上, banner高度为150, 但他的位置, 因为交互安全区域和视觉安全区域的不同, 距离安全多一些
                    bannerHeight += 24; //banner在顶部 34/1920*1334
                    //bannerHeight += 34;    //banner在底部
                }
#endif
            }

            return bannerHeight;
        }


        private static float screenAdaptValueHeight = 1334.0f / Screen.height;
        private static float screenAdaptValueWidth = 750.0f / Screen.width;

        /// <summary>
        /// 最小banner高度，小于这个高度的默认返回105
        /// </summary>
        public const float MIN_BANNER_HEIGHT = 105f;

        public static bool IsPad()
        {
            bool isPad = false;
#if UNITY_ANDROID
            float physicscreen = Mathf.Sqrt(Screen.width * Screen.width + Screen.height * Screen.height) / Screen.dpi;
            if (physicscreen >= 7f)
            {
                isPad = true;
            }
            else
            {
                isPad = false;
            }

#elif UNITY_IOS
        //Debug.Log(UnityEngine.iOS.Device.generation);
        string iP_genneration = UnityEngine.iOS.Device.generation.ToString();
        //The second Mothod: 
        //string iP_genneration=SystemInfo.deviceModel.ToString();
 
        if (iP_genneration.Substring(0, 3) == "iPa")
        {
            isPad = true;
        }
        else
        {
            isPad = false;
        }
#endif
            return isPad;
        }

        /// <summary>
        /// 判断是不是比设计分辨率更窄
        /// </summary>
        public static bool IsScreenMoreThinThanDesign()
        {
            float screenAspect = 1.0f * Screen.width / Screen.height;
            float minValueForScreenAspect = 0.005f;
            if (screenAspect < (750.0f / 1334.0f - minValueForScreenAspect))
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        /// <summary>
        /// 根据像素计算banner高度
        /// </summary>
        /// <param name="heightPixel"></param>
        /// <returns></returns>
        public static float GetBannerHeightByPixel(float heightPixel)
        {
            return heightPixel * screenAdaptValueHeight;
        }

        /// <summary>
        /// 根据像素计算banner宽度
        /// </summary>
        /// <param name="widthPixel"></param>
        /// <returns></returns>
        public static float GetBannerWidthByPixel(float widthPixel)
        {
            return widthPixel * screenAdaptValueWidth;
        }

        /*
        /// <summary>
        /// 判断用户设备是否是Pad
        /// </summary>
        /// <returns></returns>
        public bool IsPad()
        {
            bool isPad = false;
            if (BuildSettings.DistChannels == DistChannels.Apple)
            {
                string iP_genneration = UnityEngine.iOS.Device.generation.ToString();

                if (iP_genneration.Substring(0, 3) == "iPa")
                {
                    isPad = true;
                }
                else
                {
                    isPad = false;
                }
            }
            else
            {
                float physicscreen = Mathf.Sqrt(Screen.width * Screen.width + Screen.height * Screen.height) /
                                     Screen.dpi;
                if (physicscreen >= 7f)
                {
                    isPad = true;
                }
                else
                {
                    isPad = false;
                }
            }

            return isPad;
        }

        private float DpToPx(int dp)
        {
            float px = 0;
            float deviceDPI = Screen.dpi;

            if (Screen.dpi >= 0 && Screen.dpi < 120)
            {
                px = dp * 0.75f;
            }
            else if (Screen.dpi >= 120 && Screen.dpi < 160)
            {
                px = dp * 1.0f;
            }
            else if (Screen.dpi >= 160 && Screen.dpi < 240)
            {
                px = dp * 1.5f;
            }
            else if (Screen.dpi >= 240 && Screen.dpi < 320)
            {
                px = dp * 2.0f;
            }
            else if (Screen.dpi >= 320 && Screen.dpi < 480)
            {
                px = dp * 3f;
            }
            else if (Screen.dpi >= 480 && Screen.dpi < 640)
            {
                px = dp * 4f;
            }
            else
            {
                px = dp * 4f;
            }

            return px;
        }

        public float GetBannerHeightPixel()
        {
            float x;
            if (IsPad())
            {
                x = DpToPx(90); //728*90
            }
            else
            {
                x = DpToPx(50); //320*50
            }

            float w = (float) Screen.width / 750f;
            float h = (float) Screen.height / 1334f;

            if (w > h)
            {
                x = x / h;
            }
            else
            {
                x = x / w;
            }

            return x;
        }
        */
        
    }
}