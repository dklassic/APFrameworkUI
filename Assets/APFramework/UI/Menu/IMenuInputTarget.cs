using UnityEngine;

namespace ChosenConcept.APFramework.Interface.Framework
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