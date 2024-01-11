/**********************************************

Copyright(c) 2020 by Me2zen
All right reserved

Author : Terrence Rao
Date : 2020-07-25 17:13:55
Ver:1.0.0
Description :
ChangeLog :
**********************************************/

using UnityEngine;
using UnityEngine.UI;

namespace WordGame.Utils
{
    /// <summary>
    /// 背景按钮，用于UICanvas Width Or Height模式下. 保证背景图全屏
    /// 1. 图提供3:4的比例
    /// 2. thin屏, 左右切掉内容
    /// 3. 16:9的屏, 左右也要切掉部分内容
    /// 4. fat 屏, 能显示全部内容.
    /// </summary>
    public class ToolBackgroundAdpat : MonoBehaviour
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
        [SerializeField] private float imageBaseWidth = 1024f;

        /// <summary>
        /// 渲染组件
        /// </summary>
        private CanvasScaler tempCanvasScaler = null;

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
            tempCanvasScaler = transform.GetComponentInParent<CanvasScaler>();
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
            float value = Screen.height * tempCanvasScaler.referenceResolution.x /
                          (float) (Screen.width * tempCanvasScaler.referenceResolution.y);

            if (value < 1)
            {
                value = 1;
            }

            value *= (tempCanvasScaler.referenceResolution.y + 10);

            bg.sizeDelta = new Vector2(value * (imageWidth / imageBaseWidth), value);
        }
    }
}