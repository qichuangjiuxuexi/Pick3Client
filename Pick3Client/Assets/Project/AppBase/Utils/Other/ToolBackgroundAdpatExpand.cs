/**********************************************

Copyright(c) 2020 by Me2zen
All right reserved

Author : Terrence Rao
Date : 2020-07-25 17:13:55
Ver:1.0.0
Description :
ChangeLog :
**********************************************/

using System;
using UnityEngine;
using UnityEngine.UI;

namespace WordGame.Utils
{
    /// <summary>
    /// 背景全屏, 用于UICanvas expand模式下. 保证背景图全屏
    /// 1. thin屏, 左右切掉内容
    /// 2. fat屏, 上下切掉内容
    /// </summary>
    public class ToolBackgroundAdpatExpand : MonoBehaviour
    {
        /// <summary>
        /// 间隔时间
        /// </summary>
        private const float intervalTime = 1f;

        /// <summary>
        /// 图片默认宽度
        /// </summary>
        [SerializeField] private float imageWidth = 1024f;

        /// <summary>
        /// 图片默认高度
        /// </summary>
        [SerializeField] private float imageHeight = 1024f;

        /// <summary>
        /// 渲染组件
        /// </summary>
        //private CanvasScaler tempCanvasScaler = null;

        /// <summary>
        /// 设计宽高
        /// </summary>
        private float referenceWidth;
        private float referenceHeight;

        /// <summary>
        /// 当前节点rect
        /// </summary>
        private RectTransform bg;

        /// <summary>
        /// 计时器
        /// </summary>
        private float timer;

        // Use this for initialization
        void Start()
        {
            Init();
        }

        // Update is called once per frame
        void Update()
        {
            Check();
        }

        /// <summary>
        /// 初始化
        /// </summary>
        private void Init()
        {
            bg = transform as RectTransform;
            CanvasScaler tempCanvasScaler = transform.GetComponentInParent<CanvasScaler>();
            if (tempCanvasScaler == null)
            {
                referenceWidth = imageWidth;
                referenceHeight = imageHeight;
            }
            
            
            Debugger.LogD(tempCanvasScaler.referenceResolution);
            SetScale();
        }

        /// <summary>
        /// 检测
        /// </summary>
        private void Check()
        {
            timer += Time.deltaTime;
            if (timer > intervalTime)
            {
                SetScale();
            }
        }

        /// <summary>
        /// 设置节点
        /// </summary>
        private void SetScale()
        {
            float value = Screen.height * referenceWidth /
                          (float) (Screen.width * referenceHeight);

            if (ToolMath.floatEquals(value, 1.0f))
            {
                bg.sizeDelta = new Vector2(referenceWidth, referenceHeight);
            }
            //thin
            else if (value > 1)
            {
                value *= (referenceHeight + 2); //加两个像素, 保证不漏
                bg.sizeDelta = new Vector2(value * (imageWidth / imageHeight), value);
            }
            //fat
            else if (value < 1)
            {
                var valueByWidth = Screen.width * referenceHeight /
                                   (float) (Screen.height * referenceWidth);
                valueByWidth = (referenceWidth + 2) * valueByWidth;
                bg.sizeDelta = new Vector2(valueByWidth, valueByWidth * imageHeight / imageWidth);
            }
        }
    }
}