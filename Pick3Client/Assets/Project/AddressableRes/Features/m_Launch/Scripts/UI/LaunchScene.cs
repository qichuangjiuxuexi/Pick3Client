using System;
using AppBase.Event;
using AppBase.UI.Scene;
using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using WordGame.Utils;

/// <summary>
/// 启动场景
/// </summary>
public partial class LaunchScene : UIScene
{
    public Image progressBar;
    public TextMeshProUGUI progressTxt;

    private Tweener mProgressTweener;
    private float mCurrentTargetProgress;

    private static bool sendLoadingEvent = false;
    protected void Awake()
    {
        gameObject.AddComponent<EventBehaviour>()
            .Subscribe<LoadingProgressEvent>(OnProgressUpdate);
        
        UICommonFun.AddButtonLisenter(StartButton.Button, () =>
        {
            // Game.Scene.SwitchScene(new TransitionData(AAConst.PickScene, new UISceneData(AAConst.PickScene)));
            Game.Scene.SwitchScene(new UISceneData(AAConst.PickScene));
        });
    }

    protected void OnProgressUpdate(LoadingProgressEvent e)
    {
        // progressBar.fillAmount = e.progress;
        if (mCurrentTargetProgress >= e.progress)
        {
            return;
        }
        mCurrentTargetProgress = e.progress;
        
        if (mProgressTweener != null)
        {
            mProgressTweener.Kill();
        }

        if (mCurrentTargetProgress == 1)
        {
            progressBar.fillAmount = 1;
            progressTxt?.SetText("100%");
            ProgressBar.SetActive(false);
            ButtonParent.SetActive(true);
        }
        else
        {
            mProgressTweener = progressBar.DOFillAmount(mCurrentTargetProgress, 0.5f).SetEase(Ease.Linear).OnUpdate(() =>
            {
                progressTxt?.SetText((int)(progressBar.fillAmount * 100) + "%");
            });
        }
    }

    private DateTime startLoadTime;
    public override void OnPlayEnterAnim(Action callback)
    {
        startLoadTime = DateTime.Now;
        if(!sendLoadingEvent)
            // Game.Analytics.TrackEvent(new AnalyticsEvent_loading_step(1,0));
        sendLoadingEvent = true;
        callback?.Invoke();
    }

    public override void OnPlayExitAnim(Action callback)
    {
        callback?.Invoke();
    }
}
