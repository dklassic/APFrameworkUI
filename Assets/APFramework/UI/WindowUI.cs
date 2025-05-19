using System;
using System.Linq;
using System.Collections.Generic;
using UnityEngine.UI;
using UnityEngine;
using Cysharp.Text;
using TMPro;
using ChosenConcept.APFramework.Interface.Framework.Element;

namespace ChosenConcept.APFramework.Interface.Framework
{
    public class WindowUI : MonoBehaviour
    {
        [Header("Components")] [SerializeField]
        TextMeshProUGUI _drawText = null;

        [SerializeField] WindowOutline _outlineBuilder = null;
        [SerializeField] WindowMask _windowMask = null;
        [SerializeField] WindowBackground _background;
        [SerializeField] LayoutElement _layout;

        [Header("Debug View")] [SerializeField]
        string _windowName = string.Empty;

        [SerializeField] string _windowTag;
        [SerializeField] string _windowLabelContent;
        [SerializeField] string _windowSubscriptContent = null;
        [SerializeField] WindowSetup _setup;
        [SerializeField] LayoutAlignment _layoutAlignment;
        [SerializeField] int _endFillCount = 5;
        [SerializeField] float _fontSize = 10f;
        [SerializeField] bool _maskReady = false;
        [SerializeField] bool _outlineReady = false;
        [SerializeField] bool _noCut = false;
        [SerializeField] int _extraWidth = 0;
        [SerializeField] bool _isDirty = true;
        [SerializeField] List<WindowElement> _elements = new();
        [SerializeField] bool _positionCached = false;
        [SerializeField] Vector2 _cachedPositionStart = Vector2.zero;
        [SerializeField] Vector2 _cachedPositionEnd = Vector2.zero;
        [SerializeField] bool _active = false;
        [SerializeField] bool _isFocused = false;
        [SerializeField] bool _available = true;
        [SerializeField] bool _inInput = false;
        [SerializeField] bool _sizeFixed = false;
        IStringLabel _windowLabel;
        IStringLabel _windowSubscript = new StringLabel("");
        List<ButtonUI> _interactables = new();

        public bool isFocused => _isFocused;
        public bool positionCached => _positionCached;
        public List<ButtonUI> interactables => _interactables;
        public bool isActive => _active;
        public (Vector2, Vector2) cachedPosition => (_cachedPositionStart, _cachedPositionEnd);
        public Vector2 cachedCenter => (_cachedPositionStart + _cachedPositionEnd) / 2f;
        public bool canNavigate => _active && _interactables.Any();
        string windowName => _windowName;
        public string windowTag => _windowTag;
        public bool isSingleButtonWindow => _interactables.Count == 1;

        public string windowLabelContent
        {
            get
            {
                if (_windowLabelContent == null)
                {
                    _windowLabelContent = _windowLabel.GetValue();
                }

                return _windowLabelContent;
            }
        }

        public string windowLabel => _windowLabel.GetValue();
        int titlePreserveLength => Mathf.FloorToInt(TextUtility.WidthSensitiveLength(windowLabelContent));

        public string windowSubscriptContent
        {
            get
            {
                if (_windowSubscriptContent == null)
                {
                    _windowSubscriptContent = _windowSubscript.GetValue();
                }

                return _windowSubscriptContent;
            }
        }

        public WindowSetup setup => _setup;
        int contentWidth => _setup.width - 4;
        public List<WindowElement> elements => _elements;
        public LayoutElement layout => _layout;
        public LayoutAlignment layoutAlignment => _layoutAlignment;
        public bool hasTitle => setup.titleStyle != WindowTitleStyle.None;
        public bool hasTitleBar => setup.titleStyle == WindowTitleStyle.TitleBar;
        public bool hasEmbeddedTitle => setup.titleStyle == WindowTitleStyle.EmbeddedTitle;
        public bool hasOutline => setup.outlineStyle != WindowOutlineStyle.None;
        public bool isFullFrame => setup.outlineStyle == WindowOutlineStyle.FullFrame;

