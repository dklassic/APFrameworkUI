using System.Collections.Generic;
using Cysharp.Text;
using UnityEngine;

namespace ChosenConcept.APFramework.Interface.Framework.Element
{
    public class ScrollableTextUI : ButtonUI
    {
        int _contentHeight = 1;
        bool _inScroll;
        int _cachedContentWidth = -1;
        public int contentHeight => _contentHeight;
        public override string displayText => formattedContent;

        public override int count
        {
            get => _count;
            set
            {
                _count = value;
                if (_count < 0)
                    _count = 0;
                int totalHeight = GetSplitDisplayTextTotalHeight(_cachedContentWidth);
                if (_count + _contentHeight >= totalHeight)
                    _count = totalHeight - _contentHeight;
                if (_count != value)
                    return;
                _parentWindow.InvokeUpdate();
            }
        }


        public ScrollableTextUI(string content, WindowUI parent) : base(content, parent)
        {
        }

        public override void Reset() => count = 0;

        public ScrollableTextUI SetContentHeight(int contentHeight)
        {
            if (contentHeight <= 1)
                return this;
            _contentHeight = contentHeight;
            return this;
        }

        public void SetScrolling(bool inScroll)
        {
            if (inScroll == _inScroll)
                return;
            _inScroll = inScroll;
            _parentWindow.InvokeUpdate();
        }

        //▲▼＜＞
        public override string[] GetSplitDisplayText(int contentWidth)
        {
            // Leave headroom for the scroll indicator
            _cachedContentWidth = contentWidth;
            string[] lines = displayText.Split('\n');
            List<string> modifiedLines = new();
            int currentLineCounter = 0;
            int totalHeight = GetSplitDisplayTextTotalHeight(_cachedContentWidth);
            for (int i = 0; i < lines.Length; i++)
            {
                if (modifiedLines.Count >= _contentHeight)
                    break;
                string line = lines[i];
                if (_cachedContentWidth > 0 && TextUtility.WidthSensitiveLength(line) > _cachedContentWidth)
                {
                    List<string> cuts = TextUtility.StringCutter(line, _cachedContentWidth);
                    for (int j = 0; j < cuts.Count; j++)
                    {
                        if (modifiedLines.Count < _contentHeight && currentLineCounter >= _count)
                            modifiedLines.Add(cuts[j]);
                        currentLineCounter++;
                    }
                }
                else
                {
                    if (modifiedLines.Count < _contentHeight && currentLineCounter >= _count)
                        modifiedLines.Add(line);
                    currentLineCounter++;
                }
            }

            // Pad out the height
            while (modifiedLines.Count < _contentHeight)
            {
                modifiedLines.Add("");
            }

            using (Utf16ValueStringBuilder builder = ZString.CreateStringBuilder())
            {
                string fullLine = TextUtility.Repeat('─', _cachedContentWidth - 2);
                {
                    builder.Clear();
                    if (_inFocus && !_inScroll)
                        builder.Append(StyleUtility.StringColored(fullLine, StyleUtility.selected));
                    else
                        builder.Append(fullLine);
                    if (_inScroll)
                        builder.Append(StyleUtility.StringColored("▲",
                            _count == 0 ? StyleUtility.disableSelected : StyleUtility.selected));
                    else
                        builder.Append(_count == 0 ? StyleUtility.StringColored("▲", StyleUtility.disabled) : "▲");
                    modifiedLines.Insert(0, builder.ToString());
                }
                {
                    builder.Clear();
                    if (_inFocus && !_inScroll)
                        builder.Append(StyleUtility.StringColored(fullLine, StyleUtility.selected));
                    else
                        builder.Append(fullLine);
                    if (_inScroll)
                        builder.Append(StyleUtility.StringColored("▼",
                            _count + _contentHeight == totalHeight
                                ? StyleUtility.disableSelected
                                : StyleUtility.selected));
                    else
                        builder.Append(_count + _contentHeight == totalHeight
                            ? StyleUtility.StringColored("▼", StyleUtility.disabled)
                            : "▼");
                    modifiedLines.Add(builder.ToString());
                }
            }

            return modifiedLines.ToArray();
        }

        public (bool _hoverOnDecrease, bool _hoverOnIncrease) HoverOnArrow(Vector2 position)
        {
            Vector2 upperArrowDelta = cachedPosition.Item2 - position;
            Vector2 lowerArrowDelta = position - cachedPosition.Item1;
            float fontSize = _parentWindow.setup.fontSize;
            bool hoverOnDecrease = false;
            bool hoverOnIncrease = false;
            if (upperArrowDelta is { x: >= 0, y: >= 0 } && lowerArrowDelta is { x: >= 0, y: >= 0 })
            {
                if (upperArrowDelta.y < lowerArrowDelta.y &&
                    upperArrowDelta.y < fontSize * 0.75f)
                {
                    hoverOnDecrease = true;
                }
                else if (upperArrowDelta.y > lowerArrowDelta.y &&
                         lowerArrowDelta.y < fontSize * 0.75f)
                {
                    hoverOnIncrease = true;
                }
            }

            return (hoverOnDecrease, hoverOnIncrease);
        }
    }
}