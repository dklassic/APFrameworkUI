using System;
using UnityEngine;
using ChosenConcept.APFramework.Interface.Framework.Element;
using Object = UnityEngine.Object;

namespace ChosenConcept.APFramework.Interface.Framework
{
    [Serializable]
    public class SimpleMenu : IMenuInputTarget
    {
        [Header("Debug View")] [SerializeField]
        string _menuName;

        [SerializeField] MenuSetup _menuSetup;
        [SerializeField] MenuStyling _menuStyling;
        [SerializeField] WindowUI _windowInstance;
        [SerializeField] LayoutAlignment _layoutAlignmentInstance;
        [SerializeField] bool _displayActive;
        [SerializeField] bool _navigationActive;
        [SerializeField] int _linkFrame = -1;
        [SerializeField] float _nextNavigationUpdate = Mathf.Infinity;
        [SerializeField] int _currentSelection = -1;
        [SerializeField] float _holdStart = Mathf.Infinity;
        [SerializeField] float _holdNavigationNext = Mathf.Infinity;
        [SerializeField] Vector2 _move = Vector2.zero;
        [SerializeField] Vector2 _mouseScroll = Vector2.zero;
        [SerializeField] Vector2 _lastMousePosition = Vector2.negativeInfinity;
        [SerializeField] bool _mouseActive;
        [SerializeField] bool _hoverOnDecrease;
        [SerializeField] bool _hoverOnIncrease;
        [SerializeField] bool _inElementInputMode;
        [SerializeField] bool _selectionUpdated;
        [SerializeField] bool _focused;
        [SerializeField] bool _movingWindow;
        Action _menuCloseAction;

        public bool canBeClosedByOutOfFocusClick =>
            _menuSetup.allowCloseOnClick is MenuCloseOnClickBehavior.Both
                or MenuCloseOnClickBehavior.OutOfFocus;

        public bool movingWindow => _movingWindow;
        bool windowPositionCached => _windowInstance.positionCached;

        public WindowElement currentSelectable
        {
            get
            {
                if (GetSelectable(_currentSelection) == null)
                    return null;
                return GetSelectable(_currentSelection);
            }
        }

        public string menuTag => $"Interface.{_menuName}";
        public bool isDisplayActive => _displayActive;
        public bool isNavigationActive => _navigationActive;
        public WindowUI windowInstance => _windowInstance;
        public bool focused => _focused;
        public bool mouseActive => _mouseActive;
        public bool inElementInputMode => _inElementInputMode;

        public SimpleMenu(string name)
        {
            _menuName = name;
            _menuSetup = MenuSetup.defaultSetup;
            _menuStyling = MenuStyling.defaultStyling;
            InitNewLayout();
        }

        public SimpleMenu(string name, MenuSetup menuSetup)
        {
            _menuName = name;
            _menuSetup = menuSetup;
            _menuStyling = MenuStyling.defaultStyling;
        }

        public SimpleMenu(string name, MenuSetup menuSetup, MenuStyling menuStyling)
        {
            _menuName = name;
            _menuSetup = menuSetup;
            _menuStyling = menuStyling;
        }

        public SimpleMenu(string name, MenuSetup menuSetup, WindowSetup windowSetup, LayoutSetup layoutSetup)
        {
            _menuName = name;
            _menuSetup = menuSetup;
            _menuStyling = MenuStyling.defaultStyling;
            _menuStyling.windowSetup = windowSetup;
            _menuStyling.layoutSetup = layoutSetup;
        }

        public SimpleMenu(string name, MenuSetup menuSetup, WindowSetup windowSetup, LayoutAlignment layoutAlignment)
        {
            _menuName = name;
            _menuSetup = menuSetup;
            _menuStyling = MenuStyling.defaultStyling;
            _menuStyling.windowSetup = windowSetup;
            _layoutAlignmentInstance = layoutAlignment;
        }

        public bool IsMouseInWindow(Vector2 mousePosition)
        {
            return _windowInstance.ContainsPosition(mousePosition);
        }

        public bool ExistsSelectable(int i) => _windowInstance.interactables.Count > i && i >= 0;

