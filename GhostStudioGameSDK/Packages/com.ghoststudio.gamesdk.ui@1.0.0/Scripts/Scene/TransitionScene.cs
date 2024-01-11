using System;
using UnityEngine;

namespace AppBase.UI.Scene
{
    /// <summary>
    /// 转场场景
    /// </summary>
    public class TransitionScene : UIScene
    {
        /// <summary>
        /// 转场场景数据
        /// </summary>
        protected TransitionData transitionData => sceneData as TransitionData;

        /// <summary>
        /// 播放入场动画
        /// </summary>
        public override void OnPlayEnterAnim(Action callback)
        {
            if (string.IsNullOrEmpty(transitionData?.openAnimName) || GetComponent<Animator>() == null)
            {
                callback?.Invoke();
                SwitchToNextScene();
            }
            else
            {
                var duration = transform.PlayAnimatorUpdate(transitionData.openAnimName);
                if (duration > 0)
                {
                    this.DelayCall(duration, () =>
                    {
                        callback?.Invoke();
                        SwitchToNextScene();
                    }, true);
                }
                else
                {
                    callback?.Invoke();
                    SwitchToNextScene();
                }
            }
        }

        /// <summary>
        /// 切换到下一个场景
        /// </summary>
        public virtual void SwitchToNextScene()
        {
            if (transitionData?.NextSceneData == null)
            {
                Debugger.LogError("TransitionScene", "NextSceneData is null");
                return;
            }
            GameBase.Instance.GetModule<SceneManager>().SwitchScene(transitionData.NextSceneData);
        }

        /// <summary>
        /// 播放离场动画
        /// </summary>
        public override void OnPlayExitAnim(Action callback)
        {
            if (string.IsNullOrEmpty(transitionData?.closeAnimName) || GetComponent<Animator>() == null)
            {
                callback?.Invoke();
            }
            else
            {
                var duration = transform.PlayAnimatorUpdate(transitionData.closeAnimName);
                if (duration > 0)
                {
                    this.DelayCall(duration, () =>
                    {
                        callback?.Invoke();
                    }, true);
                }
                else
                {
                    callback?.Invoke();
                }
            }
        }
    }
}
