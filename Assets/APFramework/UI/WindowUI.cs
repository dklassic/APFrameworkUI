using System.Linq;
using System.Collections.Generic;
using UnityEngine.UI;
using UnityEngine;
using System.Text;
using TMPro;


[System.Serializable]
public struct WindowSetup
{
    public int Width;
    public int Height;
    public WindowStyle Style;
    public WindowTransition TransitionIn;
    public WindowTransition TransitionOut;
    public float FontSize;
    public BackgroundStyle Background;
    public WindowSetup(int width = 0,
                       int height = 0,
                       WindowStyle style = WindowStyle.Single,
                       WindowTransition transitionIn = WindowTransition.Full,
                       WindowTransition transitionOut = WindowTransition.Glitch,
                       float fontSize = 30,
                       BackgroundStyle background = BackgroundStyle.None)
    {
        this.Width = width;
        this.Height = height;
        this.Style = style;
        this.TransitionIn = transitionIn;
        this.TransitionOut = transitionOut;
        this.FontSize = fontSize;
        this.Background = background;
    }
    public WindowSetup(WindowSetup setup)
    {
        this.Width = setup.Width;
        this.Height = setup.Height;
        this.Style = setup.Style;
        this.TransitionIn = setup.TransitionIn;
        this.TransitionOut = setup.TransitionOut;
        this.FontSize = setup.FontSize;
        this.Background = setup.Background;
    }
    public void SetSize(int width, int height = 0)
    {
        this.Width = width;
        this.Height = height;
    }
}
public enum WindowElementType
{
    Text,
    Button,
    Toggle,
    Slider
}
public enum WindowStyle
{
    NoOutline = 2,
    Single = 0,
    Double = 1,
    SingleLeftLine = 8,
    DoubleLeftLine = 9,
    SingleCornerOnly = 10,
    DoubleCornerOnly = 11,
    SingleWithTitle = 4,
    DoubleWithTitle = 3,
    NoOutlineWithEmbeddedTitle = 7,
    SingleWithEmbeddedTitle = 6,
    DoubleWithEmbeddedTitle = 5,
    SingleLeftLineWithEmbeddedTitle = 12,
    DoubleLeftLineEmbeddedTitle = 13,
    SingleCornerOnlyWithEmbeddedTitle = 14,
    DoubleCornerOnlyWithEmbeddedTitle = 15,
    LowerLeftCornerOnly = 16,
    LowerLeftCornerOnlyWithEmbeddedTitle = 17,
    ThickenCorner = 18,
    ThickenCornerWithEmbeddedTitle = 19,
    ThickenCornerWithTitle = 20,
}
public enum BackgroundStyle
{
    None = 0,
    FillBlack = 1,
    TransparentBlack = 2,
}
public enum LabelStyle
{
    None = 0,
    Left = 1,
    Right = 2,
}
public class WindowUI : MonoBehaviour
{
    string windowName = string.Empty;
    public string WindowName => windowName;
    string WindowNameLoc
    {
        get
        {
            return windowName;
        }
    }
    int TitlePreserveLength => Mathf.FloorToInt(TextUtility.UnbiasedLength(WindowNameLoc));
    string windowSubscript = string.Empty;
    string WindowSubscriptLoc
    {
        get
        {
            return windowSubscript;
        }
    }
    WindowSetup setup;
    public WindowSetup Setup => setup;
    GeneralUISystem masterUI;
    public GeneralUISystem MasterUI => masterUI;
    int ContentWidth { get { return setup.Width - 4; } }
    [SerializeField] List<WindowElement> elements = new List<WindowElement>();
    public List<WindowElement> Elements { get => elements; }
    public void ClearElementsFocus()
    {
        foreach (WindowElement element in elements)
        {
            if (element is not ButtonUI)
                continue;
            (element as ButtonUI).ClearFocus();
        }
    }
    public void ClearWindowFocus()
    {
        SetFocusAndAvailable(false, true);
    }
    bool singleWindowOverride = false;
    public bool SingleWindowOverride => singleWindowOverride;
    public void SetSingleWindowOverride(bool v)
    {
        singleWindowOverride = v;
    }
    int maxElementLength = 0;
    public int MaxElementLength
    {
        get
        {
            if (maxElementLength == 0)
            {
                for (int i = 0; i < elements.Count; i++)
                {
                    if (elements[i].Flexible && elements.Count > 1)
                        continue;
                    int elementLength = elements[i].GetLength;
                    if (elementLength > maxElementLength)
                        maxElementLength = elementLength;
                }
                if (TitlePreserveLength > maxElementLength)
                    maxElementLength = TitlePreserveLength;
            }
            return maxElementLength;
        }
    }
    [SerializeField] Vector2 cachedPositionStart = Vector2.zero;
    [SerializeField] Vector2 cachedPositionEnd = Vector2.zero;
    public (Vector2, Vector2) CachedPosition => (cachedPositionStart, cachedPositionEnd);
    List<ButtonUI> selectables = new List<ButtonUI>();
    public List<ButtonUI> Selectables { get => selectables; }
    bool active = false;
    Coroutine delayedWindowActionCoroutine = null;
    bool inFocus = false;
    bool available = true;
    float alpha = 1f;
    [SerializeField] TextMeshProUGUI drawText = null;
    public TextMeshProUGUI TextComponent => drawText;
    [SerializeField] WindowOutline outlineBuilder = null;
    [SerializeField] WindowMask windowMask = null;
    [SerializeField] WindowBackground background;
    [SerializeField] LayoutElement layout;
    bool hasTitle => UIManager.Instance.WindowSetting.HasTitle(setup.Style);
    bool hasTitlebar => UIManager.Instance.WindowSetting.HasTitlebar(setup.Style);
    bool hasEmbeddedTitle => UIManager.Instance.WindowSetting.HasEmbeddedTitle(setup.Style);
    bool hasOutline => UIManager.Instance.WindowSetting.HasOutline(setup.Style);
    bool isFullFrame => UIManager.Instance.WindowSetting.IsFullFrame(setup.Style);
    int endFillCount = 5;
    float fontSize = 10f;
    bool maskReady = false;
    bool outlineReady = false;
    bool noCut = false;
    int extraWidth = 0;
    StringBuilder windowStringBuilder = new StringBuilder();
    public (Vector2, Vector2) SelectableBound(int index)
    {
        if (index < 0 || index > selectables.Count || selectables[index].FirstCharacterIndex == -1 || selectables[index].LastCharacterIndex == -1 || selectables[index].CachedPosition == (Vector2.zero, Vector2.zero))
            return (Vector2.zero, Vector2.zero);
        return selectables[index].CachedPosition;
    }
    public void SetOpacity(float alpha, bool animated)
    {
        this.alpha = alpha;
        if (!active)
            return;
        drawText.color = new Color(1, 1, 1, Mathf.Clamp01(alpha));
        outlineBuilder.SetOpacity(alpha);
    }