        public WindowElement GetSelectable(int i) =>
            i >= 0 && i < _windowInstance.interactables.Count ? _windowInstance.interactables[i] : null;

        public void UpdateMenu()
        {
            if (!_displayActive)
                return;
            if (!_navigationActive && Time.unscaledTime >= _nextNavigationUpdate)
                _navigationActive = true;
            UpdateNavigation();
        }

        void UpdateNavigation()
        {
            if (!_windowInstance.canNavigate || _linkFrame == Time.frameCount)
                return;
            if (_movingWindow)
            {
                _windowInstance.Move(WindowManager.instance.inputProvider.mouseDelta);
                return;
            }

            UpdateMouseNavigation();
            if (Time.unscaledTime < _nextNavigationUpdate)
                return;
            UpdateWindowPosition();
            if (float.IsPositiveInfinity(_holdStart) || _holdNavigationNext <= Time.unscaledTime)
            {
                UpdateSelection();
            }

            ResetDirection();

            if (_selectionUpdated)
            {
                _selectionUpdated = false;
                OnSelectionUpdate();
            }
        }

        public void ForceUpdateDisplayContent()
        {
            _windowInstance.InvokeUpdate();
        }

        public void SetOpacity(float opacity)
        {
            _windowInstance.SetOpacity(opacity);
        }

        public LayoutAlignment InitNewLayout()
        {
            LayoutAlignment layout =
                WindowManager.instance.InstantiateLayout(_menuStyling.layoutSetup,
                    menuTag);
            _layoutAlignmentInstance = layout;
            return layout;
        }

        /// <summary>
        /// A simple method to spawn window
        /// </summary>
        public WindowUI NewWindow(string windowName)
        {
            if (_layoutAlignmentInstance == null)
                InitNewLayout();
            WindowUI window =
                WindowManager.instance.NewWindow(windowName, _layoutAlignmentInstance, _menuStyling.windowSetup,
                    menuTag);
            _windowInstance = window;
            return window;
        }

        /// <summary>
        /// A simple method to spawn single text UI element window
        /// </summary>
        public TextUI AddText(string elementName, int length = 0)
        {
            if (_windowInstance == null)
                NewWindow(elementName);
            TextUI text;
            if (length == 0)
                text = _windowInstance.AddText(elementName);
            else
                text = _windowInstance.AddText(TextUtility.PlaceHolder(length));
            _windowInstance.AutoResize();
            return text;
        }

        /// <summary>
        /// A simple method to spawn text UI to pre-initialized Window
        /// </summary>
        public void AddGap()
        {
            TextUI text = AddText("Blank");
            text.SetLabel("　");
        }

        public void RemoveElement(WindowElement element)
        {
            element.Remove();
        }

        public void RemoveWindowsAndLayoutGroups()
        {
            ClearWindow(true);
            // with shared instance, it is possible that another menu destroys the layout
            if (_layoutAlignmentInstance != null)
            {
                Object.Destroy(_layoutAlignmentInstance.gameObject);
                _layoutAlignmentInstance = null;
            }
        }

        public void ClearWindow(bool removeWindow)
        {
            ClearWindowLocation();
            _currentSelection = -1;
            _windowInstance.ClearElements();
            if (removeWindow)
            {
                WindowManager.instance.CloseWindow(_windowInstance);
                _windowInstance = null;
            }
        }

        public void SetWindowLocalizedByTag()
        {
            _windowInstance.SetLocalizedByTag();
        }

        public void AutoResizeWindow(int extraWidth = 0)
        {
            _windowInstance.AutoResize(extraWidth);
        }

        public void Refresh()
        {
            _windowInstance.InvokeUpdate();
            _windowInstance.RefreshSize();
        }

        public void ContextLanguageChange()
        {
            _windowInstance.ContextLanguageChange();
        }

        public void TriggerResolutionChange()
        {
            _layoutAlignmentInstance.ContextResolutionChange();
        }

