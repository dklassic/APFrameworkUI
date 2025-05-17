using System;
using System.Collections.Generic;
using UnityEngine;

namespace ChosenConcept.APFramework.Interface.Framework.Element
{
    [Serializable]
    public class WindowElement
    {
        protected IStringLabel _label;
        [SerializeField] bool _showLabel = true;
        [SerializeField] protected string _name;
        [SerializeField] string _tag;
        [SerializeField] protected string _labelCache;
        [SerializeField] protected int _count;
        [SerializeField] protected bool _flexible;
        [SerializeField] protected bool _available = true;
        [SerializeField] Vector2 _cachedPositionStart = Vector2.zero;
        [SerializeField] Vector2 _cachedPositionEnd = Vector2.zero;
        [SerializeField] Vector2Int _characterIndex = new(-1, -1);
        [SerializeField] protected WindowUI _parentWindow;
        public string name => _name;
        public string tag => _tag;

        public string label
        {
            get
            {
                if (_labelCache == null)
                    _labelCache = _label.GetValue();
                return _labelCache;
            }
        }

        public string labelPrefix
        {
            get
            {
                if (!_showLabel)
                    return string.Empty;
                if (_labelCache == null)
                    _labelCache = _label.GetValue();
                return _labelCache + TextUtility.column;
            }
        }

        public virtual int count
        {
            get => _count;
            set
            {
                _count = value;
                _parentWindow.InvokeUpdate();
            }
        }

        public virtual string formattedContent => label;
        public string rawContent => label;

        public virtual int getMaxLength
        {
            get
            {
                string[] lines = GetSplitDisplayText(0);
                int maxLength = -1;
                foreach (string line in lines)
                {
                    int length = TextUtility.ActualLength(line);
                    if (length > maxLength)
                    {
                        maxLength = length;
                    }
                }

                if (maxLength == -1)
                    return 0;
                return maxLength + 1;
            }
        }

        // For the element to determine how it is displayed by window aka with rich text
        public virtual string displayText => formattedContent;
        public bool flexible => _flexible;

        public bool available => _available;
        public int firstCharacterIndex => _characterIndex[0];
        public int lastCharacterIndex => _characterIndex[1];
        public (Vector2, Vector2) cachedPosition => (_cachedPositionStart, _cachedPositionEnd);
        public WindowUI parentWindow => _parentWindow;

        public WindowElement(string name, WindowUI parent)
        {
            _name = name;
            SetParentWindow(parent);
            _tag = $"{parentWindow.windowTag}.{name}";
            SetLabel(new StringLabel(_name));
        }
        public void SetFirstCharacterIndex(int characterIndex)
        {
            _characterIndex[0] = characterIndex;
        }

        public void SetLastCharacterIndex(int characterIndex)
        {
            _characterIndex[1] = characterIndex;
        }

        public void ClearCachedPosition()
        {
            _cachedPositionStart = Vector2.zero;
            _cachedPositionEnd = Vector2.zero;
        }

        public void SetCachedPosition((Vector2 startPosition, Vector2 endPosition) position)
        {
            _cachedPositionStart = position.startPosition;
            _cachedPositionEnd = position.endPosition;
        }

        public virtual void Activate() => _ = 0;
        public virtual void Deactivate() => _ = 0;

        public void SetLabel(string label)
        {
            ClearCachedValue();
            _label = new StringLabel(label);
            _parentWindow?.InvokeUpdate();
        }

        public void SetLabel(IStringLabel label)
        {
            ClearCachedValue();
            _label = label;
            _parentWindow?.InvokeUpdate();
        }

        public void SetParentWindow(WindowUI window) => _parentWindow = window;
        public void Remove() => _parentWindow?.RemoveElement(this);
        public void TriggerGlitch() => _parentWindow?.TriggerGlitch();
        public void AutoResize() => _parentWindow?.AutoResize();

        public virtual void SetAvailable(bool availability)
        {
            _available = availability;
            parentWindow.InvokeUpdate();
        }

        public virtual void ClearCachedValue()
        {
            _labelCache = null;
        }

        public void SetLocalizedByTag()
        {
            SetLabel(new LocalizedStringLabel(_tag));
        }

        public virtual string[] GetSplitDisplayText(int contentWidth)
        {
            string[] lines = displayText.Split('\n');
            // when given width is 0, return directly
            if (contentWidth == 0)
                return lines;
            List<string> modifiedLines = new();
            for (int i = 0; i < lines.Length; i++)
            {
                string line = lines[i];
                if (TextUtility.ActualLength(line) > contentWidth)
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
                if (TextUtility.ActualLength(line) > contentWidth)
                {
                    counter += TextUtility.StringCutter(line, contentWidth).Count;
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