    public bool UpdateElementPosition(WindowElement element)
    {
        if (element.FirstCharacterIndex >= drawText.textInfo.characterInfo.Length)
        {
            return false;
        }
        (Vector2, Vector2) result = (Vector2.zero, Vector2.zero);
        for (int i = element.FirstCharacterIndex; i <= element.LastCharacterIndex; i++)
        {
            Vector2 rangeBottomLeft = drawText.textInfo.characterInfo[i].bottomLeft;
            Vector2 rangeTopRight = drawText.textInfo.characterInfo[i].topRight;
            if (result.Item1.x > rangeBottomLeft.x || result.Item1.x == 0)
                result.Item1.x = rangeBottomLeft.x;
            if (result.Item1.y > rangeBottomLeft.y || result.Item1.y == 0)
                result.Item1.y = rangeBottomLeft.y;
            if (result.Item2.x < rangeTopRight.x || result.Item2.x == 0)
                result.Item2.x = rangeTopRight.x;
            if (result.Item2.y < rangeTopRight.y || result.Item2.y == 0)
                result.Item2.y = rangeTopRight.y;
        }
        result.Item1 = UIManager.Instance.UIBoundRetriever(transform, result.Item1);
        result.Item2 = UIManager.Instance.UIBoundRetriever(transform, result.Item2);
        element.SetCachedPosition(result);
        if (element is SliderUI uI)
        {
            (Vector2, Vector2) arrowPosition = (Vector2.zero, Vector2.zero);
            Vector2 leftArrowPosition = Vector2.zero;
            leftArrowPosition += UIManager.Instance.UIBoundRetriever(transform, drawText.textInfo.characterInfo[uI.FirstSliderArrowIndex].bottomLeft);
            leftArrowPosition += UIManager.Instance.UIBoundRetriever(transform, drawText.textInfo.characterInfo[uI.FirstSliderArrowIndex].topRight);
            leftArrowPosition /= 2f;
            arrowPosition.Item1 = leftArrowPosition;
            Vector2 rightArrowPosition = Vector2.zero;
            rightArrowPosition += UIManager.Instance.UIBoundRetriever(transform, drawText.textInfo.characterInfo[uI.LastSliderArrowIndex].bottomLeft);
            rightArrowPosition += UIManager.Instance.UIBoundRetriever(transform, drawText.textInfo.characterInfo[uI.LastSliderArrowIndex].topRight);
            rightArrowPosition /= 2f;
            arrowPosition.Item2 = rightArrowPosition;
            uI.SetCachedArrowPosition(arrowPosition);
        }
        return true;
    }
    public void UpdateWindowPosition()
    {
        if (CachedPosition != (Vector2.zero, Vector2.zero))
            return;
        (Vector2, Vector2) result = (Vector2.zero, Vector2.zero);
        if (hasOutline)
        {
            for (int i = 0; i <= outlineBuilder.Outline.textInfo.characterCount - 1; i++)
            {
                Vector2 rangeBottomLeft = outlineBuilder.Outline.textInfo.characterInfo[i].bottomLeft;
                Vector2 rangeTopRight = outlineBuilder.Outline.textInfo.characterInfo[i].topRight;
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
        for (int i = 0; i <= drawText.textInfo.characterCount - 1; i++)
        {
            Vector2 rangeBottomLeft = drawText.textInfo.characterInfo[i].bottomLeft;
            Vector2 rangeTopRight = drawText.textInfo.characterInfo[i].topRight;
            if (result.Item1.x > rangeBottomLeft.x || result.Item1.x == 0)
                result.Item1.x = rangeBottomLeft.x;
            if (result.Item1.y > rangeBottomLeft.y || result.Item1.y == 0)
                result.Item1.y = rangeBottomLeft.y;
            if (result.Item2.x < rangeTopRight.x || result.Item2.x == 0)
                result.Item2.x = rangeTopRight.x;
            if (result.Item2.y < rangeTopRight.y || result.Item2.y == 0)
                result.Item2.y = rangeTopRight.y;
        }
        result.Item1 = UIManager.Instance.UIBoundRetriever(transform, result.Item1);
        result.Item2 = UIManager.Instance.UIBoundRetriever(transform, result.Item2);

        cachedPositionStart = result.Item1;
        cachedPositionEnd = result.Item2;
    }
    public void UpdateElementsAndWindowPosition()
    {
        if (!active)
            return;
        bool quickOut = false;
        foreach (WindowElement element in elements)
        {
            quickOut = quickOut | !UpdateElementPosition(element);
        }
        if (quickOut)
            return;
        UpdateWindowPosition();
    }
    public void ClearCachedPosition()
    {
        foreach (WindowElement element in elements)
        {
            element.ClearCachedPosition();
            if (element is SliderUI uI)
                uI.ClearCachedArrowPosition();
        }
        cachedPositionStart = Vector2.zero;
        cachedPositionEnd = Vector2.zero;
    }
    void UpdateContent()
    {
        int count = elements.Count;
        if (!active)
            return;
        windowStringBuilder.Clear();
        windowStringBuilder.Append(TitleBuild());
        if (count == 0)
        {
            TextUI text = this.AddText("DummyBlankText");
            text.SetContent(TextUtility.Repeat(' ', 10));
            AutoResize();
        }
        for (int i = 0; i < elements.Count; i++)
        {
            if (elements[i].GetLength > ContentWidth)
            {
                if (noCut)
                {
                    windowStringBuilder.Append(TextUtility.FullsizeSpace);
                    elements[i].FirstCharacterIndex = TextUtility.StripRichTagsFromStr(windowStringBuilder.ToString()).Length;
                    windowStringBuilder.Append(elements[i].ToDisplay);
                    elements[i].LastCharacterIndex = TextUtility.StripRichTagsFromStr(windowStringBuilder.ToString()).Length - 1;
                    windowStringBuilder.Append(TextUtility.LineBreaker);
                }
                else
                {
                    List<string> cuttedString = TextUtility.StringCutter(elements[i].ToDisplay, ContentWidth);
                    for (int j = 0; j < cuttedString.Count; j++)
                    {
                        windowStringBuilder.Append(TextUtility.FullsizeSpace);
                        if (j == 0)
                            elements[i].FirstCharacterIndex = TextUtility.StripRichTagsFromStr(windowStringBuilder.ToString()).Length;
                        windowStringBuilder.Append(cuttedString[j]);
                        if (j == cuttedString.Count - 1)
                            elements[i].LastCharacterIndex = TextUtility.StripRichTagsFromStr(windowStringBuilder.ToString()).Length - 1;
                        windowStringBuilder.Append(TextUtility.LineBreaker);
                    }
                }
            }
            else
            {
                windowStringBuilder.Append(TextUtility.FullsizeSpace);
                elements[i].FirstCharacterIndex = TextUtility.StripRichTagsFromStr(windowStringBuilder.ToString()).Length;
                windowStringBuilder.Append(elements[i].ToDisplay);
                elements[i].LastCharacterIndex = TextUtility.StripRichTagsFromStr(windowStringBuilder.ToString()).Length - 1;
                windowStringBuilder.Append(TextUtility.LineBreaker);
            }
        }
        if (count == 0)
        {
            elements.Clear();
        }
        int compensate = 0;
        if (!hasTitle)
            compensate = -1;
        for (int i = 0; i < endFillCount; i++)
        {
            if (windowSubscript != string.Empty && i == endFillCount - 1 + compensate && !isFullFrame)
            {
                windowStringBuilder.Append(TextUtility.FullsizeSpace);
                windowStringBuilder.Append(TextUtility.PlaceHolder(ContentWidth + TextUtility.SubscriptCompensation(WindowSubscriptLoc) - TextUtility.ActualLength(WindowSubscriptLoc)));
                if (inFocus)
                    windowStringBuilder.Append(StyleUtility.StringColored(WindowSubscriptLoc, available ? StyleUtility.Selected : StyleUtility.DisableSelected));
                else
                    windowStringBuilder.Append(available ? WindowSubscriptLoc : StyleUtility.StringColored(WindowSubscriptLoc, StyleUtility.Disabled));
                windowStringBuilder.Append(TextUtility.LineBreaker);
            }
            else if (windowSubscript != string.Empty && i == endFillCount - 1 + compensate && isFullFrame)
            {
                windowStringBuilder.Append(TextUtility.FullsizeSpace);
                windowStringBuilder.Append(TextUtility.PlaceHolder(ContentWidth + TextUtility.SubscriptCompensation(WindowSubscriptLoc) - TextUtility.ActualLength(WindowSubscriptLoc)));
                if (inFocus)
                    windowStringBuilder.Append(StyleUtility.StringColored(WindowSubscriptLoc, available ? StyleUtility.Selected : StyleUtility.DisableSelected));
                else
                    windowStringBuilder.Append(available ? WindowSubscriptLoc : StyleUtility.StringColored(WindowSubscriptLoc, StyleUtility.Disabled));
                windowStringBuilder.Append(TextUtility.LineBreaker);
            }
            else if (i == endFillCount - 2 + compensate && !isFullFrame)
                windowStringBuilder.Append(TextUtility.LineBreaker);
            else
                windowStringBuilder.Append(TextUtility.LineBreaker);
        }
        drawText.SetText(windowStringBuilder.ToString());
    }
    public void ClearElements()
    {
        elements.Clear();
        selectables.Clear();
    }
    string TitleBuild()
    {
        if (!hasTitle)
            return TextUtility.LineBreaker + TextUtility.LineBreaker;
        else if (hasTitlebar)
            return TextUtility.TitleOpener + (elements.Count == 1) switch
            {
                true => inFocus switch
                {
                    false => available ? StyleUtility.StringBold(WindowNameLoc.ToUpper()) : StyleUtility.StringColored(StyleUtility.StringBold(WindowNameLoc.ToUpper()), StyleUtility.Disabled),
                    true => StyleUtility.StringColored(StyleUtility.StringBold(WindowNameLoc.ToUpper()), available ? StyleUtility.Selected : StyleUtility.DisableSelected),
                },
                false => StyleUtility.StringBold(WindowNameLoc.ToUpper()),
            } + TextUtility.LineBreaker + TextUtility.LineBreaker;
        else if (hasEmbeddedTitle)
            return TextUtility.Repeat(' ', TextUtility.TitleCompensation) + (elements.Count == 1) switch
            {
                true => inFocus switch
                {
                    false => available ? StyleUtility.StringBold(WindowNameLoc.ToUpper()) : StyleUtility.StringColored(StyleUtility.StringBold(WindowNameLoc.ToUpper()), StyleUtility.Disabled),
                    true => StyleUtility.StringColored(StyleUtility.StringBold(WindowNameLoc.ToUpper()), available ? StyleUtility.Selected : StyleUtility.DisableSelected),
                },
                false => StyleUtility.StringBold(WindowNameLoc.ToUpper()),
            } + TextUtility.LineBreaker;
        else
            return TextUtility.LineBreaker;
    }
    public void Initialize(string name, WindowSetup setup, GeneralUISystem masterUI)
    {
        this.windowName = name;
        this.setup = setup;
        this.masterUI = masterUI;
        SetFont(setup.FontSize);
        SetActive(false);
        if (setup.Width != 0 && setup.Height != 0)
            Resize(setup.Width, setup.Height);
        else if (setup.Width != 0 && setup.Height == 0)
            Resize(setup.Width);
        background.SetType(setup.Background);
    }
    public void SetBackgroundColor(Color color)
    {
        background.SetColor(color, active);
    }
    public void ChangeSetup(WindowSetup setup)
    {
        this.setup = setup;
    }
    void SetLayout(int widthCount, int heightCount)
    {
        // Here's a series of magic numbers that are used to calculate the minimum width and height of the window for this specific font setup.

        // layout.minWidth = fontSize * .62f * widthCount * (hasOutline ? 1.1f : 1f);
        layout.minWidth = fontSize * Mathf.Max(0.65f * widthCount, 3);

        // layout.minHeight = fontSize * .67f * heightCount * 1.6f;
        layout.minHeight = fontSize * 1.05f * heightCount;
    }

    public void SetFont(float fontSize)
    {
        this.fontSize = fontSize;
        drawText.fontSize = fontSize;
        outlineBuilder.Outline.fontSize = fontSize;
        windowMask.Mask.fontSize = fontSize;
    }
    /// <summary>
    /// To automatically fit UI according to content size
    /// </summary>
    /// <param name="extraWidth">Extra amount of width to preserve.</param>
    public void AutoResize(int extraWidth = 0)
    {
        this.extraWidth = extraWidth;
        int targetHeight = GetAutoResizeHeight();
        int targetWidth = GetAutoResizeWidth(extraWidth);
        endFillCount = GetEndFillCount();
        if (hasOutline)
        {
            int subscriptLength = TextUtility.UnbiasedLength(WindowSubscriptLoc) == 0 ? 0 : TextUtility.UnbiasedLength(WindowSubscriptLoc) + (2 - TextUtility.SubscriptCompensation(WindowSubscriptLoc));
            SetupOutline(targetWidth + 1, targetHeight, setup.Style, TitlePreserveLength + 2, subscriptLength);
        }
        setup.Width = targetWidth;
        setup.Height = targetHeight;
        SetupMask(targetWidth, targetHeight + 2, setup);
        SetLayout(targetWidth, targetHeight);
        noCut = true;
    }
    public int GetAutoResizeWidth(int extraWidth)
    {
        int count = elements.Count;
        int minimumWidth = 0;
        if (hasTitlebar)
            minimumWidth = TitlePreserveLength + 2;
        if (hasEmbeddedTitle)
            minimumWidth = TitlePreserveLength;
        if (windowSubscript != string.Empty)
            minimumWidth = Mathf.Max(minimumWidth, TextUtility.UnbiasedLength(WindowSubscriptLoc) + 1);
        for (int i = 0; i < count; i++)
        {
            if (!elements[i].Flexible || count == 1)
            {
                int elementLength = elements[i].GetLength;
                if (elementLength > minimumWidth)
                {
                    minimumWidth = elementLength;
                }
            }
        }
        int targetWidth = minimumWidth + 4 + extraWidth;
        return targetWidth;
    }
    public int GetAutoResizeHeight()
    {
        int count = elements.Count;
        int minimumHeight = 0;
        for (int i = 0; i < count; i++)
        {
            int splits = elements[i].ToDisplay.Split("\n").Length;
            minimumHeight += splits;
        }
        if (hasTitlebar)
            minimumHeight += 1;
        int targetHeight = minimumHeight + 2;
        return targetHeight;
    }
    public int GetEndFillCount()
    {
        if (hasEmbeddedTitle)
            return 1;
        else
            return 2;
    }
    /// <summary>
    /// To resize UI with specified width. Height will be automatically adjusted.
    /// </summary>
    public void Resize(int width)
    {
        extraWidth = 0;
        int minimumHeight = 0;
        int count = elements.Count;
        for (int i = 0; i < count; i++)
        {
            minimumHeight += TextUtility.StringCutter(elements[i].ToDisplay, width).Count;
        }
        endFillCount = 2;
        if (hasTitlebar)
            minimumHeight += 3;
        else if (hasEmbeddedTitle)
            endFillCount = 1;
        int targetWidth = width + 4;
        int targetHeight = minimumHeight + 2;
        if (hasOutline)
        {
            int subscriptLength = TextUtility.ActualLength(WindowSubscriptLoc) == 0 ? 0 : TextUtility.ActualLength(WindowSubscriptLoc) + (2 - TextUtility.SubscriptCompensation(WindowSubscriptLoc));
            SetupOutline(targetWidth + 1, targetHeight, setup.Style, TitlePreserveLength + 2, subscriptLength);
        }
        setup.Width = targetWidth;
        setup.Height = targetHeight;
        SetupMask(targetWidth, targetHeight + 2, setup);
        SetLayout(targetWidth, targetHeight);
        noCut = false;
    }
    /// <summary>
    /// To resize UI with specified width and height.
    /// </summary>
    public void Resize(int width, int height)
    {
        extraWidth = 0;
        endFillCount = 2;
        if (hasEmbeddedTitle)
            endFillCount = 1;
        int targetWidth = width + 4;
        int targetHeight = height + 2;
        if (hasOutline)
        {
            int subscriptLength = TextUtility.ActualLength(WindowSubscriptLoc) == 0 ? 0 : TextUtility.ActualLength(WindowSubscriptLoc) + (2 - TextUtility.SubscriptCompensation(WindowSubscriptLoc));
            SetupOutline(targetWidth + 1, targetHeight, setup.Style, TitlePreserveLength + 2, subscriptLength);
        }
        setup.Width = targetWidth;
        setup.Height = targetHeight;
        SetupMask(targetWidth, targetHeight + 2, setup);
        SetLayout(targetWidth, targetHeight);
        noCut = false;
    }
    void SetupOutline(int width, int height, WindowStyle style, int titleOverride, int subLength)
    {
        outlineBuilder.SetOutline(width, height, style, titleOverride, subLength);
        outlineReady = true;
    }
    void SetupMask(int width, int height, WindowSetup setup)
    {
        windowMask.Setup(width, height, setup);
        maskReady = true;
    }
    public void Close()
    {
        // Do fancy animation
        UIManager.Instance.DelistWindow(this);
    }
    public void MoveElement(WindowElement element, int targetIndex)
    {
        elements.Remove(element);
        elements.Insert(targetIndex, element);
        if (element is ButtonUI uI)
        {
            UpdateSelectables();
        }
    }
    public void UpdateSelectables()
    {
        selectables.Clear();
        elements.Where(x => x is ButtonUI).ToList().ForEach(x => selectables.Add(x as ButtonUI));
    }
    public void RemoveElement(WindowElement element)
    {
        elements.Remove(element);
        if (element is ButtonUI uI)
            selectables.Remove(uI);
    }
    public TextUI AddText(string name)
    {
        TextUI text = new TextUI(name);
        text.SetParent(this);
        elements.Add(text);
        return text;
    }
    public ButtonUI AddButton(string name, System.Action action = null)
    {
        ButtonUI button = new ButtonUI(name);
        button.SetParent(this);
        button.SetAction(action);
        elements.Add(button);
        selectables.Add(button);
        return button;
    }
    public ButtonUIDoubleConfirm AddDoubleConfirmButton(string name, System.Action action = null, System.Action selectAction = null)
    {
        ButtonUIDoubleConfirm button = new ButtonUIDoubleConfirm(name);
        button.SetParent(this);
        button.SetAction(action);
        button.SetSelectAction(selectAction);
        elements.Add(button);
        selectables.Add(button);
        return button;
    }
    public ToggleUI AddToggle(string name, System.Action<bool> action = null)
    {
        ToggleUI toggle = new ToggleUI(name);
        toggle.SetAction(action);
        toggle.SetParent(this);
        elements.Add(toggle);
        selectables.Add(toggle);
        return toggle;
    }
    public ToggleUIExclusive AddExclusiveToggle(string name, System.Action<bool> action = null)
    {
        ToggleUIExclusive toggle = new ToggleUIExclusive(name);
        toggle.SetAction(action);
        toggle.SetParent(this);
        elements.Add(toggle);
        selectables.Add(toggle);
        return toggle;
    }
    public SliderUI AddSlider(string name, System.Action<int> action = null)
    {
        SliderUI slider = new SliderUI(name);
        slider.SetParent(this);
        slider.SetAction(action);
        elements.Add(slider);
        selectables.Add(slider);
        return slider;
    }

    public SliderUIChoice AddSliderWithChoice(string name, System.Action<int> action = null)
    {
        SliderUIChoice slider = new SliderUIChoice(name);
        slider.SetAction(action);
        slider.SetParent(this);
        elements.Add(slider);
        selectables.Add(slider);
        return slider;
    }
    public ButtonUICountable AddButtonWithCount(string name, System.Action<int> action = null)
    {
        ButtonUICountable button = new ButtonUICountable(name);
        button.SetAction(action);
        button.SetParent(this);
        elements.Add(button);
        selectables.Add(button);
        return button;
    }

    internal void SetActive(bool v, bool syncGameObject = true)
    {
        if (hasOutline && !outlineReady || !maskReady)
            return;
        if (active == v)
            return;
        if (delayedWindowActionCoroutine != null)
        {
            StopCoroutine(delayedWindowActionCoroutine);
            delayedWindowActionCoroutine = null;
        }
        active = v;
        if (syncGameObject && v)
            gameObject.SetActive(true);
        if (active)
        {
            // SetOpacity(alpha, false);
            float initializeTime = outlineBuilder.SetActive(true);
            windowMask.FadeIn();
            background.SetActive(true);
            delayedWindowActionCoroutine = StartCoroutine(CoroutineUtility.WaitThenExecuteRealtime(initializeTime, InvokeUpdate));
        }
        else
        {
            outlineBuilder.SetActive(false);
            float fadeOutDelay = windowMask.FadeOut();
            delayedWindowActionCoroutine = StartCoroutine(CoroutineUtility.WaitThenExecuteRealtime(fadeOutDelay, DeactivateGameObject));
            background.SetActive(false);
            drawText.SetText(string.Empty);
        }
    }
    internal void SetActiveNoVFX(bool v, bool syncGameObject = true)
    {
        if (hasOutline && !outlineReady || !maskReady)
            return;
        if (active == v)
            return;
        if (delayedWindowActionCoroutine != null)
        {
            StopCoroutine(delayedWindowActionCoroutine);
            delayedWindowActionCoroutine = null;
        }
        active = v;
        if (syncGameObject)
            gameObject.SetActive(v);
        if (active)
        {
            // SetOpacity(alpha, false);
            float initializeTime = outlineBuilder.SetActive(true);
            background.SetActive(true);
            delayedWindowActionCoroutine = StartCoroutine(CoroutineUtility.WaitThenExecuteRealtime(initializeTime, InvokeUpdate));
        }
        else
        {
            outlineBuilder.SetActive(false);
            background.SetActive(false);
            drawText.SetText(string.Empty);
        }
    }
    public void SetGameObjectActive(bool v)
    {
        gameObject.SetActive(v);
    }
    void DeactivateGameObject()
    {
        SetGameObjectActive(false);
        delayedWindowActionCoroutine = null;
    }
    public void SetName(string name)
    {
        this.windowName = name;
    }
    public void SetSubscript(string name)
    {
        this.windowSubscript = name;
        UpdateContent();
    }
    public void RefreshSize()
    {
        if (!noCut)
            return;
        AutoResize(extraWidth);
    }
    public void InvokeUpdate()
    {
        if (!active)
            return;
        UpdateContent();
    }
    public void InvokeValueUpdate()
    {
        if (!active)
            return;
        masterUI.TriggerValueUpdate();
    }
    public void TriggerGlitch() { if (maskReady) windowMask.TriggerGlitch(); }
    public void TriggerEffect(WindowTransition transitionSetup) { if (maskReady) windowMask.TriggerEffect(transitionSetup); }
    public void TriggerDamageGlitch() { if (maskReady) windowMask.TriggerDamageGlitch(); }
    internal void SetMaskColor(ColorCode code)
    {
        windowMask.SetColor(code);
    }

    internal void SetFocusAndAvailable(bool f, bool a)
    {
        inFocus = f;
        available = a;
        outlineBuilder.SetFocusAndAvailable(f, a);
        UpdateContent();
    }

    internal void CancelOthers(ButtonUIDoubleConfirm confirmedUI)
    {
        foreach (WindowElement element in elements)
        {
            if (element is not ButtonUIDoubleConfirm)
                continue;
            if (element == confirmedUI)
                continue;
            ((ButtonUIDoubleConfirm)element).SetConfirm(false);
        }
    }

    internal void ToggleOffOthers(ToggleUIExclusive exclusiveToggleUI)
    {
        foreach (WindowElement element in elements)
        {
            if (element is not ToggleUIExclusive)
                continue;
            if (element == exclusiveToggleUI)
                continue;
            ((ToggleUIExclusive)element).Set = false;
        }
    }
}

