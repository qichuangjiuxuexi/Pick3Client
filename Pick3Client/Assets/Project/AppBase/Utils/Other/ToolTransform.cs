/**********************************************

Copyright(c) 2020 by Me2zen
All right reserved

Author : Terrence Rao
Date : 2020-07-23 17:13:55
Ver:1.0.0
Description :
ChangeLog :
**********************************************/

using System.Collections.Generic;
using UnityEngine;

namespace WordGame.Utils
{
    /// <summary>
    /// 
    /// </summary>
    public static class ToolTransform
    {
        /// <summary>
        /// 节点归零
        /// </summary>
        /// <param name="rectTransform">当前物体</param>
        /// <param name="parent">父节点</param>
        public static void SetHiParentResetZero(this RectTransform rectTransform, RectTransform parent)
        {
            rectTransform.SetParent(parent);
            rectTransform.localPosition = Vector3.zero;
            rectTransform.localScale = Vector3.one;
            rectTransform.localRotation = Quaternion.identity;
            rectTransform.offsetMax = Vector2.zero;
            rectTransform.offsetMin = Vector2.zero;
        }
        
        
        

        /// <summary>
        /// 设置 RectTransform 真实尺寸
        /// </summary>
        /// <param name="trans"></param>
        /// <param name="newSize"></param>
        public static void SetRectTransformSize(RectTransform trans, float height)
        {
            if (trans != null)
            {
                SetRectTransformSize(trans, new Vector2(trans.rect.width, height));
            }
        }

        /// <summary>
        /// 设置 RectTransform 真实尺寸
        /// </summary>
        /// <param name="trans"></param>
        /// <param name="newSize"></param>
        public static void SetRectTransformSize(RectTransform trans, Vector2 newSize)
        {
            if (trans != null)
            {
                Vector2 oldSize = trans.rect.size;
                Vector2 deltaSize = newSize - oldSize;
                trans.offsetMin = trans.offsetMin - new Vector2(deltaSize.x * trans.pivot.x, deltaSize.y * trans.pivot.y);
                trans.offsetMax = trans.offsetMax + new Vector2(deltaSize.x * (1f - trans.pivot.x), deltaSize.y * (1f - trans.pivot.y));
            }
        }

        public static Vector2 screen2world(Vector3 screenPos)
        {
            Vector3 wp = Camera.main.ScreenToWorldPoint(screenPos);
            Vector2 touchPos = new Vector2(wp.x, wp.y);
            return touchPos;
        }

        public static Vector2 screen2UIWorld(Vector3 screenPos)
        {
            Vector3 wp;
            wp = Camera.main.ScreenToWorldPoint(screenPos);
            Vector2 touchPos = new Vector2(wp.x, wp.y);
            return touchPos;
        }

        public static Vector2 world2screen(Vector3 worldPos)
        {
            Vector3 wp = Camera.main.WorldToScreenPoint(worldPos);
            Vector2 touchPos = new Vector2(wp.x, wp.y);
            return touchPos;
        }

        public static Vector2 world2UIScreen(Vector3 worldPos)
        {
            Vector3 sp;
            sp = Camera.main.WorldToScreenPoint(worldPos);
            Vector2 touchPos = new Vector2(sp.x, sp.y);
            return touchPos;
        }
        
        public static bool ScreenPointToLocalPointInRectangle(RectTransform rect, Vector2 screenPoint, Camera cam, out Vector2 localPoint)
        {
            return RectTransformUtility.ScreenPointToLocalPointInRectangle(rect, screenPoint, cam, out localPoint);
        }

        public static Vector3 World2LocalPosition(RectTransform rect, Vector3 worldPos)
        {
            Vector3 localPoint = (Vector2) rect.InverseTransformPoint(worldPos);
            return localPoint;
        }
        
        public static T[] GetTypeArray<T>(GameObject[] temps)
        {
            T[] saveArray = new T[temps.Length];
            for (int i = 0; i < temps.Length; i++)
            {
                saveArray[i] = temps[i].GetComponent<T>();
            }

            return saveArray;
        }

        public static void FindInChild<T>(Transform root, List<T> list)
        {
            for (int i = 0; i < root.childCount; i++)
            {
                T temp = root.GetChild(i).GetComponent<T>();
                if (temp != null)
                    list.Add(temp);
                FindInChild<T>(root.GetChild(i).transform, list);
            }
        }

        public static T FindInParent<T>(Transform root) where T : Component
        {
            T temp = null;

            while (root != null)
            {
                temp = root.GetComponent<T>();
                if (temp != null)
                    break;
                root = root.parent;
            }

            return temp;
        }
    }
}