using System;
using UnityEngine;

namespace AppBase.OA
{
    public abstract class BaseAnimBehaviour<T> : BaseAnimBehaviour
    {
        public T correctedStart;
        public T correctedEnd;
        public T correctedBase;
        public byte correctedInfo = 0;

        public bool HasCorrectedStart
        {
            get { return (correctedInfo & 1) > 0; }
        }
        
        public bool HasCorrectedEnd
        {
            get { return (correctedInfo & 2) > 0; }
        }
        
        public bool HasCorrectedBase
        {
            get { return (correctedInfo & 4) > 0; }
        }
        public virtual void CorrectStart(T newValue)
        {
            correctedStart = newValue;
            correctedInfo |= 1;
        }

        public virtual void CorrectEnd(T newValue)
        {
            correctedEnd = newValue;
            correctedInfo |= 2;
        }

        public virtual void CorrectBase(T newValue)
        {
            correctedBase = newValue;
            correctedInfo |= 4;
        }

        public virtual T GetRealStart()
        {
            if (HasCorrectedStart)
            {
                return correctedStart;
            }

            if (config is BaseObjectAnimConfig<T> typeConfig)
            {
                return typeConfig.startValue;
            }

            return default;
        }

        public virtual T GetRealEnd()
        {
            if (HasCorrectedEnd)
            {
                return correctedEnd;
            }

            if (config is BaseObjectAnimConfig<T> typeConfig)
            {
                return typeConfig.endValue;
            }

            return default;
        }

        public virtual T GetRealBase()
        {
            if (HasCorrectedBase)
            {
                return correctedBase;
            }

            return default;
        }
        public override void UpdateDuration()
        {
            if (config is BaseObjectAnimConfig<T> typeConfig && typeConfig.dynamicDurationCorrectStrategy)
            {
                float curRealDuration = typeConfig.dynamicDurationCorrectStrategy.GetDynamicDuraion(config,(T)GetRealStart(),(T)GetRealEnd());
                if (!Mathf.Approximately(realDuration, curRealDuration))
                {
                    realDuration = curRealDuration;
                    if (config.baseDuration)
                    {
                        onBaseDurationChanged?.Invoke(realDuration);
                    }
                }
            }
            else
            {
                realDuration = config.duration;
                if (config.baseDuration)
                {
                    onBaseDurationChanged?.Invoke(realDuration);
                }
            }
        }

        public override void AfterInit()
        {
            base.AfterInit();
            correctedInfo = 0;
            correctedStart = default;
            correctedEnd = default;
            correctedBase = default;
        }
    }
    public abstract class BaseAnimBehaviour
    {
        public float delayTime;
        public GameObject target;
        public ObjectAnimState state;
        public BaseObjectAnimConfig config;
        /// <summary>
        /// 参数依次是：deltaTime，elapsedTime，percent
        /// </summary>
        public Action<float, float,float> onUpdate;
        public Action onFinished;
        /// <summary>
        /// 当动画对象的z轴与世界坐标的z轴方向不一样时调用，可用于UGUI图片旋转时的切背面图使用.参数为true时代表二者方向一致
        /// </summary>
        public Action<bool> onFaceForwardChanged;

        /// <summary>
        /// duration基准参考组件的duraion变化时的回调。参数即为基准duration。比如一个物体有移动和旋转的动画，那么大部分情况下移动会作为动画时长的基准。移动动画做完后，其他动画通常也会完成。
        /// 那么，当移动动画根据某些策略产生变化后，其他动画可能需要同步改变。
        /// </summary>
        public Action<float> onBaseDurationChanged;
        protected float realDuration = -1;

        public BaseAnimBehaviour()
        {
            
        }
        public virtual float RealDuraion
        {
            get
            {
                if (realDuration > 0)
                {
                    return realDuration;
                }
                UpdateDuration();
                return realDuration;
            }
        }

        public abstract void UpdateDuration();
        
        /// <summary>
        /// 基准duration 变化后，如何根据新基准修改自身的duration 
        /// </summary>
        /// <param name="newDuration"></param>
        public virtual void OnBaseDurationChanged(float newDuration)
        {
            realDuration = newDuration;
        }

        public abstract void Update(float deltaTime, float elapsedTime);

        public void Init(BaseObjectAnimConfig config,GameObject target)
        {
            this.config = config;
            this.target = target;
            AfterInit();
        }
        /// <summary>
        /// 处理一些子类特有的数据
        /// </summary>
        public virtual void AfterInit()
        {
            
        }
        public void Start()
        {
            state = ObjectAnimState.Running;
            OnStart();
        }
        
        public virtual void OnStart()
        {
            
        }

        public void Restart()
        {
            OnBeforeRestart();
            state = ObjectAnimState.Running;
        }
        public virtual void OnBeforeRestart()
        {
            
        }

        public void Stop()
        {
            OnBeforeStop();
            state = ObjectAnimState.Stopped;
            OnAfterStop();
        }
        
        public virtual void OnBeforeStop()
        {
            
        }
        
        public virtual void OnAfterStop()
        {
            
        }
        
        public float LerpEx(float start, float end, float t,bool passZero = false)
        {
            if (!passZero)
            {
                return start + (end - start) * t;
            }

            float total = Mathf.Abs(start) + Math.Abs(end);
            float zeroProgress = Mathf.Abs(start) / total;
            float tPosAbs = total * t;
            if (t <= zeroProgress)
            {
                if (Mathf.Approximately(Mathf.Abs(start),0))
                {
                    return start + (end - start) * t;;
                }
                float t1 = tPosAbs / Mathf.Abs(start);
                return start + (0 - start) * t1;
            }
            else
            {
                if (Mathf.Approximately(Mathf.Abs(end),0))
                {
                    return start + (end - start) * t;
                }
                float t1 = (tPosAbs - Mathf.Abs(start)) / Mathf.Abs(end);
                return end * t1;
            }
            
        }

        public virtual T GetConfigStartValue<T>() where T : struct
        {
            if (config is BaseObjectAnimConfig<T> realConfig)
            {
                return realConfig.startValue;
            }

            return default;
        }
        
        public virtual T GetConfigEndValue<T>() where T : struct
        {
            if (config is BaseObjectAnimConfig<T> realConfig)
            {
                return realConfig.endValue;
            }

            return default;
        }
        
        /// <summary>
        /// delay之后真正执行的时间
        /// </summary>
        /// <param name="elapsedTime"></param>
        public virtual float GetRealActionTime(float elapsedTime)
        {
            return Mathf.Max(0,elapsedTime - config.delayTime - delayTime);
        }
    }
}