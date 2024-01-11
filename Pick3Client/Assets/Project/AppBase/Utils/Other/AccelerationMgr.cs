/**********************************************

Copyright(c) 2020 by Me2zen
All right reserved

Author  : Terrence Rao
Date : 2020-07-24 15:45:02
Ver:1.0.0
Description :加速度 管理器
ChangeLog :
**********************************************/

using System;
using UnityEngine;
using WordGame.Utils;
 

namespace WordGame.Utils
{
    /// <summary>
    /// 加速度 管理器
    ///
    /// 手机竖起放置时
    /// Physics.gravity : <0, -9.81>
    /// 此时加速计的值为
    /// Input.acceleration : <0, -1, 0>
    /// </summary>
    public class AccelerationMgr : MonoSingleton<AccelerationMgr>
    {
        /// <summary>
        /// 默认重力持续的时间
        /// </summary>
        public const float TIME_DURATION_DEFAULT_GRAVITY = 2.5f;
        
        /// <summary>
        /// 设备加速计和重力值关系
        /// </summary>
        public const float DEFAULT_GRAVITY_SCALE = 9.81f;
        
        /// <summary>
        /// 人为增加偏移量
        /// </summary>
        //private Vector3 offset = new Vector3(0, (-1 * 20) / 90.0f, 0);

        ///// <summary>
        ///// X轴方向上, 过滤
        ///// </summary>
        //public float Filter_XOffset = PhysicsConfigMgr.Config.acceleX;

        ///// <summary>
        ///// 在竖直方向的修正
        ///// 1. 玩家躺着玩时, 卡不至于滚出
        ///// </summary>
        //public float ADJUST_IN_Z_AXIS = PhysicsConfigMgr.Config.acceleY;

        ///// <summary>
        ///// 向上纠正 结束角度
        ///// </summary>
        //public float ADJUST_Y_Angular = PhysicsConfigMgr.Config.acceleYAngular;

        /// <summary>
        /// Shuffle是力度和角度的关联
        /// </summary>
        private float ShuffleScaleBaseValue = 0.7f;

        // 开启设备加速
        private bool deviceAccelerationEnable = true;

        // 是否倒置
        private bool isGravityUpsideDown = false;

        /// <summary>
        /// 超过该角度后，不在向Y正方向矫正设备(向上纠正 结束角度)(0-180)
        /// </summary>
        private float acceleYAngular =145f;
        
        /// <summary>
        /// 向Y正方向矫正 百分比(0-1)
        /// </summary>
        public float acceleY = 0.5f;
        
        /// <summary>
        /// x方向滤波 百分比，轻微x偏移(0-1) 不会增加 x方向的力
        /// </summary>
        public float acceleX=10f;

        /// <summary>
        /// 归零计算 使用的数值
        /// </summary>
        private Vector3 tempToZeroValue = new Vector3(1, 0, 0);

        /// <summary>
        /// 设备加速是否开启
        /// </summary>
        public bool DeviceAccelerationEnable
        {
            get => deviceAccelerationEnable;
            set => deviceAccelerationEnable = value;
        }

        /// <summary>
        /// 默认重力
        /// </summary>
        private Vector3 defaultGravity = new Vector3(0, -9.81f, 0);


        /// <summary>
        /// 使用默认重力加速动
        /// 1. 在关卡开始, 或其它情况下, 忽略设备加速, 改用默认重力加速
        /// 2. 开启并持续一小段时间
        /// </summary>
        private bool isDeviceAccerateDisabledForAWhile;

        private float timeForDisableDeviceAccerate = 0f;

        /// <summary>
        /// 初始化
        /// </summary>
        public void Init()
        {
            Debugger.LogDFormat("gravity<{0},{1}>", Physics2D.gravity.x, Physics2D.gravity.y);
        }

        void Update()
        {
            //设备加速计失效时长检测
            if (isDeviceAccerateDisabledForAWhile)
            {
                if (timeForDisableDeviceAccerate > 0)
                {
                    timeForDisableDeviceAccerate -= Time.deltaTime;
                    if (timeForDisableDeviceAccerate < 0)
                    {
                        isDeviceAccerateDisabledForAWhile = false;
                    }
                }
            }
        }

        private void FixedUpdate()
        {
            UpdatePhysic2DGravity();

            //Debugger.LogDFormat("gravity:<{0}, {1}>", Physics.gravity.x, Physics.gravity.y);
            //Debugger.LogDFormat("Input.acceleration:<{0}, {1}, {2}>", Input.acceleration.x, Input.acceleration.y,Input.acceleration.z);
        }

        /// <summary>
        /// 更新重力方向
        /// </summary>
        private void UpdatePhysic2DGravity()
        {
            Physics2D.gravity = (Vector2) GetCurrentAcceleration();
        }


