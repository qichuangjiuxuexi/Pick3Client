using UnityEngine;

namespace WordGame.Utils
{
    //非运行时也触发效果
    [ExecuteInEditMode]
    //屏幕后处理特效一般都需要绑定在摄像机上
    [RequireComponent(typeof(Camera))]
    /// <summary>
    /// 提供一个后处理的基类，主要功能在于直接通过Inspector面板拖入shader，生成shader对应的材质
    /// </summary>
    public class PostEffectBase : MonoBehaviour
    {
        //Inspector面板上直接拖入    
        public Shader shader = null;
        private Material _material = null;

        public Material _Material
        {
            get
            {
                if (_material == null)
                {
                    _material = GenerateMaterial(shader);
                }

                return _material;
            }
            set { _material = value; }
        }

        //根据shader创建用于屏幕特效的材质    
        protected Material GenerateMaterial(Shader shader)
        {
            if (shader == null)
            {
                return null;
            }

            //需要判断shader是否支持        
            if (shader.isSupported == false)
            {
                return null;
            }

            Material material = new Material(shader);
            material.hideFlags = HideFlags.DontSave;
            if (material)
            {
                return material;
            }

            return null;
        }
    }

    //编辑状态下也运行  
    [ExecuteInEditMode]
    //继承自PostEffectBase 
    /// <summary>
    /// 高斯模糊特效
    /// </summary>
    public class GaussianBlur : PostEffectBase
    {
        //模糊半径  
        public float BlurRadius = 1.0f;

        //降分辨率  
        public int downSample = 2;

        //迭代次数  
        public int iteration = 1;

        void OnRenderImage(RenderTexture source, RenderTexture destination)
        {
            if (_Material)
            {
                //申请RenderTexture，RT的分辨率按照downSample降低  
                RenderTexture rt1 = RenderTexture.GetTemporary(source.width >> downSample, source.height >> downSample,
                    0, source.format);
                RenderTexture rt2 = RenderTexture.GetTemporary(source.width >> downSample, source.height >> downSample,
                    0, source.format);

                //直接将原图拷贝到降分辨率的RT上  
                Graphics.Blit(source, rt1);

                //进行迭代高斯模糊  
                for (int i = 0; i < iteration; i++)
                {
                    //第一次高斯模糊，设置offsets，竖向模糊  
                    _Material.SetVector("_offsets", new Vector4(0, BlurRadius, 0, 0));
                    Graphics.Blit(rt1, rt2, _Material);
                    //第二次高斯模糊，设置offsets，横向模糊  
                    _Material.SetVector("_offsets", new Vector4(BlurRadius, 0, 0, 0));
                    Graphics.Blit(rt2, rt1, _Material);
                }

                //将结果输出  
                Graphics.Blit(rt1, destination);

                //释放申请的两块RenderBuffer内容  
                RenderTexture.ReleaseTemporary(rt1);
                RenderTexture.ReleaseTemporary(rt2);
            }
        }
    }
}