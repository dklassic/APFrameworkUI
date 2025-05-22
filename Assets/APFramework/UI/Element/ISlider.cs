using UnityEngine;

namespace ChosenConcept.APFramework.Interface.Framework.Element
{
    public interface ISlider
    {
        (bool hoverOnDecrease, bool hoverOnIncrease) HoverOnArrow(Vector2 lastMousePosition);
        int firstSliderArrowIndex { get; }
        int lastSliderArrowIndex { get; }
        void SetCachedArrowPosition((Vector2, Vector2) arrowPosition);
        void SetInput(bool inInput);
    }
}