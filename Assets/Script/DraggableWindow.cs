using ChosenConcept.APFramework.Interface.Framework;
using UnityEngine;

public class DraggableWindow : CompositeMenuMono
{
    protected override void InitializeUI()
    {
        WindowUI window = NewWindow("Window");
        window.AddText("This Window can be dragged!");
        AddGap(window);
        window.AddButton("Return", () => { CloseMenu(true); });
    }
}