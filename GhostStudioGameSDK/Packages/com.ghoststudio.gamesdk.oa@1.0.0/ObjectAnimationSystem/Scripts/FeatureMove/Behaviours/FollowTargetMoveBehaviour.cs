using AppBase.OA.Configs;
using UnityEngine;

namespace AppBase.OA.Behaviours
{
    public class FollowTargetMoveBehaviour : BaseAnimBehaviour<Vector3>
    {
        // x=rsinθcosφ.
        // y=rcosθ.
        // z=rsinθsinφ
        private FollowTargetMoveConfig realConfig;

        private Transform followTaregt;
        private Vector3 constOffset;
        public override void AfterInit()
        {
            base.AfterInit();
            realConfig = config as FollowTargetMoveConfig;
            constOffset = Vector3.zero;
        }

        /// <summary>
        /// 设置跟随的目标
        /// </summary>
        /// <param name="trf"></param>
        public void SetFollowTarget(Transform trf)
        {
            followTaregt = trf;
        }

        /// <summary>
        /// 设置偏移
        /// </summary>
        /// <param name="posOffset"></param>
        public void SetOffset(Vector3 posOffset)
        {
            constOffset = posOffset;
        }

        public override void OnStart()
        {
            base.OnStart();
            CorrectStart(GetInitStartValue());
            // Debug.Log(GetRealStart());
        }

        public Vector3 GetInitStartValue()
        {
            var followTaregt = this.followTaregt
                ? this.followTaregt
                : (realConfig.GetFollowTarget() ? realConfig.GetFollowTarget() : null);
            Vector3 dir = Vector3.zero;
            if (followTaregt)
            {
                dir = target.transform.position - followTaregt.transform.position - constOffset;
            }
            else
            {
                dir = target.transform.position - constOffset;
            }
            
            float r = dir.magnitude;
            float theta = 0;
            // if (r != 0)
            // {
            //     theta = Mathf.Acos(dir.y / r) * Mathf.Rad2Deg;
            // }
            theta = Vector3.Angle(dir, Vector3.up);

            float fai = 0;
            // if (dir.x != 0)
            // {
            //     fai = Mathf.Atan(dir.z / dir.x)* Mathf.Rad2Deg;
            // }
            // else
            // {
            //     fai = dir.z > 0 ? 90 : -90;
            // }
            dir.y = 0;
            fai = Vector3.SignedAngle(Vector3.right,dir,Vector3.up);
            return new Vector3(r, theta, fai);
        }

        public override void Update(float deltaTime, float elapsedTime)
        {
            if (state != ObjectAnimState.Running)
            {
                return;
            }
            float timeProgress = Mathf.Clamp01(elapsedTime / RealDuraion);
            float positionProgress = realConfig.speedCurve.Evaluate(timeProgress);
            float rProgress = realConfig.rCurve.Evaluate(positionProgress);
            float r = LerpEx(GetRealStart().x, GetRealEnd().x, rProgress);
            float thetaProgress = realConfig.thetaCurve.Evaluate(timeProgress);
            float theta = GetRealStart().y * Mathf.Deg2Rad;
            if (!realConfig.freezeTheta)
            {
                theta = LerpEx(GetRealStart().y, GetRealEnd().y, thetaProgress) * Mathf.Deg2Rad;
            }
            
            float faiProgress = realConfig.faiCurve.Evaluate(timeProgress);

            float fai = GetRealStart().z * Mathf.Deg2Rad;
            if (!realConfig.freezeFai)
            {
                fai = LerpEx(GetRealStart().z, GetRealEnd().z, faiProgress) * Mathf.Deg2Rad;
            }
                
            Vector3 offsetPos = new Vector3(r * Mathf.Sin(theta) * Mathf.Cos(fai),r * Mathf.Cos(theta),r * Mathf.Sin(theta) * Mathf.Sin(-fai));
            if (followTaregt)
            {
                target.transform.position = followTaregt.position + offsetPos + constOffset;
            }
            else
            {
                var configFollowTarget = realConfig.GetFollowTarget();
                if (configFollowTarget)
                {
                    target.transform.position = configFollowTarget.position + offsetPos + constOffset;
                }
                else
                {
                    target.transform.position = offsetPos + constOffset;
                }
            }

            if (timeProgress >= 1)
            {
                state = ObjectAnimState.Finished;
                var tmpFinished = onFinished;
                onFinished = null;
                tmpFinished?.Invoke();
            }
        }
    }
}