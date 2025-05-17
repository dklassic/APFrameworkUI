using System;
using UnityEngine;
using ChosenConcept.APFramework.Interface.Framework.Element;
using Unity.Collections;
using Object = UnityEngine.Object;

namespace ChosenConcept.APFramework.Interface.Framework
{
    [Serializable]
    public class SimpleMenu : IMenuInputTarget, ITextInputTarget, ISelectionInputTarget
    {
        [Header("Debug View")]
        [SerializeField]  string _menuName;
        [SerializeField]  MenuSetup _menuSetup;
        [SerializeField]  MenuStyling _menuStyling;
        [SerializeField]  WindowUI _instanceWindow;
        [SerializeField]  LayoutAlignment _instanceLayoutAlignment;
        [SerializeField]  bool _displayActive;
        [SerializeField]  bool _navigationActive;
        [SerializeField]  int _linkFrame = -1;
        [SerializeField]  float _nextNavigationUpdate = Mathf.Infinity;
        [SerializeField]  int _currentSelection = -1;
        [SerializeField]  bool _windowElementLocationCached;
        [SerializeField]  float _holdStart = Mathf.Infinity;
        [SerializeField]  float _holdNavigationNext = Mathf.Infinity;
        [SerializeField]  Vector2 _move = Vector2.zero;
        [SerializeField]  Vector2 _mouseScroll = Vector2.zero;
        [SerializeField]  Vector2 _lastMousePosition = Vector2.negativeInfinity;
        [SerializeField]  bool _mouseActive;
        [SerializeField]  bool _hoverOnDecrease;
        [SerializeField]  bool _hoverOnIncrease;
        [SerializeField]  bool _inElementInputMode;
        [SerializeField]  bool _selectionUpdated;
        [SerializeField]  bool _focused;

        public ButtonUI currentSelectable
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
        public WindowUI instanceWindow => _instanceWindow;
        public bool focused => _focused;
        public bool mouseActive => _mouseActive;

        public SimpleMenu(string name)
        {
            _menuName = name;
            _menuSetup = MenuSetup.defaultSetup;
            _menuStyling = MenuStyling.defaultStyling;
            InitNewLayout();
        }

        public SimpleMenu(string name, MenuSetup menuSetup, WindowSetup windowSetup, LayoutSetup layoutSetup)
        {
            _menuName = name;
            _menuSetup = menuSetup;
            _menuStyling = MenuStyling.defaultStyling;
            _menuStyling.windowSetup = windowSetup;
            _menuStyling.layoutSetup = layoutSetup;
        }

        public bool IsMouseInWindow(Vector2 mousePosition)
        {
            if (_instanceWindow.interactables.Count == 0)
                return false;
            (Vector2 topLeft, Vector2 bottomRight) = _instanceWindow.cachedPosition;
            if (topLeft == Vector2.zero && bottomRight == Vector2.zero)
                return false;
            Vector2 topLeftDelta = mousePosition - topLeft;
            if (topLeftDelta.x <= 0 || topLeftDelta.y <= 0)
                return false;
            Vector2 bottomRightDelta = mousePosition - bottomRight;
            if (bottomRightDelta.x >= 0 || bottomRightDelta.y >= 0)
                return false;
            return true;
        }

        public bool ExistsSelectable(int i) => _instanceWindow.interactables.Count > i && i >= 0;

        public ButtonUI GetSelectable(int i) =>
            i >= 0 && i < _instanceWindow.interactables.Count ? _instanceWindow.interactables[i] : null;

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
            if (_instanceWindow.interactables.Count == 0 || _linkFrame == Time.frameCount)
                return;
            UpdateMouseNavigation();
            if (Time.unscaledTime < _nextNavigationUpdate)
                return;
            UpdateWindowLocation();
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
            _instanceWindow.InvokeUpdate();
        }

        public void SetOpacity(float opacity)
        {
            _instanceWindow.SetOpacity(opacity);
        }