        public bool ContainsPosition(Vector2 position)
        {
            if (!_positionCached)
                return false;
            if (_cachedPositionStart == Vector2.zero && _cachedPositionEnd == Vector2.zero)
                return false;
            Vector2 topLeftDelta = position - _cachedPositionStart;
            if (topLeftDelta.x <= 0 || topLeftDelta.y <= 0)
                return false;
            Vector2 bottomRightDelta = position - _cachedPositionEnd;
            if (bottomRightDelta.x >= 0 || bottomRightDelta.y >= 0)
                return false;
            return true;
        }

        public (Vector2, Vector2) SelectableBound(int index)
        {
            if (index < 0 || index > _interactables.Count || _interactables[index].firstCharacterIndex == -1 ||
                _interactables[index].lastCharacterIndex == -1 ||
                _interactables[index].cachedPosition == (Vector2.zero, Vector2.zero))
                return (Vector2.zero, Vector2.zero);
            return _interactables[index].cachedPosition;
        }

        public bool InteractableContainsPosition(int index, Vector2 position)
        {
            (Vector2 bottomLeft, Vector2 topRight) = SelectableBound(index);
            if (bottomLeft == Vector2.zero && topRight == Vector2.zero)
                return false;
            Vector2 bottomLeftDelta = position - bottomLeft;
            if (bottomLeftDelta.x <= 0 || bottomLeftDelta.y <= 0)
                return false;
            Vector2 topRightDelta = position - topRight;
            if (topRightDelta.x >= 0 || topRightDelta.y >= 0)
                return false;
            return true;
        }

        public void ClearElementsFocus()
        {
            foreach (ButtonUI element in _interactables)
            {
                element.ClearFocus();
            }
        }

        public void ClearWindowFocus()
        {
            SetFocus(false);
        }

        public void ContextLanguageChange()
        {
            ClearCachedValue();
        }

        private void ClearCachedValue()
        {
            _windowLabelContent = null;
            _windowSubscriptContent = null;
            _elements.ForEach(x => x.ClearCachedValue());
        }

        public void SetLocalizedByTag()
        {
            SetLabel(new LocalizedStringLabel(_windowTag));
            _elements.ForEach(x => x.SetLocalizedByTag());
        }

        public void ContextUpdate()
        {
            if (!_active)
                return;
            _windowMask.ContextUpdate();
        }

        public void TriggerSelectionUpdate()
        {
            foreach (WindowElement element in _elements)
            {
                if (element is ButtonUIDoubleConfirm doubleConfirm)
                {
                    doubleConfirm.CancelAwait();
                }
            }
        }

        public void SetOpacity(float alpha)
        {
            if (!_active)
                return;
            _drawText.color = new Color(1, 1, 1, Mathf.Clamp01(alpha));
            _outlineBuilder.SetOpacity(alpha);
        }

        public bool UpdateElementPosition(WindowElement element)
        {
            if (element.firstCharacterIndex >= _drawText.textInfo.characterInfo.Length)
            {
                return false;
            }

            (Vector2, Vector2) result = (Vector2.zero, Vector2.zero);
            for (int i = element.firstCharacterIndex; i <= element.lastCharacterIndex; i++)
            {
                Vector2 rangeBottomLeft = _drawText.textInfo.characterInfo[i].bottomLeft;
                Vector2 rangeTopRight = _drawText.textInfo.characterInfo[i].topRight;
                if (result.Item1.x > rangeBottomLeft.x || result.Item1.x == 0)
                    result.Item1.x = rangeBottomLeft.x;
                if (result.Item1.y > rangeBottomLeft.y || result.Item1.y == 0)
                    result.Item1.y = rangeBottomLeft.y;
                if (result.Item2.x < rangeTopRight.x || result.Item2.x == 0)
                    result.Item2.x = rangeTopRight.x;
                if (result.Item2.y < rangeTopRight.y || result.Item2.y == 0)
                    result.Item2.y = rangeTopRight.y;
            }

            result.Item1 = WindowManager.instance.UIBoundRetriever(transform, result.Item1);
            result.Item2 = WindowManager.instance.UIBoundRetriever(transform, result.Item2);
            element.SetCachedPosition(result);
            if (element is SliderUI uI)
            {
                (Vector2, Vector2) arrowPosition = (Vector2.zero, Vector2.zero);
                Vector2 leftArrowPosition = Vector2.zero;
                leftArrowPosition += WindowManager.instance.UIBoundRetriever(transform,
                    _drawText.textInfo.characterInfo[uI.firstSliderArrowIndex].bottomLeft);
                leftArrowPosition += WindowManager.instance.UIBoundRetriever(transform,
                    _drawText.textInfo.characterInfo[uI.firstSliderArrowIndex].topRight);
                leftArrowPosition /= 2f;
                arrowPosition.Item1 = leftArrowPosition;
                Vector2 rightArrowPosition = Vector2.zero;
                rightArrowPosition += WindowManager.instance.UIBoundRetriever(transform,
                    _drawText.textInfo.characterInfo[uI.lastSliderArrowIndex].bottomLeft);
                rightArrowPosition += WindowManager.instance.UIBoundRetriever(transform,
                    _drawText.textInfo.characterInfo[uI.lastSliderArrowIndex].topRight);
                rightArrowPosition /= 2f;
                arrowPosition.Item2 = rightArrowPosition;
                uI.SetCachedArrowPosition(arrowPosition);
            }

            return true;
        }

