using UnityEngine;
using UnityEngine.UI;

public class BlurBoundsOffsets : MonoBehaviour
{
    /// <summary>
    /// 目标图片
    /// </summary>
    [SerializeField] private Image TargetBgImage;

    /// <summary>
    /// 目标图片
    /// </summary>
    [SerializeField] private RectTransform TargetLittleRectTransform;

    private RectTransform selfRectTransform = null;

    public Material _Material = null;

    // Start is called before the first frame update
    private void Start()
    {
        SetImageSize();
    }

    // Update is called once per frame
    private void Update()
    {
    }

    [ContextMenu("SetImageSize")]
    private void SetImageSize()
    {
        Vector3[] selfBounds = new Vector3[4];
        Vector3[] bgBounds = new Vector3[4];

        TargetLittleRectTransform.GetWorldCorners(selfBounds);
        TargetBgImage.rectTransform.GetWorldCorners(bgBounds);

        selfBounds = WordPosToScreentPos(selfBounds);
        bgBounds = WordPosToScreentPos(bgBounds);

        Vector4 tempScale = new Vector4(0, 0, 0, 0);

        float imageH = bgBounds[2].y - bgBounds[0].y;
        float imageW = bgBounds[2].x - bgBounds[0].x;

        float selfH = selfBounds[2].y - selfBounds[0].y;
        float selfW = selfBounds[2].x - selfBounds[0].x;


        tempScale[0] = (selfBounds[0].x - bgBounds[0].x) / imageW;
        tempScale[1] = selfW / imageW;
        tempScale[2] = (selfBounds[0].y - bgBounds[0].y) / imageH;
        tempScale[3] = selfH / imageH;

        _Material.SetVector("_Bounds", tempScale);
    }

    //模糊半径  
    public float BlurRadius = 1.0f;

    //降分辨率  
    public int downSample = 2;

    //迭代次数  
    public int iteration = 1;

    [ContextMenu("SetBlurInfo")]
    private void OnRenderImage(RenderTexture source, RenderTexture destination)
    {
        if (_Material)
        {
            //申请RenderTexture，RT的分辨率按照downSample降低  
            RenderTexture rt1 = RenderTexture.GetTemporary(source.width >> downSample, source.height >> downSample, 0,
                source.format);
            RenderTexture rt2 = RenderTexture.GetTemporary(source.width >> downSample, source.height >> downSample, 0,
                source.format);

            //直接将原图拷贝到降分辨率的RT上  
            Graphics.Blit(source, rt1);

            //进行迭代高斯模糊  
            for (int i = 0; i < iteration; i++)
            {
                //第一次高斯模糊，设置offsets，竖向模糊  
                _Material.SetVector("offsets", new Vector4(0, BlurRadius, 0, 0));
                Graphics.Blit(rt1, rt2, _Material);
                //第二次高斯模糊，设置offsets，横向模糊  
                _Material.SetVector("offsets", new Vector4(BlurRadius, 0, 0, 0));
                Graphics.Blit(rt2, rt1, _Material);
            }

            //将结果输出  
            Graphics.Blit(rt1, destination);

            //释放申请的两块RenderBuffer内容  
            RenderTexture.ReleaseTemporary(rt1);
            RenderTexture.ReleaseTemporary(rt2);
        }
    }


    /// <summary>
    /// 世界坐标 转换为 屏幕坐标
    /// </summary>
    /// <param name="bounds"></param>
    /// <returns></returns>
    private Vector3[] WordPosToScreentPos(Vector3[] bounds)
    {
        Vector3[] temp = new Vector3[4];

        for (int i = 0, iMax = bounds.Length; i < iMax; i++)
        {
            temp[i] = Camera.main.WorldToScreenPoint(bounds[i]);
        }

        return temp;
    }
}