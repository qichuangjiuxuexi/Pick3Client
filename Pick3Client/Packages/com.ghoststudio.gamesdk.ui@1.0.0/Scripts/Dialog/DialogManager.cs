using System;
using AppBase.Event;
using AppBase.Module;
using AppBase.Resource;
using Unified.Universal.Blur;
using UnityEngine;

namespace AppBase.UI.Dialog
{
    /// <summary>
    /// 对话框管理器
    /// </summary>
    public class DialogManager : MonoModule
    {
        public override string GameObjectPath => "UICanvas/Dialogs";
        public bool IsHaveDialog => GameObject.transform.childCount > 0;

        /// <summary>
        /// 弹出对话框
        /// </summary>
        /// <param name="address">对话框地址</param>
        /// <param name="data">用户数据</param>
        /// <param name="loadedCallback">加载完成后，弹出之前的回调</param>
        /// <param name="closeCallback">关闭动画播放结束后的回调</param>
        public DialogData PopupDialog(string address, object data = null, Action<UIDialog> loadedCallback = null, Action<UIDialog> closeCallback = null)
        {
            if (string.IsNullOrEmpty(address)) return null;
            return PopupDialog(new DialogData(address, data, loadedCallback, closeCallback));
        }
        
        /// <summary>
        /// 弹出对话框
        /// </summary>
        /// <param name="dialogData">对话框数据</param>
        /// <returns>对话框数据</returns>
        public DialogData PopupDialog(DialogData dialogData)
        {
            if (dialogData == null || string.IsNullOrEmpty(dialogData.address)) return dialogData;

            GameBase.Instance.GetModule<EventManager>().Broadcast(new BeforeDialogPopEvent(dialogData));
            
            //底部遮罩
            var container = DialogContainer.Create(GameObject, dialogData.address);

            //加载prefab
            dialogData.handler = GameBase.Instance.GetModule<ResourceManager>().LoadAssetHandler<GameObject>(dialogData.address, handler =>
            {
                //对话框已销毁
                if (container.dialogAddress == null)
                {
                    dialogData.handler = null;
                    handler.Release();
                    return;
                }

                //实例化prefab
                var prefab = handler.GetAsset<GameObject>();
                prefab.SetActive(false);
                var dialog = container.AddUIDialog(prefab);
                //先初始化dialogData 
                dialog.Init(dialogData);
                dialog.GetResourceReference().AddHandler(handler);

                //对话框加载回调
                dialog.OnLoad(() =>
                {
                    //对话框遮罩
                    var previousDialog = GetPreviousDialogContainer(dialog, true);
                    if (previousDialog != null && previousDialog.dialog.dialogData.isBlurBgMask)
                    {
                        //如果前一个界面是毛玻璃效果 则新覆盖的界面保持
                        dialogData.isBlurBgMask = true;
                    }
                    
                    container.AddColorMask(dialogData);
                    container.AddTouchMask();

                    //弹出
                    dialog.GetOrAddComponent<DialogRuntimeComponent>();
                    handler.Release();
                    dialog.SetActive(true);
                    GameBase.Instance.GetModule<EventManager>().Broadcast(new AfterDialogPopEvent(dialogData));
                });
            }, () =>
            {
                //加载失败
                container.dialogAddress = null;
                dialogData.handler = null;
                GameObject.Destroy(container);
                Debugger.LogError(TAG, $"ShowDialog failed，address={dialogData.address}");
            });
            return dialogData;
        }

        /// <summary>
        /// 查找对话框
        /// </summary>
        public UIDialog FindDialog(string address)
        {
            for (int i = Transform.childCount - 1; i >= 0; i--)
            {
                var dialogContainer = Transform.GetChild(i)?.GetComponent<DialogContainer>();
                if (dialogContainer == null) continue;
                if (dialogContainer.dialogAddress == address)
                {
                    return dialogContainer.dialog;
                }
            }
            return null;
        }

