using System;
using System.Collections.Generic;
using System.Linq;
using ChosenConcept.APFramework.UI.Element;
using ChosenConcept.APFramework.UI.Layout;
using ChosenConcept.APFramework.UI.Utility;
using Cysharp.Text;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace ChosenConcept.APFramework.UI.Window
{
    public class WindowUI : MonoBehaviour
    {
        const int OUTLINE_PADDING = 1;

        [Header("Components")] [SerializeField]
        private RectTransform _transform;

        [SerializeField] TextMeshProUGUI _drawText;

        [SerializeField] WindowOutline _outlineBuilder;
        [SerializeField] WindowMask _windowMask;
        [SerializeField] WindowBackground _background;
        [SerializeField] LayoutElement _layout;

        [Header("Debug View")] [SerializeField]
        string _windowName = string.Empty;

        [SerializeField] string _windowTag;
        [SerializeField] string _windowLabelCache;
        [SerializeField] string _windowSubscriptCache;
        [SerializeField] WindowSetup _setup;
        [SerializeField] LayoutAlignment _layoutAlignment;
        [SerializeField] int _endFillCount = 5;
        [SerializeField] bool _maskReady;
        [SerializeField] bool _outlineReady;
        [SerializeField] int _extraWidth;
        [SerializeField] bool _isDirty = true;
        [SerializeField] List<WindowElement> _elements = new();
        [SerializeField] bool _positionCached;
        [SerializeField] Vector2 _cachedPositionStart = Vector2.zero;
        [SerializeField] Vector2 _cachedPositionEnd = Vector2.zero;
        [SerializeField] bool _active;
        [SerializeField] bool _isFocused;
        [SerializeField] bool _available = true;
        [SerializeField] bool _inInput;
        [SerializeField] int _designatedWidth;
        [SerializeField] int _designatedHeight;
        [SerializeField] bool _awaitDeactivate;
        [SerializeField] bool _preciseSizeSync;
        [SerializeField] float _nextFunctionStringUpdate = Mathf.NegativeInfinity;
        IStringLabel _windowLabel;
        IStringLabel _windowSubscript = new StringLabel("");
        List<WindowElement> _interactables = new();

        public bool isFocused => _isFocused;
        public bool positionCached => _positionCached;
        public List<WindowElement> interactables => _interactables;
        public bool isActive => _active;
        public (Vector2, Vector2) cachedPosition => (_cachedPositionStart, _cachedPositionEnd);
        public Vector2 cachedCenter => (_cachedPositionStart + _cachedPositionEnd) / 2f;
        public bool canNavigate => _active && _interactables.Count > 0;
        string windowName => _windowName;
        public string windowTag => _windowTag;
        public bool isSingleButtonWindow => _elements.Count == 1 && _elements[0] is not TextUI;

        public string windowLabel
        {
            get
            {
                if (string.IsNullOrEmpty(_windowLabelCache))
                {
                    _windowLabelCache = _windowLabel.GetValue();
                }

                return _windowLabelCache;
            }
        }

        int titlePreserveLength => Mathf.FloorToInt(TextUtility.WidthSensitiveLength(windowLabel));

        public string windowSubscript
        {
            get
            {
                if (string.IsNullOrEmpty(_windowSubscriptCache))
                {
                    _windowSubscriptCache = _windowSubscript.GetValue();
                }

                return _windowSubscriptCache;
            }
        }

        public WindowSetup setup => _setup;
        int contentWidth => _setup.width - 2;
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
            foreach (WindowElement element in _interactables)
            {
                element.ClearFocus();
            }
        }

        public void ClearWindowFocus()
        {
            SetFocus(false);
        }

        void ClearCachedValue()
        {
            _windowLabelCache = null;
            _windowSubscriptCache = null;
            _elements.ForEach(x => x.ClearCache());
        }

        void CheckFunctionStringLabelDirty()
        {
            foreach (WindowElement element in _elements)
            {
                if (element.rawContent is FunctionStringLabel content)
                {
                    string result = ((IStringLabel)content).GetValue();
                    if (result != element.content)
                        element.SetContentCache(result);
                }

                if (element.rawLabel is FunctionStringLabel label)
                {
                    string result = ((IStringLabel)label).GetValue();
                    if (result != element.label)
                        element.SetLabelCache(result);
                }
            }
        }


        public void SetLocalizedByTag()
        {
            SetLabel(new LocalizedStringLabel(_windowTag));
            _elements.ForEach(x => x.SetLocalizedByTag());
        }

        public void UpdateWindow()
        {
            if (_active && _nextFunctionStringUpdate < Time.unscaledTime)
            {
                _nextFunctionStringUpdate = Time.unscaledTime + _setup.functionStringUpdateInterval;
                CheckFunctionStringLabelDirty();
                if (_setup.syncActiveValueAutomatically)
                {
                    SyncActiveValue();
                }
            }

            if (!_active && !_awaitDeactivate)
                return;
            _windowMask.ContextUpdate();
            if (_awaitDeactivate && !_windowMask.needUpdate)
                SetGameObjectActive(false);
        }

        public void TriggerSelectionUpdate()
        {
            foreach (WindowElement element in _elements)
            {
                if (element is ButtonUI button)
                {
                    button.CancelAwait();
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
            if (element is ISlider uI)
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
            if (_designatedWidth == 0 || _designatedHeight == 0)
            {
                RefreshSize();
            }

            int count = _elements.Count;
            int characterCount = 0;
            using (Utf16ValueStringBuilder windowStringBuilder = ZString.CreateStringBuilder())
            {
                // Build window title
                if (!hasTitle)
                {
                    windowStringBuilder.Append(TextUtility.LineBreaker);
                    windowStringBuilder.Append(TextUtility.LineBreaker);
                }
                else if (hasTitleBar)
                {
                    windowStringBuilder.Append(TextUtility.TitleOpener);
                    windowStringBuilder.Append((_elements.Count == 1) switch
                    {
                        true => _isFocused switch
                        {
                            false => _available
                                ? StyleUtility.StringBold(windowLabel.ToUpper())
                                : StyleUtility.StringColored(StyleUtility.StringBold(windowLabel.ToUpper()),
                                    StyleUtility.disabled),
                            true => StyleUtility.StringColored(StyleUtility.StringBold(windowLabel.ToUpper()),
                                _available ? StyleUtility.selected : StyleUtility.disableSelected),
                        },
                        false => StyleUtility.StringBold(windowLabel.ToUpper()),
                    });
                    windowStringBuilder.Append(TextUtility.LineBreaker);
                    windowStringBuilder.Append(TextUtility.LineBreaker);
                }
                else if (hasEmbeddedTitle)
                {
                    windowStringBuilder.Append(" ");
                    windowStringBuilder.Append((_elements.Count == 1) switch
                    {
                        true => _isFocused switch
                        {
                            false => _available
                                ? StyleUtility.StringBold(windowLabel.ToUpper())
                                : StyleUtility.StringColored(StyleUtility.StringBold(windowLabel.ToUpper()),
                                    StyleUtility.disabled),
                            true => StyleUtility.StringColored(StyleUtility.StringBold(windowLabel.ToUpper()),
                                _available ? StyleUtility.selected : StyleUtility.disableSelected),
                        },
                        false => StyleUtility.StringBold(windowLabel.ToUpper()),
                    });
                    windowStringBuilder.Append(TextUtility.LineBreaker);
                }
                else
                {
                    windowStringBuilder.Append(TextUtility.LineBreaker);
                }

                if (count == 0)
                {
                    TextUI text = AddText("DummyBlankText");
                    text.SetLabel(TextUtility.Repeat(' ', 10));
                    AutoResize();
                }

                for (int i = 0; i < count; i++)
                {
                    string[] texts = _elements[i].GetSplitDisplayText(contentWidth);
                    for (int k = 0; k < texts.Length; k++)
                    {
                        string text = texts[k];
                        if (TextUtility.WidthSensitiveLength(text) > contentWidth)
                        {
                            List<string> splitString =
                                TextUtility.StringCutter(text, contentWidth);
                            for (int j = 0; j < splitString.Count; j++)
                            {
                                windowStringBuilder.Append(TextUtility.FULL_WIDTH_SPACE);
                                if (j == 0 && k == 0)
                                    _elements[i].SetFirstCharacterIndex(
                                        TextUtility.RichTagsStrippedLength(windowStringBuilder));
                                windowStringBuilder.Append(splitString[j]);
                                if (j == splitString.Count - 1 && k == texts.Length - 1)
                                    _elements[i].SetLastCharacterIndex(
                                        TextUtility.RichTagsStrippedLength(windowStringBuilder) - 1);
                                windowStringBuilder.Append(TextUtility.LINE_BREAK);
                            }
                        }
                        else
                        {
                            windowStringBuilder.Append(TextUtility.FULL_WIDTH_SPACE);
                            if (k == 0)
                                _elements[i].SetFirstCharacterIndex(
                                    TextUtility.RichTagsStrippedLength(windowStringBuilder));
                            windowStringBuilder.Append(text);
                            if (k == texts.Length - 1)
                                _elements[i].SetLastCharacterIndex(
                                    TextUtility.RichTagsStrippedLength(windowStringBuilder) - 1);
                            windowStringBuilder.Append(TextUtility.LINE_BREAK);
                        }
                    }
                }

                int compensate = 0;
                if (!hasTitle)
                    compensate = -1;
                for (int i = 0; i < _endFillCount; i++)
                {
                    if (windowSubscript != string.Empty && i == _endFillCount - 1 + compensate && !isFullFrame)
                    {
                        windowStringBuilder.Append(TextUtility.FULL_WIDTH_SPACE);
                        windowStringBuilder.Append(TextUtility.PlaceHolder(contentWidth +
                                                                           2 -
                                                                           TextUtility.WidthSensitiveLength(
                                                                               windowSubscript)));
                        if (_isFocused)
                            windowStringBuilder.Append(StyleUtility.StringColored(windowSubscript,
                                _available ? StyleUtility.selected : StyleUtility.disableSelected));
                        else
                            windowStringBuilder.Append(_available
                                ? windowSubscript
                                : StyleUtility.StringColored(windowSubscript, StyleUtility.disabled));
                        windowStringBuilder.Append(TextUtility.LineBreaker);
                    }
                    else if (windowSubscript != string.Empty && i == _endFillCount - 1 + compensate &&
                             isFullFrame)
                    {
                        windowStringBuilder.Append(TextUtility.FULL_WIDTH_SPACE);
                        windowStringBuilder.Append(TextUtility.PlaceHolder(contentWidth +
                                                                           2 -
                                                                           TextUtility.WidthSensitiveLength(
                                                                               windowSubscript)));
                        if (_isFocused)
                            windowStringBuilder.Append(StyleUtility.StringColored(windowSubscript,
                                _available ? StyleUtility.selected : StyleUtility.disableSelected));
                        else
                            windowStringBuilder.Append(_available
                                ? windowSubscript
                                : StyleUtility.StringColored(windowSubscript, StyleUtility.disabled));
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

        public void Initialize(string elementName, string parent, WindowSetup windowSetup)
        {
            _windowName = elementName;
            _windowTag = ZString.Concat(parent, ".", elementName);
            _windowLabel = new StringLabel(_windowName);
            _setup = windowSetup;
            _drawText.fontSize = _setup.fontSize;
            _outlineBuilder.outline.fontSize = _setup.fontSize;
            _windowMask.mask.fontSize = _setup.fontSize;
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
            _layout.minWidth = setup.fontSize * 0.635f * widthCount;
            _layout.minHeight = setup.fontSize * 1.05f * heightCount;
            _layoutAlignment.UpdateLayout();
            _transform.sizeDelta = new Vector2(_layout.minWidth, _layout.minHeight);
        }

        /// <summary>
        /// To automatically fit UI according to content size
        /// </summary>
        /// <param name="extraWidth">Extra amount of width to preserve.</param>
        public void AutoResize(int extraWidth = 0, bool sizeFixed = false)
        {
            _extraWidth = extraWidth;
            int targetHeight = GetAutoResizeHeight();
            int targetWidth = GetAutoResizeWidth(extraWidth);
            if (_setup.width == targetWidth && _setup.height == targetHeight)
                return;
            if (sizeFixed)
            {
                _designatedHeight = targetHeight;
                _designatedWidth = targetWidth;
            }

            _endFillCount = GetEndFillCount();
            _setup.SetWidth(targetWidth);
            _setup.SetHeight(targetHeight);
            SetupMask(targetWidth, targetHeight + 2, _setup);
            if (hasOutline)
            {
                int subscriptLength = TextUtility.WidthSensitiveLength(windowSubscript);
                SetupOutline(targetWidth + OUTLINE_PADDING, targetHeight, _setup, titlePreserveLength + 2,
                    subscriptLength);
                SetLayout(targetWidth + OUTLINE_PADDING, targetHeight);
            }
            else
            {
                SetLayout(targetWidth, targetHeight);
            }

            _positionCached = false;
        }

        public int GetAutoResizeWidth(int extraWidth)
        {
            int count = _elements.Count;
            int minimumWidth = 0;
            if (hasTitleBar)
                minimumWidth = titlePreserveLength + 2;
            if (hasEmbeddedTitle)
                minimumWidth = titlePreserveLength;
            if (windowSubscript != string.Empty)
                minimumWidth = Mathf.Max(minimumWidth, TextUtility.WidthSensitiveLength(windowSubscript) + 1);
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
                string text = _elements[i].displayText;
                if (!text.Contains('\n'))
                    minimumHeight += 1;
                else
                    minimumHeight += text.Split('\n').Length;
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
        public void Resize(int width, bool sizeFixed = false)
        {
            _extraWidth = 0;
            int minimumHeight = 0;
            int count = _elements.Count;
            for (int i = 0; i < count; i++)
            {
                minimumHeight += _elements[i].GetSplitDisplayTextTotalHeight(width);
            }

            _endFillCount = 2;
            if (hasTitleBar)
                minimumHeight += 3;
            else if (hasEmbeddedTitle)
                _endFillCount = 1;
            int targetWidth = width + 4;
            int targetHeight = minimumHeight + 2;
            if (_setup.width == targetWidth && _setup.height == targetHeight)
                return;
            _designatedWidth = width;
            if (sizeFixed)
                _designatedHeight = targetHeight;
            _setup.SetWidth(targetWidth);
            _setup.SetHeight(targetHeight);
            SetupMask(targetWidth, targetHeight + 2, _setup);
            if (hasOutline)
            {
                int subscriptLength = TextUtility.WidthSensitiveLength(windowSubscript);
                SetupOutline(targetWidth + OUTLINE_PADDING, targetHeight, _setup, titlePreserveLength + 2,
                    subscriptLength);
                SetLayout(targetWidth + OUTLINE_PADDING, targetHeight);
            }
            else
            {
                SetLayout(targetWidth, targetHeight);
            }

            _positionCached = false;
        }

        /// <summary>
        /// To resize UI with specified width and height.
        /// </summary>
        public void Resize(int width, int height)
        {
            _extraWidth = 0;
            _endFillCount = 2;
            if (hasEmbeddedTitle)
                _endFillCount = 1;
            int targetWidth = width + 4;
            int targetHeight = height + 2;
            if (_setup.width == targetWidth && _setup.height == targetHeight)
                return;
            _designatedWidth = targetWidth;
            _designatedHeight = targetHeight;
            _setup.SetWidth(targetWidth);
            _setup.SetHeight(targetHeight);
            SetupMask(targetWidth, targetHeight + 2, _setup);
            if (hasOutline)
            {
                int subscriptLength = TextUtility.WidthSensitiveLength(windowSubscript);
                SetupOutline(targetWidth + OUTLINE_PADDING, targetHeight, _setup, titlePreserveLength + 2,
                    subscriptLength);
                SetLayout(targetWidth + OUTLINE_PADDING, targetHeight);
            }
            else
            {
                SetLayout(targetWidth, targetHeight);
            }

            _positionCached = false;
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
            _layoutAlignment.UnregisterWindow(this);
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

        public void AddElement(WindowElement element)
        {
            _elements.Add(element);
            if (element is not TextUI)
                _interactables.Add(element);
        }

        public void RemoveElement(WindowElement element)
        {
            _elements.Remove(element);
            if (_interactables.Contains(element))
                _interactables.Remove(element);
        }

        public TextUI AddText(string elementName)
        {
            TextUI text = new(elementName, this);
            AddElement(text);
            return text;
        }

        public void AddGap()
        {
            AddText("Blank")
                .SetLabel("　");
        }

        public ButtonUI AddButton(string elementName, Action action = null)
        {
            ButtonUI button = new(elementName, this);
            button.SetAction(action);
            AddElement(button);
            return button;
        }

        public ScrollableTextUI AddScrollableText(string elementName, Action action = null)
        {
            ScrollableTextUI button = new(elementName, this);
            button.SetAction(action);
            AddElement(button);
            return button;
        }

        public ToggleUI AddToggle(string elementName, Action<bool> action = null)
        {
            ToggleUI toggle = new(elementName, this);
            toggle.SetAction(action);
            AddElement(toggle);
            return toggle;
        }

        public SliderUI<T> AddSlider<T>(string elementName, Action<T> action = null)
        {
            SliderUI<T> slider = new SliderUI<T>(elementName, this);
            slider.SetAction(action);
            AddElement(slider);
            return slider;
        }

        public QuickSelectionUI<T> AddQuickSelectionUI<T>(string elementName, Action<T> action = null)
        {
            QuickSelectionUI<T> selection = new(elementName, this);
            selection.SetAction(action);
            AddElement(selection);
            return selection;
        }

        public SelectionUI<T> AddSingleSelection<T>(string elementName, Action<T> action = null)
        {
            SelectionUI<T> selection = new SelectionUI<T>(elementName, this);
            selection.SetAction(action);
            AddElement(selection);
            return selection;
        }

        public TextInputUI AddTextInput(string elementName, Action<string> action = null)
        {
            TextInputUI textInput = new(elementName, this);
            textInput.SetAction(action);
            AddElement(textInput);
            return textInput;
        }

        public void SetActive(bool v, bool showMaskAnimation = true, bool syncGameObject = true)
        {
            if (_active == v)
                return;
            _active = v;
            if (_active)
            {
                RefreshSize();
                if (syncGameObject)
                    SetGameObjectActive(true);
                _awaitDeactivate = false;
                ResetAllWindowElement();
                _outlineBuilder.SetActive(true);
                if (showMaskAnimation)
                    _windowMask.FadeIn();
                _background.SetActive(true);
                _drawText.gameObject.SetActive(true);
                SyncActiveValue();
                InvokeUpdate();
            }
            else
            {
                _outlineBuilder.SetActive(false);
                if (showMaskAnimation)
                    _windowMask.FadeOut();
                _awaitDeactivate = true;
                ResetAllWindowElement();
                _background.SetActive(false);
                // Cancel out the next update
                _isDirty = false;
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

        public void SetGameObjectActive(bool v)
        {
            gameObject.SetActive(v);
        }

        public void SetLabel(string label)
        {
            _windowLabelCache = null;
            _windowLabel = new StringLabel(label);
            _isDirty = true;
        }

        public void SetLabel(IStringLabel label)
        {
            _windowLabelCache = null;
            _windowLabel = label;
            _isDirty = true;
        }

        public void SetSubscript(string subscript)
        {
            _windowSubscriptCache = null;
            _windowSubscript = new StringLabel(subscript);
            _isDirty = true;
        }

        public void SetSubscript(IStringLabel label)
        {
            _windowSubscriptCache = null;
            _windowSubscript = label;
            _isDirty = true;
        }

        public void RefreshSize()
        {
            if (_designatedWidth == 0)
                AutoResize(_extraWidth);
            else if (_designatedHeight == 0)
                Resize(_designatedWidth);
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

        internal void SetMaskColor(Color color)
        {
            _windowMask.SetColor(color);
        }

        public void SetInput(bool inInput)
        {
            if (_inInput == inInput)
                return;
            _inInput = inInput;
            _outlineBuilder.SetFocusAndAvailable(isSingleButtonWindow, _isFocused, _available, _inInput);
            _isDirty = true;
        }

        internal void SetFocus(bool inFocus)
        {
            if (inFocus == _isFocused)
                return;
            _isFocused = inFocus;
            _outlineBuilder.SetFocusAndAvailable(isSingleButtonWindow, _isFocused, _available, _inInput);
            _isDirty = true;
        }

        internal void SetAvailable(bool available)
        {
            if (_available == available)
                return;
            _available = available;
            _outlineBuilder.SetFocusAndAvailable(isSingleButtonWindow, _isFocused, _available, _inInput);
            _isDirty = true;
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

        public void MoveTo(Vector2 position)
        {
            if (transform.IsChildOf(_layoutAlignment.transform))
                transform.SetParent(_layoutAlignment.transform.parent);
            transform.position = new(position.x, position.y);
        }

        public void RevertAlignment()
        {
            transform.SetParent(_layoutAlignment.transform);
        }

        public IEnumerable<string> ExportLocalizationTag()
        {
            List<string> tags = new List<string>();
            tags.Add(_windowTag);
            foreach (WindowElement element in _elements)
            {
                if (element.rawLabel is LocalizedStringLabel label)
                    tags.Add(label.tag);
                if (element.rawContent is LocalizedStringLabel content)
                    tags.Add(content.tag);
            }

            return tags;
        }

        public void SyncActiveValue()
        {
            foreach (WindowElement element in _elements)
            {
                if (element is IValueSyncTarget valurTarget && valurTarget.needSync)
                    valurTarget.SyncValue();
                if (element is IAvailabilitySyncTarget availabilityTarget && availabilityTarget.needSync)
                    availabilityTarget.SyncAvailability();
            }
        }
    }
}