        void UpdateMouseNavigation()
        {
            if (!_displayActive || !WindowManager.instance.inputProvider.inputEnabled ||
                _lastMousePosition == WindowManager.instance.inputProvider.mousePosition ||
                !WindowManager.instance.inputProvider.hasMouse || _windowInstance == null
                || !_navigationActive)
            {
                return;
            }

            _lastMousePosition = WindowManager.instance.inputProvider.mousePosition;
            if (!_inElementInputMode)
            {
                _mouseActive = false;
                // if the window is a single button window, perform window check only
                if (_windowInstance.isSingleButtonWindow)
                {
                    if (!IsMouseInWindow(_lastMousePosition))
                    {
                        _windowInstance.interactables[0].SetFocus(false);
                        ClearSelection();
                        return;
                    }

                    _currentSelection = 0;
                    _windowInstance.interactables[0].SetFocus(true);
                    _selectionUpdated = true;
                    _mouseActive = true;
                }
                else
                {
                    if (!IsMouseInWindow(_lastMousePosition))
                    {
                        _windowInstance.SetFocus(false);
                        ClearSelection();
                        return;
                    }

                    _windowInstance.SetFocus(true);
                    for (int i = 0; i < _windowInstance.interactables.Count; i++)
                    {
                        if (!_windowInstance.InteractableContainsPosition(i, _lastMousePosition))
                            continue;
                        if (i != _currentSelection)
                        {
                            _selectionUpdated = true;
                            currentSelectable?.SetFocus(false);
                            _currentSelection = i;
                            currentSelectable?.SetFocus(true);
                        }

                        _mouseActive = true;
                        break;
                    }

                    if (!_mouseActive)
                        ClearSelection();
                }
            }
            // if input mode is active
            else
            {
                if (currentSelectable is ISlider slider)
                {
                    (_hoverOnDecrease, _hoverOnIncrease) = slider.HoverOnArrow(_lastMousePosition);
                }

                if (currentSelectable is ScrollableTextUI scrollableTextUI)
                {
                    (_hoverOnDecrease, _hoverOnIncrease) = scrollableTextUI.HoverOnArrow(_lastMousePosition);
                }
            }
        }

        void UpdateWindowPosition()
        {
            if (!windowPositionCached)
            {
                _windowInstance.UpdateElementsAndWindowPosition();
            }
        }

        public void LinkInput()
        {
            _linkFrame = Time.frameCount;
            ResetInput();
            WindowManager.instance.LinkInputTarget(this);
        }

        public void UnlinkInput()
        {
            ResetInput();
            WindowManager.instance.UnlinkInput(this);
        }

        void ResetInput()
        {
            _move = Vector2.zero;
            _mouseScroll = Vector2.zero;
        }

        public void ResetSelection()
        {
            ResetInput();
            foreach (WindowElement element in _windowInstance.interactables)
            {
                element.SetFocus(false);
            }

            _currentSelection = -1;
            if (GetSelectable(_currentSelection) == null && _windowInstance.canNavigate)
            {
                _currentSelection = 0;
            }

            GetSelectable(_currentSelection)?.SetFocus(true);
        }

        // Used to set all windows in unselected state
        public void ClearSelection()
        {
            foreach (WindowElement element in _windowInstance.interactables)
            {
                element.SetFocus(false);
            }

            _currentSelection = -1;
        }

        public void SetCurrentSelection(int x)
        {
            GetSelectable(_currentSelection)?.SetFocus(false);
            _currentSelection = x;
            GetSelectable(_currentSelection)?.SetFocus(true);
        }

        public void SetNavigationActive(bool active)
        {
            if (active)
            {
                LinkInput();
                DelayInput(_menuSetup.menuOpenInputDelay);
            }
            else
            {
                UnlinkInput();
                _nextNavigationUpdate = Mathf.Infinity;
            }
        }

        public void ClearElementsFocus()
        {
            _windowInstance.ClearElementsFocus();
        }

        public void ClearWindowFocus()
        {
            _windowInstance.ClearWindowFocus();
        }