        public void InitNewLayout()
        {
            LayoutAlignment layout =
                WindowManager.instance.InstantiateLayout(_menuStyling.layoutSetup,
                    menuTag);
            _instanceLayoutAlignment = layout;
        }

        /// <summary>
        /// A simple method to spawn window
        /// </summary>
        void NewWindow(string windowName, WindowSetup setup)
        {
            if (_instanceLayoutAlignment == null)
                InitNewLayout();
            WindowUI window =
                WindowManager.instance.NewWindow(windowName, _instanceLayoutAlignment, setup,
                    menuTag);
            _instanceWindow = window;
        }

        /// <summary>
        /// A simple method to spawn single text UI element window
        /// </summary>
        public TextUI AddText(string elementName, int length = 0)
        {
            if (_instanceWindow == null)
                NewWindow(elementName, _menuStyling.windowSetup);
            TextUI text;
            if (length == 0)
                text = _instanceWindow.AddText(elementName);
            else
                text = _instanceWindow.AddText(TextUtility.PlaceHolder(length));
            _instanceWindow.AutoResize();
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
            Object.Destroy(_instanceLayoutAlignment.gameObject);
            _instanceLayoutAlignment = null;
        }

        public void ClearWindow(bool removeWindow)
        {
            ClearWindowLocation();
            _currentSelection = -1;
            _instanceWindow.ClearElements();
            if (removeWindow)
            {
                WindowManager.instance.CloseWindow(_instanceWindow);
                _instanceWindow = null;
            }
        }

        public void SetWindowLocalizedByTag()
        {
            _instanceWindow.SetLocalizedByTag();
        }

        public void AutoResizeWindow(int extraWidth = 0)
        {
            _instanceWindow.AutoResize(extraWidth);
        }

        public void Refresh()
        {
            _instanceWindow.InvokeUpdate();
            _instanceWindow.RefreshSize();
        }

        public void ContextLanguageChange()
        {
            _instanceWindow.ContextLanguageChange();
        }

        public void TriggerResolutionChange()
        {
            _instanceLayoutAlignment.ContextResolutionChange();
        }

