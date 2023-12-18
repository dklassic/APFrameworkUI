using System.Collections;
using System.Collections.Generic;
using UnityEditor.PackageManager.UI;
using UnityEngine;
using UnityEngine.UI;

public class ExampleMenu : GeneralUISystemWithNavigation
{
    IEnumerator Start()
    {
        while (!UIManager.Instance.DefaultUIStarted)
            yield return null;
        OpenMenu();
    }

    protected override void InitializeUI()
    {
        WindowUI systemWindow = NewWindow("Example Menu", DefaultSetup);
        AddButton("All window elements available", systemWindow, () => OpenSubMenu(0));
        AddButton("Multiple window example", systemWindow, () => OpenSubMenu(1));
        AddButton("中文顯示", systemWindow, () => OpenSubMenu(2));
        AddButton("Setting", systemWindow, () => OpenSubMenu(3));
        systemWindow.AutoResize();
    }
}
