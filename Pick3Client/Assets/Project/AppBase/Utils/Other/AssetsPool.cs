/**********************************************

Copyright(c) 2020 by Me2zen
All right reserved

Author : Terrence Rao
Date : 2020-07-24 11:46:27
Ver:1.0.0
Description :
ChangeLog :
**********************************************/

using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;

namespace WordGame.Utils
{
    /// <summary>
    /// 资源缓存池
    /// </summary>
    public class AssetsPool : MonoSingleton<AssetsPool>
    {
        /// <summary>
        /// 背景图片路径
        /// </summary>
        public const string BG_SPRITE_PATH = "BG/";

        /// <summary>
        /// 图片缓存池
        /// </summary>
        private Dictionary<string, Sprite> spriteDic = new Dictionary<string, Sprite>();

        /// <summary>
        /// 声音缓存池
        /// </summary>
        private Dictionary<string, AudioClip> soundDic = new Dictionary<string, AudioClip>();

        /// <summary>
        /// shaderDic缓存池
        /// </summary>
        private Dictionary<string, Shader> shaderDic = new Dictionary<string, Shader>();

        /// <summary>
        /// 材质缓存池
        /// </summary>
        private Dictionary<string, Material> materialDic = new Dictionary<string, Material>();

        /// <summary>
        /// 物理材质缓存池
        /// </summary>
        private Dictionary<string, PhysicsMaterial2D> physicsMaterialDic = new Dictionary<string, PhysicsMaterial2D>();

        //游戏背影图缓冲池, 只存少量的小图
        private Dictionary<string, Sprite> gameBgSpriteFromStreamingPool = new Dictionary<string, Sprite>();
        private Dictionary<string, Texture2D> gameBgTextureFromStreamingPool = new Dictionary<string, Texture2D>();

        //最多缓冲图数(内部图数)
        public int MaxGameBgAtlasCountInPool = 3;

        private void Start()
        {
            GameObject.DontDestroyOnLoad(gameObject);
        }

        /// <summary>
        /// 设置最多缓冲图数(闪屏时可以多缓存图片，正常游戏中只缓存两张)
        /// </summary>
        /// <param name="splash"></param>
        public void SetMaxGameBgAtlasCountInPool(bool splash)
        {
            if (splash)
            {
                MaxGameBgAtlasCountInPool = 3;
            }
            else
            {
                MaxGameBgAtlasCountInPool = 3;
            }
        }

        /// <summary>
        /// 预加载图片
        /// </summary>
        public void PreloadSprites(List<string> preloadRes)
        {
            for (int i = 0; i < preloadRes.Count; i++)
            {
                GetSpriteFromImagePrefab(preloadRes[i], true);
            }
        }

        /// <summary>
        /// 从Prefab中, 动态获取Sprite
        /// </summary>
        /// <param name="path">资源路径</param>
        /// <returns></returns>
        public Sprite GetSpriteFromImagePrefab(string path, bool addIntoPool = true)
        {
            if (!spriteDic.ContainsKey(path))
            {
                Sprite sprite = null;
                GameObject obj = Resources.Load<GameObject>("DynamicImages/" + path);
                if (obj != null)
                {
                    Image image = obj.GetComponent<Image>();
                    if (image != null)
                    {
                        sprite = image.sprite;
                    }
                }
                else
                {
                    Debugger.LogDError("error in AssetsPool GetSpriteFromImagePrefab obj is null. " + path);
                }

                if (addIntoPool)
                {
                    spriteDic.Add(path, sprite);
                }

                return sprite;
            }
            else
            {
                return spriteDic[path];
            }
        }

        /// <summary>
        /// 从缓存池中加载audioclip
        /// </summary>
        /// <param name="path"></param>
        /// <param name="addIntoPool"></param>
        /// <returns></returns>
        public AudioClip GetAudioClipFromSoundPool(string path, bool addIntoPool = true)
        {
            if (!soundDic.ContainsKey(path))
            {
                AudioClip result = null;
                AudioClip clip = Resources.Load<AudioClip>(path);
                if (clip != null)
                {
                    result = clip;
                    if (addIntoPool)
                    {
                        soundDic.Add(path, result);
                    }
                }
                return result;
            }
            else
            {
                return soundDic[path];
            }
        }

