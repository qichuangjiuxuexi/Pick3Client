using System;
using AppBase.Utils;
using Spine;
using Spine.Unity;
using UnityEngine;

namespace AppBase.OA.SpineGroupModule
{
    public class SpineGroupElementBehaviour : BaseAnimBehaviour<string>
    {
        public SkeletonAnimation spineAnim;
        public bool updateNothingOnFinished = false;
        private bool played = false;
        public override void Update(float deltaTime, float elapsedTime)
        {
            if (!played && state == ObjectAnimState.Running)
            {
                played = true;
                if (!spineAnim)
                {
                    spineAnim = target.GetComponent<SkeletonAnimation>();
                    if (spineAnim)
                    {
                        spineAnim.UpdateMode = UpdateMode.FullUpdate;
                    }
                }

                if (spineAnim)
                {
                    spineAnim.AnimationState.Complete += OnPlayAnimEnd;
                    try
                    {
                        spineAnim.AnimationState.SetAnimation(0, GetRealStart(), false);
                    }
                    catch (Exception e)
                    {
                        if (AppUtil.IsDebug)
                        {
                            Debugger.LogDError($"Spine动画:{GetRealStart()}找不到！");
                        }
                        OnPlayAnimEnd(null);
                    }
                    
                }
                else
                {
                    Debug.LogError("Spine SkeletonAnimation找不到！");
                }

            }
        }

        public override void OnStart()
        {
            base.OnStart();
            played = false;
        }

        void OnPlayAnimEnd(TrackEntry trackEntry)
        {
            state = ObjectAnimState.Finished;
            spineAnim.AnimationState.Complete -= OnPlayAnimEnd;
            if (updateNothingOnFinished)
            {
                spineAnim.UpdateMode = UpdateMode.Nothing;
            }
            played = false;
        }

        public override void CorrectStart(string newValue)
        {
            base.CorrectStart(newValue);
        }
    }
}