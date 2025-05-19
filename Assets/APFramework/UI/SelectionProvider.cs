using System.Collections.Generic;
using ChosenConcept.APFramework.Interface.Framework.Element;

namespace ChosenConcept.APFramework.Interface.Framework
{
    public class SelectionProvider : CompositeMenuMono
    {
        LayoutAlignment _layout;
        ISelectionInputTarget _target;

        protected override void InitializeUI()
        {
            _layout = InitNewLayout();
        }

        void UpdateChoices(List<string> choices)
        {
            ClearWindows(true);
            foreach (string choice in choices)
            {
                ButtonUI button = AddButton(choice, _layout);
                button.SetLabel(choice);
                button.SetAction(CompleteInput);
                button.parentWindow.AutoResize();
            }

            ClearWindowLocation();
        }

        public void GetSingleSelection(ISelectionInputTarget target, List<string> choices, int currentChoice)
        {
            UpdateChoices(choices);
            // take away the input from the sourceUI
            _target = target;
            OpenMenu(null);
            currentSelectable?.SetFocus(false);
            _currentSelection[0] = currentChoice;
            currentSelectable?.SetFocus(true);
        }

        void CompleteInput()
        {
            CloseMenu(false);
            _target.SetSelection(_currentSelection[0]);
            _target = null;
        }
    }
}