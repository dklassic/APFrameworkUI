using Cysharp.Text;
using UnityEngine;

public class StyleUtility
{
    public static ColorCode Selected = ColorCode.Blue;
    public static ColorCode DisableSelected = ColorCode.BlueSapphire;
    public static ColorCode Disabled = ColorCode.Grey;
    public static ColorCode Negative = ColorCode.Red;

    public static ColorCode RareColor(int rarity) => rarity switch
    {
        1 => ColorCode.White,
        2 => ColorCode.DodgerBlue,
        3 => ColorCode.PaleMagenta,
        4 => ColorCode.Tangerine,
        _ => ColorCode.Blue
    };

    public static Color ColorSetting(ColorCode color) => color switch
    {
        ColorCode.Red => new Color32(247, 49, 86, 255),
        ColorCode.MagicPotion => new Color32(255, 62, 96, 255),
        ColorCode.Tangerine => new Color32(250, 191, 56, 255),
        ColorCode.MikadoYellow => new Color32(250, 194, 15, 255),
        ColorCode.Blue => new Color32(10, 239, 254, 255),
        ColorCode.BlueSapphire => new Color32(0, 90, 125, 255),
        ColorCode.RichElectricBlue => new Color32(0, 148, 205, 255),
        ColorCode.Purple => new Color32(219, 62, 250, 255),
        ColorCode.Volt => new Color32(217, 250, 50, 255),
        ColorCode.PersianRose => new Color32(250, 50, 173, 255),
        ColorCode.Violet => new Color32(83, 50, 250, 255),
        ColorCode.Grey => new Color32(100, 100, 100, 255),
        ColorCode.VioletBlue => new Color32(49, 75, 168, 255),
        ColorCode.DodgerBlue => new Color32(50, 93, 250, 255),
        ColorCode.CatalinaBlue => new Color32(18, 38, 126, 255),
        ColorCode.Tangle => new Color32(240, 117, 24, 255),
        ColorCode.RoyalOrange => new Color32(250, 140, 72, 255),
        ColorCode.CosmicLatte => new Color32(255, 249, 226, 255),
        ColorCode.Crayola => new Color32(251, 214, 100, 255),
        ColorCode.PaleMagenta => new Color32(255, 130, 226, 255),
        ColorCode.LightMediumOrchid => new Color32(217, 166, 195, 255),
        ColorCode.GoldenYellow => new Color32(255, 223, 0, 255),
        _ => new Color32(255, 255, 255, 255)
    };

    public static string ColorString(ColorCode color)
    {
        Color32 convertedColor = ColorSetting(color);
        return ZString.Format("#{0}{1}{2}", convertedColor.r.ToString("X2"),
            convertedColor.g.ToString("X2"),
            convertedColor.b.ToString("X2"));
    }

    public static string StringColored(string text, ColorCode color)
    {
        return ZString.Format("<color={0}>{1}</color>", ColorString(color), text);
    }

    public static string StringColoredRange(string text, ColorCode color, int min, int max)
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