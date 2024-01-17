using System;
using DG.Tweening;
using Project.AppBase.Audio;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using WordGame.Utils;
using WordGame.Utils.Timer;

public static class UICommonFun 
{
    public static void AddButtonLisenter(Button btn,UnityAction<GameObject> action, bool isOnce = false, bool isNoPlayAudio = false)
    {
        if (btn != null)
        {
            btn.onClick.RemoveAllListeners();
            btn.onClick.AddListener(delegate ()
            {
                if (isOnce)
                    btn.SetInteractable(false);
                if (!isNoPlayAudio)
                    Game.Sound.PlayAudio(DefaultSoundNameConst.CLICK);
                action(btn.gameObject);
            });
        }
    }

    public static void AddButtonLisenter(Button btn, UnityAction action, bool isOnce = false, bool isNoPlayAudio = false)
    {
        if (btn != null)
        {
            btn.onClick.RemoveAllListeners();
            btn.onClick.AddListener(delegate ()
            {
                if (isOnce)
                    btn.SetInteractable(false);
                if (!isNoPlayAudio)
                    TimeUpdateMgr.Instance.AddWaitSecondToPlay(0.1f,()=>
                     Game.Sound.PlayAudio(DefaultSoundNameConst.CLICK));
                action();
            });
        }
    }

    public static void SetButtonInteractable(Button btn, bool interactable)
    {
        if (btn != null)
        {
            btn.SetInteractable(interactable);
        }
    }
    public static void AddButtonDownLisenter(EventTrigger trigger,UnityAction<BaseEventData> action)
    {
        if (trigger == null)
            return;

        EventTrigger.Entry entry = new EventTrigger.Entry();

        entry.eventID = EventTriggerType.PointerDown;

        entry.callback = new EventTrigger.TriggerEvent();

        entry.callback.AddListener((e) =>
        {
            Game.Sound.PlayAudio(DefaultSoundNameConst.CLICK);
            action(e);
        });


        trigger.triggers.Add(entry);

    }

    public static void AddButtonUpLisenter(EventTrigger trigger,UnityAction<BaseEventData> action)
    {

        if (trigger == null)
            return;

        EventTrigger.Entry entry = new EventTrigger.Entry();

        entry.eventID = EventTriggerType.PointerUp;

        entry.callback = new EventTrigger.TriggerEvent();

        entry.callback.AddListener((e) =>
        {
            Game.Sound.PlayAudio(DefaultSoundNameConst.CLICK);
            action(e);
        });


        trigger.triggers.Add(entry);
    }




    public static void AddSliderLisenter(Slider btn ,UnityAction<float> action)
    {
        if (btn != null)
        {
            btn.onValueChanged.RemoveAllListeners();
            btn.onValueChanged.AddListener(action);
        }
    }



    public static void AddInputFiledLisenter(InputField btn,UnityAction<string> action)
    {
        if (btn != null)
        {
            btn.onValueChanged.RemoveAllListeners();
            btn.onValueChanged.AddListener(action);
        }
    }
    
    public static void AddInputFiledEndLisenter(InputField btn,UnityAction<string> action)
    {
        if (btn != null)
        {
            btn.onEndEdit.RemoveAllListeners();
            btn.onEndEdit.AddListener(action);
        }
    }

    public static void AddToggleLisenter(Toggle btn,UnityAction<bool> action)
    {
        if (btn != null)
        {
            btn.onValueChanged.RemoveAllListeners();
            btn.onValueChanged.AddListener((e) =>
            {
                Game.Sound.PlayAudio(DefaultSoundNameConst.CLICK);
                action(e);
            });
        }
    }


    public static void SetTextMeshProUGUI(TextMeshProUGUI textMesh, string key, params string[] param)
    {
        if (textMesh != null)
        {
            var binder = textMesh.GetOrAddComponent<LocalizationTextBinder>();
            binder.SetKey(key, param);
        }
    }
    public static void SetTextNokey(TMP_Text textMesh, string txt)
    {
        if (textMesh != null)
            textMesh.text = txt;
    }
    

    public static void SetImageSprite(Image icon,string spriteName, Action<Image, Sprite> action = null)
    {
        if (icon == null)
            action?.Invoke(null, null);
        else
        {
            Game.Resource.LoadAsset<Sprite>(AAConst.GetAddress(spriteName), icon.GetResourceReference(), sprite =>
            {
                icon.sprite = sprite;
                action?.Invoke(icon, sprite);
            });
        }
    }
    public static void SetImageFillAmount(Image icon,float fillAmount)
    {
        if (icon != null)
            icon.fillAmount = fillAmount;
    }
    public static Tween SetImageDoFillAmount(Image icon,float fillAmount, float duration, Action callBack = null)
    {
        if (icon != null)
        {
            Tween tween = icon.DOFillAmount(fillAmount, duration);
            tween.onComplete = () => callBack?.Invoke();
            return tween;
        }
        return null;
    }
    
}