        public void UpdateWindowPosition()
        {
            (Vector2, Vector2) result = (Vector2.zero, Vector2.zero);
            if (hasOutline && setup.outlineDisplayStyle == WindowOutlineDisplayStyle.Always)
            {
                for (int i = 0; i <= _outlineBuilder.outline.textInfo.characterCount - 1; i++)
                {
                    Vector2 rangeBottomLeft = _outlineBuilder.outline.textInfo.characterInfo[i].bottomLeft;
                    Vector2 rangeTopRight = _outlineBuilder.outline.textInfo.characterInfo[i].topRight;
                    if (result.Item1.x > rangeBottomLeft.x || result.Item1.x == 0)
                        result.Item1.x = rangeBottomLeft.x;
                    if (result.Item1.y > rangeBottomLeft.y || result.Item1.y == 0)
                        result.Item1.y = rangeBottomLeft.y;
                    if (result.Item2.x < rangeTopRight.x || result.Item2.x == 0)
                        result.Item2.x = rangeTopRight.x;
                    if (result.Item2.y < rangeTopRight.y || result.Item2.y == 0)
                        result.Item2.y = rangeTopRight.y;
                }
            }

            for (int i = 0; i <= _drawText.textInfo.characterCount - 1; i++)
            {
                Vector2 rangeBottomLeft = _drawText.textInfo.characterInfo[i].bottomLeft;
                Vector2 rangeTopRight = _drawText.textInfo.characterInfo[i].topRight;
                if (result.Item1.x > rangeBottomLeft.x || result.Item1.x == 0)
                    result.Item1.x = rangeBottomLeft.x;
                if (result.Item1.y > rangeBottomLeft.y || result.Item1.y == 0)
                    result.Item1.y = rangeBottomLeft.y;
                if (result.Item2.x < rangeTopRight.x || result.Item2.x == 0)
                    result.Item2.x = rangeTopRight.x;
                if (result.Item2.y < rangeTopRight.y || result.Item2.y == 0)
                    result.Item2.y = rangeTopRight.y;
            }

            result.Item1 = WindowManager.instance.UIBoundRetriever(transform, result.Item1);
            result.Item2 = WindowManager.instance.UIBoundRetriever(transform, result.Item2);

            _cachedPositionStart = result.Item1;
            _cachedPositionEnd = result.Item2;
        }


        public void UpdateElementsAndWindowPosition()
        {
            if (!_active || _positionCached)
                return;
            bool quickOut = false;
            foreach (WindowElement element in _elements)
            {
                quickOut = quickOut || !UpdateElementPosition(element);
            }

            if (quickOut)
                return;
            UpdateWindowPosition();
            _positionCached = true;
        }

        public void ClearCachedPosition()
        {
            foreach (WindowElement element in _elements)
            {
                element.ClearCachedPosition();
            }

            _cachedPositionStart = Vector2.zero;
            _cachedPositionEnd = Vector2.zero;
            _positionCached = false;
        }