        /// <summary>
        /// 直接从文件中获取 Shader
        /// </summary>
        /// <param name="path">资源路径</param>
        /// <returns></returns>
        public Shader GetShaderFromFile(string path, bool addIntoPool = true)
        {
            if (!shaderDic.ContainsKey(path))
            {
                Shader shader = null;
                Shader obj = Resources.Load<Shader>(path);
                if (obj != null)
                {
                    shader = obj;

                    if (addIntoPool)
                    {
                        shaderDic.Add(path, shader);
                    }
                }

                return shader;
            }
            else
            {
                return shaderDic[path];
            }
        }

        /// <summary>
        /// 直接从文件中获取 Material
        /// </summary>
        /// <param name="path">资源路径</param>
        /// <returns></returns>
        public Material GetMaterial(string path, bool addIntoPool = true)
        {
            if (!materialDic.ContainsKey(path))
            {
                Material material = null;
                Material obj = Resources.Load<Material>(path);
                if (obj != null)
                {
                    material = obj;

                    if (addIntoPool)
                    {
                        materialDic.Add(path, material);
                    }
                }

                return material;
            }
            else
            {
                return materialDic[path];
            }
        }

        /// <summary>
        /// 直接从文件中获取 PhysicsMaterial2D
        /// </summary>
        /// <param name="path">资源路径</param>
        /// <returns></returns>
        public PhysicsMaterial2D GetPhysicsMaterial2D(string path, bool addIntoPool = true)
        {
            if (!physicsMaterialDic.ContainsKey(path))
            {
                PhysicsMaterial2D material = null;
                PhysicsMaterial2D obj = Resources.Load<PhysicsMaterial2D>(path);
                if (obj != null)
                {
                    material = obj;

                    if (addIntoPool)
                    {
                        physicsMaterialDic.Add(path, material);
                    }
                }

                return material;
            }
            else
            {
                return physicsMaterialDic[path];
            }
        }

        /// <summary>
        /// 获取图片
        /// </summary>
        /// <param name="spriteName">资源路径</param>
        /// <returns></returns>
        public Sprite GetBgSprite(string spriteName)
        {
            Sprite sprite = null;
            GameObject obj = Resources.Load<GameObject>(BG_SPRITE_PATH + spriteName);
            if (obj != null)
            {
                Image image = obj.GetComponent<Image>();
                if (image != null)
                {
                    sprite = image.sprite;
                }
            }

            return sprite;
        }

        /// <summary>
        /// 获取声音
        /// </summary>
        /// <param name="path">资源路径</param>
        /// <returns></returns>
        public AudioClip GetAudio(string path)
        {
            AudioClip audioClip = null;
            GameObject obj = Resources.Load<GameObject>(path);
            if (obj != null)
            {
                AudioSource audio = obj.GetComponent<AudioSource>();
                if (audio != null)
                {
                    audioClip = audio.clip;
                }
            }

            return audioClip;
        }

        /// <summary>
        /// 缓存中没有, 去streamingAssets中读取
        /// 1. WWW加载方式, 已经不再使用, 改用WebRequest
        /// </summary>
        /// <param name="fileName"></param>
        /// <returns></returns>
        public IEnumerator LoadGameBgFromStreaming(string fileName,
            System.Action<Sprite> loadCmplCallback = null)
        {
            //使用缓存
            if (gameBgSpriteFromStreamingPool.ContainsKey(fileName))
            {
                // PrintDic();
                if (loadCmplCallback != null && loadCmplCallback.Target != null)
                {
                    loadCmplCallback(gameBgSpriteFromStreamingPool[fileName]);
                }

                //直接结束协程
                yield break;
            }

            if (string.IsNullOrWhiteSpace(fileName))
            {
                if (loadCmplCallback != null)
                {
                    loadCmplCallback(null);
                }

                //直接结束协程
                yield break;
            }

            Sprite newSpriteBg = null;

            string filePath = Application.streamingAssetsPath + "/" + BG_SPRITE_PATH + fileName + ".jpg";
            if (Application.platform != RuntimePlatform.WebGLPlayer)
            {
                if (!filePath.Contains("://"))
                {
                    filePath = "file://" + filePath;
                }
            }

            WWW www = new WWW(filePath);
            yield return www;

            //转化成Texture2D
            //Texture2D tex2d = new Texture2D(www.texture.width, www.texture.height);

            //加载纹理到图像
            //www.LoadImageIntoTexture(tex2d);

            if (www != null && www.texture != null)
            {
                Texture2D tempTexture2D =
                    new Texture2D(www.texture.width, www.texture.height, TextureFormat.RGBA32, false);
                tempTexture2D.SetPixels(www.texture.GetPixels());
                tempTexture2D.wrapMode = TextureWrapMode.Clamp;
                tempTexture2D.Apply();

                newSpriteBg = Sprite.Create(tempTexture2D,
                    new Rect(0, 0, www.texture.width, www.texture.height),
                    Vector2.zero);
                www.Dispose();
            }


            if (newSpriteBg != null)
            {
                //缓存超限清除
                if (gameBgSpriteFromStreamingPool.Count > MaxGameBgAtlasCountInPool)
                {
                    CleanResources();
                }

                //添加新图入缓存
                if (!gameBgSpriteFromStreamingPool.ContainsKey(fileName))
                {
                    gameBgSpriteFromStreamingPool.Add(fileName, newSpriteBg);
                }

                if (loadCmplCallback != null && loadCmplCallback.Target != null)
                {
                    loadCmplCallback(gameBgSpriteFromStreamingPool[fileName]);
                }
            }
            else
            {
                Debugger.LogDError("error in LoadGameBgFromStreaming newSpriteBg is null");
                if (loadCmplCallback != null && loadCmplCallback.Target != null)
                {
                    loadCmplCallback(null);
                }
            }

            yield return true;
        }