        void UpdateSelection()
        {
            if (!_navigationActive)
                return;
            bool mouseScrollOverride = false;
            if (_move.sqrMagnitude < _mouseScroll.sqrMagnitude)
            {
                _move = _mouseScroll;
                mouseScrollOverride = true;
            }

            if (_move.magnitude < .5f)
                return;
            // if all elements are deselected, reset to the first element first
            if (_currentSelection < 0)
            {
                ResetSelection();
            }
            else
            {
                UpdateSelectionByMovement(_move.normalized);
                // Ignore when mouse scroll to prevent scrolling out of a window
                if (!_inElementInputMode && !_selectionUpdated && !mouseScrollOverride)
                {
                    WindowManager.instance.CheckClosestDirectionalMatch(this,
                        currentSelectable.cachedPosition.Item1, _move.normalized, _menuSetup.allowCycleBetweenWindows);
                }

                _move = Vector2.zero;
            }

            if (mouseScrollOverride)
            {
                _move = Vector2.zero;
                _mouseScroll = Vector2.zero;
            }
        }

        void UpdateSelectionByMovement(Vector2 move)
        {
            if (!_inElementInputMode)
            {
                currentSelectable?.SetFocus(false);
                int yBefore = _currentSelection;
                int offset = Mathf.RoundToInt(move.y) switch
                {
                    1 => -1,
                    -1 => 1,
                    _ => 0
                };
                _currentSelection += offset;
                int count = _windowInstance.interactables.Count;
                if (_menuSetup.allowCycleWithinWindow)
                    _currentSelection = _currentSelection >= 0 ? _currentSelection % count : count - 1;
                else
                    _currentSelection = Mathf.Clamp(_currentSelection, 0, count - 1);

                if (yBefore != _currentSelection)
                {
                    _selectionUpdated = true;
                }


                currentSelectable?.SetFocus(true);
            }
            else
            {
                int result = currentSelectable.count;
                int offset = Mathf.RoundToInt(move.x) switch
                {
                    1 => 1,
                    -1 => -1,
                    _ => 0
                };
                if (currentSelectable is ScrollableTextUI scrollableText)
                {
                    if (move.y != 0)
                    {
                        offset = Mathf.RoundToInt(move.y) switch
                        {
                            1 => -1,
                            -1 => 1,
                            _ => 0
                        };
                    }
                    else
                    {
                        offset *= scrollableText.contentHeight;
                    }
                }

                result += offset;
                currentSelectable.count = result;
                if (offset != 0 && currentSelectable.count == result)
                {
                    _selectionUpdated = true;
                    ClearWindowLocation();
                }
            }
        }

        void ResetDirection()
        {
            if (!_menuSetup.singlePressOnly && _move.sqrMagnitude > 0)
            {
                if (float.IsPositiveInfinity(_holdStart))
                {
                    _holdStart = Time.unscaledTime;
                }
                else if (Time.unscaledTime - _holdStart >= _menuSetup.holdNavigationDelay &&
                         (float.IsPositiveInfinity(_holdNavigationNext) ||
                          Time.unscaledTime >= _holdNavigationNext))
                {
                    _holdNavigationNext = Time.unscaledTime + _menuSetup.holdNavigationInterval /
                        Mathf.CeilToInt((Time.unscaledTime - _holdStart - _menuSetup.holdNavigationDelay) /
                                        _menuSetup.holdNavigationSpeedUpInterval);
                }
            }
            else
            {
                ResetHold();
            }

            DelayInput(0.005f);
        }

        void ResetHold()
        {
            _holdStart = Mathf.Infinity;
            _holdNavigationNext = Mathf.Infinity;
            _move = Vector2.zero;
        }

        public void RefocusAtNearestElement(Vector2 referenceLocation)
        {
            if (!_displayActive)
                return;
            if (!windowPositionCached)
                UpdateWindowPosition();
            float minDistance = Mathf.Infinity;
            currentSelectable?.SetFocus(false);
            if (!_windowInstance.canNavigate)
                return;
            for (int i = 0; i < _windowInstance.interactables.Count; i++)
            {
                Vector2 selectableLocation = _windowInstance.interactables[i].cachedPosition.Item1;
                float distance = (selectableLocation - referenceLocation).sqrMagnitude;
                if (distance < minDistance)
                {
                    minDistance = distance;
                    _currentSelection = i;
                }
            }

            _currentSelection = Mathf.Clamp(_currentSelection, 0, _windowInstance.interactables.Count - 1);

            currentSelectable?.SetFocus(true);
        }

        public ButtonUI AddButton(string elementName, Action action = null)
        {
            return AddButton(elementName, _menuStyling.windowSetup, action);
        }

