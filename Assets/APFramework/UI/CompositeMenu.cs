using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using ChosenConcept.APFramework.Interface.Framework.Element;
using Unity.Collections;
using Object = UnityEngine.Object;

namespace ChosenConcept.APFramework.Interface.Framework
{
    [Serializable]
    public class CompositeMenu : IMenuInputTarget, ITextInputTarget, ISelectionInputTarget
    {
        [SerializeField]  string _menuName;
        [SerializeField]  MenuSetup _menuSetup;
        [SerializeField]  MenuStyling _menuStyling;
        [SerializeField]  List<WindowUI> _instanceWindows = new();
        [SerializeField]  List<LayoutAlignment> _instanceLayoutAlignment = new();
        [SerializeField]  bool _displayActive;
        [SerializeField]  bool _navigationActive;
        [SerializeField]  int _linkFrame = -1;
        [SerializeField]  float _nextNavigationUpdate = Mathf.Infinity;
        [SerializeField]  Vector2Int _currentSelection = Vector2Int.zero;
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

        public bool ExistsSelectable(int i) => _instanceWindows[0].interactables.Count > i && i >= 0;

        public bool ExistsSelectable(int i, int j) =>
            _instanceWindows.Count > i && i >= 0 && _instanceWindows[i].interactables.Count > j && j >= 0;

        public List<ButtonUI> GetSelectableList(int i = 0) => _instanceWindows[i].interactables;
        public ButtonUI GetSelectable(int i) => _instanceWindows[0].interactables[i];
        public ButtonUI GetSelectable(int i, int j) => _instanceWindows[i].interactables[j];

        public ButtonUI GetSelectable(Vector2Int select) =>
            select.x >= 0 && select.x < _instanceWindows.Count && select.y >= 0 &&
            select.y < _instanceWindows[select.x].interactables.Count
                ? _instanceWindows[select.x].interactables[select.y]
                : null;

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
            if (!_instanceWindows.Any(x => x.interactables.Any()))
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
            foreach (WindowUI window in _instanceWindows)
            {
                window.InvokeUpdate();
            }
        }

        public void SetOpacity(float opacity)
        {
            foreach (WindowUI window in _instanceWindows)
            {
                window.SetOpacity(opacity);
            }
        }

        public LayoutAlignment InitializeNewLayout()
        {
            LayoutAlignment layout =
                WindowManager.instance.InstantiateLayout(_menuStyling.layoutSetup,
                    menuTag);
            _instanceLayoutAlignment.Add(layout);
            return layout;
        }

        /// <summary>
        /// A simple method to spawn window
        /// </summary>
        public WindowUI NewWindow(string windowName, WindowSetup setup)
        {
            LayoutAlignment layout = InitializeNewLayout();
            WindowUI window = WindowManager.instance.NewWindow(windowName, layout, setup, menuTag);
            _instanceWindows.Add(window);
            return window;
        }

        /// <summary>
        /// A simple method to spawn window with designated layout
        /// </summary>
        public WindowUI NewWindow(string windowName, LayoutAlignment layout, WindowSetup setup)
        {
            WindowUI window = WindowManager.instance.NewWindow(windowName, layout, setup, menuTag);
            _instanceWindows.Add(window);
            return window;
        }

        public void RemoveElement(WindowElement element)
        {
            element.Remove();
        }

        public void RemoveWindowsAndLayoutGroups()
        {
            ClearWindows(true);
            foreach (LayoutAlignment alignment in _instanceLayoutAlignment)
            {
                Object.Destroy(alignment.gameObject);
            }

            _instanceLayoutAlignment.Clear();
        }

        public void ClearWindows(bool removeWindow)
        {
            ClearWindowLocation();
            _currentSelection = Vector2Int.zero;
            foreach (WindowUI window in _instanceWindows)
            {
                ClearWindow(window, false);
                if (removeWindow)
                    WindowManager.instance.CloseWindow(window);
            }

            _instanceWindows.Clear();
        }

