using System;

namespace ChosenConcept.APFramework.Interface.Framework
{
    [Serializable]
    public struct MenuSetup
    {
        public bool allowCycleWithinWindow;
        public bool allowCycleBetweenWindows;
        public bool cancelOutAllowed;
        public UISystemResetOnOpenBehavior resetOnOpen;
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
            cancelOutAllowed = false,
            resetOnOpen = UISystemResetOnOpenBehavior.ClearSelection,
            allowNavigationOnOpen = true,
            menuOpenInputDelay = .2f,
            singlePressOnly = false,
            holdNavigationDelay = 0.5f,
            holdNavigationInterval = 0.2f,
            holdNavigationSpeedUpInterval = 2f,
        };
    }
}