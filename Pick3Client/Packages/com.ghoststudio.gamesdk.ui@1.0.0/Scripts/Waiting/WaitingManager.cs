using System.Collections;
using AppBase.Module;
using AppBase.Resource;
using AppBase.Timing;
using UnityEngine;
using UnityEngine.UI;

namespace AppBase.UI.Waiting
{
    /// <summary>
    /// 等待遮罩管理器
    /// </summary>
    public class WaitingManager : MonoModule
    {
        public override string GameObjectPath => "UICanvas/Waiting";
        protected WaitingData waitingData;
        protected Coroutine waitingCoroutine;

        protected override void OnInit()
        {
            base.OnInit();
            GameObject.AddFullScreenRectTransform();
            var img = GameObject.GetOrAddComponent<Image>();
            img.color = Color.clear;
            img.isMaskingGraphic = true;
            Transform.SetAsLastSibling();
            GameObject.SetActive(false);
        }

        /// <summary>
        /// 展示等待遮罩
        /// </summary>
        public void ShowWaiting(WaitingData waitingData = null)
        {
            GameObject.SetActive(true);
            this.waitingData = waitingData;
            if (waitingCoroutine != null)
            {
                GameBase.Instance.GetModule<TimingManager>().StopCoroutine(waitingCoroutine);
                waitingCoroutine = null;
            }
            
            if (waitingData == null) return;
            if (waitingData.delayTime > 0)
            {
                waitingCoroutine = GameBase.Instance.GetModule<TimingManager>().GlobalDelayCall(waitingData.delayTime, () => OnWaitingTimeout(waitingData));
            }

            if (string.IsNullOrEmpty(waitingData.address)) return;
            GameBase.Instance.GetModule<ResourceManager>().LoadAssetHandler<GameObject>(waitingData.address, handler =>
            {
                //已销毁
                if (this.waitingData != waitingData)
                {
                    handler.Release();
                    return;
                }

                //实例化prefab
                var prefab = handler.GetAsset<GameObject>();
                prefab.SetActive(false);
                var obj = GameObject.Instantiate(prefab, Transform);
                obj.GetResourceReference().AddHandler(handler);
                handler.Release();
                obj.transform.localPosition = Vector3.zero;
                obj.transform.localScale = Vector3.one;
                obj.transform.rotation = Quaternion.identity;
                obj.SetActive(true);
            }, () =>
            {
                Debugger.LogError(TAG, $"ShowWaiting failed，address={waitingData.address}");
            });
        }
        
        /// <summary>
        /// 隐藏等待遮罩
        /// </summary>
        public void HideWaiting()
        {
            if (!GameObject.activeSelf) return;
            if (waitingCoroutine != null)
            {
                GameBase.Instance.GetModule<TimingManager>().StopCoroutine(waitingCoroutine);
                waitingCoroutine = null;
            }
            var data = waitingData;
            waitingData = null;
            Transform.DestroyChildren();
            GameObject.SetActive(false);
            data?.OnHideCallback();
        }
        
        /// <summary>
        /// 等待遮罩超时
        /// </summary>
        protected void OnWaitingTimeout(WaitingData waitingData)
        {
            if (waitingData != this.waitingData) return;
            waitingCoroutine = null;
            this.waitingData = null;
            Transform.DestroyChildren();
            GameObject.SetActive(false);
            waitingData.OnTimeoutCallback();
        }
    }
}