        void UpdateContent()
        {
            if (!_sizeFixed)
                AutoResize(_extraWidth);
            int count = _elements.Count;
            using (Utf16ValueStringBuilder windowStringBuilder = ZString.CreateStringBuilder())
            {
                windowStringBuilder.Append(TitleBuild());
                if (count == 0)
                {
                    TextUI text = AddText("DummyBlankText");
                    text.SetLabel(TextUtility.Repeat(' ', 10));
                    AutoResize();
                }

                for (int i = 0; i < _elements.Count; i++)
                {
                    string[] texts = _elements[i].GetSplitDisplayText(_noCut ? 0 : contentWidth);
                    for (int k = 0; k < texts.Length; k++)
                    {
                        string text = texts[k];
                        if (TextUtility.WidthSensitiveLength(text) > contentWidth)
                        {
                            if (_noCut)
                            {
                                windowStringBuilder.Append(TextUtility.FULL_SIZE_SPACE);
                                if (k == 0)
                                    _elements[i].SetFirstCharacterIndex(
                                        TextUtility.RichTagsStrippedLength(windowStringBuilder.ToString()));
                                windowStringBuilder.Append(text);
                                if (k == texts.Length - 1)
                                    _elements[i].SetLastCharacterIndex(
                                        TextUtility.RichTagsStrippedLength(windowStringBuilder.ToString()) - 1);
                                windowStringBuilder.Append(TextUtility.LineBreaker);
                            }
                            else
                            {
                                List<string> splitString =
                                    TextUtility.StringCutter(text, contentWidth);
                                for (int j = 0; j < splitString.Count; j++)
                                {
                                    windowStringBuilder.Append(TextUtility.FULL_SIZE_SPACE);
                                    if (j == 0 && k == 0)
                                        _elements[i].SetFirstCharacterIndex(
                                            TextUtility.RichTagsStrippedLength(windowStringBuilder.ToString()));
                                    windowStringBuilder.Append(splitString[j]);
                                    if (j == splitString.Count - 1 && k == texts.Length - 1)
                                        _elements[i].SetLastCharacterIndex(
                                            TextUtility.RichTagsStrippedLength(windowStringBuilder.ToString()) - 1);
                                    windowStringBuilder.Append(TextUtility.LineBreaker);
                                }
                            }
                        }
                        else
                        {
                            windowStringBuilder.Append(TextUtility.FULL_SIZE_SPACE);
                            if (k == 0)
                                _elements[i].SetFirstCharacterIndex(
                                    TextUtility.RichTagsStrippedLength(windowStringBuilder.ToString()));
                            windowStringBuilder.Append(text);
                            if (k == texts.Length - 1)
                                _elements[i].SetLastCharacterIndex(
                                    TextUtility.RichTagsStrippedLength(windowStringBuilder.ToString()) - 1);
                            windowStringBuilder.Append(TextUtility.LineBreaker);
                        }
                    }
                }

                int compensate = 0;
                if (!hasTitle)
                    compensate = -1;
                for (int i = 0; i < _endFillCount; i++)
                {
                    if (windowSubscriptContent != string.Empty && i == _endFillCount - 1 + compensate && !isFullFrame)
                    {
                        windowStringBuilder.Append(TextUtility.FULL_SIZE_SPACE);
                        windowStringBuilder.Append(TextUtility.PlaceHolder(contentWidth +
                                                                           2 -
                                                                           TextUtility.WidthSensitiveLength(
                                                                               windowSubscriptContent)));
                        if (_isFocused)
                            windowStringBuilder.Append(StyleUtility.StringColored(windowSubscriptContent,
                                _available ? StyleUtility.Selected : StyleUtility.DisableSelected));
                        else
                            windowStringBuilder.Append(_available
                                ? windowSubscriptContent
                                : StyleUtility.StringColored(windowSubscriptContent, StyleUtility.Disabled));
                        windowStringBuilder.Append(TextUtility.LineBreaker);
                    }
                    else if (windowSubscriptContent != string.Empty && i == _endFillCount - 1 + compensate &&
                             isFullFrame)
                    {
                        windowStringBuilder.Append(TextUtility.FULL_SIZE_SPACE);
                        windowStringBuilder.Append(TextUtility.PlaceHolder(contentWidth +
                                                                           2 -
                                                                           TextUtility.WidthSensitiveLength(
                                                                               windowSubscriptContent)));
                        if (_isFocused)
                            windowStringBuilder.Append(StyleUtility.StringColored(windowSubscriptContent,
                                _available ? StyleUtility.Selected : StyleUtility.DisableSelected));
                        else
                            windowStringBuilder.Append(_available
                                ? windowSubscriptContent
                                : StyleUtility.StringColored(windowSubscriptContent, StyleUtility.Disabled));
                        windowStringBuilder.Append(TextUtility.LineBreaker);
                    }
                    else if (i == _endFillCount - 2 + compensate && !isFullFrame)
                        windowStringBuilder.Append(TextUtility.LineBreaker);
                    else
                        windowStringBuilder.Append(TextUtility.LineBreaker);
                }

                _drawText.SetText(windowStringBuilder);
            }
        }

