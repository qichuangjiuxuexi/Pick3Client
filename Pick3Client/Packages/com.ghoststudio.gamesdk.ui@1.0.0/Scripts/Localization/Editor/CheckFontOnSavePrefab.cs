
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

public class CheckFontOnSavePrefab
{
    [InitializeOnLoadMethod]
    static void AddSavePrefabCallBack()
    {
        PrefabStage.prefabSaving += OnSaving;
    }

    private static void OnSaving(GameObject go)
    {
        var textBinders = go.transform.GetComponentsInChildren<LocalizationTextBinder>(true);
        foreach (var binder in textBinders)
        {
            if (binder.curLanguage != SystemLanguage.English)
            {
                var config = TextMeshBindEditor.GetConfig(SystemLanguage.English);
                if (config != null)
                {
                    TextMeshBindEditor.SetText(binder, config);
                }
                TextMeshBindEditor.ChangeFontInEditor(binder, SystemLanguage.English);
                binder.curLanguage = SystemLanguage.English;
            }
        }
    }
}