        /// <summary>
        /// 缓存中没有, 去streamingAssets中读取
        /// UnityWebRequest读取速度, 比WWW慢
        /// </summary>
        /// <param name="fileName"></param>
        /// <returns></returns>
        public IEnumerator LoadGameBgFromStreamingWebRequest(string fileName,
            Action<Sprite> loadCmplCallback = null)
        {
            //使用缓存
            if (gameBgSpriteFromStreamingPool.ContainsKey(fileName))
            {
                // PrintDic();
                if (loadCmplCallback != null && loadCmplCallback.Target != null)
                {
                    loadCmplCallback(gameBgSpriteFromStreamingPool[fileName]);
                }

                //直接结束协程
                yield break;
            }

            if (string.IsNullOrWhiteSpace(fileName))
            {
                if (loadCmplCallback != null)
                {
                    loadCmplCallback(null);
                }

                //直接结束协程
                yield break;
            }

            Sprite newSpriteBg = null;

            string filePath = Application.streamingAssetsPath + "/" + BG_SPRITE_PATH + fileName + ".jpg";
            if (Application.platform != RuntimePlatform.WebGLPlayer)
            {
                if (!filePath.Contains("://"))
                {
                    filePath = "file://" + filePath;
                }
            }

            //UnityWebRequest
            Uri uri = new Uri(filePath);
            UnityWebRequest webRequest = new UnityWebRequest(uri);
            webRequest.timeout = 10;

            //DownloadHandler
            DownloadHandlerTexture downloadHandler = new DownloadHandlerTexture();
            webRequest.downloadHandler = downloadHandler;

            //等待加载
            yield return webRequest.SendWebRequest();

            //如果其 请求失败，或是 网络错误
            if (webRequest.isHttpError || webRequest.isNetworkError)
            {
                //打印错误原因
                Debugger.LogDError("error in LoadGameBgFromStreamingWebRequest: " + webRequest.error + ".  file: " +
                                   filePath);
                yield break;
            }
            else
            {
                Texture2D tempTexture2D =
                    new Texture2D(downloadHandler.texture.width, downloadHandler.texture.height, TextureFormat.RGBA32,
                        false);
                tempTexture2D.SetPixels(downloadHandler.texture.GetPixels());
                tempTexture2D.wrapMode = TextureWrapMode.Clamp;
                tempTexture2D.Apply();

                //创建一个Sprite, 开销很大
                newSpriteBg = Sprite.Create(tempTexture2D,
                    new Rect(0, 0, downloadHandler.texture.width, downloadHandler.texture.height),
                    Vector2.zero);


                downloadHandler.Dispose();
                webRequest.Dispose();

                tempTexture2D = null;

            }


            if (newSpriteBg != null)
            {
                //缓存超限清除
                if (gameBgSpriteFromStreamingPool.Count > MaxGameBgAtlasCountInPool)
                {
                    CleanResources();
                }

                //添加新图入缓存
                if (!gameBgSpriteFromStreamingPool.ContainsKey(fileName))
                {
                    gameBgSpriteFromStreamingPool.Add(fileName, newSpriteBg);
                }

                if (loadCmplCallback != null && loadCmplCallback.Target != null)
                {
                    loadCmplCallback(gameBgSpriteFromStreamingPool[fileName]);
                }
            }
            else
            {
                Debugger.LogDError("error in LoadGameBgFromStreaming newSpriteBg is null");
                if (loadCmplCallback != null && loadCmplCallback.Target != null)
                {
                    loadCmplCallback(null);
                }
            }

            yield return true;
        }


