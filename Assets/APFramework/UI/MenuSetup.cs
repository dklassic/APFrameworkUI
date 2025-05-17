using System;

namespace ChosenConcept.APFramework.Interface.Framework
{
    [Serializable]
    public struct MenuSetup
    {
        public UISystemNavigationDirection navigationDirection;
        public UISystemSliderDirection slideDirection;
        public bool allowCycle;
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
            navigationDirection = UISystemNavigationDirection.TwoWay,
            slideDirection = UISystemSliderDirection.X,
            allowCycle = false,
            cancelOutAllowed = false,
            resetOnOpen = UISystemResetOnOpenBehavior.Disable,
            allowNavigationOnOpen = true,
            menuOpenInputDelay = .2f,
            singlePressOnly = false,
            holdNavigationDelay = 0.5f,
            holdNavigationInterval = 0.2f,
            holdNavigationSpeedUpInterval = 2f,
        };
    }
}