        public ButtonUI AddButton(string elementName, WindowSetup setup, Action action = null)
        {
            if (_windowInstance == null)
                NewWindow(elementName);
            ButtonUI button = _windowInstance.AddButton(elementName, action);
            _windowInstance.AutoResize();
            return button;
        }

        public ButtonUI AddButton(string elementName, WindowUI window, Action action = null) =>
            window.AddButton(elementName, action);

        public ScrollableTextUI AddScrollableText(string elementName, Action action = null)
        {
            return AddScrollableText(elementName, _menuStyling.windowSetup, action);
        }

        public ScrollableTextUI AddScrollableText(string elementName, WindowSetup setup, Action action = null)
        {
            if (_windowInstance == null)
                NewWindow(elementName);
            ScrollableTextUI button = _windowInstance.AddScrollableText(elementName, action);
            _windowInstance.AutoResize();
            return button;
        }

        public ScrollableTextUI AddScrollableText(string elementName, WindowUI window, Action action = null) =>
            window.AddScrollableText(elementName, action);

        public SingleSelectionUI<T> AddSingleSelection<T>(string elementName, Action<T> action = null)
        {
            return AddSingleSelection(elementName, _menuStyling.windowSetup, action);
        }

        public SingleSelectionUI<T> AddSingleSelection<T>(string elementName, WindowSetup setup,
            Action<T> action = null)
        {
            if (_windowInstance == null)
                NewWindow(elementName);
            SingleSelectionUI<T> button = _windowInstance.AddSingleSelection(elementName, action);
            _windowInstance.AutoResize();
            return button;
        }

        public SingleSelectionUI<T>
            AddSingleSelection<T>(string elementName, WindowUI window, Action<T> action = null) =>
            window.AddSingleSelection(elementName, action);

        public TextInputUI AddTextInput(string elementName, Action<string> action = null)
        {
            return AddTextInput(elementName, _menuStyling.windowSetup, action);
        }

        public TextInputUI AddTextInput(string elementName, WindowSetup setup, Action<string> action = null)
        {
            if (_windowInstance == null)
                NewWindow(elementName);
            TextInputUI button = _windowInstance.AddTextInput(elementName, action);
            _windowInstance.AutoResize();
            return button;
        }

        public TextInputUI AddTextInput(string elementName, WindowUI window, Action<string> action = null) =>
            window.AddTextInput(elementName, action);

        public ToggleUI AddToggle(string elementName, float font = 30f, Action<bool> action = null)
        {
            return AddToggle(elementName, _menuStyling.windowSetup, font, action);
        }

        public ToggleUI AddToggle(string elementName, WindowSetup setup, float font = 30f,
            Action<bool> action = null)
        {
            if (_windowInstance == null)
                NewWindow(elementName);
            ToggleUI toggle = _windowInstance.AddToggle(elementName, action);
            _windowInstance.AutoResize();
            return toggle;
        }

        public ToggleUI AddToggle(string elementName, WindowUI window, Action<bool> action = null) =>
            window.AddToggle(elementName, action);

        public SliderUI<T> AddSlider<T>(string elementName, Action<T> action = null)
        {
            return AddSlider(elementName, _menuStyling.windowSetup, action);
        }

        public SliderUI<T> AddSlider<T>(string elementName, WindowSetup setup,
            Action<T> action = null)
        {
            if (_windowInstance == null)
                NewWindow(elementName);
            SliderUI<T> slider = _windowInstance.AddSlider(elementName, action);
            _windowInstance.AutoResize();
            return slider;
        }

        public SliderUI<T>
            AddSlider<T>(string elementName, WindowUI window, Action<T> action = null) =>
            window.AddSlider(elementName, action);

        bool BaseConfirmAction()
        {
            bool result = currentSelectable switch
            {
                ToggleUI toggle => ToggleAction(toggle),
                IQuickSelect button => QuickSelectionAction(button, true),
                TextInputUI textInput => TextInputAction(textInput),
                ISelectable selection => SingleSelectionAction(selection),
                ISlider input => InputAction(input, !_inElementInputMode),
                ScrollableTextUI scrollableText => ScrollableTextAction(scrollableText, !_inElementInputMode),
                ButtonUI button => ButtonAction(button),
                _ => false,
            };
            return result;
        }