        void UpdateMouseNavigation()
        {
            if (!_displayActive || !WindowManager.instance.inputProvider.inputEnabled ||
                _lastMousePosition == WindowManager.instance.inputProvider.mousePosition ||
                !WindowManager.instance.inputProvider.hasMouse || _instanceWindow == null
                || !_navigationActive)
            {
                return;
            }

            _lastMousePosition = WindowManager.instance.inputProvider.mousePosition;
            if (!_inElementInputMode)
            {
                _mouseActive = false;
                // if selectables are more than 1, do precise detection
                if (_instanceWindow.interactables.Count > 1)
                {
                    for (int i = 0; i < _instanceWindow.interactables.Count; i++)
                    {
                        (Vector2 bottomLeft, Vector2 topRight) = _instanceWindow.SelectableBound(i);
                        if (bottomLeft == Vector2.zero && topRight == Vector2.zero)
                            continue;
                        Vector2 bottomLeftDelta = _lastMousePosition - bottomLeft;
                        if (bottomLeftDelta.x <= 0 || bottomLeftDelta.y <= 0)
                            continue;
                        Vector2 topRightDelta = _lastMousePosition - topRight;
                        if (topRightDelta.x >= 0 || topRightDelta.y >= 0)
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
                }
                // Otherwise do window check alone
                else
                {
                    if (!IsMouseInWindow(_lastMousePosition))
                        return;
                    if (0 != _currentSelection)
                    {
                        _selectionUpdated = true;
                        currentSelectable?.SetFocus(false);
                        _currentSelection = 0;
                        currentSelectable?.SetFocus(true);
                    }

                    _mouseActive = true;
                }
            }
            // if input mode is active
            else
            {
                float fontSize = currentSelectable.parentWindow.setup.fontSize;
                if (currentSelectable is SliderUI slider)
                {
                    Vector2 leftArrowPosition = slider.cachedArrowPosition.Item1;
                    Vector2 rightArrowPosition = slider.cachedArrowPosition.Item2;
                    _hoverOnDecrease = false;
                    _hoverOnIncrease = false;
                    Vector2 leftArrowDelta = _lastMousePosition - leftArrowPosition;
                    Vector2 rightArrowDelta = _lastMousePosition - rightArrowPosition;
                    if (leftArrowDelta.sqrMagnitude < rightArrowDelta.sqrMagnitude &&
                        Mathf.Abs(leftArrowDelta.x) < fontSize && Mathf.Abs(leftArrowDelta.y) < fontSize)
                    {
                        _hoverOnDecrease = true;
                    }
                    else if (Mathf.Abs(rightArrowDelta.x) < fontSize && Mathf.Abs(rightArrowDelta.y) < fontSize)
                    {
                        _hoverOnIncrease = true;
                    }
                }

                if (currentSelectable is ScrollableTextUI scrollableTextUI)
                {
                    Vector2 upperArrowDelta = scrollableTextUI.cachedPosition.Item2 - _lastMousePosition;
                    Vector2 lowerArrowDelta = _lastMousePosition - scrollableTextUI.cachedPosition.Item1;
                    _hoverOnDecrease = false;
                    _hoverOnIncrease = false;
                    if (upperArrowDelta is { x: >= 0, y: >= 0 } && lowerArrowDelta is { x: >= 0, y: >= 0 })
                    {
                        if (upperArrowDelta.y < lowerArrowDelta.y &&
                            upperArrowDelta.y < fontSize * 0.75f)
                        {
                            _hoverOnDecrease = true;
                        }
                        else if (upperArrowDelta.y > lowerArrowDelta.y &&
                                 lowerArrowDelta.y < fontSize * 0.75f)
                        {
                            _hoverOnIncrease = true;
                        }
                    }
                }
            }
        }

        void UpdateWindowLocation()
        {
            if (!_windowElementLocationCached)
            {
                // SetOpacity(1, true);
                _windowElementLocationCached = true;
                _instanceWindow.UpdateElementsAndWindowPosition();
            }
        }

        public void LinkInput()
        {
            _linkFrame = Time.frameCount;
            WindowManager.instance.LinkInputTarget(this);
        }

        public void UnlinkInput()
        {
            WindowManager.instance.UnlinkInput(this);
        }

        public void ResetSelection()
        {
            foreach (ButtonUI element in _instanceWindow.interactables)
            {
                element.SetFocus(false);
            }

            _currentSelection = -1;
            if (GetSelectable(_currentSelection) == null && _instanceWindow.interactables.Count > 0)
            {
                _currentSelection = 0;
            }

            GetSelectable(_currentSelection)?.SetFocus(true);
        }

        // Used to set all windows in unselected state
        public void ClearSelection()
        {
            foreach (ButtonUI element in _instanceWindow.interactables)
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
            _instanceWindow.ClearElementsFocus();
        }

        public void ClearWindowFocus()
        {
            _instanceWindow.ClearWindowFocus();
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
                UpdateSelectionProcess();
                if (!_selectionUpdated && !mouseScrollOverride)
                {
                    WindowManager.instance.CheckClosestDirectionalMatch(this,
                        currentSelectable.cachedPosition.Item1, _move.normalized);
                }

                _move = Vector2.zero;
            }

            if (mouseScrollOverride)
            {
                _move = Vector2.zero;
                _mouseScroll = Vector2.zero;
            }
        }

