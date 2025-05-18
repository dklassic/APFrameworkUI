using System;
using Cysharp.Text;
using UnityEngine;
using TMPro;

namespace ChosenConcept.APFramework.Interface.Framework
{
    public class WindowOutline : MonoBehaviour
    {
        [SerializeField] TextMeshProUGUI _outline;
        public TextMeshProUGUI outline => _outline;
        string _outlineText;

        public string outlineText
        {
            get
            {
                if (String.IsNullOrEmpty(_outlineText))
                    return string.Empty;
                else
                {
                    return _inFocus switch
                    {
                        false => _displayStyle switch
                        {
                            WindowOutlineDisplayStyle.Always => _available switch
                            {
                                false => StyleUtility.StringColored(_outlineText.ToUpper(), StyleUtility.Disabled),
                                true => _outlineText.ToUpper(),
                            },
                            WindowOutlineDisplayStyle.WhenSelected => string.Empty,
                            _ => throw new NotImplementedException()
                        },
                        true => (!_singleWindowOverride || _inInput) switch
                        {
                            true => _outlineText.ToUpper(),
                            false => StyleUtility.StringColored(_outlineText.ToUpper(),
                                _available ? StyleUtility.Selected : StyleUtility.DisableSelected)
                        }
                    };
                }
            }
        }

        [SerializeField]  bool _singleWindowOverride = false;
        [SerializeField]  bool _inFocus = false;
        [SerializeField]  bool _available = true;
        [SerializeField]  bool _inInput = false;
        [SerializeField]  bool _active = false;
        [SerializeField]  bool _hasOutline = false;
        Coroutine _coroutine = null;
        [SerializeField]  Vector2Int _size;
        [SerializeField]  WindowOutlineDisplayStyle _displayStyle;
        public string targetText => ZString.Format("Allocating {0}x{1}", _size.x, _size.y);

        public void SetOpacity(float alpha)
        {
            _outline.color = new Color(1, 1, 1, Mathf.Clamp01(alpha));
        }

        public void SetOutline(int widthCount, int heightCount, WindowSetup setup, int titleOverride,
            int subscriptOverride)
        {
            _hasOutline = setup.outlineStyle != WindowOutlineStyle.None;
            _displayStyle = setup.outlineDisplayStyle;
            if (!_hasOutline)
            {
                return;
            }

            _size.x = widthCount;
            _size.y = heightCount;
            WindowThickenStyle thicken = setup.thickenStyle;
            int windowHeight = heightCount;
            // Setup First Line
            string filler = setup.outlineStyle switch
            {
                WindowOutlineStyle.FullFrame => LineFill(OutlineSets(0, thicken, setup.labelStyle), widthCount),
                WindowOutlineStyle.CornerOnly => LineFill(CornerSets(0, thicken, setup.labelStyle), widthCount),
                WindowOutlineStyle.LeftLine => LineFill(LeftLineSets(thicken, setup.labelStyle), widthCount),
                WindowOutlineStyle.RightLine => LineFill(RightLineSets(thicken, setup.labelStyle), widthCount),
                _ => TextUtility.Repeat(' ', widthCount) + TextUtility.LineBreaker
            };
            if (setup.titleStyle == WindowTitleStyle.EmbeddedTitle &&
                setup.outlineStyle != WindowOutlineStyle.LeftLine && setup.labelStyle == WindowLabelStyle.None)
            {
                // filler = filler.Substring(0, 1) + TextUtility.Repeat(' ', titleOverride - 1) + filler.Substring(titleOverride);
                filler = TextUtility.Repeat(' ', titleOverride) + filler.Substring(titleOverride);
            }

            if (setup.titleStyle != WindowTitleStyle.TitleBar)
                windowHeight -= 2;

            using (var windowStringBuilder = ZString.CreateStringBuilder())
            {
                windowStringBuilder.Append(filler);
                // Fill in the rest
                for (int i = 0; i < windowHeight; i++)
                {
                    if (setup.outlineStyle == WindowOutlineStyle.CornerOnly)
                    {
                        if (i != windowHeight - 1)
                            windowStringBuilder.Append(LineFill(CornerSets(2, thicken, setup.labelStyle), widthCount));
                        else
                            windowStringBuilder.Append(LineFill(CornerSets(3, thicken, setup.labelStyle), widthCount));
                    }
                    else if (setup.outlineStyle == WindowOutlineStyle.LeftLine)
                    {
                        if (i != windowHeight - 1)
                            windowStringBuilder.Append(LineFill(LeftLineSets(thicken, setup.labelStyle), widthCount));
                        else
                            windowStringBuilder.Append(LineFill(OutlineSets(4, thicken, setup.labelStyle), widthCount));
                    }
                    else if (setup.outlineStyle == WindowOutlineStyle.RightLine)
                    {
                        if (i != windowHeight - 1)
                            windowStringBuilder.Append(LineFill(RightLineSets(thicken, setup.labelStyle), widthCount));
                        else
                            windowStringBuilder.Append(LineFill(OutlineSets(4, thicken, setup.labelStyle), widthCount));
                    }
                    else if (setup.outlineStyle == WindowOutlineStyle.FullFrame)
                    {
                        if (setup.titleStyle == WindowTitleStyle.TitleBar)
                        {
                            if (i == 1)
                                windowStringBuilder.Append(LineFill(OutlineSets(1, thicken, setup.labelStyle),
                                    widthCount));
                            else
                                windowStringBuilder.Append(LineFill(OutlineSets(2, thicken, setup.labelStyle),
                                    widthCount));
                        }
                        else
                            windowStringBuilder.Append(LineFill(OutlineSets(2, thicken, setup.labelStyle), widthCount));

                        if (i == windowHeight - 1)
                            windowStringBuilder.Append(LineFill(OutlineSets(3, thicken, setup.labelStyle), widthCount));
                    }
                    else if (setup.outlineStyle == WindowOutlineStyle.LowerLeftCornerOnly)
                    {
                        if (setup.titleStyle == WindowTitleStyle.TitleBar)
                        {
                            if (i == 1)
                                windowStringBuilder.Append(LineFill(LowerLeftOutlineSets(1, thicken, setup.labelStyle),
                                    widthCount));
                            else
                                windowStringBuilder.Append(LineFill(LowerLeftOutlineSets(2, thicken, setup.labelStyle),
                                    widthCount));
                        }
                        else
                            windowStringBuilder.Append(LineFill(LowerLeftOutlineSets(2, thicken, setup.labelStyle),
                                widthCount));

                        if (i == windowHeight - 1)
                            windowStringBuilder.Append(LineFill(LowerLeftOutlineSets(3, thicken, setup.labelStyle),
                                widthCount));
                    }
                }

                if (subscriptOverride > 0)
                {
                    windowStringBuilder.Remove(windowStringBuilder.Length - subscriptOverride - 4,
                        subscriptOverride + 4);
                    windowStringBuilder.Append(TextUtility.PlaceHolder(subscriptOverride + 4));
                }

                _outlineText = windowStringBuilder.ToString();
            }

            if (_active)
                SetOutlineText(outlineText);
        }

