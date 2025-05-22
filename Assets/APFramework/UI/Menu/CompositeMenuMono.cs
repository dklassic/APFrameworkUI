using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using ChosenConcept.APFramework.Interface.Framework.Element;

namespace ChosenConcept.APFramework.Interface.Framework
{
    public class CompositeMenuMono : MonoBehaviour, IMenuInputTarget
    {
        [SerializeField] protected MenuSetup _menuSetup = MenuSetup.defaultSetup;
        [SerializeField] protected MenuStyling _menuDefaultStyling = MenuStyling.defaultStyling;

        [Header("Debug View")] [SerializeField]
        protected List<WindowUI> _windowInstances = new();

        [SerializeField] protected List<LayoutAlignment> _layoutAlignmentInstances = new();
        [SerializeField] protected bool _initialized;
        [SerializeField] protected bool _displayActive;
        [SerializeField] protected bool _navigationActive;
        [SerializeField] protected bool _valueDirty;
        [SerializeField] protected int _linkFrame = -1;
        [SerializeField] protected float _nextNavigationUpdate = Mathf.Infinity;
        [SerializeField] protected Vector2Int _currentSelection = Vector2Int.zero;
        [SerializeField] protected float _holdStart = Mathf.Infinity;
        [SerializeField] protected float _holdNavigationNext = Mathf.Infinity;
        [SerializeField] protected Vector2 _move = Vector2.zero;
        [SerializeField] protected Vector2 _mouseScroll = Vector2.zero;
        [SerializeField] protected Vector2 _lastMousePosition = Vector2.negativeInfinity;
        [SerializeField] protected bool _mouseSelectionTargetExists;
        [SerializeField] protected bool _hoverOnDecrease;
        [SerializeField] protected bool _hoverOnIncrease;
        [SerializeField] protected bool _inElementInputMode;
        [SerializeField] protected bool _selectionUpdated;
        [SerializeField] protected CompositeMenuMono _lastMenu;
        [SerializeField] protected bool _movingWindow;

        Action _menuCloseAction;

        bool windowPositionCached
        {
            get
            {
                foreach (WindowUI window in _windowInstances)
                {
                    if (!window.positionCached)
                        return false;
                }

                return true;
            }
        }

        public WindowElement currentSelectable
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

        public string menuTag => $"Interface.{GetType().Name}";
        public bool isDisplayActive => _displayActive;
        public CompositeMenuMono lastMenu => _lastMenu;

        public CompositeMenuMono rootMenu
        {
            get
            {
                CompositeMenuMono menu = _lastMenu;
                if (menu == null)
                    return null;
                while (menu.lastMenu != null)
                    menu = menu.lastMenu;
                return menu;
            }
        }

        protected bool ExistsSelectable(int i) => _windowInstances[0].interactables.Count > i && i >= 0;

        protected bool ExistsSelectable(int i, int j) =>
            _windowInstances.Count > i && i >= 0 && _windowInstances[i].interactables.Count > j && j >= 0;

        protected virtual List<WindowElement> GetSelectableList(int i = 0) => _windowInstances[i].interactables;
        protected virtual WindowElement GetSelectable(int i) => _windowInstances[0].interactables[i];
        protected virtual WindowElement GetSelectable(int i, int j) => _windowInstances[i].interactables[j];

        protected virtual WindowElement GetSelectable(Vector2Int select) =>
            select.x >= 0 && select.x < _windowInstances.Count && select.y >= 0 &&
            select.y < _windowInstances[select.x].interactables.Count
                ? _windowInstances[select.x].interactables[select.y]
                : null;

        void Start()
        {
            Initialize();
        }
        public virtual void Initialize()
        {
            if (_initialized)
                return;
            InitializeMenu();
            WindowManager.instance.RegisterMenu(this);
            _initialized = true;
        }

        protected virtual void InitializeMenu() => _ = 0;

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

