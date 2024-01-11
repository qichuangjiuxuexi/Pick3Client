using AppBase.OA.Config;
using UnityEngine;

namespace AppBase.OA.Behaviours
{
    public class CompositionScaleBehaviour : BaseAnimBehaviour<Vector3>
    {
        private CompositionScaleConfig realConfig;
        private Vector3 scaled;
#if UNITY_EDITOR
        public bool debug;
#endif
        
        public CompositionScaleBehaviour()
        {
            
        }

        public override void OnStart()
        {
            base.OnStart();
            scaled = Vector3.zero;
        }
        
        public override void AfterInit()
        {
            base.AfterInit();
            realConfig = config as CompositionScaleConfig;
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
            ScaleX(globalProgress);
            ScaleY(globalProgress);
            ScaleZ(globalProgress);
            target.transform.localScale = scaled;
            onUpdate?.Invoke(deltaTime,elapsedTime,timeProgress);
            if (globalProgress >= 1)
            {
                state = ObjectAnimState.Finished;
                onFinished?.Invoke();
            }
        }

        void ScaleX(float globalProgress)
        {
            float scaleProgressX = realConfig.xAxisCurve.Evaluate(globalProgress);
            scaled.x = realConfig.xBase + scaleProgressX;
        }
        
        void ScaleY(float globalProgress)
        {
            float scaleProgressY = realConfig.yAxisCurve.Evaluate(globalProgress);
            scaled.y = realConfig.yBase + scaleProgressY;
        }
        
        void ScaleZ(float globalProgress)
        {
            float scaleProgressZ = realConfig.zAxisCurve.Evaluate(globalProgress);
            scaled.z = realConfig.zBase + scaleProgressZ;
        }
        
    }
}