using System;
using UnityEngine;

namespace ChosenConcept.APFramework.UI.Menu
{
    [Serializable]
    public struct MenuSetup
    {
        [SerializeField] bool _allowCycleWithinWindow;
        [SerializeField] bool _allowCycleBetweenWindows;
        [SerializeField] bool _allowCloseMenuWithCancelAction;
        [SerializeField] MenuCloseOnClickBehavior _allowCloseOnClick;
        [SerializeField] MenuResetOnOpenBehavior _resetOnOpen;
        [SerializeField] bool _allowDraggingWithMouse;
        [SerializeField] bool _allowNavigationOnOpen;
        [SerializeField] float _menuOpenInputDelay;
        [SerializeField] bool _singlePressOnly;
        [SerializeField] float _holdNavigationDelay;
        [SerializeField] float _holdNavigationInterval;
        [SerializeField] float _holdNavigationSpeedUpInterval;
        [SerializeField] float _functionStringLabelUpdateInterval;

        public bool allowCycleWithinWindow => _allowCycleWithinWindow;
        public bool allowCycleBetweenWindows => _allowCycleBetweenWindows;
        public bool allowCloseMenuWithCancelAction => _allowCloseMenuWithCancelAction;
        public MenuCloseOnClickBehavior allowCloseOnClick => _allowCloseOnClick;
        public MenuResetOnOpenBehavior resetOnOpen => _resetOnOpen;
        public bool allowDraggingWithMouse => _allowDraggingWithMouse;
        public bool allowNavigationOnOpen => _allowNavigationOnOpen;
        public float menuOpenInputDelay => _menuOpenInputDelay;
        public bool singlePressOnly => _singlePressOnly;
        public float holdNavigationDelay => _holdNavigationDelay;
        public float holdNavigationInterval => _holdNavigationInterval;
        public float holdNavigationSpeedUpInterval => _holdNavigationSpeedUpInterval;
        public float functionStringLabelUpdateInterval => _functionStringLabelUpdateInterval;


        public static MenuSetup defaultSetup => new()
        {
            _allowCycleWithinWindow = false,
            _allowCycleBetweenWindows = false,
            _allowCloseMenuWithCancelAction = false,
            _allowCloseOnClick = MenuCloseOnClickBehavior.Disable,
            _resetOnOpen = MenuResetOnOpenBehavior.ClearSelection,
            _allowNavigationOnOpen = true,
            _allowDraggingWithMouse = false,
            _menuOpenInputDelay = .2f,
            _singlePressOnly = false,
            _holdNavigationDelay = 0.5f,
            _holdNavigationInterval = 0.2f,
            _holdNavigationSpeedUpInterval = 2f,
            _functionStringLabelUpdateInterval = .1f,
        };

        public MenuSetup SetAllowCycleWithinWindow(bool allowCycleWithinWindow)
        {
            _allowCycleWithinWindow = allowCycleWithinWindow;
            return this;
        }

        public MenuSetup SetAllowCycleBetweenWindows(bool allowCycleBetweenWindows)
        {
            _allowCycleBetweenWindows = allowCycleBetweenWindows;
            return this;
        }

        public MenuSetup SetAllowCloseMenuWithCancelAction(bool allowCloseMenuWithCancelAction)
        {
            _allowCloseMenuWithCancelAction = allowCloseMenuWithCancelAction;
            return this;
        }

        public MenuSetup SetAllowCloseOnClick(MenuCloseOnClickBehavior behavior)
        {
            _allowCloseOnClick = behavior;
            return this;
        }

        public MenuSetup SetAllowNavigationOnOpen(bool allowNavigationOnOpen)
        {
            _allowNavigationOnOpen = allowNavigationOnOpen;
            return this;
        }

        public MenuSetup SetAllowDraggingWithMouse(bool allowDraggingWithMouse)
        {
            _allowDraggingWithMouse = allowDraggingWithMouse;
            return this;
        }

        public MenuSetup SetSinglePressOnly(bool singlePressOnly)
        {
            _singlePressOnly = singlePressOnly;
            return this;
        }
    }
}