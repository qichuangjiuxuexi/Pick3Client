using System;
using System.Collections.Generic;
using UnityEngine;

namespace AppBase.OA
{
    [CreateAssetMenu(fileName = "ObjectAnimationClip",menuName = "OA动画/配置/Clip(各种移动、旋转、缩放等的动画组合)")]
    public class ObjectAnimationClip : ScriptableObject
    {
        public string clipName;
        public List<BaseObjectAnimConfig> animationConfigs;
    } 
}