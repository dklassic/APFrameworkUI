using System;

namespace ChosenConcept.APFramework.Interface.Framework
{
    [Serializable]
    public struct MenuSetup
    {
        public bool allowCycleWithinWindow;
        public bool allowCycleBetweenWindows;
        public bool allowCloseMenuWithCancelAction;
        public UISystemCloseOnClickBehavior allowCloseOnClick;
        public UISystemResetOnOpenBehavior resetOnOpen;
        public bool allowDraggingWithMouse;
        public bool allowNavigationOnOpen;
        public float menuOpenInputDelay;
        public bool singlePressOnly;
        public float holdNavigationDelay;
        public float holdNavigationInterval;
        public float holdNavigationSpeedUpInterval;


        public static MenuSetup defaultSetup => new()
        {
            allowCycleWithinWindow = false,
            allowCycleBetweenWindows = false,
            allowCloseMenuWithCancelAction = false,
            allowCloseOnClick = UISystemCloseOnClickBehavior.Disable,
            resetOnOpen = UISystemResetOnOpenBehavior.ClearSelection,
            allowNavigationOnOpen = true,
            allowDraggingWithMouse = false,
            menuOpenInputDelay = .2f,
            singlePressOnly = false,
            holdNavigationDelay = 0.5f,
            holdNavigationInterval = 0.2f,
            holdNavigationSpeedUpInterval = 2f,
        };
    }
}