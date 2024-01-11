/**********************************************

Copyright(c) 2020 by Me2zen
All right reserved

Author : Terrence Rao
Date : 2020-07-30 17:34:36
Ver:1.0.0
Description :


iPhoneX 设备        
    //苹果要求的交互安全区域 Top: 44pt(132px, 126px_indesigin) Bottom: 34pt(102px, 98px_indesign), 这也是banner出现的位置
    //而游戏中, 用到的是可视位置, 视觉上比较理想的是. Top:84px, Bottom:68px
    //Rect saveRect = Me2zen.Utils.SafeAreaUtils.GetSafeArea();
    s_toTop = 61;       //(132 由弹出banner的位置测算出来)
    //s_toBottom = 68 ;   //(banner在上方时, 68  视觉上比较好看的位置)
    s_toBottom = 68 ;   //(banner在下方时, 98 由弹出banner的位置测算出来)

iPad Pro 设备
    ScreenInfo Screen<2048,2732>, SafeArea<0,40,2048,2732> Screen.dpi:264 SafeArea:<0,40,2048,2732>
    底的安全区为 40. 在750x1334设计分辨率下为17 

ChangeLog :
**********************************************/

using UnityEngine;
using UnityEngine.UI;

namespace AppBase.UI
{

    public class ScreenInfo
    {
        /// <summary>
        /// 设计分辨率宽
        /// </summary>
        public const float DESIGN_SCREEN_WIDTH = 1080;
        
        /// <summary>
        /// 4:3屏幕, 实际的屏幕宽
        /// </summary>
        public const float MAX_SCREEN_WIDTH = 1000;

        /// <summary>
        /// 设计分辨率高
        /// </summary>
        public const float DESIGN_SCREEN_HEIGHT = 1920;

        /// <summary>
        /// 宽高比, 极小差异
        /// </summary>
        public const float MIN_VALUE_FOR_SCREEN_ASPECT = 0.005f;
    }
    
    /// <summary>
    /// 安全区适配脚本
    /// </summary>
    public class SafeAdapt : MonoBehaviour
    {
        public bool forceNoBottom;
        /// <summary>
        /// 当前节点
        /// </summary>
        private RectTransform rectTransform;

        /// <summary>
        /// 屏幕比例
        /// </summary>
        private static float screenAdaptValue = 1;

        private static CanvasScaler scaler = null;
        private static RectTransform scalerRectTransform = null;

        ///// <summary>
        ///// iphoneX 屏幕的安全区
        ///// </summary>
        //public const float TO_TOP_FOR_IPHONEX = 61f;

        //public const float TO_BOTTOM_FOR_IPHONEX = 68f;

        /// <summary>
        /// 开始界面
        /// </summary>
        private void Start()
        {
            Debugger.LogDWarningFormat(
                "ScreenInfo Screen<{0},{1}>, SafeArea<{2},{3},{4},{5}> Screen.dpi:{6} SafeArea:<{7},{8},{9},{10}>",
                Screen.width
                , Screen.height
                , Screen.safeArea.min.x, Screen.safeArea.min.y, Screen.safeArea.max.x,
                Screen.safeArea.max.y
                , Screen.dpi
                , Screen.safeArea.x, Screen.safeArea.y, Screen.width, Screen.height
            );

            scaler = transform.GetComponentInParent<CanvasScaler>();
            if (scaler != null)
            {
                scalerRectTransform = scaler.transform as RectTransform;
            }

            //窄屏由宽决定
            if ((Screen.height / (float) Screen.width) >
                scaler.referenceResolution.y / scaler.referenceResolution.x) //窄屏
            {
                screenAdaptValue = scaler.referenceResolution.x / Screen.width;
            }
            //宽屏由高决定
            else
            {
                screenAdaptValue = scaler.referenceResolution.y / Screen.height;
            }

            SetAdapt(IsIPhonePlayer());
        }

