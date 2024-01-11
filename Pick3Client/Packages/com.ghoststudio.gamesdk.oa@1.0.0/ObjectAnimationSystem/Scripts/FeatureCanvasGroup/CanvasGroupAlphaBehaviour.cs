using Spine;
using Spine.Unity;
using UnityEngine;

namespace AppBase.OA
{
    public class CanvasGroupAlphaBehaviour : BaseAnimBehaviour<string>
    {
        public CanvasGroup canvasGroup;

        public override void AfterInit()
        {
            base.AfterInit();
            canvasGroup = target.GetComponent<CanvasGroup>();
            if (!canvasGroup)
            {
                canvasGroup = target.AddComponent<CanvasGroup>();
            }
        }

        public override void Update(float deltaTime, float elapsedTime)
        {
            if (state == ObjectAnimState.Running)
            {
                if (canvasGroup)
                {
                    float timeProgress = GetRealActionTime(elapsedTime) / RealDuraion;
                    bool finished = timeProgress >= 1;
                    float val = config.speedCurve.Evaluate(finished ? 1 : Mathf.Clamp01(timeProgress));
                    canvasGroup.alpha = val;
                    onUpdate?.Invoke(deltaTime,elapsedTime,timeProgress);
                    if (finished)
                    {
                        onFinished?.Invoke();
                        onFinished = null;
                        state = ObjectAnimState.Finished;
                    }

                }
            }
        }
    }
}