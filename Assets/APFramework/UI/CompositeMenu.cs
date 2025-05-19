using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using ChosenConcept.APFramework.Interface.Framework.Element;
using Object = UnityEngine.Object;

namespace ChosenConcept.APFramework.Interface.Framework
{
    [Serializable]
    public class CompositeMenu : IMenuInputTarget, ITextInputTarget, ISelectionInputTarget
    {
        [Header("Debug View")] [SerializeField]
        string _menuName;

        [SerializeField] MenuSetup _menuSetup;
        [SerializeField] MenuStyling _menuStyling;
        [SerializeField] List<WindowUI> _windowInstances = new();
        [SerializeField] List<LayoutAlignment> _layoutAlignmentInstances = new();
        [SerializeField] bool _displayActive;
        [SerializeField] bool _navigationActive;
        [SerializeField] int _linkFrame = -1;
        [SerializeField] float _nextNavigationUpdate = Mathf.Infinity;
        [SerializeField] Vector2Int _currentSelection = Vector2Int.zero;
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
        [SerializeField] bool _movingWindow;

        Action _menuCloseAction;

        bool windowPositionCached => _windowInstances.All(x => x.positionCached);

        public ButtonUI currentSelectable
        {
            get
            {
                if (GetSelectable(_currentSelection) == null)
                    return null;
                return GetSelectable(_currentSelection);
            }
        }

        public WindowUI currentWindow
        {
            get
            {
                if (_currentSelection[0] < 0 || _currentSelection[0] >= _windowInstances.Count)
                    return null;
                return _windowInstances[_currentSelection[0]];
            }
        }

        public string menuTag => $"Interface.{_menuName}";
        public bool isDisplayActive => _displayActive;

        public CompositeMenu(string name)
        {
            _menuName = name;
            _menuSetup = MenuSetup.defaultSetup;
            _menuStyling = MenuStyling.defaultStyling;
        }

        public CompositeMenu(string name, MenuSetup menuSetup, WindowSetup windowSetup, LayoutSetup layoutSetup)
        {
            _menuName = name;
            _menuSetup = menuSetup;
            _menuStyling = MenuStyling.defaultStyling;
            _menuStyling.windowSetup = windowSetup;
            _menuStyling.layoutSetup = layoutSetup;
        }

        public bool ExistsSelectable(int i) => _windowInstances[0].interactables.Count > i && i >= 0;

        public bool ExistsSelectable(int i, int j) =>
            _windowInstances.Count > i && i >= 0 && _windowInstances[i].interactables.Count > j && j >= 0;

        public List<ButtonUI> GetSelectableList(int i = 0) => _windowInstances[i].interactables;
        public ButtonUI GetSelectable(int i) => _windowInstances[0].interactables[i];
        public ButtonUI GetSelectable(int i, int j) => _windowInstances[i].interactables[j];

        public ButtonUI GetSelectable(Vector2Int select) =>
            select.x >= 0 && select.x < _windowInstances.Count && select.y >= 0 &&
            select.y < _windowInstances[select.x].interactables.Count
                ? _windowInstances[select.x].interactables[select.y]
                : null;

        public virtual void UpdateMenu()
        {
            if (!_displayActive)
                return;
            if (!_navigationActive && Time.unscaledTime >= _nextNavigationUpdate)
                _navigationActive = true;
            UpdateNavigation();
        }

        void UpdateNavigation()
        {
            if (_movingWindow)
            {
                currentWindow.Move(WindowManager.instance.inputProvider.mouseDelta);
                return;
            }

            if (!_windowInstances.Any(x => x.interactables.Any()))
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
                TriggerSelectionUpdate();
            }
        }

        public void ForceUpdateDisplayContent()
        {
            foreach (WindowUI window in _windowInstances)
            {
                window.InvokeUpdate();
            }
        }

        public void SetOpacity(float opacity)
        {
            foreach (WindowUI window in _windowInstances)
            {
                window.SetOpacity(opacity);
            }
        }

        public LayoutAlignment InitializeNewLayout()
        {
            LayoutAlignment layout =
                WindowManager.instance.InstantiateLayout(_menuStyling.layoutSetup,
                    menuTag);
            _layoutAlignmentInstances.Add(layout);
            return layout;
        }

        /// <summary>
        /// A simple method to spawn window
        /// </summary>
        public WindowUI NewWindow(string windowName, WindowSetup setup)
        {
            LayoutAlignment layout = InitializeNewLayout();
            WindowUI window = WindowManager.instance.NewWindow(windowName, layout, setup, menuTag);
            _windowInstances.Add(window);
            return window;
        }