        string OutlineSets(int i, WindowThickenStyle thickenType = WindowThickenStyle.None,
            WindowLabelStyle labeled = WindowLabelStyle.None) => thickenType switch
        {
            WindowThickenStyle.None => labeled switch
            {
                WindowLabelStyle.None => i switch
                {
                    0 => "┌─┐",
                    1 => "├─┤",
                    2 => "│ │",
                    3 => "└─┘",
                    _ => "   "
                },
                WindowLabelStyle.Left => i switch
                {
                    0 => "▄─┐",
                    1 => "█─┤",
                    2 => "█ │",
                    3 => "▀─┘",
                    _ => "   "
                },
                WindowLabelStyle.Right => i switch
                {
                    0 => "┌─▄",
                    1 => "├─█",
                    2 => "│ █",
                    3 => "└─▀",
                    _ => "   "
                },
                _ => throw new NotImplementedException()
            },
            WindowThickenStyle.Whole => labeled switch
            {
                WindowLabelStyle.None => i switch
                {
                    0 => "╔═╗",
                    1 => "╠═╣",
                    2 => "║ ║",
                    3 => "╚═╝",
                    _ => "   "
                },
                WindowLabelStyle.Left => i switch
                {
                    0 => "▄═╗",
                    1 => "█═╣",
                    2 => "█ ║",
                    3 => "▀═╝",
                    _ => "   "
                },
                WindowLabelStyle.Right => i switch
                {
                    0 => "╔═▄",
                    1 => "╠═█",
                    2 => "║ █",
                    3 => "╚═▀",
                    _ => "   "
                },
                _ => throw new NotImplementedException()
            },
            WindowThickenStyle.CornerOnly => labeled switch
            {
                WindowLabelStyle.None => i switch
                {
                    0 => "╔─╗",
                    1 => "╠─╣",
                    2 => "│ │",
                    3 => "╚─╝",
                    _ => "   "
                },
                WindowLabelStyle.Left => i switch
                {
                    0 => "▄─╗",
                    1 => "█─╣",
                    2 => "█ │",
                    3 => "▀─╝",
                    _ => "   "
                },
                WindowLabelStyle.Right => i switch
                {
                    0 => "╔─▄",
                    1 => "╠─█",
                    2 => "│ █",
                    3 => "╚─▀",
                    _ => "   "
                },
                _ => throw new NotImplementedException()
            },
            _ => throw new NotImplementedException()
        };

