using System;
using System.Collections.Generic;
using GameSDK.OA.Utils;
using AppBase.OA.Configs;
using UnityEngine;
namespace AppBase.OA.Behaviours
{
    public class WorldPostionMoveBehaviour : BaseAnimBehaviour<Vector3>
    {
        public Action<int> onReachWayPoint;
#if UNITY_EDITOR
        public bool debug;
#endif
        
        public WorldPostionMoveBehaviour()
        {
            
        }
        
        private WorldPositionMoveConfig realConfig;

        public float timeScaleMuliplier { get; set; } = 1;

        /// <summary>
        /// 在auto和
        /// </summary>
        // private Vector3[] bezierPath;
        private Bezier bezier;

        /// <summary>
        /// strict模式下的路标点距离起点的累加路径距离（不是两点间的直线距离）
        /// </summary>
        private List<float> lengthWps;

        /// <summary>
        /// strict模式下，start后当前的路标目标的索引
        /// </summary>
        private int targetWPsIndex = 0;

        private List<Vector3> correctedWps;
        
        public override Vector3 GetRealStart()
        {
            if (HasCorrectedStart)
            {
                return correctedStart;
            }

            switch (realConfig.pathType)
            {
                case PathType.Auto:
                    return realConfig.startValue;
                    break;
                case PathType.BezierSmoothedWps:
                case PathType.StrictWPs:
                    if (realConfig.wps != null && realConfig.wps.Count > 0)
                    {
                        return realConfig.wps[0];
                    }

                    return realConfig.startValue;
                    break;
                default:
                    return realConfig.startValue;
            }
        }

        public override Vector3 GetRealEnd()
        {
            if (HasCorrectedStart)
            {
                return correctedEnd;
            }
            switch (realConfig.pathType)
            {
                case PathType.Auto:
                    return realConfig.endValue;
                    break;
                case PathType.BezierSmoothedWps:
                case PathType.StrictWPs:
                    if (realConfig.wps != null && realConfig.wps.Count > 0)
                    {
                        return realConfig.wps[^1];
                    }

                    return realConfig.endValue;
                    break;
                default:
                    return realConfig.endValue;
            }
        }
        
        public override void AfterInit()
        {
            base.AfterInit();
            state = ObjectAnimState.None;
        }

        void InitState()
        {
            realConfig = config as WorldPositionMoveConfig;
            realDuration = 0;
            // correctStart = realConfig.wps[0] +
            //                new Vector3(UnityEngine.Random.Range(-100, 1000)/100f, UnityEngine.Random.Range(-500, 200)/100f, 0);
            //
            // correctEnd = realConfig.wps[^1] +
            //                new Vector3(UnityEngine.Random.Range(-100, 1000)/100f, UnityEngine.Random.Range(-500, 200)/100f, 0);
            switch (realConfig.pathType)
            {
                case PathType.BezierSmoothedWps:
                {
                    // bezierPath = GetBezierPath(realConfig.wps, realConfig.bezierPointsCount);
                    if (realConfig.correctPathStrategy == null || !HasCorrectedStart && !HasCorrectedEnd)
                    {
                        bezier = new Bezier(realConfig.wps);
                    }
                    else
                    {
                        bezier = new Bezier(realConfig.correctPathStrategy.GetAutoPathCtrlPoints(realConfig,HasCorrectedStart?correctedStart : realConfig.wps[0],HasCorrectedEnd?correctedEnd : realConfig.wps[^1]));
                    }
                    bezier.debugLine = true;
                    break;
                }
                // case PathType.AutoAbove:
                // case PathType.AutoBelow:
                case PathType.Auto:
                    if (!HasCorrectedEnd && !HasCorrectedStart)
                    {
                        var bezierCtrlPoints = GetAutoPathStrategy().GetAutoPathCtrlPoints(realConfig,realConfig.startValue,realConfig.endValue);
                        bezier = new Bezier(bezierCtrlPoints);
                    }
                    else
                    {
                        var bezierCtrlPoints = GetAutoPathStrategy().GetAutoPathCtrlPoints(realConfig,HasCorrectedStart ? correctedStart : realConfig.startValue,HasCorrectedEnd ? correctedEnd : realConfig.endValue);
                        bezier = new Bezier(bezierCtrlPoints);
                    }

                    bezier.debugLine = true;
                    break;
                case PathType.StrictWPs:
                    float sum = 0;
                    lengthWps = new List<float>(realConfig.wps.Count) {sum};
                    var list = realConfig.wps;
                    if (HasCorrectedEnd || HasCorrectedStart)
                    {
                        correctedWps = GetCorrectPathStrategy()
                            .GetAutoPathCtrlPoints(realConfig, HasCorrectedStart?correctedStart : realConfig.wps[0],HasCorrectedEnd?correctedEnd : realConfig.wps[^1]);
                    }
                    if (correctedWps != null)
                    {
                        list = correctedWps;
                        for (int i = 0; i < list.Count - 1; i++)
                        {
                            Debug.DrawLine(list[i], list[i + 1], Color.magenta, 10);
                        }
                    }
                    else
                    {
                        for (int i = 0; i < realConfig.wps.Count - 1; i++)
                        {
                            Debug.DrawLine(realConfig.wps[i], realConfig.wps[i + 1], Color.magenta, 10);
                        }
                    }
                    for (int i = 1; i <= list.Count - 1; i++)
                    {
                        sum += Vector3.Distance(list[i], list[i - 1]);
                        lengthWps.Add(sum);
                    }

                    targetWPsIndex = 1;
                    break;
            }
        }

