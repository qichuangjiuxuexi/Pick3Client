using System.Collections;
using AppBase;
using AppBase.Config;
using AppBase.Debugging;
using AppBase.Event;
using AppBase.GetOrWait;
using AppBase.Localization;
using AppBase.Resource;
using AppBase.Sound;
using AppBase.Timing;
using AppBase.UI.Dialog;
using AppBase.UI.Scene;
using AppBase.UI.Waiting;
using UnityEngine;

/// <summary>
/// 与用户无关的游戏模块
/// </summary>
public partial class Game : GameBase
{
    public static Game Module => (Game)Instance;
    public static EventManager Event { get; private set; }
    public static GetOrWaitManager GetWait { get; private set; }
    public static ResourceManager Resource { get; private set; }
    public static ConfigManager Config { get; private set; }
    public static LocalizationManager Localization { get; private set; }
    public static DialogManager Dialog { get; private set; }
    public static ObjectAnimationModule OA { get; private set; }

    public static SoundManager Sound { get; private set; }
    public static SceneManager Scene { get; private set; }
    public static CameraManager Cameras { get; private set; }
    public static TimingManager Timing { get; private set; }
    public static WaitingManager Waiting { get; private set; }

    /// <summary>
    /// 这里初始化既不依赖配置，也不依赖存档的模块
    /// </summary>
    protected override void OnInit()
    {
        Event = AddModule<EventManager>();
        GetWait = AddModule<GetOrWaitManager>();
        Timing = AddModule<TimingManager>();
        Resource = AddModule<ResourceManager>();
        Cameras = AddModule<CameraManager>();
        Scene = AddModule<SceneManager>();
        Dialog = AddModule<DialogManager>();
        Waiting = AddModule<WaitingManager>();
        Sound = AddModule<SoundManager>();
        OA = AddModule<ObjectAnimationModule>();
        Config = AddModule<ConfigManager>();
    }

    /// <summary>
    /// 这里初始化依赖配置，但不依赖存档的模块
    /// </summary>
    public override IEnumerator InitAfterConfig()
    {
        Localization = AddModule<LocalizationManager>();
        AddAllLanguage();
        bool fontLoaded = false;
        Localization.InitFontAsset((_)=>fontLoaded = true);
        yield return new WaitUntil(()=>fontLoaded);
    }
    
    private void AddAllLanguage()
    {

    }


    /// <summary>
    /// 启动游戏
    /// </summary>
    public static void Start()
    {
        if (Instance != null) return;
        var game = new Game();
        game.Init();
    }
}