        public void ClearWindow(WindowUI window, bool removeWindow)
        {
            window.ClearElements();
            if (removeWindow)
            {
                _instanceWindows.Remove(window);
                WindowManager.instance.CloseWindow(window);
            }
        }

        public void SetAllWindowLocalizedByTag()
        {
            foreach (WindowUI window in _instanceWindows)
            {
                window.SetLocalizedByTag();
            }
        }

        public void AutoResizeAllWindows(int extraWidth = 0)
        {
            foreach (WindowUI window in _instanceWindows)
            {
                window.AutoResize(extraWidth);
            }
        }

        public void SyncAutoResizeAllWindows(int extraWidth = 0)
        {
            int maxWidth = 0;
            List<int> autoResizeWidths = new List<int>();
            for (int i = 0; i < _instanceWindows.Count; i++)
            {
                int autoResizeWidth = _instanceWindows[i].GetAutoResizeWidth(0);
                autoResizeWidths.Add(autoResizeWidth);
                if (autoResizeWidth > maxWidth)
                    maxWidth = autoResizeWidth;
            }

            for (int i = 0; i < _instanceWindows.Count; i++)
            {
                _instanceWindows[i].AutoResize(maxWidth - autoResizeWidths[i] + extraWidth);
            }
        }

        public void Refresh()
        {
            foreach (WindowUI window in _instanceWindows)
            {
                window.InvokeUpdate();
                window.RefreshSize();
            }
        }

        public void ContextLanguageChange()
        {
            foreach (WindowUI window in _instanceWindows)
            {
                window.ContextLanguageChange();
            }
        }

        public void TriggerResolutionChange()
        {
            foreach (LayoutAlignment layout in _instanceLayoutAlignment)
            {
                layout.ContextResolutionChange();
            }
        }

        public void MoveWindowToIndex(WindowUI window, int index)
        {
            _instanceWindows.Remove(window);
            _instanceWindows.Insert(index, window);
            window.layoutAlignment.MoveWindowToIndex(window, index);
        }

