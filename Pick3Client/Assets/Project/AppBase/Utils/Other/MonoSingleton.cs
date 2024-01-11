/**********************************************

Copyright(c) 2020 by com.me2zen
All right reserved

Author : Terrence Rao 
Date : 2020-07-18 19:30:13
Ver : 1.0.0
Description : 
ChangeLog :
**********************************************/

using System;
using UnityEngine;

namespace WordGame.Utils
{
    /// <summary>
    /// MonoSingleton
    /// </summary>
    /// <typeparam name="T"></typeparam>
    [Obsolete("不建议使用MonoSingleton，建议使用MonoManager，并放到Game.cs中")]
    public abstract class MonoSingleton<T> : MonoBehaviour where T : MonoBehaviour
    {
        /// <summary>
        /// 
        /// </summary>
        private const string PARENT_NAME = "_MonoSingletonRoot";

        /// <summary>
        /// 
        /// </summary>
        private static T instance = null;

        /// <summary>
        /// 
        /// </summary>
        public static T Instance
        {
            get
            {
                GameObject parent = GameObject.Find(PARENT_NAME);
                if (parent == null)
                {
                    parent = new GameObject();
                    parent.name = PARENT_NAME;
#if UNITY_EDITOR
                    if (Application.isPlaying)
#endif
                    {
                        DontDestroyOnLoad(parent);
                    }
                }

                if (instance == null)
                {
                    instance = FindObjectOfType(typeof(T)) as T;
                    if (instance == null)
                    {
                        instance = new GameObject("_" + typeof(T).Name).AddComponent<T>();
#if UNITY_EDITOR
                        if (Application.isPlaying)
#endif
                        {
                            DontDestroyOnLoad(instance);
                        }
                    }

                    if (instance != null)
                    {
                        instance.transform.SetParent(parent.transform);
                    }
                }

                return instance;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        void OnApplicationQuit()
        {
            if (instance != null)
            {
                instance = null;
            }
        }
    }
}