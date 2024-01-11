using System;
using System.Collections.Generic;
using UnityEngine;

namespace AppBase.OA
{
    public class ObjectAnimationComponentGroup : BaseObjectAnimationComponent
    {
        public List<ObjectAnimationComponent> targets;
        public AnimationCurve delayCurve;
        public bool useCurveToSetDelay = false;
        public Vector2 delayRange = Vector2.zero; 
        public override void OnInit()
        {
            for (int i = 0; i < targets.Count; i++)
            {
                targets[i].Init();
            }
        }

        public override void Play()
        {
            if (targets == null)
            {
                return;
            }

            if (useCurveToSetDelay)
            {
                int count = targets.Count;
                for (int i = 0; i < count; i++)
                {
                    targets[i].delayTime = Mathf.Lerp(delayRange.x,delayRange.y,i == 0 ? 0 : delayCurve.Evaluate(i / (float)(count - 1)));
                    targets[i].Play();
                }
            }
            state = ObjectAnimState.Running;
        }

        public override void ResetState()
        {
            totalElapsedTime = 0;
            state = ObjectAnimState.None;
        }

        public override void Restart()
        {
            RePlay(); 
        }

        public override void Stop()
        {
            state = ObjectAnimState.Stopped;
        }

        public void SimulatePlay()
        {
            if (targets == null || targets.Count == 0)
            {
                return;
            }

            totalElapsedTime = 0;
            if (useCurveToSetDelay)
            {
                int count = targets.Count;
                for (int i = 0; i < count; i++)
                {
                    targets[i].delayTime = Mathf.Lerp(delayRange.x,delayRange.y,i == 0 ? 0 : delayCurve.Evaluate(i / (float)(count - 1)));
                }
            }
            for (int i = 0; i < targets.Count; i++)
            {
                targets[i].Play(); 
            }

            state = ObjectAnimState.Running;
        }
        
        public void RePlay()
        {
            totalElapsedTime = 0;
            if (targets == null)
            {
                state = ObjectAnimState.Finished;
                return;
            }
            for (int i = 0; i < targets.Count; i++)
            {
                targets[i].Restart();
            }
            state = ObjectAnimState.Running;
        }

        private void Start()
        {
            totalElapsedTime = 0;
            if (autoPlayOnStart)
            {
                Play();
            }
        }
        
        public override void SimuUpdateAll(float deltaTime)
        {
            OnNormalUpdate(deltaTime);
        }

        public override void OnNormalUpdate(float deltaTime)
        {
            bool allFinished = true;
            if (state == ObjectAnimState.Running)
            {
                for (int i = 0; i < targets.Count; i++)
                {
                    allFinished &= targets[i].state == ObjectAnimState.Finished;
                }

                totalElapsedTime += deltaTime;
                if (allFinished)
                {
                    state = ObjectAnimState.Finished;
                    var tmpAction = onfinished;
                    onfinished = null;
                    tmpAction?.Invoke();
                }
            }
        }

        public override void OnLateUpdate(float deltaTime)
        {
        }

        public override void OnFixedUpdate(float deltaTime)
        {
        }

        public override void UpdateManual(float deltaTime)
        {
        }
    }
}