            UpdateMouseNavigation();
            if (Time.unscaledTime < _nextNavigationUpdate)
                return;
            UpdateWindowLocation();
            if (float.IsPositiveInfinity(_holdStart) || _holdNavigationNext <= Time.unscaledTime)
            {
                UpdateSelection();
            }

            ResetDirection();
            if (_valueDirty)
            {
                _valueDirty = false;
                ValueUpdate();
            }

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

        protected virtual void ValueUpdate() => _ = 0;

        public void TriggerValueUpdate()
        {
            _valueDirty = true;
        }

        protected void MarkValueDirty() => _valueDirty = true;

        public void SetOpacity(float opacity)
        {
            foreach (WindowUI window in _windowInstances)
            {
                window.SetOpacity(opacity);
            }
        }

        protected LayoutAlignment InitNewLayout()
        {
            LayoutAlignment layout =
                WindowManager.instance.InstantiateLayout(_menuDefaultStyling.layoutSetup, menuTag);
            _layoutAlignmentInstances.Add(layout);
            return layout;
        }

        /// <summary>
        /// A simple method to spawn window
        /// </summary>
        protected WindowUI NewWindow(string windowName)
        {
            LayoutAlignment layout = InitNewLayout();
            WindowUI window =
                WindowManager.instance.NewWindow(windowName, layout, _menuDefaultStyling.windowSetup, menuTag);
            _windowInstances.Add(window);
            return window;
        }

        /// <summary>
        /// A simple method to spawn window
        /// </summary>
        protected WindowUI NewWindow(string windowName, WindowSetup setup)
        {
            LayoutAlignment layout = InitNewLayout();
            WindowUI window = WindowManager.instance.NewWindow(windowName, layout, setup, menuTag);
            _windowInstances.Add(window);
            return window;
        }

        /// <summary>
        /// A simple method to spawn window with designated layout
        /// </summary>
        protected WindowUI NewWindow(string windowName, LayoutAlignment layout, WindowSetup setup)
        {
            WindowUI window = WindowManager.instance.NewWindow(windowName, layout, setup, menuTag);
            _windowInstances.Add(window);
            return window;
        }

        /// <summary>
        /// A simple method to spawn single text UI element window
        /// </summary>
        protected TextUI AddText(string elementName)
        {
            WindowUI window = NewWindow(elementName, _menuDefaultStyling.windowSetup);
            return window.AddText(elementName);
        }

        protected void AddGap(WindowUI window)
        {
            window.AddGap();
        }

        protected TextUI AddText(string elementName, LayoutAlignment layout)
        {
            return AddText(elementName, layout, _menuDefaultStyling.windowSetup);
        }

        protected TextUI AddText(string elementName, LayoutAlignment layout, WindowSetup setup)
        {
            WindowUI window = NewWindow(elementName, layout, setup);
            return window.AddText(elementName);
        }

        protected virtual void RemoveElement(WindowElement element)
        {
            element.Remove();
        }

        protected virtual void RemoveWindowsAndLayoutGroups()
        {
            _initialized = false;
            ClearWindows(true);
            foreach (LayoutAlignment alignment in _layoutAlignmentInstances)
            {
                Destroy(alignment.gameObject);
            }

            _layoutAlignmentInstances.Clear();
        }

        protected virtual void ClearWindows(bool removeWindow)
        {
            ClearWindowLocation();
            _currentSelection = Vector2Int.zero;
            _initialized = false;
            foreach (WindowUI window in _windowInstances)
            {
                ClearWindow(window, false);
                if (removeWindow)
                    WindowManager.instance.CloseWindow(window);
            }

            _windowInstances.Clear();
        }

        protected void ClearWindow(WindowUI window, bool removeWindow)
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

        protected void AutoResizeAllWindows(int extraWidth = 0, bool sizeFixed = false)
        {
            foreach (WindowUI window in _windowInstances)
            {
                window.AutoResize(extraWidth, sizeFixed);
            }
        }

