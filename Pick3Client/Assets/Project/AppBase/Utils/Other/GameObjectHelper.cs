using System.Collections.Generic;
using UnityEngine;
using GObject = UnityEngine.Object;

namespace WordGame.Utils
{
    /// <summary>
    /// 对象操作工具
    /// </summary>
    public class GameObjectHelper
    {
        /// <summary>
        /// 实例化物体
        /// </summary>
        /// <param name="">prefab</param>
        /// <param name="parent">父物体</param>
        /// <returns></returns>
        public static GameObject CreateGameObject(GObject gObject, Transform parent)
        {
            GameObject gameObject = GameObject.Instantiate(gObject, parent) as GameObject;
            gameObject.transform.SetParent(parent);
            gameObject.transform.localPosition = Vector3.zero;
            gameObject.transform.localRotation = Quaternion.identity;
            gameObject.transform.localScale = Vector3.one;
            gameObject.name = gObject.name;
            return gameObject;
        }

        /// <summary>
        /// 实例化物体
        /// </summary>
        /// <param name="gObject"></param>
        /// <param name="parent"></param>
        /// <param name="addPrefabToPool"></param>
        /// <returns></returns>
        public static GameObject CreatGameObjectFromPool(GObject gObject, Transform parent)
        {
            GameObject gameObject = GameObjectPool.GetObj(gObject);
            gameObject.transform.SetParent(parent);
            gameObject.transform.localPosition = Vector3.zero;
            gameObject.transform.localRotation = Quaternion.identity;
            gameObject.transform.localScale = Vector3.one;
            gameObject.name = gObject.name;
            return gameObject;
        }

        /// <summary>
        /// 实例化物体
        /// </summary>
        /// <param name="">prefab</param>
        /// <param name="parent">父物体</param>
        /// <returns></returns>
        public static GameObject CreateGameObject(GObject gObject)
        {
            if (gObject != null)
            {
                GameObject gameObject = GameObject.Instantiate(gObject) as GameObject;
                return gameObject;
            }

            return null;
        }

        /// <summary>
        /// 实例化物体
        /// </summary>
        /// <param name="">prefab</param>
        /// <param name="parent">父物体</param>
        /// <returns></returns>
        public static GameObject CreateGameObject(string path, Transform parent = null, bool addPrefabToPool = false)
        {
            GameObject gameObject = GameObjectPool.GetObj(path);
            //GameObject prefab = Resources.Load<GameObject>(path);
            if (gameObject != null)
            {
                if (parent != null)
                {
                    gameObject.transform.SetParent(parent);
                }

                if (gameObject != null)
                {
                    gameObject.transform.localPosition = Vector3.zero;
                    gameObject.transform.localRotation = Quaternion.identity;
                    gameObject.transform.localScale = Vector3.one;
                }
            }

            return gameObject;
        }
        
        public static void ResetGameObjectPos(GameObject gameObject, Transform parent = null)
        {
            if (gameObject != null)
            {
                if (parent != null)
                {
                    gameObject.transform.SetParent(parent);
                }

                if (gameObject != null)
                {
                    gameObject.transform.localPosition = Vector3.zero;
                    gameObject.transform.localRotation = Quaternion.identity;
                    gameObject.transform.localScale = Vector3.one;
                }
            }
        }
        
        public static void ChangeGameObjectPos(GameObject gameObject,Transform parent,Vector3 anchoredPosition)
        {
            //GameObject prefab = Resources.Load<GameObject>(path);
            if (gameObject)
            {
                gameObject.SetActive(true);
                RectTransform trans = gameObject.transform as RectTransform;
                if (gameObject != null)
                {
                    if (parent != null)
                    {
                        trans.SetParent(parent);
                    }

                    if (gameObject != null)
                    {
                        trans.anchoredPosition = anchoredPosition;
                        trans.localRotation = Quaternion.identity;
                        trans.localScale = Vector3.one;
                    }
                }
            }
        }


        /// <summary>
        /// 从对象池中创建对象
        /// </summary>
        /// <param name="prefabPath"></param>
        /// <param name="parent"></param>
        /// <returns></returns>
        public static GameObject CreatGameObjectFromPool(string prefabPath, Transform parent, bool addToPool = true)
        {
            GameObject gameObject = GameObjectPool.GetObj(prefabPath);
            gameObject.transform.SetParent(parent);
            gameObject.transform.localPosition = Vector3.zero;
            gameObject.transform.localRotation = Quaternion.identity;
            gameObject.transform.localScale = Vector3.one;

            return gameObject;
        }

        /// <summary>
        /// 从对象池中创建对象
        /// </summary>
        /// <param name="prefabPath"></param>
        /// <param name="parent"></param>
        /// <returns></returns>
        public static GameObject CreatGameObjectFromPool(string prefabPath)
        {
            GameObject gameObject = GameObjectPool.GetObj(prefabPath);

            return gameObject;
        }

        /// <summary>
        /// 回收GameObject
        /// </summary>
        /// <param name="go"></param>
        public static void Recycle(GameObject go)
        {
            GameObjectPool.Recycle(go);
        }

