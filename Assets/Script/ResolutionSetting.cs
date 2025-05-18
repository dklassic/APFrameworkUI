using System.Collections.Generic;
using System.Linq;
using ChosenConcept.APFramework.Interface.Framework;
using ChosenConcept.APFramework.Interface.Framework.Element;
using UnityEngine;

public class ResolutionSetting : CompositeMenuMono
{
    WindowUI systemWindow;

    protected override void InitializeUI()
    {
        systemWindow ??= NewWindow("Resolution", WindowSetup.defaultSetup);
        AddText("When ever resolution changes, need to clear cache of window location.", systemWindow);
        List<(int, int)> activeResolutionList = ResolutionUtility.AvailableResolutions();
        SingleSelectionUI<(int, int)> resolutionChoice =
            AddSingleSelection<(int, int)>("Resolution", systemWindow, UpdateResolution);
        resolutionChoice.SetChoiceValue(activeResolutionList);
        resolutionChoice.SetChoice(activeResolutionList.Select(x => x.Item1 + "x" + x.Item2).ToList());
        resolutionChoice.SetActiveValue((Screen.width, Screen.height));
        AddGap(systemWindow);
        AddText("Both camera and overlay mode of canvas are supported.", systemWindow);
        SliderUIChoice<RenderMode> uiMode =
            AddSliderWithChoice<RenderMode>("CanvasMode", systemWindow, ChangeCanvasMode);
        uiMode.AddChoice(nameof(RenderMode.ScreenSpaceOverlay));
        uiMode.AddChoice(nameof(RenderMode.ScreenSpaceCamera));
        uiMode.AddChoiceValue(RenderMode.ScreenSpaceOverlay);
        uiMode.AddChoiceValue(RenderMode.ScreenSpaceCamera);
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