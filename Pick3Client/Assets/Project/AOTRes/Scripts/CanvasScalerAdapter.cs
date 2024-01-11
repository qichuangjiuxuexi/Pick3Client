using UnityEngine;
using UnityEngine.UI;

public class CanvasScalerAdapter : MonoBehaviour
{
    private void Awake()
    {
        SetUIAdaptive();
    }
    
    public void SetUIAdaptive()
    {
        var canvasScaler = GetComponent<CanvasScaler>();
        var res = canvasScaler.referenceResolution;
        canvasScaler.matchWidthOrHeight = (float)Screen.width / Screen.height >= res.x / res.y ? 1 : 0;
    }
}