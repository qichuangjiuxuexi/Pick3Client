using System;
using AppBase.Sound;
using Project.AppBase.Audio;
using UnityEngine.UI;

namespace AppBase.UI.Dialog
{
    /// <summary>
    /// 对话框基类，对话框根节点挂这个脚本或其子类
    /// 打开时序：OnInit -> OnLoad(异步) -> Awake -> OnAwake -> OnStart -> OnBeforeOpenAnim -> PlayOpenAnim(异步) -> OnAfterOpenAnim
    /// 关闭时序：OnBeforeCloseAnim -> PlayCloseAnim(异步) -> OnAfterCloseAnim -> OnBeforeDestroy -> OnDestroy
    /// </summary>
    public class UIDialog : UIView
    {
        /// <summary>
        /// 对话框数据
        /// </summary>
        public DialogData dialogData;

        /// <summary>
        /// 对话框父节点容器
        /// </summary>
        public DialogContainer dialogContainer;

        /// <summary>
        /// 关闭按钮
        /// </summary>
        public Button closeBtn;

        #region 生命周期

        /// <summary>
        /// dialogData初始化完成后调用，注意此时isActive = false
        /// </summary>
        protected virtual void OnInit()
        {
        }
        
        /// <summary>
        /// 当对话框被加载时调用，用来加载对话框其他资源，加载完成后调用callback返回，注意此时isActive = false
        /// </summary>
        public virtual void OnLoad(Action callback)
        {
            callback?.Invoke();
        }

        /// <summary>
        /// 对话框被创建时调用
        /// </summary>
        public virtual void OnAwake()
        {
        }

        /// <summary>
        /// 对话框被打开时调用，用于执行动画
        /// </summary>
        public virtual void OnStart()
        {
        }

        /// <summary>
        /// 播放打开对话框动画前调用
        /// </summary>
        protected virtual void OnBeforeOpenAnim()
        {
        }

        /// <summary>
        /// 播放打开对话框动画完成后调用
        /// </summary>
        protected virtual void OnAfterOpenAnim()
        {
        }

        /// <summary>
        /// 播放关闭对话框动画前调用
        /// </summary>
        protected virtual void OnBeforeCloseAnim()
        {
        }

        /// <summary>
        /// 播放关闭对话框动画完成后调用
        /// </summary>
        protected virtual void OnAfterCloseAnim()
        {
        }

        /// <summary>
        /// 在对话框销毁前调用
        /// </summary>
        public virtual void OnBeforeDestroy()
        {
        }

        #endregion

        /// <summary>
        /// 绑定对话框的数据
        /// </summary>
        public void Init(DialogData dialogData)
        {
            this.dialogData = dialogData;
            dialogData?.OnLoadedCallback(this);
            OnInit();
        }

        /// <summary>
        /// 绑定默认控件，比如关闭按钮
        /// </summary>
        public virtual void OnBindComponents()
        {
            if (closeBtn == null)
            {
                closeBtn = transform.Finds("CloseBtn", "UI/CloseBtn")?.GetComponent<Button>();
            }
            if (closeBtn != null)
            {
                closeBtn.AddListener(OnCloseClicked);
            }
        }

        /// <summary>
        /// 播放打开对话框动画
        /// </summary>
        public virtual void PlayOpenAnim(Action callback)
        {
            OnBeforeOpenAnim();
            if (string.IsNullOrEmpty(dialogData.openAnimName))
            {
                GameBase.Instance.GetModule<DialogManager>().PlayColorMaskOpenAnim(this, 0);
                OnAfterOpenAnim();
                callback?.Invoke();
            }
            else
            {
                var duration = transform.PlayAnimatorUpdate(dialogData.openAnimName);
                GameBase.Instance.GetModule<DialogManager>().PlayColorMaskOpenAnim(this, duration);
                if (duration > 0)
                {
                    this.DelayCall(duration, () =>
                    {
                        OnAfterOpenAnim();
                        callback?.Invoke();
                    }, true);
                }
                else
                {
                    OnAfterOpenAnim();
                    callback?.Invoke();
                }
            }
        }

        /// <summary>
        /// 用户点击关闭按钮
        /// </summary>
        public virtual void OnCloseClicked()
        {
            if (closeBtn != null && closeBtn.interactable == false) return;
            GameBase.Instance.GetModule<SoundManager>().PlayAudio(DefaultSoundNameConst.CLICK);
            CloseDialog();
        }

        /// <summary>
        /// 关闭对话框
        /// </summary>
        public void CloseDialog()
        {
            PlayCloseAnim(DestroyDialog);
        }

        /// <summary>
        /// 播放关闭对话框动画
        /// </summary>
        public virtual void PlayCloseAnim(Action callback)
        {
            OnBeforeCloseAnim();
            dialogContainer.SetTouchEnabled(false);
            if (string.IsNullOrEmpty(dialogData.closeAnimName))
            {
                GameBase.Instance.GetModule<DialogManager>().PlayColorMaskCloseAnim(this, 0);
                OnAfterCloseAnim();
                callback?.Invoke();
            }
            else
            {
                var duration = transform.PlayAnimatorUpdate(dialogData.closeAnimName);
                GameBase.Instance.GetModule<DialogManager>().PlayColorMaskCloseAnim(this, duration);
                if (duration > 0)
                {
                    this.DelayCall(duration, () =>
                    {
                        OnAfterCloseAnim();
                        callback?.Invoke();
                    }, true);
                }
                else
                {
                    OnAfterCloseAnim();
                    callback?.Invoke();
                }
            }
        }

        /// <summary>
        /// 直接销毁对话框
        /// </summary>
        public void DestroyDialog()
        {
            GameBase.Instance.GetModule<DialogManager>().DestroyDialog(this);
        }

        /// <summary>
        /// 设置是否可以点击对话框
        /// </summary>
        public void SetTouchEnabled(bool enabled)
        {
            dialogContainer.SetTouchEnabled(enabled);
        }
    }
}
