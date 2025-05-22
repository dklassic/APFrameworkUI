using System.Collections.Generic;
using System.Linq;
using ChosenConcept.APFramework.Interface.Framework;
using ChosenConcept.APFramework.Interface.Framework.Element;
using UnityEngine;

public class ResolutionSetting : CompositeMenuMono
{
    protected override void InitializeMenu()
    {
        WindowUI systemWindow = NewWindow("Resolution", WindowSetup.defaultSetup);
        systemWindow.AddText("When ever resolution changes, need to clear cache of window location.");
        List<(int, int)> activeResolutionList = ResolutionUtility.AvailableResolutions();
        systemWindow.AddSingleSelection<(int, int)>("Resolution", UpdateResolution)
            .SetChoice(activeResolutionList.Select(x => x.Item1 + "x" + x.Item2).ToList(), activeResolutionList)
            .SetActiveValue((Screen.width, Screen.height));
        systemWindow.AddGap();
        systemWindow.AddText("Both camera and overlay mode of canvas are supported.");
        systemWindow.AddSlider<RenderMode>("CanvasMode", ChangeCanvasMode)
            .AddChoiceByValue(RenderMode.ScreenSpaceOverlay)
            .AddChoiceByValue(RenderMode.ScreenSpaceCamera)
            .SetActiveValue(WindowManager.instance.overlayMode);
        systemWindow.Resize(50);
    }

    void UpdateResolution((int, int) resolution)
    {
        ResolutionUtility.SetResolution(resolution.Item1, resolution.Item2);
        WindowManager.instance.ClearAllWindowLocation();
    }

    void ChangeCanvasMode(RenderMode mode)
    {
        WindowManager.instance.SetOverlayMode(mode);
    }
}