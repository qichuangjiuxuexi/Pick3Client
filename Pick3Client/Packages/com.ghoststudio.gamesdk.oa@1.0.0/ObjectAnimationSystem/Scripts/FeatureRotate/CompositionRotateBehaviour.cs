using System;
using AppBase.OA.Config;
using AppBase.OA.Configs;
using UnityEngine;
namespace AppBase.OA.Behaviours
{
    public class CompositionRotateBehaviour : BaseAnimBehaviour<Vector3>
    {
        public Action<bool> onFaceDirChanged = null;
        private bool faceForward;
        private Vector3 baseRotation = new Vector3(0,0,0);
        private CompositionRotateConfig realConfig;
        private Vector3 rotated;
        private Vector3 direction;
#if UNITY_EDITOR
        public bool debug;
#endif
        
        public CompositionRotateBehaviour()
        {
            
        }

        public override void OnStart()
        {
            base.OnStart();
            rotated = Vector3.zero;
            faceForward = Vector3.Dot(target.transform.forward, Vector3.forward) > 0;
        }
        public override void AfterInit()
        {
            base.AfterInit();
            realConfig = config as CompositionRotateConfig;
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
            RotateX(globalProgress);
            RotateY(globalProgress);
            RotateZ(globalProgress);
            if (realConfig.isWorldRotation)
            {
                target.transform.rotation = Quaternion.Euler(rotated.x + baseRotation.x, rotated.y + baseRotation.y, rotated.z + baseRotation.z);
            }
            else
            {
                target.transform.localRotation = Quaternion.Euler(rotated.x + baseRotation.x, rotated.y + baseRotation.y, rotated.z + baseRotation.z);
            }
            
            bool curFaceForward = Vector3.Dot(target.transform.forward, Vector3.forward) > 0;
            if (curFaceForward ^ this.faceForward)
            {
                this.faceForward = curFaceForward;
                onFaceDirChanged?.Invoke(curFaceForward);
            }
            onUpdate?.Invoke(deltaTime,elapsedTime,timeProgress);
            if (globalProgress >= 1)
            {
                state = ObjectAnimState.Finished;
                onFinished?.Invoke();
            }
        }

        /// <summary>
        /// 获取真正的起点
        /// </summary>
        /// <param name="axis">0为x，1为y，2为z轴</param>
        /// <returns></returns>
        float GetRealStart(int axis)
        {
            if (HasCorrectedStart && axis >= 0 && axis <= 2)
            {
                return correctedStart[axis];
            }
            switch (axis)
            {
                case 0:
                    return realConfig.xRange.x;
                    break;
                case 1:
                    return realConfig.yRange.x;
                    break;
                case 2:
                    return realConfig.zRange.x;
                    break;
                default:
                    return realConfig.xRange.x;
                    break;
            }
        }
        
        /// <summary>
        /// 获取真正的终点
        /// </summary>
        /// <param name="axis">0为x，1为y，2为z轴</param>
        /// <returns></returns>
        float GetRealEnd(int axis)
        {
            if (HasCorrectedEnd && axis >= 0 && axis <= 2)
            {
                return correctedEnd[axis];
            }
            switch (axis)
            {
                case 0:
                    return realConfig.xRange.y;
                    break;
                case 1:
                    return realConfig.yRange.y;
                    break;
                case 2:
                    return realConfig.zRange.y;
                    break;
                default:
                    return realConfig.xRange.y;
                    break;
            }
        }

        void RotateX(float globalProgress)
        {
            float rotateProgressX = realConfig.xAxisCurve.Evaluate(globalProgress);
            float newRotated = 0; 
            //如果x轴的起点和终点都是0，说明是一个对称回归动画，幅度则由realConfig.xRange.z来决定，此时realConfig.xPassZero是无效的
            if (Mathf.Approximately(realConfig.xRange.x, 0) && Mathf.Approximately(realConfig.xRange.y, 0))
            {
                float end = realConfig.xRange.z;
                if (HasCorrectedEnd)
                {
                    end = correctedEnd.x;
                }
                newRotated = LerpEx(0,end , rotateProgressX,realConfig.xPassZero);
            }
            else
            {
                //如果x轴的起点和终点不都是0，此时realConfig.xPassZero是有效的，比如配置的是20，30，那么realConfig.xPassZero为true的话，实际过程走的是20-0-30
                newRotated = LerpEx(GetRealStart(0),GetRealEnd(0), rotateProgressX,realConfig.xPassZero);
            }
             
            if (Mathf.Approximately(newRotated, realConfig.xRange.y))
            {
                newRotated = realConfig.xRange.y;
            }
            float delta = newRotated - rotated.x;
            // target.transform.Rotate(realConfig.space == Space.Self ? target.transform.right : Vector3.right,delta,realConfig.space);
            // target.transform.Rotate(Vector3.right,delta,Space.World);
            rotated.x += delta;
        }
        
