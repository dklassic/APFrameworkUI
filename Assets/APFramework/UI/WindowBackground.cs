using UnityEngine;
using UnityEngine.UI;


public class WindowBackground : MonoBehaviour
{
    public RawImage Background;
    Color bgColor = Color.clear;
    internal void SetType(BackgroundStyle background)
    {
        bgColor = background switch
        {
            BackgroundStyle.FillBlack => Color.black,
            BackgroundStyle.TransparentBlack => new Color(0, 0, 0, 0.8f),
            _ => Color.clear
        };
    }
    public void SetColor(Color color, bool active)
    {
        bgColor = color;
        if (!active)
            return;
        Background.color = bgColor;
    }

    internal void SetActive(bool v)
    {
        if (bgColor == Color.clear)
            return;
        Background.color = v ? bgColor : Color.clear;
    }
}