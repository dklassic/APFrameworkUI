using Cysharp.Text;
using UnityEngine;

public class StyleUtility
{
    public static Color selected = new Color32(10, 239, 254, 255);
    public static Color disableSelected = new Color32(0, 90, 125, 255);
    public static Color disabled = new Color32(100, 100, 100, 255);
    public static Color negative = new Color32(247, 49, 86, 255);

    public static string StringColored(string text, Color color)
    {
        return ZString.Format("<color=#{0}>{1}</color>", ColorUtility.ToHtmlStringRGBA(color), text);
    }

    public static string StringColoredRange(string text, Color color, int min, int max)
    {
        int actualMin = Mathf.Min(min, max);
        int actualMax = Mathf.Max(min, max);
        using (Utf16ValueStringBuilder builder = ZString.CreateStringBuilder())
        {
            if (actualMin > 0)
                builder.Append(text.Substring(0, actualMin));
            builder.Append(StringColored(text.Substring(actualMin, actualMax - actualMin), color));
            if (text.Length - actualMax > 0)
                builder.Append(text.Substring(actualMax, text.Length - actualMax));
            return builder.ToString();
        }
    }

    public static string Sized(string tag, int size)
    {
        return ZString.Format("<size={0}>{1}</size>", size, tag);
    }

    public static string StringTransparent(string text, int alpha)
    {
        return ZString.Format("<alpha=#{0}>", alpha.ToString("X2"));
    }

    public static string StringBold(string text)
    {
        return ZString.Format("<b>{0}</b>", text);
    }

    public static Color DarkenColor(Color color, float percentage)
    {
        Color.RGBToHSV(color, out float h, out float s, out float v);
        return Color.HSVToRGB(h, s, v * Mathf.Clamp01(percentage));
    }

    public static Color ClearColor(Color color) => new Color(color.r, color.g, color.b, 0);
    public static Color FullColor(Color color) => new Color(color.r, color.g, color.b, 1);
    public static Color AlphaColor(Color color, float alpha) => new Color(color.r, color.g, color.b, alpha);
}