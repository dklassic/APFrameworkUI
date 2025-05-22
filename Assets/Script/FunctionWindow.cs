using ChosenConcept.APFramework.UI.Menu;
using ChosenConcept.APFramework.UI.Window;
using UnityEngine;

public class FunctionWindow : CompositeMenuMono
{
    protected override void InitializeMenu()
    {
        WindowUI systemWindow = NewWindow("Function Window", WindowSetup.defaultSetup);
        systemWindow.AddText("Time.time")
            .SetContent(() => Time.time.ToString("F2"));
        systemWindow.AddText("Time.frameCount")
            .SetContent(() => Time.frameCount.ToString());
    }
}