        /// <summary>
        /// A simple method to spawn window with designated layout
        /// </summary>
        public WindowUI NewWindow(string windowName, LayoutAlignment layout, WindowSetup setup)
        {
            WindowUI window = WindowManager.instance.NewWindow(windowName, layout, setup, menuTag);
            _windowInstances.Add(window);
            return window;
        }

        public void RemoveElement(WindowElement element)
        {
            element.Remove();
        }

        public void RemoveWindowsAndLayoutGroups()
        {
            ClearWindows(true);
            foreach (LayoutAlignment alignment in _layoutAlignmentInstances)
            {
                Object.Destroy(alignment.gameObject);
            }

            _layoutAlignmentInstances.Clear();
        }

        public void ClearWindows(bool removeWindow)
        {
            ClearWindowLocation();
            _currentSelection = Vector2Int.zero;
            foreach (WindowUI window in _windowInstances)
            {
                ClearWindow(window, false);
                if (removeWindow)
                    WindowManager.instance.CloseWindow(window);
            }

            _windowInstances.Clear();
        }

        public void ClearWindow(WindowUI window, bool removeWindow)
        {
            window.ClearElements();
            if (removeWindow)
            {
                _windowInstances.Remove(window);
                WindowManager.instance.CloseWindow(window);
            }
        }

        public void SetAllWindowLocalizedByTag()
        {
            foreach (WindowUI window in _windowInstances)
            {
                window.SetLocalizedByTag();
            }
        }

        public void AutoResizeAllWindows(int extraWidth = 0)
        {
            foreach (WindowUI window in _windowInstances)
            {
                window.AutoResize(extraWidth);
            }
        }

        public void SyncAutoResizeAllWindows(int extraWidth = 0)
        {
            int maxWidth = 0;
            List<int> autoResizeWidths = new List<int>();
            for (int i = 0; i < _windowInstances.Count; i++)
            {
                int autoResizeWidth = _windowInstances[i].GetAutoResizeWidth(0);
                autoResizeWidths.Add(autoResizeWidth);
                if (autoResizeWidth > maxWidth)
                    maxWidth = autoResizeWidth;
            }

            for (int i = 0; i < _windowInstances.Count; i++)
            {
                _windowInstances[i].AutoResize(maxWidth - autoResizeWidths[i] + extraWidth);
            }
        }

        public void Refresh()
        {
            foreach (WindowUI window in _windowInstances)
            {
                window.InvokeUpdate();
                window.RefreshSize();
            }
        }

        public void ContextLanguageChange()
        {
            foreach (WindowUI window in _windowInstances)
            {
                window.ContextLanguageChange();
            }
        }

        public void TriggerResolutionChange()
        {
            foreach (LayoutAlignment layout in _layoutAlignmentInstances)
            {
                layout.ContextResolutionChange();
            }
        }

        public void MoveWindowToIndex(WindowUI window, int index)
        {
            _windowInstances.Remove(window);
            _windowInstances.Insert(index, window);
            window.layoutAlignment.MoveWindowToIndex(window, index);
        }

        void UpdateMouseNavigation()
        {
            if (!WindowManager.instance.inputProvider.inputEnabled ||
                _lastMousePosition == WindowManager.instance.inputProvider.mousePosition ||
                !WindowManager.instance.inputProvider.hasMouse || _windowInstances.Count == 0
                || !_navigationActive)
            {
                return;
            }

            _lastMousePosition = WindowManager.instance.inputProvider.mousePosition;
            _mouseActive = false;
            if (!_inElementInputMode)
            {
                Vector2Int previousSelection = _currentSelection;
                int windowCount = _windowInstances.Count;
                currentSelectable?.SetFocus(false);
                for (int i = 0; i < windowCount; i++)
                {
                    if (!_windowInstances[i].ContainsPosition(_lastMousePosition))
                    {
                        _windowInstances[i].SetFocus(false);
                        continue;
                    }

                    _currentSelection[0] = i;
                    _windowInstances[i].SetFocus(true);
                    int interactableCount = _windowInstances[i].interactables.Count;
                    if (interactableCount > 1)
                    {
                        for (int j = 0; j < interactableCount; j++)
                        {
                            if (!_windowInstances[i].InteractableContainsPosition(j, _lastMousePosition))
                                continue;
                            _currentSelection[1] = j;
                            _mouseActive = true;
                            break;
                        }
                    }

                    if (interactableCount == 1)
                    {
                        _currentSelection[1] = 0;
                        _mouseActive = true;
                    }

                    if (interactableCount == 0)
                    {
                        _currentSelection[1] = -1;
                    }
                }

                if (_mouseActive)
                {
                    currentSelectable?.SetFocus(true);
                }
                else
                {
                    _currentSelection[1] = -1;
                }

                if (previousSelection != _currentSelection)
                {
                    _selectionUpdated = true;
                }

                if (_windowInstances.All(x => !x.isFocused))
                {
                    ClearSelection();
                }
            }
            // if input mode is active
            else
            {
                if (currentSelectable is SliderUI slider)
                {
                    (_hoverOnDecrease, _hoverOnIncrease) = slider.HoverOnArrow(_lastMousePosition);
                }

                if (currentSelectable is ScrollableTextUI scrollableTextUI)
                {
                    (_hoverOnDecrease, _hoverOnIncrease) = scrollableTextUI.HoverOnArrow(_lastMousePosition);
                }

                if (_hoverOnDecrease || _hoverOnIncrease)
                    _mouseActive = true;
            }
        }

