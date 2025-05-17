using UnityEngine;

namespace ChosenConcept.APFramework.Interface.Framework
{
    public interface IInputProvider
    {
        bool hasMouse { get; }
        Vector2 mousePosition { get; }
        bool inputEnabled { get; }
        void Update();
        void SetTarget(IMenuInputTarget target);
        void EnableInput(bool enable);
    }
}