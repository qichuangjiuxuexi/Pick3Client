using System;
using AppBase.OA.Config;
using UnityEngine;

namespace AppBase.OA.Behaviours
{
    public class CameraSizeBehaviour : BaseAnimBehaviour<float>
    {
        public Action<float,float> setSize;
        private CameraSizeConfig realConfig;
        private float correctedStart = -1;
        private float correctedEnd = -1;
        private float baseValue = 0;
        
        public override void AfterInit()
        {
            base.AfterInit();
            realConfig = config as CameraSizeConfig;
            correctedStart = -1;
            correctedEnd = -1;
        }

        public override void OnStart()
        {
            base.OnStart();
            realDuration = -1;
            if (correctedStart < 0)
            {
                correctedStart = realConfig.startValue;
            }

            if (correctedEnd < 0)
            {
                correctedEnd = realConfig.endValue;
            }
        }

        public override void Update(float deltaTime, float elapsedTime)
        {
            if (state != ObjectAnimState.Running)
            {
                return;
            }

            float realTime = GetRealActionTime(elapsedTime);
            float timeProgress = realTime / RealDuraion;
            float globalProgress = realConfig.speedCurve.Evaluate(timeProgress);
            var size = LerpEx(correctedStart, correctedEnd, globalProgress);
            if (Mathf.Approximately(globalProgress,1))
            {
                setSize?.Invoke(baseValue + size,globalProgress);
                state = ObjectAnimState.Finished;
                onUpdate?.Invoke(deltaTime, elapsedTime,globalProgress);
                onFinished?.Invoke();
                return;
            }

            onUpdate?.Invoke(deltaTime, elapsedTime,globalProgress);
            setSize?.Invoke(baseValue + size,globalProgress);
        }
        
    }
}