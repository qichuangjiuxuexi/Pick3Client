using AppBase.Module;
using UnityEngine;
using UnityEngine.Rendering.Universal;

public class CameraManager : ModuleBase
{
    public Camera MainCamera { get; private set; }
    public Camera UICamera { get; private set; }

    protected override void OnInit()
    {
        UICamera = GameObject.Find("UICamera")?.GetComponent<Camera>();
        if (UICamera != null)
        {
            GameObject.DontDestroyOnLoad(UICamera.gameObject);
        }
        else
        {
            Debugger.LogError(TAG, "ui camera doesn't exist!");
        }
        
        MainCamera = GameObject.Find("MainCamera")?.GetComponent<Camera>();
        if (MainCamera != null)
        {
            GameObject.DontDestroyOnLoad(MainCamera.gameObject);
        }
    }

    /// <summary>
    /// 切换主镜头
    /// </summary>
    public void SwitchMainCamera(Camera camera)
    {
        MainCamera = camera;
    }
    
    /// <summary>
    /// 添加镜头到CameraStack
    /// </summary>
    public void AddToCameraStack(Camera overlayCam, bool isBottom = false)
    {
        if (MainCamera == null) return;
        var data = MainCamera.GetUniversalAdditionalCameraData();
        if (isBottom)
        {
            data.cameraStack.Add(overlayCam);
        }
        else
        {
            data.cameraStack.Insert(0, overlayCam);
        }
    }
    
    /// <summary>
    /// 从CameraStack移除镜头
    /// </summary>
    public bool RemoveFromCameraStack(Camera overlayCam)
    {
        if (MainCamera == null) return false;
        var data = MainCamera.GetUniversalAdditionalCameraData();
        return data.cameraStack.Remove(overlayCam);
    }
    
    /// <summary>
    /// 设置主镜头clearFlags
    /// </summary>
    /// <param name="flag"></param>
    public void SetCameraClearFlag(CameraClearFlags flag)
    {
        if (MainCamera == null) return;
        MainCamera.clearFlags = flag;
    }
}
