using System;
using System.Collections;
using System.Collections.Generic;
using AppBase.OA;
using AppBase.OA.SpineGroupModule;
using UnityEngine;

[OAConfigAttribute("Spine-基础播放",100)]
[CreateAssetMenu(fileName = "SpineGroupElementConfig",menuName = "OA动画/配置/Spine/Spine组动画元素")]
public class SpineGroupElementConfig : BaseObjectAnimConfig<string>
{
    public override Type behaviourType
    {
        get
        {
            return typeof(SpineGroupElementBehaviour);
        }
    }
}
