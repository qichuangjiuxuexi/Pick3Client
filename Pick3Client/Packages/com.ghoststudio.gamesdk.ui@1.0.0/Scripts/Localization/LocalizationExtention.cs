using AppBase.UI;
using TMPro;
using UnityEngine.UI;

public static class LocalizationExtention
{
    /// <summary>
    /// 设置本地化Key
    /// </summary>
    public static void SetLocalizationText(this TMP_Text component, string key)
    {
        if (component == null) return;
        var binder = component.GetOrAddComponent<LocalizationTextBinder>();
        if (binder != null)
        {
            binder.SetKey(key);
        }
    }
    
    /// <summary>
    /// 设置本地化Key
    /// </summary>
    public static void SetLocalizationText(this TMP_Text component, int key)
    {
        if (component == null) return;
        var binder = component.GetOrAddComponent<LocalizationTextBinder>();
        if (binder != null)
        {
            binder.SetKey(key.ToString());
        }
    }
    
    /// <summary>
    /// 设置本地化Key
    /// </summary>
    public static void SetLocalizationText(this Text component, string key)
    {
        if (component == null) return;
        var binder = component.GetOrAddComponent<LocalizationTextBinder>();
        if (binder != null)
        {
            binder.SetKey(key);
        }
    }
    
    /// <summary>
    /// 设置本地化Key
    /// </summary>
    public static void SetLocalizationText(this Text component, int key)
    {
        if (component == null) return;
        var binder = component.GetOrAddComponent<LocalizationTextBinder>();
        if (binder != null)
        {
            binder.SetKey(key.ToString());
        }
    }
}
