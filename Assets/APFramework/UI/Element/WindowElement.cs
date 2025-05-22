using System;
using System.Collections.Generic;
using ChosenConcept.APFramework.UI.Utility;
using ChosenConcept.APFramework.UI.Window;
using Cysharp.Text;
using UnityEngine;

namespace ChosenConcept.APFramework.UI.Element
{
    public class WindowElement<T> : WindowElement where T : WindowElement<T>
    {
        public WindowElement(string name, WindowUI parent) : base(name, parent)
        {
        }

        public T SetAvailable(bool availability)
        {
            if (availability != _available)
            {
                _available = availability;
                _parentWindow.InvokeUpdate();
            }

            return (T)this;
        }

        public T SetLabel(string label)
        {
            return SetLabel(new StringLabel(label));
        }

        public new T SetLabel(IStringLabel label)
        {
            base.SetLabel(label);
            return (T)this;
        }

        public T SetLabel(Func<string> content)
        {
            return SetLabel(new FunctionStringLabel(content));
        }

        public new T ShowLabel(bool show)
        {
            base.ShowLabel(show);
            return (T)this;
        }

        public T SetContent(string content)
        {
            return SetContent(new StringLabel(content));
        }

        public T SetContent(IStringLabel content)
        {
            ClearCache();
            _content = content;
            return (T)this;
        }

        public T SetContent(Func<string> content)
        {
            return SetContent(new FunctionStringLabel(content));
        }
    }

    [Serializable]
    public class WindowElement
    {
        protected IStringLabel _label;
        [SerializeField] bool _showLabel = true;
        [SerializeField] protected string _name;
        [SerializeField] string _tag;
        [SerializeField] protected string _labelCache;
        [SerializeField] string _contentCache;
        protected IStringLabel _content;
        [SerializeField] protected int _count;
        [SerializeField] protected bool _flexible;
        [SerializeField] protected bool _available = true;
        [SerializeField] Vector2 _cachedPositionStart = Vector2.zero;
        [SerializeField] Vector2 _cachedPositionEnd = Vector2.zero;
        [SerializeField] Vector2Int _characterIndex = new(-1, -1);
        [SerializeField] protected WindowUI _parentWindow;
        Action _focusAction = null;
        protected bool _inFocus;
        public bool inFocus => _inFocus;

        public string name => _name;
        public string tag => _tag;
        public IStringLabel rawLabel => _label;
        public IStringLabel rawContent => _content;

        public string label
        {
            get
            {
                if (string.IsNullOrEmpty(_labelCache) && _label != null)
                    _labelCache = _label.GetValue();
                return _labelCache;
            }
        }

        public string content
        {
            get
            {
                if (string.IsNullOrEmpty(_contentCache) && _content != null)
                {
                    _contentCache = _content.GetValue();
                }

                return _contentCache;
            }
        }

        public string labelPrefix
        {
            get
            {
                if (!_showLabel)
                    return string.Empty;
                return ZString.Concat(label, TextUtility.COLUMN);
            }
        }

        public virtual int count
        {
            get => _count;
            set
            {
                if (_count == value)
                    return;
                _count = value;
                _parentWindow.InvokeUpdate();
            }
        }

        public virtual string formattedContent =>
            string.IsNullOrEmpty(content) ? label : ZString.Concat(labelPrefix, content);

        public virtual int getMaxLength
        {
            get
            {
                string display = displayText;
                if (display.Contains('\n'))
                {
                    string[] lines = GetSplitText(display, 0);
                    int maxLength = -1;
                    foreach (string line in lines)
                    {
                        int length = TextUtility.WidthSensitiveLength(line);
                        if (length > maxLength)
                        {
                            maxLength = length;
                        }
                    }

                    if (maxLength == -1)
                        return 0;
                    return maxLength + 1;
                }

                return TextUtility.WidthSensitiveLength(display);
            }
        }

        // For the element to determine how it is displayed by window aka with rich text
        public virtual string displayText
        {
            get
            {
                if (_inFocus && !_parentWindow.isSingleButtonWindow)
                    return StyleUtility.StringColored(TextUtility.StripRichTagsFromStr(formattedContent),
                        _available ? StyleUtility.selected : StyleUtility.disableSelected);
                return _available
                    ? formattedContent
                    : StyleUtility.StringColored(formattedContent, StyleUtility.disabled);
            }
        }

