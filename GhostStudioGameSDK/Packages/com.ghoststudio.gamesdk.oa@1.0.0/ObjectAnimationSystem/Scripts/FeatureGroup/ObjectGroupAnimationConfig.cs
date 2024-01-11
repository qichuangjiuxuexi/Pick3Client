using System;
using System.Collections.Generic;
using AppBase.OA.Behaviours;
using UnityEngine;

namespace AppBase.OA.Configs
{
    [OAConfigAttribute("组动画-未实现不能用",110)]
    [CreateAssetMenu(fileName = "ObjectGroupAnimationConfig",menuName = "OA动画/配置/组动画/尚未实现")]
    public class ObjectGroupAnimationConfig : BaseObjectAnimConfig<Vector3>
    {
        public List<ObjectAnimationClip> clips;
        public override Type behaviourType
        {
            get
            {
                return typeof(ObjectGroupAnimationBehaviour);
            }
        }
    }
}