using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace WordGame.Utils.Timer
{
    public static class CoroutineStarter
    {
        private static CoroutineStarter._CoroutineStarter _instance;

        private static CoroutineStarter._CoroutineStarter instance
        {
            get
            {
                if ((Object) CoroutineStarter._instance == (Object) null)
                {
                    GameObject gameObject = new GameObject("[CoroutineStarter]");
                    CoroutineStarter._instance = gameObject.AddComponent<CoroutineStarter._CoroutineStarter>();
                    Object.DontDestroyOnLoad((Object) gameObject);
                }

                return CoroutineStarter._instance;
            }
        }

        public static Coroutine StartCoroutine(IEnumerator routine)
        {
            return CoroutineStarter.instance.StartCoroutine(routine);
        }

        public static void StopAllCoroutines()
        {
            CoroutineStarter.instance.StopAllCoroutines();
        }

        public static void StopCoroutine(Coroutine routine)
        {
            CoroutineStarter.instance.StopCoroutine(routine);
        }

        public static void StopCoroutine(IEnumerator routine)
        {
            CoroutineStarter.instance.StopCoroutine(routine);
        }

        internal class _CoroutineStarter : MonoBehaviour
        {
        }
    }
}