        /// <summary>
        /// 适配界面
        /// </summary>
        /// <param name="needBottomSafe"></param>
        private void SetAdapt(bool needBottomSafe = false)
        {
            if (rectTransform == null)
            {
                rectTransform = gameObject.GetComponent<RectTransform>();
                rectTransform.anchorMin = Vector2.zero;
                rectTransform.anchorMax = Vector2.one;
               // rectTransform.offsetMin = rectTransform.offsetMax = Vector2.zero;
                if (rectTransform != null)
                {
                    rectTransform.offsetMax = new Vector2(rectTransform.offsetMax.x, rectTransform.offsetMax.y -1 * GetSafeAreaSize().y);
                }
            }

            if (!forceNoBottom && needBottomSafe)
            {
                rectTransform.offsetMin = new Vector2(0, rectTransform.offsetMin.y+GetSafeAreaHeightFromBottom());
            }
            
            Debugger.LogDWarningFormat("offset min:{0}; offset max{1}", rectTransform.offsetMin,
                rectTransform.offsetMax);
        }

        /// 获取安全区高度
        /// </summary>
        /// <returns></returns>
        public static Vector2 GetSafeAreaSize()
        {
            float size = (Screen.height - Screen.safeArea.yMax);
            // size = Mathf.Max(size, Screen.safeArea.yMin);

            float safeHeigth = screenAdaptValue * size;
            float safeWidth = screenAdaptValue * Screen.width;
            // if (IsIPhonePlayer())
            // {
            //     safeHeigth += safeHeigth > 3 ? -(size * 0.3f) : 0;
            // }
#if UNITY_EDITOR
            //return 50;

            /// <summary>
#endif
            return new Vector2(safeWidth, safeHeigth);
        }


        public static float GetSafeAreaHeightFromBottom()
        {
            if (IsIPhonePlayer())
                return Screen.safeArea.min.y * screenAdaptValue * 1.05f;
            return 0f;

            //float result = 0f;
            //if (IsIPhonePlayer())
            //{
            //    if (IsScreenMoreThinThanDesign())
            //    {
            //        result = TO_BOTTOM_FOR_IPHONEX;
            //    }
            //}

            //return result;
        }

        /// <summary>
        /// 判断是不是比设计分辨率更窄
        /// </summary>
        private static bool IsScreenMoreThinThanDesign()
        {
            if (Screen.height / (float) Screen.width >
                ScreenInfo.DESIGN_SCREEN_HEIGHT / ScreenInfo.DESIGN_SCREEN_WIDTH +
                 ScreenInfo.MIN_VALUE_FOR_SCREEN_ASPECT) //窄屏
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        private static bool IsScreenMoreThickThanDesign()
        {
            if (Screen.height / (float) Screen.width <
                ScreenInfo.DESIGN_SCREEN_HEIGHT / ScreenInfo.DESIGN_SCREEN_WIDTH +
                ScreenInfo.MIN_VALUE_FOR_SCREEN_ASPECT) //窄屏
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        public static bool IsIPhonePlayer()
        {
            return Application.platform == RuntimePlatform.IPhonePlayer;
            //return true;
        }

        /// <summary>
        /// 真正设备上Canvas的高
        /// </summary>
        /// <returns></returns>
        public static float GetDeviceCanvasHeight()
        {
            if (scaler != null)
            {
                return scalerRectTransform.sizeDelta.y;
            }
            else
            {
                float screenAspect = 1.0f * Screen.width / Screen.height;
                if (IsScreenMoreThinThanDesign())
                {
                    return 1.0f * ScreenInfo.DESIGN_SCREEN_HEIGHT * (1.0f * Screen.height / Screen.width) /
                           (ScreenInfo.DESIGN_SCREEN_HEIGHT / ScreenInfo.DESIGN_SCREEN_WIDTH);
                }
                else
                {
                    return ScreenInfo.DESIGN_SCREEN_HEIGHT;
                }
            }
        }

        /// <summary>
        /// 真正设备上Canvas的宽
        /// </summary>
        /// <returns></returns>
        public static float GetDeviceCanvasWidth()
        {
            if (scaler != null)
            {
                return scalerRectTransform.sizeDelta.x;
            }
            else
            {
                float screenAspect = 1.0f * Screen.width / Screen.height;
                if (IsScreenMoreThickThanDesign())
                {
                    return 1.0f * ScreenInfo.DESIGN_SCREEN_WIDTH * (1.0f * Screen.width / Screen.height) /
                           (ScreenInfo.DESIGN_SCREEN_WIDTH / ScreenInfo.DESIGN_SCREEN_HEIGHT);
                }
                else
                {
                    return ScreenInfo.DESIGN_SCREEN_WIDTH;
                }
            }
        }
    }
}