        /// <summary>
        /// 关闭对话框
        /// </summary>
        public UIDialog CloseDialog(string address)
        {
            for (int i = Transform.childCount - 1; i >= 0; i--)
            {
                var dialogContainer = Transform.GetChild(i)?.GetComponent<DialogContainer>();
                if (dialogContainer == null) continue;
                if (dialogContainer.dialogAddress == address)
                {
                    if (dialogContainer.dialog != null)
                    {
                        dialogContainer.dialog.CloseDialog();
                    }
                    else
                    {
                        //对话框加载中，标记已销毁
                        dialogContainer.dialogAddress = null;
                        GameObject.Destroy(dialogContainer.gameObject);
                    }
                    return dialogContainer.dialog;
                }
            }
            return null;
        }

        /// <summary>
        /// 直接销毁对话框
        /// </summary>
        public void DestroyDialog(UIDialog dialog)
        {
            if (dialog == null || dialog.dialogContainer == null) return;
            var container = dialog.dialogContainer;
            if (container.dialogAddress == null) return;
            string address = dialog.dialogData.address;
            container.dialogAddress = null;
            dialog.OnBeforeDestroy();
            if (dialog.dialogData.isBlurBgMask && UniversalBlurFeature.Instance)
            {
                UniversalBlurFeature.Instance.Deactivate();
            }
            dialog.dialogData?.OnCloseCallback(dialog);
            container.dialog = null;
            dialog.dialogContainer = null;
            GameObject.Destroy(container.gameObject);
            GameBase.Instance.GetModule<EventManager>().Broadcast(new OnDialogDestroyEvent(address));
        }
        
        /// <summary>
        /// 关闭所有对话框，不播放关闭动画
        /// </summary>
        public void DestroyAllDialog()
        {
            for (int i = Transform.childCount - 1; i >= 0; i--)
            {
                var dialogContainer = Transform.GetChild(i)?.GetComponent<DialogContainer>();
                if (dialogContainer == null) continue;
                if (dialogContainer.dialog != null)
                {
                    DestroyDialog(dialogContainer.dialog);
                }
                else
                {
                    //对话框加载中，标记已销毁
                    dialogContainer.dialogAddress = null;
                    GameObject.Destroy(dialogContainer.gameObject);
                }
            }
        }

        //是否是最前面的对话框
        protected bool IsTopDialog(UIDialog dialog)
        {
            if (Transform.childCount == 0 || dialog == null) return false;
            return Transform.GetChild(Transform.childCount - 1).GetComponent<DialogContainer>()?.dialog == dialog;
        }

        //获取前一个对话框的容器
        protected DialogContainer GetPreviousDialogContainer(UIDialog dialog, bool colorMasked)
        {
            bool foundDialog = false;
            for (int i = Transform.childCount - 1; i >= 0; i--)
            {
                var container = Transform.GetChild(i).GetComponent<DialogContainer>();
                if (container == null || container.dialog == null || container.dialogAddress == null) continue;
                if (colorMasked && container.colorMask && container.colorMask.color == Color.clear) continue;
                if (foundDialog) return container;
                if (container.dialog == dialog) foundDialog = true;
            }
            return null;
        }

        /// <summary>
        /// 播放灰遮罩出现动画
        /// </summary>
        public void PlayColorMaskOpenAnim(UIDialog dialog, float duration)
        {
            if (!IsTopDialog(dialog)) return;
            var previousDialog = GetPreviousDialogContainer(dialog, true);
            dialog.dialogContainer.PlayColorMaskOpenAnim(dialog.dialogData.BgMaskColor, duration, previousDialog);
        }

        /// <summary>
        /// 播放灰遮罩消失动画
        /// </summary>
        public void PlayColorMaskCloseAnim(UIDialog dialog, float duration)
        {
            void func()
            {
                if (!IsTopDialog(dialog)) return;
                var previousDialog = GetPreviousDialogContainer(dialog, false);
                dialog.dialogContainer.PlayColorMaskCloseAnim(duration, previousDialog);
            }
            if (duration == 0)
            {
                func();
            }
            else
            {
                dialog.DelayCallFrame(1, func);
            }
        }
    }
}