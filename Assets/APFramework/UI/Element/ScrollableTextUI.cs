using System.Collections.Generic;
using Cysharp.Text;

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
                _parentWindow.InvokeUpdate();
            }
        }


        public ScrollableTextUI(string content, WindowUI parent) : base(content, parent)
        {
        }
        public override void Reset() => count = 0;
        public void SetContentHeight(int contentHeight)
        {
            if (contentHeight <= 1)
                return;
            _contentHeight = contentHeight;
        }
        public void SetScrolling(bool inScroll)
        {
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
                if (_cachedContentWidth > 0 && TextUtility.ActualLength(line) > _cachedContentWidth)
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
                        builder.Append(StyleUtility.StringColored(fullLine, StyleUtility.Selected));
                    else
                        builder.Append(fullLine);
                    if (_inScroll)
                        builder.Append(StyleUtility.StringColored("▲", _count == 0 ? StyleUtility.DisableSelected : StyleUtility.Selected));
                    else
                        builder.Append(_count == 0 ? StyleUtility.StringColored("▲", StyleUtility.Disabled) : "▲");
                    modifiedLines.Insert(0, builder.ToString());
                }
                {
                    builder.Clear();
                    if (_inFocus && !_inScroll)
                        builder.Append(StyleUtility.StringColored(fullLine, StyleUtility.Selected));
                    else
                        builder.Append(fullLine);
                    if (_inScroll)
                        builder.Append(StyleUtility.StringColored("▼", _count + _contentHeight == totalHeight ? StyleUtility.DisableSelected : StyleUtility.Selected));
                    else
                        builder.Append(_count + _contentHeight == totalHeight ? StyleUtility.StringColored("▼", StyleUtility.Disabled) : "▼");
                    modifiedLines.Add(builder.ToString());
                }
            }
            return modifiedLines.ToArray();
        }
    }
}
