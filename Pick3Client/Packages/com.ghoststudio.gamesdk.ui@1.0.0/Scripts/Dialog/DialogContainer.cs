using System;
using DG.Tweening;
using Unified.Universal.Blur;
using UnityEngine;
using UnityEngine.UI;

namespace AppBase.UI.Dialog
{
    /// <summary>
    /// 对话框父节点容器
    /// </summary>
    public class DialogContainer : MonoBehaviour
    {
        /// <summary>
        /// 对话框结点
        /// </summary>
        public UIDialog dialog;

        /// <summary>
        /// 底部遮罩结点
        /// </summary>
        public Graphic colorMask;

        /// <summary>
        /// 透明的点击遮罩结点
        /// </summary>
        public GameObject touchMask;

        /// <summary>
        /// 对话框地址
        /// </summary>
        public string dialogAddress;

        //ColorMask动画打断使用
        protected Action colorMaskCloseCallback;

        /// <summary>
        /// 创建对话框容器
        /// </summary>
        /// <param name="parent">根节点</param>
        /// <param name="address">地址</param>
        /// <returns></returns>
        public static DialogContainer Create(GameObject parent, string address)
        {
            var container = parent.AddGameObject(address);
            container.AddFullScreenRectTransform();
            var maskImg = container.AddComponent<Image>();
            maskImg.color = Color.clear;
            maskImg.isMaskingGraphic = true;
            var dialogContainer = container.AddComponent<DialogContainer>();
            dialogContainer.dialogAddress = address;
            return dialogContainer;
        }

        /// <summary>
        /// 创建对话框结点
        /// </summary>
        /// <param name="prefab">对话框对象</param>
        /// <returns>对话框结点</returns>
        public UIDialog AddUIDialog(GameObject prefab)
        {
            var dialogParent = gameObject.AddGameObject("Dialog");
            dialogParent.AddFullScreenRectTransform();
            var obj = dialogParent.AddInstantiate(prefab);
            dialog = obj.GetOrAddComponent<UIDialog>();
            dialog.dialogContainer = this;
            return dialog;
        }

        /// <summary>
        /// 创建底部遮罩结点
        /// </summary>
        /// <param name="dialogData">对话框数据</param>
        public void AddColorMask(DialogData dialogData)
        {
            var colorMaskObj = gameObject.AddGameObject("ColorMask");
            colorMaskObj.transform.SetAsFirstSibling();
            colorMaskObj.AddFullScreenRectTransform();
            if (dialogData.isBlurBgMask && UniversalBlurFeature.Instance)
            {
                colorMask = colorMaskObj.AddComponent<RawImage>();
                colorMask.material = UniversalBlurFeature.Instance.outMaterial;
                colorMask.color = dialogData.blurBgMaskColor;
                UniversalBlurFeature.Instance.Activate();
            }
            else
            {
                colorMask = colorMaskObj.AddComponent<Image>();
                colorMask.color = dialogData.bgMaskColor;
            }
            
            if (colorMask is Image image)
            {
                image.isMaskingGraphic = false;
            }
            else if (colorMask is RawImage rawImage)
            {
                rawImage.isMaskingGraphic = false;
            }
            
            if (dialogData.isClickBgMaskClose)
            {
                var maskBtn = colorMaskObj.AddComponent<Button>();
                maskBtn.transition = Selectable.Transition.None;
                maskBtn.onClick.AddListener(dialog.OnCloseClicked);
            }
        }

        /// <summary>
        /// 创建透明的点击遮罩结点
        /// </summary>
        public void AddTouchMask()
        {
            if (transform.Find("TouchMask")) return;
            touchMask = gameObject.AddGameObject("TouchMask");
            touchMask.transform.SetAsLastSibling();
            touchMask.AddFullScreenRectTransform();
            var touchMaskImage = touchMask.AddComponent<Image>();
            touchMaskImage.color = Color.clear;
            touchMaskImage.isMaskingGraphic = true;
            touchMask.SetActive(false);
        }

        /// <summary>
        /// 播放灰遮罩出现动画
        /// </summary>
        public void PlayColorMaskOpenAnim(Color color, float duration, DialogContainer blendDialog = null)
        {
            colorMask.DOKill();
            colorMask.color = Color.clear;
            var tweener = colorMask.DOColor(color, duration).SetUpdate(true);
            //跟下层对话框的遮罩进行颜色混合
            if (blendDialog != null)
            {
                blendDialog.colorMask.DOKill();
                tweener.OnUpdate(() =>
                {
                    if (blendDialog.dialogAddress != null)
                    {
                        var alpha = 1f - (1f - color.a) / (1f - colorMask.color.a);
                        blendDialog.colorMask.SetAlpha(alpha);
                    }
                });
                //下层对话框已销毁，立即停止混合动画
                blendDialog.colorMaskCloseCallback = () =>
                {
                    blendDialog.colorMask.color = Color.clear;
                    if (dialogAddress == null || !tweener.IsActive() || tweener.IsComplete()) return;
                    colorMask.color = color;
                    tweener.Kill();
                };
                tweener.OnComplete(() => blendDialog.colorMaskCloseCallback = null);
            }
        }

        /// <summary>
        /// 播放灰遮罩消失动画
        /// </summary>
        public void PlayColorMaskCloseAnim(float duration, DialogContainer blendDialog = null)
        {
            colorMask.DOKill();
            var tweener = colorMask.DOColor(Color.clear, duration).SetUpdate(true);
            //跟下层对话框的遮罩进行颜色混合
            if (blendDialog != null)
            {
                blendDialog.colorMask.DOKill();
                if (duration <= 0)
                {
                    //立即恢复遮罩颜色
                    blendDialog.colorMask.SetAlpha(blendDialog.dialog.dialogData.BgMaskColor.a);
                }
                else
                {
                    tweener.OnUpdate(() =>
                    {
                        if (blendDialog.dialogAddress != null)
                        {
                            var alpha = 1f - (1f - blendDialog.dialog.dialogData.BgMaskColor.a) / (1f - colorMask.color.a);
                            blendDialog.colorMask.SetAlpha(alpha);
                        }
                        else
                        {
                            //下层对话框已销毁，立即停止混合动画
                            tweener.onUpdate = null;
                        }
                    });
                }
            }
        }

        /// <summary>
        /// 设置是否可以点击对话框
        /// </summary>
        public void SetTouchEnabled(bool enabled)
        {
            if (touchMask != null) touchMask.SetActive(!enabled);
        }

        //对话框已销毁，立即停止ColorMask混合动画
        protected void OnDestroy()
        {
            colorMaskCloseCallback?.Invoke();
            colorMaskCloseCallback = null;
        }
    }
}
