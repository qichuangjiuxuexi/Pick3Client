using UnityEngine;
using UnityEngine.UI;
using WordGame.Utils;

public class TextLengthTool : MonoSingleton<TextLengthTool>
{
    /// <summary>
    /// 目标 实验标签
    /// </summary>
    public Text TargetLabel = null;

    // Start is called before the first frame update
    void Start()
    {
    }

    // Update is called once per frame
    void Update()
    {
    }

    /// <summary>
    /// 获取文本长度
    /// </summary>
    /// <param name="info"></param>
    /// <returns></returns>
    public float GetTextLength(string info, int fontSize)
    {
        float length = 0;

        if (TargetLabel != null)
        {
            TargetLabel.text = info;
            TargetLabel.fontSize = fontSize;
            Canvas.ForceUpdateCanvases();
            length = TargetLabel.preferredWidth;
        }

        return length;
    }

    /// <summary>
    /// 获取文本长度
    /// </summary>
    /// <param name="info"></param>
    /// <returns></returns>
    public int GetTextScale(string info, int fontSize, float targetLength)
    {
        if (TargetLabel != null)
        {
            fontSize++;
            do
            {
                fontSize--;
                if (fontSize < 20)
                {
                    break;
                }

                TargetLabel.text = info;
                TargetLabel.fontSize = fontSize;
                Canvas.ForceUpdateCanvases();
            } while (targetLength < TargetLabel.preferredWidth);
        }

        return fontSize;
    }
}