        void UpdateMouseNavigation()
        {
            if (!WindowManager.instance.inputProvider.inputEnabled ||
                _lastMousePosition == WindowManager.instance.inputProvider.mousePosition ||
                !WindowManager.instance.inputProvider.hasMouse || _instanceWindows.Count == 0
                || !_navigationActive)
            {
                return;
            }

            _lastMousePosition = WindowManager.instance.inputProvider.mousePosition;
            if (!_inElementInputMode)
            {
                if (_instanceWindows.Count == 1)
                {
                    _mouseActive = false;
                    // if selectables are more than 1, do precise detection
                    if (_instanceWindows[0].interactables.Count > 1)
                    {
                        for (int i = 0; i < _instanceWindows[0].interactables.Count; i++)
                        {
                            (Vector2 bottomLeft, Vector2 topRight) = _instanceWindows[0].SelectableBound(i);
                            if (bottomLeft == Vector2.zero && topRight == Vector2.zero)
                                continue;
                            Vector2 bottomLeftDelta = _lastMousePosition - bottomLeft;
                            if (bottomLeftDelta.x <= 0 || bottomLeftDelta.y <= 0)
                                continue;
                            Vector2 topRightDelta = _lastMousePosition - topRight;
                            if (topRightDelta.x >= 0 || topRightDelta.y >= 0)
                                continue;
                            if (i != _currentSelection[1])
                            {
                                _selectionUpdated = true;
                                currentSelectable?.SetFocus(false);
                                _currentSelection[0] = 0;
                                _currentSelection[1] = i;
                                currentSelectable?.SetFocus(true);
                            }

                            _mouseActive = true;
                            break;
                        }
                    }
                    // Otherwise do window check alone
                    else
                    {
                        if (_instanceWindows[0].interactables.Count == 0)
                            return;
                        (Vector2 topLeft, Vector2 bottomRight) = _instanceWindows[0].cachedPosition;
                        if (topLeft == Vector2.zero && bottomRight == Vector2.zero)
                            return;
                        Vector2 topLeftDelta = _lastMousePosition - topLeft;
                        if (topLeftDelta.x <= 0 || topLeftDelta.y <= 0)
                            return;
                        Vector2 bottomRightDelta = _lastMousePosition - bottomRight;
                        if (bottomRightDelta.x >= 0 || bottomRightDelta.y >= 0)
                            return;
                        if (0 != _currentSelection[1])
                        {
                            _selectionUpdated = true;
                            currentSelectable?.SetFocus(false);
                            _currentSelection[0] = 0;
                            _currentSelection[1] = 0;
                            currentSelectable?.SetFocus(true);
                        }

                        _mouseActive = true;
                    }
                }
                else
                {
                    _mouseActive = false;
                    bool hasEnterWindow = false;
                    for (int i = 0; i < _instanceWindows.Count; i++)
                    {
                        if (_instanceWindows[i].interactables.Count == 0)
                            continue;
                        (Vector2 topLeft, Vector2 bottomRight) = _instanceWindows[i].cachedPosition;
                        if (topLeft == Vector2.zero && bottomRight == Vector2.zero)
                            continue;
                        Vector2 topLeftDelta = _lastMousePosition - topLeft;
                        if (topLeftDelta.x <= 0 || topLeftDelta.y <= 0)
                            continue;
                        Vector2 bottomRightDelta = _lastMousePosition - bottomRight;
                        if (bottomRightDelta.x >= 0 || bottomRightDelta.y >= 0)
                            continue;
                        hasEnterWindow = true;
                        if (i != _currentSelection[0])
                        {
                            _selectionUpdated = true;
                            currentSelectable?.SetFocus(false);
                            _currentSelection[0] = i;
                            int count = _instanceWindows[i].interactables.Count;
                            RefocusAtNearestElement(_lastMousePosition, _currentSelection[0]);
                            _currentSelection[1] = Mathf.Clamp(_currentSelection[1], 0, count - 1);
                            currentSelectable?.SetFocus(true);
                        }

                        _mouseActive = true;
                        break;
                    }

                    if (!hasEnterWindow)
                        ClearSelection();

                    // Allowing currentSelection[0] to stay invalid is due to the need to have window "deselected"
                    if (_currentSelection[0] < 0)
                        return;
                    // Selectable detection
                    for (int i = 0; i < _instanceWindows[_currentSelection[0]].interactables.Count; i++)
                    {
                        (Vector2 bottomLeft, Vector2 topRight) =
                            _instanceWindows[_currentSelection[0]].SelectableBound(i);
                        if (bottomLeft == Vector2.zero && topRight == Vector2.zero)
                            continue;
                        Vector2 bottomLeftDelta = _lastMousePosition - bottomLeft;
                        if (bottomLeftDelta.x <= 0 || bottomLeftDelta.y <= 0)
                            continue;
                        Vector2 topRightDelta = _lastMousePosition - topRight;
                        if (topRightDelta.x >= 0 || topRightDelta.y >= 0)
                            continue;
                        if (i != _currentSelection[1])
                        {
                            _selectionUpdated = true;
                            currentSelectable?.SetFocus(false);
                            _currentSelection[1] = i;
                            currentSelectable?.SetFocus(true);
                        }

                        _mouseActive = true;
                        break;
                    }
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
                foreach (WindowUI window in _instanceWindows)
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
            foreach (WindowUI window in _instanceWindows)
            {
                foreach (ButtonUI element in window.interactables)
                {
                    element.SetFocus(false);
                }
            }

            _currentSelection = Vector2Int.zero;
            if (GetSelectable(_currentSelection) == null)
            {
                for (int i = 0; i < _instanceWindows.Count; i++)
                {
                    if (_instanceWindows[i].interactables.Count == 0)
                        continue;
                    _currentSelection = new Vector2Int(i, 0);
                    break;
                }
            }

            GetSelectable(_currentSelection)?.SetFocus(true);
        }

        // Used to set all windows in unselected state
        public void ClearSelection()
        {
            foreach (WindowUI window in _instanceWindows)
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
            if (_instanceWindows.Count > _currentSelection.x &&
                _instanceWindows[_currentSelection.x].interactables.Count > _currentSelection.y)
                GetSelectable(_currentSelection).SetFocus(false);
            _currentSelection = new Vector2Int(x, y);
            if (_instanceWindows.Count > _currentSelection.x &&
                _instanceWindows[_currentSelection.x].interactables.Count > _currentSelection.y)
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
            foreach (WindowUI window in _instanceWindows)
            {
                window.ClearElementsFocus();
            }
        }

        public void ClearWindowFocus()
        {
            foreach (WindowUI window in _instanceWindows)
            {
                window.ClearWindowFocus();
            }
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
            if (_currentSelection.x < 0 || _currentSelection.y < 0)
            {
                ResetSelection();
            }
            else
            {
                UpdateSelectionProcess();
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
                int xBefore = _currentSelection[0];
                int yBefore = _currentSelection[1];
                if (_menuSetup.navigationDirection is UISystemNavigationDirection.X or UISystemNavigationDirection.Y)
                {
                    int offset = _menuSetup.navigationDirection switch
                    {
                        UISystemNavigationDirection.X => Mathf.RoundToInt(_move.x) switch
                        {
                            1 => 1,
                            -1 => -1,
                            _ => 0
                        },
                        UISystemNavigationDirection.Y => Mathf.RoundToInt(_move.y) switch
                        {
                            1 => -1,
                            -1 => 1,
                            _ => 0
                        },
                        _ => 0
                    };
                    _currentSelection[1] += offset;
                    if (_instanceWindows.Count == 1)
                    {
                        int count = GetSelectableList().Count;
                        if (_menuSetup.allowCycle)
                            _currentSelection[1] = _currentSelection[1] >= 0 ? _currentSelection[1] % count : count - 1;
                        else
                            _currentSelection[1] = Mathf.Clamp(_currentSelection[1], 0, count - 1);
                    }
                    else
                    {
                        int currentSelectableCount = GetSelectableList(_currentSelection[0]).Count;
                        int windowCount = _instanceWindows.Count;
                        if (_currentSelection[1] < 0)
                        {
                            int resultWindow = _currentSelection[0] - 1;
                            while (resultWindow >= 0 && GetSelectableList(resultWindow).Count == 0)
                                resultWindow--;
                            if (resultWindow < 0)
                                resultWindow = windowCount - 1;
                            if (resultWindow > _currentSelection[0] && _menuSetup.allowCycle ||
                                resultWindow <= _currentSelection[0])
                            {
                                _currentSelection[0] = resultWindow;
                                _currentSelection[1] = GetSelectableList(_currentSelection[0]).Count - 1;
                            }
                            else
                            {
                                _currentSelection[1] = 0;
                            }
                        }
                        else if (_currentSelection[1] > currentSelectableCount - 1)
                        {
                            int resultWindow = _currentSelection[0] + 1;
                            while (resultWindow < windowCount && GetSelectableList(resultWindow).Count == 0)
                                resultWindow++;
                            if (resultWindow > windowCount - 1)
                                resultWindow = 0;
                            if (_menuSetup.allowCycle && resultWindow < _currentSelection[0] ||
                                resultWindow > _currentSelection[0])
                            {
                                _currentSelection[0] = resultWindow;
                                _currentSelection[1] = 0;
                            }
                            else
                            {
                                _currentSelection[1] = currentSelectableCount - 1;
                            }
                        }
                    }
                }
                else if (_menuSetup.navigationDirection == UISystemNavigationDirection.ClosestDirectionMatch)
                {
                    bool axisState = Mathf.Abs(_move.x) > Mathf.Abs(_move.y);
                    Vector2 inputDirection =
                        axisState ? Vector2.right * Mathf.Sign(_move.x) : Vector2.up * Mathf.Sign(_move.y);
                    float minDistance = Mathf.Infinity;
                    (Vector2 item1, _) = _instanceWindows[xBefore].interactables[yBefore].cachedPosition;
                    Vector2 currentSelectableLocation = item1;
                    foreach (WindowUI window in _instanceWindows)
                    {
                        if (window.interactables.Count == 0)
                            continue;
                        for (int i = 0; i < window.interactables.Count; i++)
                        {
                            (Vector2 position1, _) = window.interactables[i].cachedPosition;
                            Vector2 selectableLocation = position1;
                            Vector2 direction = selectableLocation - currentSelectableLocation;
                            float distance = direction.sqrMagnitude;
                            Vector2 directionNormalized = direction.normalized;
                            if (distance < minDistance && Vector2.Dot(directionNormalized, inputDirection) > .5f)
                            {
                                minDistance = distance;
                                _currentSelection[0] = _instanceWindows.IndexOf(window);
                                _currentSelection[1] = i;
                            }
                        }
                    }
                }
                else if (_menuSetup.navigationDirection == UISystemNavigationDirection.TwoWay)
                {
                    if (Mathf.Abs(_move.x) > Mathf.Abs(_move.y))
                    {
                        int offset = Mathf.RoundToInt(_move.x) switch
                        {
                            1 => 1,
                            -1 => -1,
                            _ => 0
                        };
                        int targetSelection = _currentSelection[0];
                        while (targetSelection == _currentSelection[0] || GetSelectableList(targetSelection).Count == 0)
                        {
                            targetSelection += offset;
                            if (_menuSetup.allowCycle && targetSelection < 0)
                            {
                                targetSelection = _instanceWindows.Count - 1;
                            }
                            else if (_menuSetup.allowCycle && targetSelection >= _instanceWindows.Count)
                            {
                                targetSelection = 0;
                            }
                            else if (!_menuSetup.allowCycle &&
                                     (targetSelection < 0 || targetSelection >= _instanceWindows.Count) ||
                                     targetSelection == _currentSelection[0])
                            {
                                targetSelection = _currentSelection[0];
                                break;
                            }
                        }

                        if (targetSelection != _currentSelection[0])
                        {
                            Vector2 currentSelectableLocation = _instanceWindows[_currentSelection[0]]
                                .interactables[_currentSelection[1]].cachedPosition.Item1;
                            _currentSelection[0] = targetSelection;
                            float minDistanceY = Mathf.Infinity;
                            for (int i = 0; i < _instanceWindows[_currentSelection[0]].interactables.Count; i++)
                            {
                                Vector2 selectableLocation = _instanceWindows[_currentSelection[0]].interactables[i]
                                    .cachedPosition.Item1;
                                float distanceY = Mathf.Abs(selectableLocation.y - currentSelectableLocation.y);
                                if (distanceY < minDistanceY)
                                {
                                    minDistanceY = distanceY;
                                    _currentSelection[1] = i;
                                }
                            }

                            _currentSelection[1] = Mathf.Clamp(_currentSelection[1], 0,
                                GetSelectableList(_currentSelection[0]).Count - 1);
                        }
                    }
                    else
                    {
                        int offset = Mathf.RoundToInt(_move.y) switch
                        {
                            1 => -1,
                            -1 => 1,
                            _ => 0
                        };
                        _currentSelection[1] += offset;
                        int count = GetSelectableList(_currentSelection[0]).Count;
                        if (_menuSetup.allowCycle)
                            _currentSelection[1] = _currentSelection[1] >= 0 ? _currentSelection[1] % count : count - 1;
                        else
                            _currentSelection[1] = Mathf.Clamp(_currentSelection[1], 0, count - 1);
                    }
                }

                if (xBefore != _currentSelection[0] || yBefore != _currentSelection[1])
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
            if (!_windowElementLocationCached)
                UpdateWindowLocation();
            float minDistance = Mathf.Infinity;
            currentSelectable?.SetFocus(false);
            foreach (WindowUI window in _instanceWindows)
            {
                if (window.interactables.Count == 0)
                    continue;
                for (int i = 0; i < window.interactables.Count; i++)
                {
                    Vector2 selectableLocation = window.interactables[i].cachedPosition.Item1;
                    float distance = (selectableLocation - referenceLocation).sqrMagnitude;
                    if (distance < minDistance)
                    {
                        minDistance = distance;
                        _currentSelection[0] = _instanceWindows.IndexOf(window);
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
            if (!_windowElementLocationCached)
                UpdateWindowLocation();
            float minDistance = Mathf.Infinity;
            currentSelectable?.SetFocus(false);
            WindowUI window = _instanceWindows[windowIndex];
            if (window.interactables.Count > 0)
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
            if (!_windowElementLocationCached)
                UpdateWindowLocation();
            float minDistance = Mathf.Infinity;
            currentSelectable?.SetFocus(false);
            WindowUI window = _instanceWindows[windowIndex];
            if (window.interactables.Count > 0)
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
            window.SetSingleWindowOverride(true);
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
            window.SetSingleWindowOverride(true);
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
            window.SetSingleWindowOverride(true);
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
            window.SetSingleWindowOverride(true);
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
            window.SetSingleWindowOverride(true);
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
            window.SetSingleWindowOverride(true);
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
            window.SetSingleWindowOverride(true);
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
            window.SetSingleWindowOverride(true);
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
            window.SetSingleWindowOverride(true);
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
            window.SetSingleWindowOverride(true);
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
            window.SetSingleWindowOverride(true);
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
            window.SetSingleWindowOverride(true);
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
            window.SetSingleWindowOverride(true);
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
            window.SetSingleWindowOverride(true);
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
            window.SetSingleWindowOverride(true);
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
            window.SetSingleWindowOverride(true);
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
            window.SetSingleWindowOverride(true);
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
            window.SetSingleWindowOverride(true);
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
            window.SetSingleWindowOverride(true);
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
            window.SetSingleWindowOverride(true);
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
            window.SetSingleWindowOverride(true);
            SliderUIChoice<T> slider = window.AddSliderWithChoice<T>(elementName, action);
            window.AutoResize();
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

            OpenMenu();
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
            foreach (WindowUI window in _instanceWindows)
            {
                window.InvokeUpdate();
            }
        }

        public void ClearWindowLocation(float delay = .1f)
        {
            // SetOpacity(0.1f, true);
            if (_nextNavigationUpdate < Time.unscaledTime + delay)
            {
                DelayInput(delay);
            }

            foreach (WindowUI window in _instanceWindows)
            {
                window.ClearCachedPosition();
            }

            _windowElementLocationCached = false;
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
            {
                LinkInput();
                DelayInput(_menuSetup.menuOpenInputDelay);
            }

            ResetHold();
            switch (_menuSetup.resetOnOpen)
            {
                case UISystemResetOnOpenBehavior.Always:
                    ResetSelection();
                    break;
            }

            _mouseActive = false;
            currentSelectable?.SetFocus(true);
            foreach (WindowUI window in _instanceWindows)
            {
                window.SetActive(true);
            }

            return true;
        }

        public bool CloseMenu()
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

            currentSelectable?.SetFocus(false);
            _nextNavigationUpdate = Mathf.Infinity;
            _displayActive = false;
            foreach (WindowUI window in _instanceWindows)
            {
                window.SetActive(false);
            }

            return true;
        }

        void TriggerSelectionUpdate()
        {
            foreach (WindowUI window in _instanceWindows)
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
    }
}