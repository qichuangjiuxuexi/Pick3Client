#if UNITY_EDITOR
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
[CreateAssetMenu(fileName = "ObjectAnimationSetting", menuName = "OA动画/Setting")]
public class OAEditorSettings : ScriptableObject
{
    public List<string> assemblys = new List<string>() { "GameSDK.OA","HotfixAsm" };
    private static OAEditorSettings _instance;
    public static OAEditorSettings instance
    {
        get
        {
            if (_instance == null)
            {
                CheckSettings();
            }
            return _instance;
        }
        set
        {
            _instance = value;
        }
    }
    
    [InitializeOnLoadMethod]
    public static void CheckSettings()
    {
        string[] guids = AssetDatabase.FindAssets($"t:{typeof(OAEditorSettings).Name}");
        if (guids.Length == 0)
        {
            if (EditorUtility.DisplayDialog("注意", "OA动画配置文件未创建，会导致一些辅助编辑配置功能的缺失，但是不影响运行时功能，需要自动创建一个吗？(也可以通过在合适的文件夹内右击，依次选择菜单:[Create->OA动画/Setting]在需要的位置创建)", "创建", "不了"))
            {
                var obj = ScriptableObject.CreateInstance<OAEditorSettings>();
                string dir = "Assets/Editor";
                if (!AssetDatabase.IsValidFolder(dir))
                {
                    AssetDatabase.CreateFolder("Assets","Editor");
                }
                AssetDatabase.CreateAsset(obj,dir + "/OAEditorSettings.asset");
                instance = obj;
                EditorGUIUtility.PingObject(obj); 
            }
        }
        else
        {
            instance = AssetDatabase.LoadAssetAtPath<OAEditorSettings>(AssetDatabase.GUIDToAssetPath(guids[0]));
            if (guids.Length > 1)
            { 
                for (int i = 0; i < guids.Length; i++)
                {
                    guids[i] = AssetDatabase.GUIDToAssetPath(guids[i]);
                }

                Debug.LogError($"发现多个OAEditorSettings配置文件，模式使用发现的第一个。建议删除多余的，所有配置文件位置如下：{string.Join("\n", guids)}");
            }
        }
    }

}
#endif
