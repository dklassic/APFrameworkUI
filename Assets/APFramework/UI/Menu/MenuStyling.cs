
using ChosenConcept.APFramework.UI.Layout;
using ChosenConcept.APFramework.UI.Window;
using UnityEngine;

namespace ChosenConcept.APFramework.UI.Menu
{
    [System.Serializable]
    public struct MenuStyling
    {
        [SerializeField] WindowSetup _windowSetup;
        [SerializeField] LayoutSetup _layoutSetup;
        public WindowSetup windowSetup => _windowSetup;
        public LayoutSetup layoutSetup => _layoutSetup;

        public static MenuStyling defaultStyling => new()
        {
            _windowSetup = WindowSetup.defaultSetup,
            _layoutSetup = LayoutSetup.defaultLayout,
        };

        public MenuStyling SetWindowSetup(WindowSetup windowSetup)
        {
            _windowSetup = windowSetup;
            return this;
        }

        public MenuStyling SetLayoutSetup(LayoutSetup layoutSetup)
        {
            _layoutSetup = layoutSetup;
            return this;
        }
    }
}