        void UpdateWindowLocation()
        {
            if (!windowPositionCached)
            {
                foreach (WindowUI window in _windowInstances)
                {
                    window.UpdateElementsAndWindowPosition();
                }
            }
        }

        public void LinkInput()
        {
            _navigationActive = true;
            _linkFrame = Time.frameCount;
            WindowManager.instance.LinkInputTarget(this);
        }

        public void UnlinkInput()
        {
            _navigationActive = false;
            WindowManager.instance.UnlinkInput(this);
        }

        public void ResetSelection()
        {
            foreach (WindowUI window in _windowInstances)
            {
                foreach (ButtonUI element in window.interactables)
                {
                    element.SetFocus(false);
                }
            }

            _currentSelection = Vector2Int.zero;
            if (currentSelectable == null)
            {
                for (int i = 0; i < _windowInstances.Count; i++)
                {
                    if (!_windowInstances[i].canNavigate)
                        continue;
                    _currentSelection = new Vector2Int(i, 0);
                    break;
                }
            }

            currentSelectable?.SetFocus(true);
            currentWindow?.SetFocus(true);
        }

        // Used to set all windows in unselected state
        public void ClearSelection()
        {
            foreach (WindowUI window in _windowInstances)
            {
                foreach (ButtonUI element in window.interactables)
                {
                    element.SetFocus(false);
                }
            }

            _currentSelection = new Vector2Int(-1, -1);
        }

