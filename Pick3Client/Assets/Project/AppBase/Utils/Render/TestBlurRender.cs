using UnityEngine;

namespace WordGame.Utils
{
    public class TestBlurRender : PostEffectBase
    {
        private static TestBlurRender sInstance = null;

        public static TestBlurRender Instance
        {
            get { return sInstance; }
        }

        //模糊半径  
        public float BlurRadius = 1.0f;

        //降分辨率  
        private int downSample = 1;

        //迭代次数  
        public int iteration = 1;

        [HideInInspector] public RenderTexture targetImage = null;

        [HideInInspector] public bool renderLock = true;

        private Vector2 scaleInfo = new Vector2(0, 0);
        private Vector2 offsetInfo = new Vector2(0, 0);

        private RenderTexture rt1 = null;

        private void Awake()
        {
            sInstance = this;
        }

        // Start is called before the first frame update
        private void Start()
        {
        }

        // Update is called once per frame
        private void Update()
        {
        }

        /// <summary>
        /// 获取 小图 在 大图中的 所占区域
        /// </summary>
        /// <param name="littleRectTransform"></param>
        /// <param name="bigRectTransform"></param>
        /// <returns></returns>
        [ContextMenu("SetImageSize")]
        public void SetImageSize(RectTransform littleRectTransform, RectTransform bigRectTransform)
        {
            Vector3[] littleBounds = new Vector3[4];
            //Vector3[] bigBounds = new Vector3[4];

            littleRectTransform.GetWorldCorners(littleBounds);
            //bigRectTransform.GetWorldCorners(bigBounds);

            littleBounds = WordPosToScreentPos(littleBounds, true);
            //bigBounds = WordPosToScreentPos(bigBounds, false);

            //float bigH = bigBounds[2].y - bigBounds[0].y;
            //float bigW = bigBounds[2].x - bigBounds[0].x;

            float bigH = Screen.height;
            float bigW = Screen.width;

            float llittleH = littleBounds[2].y - littleBounds[0].y;
            float littleW = littleBounds[2].x - littleBounds[0].x;

            scaleInfo[0] = littleW / bigW;
            scaleInfo[1] = llittleH / bigH;

            //offsetInfo[0] = (littleBounds[0].x - bigBounds[0].x) / bigW;
            //offsetInfo[1] = (littleBounds[0].y - bigBounds[0].y) / bigH;

            offsetInfo[0] = (littleBounds[0].x - 0) / bigW;
            offsetInfo[1] = (littleBounds[0].y - 0) / bigH;
        }

        /// <summary>
        /// 渲染的 后处理
        /// </summary>
        /// <param name="source"></param>
        /// <param name="destination"></param>
        private void OnRenderImage(RenderTexture source, RenderTexture destination)
        {
            //首先将正常图像输出
            Graphics.Blit(source, destination);

            if (_Material && !renderLock)
            {
                int w = source.width >> downSample;
                int h = source.height >> downSample;

                //申请RenderTexture，RT的分辨率按照downSample降低  
                if (rt1 == null)
                {
                    rt1 = RenderTexture.GetTemporary(w, h, 0, source.format);
                }

                if (targetImage == null)
                {
                    targetImage = RenderTexture.GetTemporary(w, h, 0, source.format);
                }

                Graphics.Blit(source, targetImage, scaleInfo, offsetInfo);

                //直接将原图拷贝到降分辨率的RT上  
                Graphics.Blit(targetImage, rt1);

                //进行迭代高斯模糊  
                for (int i = 0; i < iteration; i++)
                {
                    //第一次高斯模糊，设置offsets，竖向模糊  
                    _Material.SetVector("_offsets", new Vector4(0, BlurRadius, 0, 0));
                    Graphics.Blit(rt1, targetImage, _Material);
                    //第二次高斯模糊，设置offsets，横向模糊  
                    _Material.SetVector("_offsets", new Vector4(BlurRadius, 0, 0, 0));
                    Graphics.Blit(targetImage, rt1, _Material);
                }

                //将结果输出  
                Graphics.Blit(rt1, targetImage);
            }
            else
            {
                if (rt1 != null)
                {
                    RenderTexture.ReleaseTemporary(rt1);
                    rt1 = null;
                }

                if (targetImage != null)
                {
                    RenderTexture.ReleaseTemporary(targetImage);
                    targetImage = null;
                }
            }
        }


        /// <summary>
        /// 世界坐标 转换为 屏幕坐标
        /// </summary>
        /// <param name="bounds"></param>
        /// <returns></returns>
        private Vector3[] WordPosToScreentPos(Vector3[] bounds, bool isMainCamera)
        {
            Vector3[] temp = new Vector3[4];

            Camera targetCamera = null;
            if (isMainCamera)
            {
                targetCamera = Camera.main;
            }
            else
            {
                targetCamera = gameObject.GetComponent<Camera>();
            }

            for (int i = 0, iMax = bounds.Length; i < iMax; i++)
            {
                temp[i] = targetCamera.WorldToScreenPoint(bounds[i]);
            }

            return temp;
        }
    }
}