using System;
using System.Collections;
using System.Text;
using UnityEngine;
using TMPro;


public enum WindowThickenType
{
    None,
    CornerOnly,
    Whole
}
public class WindowOutline : MonoBehaviour
{
    public TextMeshProUGUI Outline;
    string outlineText;
    public string OutlineText
    {
        get
        {
            if (String.IsNullOrEmpty(outlineText))
                return string.Empty;
            else
            {
                return inFocus switch
                {
                    false => available switch
                    {
                        false => StyleUtility.StringColored(outlineText.ToUpper(), StyleUtility.Disabled),
                        true => outlineText.ToUpper(),
                    },
                    true => StyleUtility.StringColored(outlineText.ToUpper(), available ? StyleUtility.Selected : StyleUtility.DisableSelected)
                };
            }
        }
    }
    bool inFocus = false;
    bool available = true;
    bool active = false;
    public bool InDebug = false;
    WindowSetting Setting => UIManager.Instance.WindowSetting;
    WindowStyle style;
    Coroutine coroutine = null;
    Vector2Int size;
    StringBuilder windowStringBuilder = new StringBuilder();
    public string TargetText => size.x + "x" + size.y;
    public void SetOpacity(float alpha)
    {
        Outline.color = new Color(1, 1, 1, Mathf.Clamp01(alpha));
    }
    public void SetOutline(int widthCount, int heightCount, WindowStyle windowStyle, int titleOverride, int subscriptOverride)
    {
        style = windowStyle;
        size.x = widthCount;
        size.y = heightCount;
        WindowThickenType thicken = WindowThickenType.None;
        LabelStyle label = LabelStyle.None;
        int windowHeight = heightCount;
        // Setup First Line
        if (Setting.HasThickenCorner(windowStyle))
            thicken = WindowThickenType.CornerOnly;
        if (Setting.HasThickenEdge(windowStyle))
            thicken = WindowThickenType.Whole;
        if (Setting.HasLeftLabel(windowStyle))
            label = LabelStyle.Left;
        else if (Setting.HasRightLabel(windowStyle))
            label = LabelStyle.Right;
        string filler;
        if (Setting.IsFullFrame(windowStyle))
            filler = LineFill(OutlineSets(0, thicken, label), widthCount);
        else if (Setting.IsCornerSet(windowStyle))
            filler = LineFill(CornerSets(0, thicken, label), widthCount);
        else if (Setting.IsLeftLine(windowStyle))
            filler = LineFill(LeftLineSets(thicken, label), widthCount);
        else
            filler = TextUtility.Repeat(' ', widthCount) + TextUtility.LineBreaker;
        if (Setting.HasEmbeddedTitle(windowStyle) && !Setting.IsLeftLine(windowStyle) && label == LabelStyle.None)
        {
            // filler = filler.Substring(0, 1) + TextUtility.Repeat(' ', titleOverride - 1) + filler.Substring(titleOverride);
            filler = TextUtility.Repeat(' ', titleOverride) + filler.Substring(titleOverride);
        }
        if (!Setting.HasTitlebar(windowStyle))
            windowHeight -= 2;

        windowStringBuilder.Clear();
        windowStringBuilder.Append(filler);
        // Fill in the rest
        for (int i = 0; i < windowHeight; i++)
        {
            if (UIManager.Instance.WindowSetting.IsCornerSet(windowStyle))
            {
                if (i != windowHeight - 1)
                    windowStringBuilder.Append(LineFill(CornerSets(2, thicken, label), widthCount));
                else
                    windowStringBuilder.Append(LineFill(CornerSets(3, thicken, label), widthCount));
            }
            else if (Setting.IsLeftLine(windowStyle))
            {
                if (i != windowHeight - 1)
                    windowStringBuilder.Append(LineFill(LeftLineSets(thicken, label), widthCount));
                else
                    windowStringBuilder.Append(LineFill(OutlineSets(4, thicken, label), widthCount));
            }
            else if (Setting.IsFullFrame(windowStyle))
            {
                if (Setting.HasTitlebar(windowStyle))
                {
                    if (i == 1)
                        windowStringBuilder.Append(LineFill(OutlineSets(1, thicken, label), widthCount));
                    else
                        windowStringBuilder.Append(LineFill(OutlineSets(2, thicken, label), widthCount));
                }
                else
                    windowStringBuilder.Append(LineFill(OutlineSets(2, thicken, label), widthCount));
                if (i == windowHeight - 1)
                    windowStringBuilder.Append(LineFill(OutlineSets(3, thicken, label), widthCount));
            }
            else if (UIManager.Instance.WindowSetting.IsLowerLeft(windowStyle))
            {
                if (Setting.HasTitlebar(windowStyle))
                {
                    if (i == 1)
                        windowStringBuilder.Append(LineFill(LowerLeftOutlineSets(1, thicken, label), widthCount));
                    else
                        windowStringBuilder.Append(LineFill(LowerLeftOutlineSets(2, thicken, label), widthCount));
                }
                else
                    windowStringBuilder.Append(LineFill(LowerLeftOutlineSets(2, thicken, label), widthCount));
                if (i == windowHeight - 1)
                    windowStringBuilder.Append(LineFill(LowerLeftOutlineSets(3, thicken, label), widthCount));
            }
        }
        if (subscriptOverride > 0)
        {
            windowStringBuilder.Remove(windowStringBuilder.Length - subscriptOverride - 4, subscriptOverride + 4);
            windowStringBuilder.Append(TextUtility.PlaceHolder(subscriptOverride + 4));
        }
        outlineText = windowStringBuilder.ToString();
        if (active)
            Outline.SetText(OutlineText);

    }

