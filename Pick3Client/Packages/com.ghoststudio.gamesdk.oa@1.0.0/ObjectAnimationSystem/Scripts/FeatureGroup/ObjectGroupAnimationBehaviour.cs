using AppBase.OA.Configs;
using UnityEngine;
namespace AppBase.OA.Behaviours
{
    /// <summary>
    /// 通过控制三个轴向的百分比位置，最终合成一个任意曲线
    /// </summary>
    public class ObjectGroupAnimationBehaviour : BaseAnimBehaviour<float>
    {
#if UNITY_EDITOR
        public bool debug;
#endif

        public Vector3 startPosition;
        public Vector3 endPosition;
        private RectTransform rectTrf;
        private Transform trf;
        public ObjectGroupAnimationBehaviour()
        {
            
        }
        
        
        private CompositionAxisPostitionConfig realConfig;
        

        public override void AfterInit()
        {
            base.AfterInit();
            state = ObjectAnimState.None;
        }

        void InitState()
        {
            realConfig = config as CompositionAxisPostitionConfig;
            realDuration = 0;
        }

        public override void OnBeforeRestart()
        {
            base.OnBeforeRestart();
            InitState();
        }

        public override void Update(float deltaTime, float elapsedTime)
        {
            float actionTime = GetRealActionTime(elapsedTime);
            float timeProgress = actionTime / RealDuraion;
            float globalProgress = realConfig.speedCurve.Evaluate(timeProgress);
            Vector3 targetPos = new Vector3();
            if (realConfig.useSeperateAxis)
            {
                float xProgress = realConfig.xCurve.Evaluate(globalProgress);
                targetPos.x = LerpEx(startPosition.x, endPosition.x, xProgress);
                float yProgress = realConfig.yCurve.Evaluate(globalProgress);
                targetPos.y = LerpEx(startPosition.y, endPosition.y, yProgress);
                float zProgress = realConfig.zCurve.Evaluate(globalProgress);
                targetPos.z = LerpEx(startPosition.z, endPosition.z, zProgress);
            }
            else
            {
                float progress = realConfig.combinedCurve.Evaluate(globalProgress);
                targetPos.x = LerpEx(startPosition.x, endPosition.x, progress);
                targetPos.y = LerpEx(startPosition.y, endPosition.y, progress);
                targetPos.z = LerpEx(startPosition.z, endPosition.z, progress);
            }

            switch (realConfig.postionType)
            {
                case PositionType.WorldPosition:
                    target.transform.position = targetPos;
                    break;
                case PositionType.LocalPosition:
                    target.transform.localPosition = targetPos;
                    break;
                case PositionType.AnchoredPosition:
                    (target.transform as RectTransform).anchoredPosition = targetPos;
                    break;
                default:
                    break;
            }
            onUpdate?.Invoke(deltaTime,elapsedTime,timeProgress);
            if (globalProgress >= 1)
            {
                state = ObjectAnimState.Finished;
                onFinished?.Invoke();
            }
        }
        
        public override void OnStart()
        {
            base.OnStart();
            InitState();
        }
    }
}