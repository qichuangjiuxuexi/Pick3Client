using System;
using System.Collections;
using System.Collections.Generic;
using AppBase.Resource;
using UnityEngine;

namespace AppBase.UI.Dialog
{
    /// <summary>
    /// 对话框数据
    /// </summary>
    public class DialogData : DialogContext, IEnumerator
    {
        /// <summary>
        /// 地址
        /// </summary>
        public string address;

        /// <summary>
        /// 用户数据
        /// </summary>
        public object data;
        
        /// <summary>
        /// 上下文数据
        /// </summary>
        public List<DialogContext> context = new ();

        /// <summary>
        /// 加载完成后，弹出之前的回调
        /// </summary>
        public event Action<UIDialog> loadedCallback;
        public override void OnLoadedCallback(UIDialog obj)
        {
            loadedCallback?.Invoke(obj);
            loadedCallback = null;
            context?.ForEach(c => c?.OnLoadedCallback(obj));
        }

        /// <summary>
        /// 打开动画播放完成后的回调
        /// </summary>
        public event Action<UIDialog> openCallback;
        public override void OnOpenCallback(UIDialog obj)
        {
            openCallback?.Invoke(obj);
            openCallback = null;
            context?.ForEach(c => c?.OnOpenCallback(obj));
        }

        /// <summary>
        /// 关闭动画播放结束后的回调
        /// </summary>
        public event Action<UIDialog> closeCallback;
        public override void OnCloseCallback(UIDialog obj)
        {
            closeCallback?.Invoke(obj);
            closeCallback = null;
            context?.ForEach(c => c?.OnCloseCallback(obj));
            handler = null;
        }

        /// <summary>
        /// 打开动画名称
        /// </summary>
        public string openAnimName = "Open";

        /// <summary>
        /// 关闭动画名称
        /// </summary>
        public string closeAnimName = AppBaseProjectConst.DefaultDialogCloseAnimName;

        public Color BgMaskColor
        {
            get
            {
                return isBlurBgMask ? blurBgMaskColor : bgMaskColor;
            }
            set
            {
                if (isBlurBgMask)
                {
                    blurBgMaskColor = value;
                }
                else
                {
                    bgMaskColor = value;
                }
            }
        }
        /// <summary>
        /// 背景遮罩颜色
        /// </summary>
        public Color bgMaskColor = new Color(0, 0, 0, AppBaseProjectConst.DefaultDialogBgMaskAlpha);

        /// <summary>
        /// 是否毛玻璃遮罩
        /// </summary>
        public bool isBlurBgMask = false;

        /// <summary>
        /// 毛玻璃背景遮罩颜色
        /// </summary>
        public Color blurBgMaskColor = new Color(82 / 255f, 82 / 255f, 82 / 255f, 1);

        /// <summary>
        /// 是否点击背景遮罩关闭对话框
        /// </summary>
        public bool isClickBgMaskClose = true;
        

        /// <summary>
        /// 对话框数据
        /// </summary>
        public DialogData()
        {
        }

        /// <summary>
        /// 对话框数据
        /// </summary>
        /// <param name="address">对话框地址</param>
        /// <param name="data">用户数据</param>
        /// <param name="loadedCallback">加载完成后，弹出之前的回调</param>
        /// <param name="closeCallback">关闭动画播放结束后的回调</param>
        public DialogData(string address, object data = null, Action<UIDialog> loadedCallback = null, Action<UIDialog> closeCallback = null)
        {
            this.address = address;
            this.data = data;
            if (loadedCallback != null) this.loadedCallback += loadedCallback;
            if (closeCallback != null) this.closeCallback += closeCallback;
        }

        #region 协程相关

        /// <summary>
        /// 资源加载器，加载失败或关闭对话框后，会被置空
        /// </summary>
        public ResourceHandler handler { get; internal set; }
        
        /// <summary>
        /// 协程等待直到对话框关闭
        /// </summary>
        public virtual bool MoveNext() => handler != null;
        public object Current => handler?.Current;
        public virtual void Reset() {}
        
        #endregion
    }
}