        protected virtual void SyncAutoResizeAllWindows(int extraWidth = 0, bool sizeFixed = false)
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
                _windowInstances[i].AutoResize(maxWidth - autoResizeWidths[i] + extraWidth, sizeFixed);
            }
        }

        public virtual void RefreshWindows()
        {
            foreach (WindowUI window in _windowInstances)
            {
                window.InvokeUpdate();
                window.RefreshSize();
            }
        }

        public virtual void TriggerResolutionChange()
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
            _mouseSelectionTargetExists = false;
            if (!_inElementInputMode)
            {
                Vector2Int previousSelection = _currentSelection;
                currentSelectable?.SetFocus(false);
                currentWindow?.SetFocus(false);
                // First get windows in range
                // Get the window with closest interactable
                float minDistanceSqr = Mathf.Infinity;
                int windowCount = _windowInstances.Count;
                int windowIndex = -1;
                for (int i = 0; i < windowCount; i++)
                {
                    if (!_windowInstances[i].ContainsPosition(_lastMousePosition))
                        continue;
                    int interactableCount = _windowInstances[i].interactables.Count;
                    if (interactableCount > 0)
                    {
                        for (int j = 0; j < interactableCount; j++)
                        {
                            float distanceSqr = (_windowInstances[i].interactables[j].cachedCenter - _lastMousePosition)
                                .sqrMagnitude;
                            if (distanceSqr < minDistanceSqr)
                            {
                                minDistanceSqr = distanceSqr;
                                windowIndex = i;
                            }
                        }
                    }
                    else
                    {
                        float distanceSqr = (_windowInstances[i].cachedCenter - _lastMousePosition)
                            .sqrMagnitude;
                        if (distanceSqr < minDistanceSqr)
                        {
                            minDistanceSqr = distanceSqr;
                            windowIndex = i;
                        }
                    }
                }

                // Perform detailed check again specifically in the window
                if (windowIndex != -1)
                {
                    _currentSelection[0] = windowIndex;
                    currentWindow?.SetFocus(true);
                    if (_windowInstances[windowIndex].isSingleButtonWindow)
                    {
                        _currentSelection[1] = 0;
                        _mouseSelectionTargetExists = true;
                        currentSelectable?.SetFocus(true);
                    }
                    else
                    {
                        int interactableCount = _windowInstances[windowIndex].interactables.Count;
                        for (int i = 0; i < interactableCount; i++)
                        {
                            if (!_windowInstances[windowIndex].InteractableContainsPosition(i, _lastMousePosition))
                                continue;
                            _currentSelection[1] = i;
                            _mouseSelectionTargetExists = true;
                            currentSelectable?.SetFocus(true);
                        }
                    }
                }
                else
                {
                    _currentSelection[0] = -1;
                    _currentSelection[1] = -1;
                }

                if (previousSelection != _currentSelection)
                {
                    _selectionUpdated = true;
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

                if (_hoverOnDecrease || _hoverOnIncrease)
                    _mouseSelectionTargetExists = true;
            }
        }

        protected void UpdateWindowLocation()
        {
            if (!windowPositionCached)
            {
                foreach (WindowUI window in _windowInstances)
                {
                    window.UpdateElementsAndWindowPosition();
                }
            }
        }

        public virtual void LinkInput()
        {
            _navigationActive = true;
            _linkFrame = Time.frameCount;
            WindowManager.instance.LinkInputTarget(this);
        }

        public virtual void UnlinkInput()
        {
            _navigationActive = false;
            WindowManager.instance.UnlinkInput(this);
        }

        public void ResetSelection()
        {
            foreach (WindowUI window in _windowInstances)
            {
                foreach (WindowElement element in window.interactables)
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
                foreach (WindowElement element in window.interactables)
                {
                    element.SetFocus(false);
                }
            }

            _currentSelection = new Vector2Int(-1, -1);
        }

        protected virtual void SetCurrentSelection(int x = 0, int y = 0)
        {
            if (_windowInstances.Count > _currentSelection.x &&
                _windowInstances[_currentSelection.x].interactables.Count > _currentSelection.y)
                GetSelectable(_currentSelection).SetFocus(false);
            _currentSelection = new Vector2Int(x, y);
            if (_windowInstances.Count > _currentSelection.x &&
                _windowInstances[_currentSelection.x].interactables.Count > _currentSelection.y)
                GetSelectable(_currentSelection).SetFocus(true);
        }

        protected virtual void SetCurrentSelection(Vector2Int currentSelection)
        {
            if (_windowInstances.Count > _currentSelection.x &&
                _windowInstances[_currentSelection.x].interactables.Count > _currentSelection.y)
                GetSelectable(_currentSelection).SetFocus(false);
            _currentSelection = currentSelection;
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

        public virtual void ClearElementsFocus()
        {
            foreach (WindowUI window in _windowInstances)
            {
                window.ClearElementsFocus();
            }
        }

        public virtual void ClearWindowFocus()
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

        public ButtonUI AddButton(string elementName, Action action = null)
        {
            return AddButton(elementName, _menuDefaultStyling.windowSetup, action);
        }

        public ButtonUI AddButton(string elementName, WindowSetup setup, Action action = null)
        {
            WindowUI window = NewWindow(elementName, setup);
            return window.AddButton(elementName, action);
        }

        public ButtonUI AddButton(string elementName, LayoutAlignment layout, Action action = null)
        {
            return AddButton(elementName, layout, _menuDefaultStyling.windowSetup, action);
        }

        public ButtonUI AddButton(string elementName, LayoutAlignment layout, WindowSetup setup,
            Action action = null)
        {
            WindowUI window = NewWindow(elementName, layout, setup);
            return window.AddButton(elementName, action);
        }

        public QuickSelectionUI<T> AddQuickSelectionUI<T>(string elementName, Action<T> action = null)
        {
            return AddQuickSelectionUI(elementName, _menuDefaultStyling.windowSetup, action);
        }

        public QuickSelectionUI<T> AddQuickSelectionUI<T>(string elementName, WindowSetup setup,
            Action<T> action = null)
        {
            WindowUI window = NewWindow(elementName, setup);
            return window.AddQuickSelectionUI(elementName, action);
        }

        public QuickSelectionUI<T> AddQuickSelectionUI<T>(string elementName, LayoutAlignment layout,
            Action<T> action = null)
        {
            return AddQuickSelectionUI(elementName, layout, _menuDefaultStyling.windowSetup, action);
        }

        public QuickSelectionUI<T> AddQuickSelectionUI<T>(string elementName, LayoutAlignment layout, WindowSetup setup,
            Action<T> action = null)
        {
            WindowUI window = NewWindow(elementName, layout, setup);
            return window.AddQuickSelectionUI(elementName, action);
        }

        public ScrollableTextUI AddScrollableText(string elementName, Action action = null)
        {
            return AddScrollableText(elementName, _menuDefaultStyling.windowSetup, action);
        }

        public ScrollableTextUI AddScrollableText(string elementName, WindowSetup setup, Action action = null)
        {
            WindowUI window = NewWindow(elementName, setup);
            return window.AddScrollableText(elementName, action);
        }

        public ScrollableTextUI AddScrollableText(string elementName, LayoutAlignment layout, Action action = null)
        {
            return AddScrollableText(elementName, layout, _menuDefaultStyling.windowSetup, action);
        }

        public ScrollableTextUI AddScrollableText(string elementName, LayoutAlignment layout, WindowSetup setup,
            Action action = null)
        {
            WindowUI window = NewWindow(elementName, layout, setup);
            return window.AddScrollableText(elementName, action);
        }

        public SelectionUI<T> AddSingleSelection<T>(string elementName, Action<T> action = null)
        {
            return AddSingleSelection(elementName, _menuDefaultStyling.windowSetup, action);
        }

        public SelectionUI<T> AddSingleSelection<T>(string elementName, WindowSetup setup,
            Action<T> action = null)
        {
            WindowUI window = NewWindow(elementName, setup);
            return window.AddSingleSelection(elementName, action);
        }

        public SelectionUI<T> AddSingleSelection<T>(string elementName, LayoutAlignment layout,
            Action<T> action = null)
        {
            return AddSingleSelection(elementName, layout, _menuDefaultStyling.windowSetup, action);
        }

        public SelectionUI<T> AddSingleSelection<T>(string elementName, LayoutAlignment layout, WindowSetup setup,
            Action<T> action = null)
        {
            WindowUI window = NewWindow(elementName, layout, setup);
            return window.AddSingleSelection(elementName, action);
        }

        protected TextInputUI AddTextInput(string elementName, Action<string> action = null)
        {
            return AddTextInput(elementName, _menuDefaultStyling.windowSetup, action);
        }

        public TextInputUI AddTextInput(string elementName, WindowSetup setup, Action<string> action = null)
        {
            WindowUI window = NewWindow(elementName, setup);
            return window.AddTextInput(elementName, action);
        }

        public TextInputUI AddTextInput(string elementName, LayoutAlignment layout, Action<string> action = null)
        {
            return AddTextInput(elementName, layout, _menuDefaultStyling.windowSetup, action);
        }

        public TextInputUI AddTextInput(string elementName, LayoutAlignment layout, WindowSetup setup,
            Action<string> action = null)
        {
            WindowUI window = NewWindow(elementName, layout, setup);
            return window.AddTextInput(elementName, action);
        }

        public ToggleUI AddToggle(string elementName, Action<bool> action = null)
        {
            return AddToggle(elementName, _menuDefaultStyling.windowSetup, action);
        }

        public ToggleUI AddToggle(string elementName, WindowSetup setup,
            Action<bool> action = null)
        {
            WindowUI window = NewWindow(elementName, setup);
            return window.AddToggle(elementName, action);
        }

        public ToggleUI AddToggle(string elementName, LayoutAlignment layout, Action<bool> action = null)
        {
            return AddToggle(elementName, layout, _menuDefaultStyling.windowSetup, action);
        }

        public ToggleUI AddToggle(string elementName, LayoutAlignment layout, WindowSetup setup,
            Action<bool> action = null)
        {
            WindowUI window = NewWindow(elementName, layout, setup);
            return window.AddToggle(elementName, action);
        }

        public SliderUI<T> AddSlider<T>(string elementName, Action<T> action = null)
        {
            return AddSlider(elementName, _menuDefaultStyling.windowSetup, action);
        }

        public SliderUI<T> AddSlider<T>(string elementName, WindowSetup setup, Action<T> action = null)
        {
            WindowUI window = NewWindow(elementName, setup);
            return window.AddSlider(elementName, action);
        }

        public SliderUI<T> AddSlider<T>(string elementName, LayoutAlignment layout, Action<T> action = null)
        {
            return AddSlider(elementName, layout, _menuDefaultStyling.windowSetup, action);
        }

        public SliderUI<T> AddSlider<T>(string elementName, LayoutAlignment layout, WindowSetup setup,
            Action<T> action = null)
        {
            WindowUI window = NewWindow(elementName, layout, setup);
            return window.AddSlider(elementName, action);
        }

        protected virtual bool BaseConfirmAction()
        {
            bool result = currentSelectable switch
            {
                ToggleUI toggle => ToggleAction(toggle),
                IQuickSelect button => QuickSelectionAction(button, true),
                TextInputUI textInput => TextInputAction(textInput),
                ISlider slider => SliderAction(slider, !_inElementInputMode),
                ISelectable selection => SelectionAction(selection),
                ScrollableTextUI scrollableText => ScrollableTextAction(scrollableText, !_inElementInputMode),
                ButtonUI button => ButtonAction(button),
                _ => false,
            };
            return result;
        }

        void IMenuInputTarget.SetTextInput(string text)
        {
            if (currentSelectable is TextInputUI textInput)
            {
                textInput.SetInputContent(text);
                textInput.SetInput(false);
            }

            _inElementInputMode = false;
            LinkInput();
        }

        void IMenuInputTarget.SetSelection(int count)
        {
            if (currentSelectable is ISelectable target)
            {
                target.SetCount(count);
            }

            LinkInput();
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
            if (_menuSetup.allowDraggingWithMouse && !_movingWindow && _currentSelection[0] >= 0 &&
                _currentSelection[1] == -1 &&
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
            if (!_displayActive || !_mouseSelectionTargetExists)
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

        protected virtual bool QuickSelectionAction(IQuickSelect button, bool increment)
        {
            if (increment)
                button.CycleForward();
            else
                button.CycleBackward();
            return true;
        }

        protected virtual bool SelectionAction(ISelectable selection)
        {
            UnlinkInput();
            WindowManager.instance.GetSelectionInput(this,
                selection.values, selection.activeCount);
            return true;
        }

        protected virtual bool TextInputAction(TextInputUI textInput)
        {
            WindowManager.instance.GetTextInput(this,
                textInput);
            _inElementInputMode = true;
            textInput.SetInput(true);
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
                _valueDirty = true;
            }

            return true;
        }

        protected virtual bool SliderAction(ISlider slider, bool setInput)
        {
            if (setInput && !_inElementInputMode)
            {
                slider.SetInput(true);
                ClearWindowLocation();
                _inElementInputMode = true;
            }
            else if (!setInput && _inElementInputMode)
            {
                _inElementInputMode = false;
                slider.SetInput(false);
                ClearWindowLocation();
                _valueDirty = true;
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
                if (_menuSetup.allowCloseMenuWithCancelAction)
                    return CancelOut();
                return false;
            }

            bool result = currentSelectable switch
            {
                ISlider slider when _inElementInputMode => SliderAction(slider, false),
                IQuickSelect button when button.canCycleBackward => QuickSelectionAction(button, false),
                ScrollableTextUI scrollableText when _inElementInputMode => ScrollableTextAction(scrollableText, false),
                _ when _menuSetup.allowCloseMenuWithCancelAction => CancelOut(),
                _ => false
            };
            return result;
        }

        protected void UpdateWindows()
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

        public virtual bool OpenMenu(bool enableNavigation)
        {
            return OpenMenu(null, enableNavigation);
        }

        public virtual bool OpenMenu(CompositeMenuMono sourceMenu)
        {
            return OpenMenu(sourceMenu, _menuSetup.allowNavigationOnOpen);
        }

        public virtual bool OpenMenu(CompositeMenuMono sourceMenu, bool enableNavigation)
        {
            _valueDirty = true;
            _selectionUpdated = true;
            _displayActive = true;
            if (sourceMenu is not null)
                _lastMenu = sourceMenu;
            if (enableNavigation)
            {
                LinkInput();
                DelayInput(_menuSetup.menuOpenInputDelay);
            }

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

            _mouseSelectionTargetExists = false;
            currentSelectable?.SetFocus(true);
            currentWindow?.SetFocus(true);
            foreach (WindowUI window in _windowInstances)
            {
                window.SetActive(true);
            }

            return true;
        }

        public virtual bool CloseMenu(bool openLast)
        {
            if (float.IsPositiveInfinity(_nextNavigationUpdate) || !_displayActive)
                return false;
            UnlinkInput();
            if (_lastMenu is not null && openLast)
            {
                _lastMenu.OpenMenu(true);
                _lastMenu = null;
            }

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

        protected virtual bool CancelOut()
        {
            CloseMenu(true);
            return true;
        }

        protected void DelayInput(float delay)
        {
            _nextNavigationUpdate = Time.unscaledTime + delay;
        }

        public void SetMenuCloseAction(Action action)
        {
            _menuCloseAction = action;
        }

        public IEnumerable<string> ExportLocalizationTag()
        {
            List<string> tags = new List<string>();
            tags.Add(menuTag);
            foreach (WindowUI window in _windowInstances)
            {
                tags.AddRange(window.ExportLocalizationTag());
            }

            return tags;
        }
    }
}