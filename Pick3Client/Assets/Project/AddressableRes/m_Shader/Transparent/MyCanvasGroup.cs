using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;

#endif

public class MyCanvasGroup : MonoBehaviour
{
    public Material ImageMaterial;
    [Range(0, 1)] public float Alpha = 1;

    [HideInInspector] public float lastAlpha;

    private CanvasGroup canvasGroup;

    private void Start()
    {
        canvasGroup = this.GetComponent<CanvasGroup>();
        Alpha = canvasGroup.alpha;
    }

    public void GetMyImages(Material material, float alpha)
    {
        MyImage[] myImages = transform.GetComponentsInChildren<MyImage>();
        for (int i = 0; i < myImages.Length; i++)
        {
            myImages[i].Imagematerial = material;
            myImages[i].Imagealpha = alpha;
        }
    }
}

#if UNITY_EDITOR
[CustomEditor(typeof(MyCanvasGroup))]
public class MyImageConfig : Editor
{
    public override void OnInspectorGUI()
    {
        MyCanvasGroup myCanvasGroup = target as MyCanvasGroup;
        myCanvasGroup.ImageMaterial = new Material(Shader.Find("Custom/MyImage"));
        myCanvasGroup.ImageMaterial =
            EditorGUILayout.ObjectField("Material", myCanvasGroup.ImageMaterial, typeof(Material), true) as Material;
        myCanvasGroup.Alpha = EditorGUILayout.Slider("Alpha", myCanvasGroup.Alpha, 0, 1);

        if (myCanvasGroup.lastAlpha != myCanvasGroup.Alpha)
        {
            myCanvasGroup.GetMyImages(myCanvasGroup.ImageMaterial, myCanvasGroup.Alpha);
            myCanvasGroup.lastAlpha = myCanvasGroup.Alpha;
        }
    }
}
#endif