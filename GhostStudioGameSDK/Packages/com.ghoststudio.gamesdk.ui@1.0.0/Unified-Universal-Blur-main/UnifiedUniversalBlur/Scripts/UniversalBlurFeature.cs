using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

namespace Unified.Universal.Blur
{
    public class UniversalBlurFeature : ScriptableRendererFeature
    {
        public static UniversalBlurFeature Instance { private set; get; }
        
        public enum InjectionPoint
        {
            BeforeRenderingTransparents = RenderPassEvent.BeforeRenderingTransparents,
            BeforeRenderingPostProcessing = RenderPassEvent.BeforeRenderingPostProcessing,
            AfterRenderingPostProcessing = RenderPassEvent.AfterRenderingPostProcessing
        }

        public Material passMaterial;
        
        public Material outMaterial;

        [HideInInspector] public int passIndex = 0;

        [Header("Blur Settings")]
        public InjectionPoint injectionPoint = InjectionPoint.AfterRenderingPostProcessing;

        [Space]
        [InspectorName("强度")]
        [Tooltip("0表示无效果，1表示最大强度的效果")]
        [Range(0f, 1f)] public float intensity = 1.0f;
        [InspectorName("采样率")]
        [Tooltip("这个参数控制模糊计算过程中的降采样级别。范围在1到10之间，其中1表示不进行降采样，10表示最大程度的降采样。降采样可以提高计算效率，但会损失一些细节")]
        [Range(1f, 10f)] public float downsample = 2.0f;
        [InspectorName("缩放")]
        [Tooltip("这个参数控制模糊效果的缩放比例。范围在0到5之间，其中0表示无缩放，1表示原来的大小，大于1会放大模糊效果。较小的值会产生更柔和的模糊效果")]
        [Range(0f, 5f)] public float scale = .5f;
        [InspectorName("迭代次数")]
        [Tooltip("增加迭代次数可以增加模糊效果的质量，但也会增加计算开销")]
        [Range(1, 20)] public int iterations = 6;


        // Hidden by scope because of no need
        private ScriptableRenderPassInput _requirements = ScriptableRenderPassInput.Color;

        // Hidden by scope because of incorrect behaviour in the editor
        private bool disableInSceneView = true;

        private UniversalBlurPass _fullScreenPass;
        private bool _requiresColor;
        private bool _injectedBeforeTransparents;

        private UniversalBlurPass.PassData _PassData;


        /// <summary>
        /// 引用计数
        /// </summary>
        private int refCount;
        
        public void Activate()
        {
            refCount++;
            SetActive(refCount > 0);
        }

        public void Deactivate()
        {
            refCount = Mathf.Max(--refCount, 0);
            SetActive(refCount > 0);
        }

        /// <inheritdoc/>
        public override void Create()
        {
            _fullScreenPass = new UniversalBlurPass();
            _fullScreenPass.renderPassEvent = (RenderPassEvent)injectionPoint;

            ScriptableRenderPassInput modifiedRequirements = _requirements;

            _requiresColor = (_requirements & ScriptableRenderPassInput.Color) != 0;
            _injectedBeforeTransparents = injectionPoint <= InjectionPoint.BeforeRenderingTransparents;

            if (_requiresColor && !_injectedBeforeTransparents)
            {
                modifiedRequirements ^= ScriptableRenderPassInput.Color;
            }

            _fullScreenPass.ConfigureInput(modifiedRequirements);

            _PassData = new ();

            Instance = this;

            refCount = 0;

            if (Application.isPlaying)
            {
                Deactivate();
            }
        }

        /// <inheritdoc/>
        public override void AddRenderPasses(ScriptableRenderer renderer, ref RenderingData renderingData)
        {
            if (passMaterial == null)
            {
                Debug.LogWarningFormat("Missing Post Processing effect Material. {0} Fullscreen pass will not execute. Check for missing reference in the assigned renderer.", GetType().Name);
                return;
            }

            SetupPassData(_PassData);
            _fullScreenPass.Setup(_PassData, downsample, renderingData);

            renderer.EnqueuePass(_fullScreenPass);
        }

        /// <inheritdoc/>
        protected override void Dispose(bool disposing)
        {
            _fullScreenPass.Dispose();
        }

        void SetupPassData(UniversalBlurPass.PassData passData)
        {
            passData.effectMaterial = passMaterial;
            passData.intensity = intensity;
            passData.passIndex = passIndex;
            passData.requiresColor = _requiresColor;
            passData.scale = scale;
            passData.iterations = iterations;
        }
    }

}