using System;
using AppBase.OA.Behaviours;
using UnityEngine;

namespace AppBase.OA.Config
{
    [OAConfigAttribute("相机-正交size(非正交FOV)",50)]
    [CreateAssetMenu(fileName = "正交相机Size、FOV修改",menuName = "OA动画/相机/Size(FOV)")]
    public class CameraSizeConfig :  BaseObjectAnimConfig<float>
    {
        public override Type behaviourType
        {
            get
            {
                return typeof(CameraSizeBehaviour);
            }
        }
    }
}