using System;
using UnityEngine;

public static class TransformExtensions
{
    /// <summary>
    /// Makes the given game objects children of the transform.
    /// </summary>
    /// <param name="transform">Parent transform.</param>
    /// <param name="children">Game objects to make children.</param>
    public static void AddChildren(this Transform transform, GameObject[] children)
    {
        Array.ForEach(children, child => child.transform.parent = transform);
    }

    /// <summary>
    /// Makes the game objects of given components children of the transform.
    /// </summary>
    /// <param name="transform">Parent transform.</param>
    /// <param name="children">Components of game objects to make children.</param>
    public static void AddChildren(this Transform transform, Component[] children)
    {
        Array.ForEach(children, child => child.transform.parent = transform);
    }

    /// <summary>
    /// Sets the position of a transform's children to zero.
    /// </summary>
    /// <param name="transform">Parent transform.</param>
    /// <param name="recursive">Also reset ancestor positions?</param>
    public static void ResetChildPositions(this Transform transform, bool recursive = false)
    {
        foreach (Transform child in transform)
        {
            child.position = Vector3.zero;

            if (recursive)
            {
                child.ResetChildPositions(recursive);
            }
        }
    }

    /// <summary>
    /// Sets the layer of the transform's children.
    /// </summary>
    /// <param name="transform">Parent transform.</param>
    /// <param name="layerName">Name of layer.</param>
    /// <param name="recursive">Also set ancestor layers?</param>
    public static void SetChildLayers(this Transform transform, string layerName, bool recursive = false)
    {
        var layer = LayerMask.NameToLayer(layerName);
        SetChildLayersHelper(transform, layer, recursive);
    }

    static void SetChildLayersHelper(Transform transform, int layer, bool recursive)
    {
        foreach (Transform child in transform)
        {
            child.gameObject.layer = layer;

            if (recursive)
            {
                SetChildLayersHelper(child, layer, recursive);
            }
        }
    }

    /// <summary>
    /// Sets the x component of the transform's position.
    /// </summary>
    /// <param name="x">Value of x.</param>
    public static void SetX(this Transform transform, float x)
    {
        transform.position = new Vector3(x, transform.position.y, transform.position.z);
    }

    /// <summary>
    /// Sets the y component of the transform's position.
    /// </summary>
    /// <param name="y">Value of y.</param>
    public static void SetY(this Transform transform, float y)
    {
        transform.position = new Vector3(transform.position.x, y, transform.position.z);
    }

    /// <summary>
    /// Sets the z component of the transform's position.
    /// </summary>
    /// <param name="z">Value of z.</param>
    public static void SetZ(this Transform transform, float z)
    {
        transform.position = new Vector3(transform.position.x, transform.position.y, z);
    }

    public static void SetEulerAnglesX(this Transform transform, float x)
    {
        transform.eulerAngles = new Vector3(x, transform.eulerAngles.y, transform.eulerAngles.z);
    }

    public static void SetEulerAnglesY(this Transform transform, float y)
    {
        transform.eulerAngles = new Vector3(transform.eulerAngles.x, y, transform.eulerAngles.z);
    }

    public static void SetEulerAnglesZ(this Transform transform, float z)
    {
        transform.eulerAngles = new Vector3(transform.eulerAngles.x, transform.eulerAngles.y, z);
    }

    public static void SetLocalScaleX(this Transform transform, float x)
    {
        transform.localScale = new Vector3(x, transform.localScale.y, transform.localScale.z);
    }

    public static void SetLocalScaleY(this Transform transform, float y)
    {
        transform.localScale = new Vector3(transform.localScale.x, y, transform.localScale.z);
    }

    public static void SetLocalScaleZ(this Transform transform, float z)
    {
        transform.localScale = new Vector3(transform.localScale.x, transform.localScale.y, z);
    }


    public static float GetXZDistance(this Transform start, Transform end)
    {
        Vector2 startPos = new Vector2(start.position.x, start.position.z);
        Vector2 endPos = new Vector2(end.position.x, end.position.z);

        return Vector2.Distance(startPos, endPos);
    }
    
    /// <summary>
    /// 隐藏所有child
    /// </summary>
    public static void HideChildren(this Transform @this)
    {
        if (@this == null)
            return;
        for (int i = @this.childCount - 1; i >= 0; i--)
        {
            @this.GetChild(i).gameObject.SetActive(false);
        }
    }
    
    /// <summary>
    /// 隐藏所有child，除了指定的
    /// </summary>
    public static void HideChildrenExcept(this Transform @this, GameObject except)
    {
        if (@this == null)
            return;
        for (int i = @this.childCount - 1; i >= 0; i--)
        {
            var gameObject = @this.GetChild(i).gameObject;
            if (gameObject != except)
            {
                gameObject.SetActive(false);
            }
        }
    }
    
    /// <summary>
    /// 删除所有child
    /// </summary>
    /// <param name="this"></param>
    public static void DestroyChildren(this Transform @this)
    {
        var childCount = @this.childCount;
        for (int i = childCount - 1; i >= 0; i--)
        {
            GameObject.Destroy(@this.GetChild(i).gameObject);
        }
    }
    
    /// <summary>
    /// 删除所有child，除了指定的
    /// </summary>
    /// <param name="this"></param>
    public static void DestroyChildrenExcept(this Transform @this, GameObject except)
    {
        var childCount = @this.childCount;
        for (int i = childCount - 1; i >= 0; i--)
        {
            var gameObject = @this.GetChild(i).gameObject;
            if (gameObject != except)
            {
                GameObject.Destroy(gameObject);
            }
        }
    }
}



