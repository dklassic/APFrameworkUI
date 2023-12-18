using System.Collections;
using System.Collections.Generic;
using UnityEditor.PackageManager.UI;
using UnityEngine;
using UnityEngine.UI;

public class ResolutionSetting : GeneralUISystemWithNavigation
{
    WindowUI systemWindow;
    List<(int, int)> activeResolutionList;
    protected override void InitializeUI()
    {
        systemWindow ??= NewWindow("ui_system_resolution", DefaultSetup);
        AddText("When ever resolution changes, need to clear cache of window location.", systemWindow);
        activeResolutionList = ResolutionUtility.AvailableResolutions();
        for (int i = 0; i < activeResolutionList.Count; i++)
        {
            (int width, int height) = activeResolutionList[i];
            ToggleUIExclusive toggle = AddExclusiveToggle(width + "x" + height, systemWindow, _ => UpdateResolution());
            if (width == Screen.width && height == Screen.height)
                toggle.Set = true;
        }
        AddGap(systemWindow);
        AddText("Both camera and overlay mode of canvas are supported.", systemWindow);
        SliderUIChoice uiMode = AddSliderWithChoice("Canvas Mode", systemWindow, ChangeCanvasMode);
        uiMode.AddChoice("Screen Space - Camera");
        uiMode.AddChoice("Screen Space - Overlay");
        systemWindow.Resize(50);
    }
    void UpdateResolution()
    {
        (int width, int height) = activeResolutionList[currentSelection[1]];
        ResolutionUtility.SetResolution(width, height);
        UIManager.Instance.ClearAllWindowLocation();
    }
    void ChangeCanvasMode(int mode)
    {
        UIManager.Instance.SetOverlayMode(mode == 1);
    }
}