        string LowerLeftOutlineSets(int i, WindowThickenStyle thickenType = WindowThickenStyle.None,
            WindowLabelStyle labeled = WindowLabelStyle.None) => thickenType switch
        {
            WindowThickenStyle.None => labeled switch
            {
                WindowLabelStyle.None => i switch
                {
                    0 => "┌  ",
                    1 => "├  ",
                    2 => "│  ",
                    3 => "└─ ",
                    _ => "   "
                },
                WindowLabelStyle.Left => i switch
                {
                    0 => "▄  ",
                    1 => "█  ",
                    2 => "█  ",
                    3 => "▀─ ",
                    _ => "   "
                },
                WindowLabelStyle.Right => i switch
                {
                    0 => "┌ ▄",
                    1 => "├ █",
                    2 => "│ █",
                    3 => "└─▀",
                    _ => "   "
                },
                _ => throw new NotImplementedException()
            },
            WindowThickenStyle.Whole => labeled switch
            {
                WindowLabelStyle.None => i switch
                {
                    0 => "╔  ",
                    1 => "╠  ",
                    2 => "║  ",
                    3 => "╚═ ",
                    _ => "   "
                },
                WindowLabelStyle.Left => i switch
                {
                    0 => "▄  ",
                    1 => "█  ",
                    2 => "█  ",
                    3 => "▀═ ",
                    _ => "   "
                },
                WindowLabelStyle.Right => i switch
                {
                    0 => "╔ ▄",
                    1 => "╠ █",
                    2 => "║ █",
                    3 => "╚═▀",
                    _ => "   "
                },
                _ => throw new NotImplementedException()
            },
            WindowThickenStyle.CornerOnly => labeled switch
            {
                WindowLabelStyle.None => i switch
                {
                    0 => "╔  ",
                    1 => "╠  ",
                    2 => "│  ",
                    3 => "╚─ ",
                    _ => "   "
                },
                WindowLabelStyle.Left => i switch
                {
                    0 => "▄  ",
                    1 => "█  ",
                    2 => "█  ",
                    3 => "▀─ ",
                    _ => "   "
                },
                WindowLabelStyle.Right => i switch
                {
                    0 => "╔ ▄",
                    1 => "╠ █",
                    2 => "│ █",
                    3 => "╚─▀",
                    _ => "   "
                },
                _ => throw new NotImplementedException()
            },
            _ => throw new NotImplementedException()
        };

        string LeftLineSets(WindowThickenStyle thickenType = WindowThickenStyle.None,
            WindowLabelStyle labeled = WindowLabelStyle.None) => labeled switch
        {
            WindowLabelStyle.None => thickenType switch
            {
                WindowThickenStyle.None => "│  ",
                _ => "║  "
            },
            WindowLabelStyle.Left => "█  ",
            WindowLabelStyle.Right => throw new NotImplementedException(),
            _ => throw new NotImplementedException()
        };

        string RightLineSets(WindowThickenStyle thickenType = WindowThickenStyle.None,
            WindowLabelStyle labeled = WindowLabelStyle.None) => labeled switch
        {
            WindowLabelStyle.None => thickenType switch
            {
                WindowThickenStyle.None => "  │",
                _ => "  ║"
            },
            WindowLabelStyle.Left => "  █",
            WindowLabelStyle.Right => throw new NotImplementedException(),
            _ => throw new NotImplementedException()
        };

        string CornerSets(int i, WindowThickenStyle thickenType = WindowThickenStyle.None,
            WindowLabelStyle labeled = WindowLabelStyle.None) => labeled switch
        {
            WindowLabelStyle.None => thickenType switch
            {
                WindowThickenStyle.None => i switch
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
            WindowLabelStyle.Left => thickenType switch
            {
                WindowThickenStyle.None => i switch
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
            WindowLabelStyle.Right => thickenType switch
            {
                WindowThickenStyle.None => i switch
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
            _ => throw new NotImplementedException()
        };

        string LineFill(string set, int count)
        {
            using (var windowStringBuilder = ZString.CreateStringBuilder())
            {
                windowStringBuilder.Append(set[0]);
                windowStringBuilder.Append(TextUtility.Repeat(set[1], count - 2));
                windowStringBuilder.Append(set[2]);
                windowStringBuilder.Append(TextUtility.LineBreaker);
                return windowStringBuilder.ToString();
            }
        }

        public float SetActive(bool active)
        {
            _active = active;
            if (!_hasOutline)
                return 0f;
            if (active)
            {
                if (_coroutine != null)
                    StopCoroutine(_coroutine);
                SetOutlineText(outlineText);
                // coroutine = StartCoroutine(StartupSequence());
                return 0f;
            }
            else
            {
                if (_coroutine != null)
                    StopCoroutine(_coroutine);
                SetOutlineText(string.Empty);
                return 0f;
            }
        }

        void SetOutlineText(string text)
        {
            _outline.SetText(text);
        }

        internal void SetFocusAndAvailable(bool singleWindowOverride, bool inFocus, bool available, bool inInput)
        {
            _singleWindowOverride = singleWindowOverride;
            _inFocus = inFocus;
            _available = available;
            _inInput = inInput;
            if (!_hasOutline)
                return;
            if (_active)
                SetOutlineText(outlineText);
        }
    }
}