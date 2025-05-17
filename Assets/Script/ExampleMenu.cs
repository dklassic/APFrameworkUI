using ChosenConcept.APFramework.Interface.Framework;
using UnityEngine;

public class ExampleMenu : CompositeMenuMono
{
    protected override void InitializeUI()
    {
        WindowUI systemWindow = NewWindow("Example Menu", WindowSetup.defaultSetup);
        AddButton("All window elements available", systemWindow, () =>
        {
            CloseMenu(false);
            WindowManager.instance.GetMenu<ElementsShowcase>().OpenMenu(this, true);
        });
        AddButton("Multiple window example", systemWindow, () =>
        {
            CloseMenu(false);
            WindowManager.instance.GetMenu<MultipleWindow>().OpenMenu(this, true);
        });
        AddButton("中文顯示", systemWindow, () =>
        {
            CloseMenu(false);
            WindowManager.instance.GetMenu<ChineseDisplay>().OpenMenu(this, true);
        });
        AddButton("Setting", systemWindow, () =>
        {
            CloseMenu(false);
            WindowManager.instance.GetMenu<ResolutionSetting>().OpenMenu(this, true);
        });
        AddButton("Code Initialized CompositeMenu", systemWindow, () =>
        {
            CloseMenu(false);
            FindAnyObjectByType<CompositeMenuInstance>().OpenMenu();
        });
        AddButton("Code Initialized SimpleMenus", systemWindow, () =>
        {
            CloseMenu(false);
            FindAnyObjectByType<SimpleMenuInstance>().OpenMenu();
        });
        AddGap(systemWindow);
        AddButton("Quit", systemWindow, Quit);
        systemWindow.AutoResize();
    }

    void Quit()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }
}