        /// <summary>
        /// 忽略设备加速计, 一小段时间
        /// </summary>
        public void DisableDeviceAccerateInLevel(float duration = TIME_DURATION_DEFAULT_GRAVITY,
            bool isUpsideDown = false)
        {
            isDeviceAccerateDisabledForAWhile = true;
            timeForDisableDeviceAccerate = duration;

            //是否倒置
            isGravityUpsideDown = isUpsideDown;
        }

        /// <summary>
        /// 重新打开设备加速计
        /// </summary>
        public void EnableDeviceAccerateEnableInLevel()
        {
            isDeviceAccerateDisabledForAWhile = false;
            timeForDisableDeviceAccerate = 0f;
        }

        /// <summary>
        /// 得到设备的倾斜方向
        /// 正常状态(0, -9.81)
        ///
        /// 9.81 * sin(20) = 3.35
        /// </summary>
        /// <returns></returns>
        public DeviceLeanType GetDeviceLeanType()
        {
            //最小倾斜值
            const float DEVICE_LEAN_MIN_VALUE = 3.35f;
            Vector2 gravityForward = Physics2D.gravity;

            if (Mathf.Abs(gravityForward.x) < DEVICE_LEAN_MIN_VALUE)
            {
                return DeviceLeanType.None;
            }
            else if (gravityForward.x > 0)
            {
                return DeviceLeanType.Right;
            }
            else
            {
                return DeviceLeanType.Left;
            }
        }

        /// <summary>
        /// 获取加速度
        /// </summary>
        /// <returns></returns>
        public Vector3 GetCurrentAcceleration()
        {
            Vector3 targetGravity = defaultGravity;
#if UNITY_EDITOR

#else
            if (DeviceAccelerationEnable && !isDeviceAccerateDisabledForAWhile)
            {
                Vector3 curValue = Input.acceleration;
                //curValue += offset;
                //过滤掉x方向小的偏移
                if (Mathf.Abs(curValue.x) < acceleX)
                {
                    curValue -= curValue.x * tempToZeroValue;
                }
                //角度修正
                Vector3 targetValue = GetAccelerationAfterAdapater(curValue);

                targetValue *= DEFAULT_GRAVITY_SCALE;
                
                targetGravity = targetValue;
            }
            else
            {
                
            }
#endif
            if (isGravityUpsideDown)
            {
                return -1 * targetGravity;
            }
            else
            {
                return targetGravity;
            }
        }

        /// <summary>
        /// 当前是否处于 需要使用默认方向重力
        /// </summary>
        /// <returns></returns>
        private bool CurrentGuideNeedUseDefaultAcceleration()
        {
            bool state = false;

            /*
            if (GuideCtrl.Instance.CurrentEventType == GuideEventType.RemoveTwoCards ||
                GuideCtrl.Instance.CurrentEventType == GuideEventType.RemoveThreeCards)
            {
                state = true;
            }
            */

            return state;
        }

        /// <summary>
        /// 修正设备加速计. 尽量竖直
        /// 1. 水平放置设备时, 看起来向60度放置. 
        /// </summary>
        /// <param name="curValue"></param>
        /// <returns></returns>
        private Vector3 GetAccelerationAfterAdapater(Vector3 curValue)
        {
            float angle = Vector3.Angle(curValue, Vector3.down);

            Vector3 targetValue = curValue;
            if (Mathf.Abs(angle) <= acceleYAngular)
            {
                targetValue = Vector3.Slerp(curValue, Vector3.down, acceleY);
            }


            //Debugger.LogDFormat("Input.acceleration after adapter:<{0}, {1}, {2}> angle:{3}", curValue.x, curValue.y,
            //curValue.z, angle);
            return targetValue;
        }

        /// <summary>
        /// 获取当前的 重力缩放
        /// </summary>
        /// <returns></returns>
        public float GetCurrentGravityScale()
        {
            Vector3 curValue = Input.acceleration;
#if UNITY_EDITOR
            return 1;
#else
            return ShuffleScaleBaseValue + Mathf.Abs(curValue.y) * (1- ShuffleScaleBaseValue);
#endif
        }
    }

    /// <summary>
    /// 设备倾斜类型
    /// </summary>
    public enum DeviceLeanType
    {
        /// <summary>
        /// 基本不倾斜
        /// </summary>
        None,

        /// <summary>
        /// 向左倾斜
        /// </summary>
        Left,

        /// <summary>
        /// 向右倾斜
        /// </summary>
        Right,

        /// <summary>
        /// 向前倾斜
        /// </summary>
        Forward,

        /// <summary>
        /// 向后倾斜
        /// </summary>
        Back,
    }
};