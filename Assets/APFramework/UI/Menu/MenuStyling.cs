using ChosenConcept.APFramework.Interface.Framework;
using UnityEngine;

namespace ChosenConcept.APFramework.Interface
{
    [System.Serializable]
    public struct MenuStyling
    {
        [SerializeField] public WindowSetup windowSetup;
        [SerializeField] public LayoutSetup layoutSetup;
        public static MenuStyling defaultStyling => new()
        {
            windowSetup = WindowSetup.defaultSetup,
            layoutSetup = LayoutSetup.defaultLayout,
        };
    }
}