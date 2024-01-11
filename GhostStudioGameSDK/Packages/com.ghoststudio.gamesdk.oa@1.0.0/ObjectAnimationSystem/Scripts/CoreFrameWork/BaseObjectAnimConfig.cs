using System;
using UnityEngine;

namespace AppBase.OA
{
    public abstract class BaseObjectAnimConfig : ScriptableObject
    {
        public string description = "";
        public bool enabled = true;
        public abstract Type behaviourType { get; }
        public ObjectAnimUpdateType updateType;
        public bool baseDuration = false;
        public float duration;
        public float timeScale = 1;
        public AnimationCurve speedCurve;
        public float delayTime = 0;
    }
    
    [Serializable]
    public abstract class BaseObjectAnimConfig<T> : BaseObjectAnimConfig
    {
        public BaseObjectAnimDurationCorrectStrategy<T> dynamicDurationCorrectStrategy;
        public T startValue;
        public T endValue;
    }

}