using System.Collections;
using System.Collections.Generic;
using UnityEngine;
public class BoxColliderDrawer : MonoBehaviour
{
    private void OnDrawGizmos()
    {
        BoxCollider boxCollider = GetComponent<BoxCollider>();
        if (boxCollider != null)
        {
            Vector3 center = boxCollider.center + transform.position;
            Vector3 size = boxCollider.size ;
            Vector3 lossyScale = transform.lossyScale;
            size.x *= lossyScale.x;
            size.y *= lossyScale.y;
            size.z *= lossyScale.z;
            Gizmos.color = Color.green;
            Gizmos.DrawWireCube(center, size);
        }
    }
}