        public override void OnBeforeRestart()
        {
            base.OnBeforeRestart();
            InitState();
        }
        

        BaseWorldPostionMoveAutoPathStrategy GetAutoPathStrategy()
        {
            if (realConfig.autoPathStrategy == null)
            {
                realConfig.autoPathStrategy =
                    ScriptableObject.CreateInstance<MovePoint2LineAutoModeStrategy>();
            }

            return realConfig.autoPathStrategy;
        }

        BaseWorldPostionCorrectPathStrategy GetCorrectPathStrategy()
        {
            if (realConfig.correctPathStrategy == null)
            {
                realConfig.correctPathStrategy =
                    ScriptableObject.CreateInstance<MovePoint2LineWPSCorrectStrategy>();
            }

            return realConfig.correctPathStrategy;
        }
        
        public override void Update(float deltaTime, float elapsedTime)
        {
            if (state == ObjectAnimState.Running)
            {
                float actionTime = GetRealActionTime(elapsedTime);
                switch (realConfig.pathType)
                {
                    // case PathType.AutoAbove:
                    // case PathType.AutoBelow:
                    case PathType.Auto:
                        UpdateAuto(deltaTime, actionTime);
                        break;
                    case PathType.BezierSmoothedWps:
                        UpdateBezier(deltaTime, actionTime);
                        break;
                    case PathType.StrictWPs:
                        UpdateStrict(deltaTime, actionTime);
                        break;
                    default:
                        break;
                }

                if (actionTime >= RealDuraion)
                {
                    // switch (realConfig.pathType)
                    // {
                    //     case PathType.Auto:
                    //         target.transform.position = basePosition + (correctedStartEnd ? correctEnd : realConfig.endValue);
                    //         break;
                    //     case PathType.BezierSmoothedWps:
                    //         target.transform.position = basePosition + bezier.GetPoint(1);
                    //         break;
                    //     case PathType.StrictWPs:
                    //         if (correctedWps != null)
                    //         {
                    //             target.transform.position = basePosition + correctedWps[^1];
                    //         }
                    //         else
                    //         {
                    //             target.transform.position = basePosition + realConfig.wps[^1];
                    //         }
                    //         break;
                    //     default:
                    //         break;
                    // }
                    state = ObjectAnimState.Finished;
                    onFinished?.Invoke();
                    onFinished = null;
                }
            }

        }