        public void ContextLateUpdate()
        {
            CheckDirty();
        }

        void CheckDirty()
        {
            if (_isDirty)
            {
                UpdateContent();
                _isDirty = false;
            }
        }

        public void ClearElements()
        {
            _elements.Clear();
            _interactables.Clear();
        }

        string TitleBuild()
        {
            if (!hasTitle)
                return TextUtility.LineBreaker + TextUtility.LineBreaker;
            if (hasTitleBar)
                return TextUtility.TitleOpener + (_elements.Count == 1) switch
                {
                    true => _isFocused switch
                    {
                        false => _available
                            ? StyleUtility.StringBold(windowLabelContent.ToUpper())
                            : StyleUtility.StringColored(StyleUtility.StringBold(windowLabel.ToUpper()),
                                StyleUtility.Disabled),
                        true => StyleUtility.StringColored(StyleUtility.StringBold(windowLabel.ToUpper()),
                            _available ? StyleUtility.Selected : StyleUtility.DisableSelected),
                    },
                    false => StyleUtility.StringBold(windowLabelContent.ToUpper()),
                } + TextUtility.LineBreaker + TextUtility.LineBreaker;
            if (hasEmbeddedTitle)
                return " " +
                       (_elements.Count == 1) switch
                       {
                           true => _isFocused switch
                           {
                               false => _available
                                   ? StyleUtility.StringBold(windowLabelContent.ToUpper())
                                   : StyleUtility.StringColored(StyleUtility.StringBold(windowLabel.ToUpper()),
                                       StyleUtility.Disabled),
                               true => StyleUtility.StringColored(StyleUtility.StringBold(windowLabel.ToUpper()),
                                   _available ? StyleUtility.Selected : StyleUtility.DisableSelected),
                           },
                           false => StyleUtility.StringBold(windowLabelContent.ToUpper()),
                       } + TextUtility.LineBreaker;
            return TextUtility.LineBreaker;
        }

        public void Initialize(string elementName, string parent, WindowSetup windowSetup)
        {
            _windowName = elementName;
            _windowTag = $"{parent}.{elementName}";
            _windowLabel = new StringLabel(_windowName);
            _setup = windowSetup;
            SetFont(windowSetup.fontSize);
            SetActive(false);
            if (windowSetup.width != 0 && windowSetup.height != 0)
                Resize(windowSetup.width, windowSetup.height);
            else if (windowSetup.width != 0 && windowSetup.height == 0)
                Resize(windowSetup.width);
            _background.SetColor(windowSetup.backgroundColor);
        }

        public void SetBackgroundColor(Color color)
        {
            _background.SetColor(color, _active);
        }

        public void ChangeSetup(WindowSetup windowSetup)
        {
            _setup = windowSetup;
        }

        void SetLayout(int widthCount, int heightCount)
        {
            // layout.minWidth = fontSize * .62f * widthCount * (hasOutline ? 1.1f : 1f);
            _layout.minWidth = _fontSize * Mathf.Max(0.65f * widthCount, 7);

            // layout.minHeight = fontSize * .67f * heightCount * 1.6f;
            _layout.minHeight = _fontSize * 1.05f * heightCount;

            _layoutAlignment.UpdateLayout();
        }

        public void SetFont(float fontSize)
        {
            _fontSize = fontSize;
            _drawText.fontSize = fontSize;
            _outlineBuilder.outline.fontSize = fontSize;
            _windowMask.mask.fontSize = fontSize;
        }

