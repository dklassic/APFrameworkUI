using System.Collections;
using System.Collections.Generic;
using UnityEditor.PackageManager.UI;
using UnityEngine;
using UnityEngine.UI;

public class MultipleWindow : GeneralUISystemWithNavigation
{
    protected override void InitializeUI()
    {
        Transform layout = UIManager.Instance.InstantiateLayout(init.Group, nameof(MultipleWindow));

        WindowUI window1 = NewWindow("Window 1", layout, DefaultSetup);
        AddButton("Button from Window 1", window1);
        AddButton("Button from Window 1", window1);
        AddButton("Button from Window 1", window1);
        window1.AutoResize();

        WindowUI window2 = NewWindow("Window 2", layout, DefaultSetup);
        AddButton("Button from Window 2", window2);
        AddButton("Button from Window 2", window2);
        AddButton("Button from Window 2", window2);
        window2.AutoResize();

        WindowUI window3 = NewWindow("Window 3", layout, DefaultSetup);
        AddButton("Button from Window 3", window3);
        AddButton("Button from Window 3", window3);
        AddButton("Button from Window 3", window3);
        window3.AutoResize();
    }
}
