using System;
using System.Collections;
using System.Collections.Generic;
using AppBase.OA;
using AppBase.OA.SpineGroupModule;
using UnityEngine;

[OAConfigAttribute("透明度-UI-CanvasGroup",80)]
[CreateAssetMenu(fileName = "CanvasGroupAlphaConfig",menuName = "OA动画/配置/透明度/CanvasGroup形式")]
public class CanvasGroupAlphaConfig : BaseObjectAnimConfig<float>
{
    public override Type behaviourType
    {
        get
        {
            return typeof(CanvasGroupAlphaBehaviour);
        }
    }
}
