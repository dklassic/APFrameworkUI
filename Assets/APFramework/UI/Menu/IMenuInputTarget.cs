using UnityEngine;

namespace ChosenConcept.APFramework.UI.Menu
{
    public interface IMenuInputTarget
    {
        void OnConfirm();
        void OnCancel();
        void OnMove(Vector2 move);
        void OnScroll(Vector2 scroll);
        void OnMouseConfirmPressed();
        void OnMouseConfirmReleased();
        void OnMouseCancel();
        void OnKeyboardEscape();
        void SetSelection(int i);
        void SetTextInput(string inputFieldText);
    }
}