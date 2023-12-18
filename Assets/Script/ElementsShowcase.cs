using System.Collections;
using System.Collections.Generic;
using UnityEditor.PackageManager.UI;
using UnityEngine;
using UnityEngine.UI;

public class ElementsShowcase : GeneralUISystemWithNavigation
{
    protected override void InitializeUI()
    {
        WindowUI systemWindow = NewWindow("Elements Showcase", DefaultSetup);
        AddToggle("This is a Toggle", systemWindow);
        AddButton("This is a Button", systemWindow);
        AddButtonWithCount("This is a Button that increases per confirm and decreases per cancel", systemWindow);
        AddGap(systemWindow);

        SliderUI slider = AddSlider("This is a simple Slider with range", systemWindow);
        slider.SetLimit(-10, 10);
        SliderUIChoice sliderChoice = AddSliderWithChoice("This is a Slider that takes string options", systemWindow);
        sliderChoice.AddChoice("Low");
        sliderChoice.AddChoice("Medium");
        sliderChoice.AddChoice("High");
        sliderChoice.AddChoice("Ultra");
        sliderChoice.SetCountNoAction(2);

        AddGap(systemWindow);
        AddExclusiveToggle("All these toggles", systemWindow);
        AddExclusiveToggle("Can only have one", systemWindow);
        AddExclusiveToggle("Active at a time", systemWindow);

        AddGap(systemWindow);
        ButtonUI disabledButton = AddButton("You can also disable interaction", systemWindow);
        disabledButton.SetAvailable(false);
        AddGap(systemWindow);
        AddText("Here is a Button that takes double confirm to trigger:", systemWindow);
        AddDoubleConfirmButton("Double confirm to return", systemWindow, () => CloseMenu(false, true));
        systemWindow.AutoResize();
    }
}
