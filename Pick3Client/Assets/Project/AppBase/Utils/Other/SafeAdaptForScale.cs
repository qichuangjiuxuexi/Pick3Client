using UnityEngine;
using UnityEngine.UI;

public class SafeAdaptForScale : MonoBehaviour
{
    /// <summary>
    /// 单例
    /// </summary>
    public static SafeAdaptForScale sInstance = null;

    /// <summary>
    /// 
    /// </summary>
    private Vector2 targetUISize = new Vector2(750, 1334);

    /// <summary>
    /// 顶部 非安全区的 高度
    /// </summary>
    private float topUnSafeArea = 0;

    /// <summary>
    /// 底部 非安全区的 高度
    /// </summary>
    private float bottomUnSafeArea = 0;

    /// <summary>
    /// banner的 高度
    /// </summary>
    private float bannerUnSafeArea = 0;

    private bool topBanner = false;

    /// <summary>
    /// 非安全区的 高度
    /// </summary>
    private float UnSafeArea = 0;

    /// <summary>
    /// 安全区的尺寸
    /// </summary>
    private Vector2 SafeAreaSize = Vector2.zero;

    /// <summary>
    /// 缩放数值
    /// </summary>
    private float scaleValue = 1;

    /// <summary>
    /// 换算后，缩放控制器的RectTransform
    /// </summary>
    private RectTransform scalerRectTransform = null;

    /// <summary>
    /// RectTransform
    /// </summary>
    private RectTransform selfRectTransform = null;

    private void Awake()
    {
        sInstance = this;
    }

    // Start is called before the first frame update
    private void Start()
    {
        Init();
        RefreshUI(false, 0);
        SetAdapt();
    }

    // Update is called once per frame
    private void Update()
    {
    }

    /// <summary>
    /// 初始化数据
    /// </summary>
    private void Init()
    {
        selfRectTransform = transform as RectTransform;
        CanvasScaler scaler = transform.GetComponentInParent<CanvasScaler>();
        if (scaler != null)
        {
            scalerRectTransform = scaler.transform as RectTransform;
        }
    }

    /// <summary>
    /// 刷新数据信息
    /// </summary>
    private void RefreshData()
    {
        if (topBanner)
        {
            topUnSafeArea = ScreenValueToUIForH(Screen.height - Screen.safeArea.yMax) + bannerUnSafeArea;
            bottomUnSafeArea = ScreenValueToUIForH(Screen.safeArea.yMin);
        }
        else
        {
            topUnSafeArea = ScreenValueToUIForH(Screen.height - Screen.safeArea.yMax);
            bottomUnSafeArea = ScreenValueToUIForH(Screen.safeArea.yMin) + bannerUnSafeArea;
        }

        UnSafeArea = topUnSafeArea + bottomUnSafeArea;
        SafeAreaSize = ScreenValueToUI(Screen.safeArea.size);

        scaleValue = Mathf.Min(SafeAreaSize.x / targetUISize.x, SafeAreaSize.y / targetUISize.y);
    }

    /// <summary>
    /// 适配界面
    /// </summary>
    /// <param name="needBottomSafe"></param>
    private void SetAdapt()
    {
        SetRectTransformSize(selfRectTransform, new Vector2(SafeAreaSize.x / scaleValue, SafeAreaSize.y / scaleValue));
        selfRectTransform.localScale = Vector2.one * scaleValue;
        selfRectTransform.anchoredPosition = new Vector2(0, (bottomUnSafeArea - topUnSafeArea) * 0.5f);
    }

    /// <summary>
    /// 刷新
    /// </summary>
    /// <param name="bannerHeight"></param>
    public void RefreshUI(bool top, float bannerHeight)
    {
        topBanner = top;
        bannerUnSafeArea = bannerHeight;
        RefreshData();
        SetAdapt();
    }

    #region Tool

    /// <summary>
    /// 将 屏幕参数转换为UI 储存 宽转换
    /// </summary>
    /// <param name="soucesValue"></param>
    /// <returns></returns>
    private float ScreenValueToUIForW(float soucesValue)
    {
        float targetValue = soucesValue * (scalerRectTransform.rect.size.x / Screen.width);

        return targetValue;
    }

    /// <summary>
    /// 将 屏幕参数转换为UI 储存 高转换
    /// </summary>
    /// <param name="soucesValue"></param>
    /// <returns></returns>
    private float ScreenValueToUIForH(float soucesValue)
    {
        float targetValue = soucesValue * (scalerRectTransform.rect.size.y / Screen.height);

        return targetValue;
    }

    /// <summary>
    /// 将 屏幕参数转换为UI 储存 高转换
    /// </summary>
    /// <param name="soucesValue"></param>
    /// <returns></returns>
    private Vector2 ScreenValueToUI(Vector2 soucesValue)
    {
        Vector2 targetValue = new Vector2(
            soucesValue.x * (scalerRectTransform.rect.size.x / Screen.width),
            soucesValue.y * (scalerRectTransform.rect.size.y / Screen.height)
        );

        return targetValue;
    }

    /// <summary>
    /// 设置 RectTransform 真实尺寸
    /// </summary>
    /// <param name="trans"></param>
    /// <param name="newSize"></param>
    public static void SetRectTransformSize(RectTransform trans, Vector2 newSize)
    {
        Vector2 oldSize = trans.rect.size;
        Vector2 deltaSize = newSize - oldSize;
        trans.offsetMin = trans.offsetMin - new Vector2(deltaSize.x * trans.pivot.x, deltaSize.y * trans.pivot.y);
        trans.offsetMax = trans.offsetMax +
                          new Vector2(deltaSize.x * (1f - trans.pivot.x), deltaSize.y * (1f - trans.pivot.y));
    }

    #endregion


    /*
float targetUIWidth = 750;
float targetUIHeight = 1334;
float UnSafeArea = topUnSafeArea + bottomUnSafeArea ;
float SafeAreaWidth = Scaler.RectTransform.Width;
float SafeAreaHeight = Scaler.RectTransform.Height - UnSafeArea;


float scaleValue = min(SafeAreaWidth  / targetUIWidth  ,SafeAreaHeight / targetUIHeight );

SafeAdaptRectTransform.Size = (SafeAreaWidth / scaleValue ,  SafeAreaHeight  / scaleValue);
SafeAdaptRectTransform.localScale = (scaleValue , scaleValue , 1);
SafeAdaptRectTransform.localPosition = (0 , bottomUnSafeArea * 0.5 - topUnSafeArea * 0.5 , 0);

     */
}