using System;
using AppBase.Event;
using AppBase.UI.Scene;
using DG.Tweening;
using TMPro;
using UnityEngine.UI;

/// <summary>
/// 启动场景
/// </summary>
public class LaunchScene : UIScene
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
        
    }
}