        public void SetSelection(int x = 0, int y = 0)
        {
            if (_windowInstances.Count > _currentSelection.x &&
                _windowInstances[_currentSelection.x].interactables.Count > _currentSelection.y)
                GetSelectable(_currentSelection).SetFocus(false);
            _currentSelection = new Vector2Int(x, y);
            if (_windowInstances.Count > _currentSelection.x &&
                _windowInstances[_currentSelection.x].interactables.Count > _currentSelection.y)
                GetSelectable(_currentSelection).SetFocus(true);
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
            }
        }

        public void ClearElementsFocus()
        {
            foreach (WindowUI window in _windowInstances)
            {
                window.ClearElementsFocus();
            }
        }

        public void ClearWindowFocus()
        {
            foreach (WindowUI window in _windowInstances)
            {
                window.ClearWindowFocus();
            }
        }

        protected virtual void UpdateSelection()
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
            if (_currentSelection.x < 0 || _currentSelection.y < 0)
            {
                ResetSelection();
            }
            else
            {
                UpdateSelectionByMovement(_move.normalized);
            }

            if (mouseScrollOverride)
            {
                _move = Vector2.zero;
                _mouseScroll = Vector2.zero;
            }
        }

        protected virtual void UpdateSelectionByMovement(Vector2 move)
        {
            if (!_inElementInputMode)
            {
                currentWindow?.SetFocus(false);
                currentSelectable?.SetFocus(false);
                // First determine if movement within window happens
                float minScore = Mathf.Infinity;
                int nearestInteractableIndex = _currentSelection[1];
                for (int i = 0; i < currentWindow.interactables.Count; i++)
                {
                    if (i == _currentSelection[1])
                        continue;
                    Vector2 selectableLocation = currentWindow.interactables[i].cachedPosition.Item1;
                    Vector2 direction = selectableLocation - currentSelectable.cachedPosition.Item1;
                    float distance = direction.sqrMagnitude;
                    Vector2 directionNormalized = direction.normalized;
                    float dotProduct = Vector2.Dot(move, directionNormalized);
                    if (distance < minScore && dotProduct > .5f)
                    {
                        minScore = distance;
                        nearestInteractableIndex = i;
                    }
                }

                if (nearestInteractableIndex != _currentSelection[1])
                {
                    _currentSelection[1] = nearestInteractableIndex;
                    _selectionUpdated = true;
                }

                // Check for movement between windows if no changes were made within the same window
                if (!_selectionUpdated && _windowInstances.Count(x => x.canNavigate) > 1)
                {
                    minScore = Mathf.Infinity;
                    int nearestWindowIndex = _currentSelection[0];
                    for (int i = 0; i < _windowInstances.Count; i++)
                    {
                        if (i == _currentSelection[0] || !_windowInstances[i].canNavigate)
                            continue;
                        Vector2 windowCenter = _windowInstances[i].cachedCenter;
                        Vector2 direction = windowCenter - currentWindow.cachedCenter;
                        float distance = direction.magnitude;
                        Vector2 directionNormalized = direction / distance;
                        float dotProduct = Vector2.Dot(move, directionNormalized);
                        if (dotProduct < .3f)
                            continue;
                        // Favoring both shorter distance and better directional match
                        float score = distance * (2 - dotProduct);
                        if (score < minScore)
                        {
                            minScore = score;
                            nearestWindowIndex = i;
                        }
                    }

                    if (nearestWindowIndex != _currentSelection[0])
                    {
                        nearestInteractableIndex = -1;
                        minScore = Mathf.Infinity;
                        for (int i = 0; i < _windowInstances[nearestWindowIndex].interactables.Count; i++)
                        {
                            Vector2 selectableLocation = _windowInstances[nearestWindowIndex].interactables[i]
                                .cachedPosition.Item1;
                            Vector2 direction = selectableLocation - currentSelectable.cachedPosition.Item1;
                            float distance = direction.sqrMagnitude;
                            if (distance < minScore)
                            {
                                minScore = distance;
                                nearestInteractableIndex = i;
                            }
                        }

                        _currentSelection[0] = nearestWindowIndex;
                        _currentSelection[1] = nearestInteractableIndex;
                        _selectionUpdated = true;
                    }
                }

                // If no movement within and between window, we prioritize cycling within window if allowed
                if (!_selectionUpdated && _menuSetup.allowCycleWithinWindow && Mathf.Abs(_move.y) >= Mathf.Abs(_move.x))
                {
                    float maxScore = Mathf.NegativeInfinity;
                    int farthestInteractableIndex = _currentSelection[1];
                    for (int i = 0; i < currentWindow.interactables.Count; i++)
                    {
                        if (i == _currentSelection[1])
                            continue;
                        Vector2 selectableLocation = currentWindow.interactables[i].cachedPosition.Item1;
                        Vector2 direction = selectableLocation - currentSelectable.cachedPosition.Item1;
                        float distance = direction.sqrMagnitude;
                        if (distance > maxScore)
                        {
                            maxScore = distance;
                            farthestInteractableIndex = i;
                        }
                    }

                    if (farthestInteractableIndex != _currentSelection[1])
                    {
                        _currentSelection[1] = farthestInteractableIndex;
                        _selectionUpdated = true;
                    }
                }

                // If all things failed, cycling between windows will be checked
                if (!_selectionUpdated && _menuSetup.allowCycleBetweenWindows &&
                    _windowInstances.Count(x => x.canNavigate) > 1)
                {
                    float maxScore = Mathf.NegativeInfinity;
                    int farthestWindowIndex = _currentSelection[0];
                    for (int i = 0; i < _windowInstances.Count; i++)
                    {
                        if (i == _currentSelection[0] || !_windowInstances[i].canNavigate)
                            continue;
                        Vector2 windowCenter = _windowInstances[i].cachedCenter;
                        Vector2 direction = windowCenter - currentWindow.cachedCenter;
                        float distance = direction.magnitude;
                        Vector2 directionNormalized = direction / distance;
                        float dotProduct = Vector2.Dot(-move, directionNormalized);
                        if (dotProduct < .3f)
                            continue;
                        // Favoring both longer distance and better directional match
                        float score = distance * dotProduct;
                        if (score > maxScore)
                        {
                            maxScore = score;
                            farthestWindowIndex = i;
                        }
                    }

                    if (farthestWindowIndex != _currentSelection[0])
                    {
                        _currentSelection[0] = farthestWindowIndex;
                        _currentSelection[1] = 0;
                        _selectionUpdated = true;
                    }
                }

                currentSelectable?.SetFocus(true);
                currentWindow?.SetFocus(true);
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

            DelayInput(.005f);
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
                UpdateWindowLocation();
            float minDistance = Mathf.Infinity;
            currentSelectable?.SetFocus(false);
            foreach (WindowUI window in _windowInstances)
            {
                if (!window.canNavigate)
                    continue;
                for (int i = 0; i < window.interactables.Count; i++)
                {
                    Vector2 selectableLocation = window.interactables[i].cachedPosition.Item1;
                    float distance = (selectableLocation - referenceLocation).sqrMagnitude;
                    if (distance < minDistance)
                    {
                        minDistance = distance;
                        _currentSelection[0] = _windowInstances.IndexOf(window);
                        _currentSelection[1] = i;
                    }
                }

                _currentSelection[1] = Mathf.Clamp(_currentSelection[1], 0, window.interactables.Count - 1);
            }

            currentSelectable?.SetFocus(true);
        }

        public void RefocusAtNearestElement(Vector2 referenceLocation, int windowIndex)
        {
            if (!_displayActive)
                return;
            if (!windowPositionCached)
                UpdateWindowLocation();
            float minDistance = Mathf.Infinity;
            currentSelectable?.SetFocus(false);
            WindowUI window = _windowInstances[windowIndex];
            if (window.canNavigate)
            {
                for (int i = 0; i < window.interactables.Count; i++)
                {
                    Vector2 selectableLocation = window.interactables[i].cachedPosition.Item1;
                    float distance = (selectableLocation - referenceLocation).sqrMagnitude;
                    if (distance < minDistance)
                    {
                        minDistance = distance;
                        _currentSelection[0] = windowIndex;
                        _currentSelection[1] = i;
                    }
                }
            }

            _currentSelection[1] = Mathf.Clamp(_currentSelection[1], 0, window.interactables.Count - 1);
            currentSelectable?.SetFocus(true);
        }

        public void RefocusAtNearestElement(float yLocation, int windowIndex)
        {
            if (!_displayActive)
                return;
            if (!windowPositionCached)
                UpdateWindowLocation();
            float minDistance = Mathf.Infinity;
            currentSelectable?.SetFocus(false);
            WindowUI window = _windowInstances[windowIndex];
            if (window.canNavigate)
            {
                for (int i = 0; i < window.interactables.Count; i++)
                {
                    Vector2 selectableLocation = window.interactables[i].cachedPosition.Item1;
                    float distance = Mathf.Abs(selectableLocation.y - yLocation);
                    if (distance < minDistance)
                    {
                        minDistance = distance;
                        _currentSelection[0] = windowIndex;
                        _currentSelection[1] = i;
                    }
                }
            }

            _currentSelection[1] = Mathf.Clamp(_currentSelection[1], 0, window.interactables.Count - 1);
            currentSelectable?.SetFocus(true);
        }

        /// <summary>
        /// A simple method to spawn single text UI element window
        /// </summary>
        public TextUI AddText(string elementName, int length = 0)
        {
            WindowUI window = NewWindow(elementName, _menuStyling.windowSetup);
            TextUI text;
            if (length == 0)
                text = window.AddText(elementName);
            else
                text = window.AddText(TextUtility.PlaceHolder(length));
            window.AutoResize();
            return text;
        }

        /// <summary>
        /// A simple method to spawn text UI to pre-initialized Window
        /// </summary>
        public void AddGap(WindowUI window)
        {
            TextUI text = AddText("Blank", window);
            text.SetLabel("　");
        }

        public TextUI AddText(string elementName, WindowUI window, int length = 0)
        {
            TextUI text;
            if (length == 0)
                text = window.AddText(elementName);
            else
                text = window.AddText(TextUtility.PlaceHolder(length));
            return text;
        }

        public TextUI AddText(string elementName, LayoutAlignment layout, int length = 0)
        {
            return AddText(elementName, layout, _menuStyling.windowSetup, length);
        }

        public TextUI AddText(string elementName, LayoutAlignment layout, WindowSetup setup, int length = 0)
        {
            WindowUI window = NewWindow(elementName, layout, setup);
            TextUI text;
            if (length == 0)
                text = window.AddText(elementName);
            else
                text = window.AddText(TextUtility.PlaceHolder(length));
            window.AutoResize();
            return text;
        }

        public ButtonUIDoubleConfirm AddDoubleConfirmButton(string elementName, Action action = null)
        {
            return AddDoubleConfirmButton(elementName, _menuStyling.windowSetup, action);
        }

        public ButtonUIDoubleConfirm AddDoubleConfirmButton(string elementName, WindowSetup setup,
            Action action = null)
        {
            WindowUI window = NewWindow(elementName, setup);
            ButtonUIDoubleConfirm button = AddDoubleConfirmButton(elementName, window, action);
            window.AutoResize();
            return button;
        }

        public ButtonUIDoubleConfirm AddDoubleConfirmButton(string elementName, LayoutAlignment layout,
            Action action = null)
        {
            return AddDoubleConfirmButton(elementName, _menuStyling.windowSetup, layout, action);
        }

        public ButtonUIDoubleConfirm AddDoubleConfirmButton(string elementName, WindowSetup setup,
            LayoutAlignment layout,
            Action action = null)
        {
            WindowUI window = NewWindow(elementName, layout, setup);
            ButtonUIDoubleConfirm button = AddDoubleConfirmButton(elementName, window, action);
            window.AutoResize();
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
            WindowUI window = NewWindow(elementName, setup);
            ButtonUI button = window.AddButton(elementName, action);
            window.AutoResize();
            return button;
        }

        public ButtonUI AddButton(string elementName, LayoutAlignment layout, Action action = null)
        {
            return AddButton(elementName, layout, _menuStyling.windowSetup, action);
        }

        public ButtonUI AddButton(string elementName, LayoutAlignment layout, WindowSetup setup,
            Action action = null)
        {
            WindowUI window = NewWindow(elementName, layout, setup);
            ButtonUI button = window.AddButton(elementName, action);
            window.AutoResize();
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
            WindowUI window = NewWindow(elementName, setup);
            ScrollableTextUI button = window.AddScrollableText(elementName, action);
            window.AutoResize();
            return button;
        }

        public ScrollableTextUI AddScrollableText(string elementName, LayoutAlignment layout, Action action = null)
        {
            return AddScrollableText(elementName, layout, _menuStyling.windowSetup, action);
        }

        public ScrollableTextUI AddScrollableText(string elementName, LayoutAlignment layout, WindowSetup setup,
            Action action = null)
        {
            WindowUI window = NewWindow(elementName, layout, setup);
            ScrollableTextUI button = window.AddScrollableText(elementName, action);
            window.AutoResize();
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
            WindowUI window = NewWindow(elementName, setup);
            ButtonUIWithContent button = window.AddButtonWithContent(elementName, action);
            window.AutoResize();
            return button;
        }

        public ButtonUIWithContent AddButtonWithContent(string elementName, LayoutAlignment layout,
            Action action = null)
        {
            return AddButtonWithContent(elementName, layout, _menuStyling.windowSetup, action);
        }

        public ButtonUIWithContent AddButtonWithContent(string elementName, LayoutAlignment layout,
            WindowSetup setup,
            Action action = null)
        {
            WindowUI window = NewWindow(elementName, layout, setup);
            ButtonUIWithContent button = window.AddButtonWithContent(elementName, action);
            window.AutoResize();
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
            WindowUI window = NewWindow(elementName, setup);
            SingleSelectionUI<T> button = window.AddSingleSelection(elementName, action);
            window.AutoResize();
            return button;
        }

        public SingleSelectionUI<T> AddSingleSelection<T>(string elementName, LayoutAlignment layout,
            Action<T> action = null)
        {
            return AddSingleSelection(elementName, layout, _menuStyling.windowSetup, action);
        }

        public SingleSelectionUI<T> AddSingleSelection<T>(string elementName, LayoutAlignment layout, WindowSetup setup,
            Action<T> action = null)
        {
            WindowUI window = NewWindow(elementName, layout, setup);
            SingleSelectionUI<T> button = window.AddSingleSelection(elementName, action);
            window.AutoResize();
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
            WindowUI window = NewWindow(elementName, setup);
            TextInputUI button = window.AddTextInput(elementName, action);
            window.AutoResize();
            return button;
        }

        public TextInputUI AddTextInput(string elementName, LayoutAlignment layout, Action<string> action = null)
        {
            return AddTextInput(elementName, layout, _menuStyling.windowSetup, action);
        }

        public TextInputUI AddTextInput(string elementName, LayoutAlignment layout, WindowSetup setup,
            Action<string> action = null)
        {
            WindowUI window = NewWindow(elementName, layout, setup);
            TextInputUI button = window.AddTextInput(elementName, action);
            window.AutoResize();
            return button;
        }

        public TextInputUI AddTextInput(string elementName, WindowUI window, Action<string> action = null) =>
            window.AddTextInput(elementName, action);

        public TextInputUIWithPrediction AddTextInputNoLabelWithPrediction(string elementName,
            LayoutAlignment layout,
            Action<string> action = null)
        {
            return AddTextInputNoLabelWithPrediction(elementName, layout, _menuStyling.windowSetup, action);
        }

        public TextInputUIWithPrediction AddTextInputNoLabelWithPrediction(string elementName,
            LayoutAlignment layout, WindowSetup setup,
            Action<string> action = null)
        {
            WindowUI window = NewWindow(elementName, layout, setup);
            TextInputUIWithPrediction button = window.AddTextInputNoLabelWithPrediction(elementName, action);
            window.AutoResize();
            return button;
        }

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
            WindowUI window = NewWindow(elementName, setup);
            ToggleUI toggle = window.AddToggle(elementName, action);
            window.AutoResize();
            return toggle;
        }

        public ToggleUI AddToggle(string elementName, LayoutAlignment layout, Action<bool> action = null)
        {
            return AddToggle(elementName, layout, _menuStyling.windowSetup, action);
        }

        public ToggleUI AddToggle(string elementName, LayoutAlignment layout, WindowSetup setup,
            Action<bool> action = null)
        {
            WindowUI window = NewWindow(elementName, layout, setup);
            ToggleUI toggle = window.AddToggle(elementName, action);
            window.AutoResize();
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
            WindowUI window = NewWindow(elementName, setup);
            ToggleUIWithContent toggle = window.AddToggleWithContent(elementName, action);
            window.AutoResize();
            return toggle;
        }

        public ToggleUIWithContent AddToggleWithContent(string elementName, LayoutAlignment layout,
            Action<bool> action = null)
        {
            return AddToggleWithContent(elementName, layout, _menuStyling.windowSetup, action);
        }

        public ToggleUIWithContent AddToggleWithContent(string elementName, LayoutAlignment layout,
            WindowSetup setup,
            Action<bool> action = null)
        {
            WindowUI window = NewWindow(elementName, layout, setup);
            ToggleUIWithContent toggle = window.AddToggleWithContent(elementName, action);
            window.AutoResize();
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
            WindowUI window = NewWindow(elementName, setup);
            SliderUI slider = window.AddSlider(elementName, action);
            window.AutoResize();
            return slider;
        }

        public SliderUI AddSlider(string elementName, LayoutAlignment layout, Action<int> action = null)
        {
            return AddSlider(elementName, layout, _menuStyling.windowSetup, action);
        }

        public SliderUI AddSlider(string elementName, LayoutAlignment layout, WindowSetup setup,
            Action<int> action = null)
        {
            WindowUI window = NewWindow(elementName, layout, setup);
            SliderUI slider = window.AddSlider(elementName, action);
            window.AutoResize();
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
            WindowUI window = NewWindow(elementName, setup);
            SliderUIChoice<T> slider = window.AddSliderWithChoice<T>(elementName, action);
            window.AutoResize();
            return slider;
        }

        public SliderUIChoice<T> AddSliderWithChoice<T>(string elementName, LayoutAlignment layout,
            Action<T> action = null)
        {
            return AddSliderWithChoice<T>(elementName, layout, _menuStyling.windowSetup, action);
        }

        public SliderUIChoice<T> AddSliderWithChoice<T>(string elementName, LayoutAlignment layout, WindowSetup setup,
            Action<T> action = null)
        {
            WindowUI window = NewWindow(elementName, layout, setup);
            SliderUIChoice<T> slider = window.AddSliderWithChoice<T>(elementName, action);
            window.AutoResize();
            return slider;
        }

        public SliderUIChoice<T>
            AddSliderWithChoice<T>(string elementName, WindowUI window, Action<T> action = null) =>
            window.AddSliderWithChoice<T>(elementName, action);

        public virtual bool BaseConfirmAction()
        {
            bool result = currentSelectable switch
            {
                SliderUI slider => SliderAction(slider, !_inElementInputMode),
                ToggleUI toggle => ToggleAction(toggle),
                ButtonUIDoubleConfirm button => DoubleConfirmAction(button, true),
                ButtonUICountable button => ButtonWithCountAction(button, true),
                TextInputUI textInput => TextInputAction(textInput),
                ISelectable selection => SingleSelectionAction(selection),
                ScrollableTextUI scrollableText => ScrollableTextAction(scrollableText, !_inElementInputMode),
                not null => ButtonAction(currentSelectable),
                _ => false,
            };
            return result;
        }

        bool SingleSelectionAction(ISelectable selection)
        {
            CloseMenu();
            WindowManager.instance.GetSingleSelectionInput(this,
                selection.values, selection.activeCount);
            return true;
        }

        void ITextInputTarget.SetTextInput(string text)
        {
            if (currentSelectable is TextInputUI textInput)
            {
                textInput.SetInputContent(text);
            }

            _inElementInputMode = false;
            LinkInput();
        }

        void ISelectionInputTarget.SetSelection(int count)
        {
            if (currentSelectable is ISelectable target)
            {
                target.SetCount(count);
            }

            OpenMenu();
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
            if (!_movingWindow && _currentSelection[0] >= 0 && _currentSelection[1] == -1 &&
                currentWindow.ContainsPosition(WindowManager.instance.inputProvider.mousePosition))
            {
                _movingWindow = true;
                currentWindow.Move(Vector2.zero);
            }
        }

        void IMenuInputTarget.OnMouseConfirmReleased()
        {
            if (!_movingWindow)
                return;
            _movingWindow = false;
            ClearWindowLocation();
            UpdateWindowLocation();
        }

        void IMenuInputTarget.OnMouseCancel()
        {
            CancelSelection();
        }

        void IMenuInputTarget.OnKeyboardEscape()
        {
        }

        protected virtual bool ConfirmSelection()
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

        protected virtual bool MouseConfirmSelection()
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

        protected virtual bool ButtonAction(ButtonUI button)
        {
            button.TriggerAction();
            return true;
        }

        protected virtual bool ToggleAction(ToggleUI toggle)
        {
            if (toggle.available)
                toggle.Toggle();
            return true;
        }

        protected virtual bool ButtonWithCountAction(ButtonUICountable button, bool increment)
        {
            if (increment)
                button.count++;
            else
                button.count--;
            return true;
        }

        protected virtual bool DoubleConfirmAction(ButtonUIDoubleConfirm button, bool confirm)
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

        protected virtual bool TextInputAction(TextInputUI textInput)
        {
            WindowManager.instance.GetTextInput(this,
                textInput);
            textInput.SetInput(true);
            _inElementInputMode = true;
            return true;
        }

        protected virtual bool ScrollableTextAction(ScrollableTextUI scrollableText, bool setInput)
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

        protected virtual bool SliderAction(SliderUI slider, bool setInput)
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

        protected virtual bool CancelSelection()
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
            foreach (WindowUI window in _windowInstances)
            {
                window.InvokeUpdate();
            }
        }

        public void ClearWindowLocation(float delay = .1f)
        {
            if (_nextNavigationUpdate < Time.unscaledTime + delay)
            {
                DelayInput(delay);
            }

            foreach (WindowUI window in _windowInstances)
            {
                window.ClearCachedPosition();
            }
        }

        public virtual bool OpenMenu()
        {
            return OpenMenu(_menuSetup.allowNavigationOnOpen);
        }

        public virtual bool OpenMenu(bool enableNavigation)
        {
            _displayActive = true;
            _selectionUpdated = true;
            if (enableNavigation)
            {
                LinkInput();
                DelayInput(_menuSetup.menuOpenInputDelay);
            }

            ResetHold();
            switch (_menuSetup.resetOnOpen)
            {
                case UISystemResetOnOpenBehavior.ResetSelection:
                    ResetSelection();
                    break;
                case UISystemResetOnOpenBehavior.ClearSelection:
                    ClearSelection();
                    break;
                case UISystemResetOnOpenBehavior.Disable:
                    break;
            }

            _mouseActive = false;
            currentSelectable?.SetFocus(true);
            foreach (WindowUI window in _windowInstances)
            {
                window.SetActive(true);
            }

            return true;
        }

        public virtual bool CloseMenu()
        {
            if (!_displayActive)
                return false;
            UnlinkInput();

            if (_inElementInputMode)
            {
                _inElementInputMode = false;
                if (currentSelectable is SliderUI slider)
                    slider.SetInput(false);
                if (currentSelectable is ScrollableTextUI scrollableText)
                    scrollableText.SetScrolling(false);
            }

            ClearWindowFocus();
            ClearElementsFocus();
            _displayActive = false;
            _navigationActive = false;
            _nextNavigationUpdate = Mathf.Infinity;
            foreach (WindowUI window in _windowInstances)
            {
                window.SetActive(false);
            }

            _menuCloseAction?.Invoke();

            return true;
        }

        protected virtual void TriggerSelectionUpdate()
        {
            foreach (WindowUI window in _windowInstances)
            {
                window.TriggerSelectionUpdate();
            }
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

        public void SetMenuCloseAction(Action action)
        {
            _menuCloseAction = action;
        }
    }
}