        /// <summary>
        /// 激活GameObject
        /// </summary>
        /// <param name="obj"></param>
        public static void ActiveObj(GameObject obj)
        {
            if (obj != null && !obj.activeSelf)
            {
                obj.SetActive(true);
            }
        }

        /// <summary>
        /// 激活GameObject
        /// </summary>
        /// <param name="obj"></param>
        public static void ActiveObj(Transform obj)
        {
            if (obj != null && !obj.gameObject.activeSelf)
            {
                obj.gameObject.SetActive(true);
            }
        }

        /// <summary>
        /// 激活GameObject
        /// </summary>
        /// <param name="obj"></param>
        public static void ActiveObj(List<GameObject> obj)
        {
            for (int i = 0, count = obj.Count; i < count; i++)
            {
                if (obj[i] != null && !obj[i].activeSelf)
                {
                    obj[i].SetActive(true);
                }
            }
        }

        /// <summary>
        /// 激活GameObject
        /// </summary>
        /// <param name="obj"></param>
        public static void ActiveObj(List<Transform> obj)
        {
            for (int i = 0, count = obj.Count; i < count; i++)
            {
                if (obj[i] != null && !obj[i].gameObject.activeSelf)
                {
                    obj[i].gameObject.SetActive(true);
                }
            }
        }

        /// <summary>
        /// 不激活GameObject
        /// </summary>
        /// <param name="obj"></param>
        public static void DisActiveObj(GameObject obj)
        {
            if (obj != null && obj.activeSelf)
            {
                obj.SetActive(false);
            }
        }

        /// <summary>
        /// 不激活GameObject
        /// </summary>
        /// <param name="obj"></param>
        public static void DisActiveObj(Transform obj)
        {
            if (obj != null && obj.gameObject.activeSelf)
            {
                obj.gameObject.SetActive(false);
            }
        }

        /// <summary>
        /// 不激活GameObject
        /// </summary>
        /// <param name="obj"></param>
        public static void DisActiveObj(List<GameObject> obj)
        {
            for (int i = 0, count = obj.Count; i < count; i++)
            {
                if (obj[i] != null && obj[i].activeSelf)
                {
                    obj[i].SetActive(false);
                }
            }
        }

        /// <summary>
        /// 不激活GameObject
        /// </summary>
        /// <param name="obj"></param>
        public static void DisActiveObj(List<Transform> obj)
        {
            for (int i = 0, count = obj.Count; i < count; i++)
            {
                if (obj[i] != null && obj[i].gameObject.activeSelf)
                {
                    obj[i].gameObject.SetActive(false);
                }
            }
        }

        /// <summary>
        /// 立即摧毁GameObject
        /// </summary>
        /// <param name="obj"></param>
        public static void DestroyObjImmediate(GameObject obj, bool isClean = true)
        {
            if (obj != null)
            {
                //GObject.Destroy(obj);
                GObject.DestroyImmediate(obj);
            }

            if (isClean)
            {
                // Resources.UnloadUnusedAssets();
            }
        }

        /// <summary>
        /// 销毁物体
        /// </summary>
        /// <param name="obj"></param>
        /// <param name="isClean"></param>
        public static void DestroyObj(GameObject obj, bool isClean = true)
        {
            if (obj != null)
            {
                GObject.Destroy(obj);
            }

            if (isClean)
            {

            }
        }

        /// <summary>
        /// 摧毁GameObject
        /// </summary>
        /// <param name="obj"></param>
        public static void DestroyObj(Transform obj, bool isClean = true)
        {
            if (obj != null && obj.gameObject != null)
            {
                DestroyObj(obj.gameObject, isClean);
            }
        }

        /// <summary>
        /// 摧毁GameObject
        /// </summary>
        /// <param name="obj"></param>
        public static void DestroyObj(List<GameObject> obj)
        {
            for (int i = 0, count = obj.Count; i < count; i++)
            {
                if (obj[i] != null)
                {
                    DestroyObj(obj[i], false);
                }
            }

            //Resources.UnloadUnusedAssets();
        }

        /// <summary>
        /// 摧毁GameObject
        /// </summary>
        /// <param name="obj"></param>
        public static void DestroyObj(List<Transform> obj)
        {
            for (int i = 0, count = obj.Count; i < count; i++)
            {
                if (obj[i] != null && obj[i].gameObject != null)
                {
                    DestroyObj(obj[i].gameObject, false);
                }
            }
        }

        /// <summary>
        /// 移除并销毁所有子结点
        /// </summary>
        /// <param name="parentNode"></param>
        public static void DestroyAllChildren(Transform parentNode)
        {
            int childCountInRow = parentNode.childCount;
            for (int i = childCountInRow - 1; i >= 0; i--)
            {
                GameObject.Destroy(parentNode.GetChild(i).gameObject);
            }
        }

        /// <summary>
        /// 移除并回收所有子结点
        /// </summary>
        /// <param name="parentNode"></param>
        public static void RecycleAllChildren(Transform parentNode)
        {
            int childCountInRow = parentNode.childCount;
            for (int i = childCountInRow - 1; i >= 0; i--)
            {
                GameObjectPool.Recycle(parentNode.GetChild(i).gameObject);
            }
        }
    }
}