        /// <summary>
        /// To automatically fit UI according to content size
        /// </summary>
        /// <param name="extraWidth">Extra amount of width to preserve.</param>
        public void AutoResize(int extraWidth = 0)
        {
            _sizeFixed = false;
            _extraWidth = extraWidth;
            int targetHeight = GetAutoResizeHeight();
            int targetWidth = GetAutoResizeWidth(extraWidth);
            _endFillCount = GetEndFillCount();
            if (hasOutline)
            {
                int subscriptLength = TextUtility.WidthSensitiveLength(windowSubscriptContent);
                SetupOutline(targetWidth + 1, targetHeight, _setup, titlePreserveLength + 2, subscriptLength);
            }

            _setup.SetWidth(targetWidth);
            _setup.SetHeight(targetHeight);
            SetupMask(targetWidth, targetHeight + 2, _setup);
            SetLayout(targetWidth, targetHeight);
            _noCut = true;
        }

        public int GetAutoResizeWidth(int extraWidth)
        {
            int count = _elements.Count;
            int minimumWidth = 0;
            if (hasTitleBar)
                minimumWidth = titlePreserveLength + 2;
            if (hasEmbeddedTitle)
                minimumWidth = titlePreserveLength;
            if (windowSubscriptContent != string.Empty)
                minimumWidth = Mathf.Max(minimumWidth, TextUtility.WidthSensitiveLength(windowSubscriptContent) + 1);
            for (int i = 0; i < count; i++)
            {
                if (!_elements[i].flexible || count == 1)
                {
                    int elementLength = _elements[i].getMaxLength;
                    if (elementLength > minimumWidth)
                        minimumWidth = elementLength;
                }
            }

            int targetWidth = minimumWidth + 2 + extraWidth;
            return targetWidth;
        }

        public int GetAutoResizeHeight()
        {
            int count = _elements.Count;
            int minimumHeight = 0;
            for (int i = 0; i < count; i++)
            {
                int splits = _elements[i].displayText.Split("\n").Length;
                minimumHeight += splits;
            }

            if (hasTitleBar)
                minimumHeight += 1;
            int targetHeight = minimumHeight + 2;
            return targetHeight;
        }

        public int GetEndFillCount()
        {
            if (hasEmbeddedTitle)
                return 1;
            return 2;
        }

        /// <summary>
        /// To resize UI with specified width. Height will be automatically adjusted.
        /// </summary>
        public void Resize(int width)
        {
            _sizeFixed = true;
            _extraWidth = 0;
            int minimumHeight = 0;
            int count = _elements.Count;
            for (int i = 0; i < count; i++)
            {
                minimumHeight += _elements[i].GetSplitDisplayText(width).Length;
            }

            _endFillCount = 2;
            if (hasTitleBar)
                minimumHeight += 3;
            else if (hasEmbeddedTitle)
                _endFillCount = 1;
            int targetWidth = width + 4;
            int targetHeight = minimumHeight + 2;
            if (hasOutline)
            {
                int subscriptLength = TextUtility.WidthSensitiveLength(windowSubscriptContent);
                SetupOutline(targetWidth + 1, targetHeight, _setup, titlePreserveLength + 2, subscriptLength);
            }

            _setup.SetWidth(targetWidth);
            _setup.SetHeight(targetHeight);
            SetupMask(targetWidth, targetHeight + 2, _setup);
            SetLayout(targetWidth, targetHeight);
            _noCut = false;
        }

        /// <summary>
        /// To resize UI with specified width and height.
        /// </summary>
        public void Resize(int width, int height)
        {
            _sizeFixed = true;
            _extraWidth = 0;
            _endFillCount = 2;
            if (hasEmbeddedTitle)
                _endFillCount = 1;
            int targetWidth = width + 4;
            int targetHeight = height + 2;
            if (hasOutline)
            {
                int subscriptLength = TextUtility.WidthSensitiveLength(windowSubscriptContent);
                SetupOutline(targetWidth + 1, targetHeight, _setup, titlePreserveLength + 2, subscriptLength);
            }

            _setup.SetWidth(targetWidth);
            _setup.SetHeight(targetHeight);
            SetupMask(targetWidth, targetHeight + 2, _setup);
            SetLayout(targetWidth, targetHeight);
            _noCut = false;
        }

        void SetupOutline(int width, int height, WindowSetup windowSetup, int titleOverride, int subLength)
        {
            _outlineBuilder.SetOutline(width, height, windowSetup, titleOverride, subLength);
            _outlineReady = true;
        }

