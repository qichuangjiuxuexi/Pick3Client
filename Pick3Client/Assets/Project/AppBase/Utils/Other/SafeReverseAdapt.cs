/**********************************************

Copyright(c) 2020 by Me2zen
All right reserved

Author : Terrence Rao
Date : 2020-07-19 20:49:02
Ver : 1.0.0
Description :
ChangeLog :
**********************************************/

using AppBase.UI;
using UnityEngine;

namespace WordGame.Utils
{
    /// <summary>
    /// 
    /// </summary>
    public class SafeReverseAdapt : MonoBehaviour
    {
        /// <summary>
        /// 当前节点
        /// </summary>
        private RectTransform rectTransform;

        /// <summary>
        /// 屏幕比例
        /// </summary>
        private static float screenAdaptValue = 1334.0f / Screen.height;

        /// <summary>
        /// 开始界面
        /// </summary>
        private void Start()
        {
            SetAdapt(SafeAdapt.IsIPhonePlayer());
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
                if (rectTransform != null)
                {
                    rectTransform.offsetMax = new Vector2(rectTransform.offsetMax.x, SafeAdapt.GetSafeAreaSize().y);
                }
            }

            if (needBottomSafe)
            {
                rectTransform.offsetMin =
                    new Vector2(rectTransform.offsetMin.x, -1 * SafeAdapt.GetSafeAreaHeightFromBottom());
            }
        }
    }
}