using System.Collections.Generic;
using UnityEngine;
using WordGame;
public class AsyncLoadData
{
    public static int count = 0;
    public int id;
    public string path;
    public ResourceRequest resourceRequest;
    public int instanceCount = 1;

    public static int GetNewID()
    {
        count++;
        return count;
    }
    public void Clear()
    {
        id = 0;
        path = "";
        resourceRequest = null;
    }
}
namespace WordGame.Utils
{

    /// <summary>
    /// 对象池管理类
    /// </summary>
    public static class GameObjectPool
    {
        /// <summary>
        /// prefab的对象池
        /// </summary>
        private static Dictionary<string, Object> prefabDictionary = new Dictionary<string, Object>();

        private static Dictionary<string, AsyncLoadData> asyncLoadingPrefab = new Dictionary<string, AsyncLoadData>();
        private static List<string> asyncLoadingPrefabsTmpKeys = new List<string>(16);

        //池子根节点
        private static Transform poolRoot;

        /// <summary>
        /// 对象池缓存的对象
        /// </summary>
        private static Dictionary<Object, List<GameObject>> inactiveDic = new Dictionary<Object, List<GameObject>>();

        /// <summary>
        /// 对象池激活的对象
        /// </summary>
        private static Dictionary<GameObject, Object> activeDic = new Dictionary<GameObject, Object>();

        /// <summary>
        /// 记录进入池的循序
        /// </summary>
        private static List<GameObjectInfo> enterInfoLs = new List<GameObjectInfo>();

        #region Config

        /// <summary>
        /// 池子最大容量
        /// </summary>
        public const int MAX = 250;

        #endregion


        /// <summary>
        /// 预加载一些预制件
        /// </summary>
        public static void PreloadGameObjects(List<string> preloadRes)
        {
            for (int i = 0; i < preloadRes.Count; i++)
            {
                LoadPrefab(preloadRes[i]);
            }
        }
        
        public static void ClearUnUsed()
        {
            List<Object> obj = new List<Object>();
            foreach (var item in inactiveDic)
            {
                if ((item.Value == null || item.Value.Count == 0) || !activeDic.ContainsValue(item.Key))
                {
                    obj.Add(item.Key);
                }
            }

            for (int i = 0; i < obj.Count; i++)
            {
                int cont = inactiveDic[obj[i]].Count;
                for (int j = 0; j < cont; j++)
                {
                    MyDestroy(inactiveDic[obj[i]][j]);
                }
                inactiveDic.Remove(obj[i]);
            }

            Resources.UnloadUnusedAssets();
        }

        public static void LoadPrefab(string path)
        {
            if (!string.IsNullOrEmpty(path))
            {
                if (!prefabDictionary.ContainsKey(path))
                {
                    Object prefab = Resources.Load(path);
                    if (prefab != null)
                    {
                        prefabDictionary.Add(path, prefab);
                    }
                    else
                    {
                        Debugger.LogDError("error in GameObjectPool LoadPrefab");
                    }
                }
            }
        }

        /// <summary>
        /// 获取Prefab
        /// </summary>
        /// <param name="prefabPath"></param>
        /// <returns></returns>
        public static Object GetPrefab(string prefabPath, bool addToPool = true)
        {
            Object prefabResult = null;
            if (!string.IsNullOrEmpty(prefabPath))
            {
                if (!prefabDictionary.ContainsKey(prefabPath))
                {
                    Object prefab = Resources.Load(prefabPath);
                    if (addToPool && prefab != null)
                    {
                        prefabDictionary.Add(prefabPath, prefab);
                    }

                    prefabResult = prefab;
                }
                else
                {
                    prefabResult = prefabDictionary[prefabPath];
                }
            }

            return prefabResult;
        }

        /// <summary>取出来对象池</summary>
        /// <param name="o"></param>
        public static GameObject GetObj(string path)
        {
            GameObject ret = null;
            if (!string.IsNullOrEmpty(path))
            {
                Object prefab = null;
                if (!prefabDictionary.ContainsKey(path))
                {
                    prefab = Resources.Load(path);
                    
                    prefabDictionary.Add(path, prefab);
                }
                else
                {
                    prefab = prefabDictionary[path];
                }



                ret = GetObj(prefab);
            }

            return ret;
        }

        public static void PreloadGameObjAsync(string path,int count)
        {
            if (prefabDictionary.ContainsKey(path) || asyncLoadingPrefab.ContainsKey(path))
            {
                return;
            }

            GameObject ret = null;
            ResourceRequest request = Resources.LoadAsync(path);
            AsyncLoadData data = new AsyncLoadData();
            data.id = AsyncLoadData.GetNewID();
            data.path = path;
            data.instanceCount = count;
            data.resourceRequest = request;
            asyncLoadingPrefab.Add(path, data);
        }
        
        public static void UpdateResourceLoad()
        {
            if (asyncLoadingPrefab == null || asyncLoadingPrefab.Count == 0)
            {
                return;
            }
            foreach (var item in asyncLoadingPrefab)
            {
                if (item.Value.resourceRequest.isDone && item.Value.resourceRequest.asset != null)
                {
                    prefabDictionary[item.Key] = item.Value.resourceRequest.asset;
                    asyncLoadingPrefabsTmpKeys.Add(item.Key);
                    List<GameObject> gos = new List<GameObject>(item.Value.instanceCount);
                    for (int i = 0; i < item.Value.instanceCount; i++)
                    {
                        GameObject obj = GetObj(item.Value.resourceRequest.asset);
                        gos.Add(obj);
                    }

                    for (int i = 0; i < gos.Count; i++)
                    {
                        Recycle(gos[i]);
                    }
                }
            }

            for (int i = 0; i < asyncLoadingPrefabsTmpKeys.Count; i++)
            {
                asyncLoadingPrefab.Remove(asyncLoadingPrefabsTmpKeys[i]);
            }
            asyncLoadingPrefabsTmpKeys.Clear();
        }


