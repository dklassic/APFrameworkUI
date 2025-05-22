
using ChosenConcept.APFramework.UI.Layout;
using ChosenConcept.APFramework.UI.Window;
using UnityEngine;

namespace ChosenConcept.APFramework.UI.Menu
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