        /// <summary>
        /// 获取贝塞尔曲线(如果少于3个点，或者生成的曲线少于3个点，则返回默认值)
        /// </summary>
        /// <param name="PointsPositions">贝塞尔曲线点</param>
        /// <param name="createPointCount">生成路径点的数量</param>
        /// <returns></returns>
        public static Vector3[] GetBezierPath(List<Vector3> PointsPositions, int createPointCount = 20)
        {
            if (PointsPositions != null && PointsPositions.Count >= 3 && createPointCount > 3)
            {
                Bezier bezierCurve = new Bezier(PointsPositions);
                Vector3[] pos = new Vector3[createPointCount];
                for (int i = 0; i < createPointCount; i++)
                {
                    float process = i / (float) (createPointCount - 1);
                    pos[i] = bezierCurve.GetPoint(process);
                }

                return pos;
            }
            else if (PointsPositions != null && PointsPositions.Count > 0)
            {
                Vector3[] ret = new Vector3[PointsPositions.Count];
                for (int i = 0; i < PointsPositions.Count; i++)
                {
                    ret[i] = PointsPositions[i];
                }

                return ret;
            }

            //异常处理
            return new Vector3[0];
        }

        void UpdateAuto(float deltaTime, float myElapsedTime)
        {
            float timeProgress = myElapsedTime / RealDuraion;
            float positionProgress = realConfig.speedCurve.Evaluate(timeProgress);
            target.transform.position = GetRealBase() + bezier.GetPoint(positionProgress);
            onUpdate?.Invoke(deltaTime,myElapsedTime,timeProgress);
        }
        
        void UpdateBezier(float deltaTime, float myElapsedTime)
        {
            float timeProgress = myElapsedTime / RealDuraion;
            float positionProgress = realConfig.speedCurve.Evaluate(timeProgress);
            target.transform.position = GetRealBase()  + bezier.GetPoint(positionProgress);
            onUpdate?.Invoke(deltaTime,myElapsedTime,timeProgress);
        }

        void UpdateStrict(float deltaTime, float myElapsedTime)
        {
            float timeProgress = myElapsedTime / RealDuraion;
            float positionProgress = realConfig.speedCurve.Evaluate(timeProgress);
            float position = lengthWps[^1] * positionProgress;
            var list = realConfig.wps;
            if (correctedWps != null)
            {
                list = correctedWps;
            }
            //尝试刷新当前目标点
            while (position >= lengthWps[targetWPsIndex])
            {
                onReachWayPoint?.Invoke(targetWPsIndex);
                if (targetWPsIndex == lengthWps.Count - 1)
                {
                    target.transform.position = GetRealBase()  + list[^1];
                    break;
                }
                else
                {
                    targetWPsIndex++;
                }
            }

            float t = (position - lengthWps[targetWPsIndex - 1]) /
                      (lengthWps[targetWPsIndex] - lengthWps[targetWPsIndex - 1]);
            if (realConfig.strictWpsCurves != null && realConfig.strictWpsCurves.Count > targetWPsIndex - 1)
            {
                var keys = realConfig.strictWpsCurves[targetWPsIndex - 1].keys;
                if (keys != null && keys.Length > 0)
                {
                    t = realConfig.strictWpsCurves[targetWPsIndex - 1].Evaluate(t);
                }
            }
            target.transform.position = GetRealBase()  + Vector3.Lerp(list[targetWPsIndex - 1], list[targetWPsIndex],t);
            onUpdate?.Invoke(deltaTime,myElapsedTime,timeProgress);
        }
        
        public override void OnStart()
        {
            base.OnStart();
            InitState();
        }

        public override void OnAfterStop()
        {
            base.OnAfterStop();
            correctedWps = null;
        }

        /// <summary>
        /// 更换路标点。注意，如果在此前后使用了CorrectStart或者CorrectEnd，那么此设置则会失效
        /// </summary>
        /// <param name="wps"></param>
        public void CorrectWps(List<Vector3> wps)
        {
            if (wps != null)
            {
                correctedWps = wps;
            }
        }
    }
}