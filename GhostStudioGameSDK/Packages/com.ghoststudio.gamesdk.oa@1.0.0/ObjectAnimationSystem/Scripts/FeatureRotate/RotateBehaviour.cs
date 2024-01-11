using System;
using AppBase.OA.Behaviours.Config;
using UnityEngine;
namespace AppBase.OA.Behaviours
{
    public class RotateBehaviour : BaseAnimBehaviour<float>
    {
        private RotateConfig realConfig;
        public Action<bool> onFaceDirChanged = null;
        private bool faceForward;
        private float rotated;
#if UNITY_EDITOR
        public bool debug;
#endif
        
        
        public RotateBehaviour()
        {
            
        }

        public override void OnStart()
        {
            base.OnStart();
            rotated = 0;
            faceForward = Vector3.Dot(target.transform.forward, Vector3.forward) > 0;
        }
        
        public override void AfterInit()
        {
            base.AfterInit();
            realConfig = config as RotateConfig;
            correctedInfo = 0;
        }
        
        public override void Update(float deltaTime, float elapsedTime)
        {
            Vector3 axis = realConfig.rotateAxis;
            float realTime = GetRealActionTime(elapsedTime);
            float timeProgress = Mathf.Clamp01(realTime / RealDuraion);
            float rotateProgress = realConfig.speedCurve.Evaluate(timeProgress);           
            if (realConfig.axisStrategy)
            {
                axis = realConfig.axisStrategy.GetAxis(realConfig, rotateProgress);
            }

            float newRotated = LerpEx(GetRealStart(), GetRealEnd(), rotateProgress);
            float delta = newRotated - rotated - GetRealBase();
            target.transform.Rotate(axis, delta, realConfig.isWorldRotation ? Space.World : Space.Self);
            bool curFaceForward = Vector3.Dot(target.transform.forward, Vector3.forward) > 0;
            if (curFaceForward ^ this.faceForward)
            {
                this.faceForward = curFaceForward;
                onFaceDirChanged?.Invoke(curFaceForward);
            }
            rotated += delta;
            onUpdate?.Invoke(deltaTime,elapsedTime,timeProgress);
            if (timeProgress >= 1)
            {
                state = ObjectAnimState.Finished;
                onFinished?.Invoke();
            }
        }
    }
}