        public bool flexible => _flexible;

        public bool available => _available;
        public int firstCharacterIndex => _characterIndex[0];
        public int lastCharacterIndex => _characterIndex[1];
        public (Vector2, Vector2) cachedPosition => (_cachedPositionStart, _cachedPositionEnd);
        public Vector2 cachedCenter => (_cachedPositionStart + _cachedPositionEnd) / 2f;
        public WindowUI parentWindow => _parentWindow;

        public WindowElement(string name, WindowUI parent)
        {
            _name = name;
            _parentWindow = parent;
            _tag = ZString.Concat(parentWindow.windowTag, ".", name);
            SetLabel(new StringLabel(_name));
        }

        public void SetLabel(IStringLabel label)
        {
            ClearCache();
            _label = label;
            _parentWindow?.InvokeUpdate();
        }

        public void SetLabelCache(string label)
        {
            _labelCache = label;
            _parentWindow?.InvokeUpdate();
        }

        public void SetContent(IStringLabel content)
        {
            ClearCache();
            _content = content;
            _parentWindow?.InvokeUpdate();
        }

        public void SetContentCache(string content)
        {
            _contentCache = content;
            _parentWindow?.InvokeUpdate();
        }

        public void SetFirstCharacterIndex(int characterIndex)
        {
            _characterIndex[0] = characterIndex;
        }

        public void SetLastCharacterIndex(int characterIndex)
        {
            _characterIndex[1] = characterIndex;
        }

        public virtual void ClearCachedPosition()
        {
            _cachedPositionStart = Vector2.zero;
            _cachedPositionEnd = Vector2.zero;
        }

        public void SetCachedPosition((Vector2 startPosition, Vector2 endPosition) position)
        {
            _cachedPositionStart = position.startPosition;
            _cachedPositionEnd = position.endPosition;
        }

        public void ClearFocus()
        {
            if (!_inFocus)
                return;
            _inFocus = false;
            _parentWindow.InvokeUpdate();
        }

        public virtual void SetFocus(bool setFocus)
        {
            if (_inFocus == setFocus)
                return;
            _inFocus = setFocus;
            _parentWindow.InvokeUpdate();

            if (setFocus)
                _focusAction?.Invoke();
        }

        public void Remove() => _parentWindow?.RemoveElement(this);
        public void TriggerGlitch() => _parentWindow?.TriggerGlitch();
        public void AutoResize() => _parentWindow?.AutoResize();

        public virtual void ClearCache()
        {
            _labelCache = null;
            _contentCache = null;
        }

        public virtual void ClearLabelCache()
        {
            _labelCache = null;
        }

        public virtual void ClearContentCache()
        {
            _contentCache = null;
        }

        public void SetLocalizedByTag()
        {
            SetLabel(new LocalizedStringLabel(_tag));
        }

        public virtual string[] GetSplitText(string text, int contentWidth)
        {
            string[] lines = text.Split('\n');
            // when given width is 0, return directly
            if (contentWidth == 0)
                return lines;
            List<string> modifiedLines = new();
            for (int i = 0; i < lines.Length; i++)
            {
                string line = lines[i];
                if (TextUtility.WidthSensitiveLength(line) > contentWidth)
                {
                    modifiedLines.AddRange(TextUtility.StringCutter(line, contentWidth));
                }
                else
                {
                    modifiedLines.Add(line);
                }
            }

            return modifiedLines.ToArray();
        }

        public virtual string[] GetSplitDisplayText(int contentWidth)
        {
            return GetSplitText(displayText, contentWidth);
        }


        public int GetSplitDisplayTextTotalHeight(int contentWidth)
        {
            string[] lines = displayText.Split('\n');
            // when given width is 0, return directly
            if (contentWidth == 0)
                return lines.Length;
            int counter = 0;
            for (int i = 0; i < lines.Length; i++)
            {
                string line = lines[i];
                if (TextUtility.WidthSensitiveLength(line) > contentWidth)
                {
                    counter += TextUtility.StringCutterLineCount(line, contentWidth);
                }
                else
                {
                    counter++;
                }
            }

            return counter;
        }

        public void ShowLabel(bool showLabel)
        {
            _showLabel = showLabel;
        }

        public virtual void Reset() => _ = 0;
    }
}