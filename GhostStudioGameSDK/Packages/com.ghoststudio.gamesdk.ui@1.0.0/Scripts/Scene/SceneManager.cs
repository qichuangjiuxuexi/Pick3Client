using System;
using System.Linq;
using AppBase.Event;
using AppBase.Module;
using AppBase.Resource;
using AppBase.Utils;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace AppBase.UI.Scene
{
    /// <summary>
    /// 场景管理器，场景是一个全屏的UI，位于UICanvas/Scenes下，只有一个场景是激活的
    /// </summary>
    public class SceneManager : ModuleBase
    {
        protected override void OnInit()
        {
            base.OnInit();
            if (NormalSceneRoot) NormalSceneRoot.SetActive(true);
            if (UISceneRoot) UISceneRoot.SetActive(true);
            if (UISceneRoot) CurrentSceneObj = UISceneRoot.transform.Find("SplashScene")?.gameObject;
            if (CurrentSceneObj) CurrentSceneObj.GetOrAddComponent<UIScene>();
            AddModule<EventModule>().Subscribe<OnGameRestartEvent>(OnGameRestart);
        }

        /// <summary>
        /// Normal场景挂点
        /// </summary>
        public GameObject NormalSceneRoot
        {
            get
            {
                if (_normalSceneRoot != null) return _normalSceneRoot;
                _normalSceneRoot = GameObject.Find("Scenes");
                if (_normalSceneRoot == null)
                {
                    _normalSceneRoot = new GameObject("Scenes");
                    _normalSceneRoot.transform.SetAsFirstSibling();
                }
                GameObject.DontDestroyOnLoad(_normalSceneRoot);
                return _normalSceneRoot;
            }
        }
        private GameObject _normalSceneRoot;

        /// <summary>
        /// UI场景挂点
        /// </summary>
        private GameObject UISceneRoot
        {
            get
            {
                if (_uiSceneRoot != null) return _uiSceneRoot;
                var canvas = GameObject.Find("UICanvas");
                if (canvas == null)
                {
                    Debugger.LogError(TAG, "UICanvas not found");
                    return null;
                }
                GameObject.DontDestroyOnLoad(canvas);
                _uiSceneRoot = canvas.transform.Find("Scenes")?.gameObject;
                if (_uiSceneRoot == null)
                {
                    _uiSceneRoot = canvas.AddFullScreenRectTransform().gameObject;
                    _uiSceneRoot.name = "Scenes";
                    _uiSceneRoot.transform.SetAsFirstSibling();
                }
                return _uiSceneRoot;
            }
        }
        private GameObject _uiSceneRoot;
        
        /// <summary>
        /// 当前场景的GameObject
        /// </summary>
        private GameObject CurrentSceneObj;

        /// <summary>
        /// 当前的场景的SceneBase
        /// </summary>
        public SceneBase CurrentScene => CurrentSceneObj != null && CurrentSceneObj.TryGetComponent(out SceneBase scene) ? scene : null;

        /// <summary>
        /// 当前场景的数据
        /// </summary>
        public SceneData CurrentSceneData => CurrentScene != null ? CurrentScene.sceneData : null;
        
        /// <summary>
        /// 切换场景前的场景数据
        /// </summary>
        public SceneData LastSceneData { get; private set; }

        /// <summary>
        /// 切换场景
        /// </summary>
        /// <param name="sceneData">场景数据</param>
        public SceneData SwitchScene(SceneData sceneData)
        {
            if (sceneData == null || string.IsNullOrEmpty(sceneData.address)) return sceneData;
            if (sceneData is TransitionData transData) transData.PreSceneData = CurrentSceneData;
            var resource = GameBase.Instance.GetModule<ResourceManager>();
            switch (sceneData.sceneType)
            {
                case SceneType.NormalScene:
                case SceneType.UIScene:
                    sceneData.handler = resource.LoadAssetHandler<GameObject>(sceneData.address, h => OnSceneLoaded(h, sceneData), () => sceneData.handler = null);
                    break;
                case SceneType.SingleUnityScene:
                    sceneData.handler = resource.LoadUnitySceneHandler(sceneData.address, LoadSceneMode.Single, false, h => OnSceneLoaded(h, sceneData), () => sceneData.handler = null);
                    break;
                case SceneType.AdditiveUnityScene:
                    sceneData.handler = resource.LoadUnitySceneHandler(sceneData.address, LoadSceneMode.Additive, false, h => OnSceneLoaded(h, sceneData), () => sceneData.handler = null);
                    break;
            }
            return sceneData;
        }

        /// <summary>
        /// 场景加载完毕，播放入场动画
        /// </summary>
        private void OnSceneLoaded(ResourceHandler handler, SceneData newSceneData)
        {
            var oldSceneObj = CurrentSceneObj;
            var oldScene = CurrentScene;
            var oldSceneData = CurrentSceneData;
            LastSceneData = oldSceneData;
            SceneBase newScene = null;
            var flow = FlowUtil.Create();
            
            //SingleUnityScene -> SingleUnityScene
            //时序：oldScene.OnPlayExitAnim -> oldScene.OnBeforeDestroy -> oldScene.OnDestroy -> Awake -> OnLoad -> OnAwake -> OnPlayEnterAnim
            if (oldSceneData != null && oldSceneData.sceneType == SceneType.SingleUnityScene && newSceneData.sceneType == SceneType.SingleUnityScene)
            {
                flow.Add(oldScene.OnPlayExitAnim);
                flow.Add(oldScene.OnBeforeDestroy);
                flow.Add(() =>
                {
                    oldScene.OnInternalDestroy();
                    OnDestroyScene(oldSceneData);
                });
                flow.Add(ActivateUnityScene);
                flow.Add(next =>
                {
                    newSceneData.OnLoadedCallback(newScene);
                    newScene.OnLoad(next);
                });
                flow.Add(next => newScene.OnAwake(next));
                flow.Add(next => newScene.OnPlayEnterAnim(next));
            }
            else
            {
                //NormalScene -> UnityScene
                //时序：Awake -> OnLoad -> OnAwake -> oldScene.OnPlayExitAnim -> OnPlayEnterAnim -> oldScene.OnBeforeDestroy -> oldScene.OnDestroy
                if (newSceneData.sceneType is SceneType.SingleUnityScene or SceneType.AdditiveUnityScene)
                {
                    flow.Add(ActivateUnityScene);
                    flow.Add(next =>
                    {
                        newSceneData.OnLoadedCallback(newScene);
                        newScene.OnLoad(next);
                    });
                }
                //NormalScene -> NormalScene
                //时序：OnLoad -> Awake -> OnAwake -> oldScene.OnPlayExitAnim -> OnPlayEnterAnim -> oldScene.OnBeforeDestroy -> oldScene.OnDestroy
                else
                {
                    flow.Add(InitGameObject);
                    flow.Add(next =>
                    {
                        newSceneData.OnLoadedCallback(newScene);
                        newScene.OnLoad(next);
                    });
                    flow.Add(() => CurrentSceneObj.SetActive(true));
                }
                flow.Add(next => newScene.OnAwake(next));
                if (oldScene != null) flow.Add(oldScene.OnPlayExitAnim);
                flow.Add(next => newScene.OnPlayEnterAnim(next));
                if (oldScene != null) flow.Add(oldScene.OnBeforeDestroy);
                if (oldScene != null) flow.Add(oldScene.OnInternalDestroy);
                if (oldSceneObj != null) flow.Add(() =>
                {
                    GameObject.Destroy(oldSceneObj);
                    OnDestroyScene(oldSceneData);
                });
            }
            flow.Invoke(() =>
            {
                GameBase.Instance.GetModule<EventManager>().Broadcast(new AfterSwitchSceneEvent(newSceneData));
                newSceneData.OnSwitchCallback(newScene);
            });

            //初始化普通场景
            void InitGameObject()
            {
                var prefab = handler.GetAsset<GameObject>();
                prefab.SetActive(false);
                var prefabScene = prefab.GetComponent<SceneBase>();
                var isUIScene = prefabScene is UIScene || newSceneData.sceneType == SceneType.UIScene;
                var parentRoot = isUIScene ? UISceneRoot : NormalSceneRoot;
                CurrentSceneObj = parentRoot.AddInstantiate(prefab);
                newScene = isUIScene ? CurrentSceneObj.GetOrAddComponent<UIScene>() : CurrentSceneObj.GetOrAddComponent<SceneBase>();
                CurrentSceneObj.GetResourceReference().AddHandler(handler);
                handler.Release();
                newScene.sceneData = newSceneData;
            }
            
            //激活Unity场景
            void ActivateUnityScene(Action next)
            {
                handler.ActivateUnityScene(scene =>
                {
                    newScene = scene.GetRootGameObjects().Select(x => x.GetComponent<SceneBase>()).FirstOrDefault(x => x != null);
                    if (newScene != null) CurrentSceneObj = newScene.gameObject;
                    else
                    {
                        CurrentSceneObj = new GameObject("_UnityScene");
                        newScene = CurrentSceneObj.AddComponent<UnityScene>();
                    }
                    if (newScene is UnityScene unityScene) unityScene.unityScene = scene;
                    CurrentSceneObj.GetResourceReference().AddHandler(handler);
                    next();
                });
            }
        }

        private void OnDestroyScene(SceneData sceneData)
        {
            if (sceneData != null && !string.IsNullOrEmpty(sceneData.address))
            {
                GameBase.Instance.GetModule<EventManager>().Broadcast(new DestroySceneEvent(sceneData.address)); 
            }
        }

        private async UniTask OnGameRestart(OnGameRestartEvent evt)
        {
            await SwitchScene(new SceneData(AAConst.GetAddress(AppBaseProjectConst.DefaultGameRestartSceneName), null, SceneType.UIScene));
        }
    }
}
