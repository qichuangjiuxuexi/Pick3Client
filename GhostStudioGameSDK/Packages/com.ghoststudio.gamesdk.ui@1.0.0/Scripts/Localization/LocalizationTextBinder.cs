using System;
using AppBase;
using AppBase.Event;
using AppBase.Localization;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class LocalizationTextBinder : MonoBehaviour
{
    public bool useBindKey = true;
    public string key;
    public string[] formatParams;

    private TMP_FontAsset initFont;
    private Material initTextMaterial;
    private TMP_Text text;
    [NonSerialized]
    public SystemLanguage curLanguage = SystemLanguage.English;

    
    private void OnEnable()
    {
        text = GetComponent<TMP_Text>();
        RecordInitFontAndMaterial();
            RefreshText();
            GameBase.Instance.GetModule<EventManager>().Subscribe<LocalizationChangedEvent>(OnLocalizationChanged);
        }

    private void OnDisable()
        {
            GameBase.Instance?.GetModule<EventManager>()?.Unsubscribe<LocalizationChangedEvent>(OnLocalizationChanged);
        }

    public void RefreshText()
    {
        CheckAndChangeFont();
        if (!useBindKey || string.IsNullOrEmpty(key)) return;
        string txt = GameBase.Instance.GetModule<LocalizationManager>().GetText(key);
        //如果读取不到 key 直接显示 key
        if (txt == LocalizationManager.DefaultText)
        {
            txt = key;
        }
        if (formatParams != null && formatParams.Length>0)
        {
            txt = string.Format(txt, formatParams);
        }
        SetText(txt);
    }
    
    public void SetKey(string val, string[] param = null)
    {
        key = val;
        formatParams = param;
        useBindKey = true;
        RecordInitFontAndMaterial();
        RefreshText();
    }

    private void RecordInitFontAndMaterial()
    {
        text ??= GetComponent<TMP_Text>();
        if (text != null && initFont == null)
        {
            initFont = text.font;
            initTextMaterial = text.fontMaterial;
        }
    }
    
    public void SetText(string val)
    {
        if (text != null)
        {
            text.text = val;
        }
        else
        {
            var textPro = GetComponent<Text>();
            if (textPro != null)
            {
                textPro.text = val;
            }
        }
    }

    public void CheckAndChangeFont()
    {
        if (Application.isPlaying && text != null)
        {
            var lmgr = GameBase.Instance.GetModule<LocalizationManager>();
            if (lmgr.NeedChangeFont())
            {
                if (lmgr.CurSpecialFont && text.font != lmgr.CurSpecialFont)
                {
                    text.font = lmgr.CurSpecialFont;
                    var newMaterialName = initTextMaterial.name.Replace(initFont.name, lmgr.CurSpecialFont.name);
                    newMaterialName = newMaterialName.Split(" ")[0];
                    var mat = lmgr.GetNewMaterial(newMaterialName);
                    if (mat)
                    {
                        text.fontMaterial = mat;
                    }
                }
            }
            else
            {
                text.font = initFont;
                text.fontMaterial = initTextMaterial;
            }
        }
    }

    public void SetFont(TMP_FontAsset font)
    {
        if (text != null)
        {
            text.font = font;
        }
    }
    
    public void SetFontInEditor(TMP_FontAsset font, Material mater = null)
    {
        text ??= GetComponent<TMP_Text>();
        if (text != null)
        {
            text.font = font;
            if (mater)
            {
                text.fontMaterial = mater;
            }
        }
    }

    public void ResetFont()
    {
        var text = GetComponent<TMP_Text>();
        if (text != null)
        {
            if(initFont)
                text.font = initFont;
            if(initTextMaterial)
                text.fontSharedMaterial = initTextMaterial;
        }
    }
    
    
    private void OnLocalizationChanged(LocalizationChangedEvent evt)
    {
        RefreshText();
    }
    
    public void SetTextEditor(string val)
    {
        text = GetComponent<TMP_Text>();
        if (text != null)
        {
            text.text = val;
        }
        else
        {
            var textPro = GetComponent<Text>();
            if (textPro != null)
            {
                textPro.text = val;
            }
        }
    }
}