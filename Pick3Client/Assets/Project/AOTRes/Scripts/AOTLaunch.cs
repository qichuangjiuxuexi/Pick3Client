using System.Collections;
using System.Reflection;
using UnityEngine;
using UnityEngine.Scripting;

public class AOTLaunch : MonoBehaviour
{
    protected void Start()
    {
        StartCoroutine(CreateLaunchLoading());
    }

    private IEnumerator CreateLaunchLoading()
    {
        yield return null;
        var assembly = Assembly.Load("HotfixAsm");
        var type = assembly.GetType("LaunchLoadingControl");
        var method = type.GetMethod("Create", BindingFlags.Public | BindingFlags.Static);
        yield return null;
        method.Invoke(null, new object[] { });
    }
    
    [Preserve]
    public static void Create()
    {
        var scenes = GameObject.Find("UICanvas")?.transform.Find("Scenes");
        var trans = scenes?.Find("SplashScene") ?? scenes?.Find("LaunchScene");
        var go = trans?.gameObject;
        if (go == null)
        {
            Debug.LogError("[AOTLaunch] Create: Can't find SplashScene or LaunchScene");
            return;
        }
        var aotLaunch = go.GetComponent<AOTLaunch>();
        if (aotLaunch == null)
        {
            go.AddComponent<AOTLaunch>();
        }
    }
}