        /// <summary>
        /// 缓存中没有, 去streamingAssets中读取
        /// 1. 返回Texture, 不需要创建Sprite, 避免大的开销
        /// 2. 以block的形式组织代码, 更安全地管理内存
        /// </summary>
        /// <param name="fileName"></param>
        /// <returns></returns>
        public IEnumerator LoadTextureFromStreamingWebRequest(string fileName,
            Action<Texture> loadCmplCallback = null)
        {
            //文件名为空
            if (string.IsNullOrWhiteSpace(fileName))
            {
                if (loadCmplCallback != null)
                {
                    loadCmplCallback(null);
                }

                //直接结束协程
                yield break;
            }
            
            //使用缓存
            if (gameBgTextureFromStreamingPool.ContainsKey(fileName))
            {
                // PrintDic();
                if (loadCmplCallback != null && loadCmplCallback.Target != null)
                {
                    loadCmplCallback(gameBgTextureFromStreamingPool[fileName]);
                }

                //直接结束协程
                yield break;
            }
            

            string filePath = Application.streamingAssetsPath + "/" + BG_SPRITE_PATH + fileName + ".jpg";
            if (Application.platform != RuntimePlatform.WebGLPlayer)
            {
                if (!filePath.Contains("://"))
                {
                    filePath = "file://" + filePath;
                }
            }

            //UnityWebRequest
            Uri uri = new Uri(filePath);

            Texture2D tempTexture2D = null;
            using (UnityWebRequest request = new UnityWebRequest(uri, UnityWebRequest.kHttpVerbGET))
            {
                request.downloadHandler = new DownloadHandlerTexture();
                //request.timeout = 10;
                yield return request.SendWebRequest();
 
                if (request.isNetworkError || request.isHttpError)
                {
                    Debugger.LogDError("error in LoadGameBgFromStreamingWebRequest: " + request.error + ".  file: " +
                                       filePath);
                    yield break;
                }
                else
                {
                    tempTexture2D = DownloadHandlerTexture.GetContent(request);
                }
            }

            if (tempTexture2D != null)
            {
                //缓存超限清除
                if (gameBgTextureFromStreamingPool.Count > MaxGameBgAtlasCountInPool)
                {
                    gameBgTextureFromStreamingPool.Clear();
                }

                //添加新图入缓存
                if (!gameBgTextureFromStreamingPool.ContainsKey(fileName))
                {
                    gameBgTextureFromStreamingPool.Add(fileName, tempTexture2D);
                }

                if (loadCmplCallback != null && loadCmplCallback.Target != null)
                {
                    loadCmplCallback(gameBgTextureFromStreamingPool[fileName]);
                }

                //这个先不自动卸载了，外部找时机去卸载
                //回调后, 移除无用资源
                //Resources.UnloadUnusedAssets();
            }
            else
            {
                Debugger.LogDError("error in LoadGameBgFromStreaming newSpriteBg is null");
                if (loadCmplCallback != null && loadCmplCallback.Target != null)
                {
                    loadCmplCallback(null);
                }
            }

            yield return true;
        }

        /// <summary>
        /// 卸载无用资源
        /// </summary>
        public AsyncOperation UnloadUnusedAssets()
        {
            return Resources.UnloadUnusedAssets();
        }

        /// <summary>
        /// 清理时机
        /// </summary>
        private DateTime lastCleanTime = DateTime.Now;
        private const float MIN_CLEAN_POOL_INTERVAL = 1f;

        public void CleanResources()
        {
            float waitTime = Mathf.Abs((float) (DateTime.Now - lastCleanTime).TotalSeconds);
            Debugger.LogD("try clear gameBgSpriteFromStreamingPool " + waitTime.ToString());
            if (waitTime > MIN_CLEAN_POOL_INTERVAL)
            {
                Debugger.LogD("clear gameBgSpriteFromStreamingPool");
                
                gameBgSpriteFromStreamingPool.Clear();
                //多次切换后, 清理内存
                Resources.UnloadUnusedAssets();
                //System.GC.Collect();

                Debugger.LogDWarning("UnloadUnusedAssets1");
                lastCleanTime = DateTime.Now;
            }
        }
        
        
    }
}