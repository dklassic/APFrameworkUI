using UnityEngine;

namespace ChosenConcept.APFramework.Interface.Framework
{
    public interface IMenuInputTarget
    {
        void OnConfirm();
        void OnCancel();
        void OnMove(Vector2 move);
        void OnScroll(Vector2 scroll);
        void OnMouseConfirm();
        void OnMouseCancel();
        void OnKeyboardEscape();
    }
}