        /// <summary>取出来对象池</summary>
        /// <param name="o"></param>
        public static GameObject GetObj(Object prefab)
        {
            GameObject gameObject = null;
            do
            {
                if (prefab == null)
                {
                    break;
                }

                gameObject = PopPool(prefab);
                if (gameObject == null) //对象池中没有,实例化出来      
                {
                    gameObject = GameObject.Instantiate(prefab) as GameObject;
                    gameObject.name = prefab.name;
                }

                PushActivePool(gameObject, prefab);
            } while (false);

            return gameObject;
        }

        /// <summary>放回对象池</summary>
        /// <param name="gameObject"></param>
        public static void Recycle(GameObject gameObject)
        {
            Recycle(gameObject, false);
        }

        /// <summary>放回对象池</summary>
        /// <param name="gameObject"></param>
        public static void Recycle(GameObject gameObject, bool isDetory)
        {
            do
            {
                if (gameObject == null)
                {
                    break;
                }

                CreateRoot();
                Object prefab = PopActivePool(gameObject);
                if (prefab == null || isDetory) //立即销毁的 或 不是从对象池中取出来的
                {
                    MyDestroy(gameObject);
                    break;
                }

                PushPool(gameObject, prefab);

                Check();
            } while (false);
        }

        /// <summary>消除所有对象</summary>
        public static void DestroyAll()
        {
            inactiveDic.Clear();
            activeDic.Clear();
            enterInfoLs.Clear();
            if (poolRoot != null)
            {
                GameObject.Destroy(poolRoot.gameObject);
            }
        }

        //****************************私有方法************************
        //


        /// <summary>压入缓存池</summary>
        /// <param name="o"></param>
        /// <param name="prefab"></param>
        private static void PushPool(GameObject o, Object prefab)
        {
            if (!inactiveDic.ContainsKey(prefab)) inactiveDic.Add(prefab, new List<GameObject>());
            if (!inactiveDic[prefab].Contains(o))
            {
                inactiveDic[prefab].Add(o);

                o.SetActive(false);
                o.transform.SetParent(poolRoot);

                RecordEntry(o, prefab);
            }
        }

        /// <summary>压出缓存池</summary>
        /// <param name="o"></param>
        /// <param name="prefab"></param>
        private static GameObject PopPool(Object prefab)
        {
            GameObject gameObject = null;
            if (prefab != null && inactiveDic.ContainsKey(prefab) && inactiveDic[prefab].Count > 0)
            {
                do
                {
                    gameObject = inactiveDic[prefab][0];
                    inactiveDic[prefab].RemoveAt(0);
                } while (gameObject == null && inactiveDic[prefab].Count > 0); //防止之前界面存在引用，删掉了缓存池的东西

                if (gameObject != null)
                {
                    gameObject.gameObject.SetActive(true);
                    gameObject.transform.SetParent(null);

                    DeleteRecordEntry(gameObject);
                }
            }

            return gameObject;
        }

        /// <summary>压入激活缓存池</summary>
        /// <param name="gameObject"></param>
        /// <param name="prefab"></param>
        private static void PushActivePool(GameObject gameObject, Object prefab)
        {
            if (!activeDic.ContainsKey(gameObject))
            {
                activeDic.Add(gameObject, prefab);
            }
        }

        /// <summary>压出激活缓存池</summary>
        /// <param name="o"></param>
        /// <param name="prefab"></param>
        private static Object PopActivePool(GameObject prefab)
        {
            Object ret = null;
            if (activeDic.ContainsKey(prefab))
            {
                ret = activeDic[prefab];
                activeDic.Remove(prefab);
            }

            return ret;
        }

        /// <summary>创建根节点</summary>
        private static void CreateRoot()
        {
            if (poolRoot == null)
            {
                poolRoot = new GameObject().transform;
                poolRoot.name = "PoolRoot";
                UnityEngine.Object.DontDestroyOnLoad(poolRoot);
            }
        }

        /// <summary>
        /// 检查是否需要释放
        /// </summary>
        private static void Check()
        {
            while (enterInfoLs.Count > MAX && enterInfoLs.Count > 0)
            {
                MyDestroy(PopPool(enterInfoLs[0].prefab));
                enterInfoLs.RemoveAt(0);
            }
        }

        /// <summary>记录</summary>
        /// <param name="o"></param>
        private static void RecordEntry(GameObject o, Object prefab)
        {
            enterInfoLs.Add(new GameObjectInfo(o, prefab));
        }

        /// <summary>删除记录</summary>
        /// <param name="o"></param>
        private static void DeleteRecordEntry(GameObject o)
        {
            for (int i = enterInfoLs.Count - 1; i >= 0; i--)
            {
                if (enterInfoLs[i].gameobject == o)
                {
                    enterInfoLs.RemoveAt(i);
                    break;
                }

                if (enterInfoLs[i].gameobject == null)
                {
                    enterInfoLs.RemoveAt(i);
                }
            }
        }

        /// <summary>删除资源</summary>
        /// <param name="obj"></param>
        private static void MyDestroy(GameObject obj)
        {
            GameObject.Destroy(obj);
        }
    }

    public class GameObjectInfo
    {
        /// <summary>
        /// 实例化物体
        /// </summary>
        public GameObject gameobject;

        /// <summary>
        /// 预设
        /// </summary>
        public Object prefab;

        public GameObjectInfo(GameObject gameobject, Object prefab)
        {
            this.gameobject = gameobject;
            this.prefab = prefab;
        }
    }
}