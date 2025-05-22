using System;
using ChosenConcept.APFramework.UI.Element;
using ChosenConcept.APFramework.UI.Layout;
using ChosenConcept.APFramework.UI.Menu;
using ChosenConcept.APFramework.UI.Window;
using UnityEngine;

namespace ChosenConcept.APFramework.UI.Provider
{
    public class ConfirmationProvider : CompositeMenuMono
    {
        LayoutAlignment _layout;
        ConfirmationDefaultChoice _defaultChoice;
        bool _hasCancel;
        public bool active => _displayActive;

        protected override void InitializeMenu()
        {
            _layout = InitNewLayout();
        }

        public void GetConfirm(string title, string message, string confirm,
            string cancel, Action onConfirm,
            Action onCancel, ConfirmationDefaultChoice defaultChoice)
        {
            ClearWindows(true);
            WindowSetup messageSetup = WindowSetup.defaultSetup;
            messageSetup.SetTitleStyle(WindowTitleStyle.EmbeddedTitle);
            messageSetup.SetOutlineDisplayStyle(WindowOutlineDisplayStyle.Always);
            WindowUI messageWindow = NewWindow("Message", _layout, messageSetup);
            messageWindow.SetLabel(title);
            messageWindow.AddText("Message").SetLabel(message);
            messageWindow.Resize(50);
            ButtonUI confirmButton = AddButton("Confirm", _layout, () =>
            {
                WindowManager.instance.EndConfirm();
                CloseMenu(true);
                onConfirm.Invoke();
            });
            confirmButton.SetLabel(confirm);
            confirmButton.AutoResize();
            _hasCancel = cancel != null;
            if (_hasCancel)
            {
                ButtonUI cancelButton = AddButton("Cancel", _layout, () =>
                {
                    WindowManager.instance.EndConfirm();
                    CloseMenu(true);
                    onCancel.Invoke();
                });
                cancelButton.SetLabel(cancel);
                cancelButton.AutoResize();
            }

            OpenMenu(true);
            _defaultChoice = defaultChoice;
            _currentSelection = _defaultChoice switch
            {
                ConfirmationDefaultChoice.Confirm => new Vector2Int(1, 0),
                ConfirmationDefaultChoice.Cancel => new Vector2Int(2, 0),
                ConfirmationDefaultChoice.None => new Vector2Int(-1, -1),
                _ => throw new ArgumentOutOfRangeException(nameof(_defaultChoice), _defaultChoice, null)
            };
            currentSelectable?.SetFocus(true);
        }

        protected override bool CancelOut()
        {
            if (!_hasCancel)
                return true;
            _currentSelection = new Vector2Int(2, 0);
            currentSelectable?.SetFocus(true);
            return true;
        }
    }
}