        public void SetTextInput(string text)
        {
            if (currentSelectable is TextInputUI textInput)
            {
                textInput.SetInputContent(text);
            }

            _inElementInputMode = false;
            LinkInput();
        }

        void IMenuInputTarget.SetSelection(int count)
        {
            if (currentSelectable is ISelectable button)
            {
                button.SetCount(count);
            }
        }

        void IMenuInputTarget.OnMove(Vector2 vector2)
        {
            _move = vector2;
        }

        void IMenuInputTarget.OnScroll(Vector2 vector2)
        {
            _mouseScroll = vector2;
        }

        void IMenuInputTarget.OnCancel()
        {
            CancelSelection();
        }

        void IMenuInputTarget.OnConfirm()
        {
            ConfirmSelection();
        }

        void IMenuInputTarget.OnMouseConfirmPressed()
        {
            MouseConfirmSelection();
            if (_menuSetup.allowDraggingWithMouse && !_movingWindow && _currentSelection == -1 &&
                _windowInstance.ContainsPosition(WindowManager.instance.inputProvider.mousePosition))
            {
                _movingWindow = true;
                _windowInstance.Move(Vector2.zero);
            }
            else if (_menuSetup.allowCloseOnClick is MenuCloseOnClickBehavior.Both
                         or MenuCloseOnClickBehavior.InFocus && _currentSelection == -1)
            {
                CloseMenu();
            }
        }

        void IMenuInputTarget.OnMouseConfirmReleased()
        {
            if (!_movingWindow)
                return;
            _movingWindow = false;
            _windowInstance.ClearCachedPosition();
            _windowInstance.UpdateElementsAndWindowPosition();
        }

        void IMenuInputTarget.OnMouseCancel()
        {
            CancelSelection();
        }

        void IMenuInputTarget.OnKeyboardEscape()
        {
        }

        bool ConfirmSelection()
        {
            if (!_displayActive)
                return false;
            if (!_navigationActive || _linkFrame == Time.frameCount)
                return false;
            if (currentSelectable == null)
                return false;
            if (!currentSelectable.available)
                return false;
            return BaseConfirmAction();
        }

        bool MouseConfirmSelection()
        {
            if (!_displayActive || !_mouseActive)
            {
                return false;
            }

            if (!_navigationActive || _linkFrame == Time.frameCount)
            {
                return false;
            }

            if (currentSelectable == null)
            {
                return false;
            }

            if (!currentSelectable.available)
            {
                return false;
            }

            if (!_inElementInputMode)
            {
                return BaseConfirmAction();
            }

            if (!_hoverOnDecrease && !_hoverOnIncrease)
                return false;
            ClearWindowLocation();
            if (_hoverOnDecrease)
            {
                int initialCount = currentSelectable.count;
                currentSelectable.count -= 1;
                if (initialCount != currentSelectable.count)
                    _selectionUpdated = true;
            }
            else if (_hoverOnIncrease)
            {
                int initialCount = currentSelectable.count;
                currentSelectable.count += 1;
                if (initialCount != currentSelectable.count)
                    _selectionUpdated = true;
            }

            return true;
        }

        bool ButtonAction(ButtonUI button)
        {
            button.TriggerAction();
            return true;
        }

        bool ToggleAction(ToggleUI toggle)
        {
            if (toggle.available)
                toggle.Toggle();
            return true;
        }

        bool QuickSelectionAction(IQuickSelect button, bool increment)
        {
            if (increment)
                button.SetNextChoice();
            else
                button.SetPreviousChoice();
            return true;
        }

        bool TextInputAction(TextInputUI textInput)
        {
            WindowManager.instance.GetTextInput(this,
                textInput);
            _inElementInputMode = true;
            textInput.SetInput(true);
            return true;
        }

        bool SingleSelectionAction(ISelectable selection)
        {
            CloseMenu();
            WindowManager.instance.GetSingleSelectionInput(this,
                selection.values, selection.activeCount);
            return true;
        }

