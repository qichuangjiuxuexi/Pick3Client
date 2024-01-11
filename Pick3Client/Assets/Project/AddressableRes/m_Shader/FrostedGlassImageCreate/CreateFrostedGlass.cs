using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CreateFrostedGlass : MonoBehaviour
{
    /// <summary>
    /// Ŀ��ͼƬ
    /// </summary>
    [SerializeField] private Image TargetBgImage;

    // Start is called before the first frame update
    void Start()
    {
    }

    // Update is called once per frame
    void Update()
    {
    }


    [ContextMenu("SetImageSize")]
    private void CreateImage()
    {
        //Texture tempTexture = TargetBgImage.materialForRendering.GetColorArray

        //Texture2D beginTexture = new Texture2D(tempTexture.width, tempTexture.height, TextureFormat.RGB24, false);
        //beginTexture.SetPixels32(tempTexture,);
    }
}