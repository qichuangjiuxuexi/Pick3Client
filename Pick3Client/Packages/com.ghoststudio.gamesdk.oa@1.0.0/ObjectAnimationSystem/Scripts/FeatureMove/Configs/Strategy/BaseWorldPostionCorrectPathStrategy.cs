using System.Collections.Generic;
using UnityEngine;

namespace AppBase.OA.Configs
{
    public abstract class BaseWorldPostionCorrectPathStrategy : BaseObjectAnimStrategy
    {
        public bool isDebug;
        public abstract List<Vector3> GetAutoPathCtrlPoints(BaseObjectAnimConfig config,Vector3 startVal,Vector3 endVal);

    }
}