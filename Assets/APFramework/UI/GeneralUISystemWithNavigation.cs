using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class GeneralUISystemWithNavigation : GeneralUISystem
{
    public enum NavDirection
    {
        X,
        Y,
        TwoWay
    }
    public enum SlideDirection
    {
        X,
        Y
    }
    [SerializeField] protected NavDirection navDirection = NavDirection.Y;
    [SerializeField] protected SlideDirection slideDirection = SlideDirection.X;
    [SerializeField] protected bool allowCycle = false;
    [SerializeField] protected bool cancelOutAllowed = true;
    [SerializeField] protected bool resetOnOpen = false;
    [Tooltip("A hard delay to prevent accident input")]
    [SerializeField] protected float inputDelay = .2f;
    [SerializeField] protected bool inFocus = false;
    [SerializeField] protected bool inputAllowed = true;
    [SerializeField] protected List<GeneralUISystemWithNavigation> subMenus = new List<GeneralUISystemWithNavigation>();
    public GeneralUISystemWithNavigation SubMenu(int i) => subMenus[i];
    protected bool singlePressOnly = false;
    protected float holdStart = Mathf.Infinity;
    protected float holdNavigationDelay = 0.5f;
    protected float holdNavigationNext = Mathf.Infinity;
    protected float holdNavigationInterval = 0.2f;
    protected float holdNavigationSpeedUpInterval = 2f;
    protected List<ButtonUI> SelectableList(int i = 0) => instanceWindows[i].Selectables;
    protected virtual ButtonUI Selectable(int i) => instanceWindows[0].Selectables[i];
    protected bool ExistsSelectable(int i) => instanceWindows[0].Selectables.Count > i;
    protected ButtonUI Selectable(int i, int j) => instanceWindows[i].Selectables[j];
    protected bool ExistsSelectable(int i, int j) => instanceWindows.Count > i ? (instanceWindows[i].Selectables.Count > j ? true : false) : false;
    protected ButtonUI Selectable(Vector2Int select) => (select.x >= 0 && select.x < instanceWindows.Count && select.y >= 0 && select.y < instanceWindows[select.x].Selectables.Count) ? instanceWindows[select.x].Selectables[select.y] : null;
    protected List<List<int>> selection = new List<List<int>>();
    protected GeneralUISystemWithNavigation parentMenu;
    public GeneralUISystemWithNavigation ParentMenu => parentMenu;
    public GeneralUISystemWithNavigation RootParentMenu
    {
        get
        {
            GeneralUISystemWithNavigation menu = parentMenu;
            if (menu == null)
                return null;
            while (menu.ParentMenu != null)
                menu = menu.ParentMenu;
            return menu;
        }
    }
    [Header("Debug View")]
    [SerializeField] protected bool windowElementLocationCached = false;
    [SerializeField] protected float updateTime = Mathf.Infinity;
    [SerializeField] protected Vector2Int currentSelection = Vector2Int.zero;
    protected virtual ButtonUI CurrentSelectable
    {
        get
        {
            if (Selectable(currentSelection) == null)
            {
                for (int i = 0; i < instanceWindows.Count; i++)
                {
                    if (instanceWindows[i].Selectables.Count == 0)
                        continue;
                    currentSelection = new Vector2Int(i, 0);
                }
            }
            if (Selectable(currentSelection) == null)
                return null;
            return Selectable(currentSelection);
        }
    }
    [SerializeField] protected Vector2 move = Vector2.zero;
    [SerializeField] protected Vector2 mouseScroll = Vector2.zero;
    [SerializeField] protected bool inputMode = false;
    [SerializeField] protected bool disabled = false;
    protected float allowOpenTime = Mathf.NegativeInfinity;
    protected bool mouseActive = false;
    protected bool hoverOnLeft = false;
    protected bool hoverOnRight = false;
    protected bool selectionUpdated = false;
    void Update()
    {
        if (!inFocus)
            return;
        PreemptAction();
        MouseAction();
        if (Time.unscaledTime < updateTime)
            return;
        active = true;
        UpdateWindowLocation();
        if (holdStart == Mathf.Infinity || holdNavigationNext <= Time.unscaledTime)
        {
            UpdateSelection();
        }
        ResetDirection();
        if (valueDirty)
        {
            valueDirty = false;
            ValueUpdate();
        }
        if (selectionUpdated)
        {
            selectionUpdated = false;
            OnSelectionUpdate();
        }
        ConstantUpdate();
    }
    protected virtual void MouseAction()
    {
        if (!active || !Cursor.visible || Mouse.current == null || instanceWindows.Count == 0 || !inputAllowed)
            return;
        if (!inputMode)
        {
            if (instanceWindows.Count == 1)
            {
                mouseActive = false;
                for (int i = 0; i < instanceWindows[0].Selectables.Count; i++)
                {
                    (Vector2, Vector2) result = instanceWindows[0].SelectableBound(i);
                    if (result.Item1 == Vector2.zero && result.Item2 == Vector2.zero)
                        continue;
                    Vector2 bottomLeftDelta = Mouse.current.position.ReadValue() - result.Item1;
                    if (bottomLeftDelta.x <= 0 || bottomLeftDelta.y <= 0)
                        continue;
                    Vector2 TopRightDelta = Mouse.current.position.ReadValue() - result.Item2;
                    if (TopRightDelta.x >= 0 || TopRightDelta.y >= 0)
                        continue;
                    if (i != currentSelection[1])
                    {
                        selectionUpdated = true;
                        CurrentSelectable?.SetFocus(false);
                        currentSelection[1] = i;
                        CurrentSelectable?.SetFocus(true);
                    }
                    mouseActive = true;
                    break;
                }
            }
            else
            {
                mouseActive = false;
                for (int i = 0; i < instanceWindows.Count; i++)
                {
                    if (instanceWindows[i].Selectables.Count == 0)
                        continue;
                    (Vector2, Vector2) result = instanceWindows[i].CachedPosition;
                    if (result.Item1 == Vector2.zero && result.Item2 == Vector2.zero)
                        continue;
                    Vector2 TopLeftDelta = Mouse.current.position.ReadValue() - result.Item1;
                    if (TopLeftDelta.x <= 0 || TopLeftDelta.y >= 0)
                        continue;
                    Vector2 BottomRightDelta = Mouse.current.position.ReadValue() - result.Item2;
                    if (BottomRightDelta.x >= 0 || BottomRightDelta.y <= 0)
                        continue;
                    if (i != currentSelection[0])
                    {
                        selectionUpdated = true;
                        CurrentSelectable?.SetFocus(false);
                        currentSelection[0] = i;
                        int count = instanceWindows[i].Selectables.Count;
                        currentSelection[1] = currentSelection[1] >= count ? count - 1 : currentSelection[1];
                        CurrentSelectable?.SetFocus(true);
                    }
                    mouseActive = true;
                    break;
                }
                // Selectable detection
                for (int i = 0; i < instanceWindows[currentSelection[0]].Selectables.Count; i++)
                {
                    (Vector2, Vector2) result = instanceWindows[currentSelection[0]].SelectableBound(i);
                    if (result.Item1 == Vector2.zero && result.Item2 == Vector2.zero)
                        continue;
                    Vector2 bottomLeftDelta = Mouse.current.position.ReadValue() - result.Item1;
                    if (bottomLeftDelta.x <= 0 || bottomLeftDelta.y <= 0)
                        continue;
                    Vector2 TopRightDelta = Mouse.current.position.ReadValue() - result.Item2;
                    if (TopRightDelta.x >= 0 || TopRightDelta.y >= 0)
                        continue;
                    if (i != currentSelection[1])
                    {
                        selectionUpdated = true;
                        CurrentSelectable?.SetFocus(false);
                        currentSelection[1] = i;
                        CurrentSelectable?.SetFocus(true);
                    }
                    mouseActive = true;
                    break;
                }
            }
        }
        // if input mode is active
        else
        {
            Vector2 mouseLocation = Mouse.current.position.ReadValue();
            Vector2 leftArrowPosition = (CurrentSelectable as SliderUI).CachedArrowPosition.Item1;
            Vector2 rightArrowPosition = (CurrentSelectable as SliderUI).CachedArrowPosition.Item2;
            hoverOnLeft = false;
            hoverOnRight = false;
            float fontSize = (CurrentSelectable as SliderUI).ParentWindow.Setup.FontSize;
            Vector2 leftArrowDelta = mouseLocation - leftArrowPosition;
            Vector2 rightArrowDelta = mouseLocation - rightArrowPosition;
            if (leftArrowDelta.sqrMagnitude < rightArrowDelta.sqrMagnitude && Mathf.Abs(leftArrowDelta.x) < fontSize && Mathf.Abs(leftArrowDelta.y) < fontSize)
            {
                hoverOnLeft = true;
            }
            else if (Mathf.Abs(rightArrowDelta.x) < fontSize && Mathf.Abs(rightArrowDelta.y) < fontSize)
            {
                hoverOnRight = true;
            }
        }
    }
    void PreemptAction()
    {
        if (!inputAllowed || !inFocus)
            return;
        if (Mouse.current != null && Mouse.current.scroll.ReadValue().sqrMagnitude > 0)
            mouseScroll = new Vector2(0, Mathf.Sign(Mouse.current.scroll.ReadValue().y));
    }
    protected void ResizeAllWindows(int extraWidth = 0)
    {
        foreach (WindowUI window in instanceWindows) { window.AutoResize(extraWidth); }
    }
    protected void UpdateWindowLocation()
    {
        if (!windowElementLocationCached)
        {
            // SetOpacity(1, true);
            windowElementLocationCached = true;
            foreach (WindowUI window in instanceWindows)
            {
                window.UpdateElementPosition();
            }
        }
    }
    public override void ToggleDisplay()
    {
        if (disabled)
            return;
        if (active)
            CloseMenu(true, false);
        else
            OpenMenu();
    }
    public virtual void LinkInput()
    {
        inFocus = true;
        // Debug.Log(this.GetType().Name + " Linking Input");
        UIManager.UIInput.OnConfirm += WrappedConfirm;
        UIManager.UIInput.OnCancel += WrappedCancel;
        // UIManager.UIInput.OnQuickConfirm += ConfirmSelection;
        // UIManager.UIInput.OnQuickCancel += CancelSelection;
        UIManager.UIInput.OnMove += Move;
        UIManager.UIInput.OnMouseConfirm += WrappedMouseConfirm;
        UIManager.UIInput.OnMouseCancel += WrappedMouseCancel;
    }
    protected void Move(Vector2 vector2)
    {
        move = vector2;
    }
    public virtual void UnlinkInput()
    {
        inFocus = false;
        // Debug.Log(this.GetType().Name + " Unlinking Input");
        UIManager.UIInput.OnConfirm -= WrappedConfirm;
        UIManager.UIInput.OnCancel -= WrappedCancel;
        // UIManager.UIInput.OnQuickConfirm -= ConfirmSelection;
        // UIManager.UIInput.OnQuickCancel -= CancelSelection;
        UIManager.UIInput.OnMove -= Move;
        UIManager.UIInput.OnMouseConfirm -= WrappedMouseConfirm;
        UIManager.UIInput.OnMouseCancel -= WrappedMouseCancel;
    }
    void WrappedConfirm()
    {
        if (!inputAllowed || !inFocus)
            return;
        ConfirmSelection();
    }
    void WrappedCancel()
    {
        if (!inputAllowed || !inFocus)
            return;
        CancelSelection();
    }
    void WrappedMouseConfirm()
    {
        if (!inputAllowed || !inFocus)
            return;
        MouseConfirmSelection();
    }
    void WrappedMouseCancel()
    {
        if (!inputAllowed || !inFocus)
            return;
        CancelSelection();
    }
    public void ResetSelection()
    {
        foreach (WindowUI window in instanceWindows)
        {
            foreach (ButtonUI element in window.Selectables)
            {
                element.SetFocus(false);
            }
        }
        currentSelection = Vector2Int.zero;
        if (Selectable(currentSelection) == null)
        {
            for (int i = 0; i < instanceWindows.Count; i++)
            {
                if (instanceWindows[i].Selectables.Count == 0)
                    continue;
                currentSelection = new Vector2Int(i, 0);
                break;
            }
        }
        Selectable(currentSelection)?.SetFocus(true);
    }
    protected virtual void SetSelection(int x = 0, int y = 0)
    {
        if (instanceWindows.Count > currentSelection.x && instanceWindows[currentSelection.x].Selectables.Count > currentSelection.y)
            Selectable(currentSelection).SetFocus(false);
        currentSelection = new Vector2Int(x, y);
        if (instanceWindows.Count > currentSelection.x && instanceWindows[currentSelection.x].Selectables.Count > currentSelection.y)
            Selectable(currentSelection).SetFocus(true);
        if (currentSelection == Vector2Int.zero)
            selection.Clear();
    }
    public void Disable(bool disable, bool passThrough = false)
    {
        this.disabled = disable;
        allowOpenTime = Time.unscaledTime + .25f;
        if (!disable && passThrough && parentMenu != null)
            parentMenu.Disable(false, true);
    }
    public void ForcedDisable(bool disable, bool passThrough = false)
    {
        this.disabled = disable;
        allowOpenTime = Mathf.NegativeInfinity;
        if (!disable && passThrough && parentMenu != null)
            parentMenu.Disable(false, true);
    }
    public void SetFocus(bool inFocus)
    {
        this.inFocus = inFocus;
        if (inFocus)
        {
            LinkInput();
            updateTime = Time.unscaledTime + inputDelay;
        }
        else
        {
            UnlinkInput();
        }
    }
    public virtual void ClearElementsFocus()
    {
        foreach (WindowUI window in instanceWindows)
        {
            window.ClearElementsFocus();
        }
    }
    public virtual void ClearWindowFocus()
    {
        foreach (WindowUI window in instanceWindows)
        {
            window.ClearWindowFocus();
        }
    }
    protected virtual void UpdateSelection()
    {
        if (!inputAllowed || !inFocus)
            return;
        bool mouseScrollOverride = false;
        if (move.sqrMagnitude < mouseScroll.sqrMagnitude)
        {
            move = mouseScroll;
            mouseScrollOverride = true;
        }
        if (move.magnitude < .5f)
            return;

        if (!inputMode)
        {
            CurrentSelectable?.SetFocus(false);
            int xBefore = currentSelection[0];
            int yBefore = currentSelection[1];
            if (navDirection != NavDirection.TwoWay)
            {
                int offset = navDirection switch
                {
                    NavDirection.X => Mathf.RoundToInt(move.x) switch
                    {
                        1 => 1,
                        -1 => -1,
                        _ => 0
                    },
                    NavDirection.Y => Mathf.RoundToInt(move.y) switch
                    {
                        1 => -1,
                        -1 => 1,
                        _ => 0
                    },
                    _ => 0
                };
                currentSelection[1] += offset;
                if (instanceWindows.Count == 1)
                {
                    int count = SelectableList().Count;
                    if (allowCycle)
                        currentSelection[1] = currentSelection[1] >= 0 ? currentSelection[1] % count : count - 1;
                    else
                        currentSelection[1] = Mathf.Clamp(currentSelection[1], 0, count - 1);
                }
                else
                {
                    int currentSelectableCount = SelectableList(currentSelection[0]).Count;
                    int windowCount = instanceWindows.Count;
                    if (currentSelection[1] < 0)
                    {
                        int resultWindow = currentSelection[0] - 1;
                        while (resultWindow >= 0 && SelectableList(resultWindow).Count == 0)
                            resultWindow--;
                        if (resultWindow < 0)
                            resultWindow = windowCount - 1;
                        if (resultWindow > currentSelection[0] && allowCycle || resultWindow <= currentSelection[0])
                        {
                            currentSelection[0] = resultWindow;
                            currentSelection[1] = SelectableList(currentSelection[0]).Count - 1;
                        }
                        else
                        {
                            currentSelection[1] = 0;
                        }
                    }
                    else if (currentSelection[1] > currentSelectableCount - 1)
                    {
                        int resultWindow = currentSelection[0] + 1;
                        while (resultWindow < windowCount && SelectableList(resultWindow).Count == 0)
                            resultWindow++;
                        if (resultWindow > windowCount - 1)
                            resultWindow = 0;
                        if (allowCycle && resultWindow < currentSelection[0] || resultWindow > currentSelection[0])
                        {
                            currentSelection[0] = resultWindow;
                            currentSelection[1] = 0;
                        }
                        else
                        {
                            currentSelection[1] = currentSelectableCount - 1;
                        }
                    }
                }
            }
            else
            {
                if (Mathf.Abs(move.x) > Mathf.Abs(move.y))
                {
                    int offset = Mathf.RoundToInt(move.x) switch
                    {
                        1 => 1,
                        -1 => -1,
                        _ => 0
                    };
                    int targetSelection = currentSelection[0];
                    while (targetSelection == currentSelection[0] || SelectableList(targetSelection).Count == 0)
                    {
                        targetSelection += offset;
                        if (allowCycle && targetSelection < 0)
                        {
                            targetSelection = instanceWindows.Count - 1;
                        }
                        else if (allowCycle && targetSelection >= instanceWindows.Count)
                        {
                            targetSelection = 0;
                        }
                        else if (!allowCycle && (targetSelection < 0 || targetSelection >= instanceWindows.Count) || targetSelection == currentSelection[0])
                        {
                            targetSelection = currentSelection[0];
                            break;
                        }
                    }
                    if (targetSelection != currentSelection[0])
                    {
                        Vector2 currentSelectableLocation = instanceWindows[currentSelection[0]].Selectables[currentSelection[1]].CachedPosition.Item1;
                        currentSelection[0] = targetSelection;
                        float minDistanceY = Mathf.Infinity;
                        for (int i = 0; i < instanceWindows[currentSelection[0]].Selectables.Count; i++)
                        {
                            Vector2 selectableLocation = instanceWindows[currentSelection[0]].Selectables[i].CachedPosition.Item1;
                            float distanceY = Mathf.Abs(selectableLocation.y - currentSelectableLocation.y);
                            if (distanceY < minDistanceY)
                            {
                                minDistanceY = distanceY;
                                currentSelection[1] = i;
                            }
                        }
                        currentSelection[1] = Mathf.Clamp(currentSelection[1], 0, SelectableList(currentSelection[0]).Count - 1);
                    }
                }
                else
                {
                    int offset = Mathf.RoundToInt(move.y) switch
                    {
                        1 => -1,
                        -1 => 1,
                        _ => 0
                    };
                    currentSelection[1] += offset;
                    int count = SelectableList(currentSelection[0]).Count;
                    if (allowCycle)
                        currentSelection[1] = currentSelection[1] >= 0 ? currentSelection[1] % count : count - 1;
                    else
                        currentSelection[1] = Mathf.Clamp(currentSelection[1], 0, count - 1);
                }
            }
            if (xBefore != currentSelection[0] || yBefore != currentSelection[1])
            {
                selectionUpdated = true;
            }
            CurrentSelectable?.SetFocus(true);
        }
        else
        {
            int result = CurrentSelectable.Count;
            // update with opposite direction
            int offset = slideDirection switch
            {
                SlideDirection.X => Mathf.RoundToInt(move.x) switch
                {
                    1 => 1,
                    -1 => -1,
                    _ => 0
                },
                SlideDirection.Y => Mathf.RoundToInt(move.y) switch
                {
                    1 => -1,
                    -1 => 1,
                    _ => 0
                },
                _ => 0
            };
            result += offset;
            if (navDirection != NavDirection.TwoWay)
                CurrentSelectable.Count = result;
            else
                CurrentSelectable.Count = result;
            if (offset != 0 && CurrentSelectable.Count == result)
            {
                selectionUpdated = true;
                ClearWindowLocation();
            }
        }
        if (mouseScrollOverride)
        {
            move = Vector2.zero;
            mouseScroll = Vector2.zero;
        }
    }
    void ResetDirection()
    {
        if (!singlePressOnly && move.sqrMagnitude > 0)
        {
            if (holdStart == Mathf.Infinity)
            {
                holdStart = Time.unscaledTime;
            }
            else if (Time.unscaledTime - holdStart >= holdNavigationDelay && (holdNavigationNext == Mathf.Infinity || Time.unscaledTime >= holdNavigationNext))
            {
                holdNavigationNext = Time.unscaledTime + holdNavigationInterval / (float)Mathf.CeilToInt((Time.unscaledTime - holdStart - holdNavigationDelay) / holdNavigationSpeedUpInterval);
            }
        }
        else
        {
            ResetHold();
        }
        updateTime = Time.unscaledTime + .005f;
    }
    void ResetHold()
    {
        holdStart = Mathf.Infinity;
        holdNavigationNext = Mathf.Infinity;
        move = Vector2.zero;
    }
    void RefocusAtNearestElement(Vector2 referenceLocation)
    {
        float minDistance = Mathf.Infinity;
        CurrentSelectable?.SetFocus(false);
        foreach (WindowUI window in instanceWindows)
        {
            if (window.Selectables.Count == 0)
                continue;
            for (int i = 0; i < window.Selectables.Count; i++)
            {
                Vector2 selectableLocation = window.Selectables[i].CachedPosition.Item1;
                float distance = (selectableLocation - referenceLocation).sqrMagnitude;
                if (distance < minDistance)
                {
                    minDistance = distance;
                    currentSelection[0] = instanceWindows.IndexOf(window);
                    currentSelection[1] = i;
                }
            }
            currentSelection[1] = Mathf.Clamp(currentSelection[1], 0, window.Selectables.Count - 1);
        }
        CurrentSelectable?.SetFocus(true);
    }
    void RefocusAtNearestElement(Vector2 referenceLocation, int windowIndex)
    {
        float minDistance = Mathf.Infinity;
        CurrentSelectable?.SetFocus(false);
        WindowUI window = instanceWindows[windowIndex];
        if (window.Selectables.Count > 0)
        {
            for (int i = 0; i < window.Selectables.Count; i++)
            {
                Vector2 selectableLocation = window.Selectables[i].CachedPosition.Item1;
                float distance = (selectableLocation - referenceLocation).sqrMagnitude;
                if (distance < minDistance)
                {
                    minDistance = distance;
                    currentSelection[0] = instanceWindows.IndexOf(window);
                    currentSelection[1] = i;
                }
            }
        }
        currentSelection[1] = Mathf.Clamp(currentSelection[1], 0, window.Selectables.Count - 1);
        CurrentSelectable?.SetFocus(true);
    }
    protected ButtonUIDoubleConfirm AddDoubleConfirmButton(string name, float font = 30f, System.Action action = null)
    {
        WindowSetup setup = DefaultSetup;
        setup.FontSize = font;
        WindowUI window = NewWindow(name, setup);
        ButtonUIDoubleConfirm button = window.AddDoubleConfirmButton(name, action);
        window.AutoResize();
        return button;
    }
    protected ButtonUIDoubleConfirm AddDoubleConfirmButton(string name, Transform layout, float font = 30f, System.Action action = null, System.Action selectAction = null)
    {
        WindowSetup setup = DefaultSetup;
        setup.FontSize = font;
        WindowUI window = NewWindow(name, layout, setup);
        ButtonUIDoubleConfirm button = window.AddDoubleConfirmButton(name, action, selectAction);
        window.AutoResize();
        return button;
    }
    protected ButtonUIDoubleConfirm AddDoubleConfirmButton(string name, Transform layout, int length, float font = 30f, System.Action action = null, System.Action selectAction = null)
    {
        WindowSetup setup = DefaultSetup;
        setup.FontSize = font;
        WindowUI window = NewWindow(name, layout, setup);
        ButtonUIDoubleConfirm button = window.AddDoubleConfirmButton(TextUtility.PlaceHolder(length), action, selectAction);
        window.AutoResize();
        return button;
    }
    protected ButtonUIDoubleConfirm AddDoubleConfirmButton(string name, WindowUI window, System.Action action = null, System.Action selectAction = null) => window.AddDoubleConfirmButton(name, action, selectAction);
    protected ButtonUI AddButton(string name, float font = 30f, System.Action action = null)
    {
        WindowSetup setup = DefaultSetup;
        setup.FontSize = font;
        WindowUI window = NewWindow(name, setup);
        ButtonUI button = window.AddButton(name, action);
        window.AutoResize();
        return button;
    }
    protected ButtonUI AddButton(string name, Transform layout, float font = 30f, System.Action action = null)
    {
        WindowSetup setup = DefaultSetup;
        setup.FontSize = font;
        WindowUI window = NewWindow(name, layout, setup);
        ButtonUI button = window.AddButton(name, action);
        window.AutoResize();
        return button;
    }
    protected ButtonUI AddButton(string name, Transform layout, int length, float font = 30f, System.Action action = null)
    {
        WindowSetup setup = DefaultSetup;
        setup.FontSize = font;
        WindowUI window = NewWindow(name, layout, setup);
        ButtonUI button = window.AddButton(TextUtility.PlaceHolder(length), action);
        window.AutoResize();
        return button;
    }
    protected ButtonUI AddButton(string name, WindowUI window, System.Action action = null) => window.AddButton(name, action);
    protected ButtonUI AddButtonWithCount(string name, float font = 30f, System.Action<int> action = null)
    {
        WindowSetup setup = DefaultSetup;
        setup.FontSize = font;
        WindowUI window = NewWindow(name, setup);
        ButtonUI button = window.AddButtonWithCount(name, action);
        window.AutoResize();
        return button;
    }
    protected ButtonUI AddButtonWithCount(string name, Transform layout, float font = 30f, System.Action<int> action = null)
    {
        WindowSetup setup = DefaultSetup;
        setup.FontSize = font;
        WindowUI window = NewWindow(name, layout, setup);
        ButtonUI button = window.AddButtonWithCount(name, action);
        window.AutoResize();
        return button;
    }
    protected ButtonUI AddButtonWithCount(string name, Transform layout, int length, float font = 30f, System.Action<int> action = null)
    {
        WindowSetup setup = DefaultSetup;
        setup.FontSize = font;
        WindowUI window = NewWindow(name, layout, setup);
        ButtonUI button = window.AddButtonWithCount(TextUtility.PlaceHolder(length), action);
        window.AutoResize();
        return button;
    }
    protected ButtonUI AddButtonWithCount(string name, WindowUI window, System.Action<int> action = null) => window.AddButtonWithCount(name, action);
    protected ToggleUI AddToggle(string name, float font = 30f, System.Action<bool> action = null)
    {
        WindowSetup setup = DefaultSetup;
        setup.FontSize = font;
        WindowUI window = NewWindow(name, setup);
        ToggleUI toggle = window.AddToggle(name, action);
        window.AutoResize();
        return toggle;
    }
    protected ToggleUI AddToggle(string name, Transform layout, float font = 30f, System.Action<bool> action = null)
    {
        WindowSetup setup = DefaultSetup;
        setup.FontSize = font;
        WindowUI window = NewWindow(name, layout, setup);
        ToggleUI toggle = window.AddToggle(name, action);
        window.AutoResize();
        return toggle;
    }

    protected ToggleUI AddToggle(string name, WindowUI window, System.Action<bool> action = null) => window.AddToggle(name, action);
    protected ToggleUIExclusive AddExclusiveToggle(string name, float font = 30f, System.Action<bool> action = null)
    {
        WindowSetup setup = DefaultSetup;
        setup.FontSize = font;
        WindowUI window = NewWindow(name, setup);
        ToggleUIExclusive toggle = window.AddExclusiveToggle(name, action);
        window.AutoResize();
        return toggle;
    }
    protected ToggleUIExclusive AddExclusiveToggle(string name, Transform layout, float font = 30f, System.Action<bool> action = null)
    {
        WindowSetup setup = DefaultSetup;
        setup.FontSize = font;
        WindowUI window = NewWindow(name, layout, setup);
        ToggleUIExclusive toggle = window.AddExclusiveToggle(name, action);
        window.AutoResize();
        return toggle;
    }

    protected ToggleUIExclusive AddExclusiveToggle(string name, WindowUI window, System.Action<bool> action = null) => window.AddExclusiveToggle(name, action);
    protected SliderUI AddSlider(string name, float font = 30f, System.Action<int> action = null)
    {
        WindowSetup setup = DefaultSetup;
        setup.FontSize = font;
        WindowUI window = NewWindow(name, setup);
        SliderUI slider = window.AddSlider(name, action);
        window.AutoResize();
        return slider;
    }
    protected SliderUI AddSlider(string name, Transform layout, float font = 30f, System.Action<int> action = null)
    {
        WindowSetup setup = DefaultSetup;
        setup.FontSize = font;
        WindowUI window = NewWindow(name, layout, setup);
        SliderUI slider = window.AddSlider(name, action);
        window.AutoResize();
        return slider;
    }
    protected SliderUI AddSlider(string name, WindowUI window, System.Action<int> action = null) => window.AddSlider(name, action);
    protected SliderUIChoice AddSliderWithChoice(string name, float font = 30f, System.Action<int> action = null)
    {
        WindowSetup setup = DefaultSetup;
        setup.FontSize = font;
        WindowUI window = NewWindow(name, setup);
        SliderUIChoice slider = window.AddSliderWithChoice(name, action);
        window.AutoResize();
        return slider;
    }
    protected SliderUIChoice AddSliderWithChoice(string name, Transform layout, float font = 30f, System.Action<int> action = null)
    {
        WindowSetup setup = DefaultSetup;
        setup.FontSize = font;
        WindowUI window = NewWindow(name, layout, setup);
        SliderUIChoice slider = window.AddSliderWithChoice(name, action);
        window.AutoResize();
        return slider;
    }
    protected SliderUIChoice AddSliderWithChoice(string name, WindowUI window, System.Action<int> action = null) => window.AddSliderWithChoice(name, action);
    protected virtual bool BaseConfirmAction()
    {
        bool result = CurrentSelectable.GetType().ToString() switch
        {
            nameof(SliderUI) => SliderAction(true),
            nameof(SliderUIChoice) => SliderAction(true),
            nameof(ToggleUI) => ToggleAction(),
            nameof(ToggleUIExclusive) => ToggleAction(),
            nameof(ButtonUI) => ButtonAction(),
            nameof(ButtonUICountable) => ButtonWithCountAction(true),
            nameof(ButtonUIDoubleConfirm) => ButtonAction(),
            _ => false,
        };
        return result;
    }
    protected virtual bool ConfirmSelection()
    {
        if (!active)
            return false;
        if (CurrentSelectable == null)
            return false;
        if (!CurrentSelectable.Available)
            return false;
        return BaseConfirmAction();
    }
    protected virtual bool MouseConfirmSelection()
    {
        if (!active || !mouseActive)
            return false;
        if (CurrentSelectable == null)
            return false;
        if (!CurrentSelectable.Available)
            return false;
        if (!inputMode)
        {
            return BaseConfirmAction();
        }
        else
        {
            if (!hoverOnLeft && !hoverOnRight)
                return false;
            ClearWindowLocation();
            if (hoverOnLeft)
            {
                int initialCount = CurrentSelectable.Count;
                CurrentSelectable.Count -= 1;
            }
            else if (hoverOnRight)
            {
                int initialCount = CurrentSelectable.Count;
                CurrentSelectable.Count += 1;
            }
            return true;
        }
    }
    protected virtual bool ButtonAction()
    {
        CurrentSelectable.TriggerAction();
        return true;
    }
    protected virtual bool ToggleAction()
    {
        if (CurrentSelectable.Available)
            (CurrentSelectable as ToggleUI).Toggle();
        return true;
    }
    protected virtual bool ButtonWithCountAction(bool increment)
    {
        if (increment)
            ((ButtonUICountable)CurrentSelectable).Count++;
        else
            ((ButtonUICountable)CurrentSelectable).Count--;
        return true;
    }
    protected virtual bool SliderAction(bool setInput)
    {
        if (setInput && !inputMode)
        {
            if ((CurrentSelectable as SliderUI).SetInput(true))
            {
                ClearWindowLocation();
                inputMode = true;
            }
        }
        else if (!setInput && inputMode)
        {
            inputMode = false;
            (CurrentSelectable as SliderUI).SetInput(false);
            ClearWindowLocation();
            valueDirty = true;
        }
        return true;
    }
    protected virtual bool CancelSelection()
    {
        if (!active)
            return false;
        if (CurrentSelectable == null)
        {
            if (cancelOutAllowed)
                return CloseMenu(parentMenu == null, parentMenu != null);
            return false;
        }

        bool result = CurrentSelectable.GetType().ToString() switch
        {
            nameof(SliderUI) => inputMode switch
            {
                true => SliderAction(false),
                false when cancelOutAllowed => CloseMenu(parentMenu == null, parentMenu != null),
                _ => false
            },
            nameof(SliderUIChoice) => inputMode switch
            {
                true => SliderAction(false),
                false when cancelOutAllowed => CloseMenu(parentMenu == null, parentMenu != null),
                _ => false
            },
            nameof(ButtonUICountable) => ButtonWithCountAction(false),
            _ when cancelOutAllowed => CloseMenu(parentMenu == null, parentMenu != null),
            _ => false
        };
        return result;
    }
    protected void UpdateWindows()
    {
        foreach (WindowUI window in instanceWindows)
        {
            window.InvokeUpdate();
        }
    }

    protected override void ClearWindows(bool removeWindow = false)
    {
        ClearWindowLocation();
        selection.Clear();
        currentSelection = Vector2Int.zero;
        base.ClearWindows(removeWindow);
    }
    public void ClearWindowLocation(float delay = .1f)
    {
        // SetOpacity(0.1f, true);
        if (updateTime < Time.unscaledTime + delay)
        {
            updateTime = Time.unscaledTime + delay;
        }
        foreach (WindowUI window in instanceWindows)
        {
            window.ClearCachedPosition();
        }
        windowElementLocationCached = false;
    }
    public virtual bool OpenMenu(bool fromSubMenu = false)
    {
        if ((disabled || Time.unscaledTime < allowOpenTime) && !fromSubMenu)
            return false;
        valueDirty = true;
        selectionUpdated = true;
        UIManager.Instance.InMenu = true;
        UnlinkInput();
        LinkInput();
        parentMenu?.Disable(true);
        updateTime = Time.unscaledTime + inputDelay;
        ResetHold();
        if (!fromSubMenu && resetOnOpen)
            ResetSelection();
        mouseActive = false;
        CurrentSelectable?.SetFocus(true);
        foreach (WindowUI window in instanceWindows) { window.SetActive(true); }
        return true;
    }
    public virtual bool OpenMenu(GeneralUISystemWithNavigation parentMenu)
    {
        valueDirty = true;
        selectionUpdated = true;
        this.parentMenu = parentMenu;
        UIManager.Instance.InMenu = true;
        UnlinkInput();
        LinkInput();
        parentMenu?.Disable(true);
        updateTime = Time.unscaledTime + inputDelay;
        ResetHold();
        if (resetOnOpen)
            ResetSelection();
        mouseActive = false;
        CurrentSelectable?.SetFocus(true);
        foreach (WindowUI window in instanceWindows) { window.SetActive(true); }
        return true;
    }
    public virtual bool OpenSubMenu(int index)
    {
        subMenus[index].OpenMenu(this);
        CloseMenu(false, false);
        return true;
    }
    public void QuickFullCloseMenu()
    {
        CloseMenu(true, false);
    }
    public virtual bool CloseMenu(bool fullQuit, bool openParent = true)
    {
        if (updateTime == Mathf.Infinity || !active)
            return false;
        UnlinkInput();
        if (fullQuit)
        {
            parentMenu?.Disable(false, true);
            CloseMenuCleanup(false);
            UIManager.Instance.InMenu = false;
        }
        else if (parentMenu != null && openParent)
        {
            parentMenu.Disable(false, false);
            parentMenu.OpenMenu(true);
        }
        if (inputMode)
        {
            inputMode = false;
            (CurrentSelectable as SliderUI).SetInput(false);
        }
        updateTime = Mathf.Infinity;
        active = false;
        foreach (WindowUI window in instanceWindows) { window.SetActive(false); }
        return true;
    }
    public virtual void CloseMenuCleanup(bool fromSub)
    {
        parentMenu?.CloseMenuCleanup(true);
    }
    protected virtual void OnSelectionUpdate() => _ = 0;
    protected virtual void ConstantUpdate() => _ = 0;
}
