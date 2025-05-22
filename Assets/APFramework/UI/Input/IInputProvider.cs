using ChosenConcept.APFramework.UI.Menu;
using UnityEngine;

namespace ChosenConcept.APFramework.UI.Input
{
    public interface IInputProvider
    {
        bool hasMouse { get; }
        Vector2 mousePosition { get; }
        Vector2 mouseDelta { get; }
        bool inputEnabled { get; }
        void Update();
        void SetTarget(IMenuInputTarget target);
        void EnableInput(bool enable);
    }
}