        bool ScrollableTextAction(ScrollableTextUI scrollableText, bool setInput)
        {
            if (setInput && !_inElementInputMode)
            {
                scrollableText.SetScrolling(true);
                ClearWindowLocation();
                _inElementInputMode = true;
            }
            else if (!setInput && _inElementInputMode)
            {
                _inElementInputMode = false;
                scrollableText.SetScrolling(false);
                ClearWindowLocation();
            }

            return true;
        }

        bool InputAction(ISlider inputElement, bool setInput)
        {
            if (setInput && !_inElementInputMode)
            {
                inputElement.SetInput(true);
                ClearWindowLocation();
                _inElementInputMode = true;
            }
            else if (!setInput && _inElementInputMode)
            {
                _inElementInputMode = false;
                inputElement.SetInput(false);
                ClearWindowLocation();
            }

            return true;
        }

        bool CancelSelection()
        {
            if (!_displayActive)
                return false;
            if (!_navigationActive || _linkFrame == Time.frameCount)
                return false;
            if (currentSelectable == null)
            {
                if (_menuSetup.allowCloseMenuWithCancelAction)
                    return CancelOut();
                return false;
            }

            bool result = currentSelectable switch
            {
                ISlider slider when _inElementInputMode => InputAction(slider, false),
                IQuickSelect button => QuickSelectionAction(button, false),
                ScrollableTextUI scrollableText when _inElementInputMode => ScrollableTextAction(scrollableText, false),
                _ when _menuSetup.allowCloseMenuWithCancelAction => CancelOut(),
                _ => false
            };
            return result;
        }

        public void UpdateWindows()
        {
            _windowInstance.InvokeUpdate();
        }

        public void ClearWindowLocation(float delay = .1f)
        {
            if (_nextNavigationUpdate < Time.unscaledTime + delay)
            {
                DelayInput(delay);
            }

            _windowInstance.ClearCachedPosition();
        }

        public bool OpenMenu()
        {
            return OpenMenu(_menuSetup.allowNavigationOnOpen);
        }

        public bool OpenMenu(bool enableNavigation)
        {
            _displayActive = true;
            _selectionUpdated = true;
            if (enableNavigation)
                DelayInput(_menuSetup.menuOpenInputDelay);
            ResetHold();
            switch (_menuSetup.resetOnOpen)
            {
                case MenuResetOnOpenBehavior.ResetSelection:
                    ResetSelection();
                    break;
                case MenuResetOnOpenBehavior.ClearSelection:
                    ClearSelection();
                    break;
                case MenuResetOnOpenBehavior.Disable:
                    break;
            }

            _mouseActive = false;
            currentSelectable?.SetFocus(true);
            _windowInstance.SetActive(true);

            return true;
        }

        public bool CloseMenu()
        {
            if (!_displayActive)
                return false;
            UnlinkInput();
            _nextNavigationUpdate = Mathf.Infinity;

            if (_inElementInputMode)
            {
                _inElementInputMode = false;
                if (currentSelectable is ISlider slider)
                    slider.SetInput(false);
                if (currentSelectable is ScrollableTextUI scrollableText)
                    scrollableText.SetScrolling(false);
            }

            ClearWindowFocus();
            ClearElementsFocus();
            _focused = false;
            _displayActive = false;
            _navigationActive = false;
            _nextNavigationUpdate = Mathf.Infinity;
            _windowInstance.SetActive(false);
            _menuCloseAction?.Invoke();

            return true;
        }

        void OnSelectionUpdate()
        {
            _windowInstance.TriggerSelectionUpdate();
        }

        bool CancelOut()
        {
            CloseMenu();
            return true;
        }

        public void DelayInput(float delay)
        {
            _nextNavigationUpdate = Time.unscaledTime + delay;
        }

        public void SetFocused(bool focused)
        {
            if (_focused == focused)
                return;
            _focused = focused;
            if (!focused)
            {
                _currentSelection = -1;
                ClearWindowFocus();
                ClearElementsFocus();
                UnlinkInput();
            }
            else
            {
                _windowInstance.SetFocus(true);
                LinkInput();
            }
        }

        public void SetMenuCloseAction(Action action)
        {
            _menuCloseAction = action;
        }
    }
}