        void SetupMask(int width, int height, WindowSetup windowSetup)
        {
            _windowMask.Setup(width, height, windowSetup);
            _maskReady = true;
        }

        public void Close()
        {
            // Do fancy animation
            WindowManager.instance.DelistWindow(this);
        }

        public void MoveElement(WindowElement element, int targetIndex)
        {
            _elements.Remove(element);
            _elements.Insert(targetIndex, element);
            if (element is ButtonUI)
            {
                UpdateSelectables();
            }
        }

        public void UpdateSelectables()
        {
            _interactables.Clear();
            _elements.Where(x => x is ButtonUI).ToList().ForEach(x => _interactables.Add(x as ButtonUI));
        }

        public void RemoveElement(WindowElement element)
        {
            _elements.Remove(element);
            if (element is ButtonUI uI)
                _interactables.Remove(uI);
        }

        public TextUI AddText(string elementName)
        {
            TextUI text = new(elementName, this);
            _elements.Add(text);
            return text;
        }

        public ButtonUI AddButton(string elementName, Action action = null)
        {
            ButtonUI button = new(elementName, this);
            button.SetAction(action);
            _elements.Add(button);
            _interactables.Add(button);
            return button;
        }

        public ScrollableTextUI AddScrollableText(string elementName, Action action = null)
        {
            ScrollableTextUI button = new(elementName, this);
            button.SetAction(action);
            _elements.Add(button);
            _interactables.Add(button);
            return button;
        }

        public ButtonUIWithContent AddButtonWithContent(string elementName, Action action = null)
        {
            ButtonUIWithContent button = new(elementName, this);
            button.SetAction(action);
            _elements.Add(button);
            _interactables.Add(button);
            return button;
        }

        public ButtonUIDoubleConfirm AddDoubleConfirmButton(string elementName, Action action = null)
        {
            ButtonUIDoubleConfirm button = new(elementName, this);
            button.SetAction(action);
            _elements.Add(button);
            _interactables.Add(button);
            return button;
        }

        public ToggleUI AddToggle(string elementName, Action<bool> action = null)
        {
            ToggleUI toggle = new(elementName, this);
            toggle.SetAction(action);
            _elements.Add(toggle);
            _interactables.Add(toggle);
            return toggle;
        }

        public ToggleUIWithContent AddToggleWithContent(string elementName, Action<bool> action = null)
        {
            ToggleUIWithContent toggle = new(elementName, this);
            toggle.SetAction(action);
            _elements.Add(toggle);
            _interactables.Add(toggle);
            return toggle;
        }

        public SliderUI AddSlider(string elementName, Action<int> action = null)
        {
            SliderUI slider = new(elementName, this);
            slider.SetAction(action);
            _elements.Add(slider);
            _interactables.Add(slider);
            return slider;
        }

        public SliderUIChoice<T> AddSliderWithChoice<T>(string elementName, Action<T> action = null)
        {
            SliderUIChoice<T> slider = new SliderUIChoice<T>(elementName, this);
            slider.SetAction(action);
            _elements.Add(slider);
            _interactables.Add(slider);
            return slider;
        }

        public ButtonUICountable AddButtonWithCount(string elementName, Action<int> action = null)
        {
            ButtonUICountable button = new(elementName, this);
            button.SetAction(action);
            _elements.Add(button);
            _interactables.Add(button);
            return button;
        }

        public SingleSelectionUI<T> AddSingleSelection<T>(string elementName, Action<T> action = null)
        {
            SingleSelectionUI<T> selection = new SingleSelectionUI<T>(elementName, this);
            selection.SetAction(action);
            _elements.Add(selection);
            _interactables.Add(selection);
            return selection;
        }

        public TextInputUI AddTextInput(string elementName, Action<string> action = null)
        {
            TextInputUI selection = new(elementName, this);
            selection.SetAction(action);
            _elements.Add(selection);
            _interactables.Add(selection);
            return selection;
        }

        public TextInputUIWithPrediction AddTextInputNoLabelWithPrediction(string elementName,
            Action<string> action = null)
        {
            TextInputUIWithPrediction selection = new(elementName, this);
            selection.SetAction(action);
            _elements.Add(selection);
            _interactables.Add(selection);
            return selection;
        }

