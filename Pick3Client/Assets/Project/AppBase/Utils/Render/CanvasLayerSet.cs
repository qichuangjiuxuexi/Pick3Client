using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CanvasLayerSet : MonoBehaviour
{
    /// <summary>
    /// 添加层级
    /// </summary>
    public int AddLayer = 0;

    /// <summary>
    /// 在父级层级添加层级
    /// </summary>
    public Canvas parentCanvas;

    private Canvas currentCanvas;

    private void Start()
    {
        currentCanvas = GetComponent<Canvas>();
        if (currentCanvas != null)
        {
            currentCanvas.overrideSorting = true;
        }
        SetTargetLayer();
    }

    public void SetTargetLayer()
    {
        if (currentCanvas == null || parentCanvas == null)
            return;

        currentCanvas.sortingOrder = parentCanvas.sortingOrder + AddLayer;
    }
}