    string OutlineSets(int i, WindowThickenType thickenType = WindowThickenType.None, LabelStyle labeled = LabelStyle.None) => thickenType switch
    {
        WindowThickenType.None => labeled switch
        {
            LabelStyle.None => i switch
            {
                0 => "┌─┐",
                1 => "├─┤",
                2 => "│ │",
                3 => "└─┘",
                _ => "   "
            },
            LabelStyle.Left => i switch
            {
                0 => "▄─┐",
                1 => "█─┤",
                2 => "█ │",
                3 => "▀─┘",
                _ => "   "
            },
            LabelStyle.Right => i switch
            {
                0 => "┌─▄",
                1 => "├─█",
                2 => "│ █",
                3 => "└─▀",
                _ => "   "
            },
            _ => throw new System.NotImplementedException(),
        },
        WindowThickenType.Whole => labeled switch
        {
            LabelStyle.None => i switch
            {
                0 => "╔═╗",
                1 => "╠═╣",
                2 => "║ ║",
                3 => "╚═╝",
                _ => "   "
            },
            LabelStyle.Left => i switch
            {
                0 => "▄═╗",
                1 => "█═╣",
                2 => "█ ║",
                3 => "▀═╝",
                _ => "   "
            },
            LabelStyle.Right => i switch
            {
                0 => "╔═▄",
                1 => "╠═█",
                2 => "║ █",
                3 => "╚═▀",
                _ => "   "
            },
            _ => throw new System.NotImplementedException(),
        },
        WindowThickenType.CornerOnly => labeled switch
        {
            LabelStyle.None => i switch
            {
                0 => "╔─╗",
                1 => "╠─╣",
                2 => "│ │",
                3 => "╚─╝",
                _ => "   "
            },
            LabelStyle.Left => i switch
            {
                0 => "▄─╗",
                1 => "█─╣",
                2 => "█ │",
                3 => "▀─╝",
                _ => "   "
            },
            LabelStyle.Right => i switch
            {
                0 => "╔─▄",
                1 => "╠─█",
                2 => "│ █",
                3 => "╚─▀",
                _ => "   "
            },
            _ => throw new System.NotImplementedException(),
        },
        _ => throw new System.NotImplementedException(),
    };
    string LowerLeftOutlineSets(int i, WindowThickenType thickenType = WindowThickenType.None, LabelStyle labeled = LabelStyle.None) => thickenType switch
    {
        WindowThickenType.None => labeled switch
        {
            LabelStyle.None => i switch
            {
                0 => "┌  ",
                1 => "├  ",
                2 => "│  ",
                3 => "└─ ",
                _ => "   "
            },
            LabelStyle.Left => i switch
            {
                0 => "▄  ",
                1 => "█  ",
                2 => "█  ",
                3 => "▀─ ",
                _ => "   "
            },
            LabelStyle.Right => i switch
            {
                0 => "┌ ▄",
                1 => "├ █",
                2 => "│ █",
                3 => "└─▀",
                _ => "   "
            },
            _ => throw new System.NotImplementedException(),
        },
        WindowThickenType.Whole => labeled switch
        {
            LabelStyle.None => i switch
            {
                0 => "╔  ",
                1 => "╠  ",
                2 => "║  ",
                3 => "╚═ ",
                _ => "   "
            },
            LabelStyle.Left => i switch
            {
                0 => "▄  ",
                1 => "█  ",
                2 => "█  ",
                3 => "▀═ ",
                _ => "   "
            },
            LabelStyle.Right => i switch
            {
                0 => "╔ ▄",
                1 => "╠ █",
                2 => "║ █",
                3 => "╚═▀",
                _ => "   "
            },
            _ => throw new System.NotImplementedException(),
        },
        WindowThickenType.CornerOnly => labeled switch
        {
            LabelStyle.None => i switch
            {
                0 => "╔  ",
                1 => "╠  ",
                2 => "│  ",
                3 => "╚─ ",
                _ => "   "
            },
            LabelStyle.Left => i switch
            {
                0 => "▄  ",
                1 => "█  ",
                2 => "█  ",
                3 => "▀─ ",
                _ => "   "
            },
            LabelStyle.Right => i switch
            {
                0 => "╔ ▄",
                1 => "╠ █",
                2 => "│ █",
                3 => "╚─▀",
                _ => "   "
            },
            _ => throw new System.NotImplementedException(),
        },
        _ => throw new System.NotImplementedException(),
    };
    string LeftLineSets(WindowThickenType thickenType = WindowThickenType.None, LabelStyle labeled = LabelStyle.None) => labeled switch
    {
        LabelStyle.None => thickenType switch
        {
            WindowThickenType.None => "│  ",
            _ => "║  "
        },
        LabelStyle.Left => "█  ",
        LabelStyle.Right => throw new NotImplementedException(),
        _ => throw new System.NotImplementedException(),
    };
    string CornerSets(int i, WindowThickenType thickenType = WindowThickenType.None, LabelStyle labeled = LabelStyle.None) => labeled switch
    {
        LabelStyle.None => thickenType switch
        {
            WindowThickenType.None => i switch
            {
                0 => "┌ ┐",
                3 => "└ ┘",
                _ => "   "
            },
            _ => i switch
            {
                0 => "╔ ╗",
                3 => "╚ ╝",
                _ => "   "
            }
        },
        LabelStyle.Left => thickenType switch
        {
            WindowThickenType.None => i switch
            {
                0 => "▄ ┐",
                1 => "█  ",
                2 => "█  ",
                3 => "▀ ┘",
                _ => "   "
            },
            _ => i switch
            {
                0 => "▄ ╗",
                1 => "█  ",
                2 => "█  ",
                3 => "▀ ╝",
                _ => "   "
            }
        },
        LabelStyle.Right => thickenType switch
        {
            WindowThickenType.None => i switch
            {
                0 => "┌ ▄",
                1 => "  █",
                2 => "  █",
                3 => "└ ▀",
                _ => "   "
            },
            _ => i switch
            {
                0 => "╔ ▄",
                1 => "  █",
                2 => "  █",
                3 => "╚ ▀",
                _ => "   "
            }
        },
        _ => throw new System.NotImplementedException(),
    };
    string LineFill(string set, int count)
    {
        TextUtility.StringBuilder.Clear();
        TextUtility.StringBuilder.Append(set[0]);
        TextUtility.StringBuilder.Append(TextUtility.Repeat(set[1], count - 2));
        TextUtility.StringBuilder.Append(set[2]);
        TextUtility.StringBuilder.Append(TextUtility.LineBreaker);
        return TextUtility.StringBuilder.ToString();
    }
    public float SetActive(bool active)
    {
        this.active = active;
        if (!UIManager.Instance.WindowSetting.HasOutline(style))
            return 0f;
        if (active)
        {
            if (coroutine != null)
                StopCoroutine(coroutine);
            Outline.SetText(OutlineText);
            // coroutine = StartCoroutine(StartupSequence());
            return 0f;
        }
        else
        {
            if (coroutine != null)
                StopCoroutine(coroutine);
            Outline.SetText(string.Empty);
            return 0f;
        }
    }
    internal void SetFocusAndAvailable(bool f, bool a)
    {
        inFocus = f;
        available = a;
        if (!UIManager.Instance.WindowSetting.HasOutline(style))
            return;
        if (active)
            Outline.SetText(OutlineText);
    }
}