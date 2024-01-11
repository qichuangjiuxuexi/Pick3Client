using UnityEngine;
using WordGame.Utils;

namespace WordGame.Utils
{
    public class SlicedImageSizeSet : MonoBehaviour
    {
        public const float CurrentImageSliceTimes = 1;
        public const float CurrentHDImageSliceTimes = 2;

        /// <summary>
        /// 图片初始缩放
        /// </summary>
        public float ImageBaseScale = 1;

        /// <summary>
        /// 是否运行缩放
        /// </summary>
        [SerializeField] private bool runScale = true;

        // Start is called before the first frame update
        private void Start()
        {
        }

        // Update is called once per frame
        private void Update()
        {
        }

        /// <summary>
        /// 根据缩放倍率重置图片尺寸
        /// </summary>
        [ContextMenu("Refresh")]
        private void ResetImageSize()
        {
            if (runScale)
            {
                RectTransform selfRectTransform = transform as RectTransform;
                if (selfRectTransform != null)
                {
                    Vector2 currentSize = selfRectTransform.rect.size;
                    Vector3 currentScale = selfRectTransform.localScale;

                    float currentValue = ImageBaseScale / currentScale.x;

                    float targetValue = CurrentImageSliceTimes / currentValue;

                    ToolTransform.SetRectTransformSize(selfRectTransform, currentSize * targetValue);
                    selfRectTransform.localScale = Vector3.one * (1 / targetValue);
                }
            }
        }

        /// <summary>
        /// 根据缩放倍率重置图片尺寸
        /// </summary>
        [ContextMenu("SetSize")]
        public void SetImageStretch()
        {
            RectTransform selfRectTransform = transform as RectTransform;
            if (selfRectTransform != null)
            {
                Vector2 parentSize = (selfRectTransform.parent as RectTransform).rect.size;
                Vector2 targetSize = parentSize * CurrentImageSliceTimes;

                Vector2 targetAnchoredPosition =
                    -1 * (new Vector2(targetSize.x - parentSize.x, targetSize.y - parentSize.y)) / CurrentImageSliceTimes;
                Vector2 targetSizeDelta = (new Vector2(targetSize.x - parentSize.x, targetSize.y - parentSize.y)) /
                                          CurrentImageSliceTimes;

                selfRectTransform.offsetMin = new Vector2(selfRectTransform.offsetMin.x, targetAnchoredPosition.y);
                selfRectTransform.offsetMax = new Vector2(selfRectTransform.offsetMax.x, targetSizeDelta.y);
            }
        }
    }
}
