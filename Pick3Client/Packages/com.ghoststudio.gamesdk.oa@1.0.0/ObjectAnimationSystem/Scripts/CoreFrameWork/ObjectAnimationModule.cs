using System;
using System.Collections.Generic;
using AppBase;
using AppBase.Module;
using AppBase.OA.Behaviours;
using AppBase.Resource;
using AppBase.OA;
using UnityEngine;
using Object = UnityEngine.Object;

public class ObjectAnimationModule : MonoModule
{
    public override string GameObjectPath => "OA";

    private string uiFlyLayerPath = "UICanvas/OAFlyLayerUI";
    private string worldFlyLayerPath = "OAFlyLayerWorld";
    private Dictionary<string, ResourceHandler> opIdHandlersDict;

    protected override void OnInit()
    {
        base.OnInit();
        opIdHandlersDict = new Dictionary<string, ResourceHandler>();
    }

    private Transform uiFlylayer;
    public Transform UIFlyLayer
    {
        get
        {
            if (uiFlylayer == null)
            {
                uiFlylayer = GameObject.Find(uiFlyLayerPath).transform;
            }

            if (!uiFlylayer)
            {
                uiFlylayer = this.Transform;
            }

            return uiFlylayer;
        }
    }
    
    private Transform worldFlylayer;
    public Transform WorldFlyLayer
    {
        get
        {
            if (worldFlylayer == null)
            {
                worldFlylayer = GameObject.Find(worldFlyLayerPath).transform;
            }

            if (!worldFlylayer)
            {
                worldFlylayer = this.Transform;
            }

            return worldFlylayer;
        }
    }
    public void PlayAnim(string prefabAdd,Action onFinished)
    {
        string guid = System.Guid.NewGuid().ToString();
        var handler = GameBase.Instance.GetModule<ResourceManager>().InstantGameObject(prefabAdd, Transform, (go) =>
        {
            ObjectAnimationComponent cp = go.GetComponent<ObjectAnimationComponent>();
            if (cp)
            {
                cp.onfinished = () =>
                {
                    OnFlyOneComplete(guid,go);
                    onFinished?.Invoke();
                };
                cp.Play();
            }
            else
            {
                Debugger.LogWarning("OA","要飞行的东西缺少配置！");
            }
        });
        opIdHandlersDict[guid] = handler;
    }

    public void PlayUIMoveAnim(string prefabAdd, Vector3 startWorldPos, Vector3 endWorldPos, Action onFinished)
    {
        string guid = System.Guid.NewGuid().ToString();
        var handler = GameBase.Instance.GetModule<ResourceManager>().InstantGameObject(prefabAdd, UIFlyLayer, (go) =>
        {
            go.transform.position = startWorldPos;
            ObjectAnimationComponent cp = go.GetComponent<ObjectAnimationComponent>();
            if (cp)
            {
                var bh = cp.GetBehaviour<WorldPostionMoveBehaviour>();
                bh.CorrectStart(startWorldPos);
                bh.CorrectEnd(endWorldPos);
                cp.onfinished = () =>
                {
                    OnFlyOneComplete(guid, go);
                    onFinished?.Invoke();
                };
                cp.Play();
            }
        });
        opIdHandlersDict[guid] = handler;
    }
    
    public void PlayWorldMoveAnim(string prefabAdd, Vector3 startWorldPos, Vector3 endWorldPos, Action onFinished)
    {
        string guid = System.Guid.NewGuid().ToString();
        var handler = GameBase.Instance.GetModule<ResourceManager>().InstantGameObject(prefabAdd, WorldFlyLayer, (go) =>
        {
            go.transform.position = startWorldPos;
            ObjectAnimationComponent cp = go.GetComponent<ObjectAnimationComponent>();
            if (cp)
            {
                var bh = cp.GetBehaviour<WorldPostionMoveBehaviour>();
                bh.CorrectStart(startWorldPos);
                bh.CorrectEnd(endWorldPos);
                cp.onfinished = ()=>
                {
                    OnFlyOneComplete(guid,go);
                    onFinished?.Invoke();
                };
                cp.Play();
            }
        });
        opIdHandlersDict[guid] = handler;
    }

    void OnFlyOneComplete(string opGuid,GameObject go)
    {
        if (opIdHandlersDict.ContainsKey(opGuid))
        {
            Object.Destroy(go);
            opIdHandlersDict.Remove(opGuid);
        }
    }

}