        void UpdateSelectionProcess()
        {
            if (!_inElementInputMode)
            {
                currentSelectable?.SetFocus(false);
                int yBefore = _currentSelection;
                int offset = Mathf.RoundToInt(_move.y) switch
                {
                    1 => -1,
                    -1 => 1,
                    _ => 0
                };
                _currentSelection += offset;
                int count = _instanceWindow.interactables.Count;
                if (_menuSetup.allowCycle)
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
                // update with opposite direction
                int offset = _menuSetup.slideDirection switch
                {
                    UISystemSliderDirection.X => Mathf.RoundToInt(_move.x) switch
                    {
                        1 => 1,
                        -1 => -1,
                        _ => 0
                    },
                    UISystemSliderDirection.Y => Mathf.RoundToInt(_move.y) switch
                    {
                        1 => -1,
                        -1 => 1,
                        _ => 0
                    },
                    _ => 0
                };
                if (currentSelectable is ScrollableTextUI scrollableText)
                {
                    if (_move.y != 0)
                    {
                        offset = Mathf.RoundToInt(_move.y) switch
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
            if (!_windowElementLocationCached)
                UpdateWindowLocation();
            float minDistance = Mathf.Infinity;
            currentSelectable?.SetFocus(false);
            if (_instanceWindow.interactables.Count == 0)
                return;
            for (int i = 0; i < _instanceWindow.interactables.Count; i++)
            {
                Vector2 selectableLocation = _instanceWindow.interactables[i].cachedPosition.Item1;
                float distance = (selectableLocation - referenceLocation).sqrMagnitude;
                if (distance < minDistance)
                {
                    minDistance = distance;
                    _currentSelection = i;
                }
            }

            _currentSelection = Mathf.Clamp(_currentSelection, 0, _instanceWindow.interactables.Count - 1);

            currentSelectable?.SetFocus(true);
        }

        public ButtonUIDoubleConfirm AddDoubleConfirmButton(string elementName, Action action = null)
        {
            return AddDoubleConfirmButton(elementName, _menuStyling.windowSetup, action);
        }

        public ButtonUIDoubleConfirm AddDoubleConfirmButton(string elementName, WindowSetup setup,
            Action action = null)
        {
            if (_instanceWindow == null)
                NewWindow(elementName, setup);
            ButtonUIDoubleConfirm button = AddDoubleConfirmButton(elementName, _instanceWindow, action);
            _instanceWindow.AutoResize();
            return button;
        }

        public ButtonUIDoubleConfirm AddDoubleConfirmButton(string elementName, WindowUI window,
            Action action = null) => window.AddDoubleConfirmButton(elementName, action);

        public ButtonUI AddButton(string elementName, Action action = null)
        {
            return AddButton(elementName, _menuStyling.windowSetup, action);
        }

        public ButtonUI AddButton(string elementName, WindowSetup setup, Action action = null)
        {
            if (_instanceWindow == null)
                NewWindow(elementName, setup);
            ButtonUI button = _instanceWindow.AddButton(elementName, action);
            _instanceWindow.AutoResize();
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
            if (_instanceWindow == null)
                NewWindow(elementName, setup);
            ScrollableTextUI button = _instanceWindow.AddScrollableText(elementName, action);
            _instanceWindow.AutoResize();
            return button;
        }

        public ScrollableTextUI AddScrollableText(string elementName, WindowUI window, Action action = null) =>
            window.AddScrollableText(elementName, action);

        public ButtonUIWithContent AddButtonWithContent(string elementName, Action action = null)
        {
            return AddButtonWithContent(elementName, _menuStyling.windowSetup, action);
        }

        public ButtonUIWithContent AddButtonWithContent(string elementName, WindowSetup setup, Action action = null)
        {
            if (_instanceWindow == null)
                NewWindow(elementName, setup);
            ButtonUIWithContent button = _instanceWindow.AddButtonWithContent(elementName, action);
            _instanceWindow.AutoResize();
            return button;
        }

        public ButtonUIWithContent AddButtonWithContent(string elementName, WindowUI window, Action action = null) =>
            window.AddButtonWithContent(elementName, action);

        public SingleSelectionUI<T> AddSingleSelection<T>(string elementName, Action<T> action = null)
        {
            return AddSingleSelection(elementName, _menuStyling.windowSetup, action);
        }

        public SingleSelectionUI<T> AddSingleSelection<T>(string elementName, WindowSetup setup,
            Action<T> action = null)
        {
            if (_instanceWindow == null)
                NewWindow(elementName, setup);
            SingleSelectionUI<T> button = _instanceWindow.AddSingleSelection(elementName, action);
            _instanceWindow.AutoResize();
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
            if (_instanceWindow == null)
                NewWindow(elementName, setup);
            TextInputUI button = _instanceWindow.AddTextInput(elementName, action);
            _instanceWindow.AutoResize();
            return button;
        }

        public TextInputUI AddTextInput(string elementName, WindowUI window, Action<string> action = null) =>
            window.AddTextInput(elementName, action);

        public TextInputUIWithPrediction AddTextInputNoLabelWithPrediction(string elementName,
            WindowUI window,
            Action<string> action = null) => window.AddTextInputNoLabelWithPrediction(elementName, action);

        public ToggleUI AddToggle(string elementName, float font = 30f, Action<bool> action = null)
        {
            return AddToggle(elementName, _menuStyling.windowSetup, font, action);
        }

        public ToggleUI AddToggle(string elementName, WindowSetup setup, float font = 30f,
            Action<bool> action = null)
        {
            if (_instanceWindow == null)
                NewWindow(elementName, setup);
            ToggleUI toggle = _instanceWindow.AddToggle(elementName, action);
            _instanceWindow.AutoResize();
            return toggle;
        }

        public ToggleUI AddToggle(string elementName, WindowUI window, Action<bool> action = null) =>
            window.AddToggle(elementName, action);

        public ToggleUIWithContent AddToggleWithContent(string elementName, Action<bool> action = null)
        {
            return AddToggleWithContent(elementName, _menuStyling.windowSetup, action);
        }

        public ToggleUIWithContent AddToggleWithContent(string elementName, WindowSetup setup,
            Action<bool> action = null)
        {
            if (_instanceWindow == null)
                NewWindow(elementName, setup);
            ToggleUIWithContent toggle = _instanceWindow.AddToggleWithContent(elementName, action);
            _instanceWindow.AutoResize();
            return toggle;
        }

        public ToggleUIWithContent AddToggleWithContent(string elementName, WindowUI window,
            Action<bool> action = null) => window.AddToggleWithContent(elementName, action);
        
        public SliderUI AddSlider(string elementName, Action<int> action = null)
        {
            return AddSlider(elementName, _menuStyling.windowSetup, action);
        }

        public SliderUI AddSlider(string elementName, WindowSetup setup, Action<int> action = null)
        {
            if (_instanceWindow == null)
                NewWindow(elementName, setup);
            SliderUI slider = _instanceWindow.AddSlider(elementName, action);
            _instanceWindow.AutoResize();
            return slider;
        }

        public SliderUI AddSlider(string elementName, WindowUI window, Action<int> action = null) =>
            window.AddSlider(elementName, action);

        public SliderUIChoice<T> AddSliderWithChoice<T>(string elementName, Action<T> action = null)
        {
            return AddSliderWithChoice<T>(elementName, _menuStyling.windowSetup, action);
        }

        public SliderUIChoice<T> AddSliderWithChoice<T>(string elementName, WindowSetup setup,
            Action<T> action = null)
        {
            if (_instanceWindow == null)
                NewWindow(elementName, setup);
            SliderUIChoice<T> slider = _instanceWindow.AddSliderWithChoice<T>(elementName, action);
            _instanceWindow.AutoResize();
            return slider;
        }

        public SliderUIChoice<T>
            AddSliderWithChoice<T>(string elementName, WindowUI window, Action<T> action = null) =>
            window.AddSliderWithChoice<T>(elementName, action);

        bool BaseConfirmAction()
        {
            bool result = currentSelectable switch
            {
                SliderUI slider => SliderAction(slider, !_inElementInputMode),
                ToggleUI toggle => ToggleAction(toggle),
                ButtonUIDoubleConfirm button => DoubleConfirmAction(button, true),
                ButtonUICountable button => ButtonWithCountAction(button, true),
                TextInputUI textInput => TextInputAction(textInput),
                SingleSelectionUI<int> selection => SingleSelectionAction(selection),
                ScrollableTextUI scrollableText => ScrollableTextAction(scrollableText, !_inElementInputMode),
                not null => ButtonAction(currentSelectable),
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
        }

        void ISelectionInputTarget.SetSelection(int count)
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

        void IMenuInputTarget.OnMouseConfirm()
        {
            MouseConfirmSelection();
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
                currentSelectable.count -= 1;
            }
            else if (_hoverOnIncrease)
            {
                currentSelectable.count += 1;
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

        bool ButtonWithCountAction(ButtonUICountable button, bool increment)
        {
            if (increment)
                button.count++;
            else
                button.count--;
            return true;
        }

        bool DoubleConfirmAction(ButtonUIDoubleConfirm button, bool confirm)
        {
            if (confirm)
            {
                button.TriggerAction();
            }
            else
            {
                button.CancelAwait();
            }

            return true;
        }

        bool TextInputAction(TextInputUI textInput)
        {
            CloseMenu();
            WindowManager.instance.GetTextInput(this,
                textInput);
            textInput.SetInput(true);
            return true;
        }

        bool SingleSelectionAction<T>(SingleSelectionUI<T> selection)
        {
            CloseMenu();
            WindowManager.instance.GetSingleSelectionInput(this,
                selection.choiceListContent, selection.count);
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

        bool SliderAction(SliderUI slider, bool setInput)
        {
            if (setInput && !_inElementInputMode)
            {
                if (slider.SetInput(true))
                {
                    ClearWindowLocation();
                    _inElementInputMode = true;
                }
            }
            else if (!setInput && _inElementInputMode)
            {
                _inElementInputMode = false;
                slider.SetInput(false);
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
                if (_menuSetup.cancelOutAllowed)
                    return CancelOut();
                return false;
            }

            bool result = currentSelectable switch
            {
                SliderUI slider when _inElementInputMode => SliderAction(slider, false),
                ButtonUICountable button => ButtonWithCountAction(button, false),
                ButtonUIDoubleConfirm { awaitConfirm: true } button => DoubleConfirmAction(button, false),
                ScrollableTextUI scrollableText when _inElementInputMode => ScrollableTextAction(scrollableText, false),
                _ when _menuSetup.cancelOutAllowed => CancelOut(),
                _ => false
            };
            return result;
        }

        public void UpdateWindows()
        {
            _instanceWindow.InvokeUpdate();
        }

        public void ClearWindowLocation(float delay = .1f)
        {
            // SetOpacity(0.1f, true);
            if (_nextNavigationUpdate < Time.unscaledTime + delay)
            {
                DelayInput(delay);
            }

            _instanceWindow.ClearCachedPosition();

            _windowElementLocationCached = false;
        }

        public bool OpenMenu(bool enableNavigation)
        {
            return OpenMenu(null, enableNavigation);
        }

        public bool OpenMenu(CompositeMenuMono sourceMenu)
        {
            return OpenMenu(sourceMenu, _menuSetup.allowNavigationOnOpen);
        }

        public bool OpenMenu(CompositeMenuMono sourceMenu, bool enableNavigation)
        {
            _displayActive = true;
            _selectionUpdated = true;
            if (_menuSetup.allowNavigationOnOpen)
                DelayInput(_menuSetup.menuOpenInputDelay);
            ResetHold();
            switch (_menuSetup.resetOnOpen)
            {
                case UISystemResetOnOpenBehavior.Always:
                    ResetSelection();
                    break;
            }

            _mouseActive = false;
            currentSelectable?.SetFocus(true);
            _instanceWindow.SetActive(true);

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
                if (currentSelectable is SliderUI slider)
                    slider.SetInput(false);
                if (currentSelectable is ScrollableTextUI scrollableText)
                    scrollableText.SetScrolling(false);
            }

            currentSelectable?.SetFocus(false);

            _displayActive = false;
            _instanceWindow.SetActive(false);

            return true;
        }

        void OnSelectionUpdate()
        {
            _instanceWindow.TriggerSelectionUpdate();
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
                LinkInput();
            }
        }
    }
}