        void RotateY(float globalProgress)
        {
            float rotateProgressY = realConfig.yAxisCurve.Evaluate(globalProgress);
            float newRotated = 0;
            if (Mathf.Approximately(realConfig.yRange.x, 0) && Mathf.Approximately(realConfig.yRange.y, 0))
            {
                float end = realConfig.yRange.z;
                if (HasCorrectedEnd)
                {
                    end = correctedEnd.y;
                }
                newRotated = LerpEx(0, end, rotateProgressY,realConfig.yPassZero);
            }
            else
            {
                newRotated = LerpEx(GetRealStart(1),GetRealEnd(1), rotateProgressY,realConfig.yPassZero);
            }
            
            if (Mathf.Approximately(newRotated, realConfig.yRange.y))
            {
                newRotated = realConfig.yRange.y;
            }
            float delta = newRotated - rotated.y;
            // target.transform.Rotate(realConfig.space == Space.Self ? target.transform.up : Vector3.up,delta,realConfig.space);
            // target.transform.Rotate(Vector3.up,delta,Space.World);
            rotated.y += delta;
        }
        
        void RotateZ(float globalProgress)
        {
            float rotateProgressZ = realConfig.zAxisCurve.Evaluate(globalProgress);
            float newRotated = 0;
            if (Mathf.Approximately(realConfig.zRange.x, 0) && Mathf.Approximately(realConfig.zRange.y, 0))
            {
                float end = realConfig.zRange.z;
                if (HasCorrectedEnd)
                {
                    end = correctedEnd.z;
                }
                newRotated = LerpEx(0, end, rotateProgressZ,realConfig.zPassZero);
            }
            else 
            {
                newRotated = LerpEx(GetRealStart(2),GetRealEnd(2), rotateProgressZ,realConfig.zPassZero);
            }
            
            if (Mathf.Approximately(newRotated, realConfig.zRange.y))
            {
                newRotated = realConfig.zRange.y;
            }
            float delta = newRotated - rotated.z;
            // target.transform.Rotate(realConfig.space == Space.Self ? target.transform.forward : Vector3.forward,delta,realConfig.space);
            // target.transform.Rotate(Vector3.forward,delta,Space.World); 
            rotated.z += delta;
        }

        /// <summary>
        /// 设置方向。大于0按配置的方向，否则按配置的相反方向，等于0相当于使旋转失效
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <param name="z"></param>
        public void SetDirection(int x, int y, int z)
        {
            direction.x = x >= 0 ? 1 : -1;
            direction.y = y >= 0 ? 1 : -1;
            direction.z = z >= 0 ? 1 : -1;
        }

        public override void OnAfterStop()
        {
            base.OnAfterStop();
        }

        public Vector3 GetConfigStartValue()
        {
            Vector3 v = new Vector3(realConfig.xRange.x, realConfig.yRange.x, realConfig.zRange.x);
            return v;
        }
        public Vector3 GetConfigEndValue()
        {
            Vector3 v = new Vector3(realConfig.xRange.y, realConfig.yRange.y, realConfig.zRange.y);
            if (Mathf.Approximately(realConfig.xRange.x, 0) && Mathf.Approximately(realConfig.xRange.y, 0))
            {
                v.x = realConfig.xRange.z;
            }
            if (Mathf.Approximately(realConfig.yRange.x, 0) && Mathf.Approximately(realConfig.yRange.y, 0))
            {
                v.y = realConfig.yRange.z;
            }
            if (Mathf.Approximately(realConfig.zRange.x, 0) && Mathf.Approximately(realConfig.zRange.y, 0))
            {
                v.x = realConfig.zRange.z;
            }
            return v;
        }
    }
}