        public void SetActive(bool v, bool syncGameObject = true)
        {
            if (hasOutline && !_outlineReady || !_maskReady)
                return;
            if (_active == v)
                return;
            _active = v;
            if (syncGameObject && v)
                gameObject.SetActive(true);
            if (_active)
            {
                // SetOpacity(alpha, false);
                ResetAllWindowElement();
                _outlineBuilder.SetActive(true);
                _windowMask.FadeIn();
                _background.SetActive(true);
                InvokeUpdate();
            }
            else
            {
                _outlineBuilder.SetActive(false);
                _windowMask.FadeOut();
                ResetAllWindowElement();
                DeactivateGameObject();
                _background.SetActive(false);
                _drawText.SetText(string.Empty);
            }
        }

        void ResetAllWindowElement()
        {
            foreach (WindowElement element in _elements)
            {
                element.Reset();
            }
        }

        internal void SetActiveNoVFX(bool v, bool syncGameObject = true)
        {
            if (hasOutline && !_outlineReady || !_maskReady)
                return;
            if (_active == v)
                return;
            _active = v;
            if (syncGameObject)
                gameObject.SetActive(v);
            if (_active)
            {
                _outlineBuilder.SetActive(true);
                _background.SetActive(true);
                InvokeUpdate();
            }
            else
            {
                _outlineBuilder.SetActive(false);
                _background.SetActive(false);
                _drawText.SetText(string.Empty);
            }
        }

        public void SetGameObjectActive(bool v)
        {
            gameObject.SetActive(v);
        }

        void DeactivateGameObject()
        {
            SetGameObjectActive(false);
        }

        public void SetLabel(string label)
        {
            _windowLabelContent = null;
            _windowLabel = new StringLabel(label);
            _isDirty = true;
        }

        public void SetLabel(IStringLabel label)
        {
            _windowLabelContent = null;
            _windowLabel = label;
            _isDirty = true;
        }

        public void SetSubscript(string subscript)
        {
            _windowSubscriptContent = null;
            _windowSubscript = new StringLabel(subscript);
            _isDirty = true;
        }

        public void SetSubscript(IStringLabel label)
        {
            _windowSubscriptContent = null;
            _windowSubscript = label;
            _isDirty = true;
        }

        public void RefreshSize()
        {
            if (!_noCut)
                return;
            AutoResize(_extraWidth);
        }

        public void InvokeUpdate()
        {
            _isDirty = true;
        }

        public void TriggerGlitch()
        {
            if (_maskReady) _windowMask.TriggerGlitch();
        }

        public void TriggerEffect(WindowTransition transitionSetup)
        {
            if (_maskReady) _windowMask.TriggerEffect(transitionSetup);
        }

        public void TriggerDamageGlitch()
        {
            if (_maskReady) _windowMask.TriggerDamageGlitch();
        }

        internal void SetMaskColor(ColorCode code)
        {
            _windowMask.SetColor(code);
        }

        public void SetInput(bool inInput)
        {
            _inInput = inInput;
            _outlineBuilder.SetFocusAndAvailable(isSingleButtonWindow, _isFocused, _available, _inInput);
            _isDirty = true;
        }

        internal void SetFocus(bool inFocus)
        {
            _isFocused = inFocus;
            _outlineBuilder.SetFocusAndAvailable(isSingleButtonWindow, _isFocused, _available, _inInput);
            _isDirty = true;
        }

        internal void SetAvailable(bool available)
        {
            _available = available;
            _outlineBuilder.SetFocusAndAvailable(isSingleButtonWindow, _isFocused, _available, _inInput);
            _isDirty = true;
        }

        void ElementClearState()
        {
            foreach (WindowElement element in _elements)
            {
                if (element is ButtonUIDoubleConfirm)
                    (element as ButtonUIDoubleConfirm).SetConfirm(false);
            }
        }

        public void RegisterLayout(LayoutAlignment layout)
        {
            _layoutAlignment = layout;
        }

        public void Move(Vector2 delta)
        {
            if (transform.IsChildOf(_layoutAlignment.transform))
                transform.SetParent(_layoutAlignment.transform.parent);
            transform.Translate(delta.x, delta.y, 0);
        }

        public void RevertPosition()
        {
            transform.SetParent(_layoutAlignment.transform);
        }
    }
}