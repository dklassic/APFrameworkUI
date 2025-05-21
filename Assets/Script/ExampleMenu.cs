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
        AddButton("Multiple window example (Horizontal)", systemWindow, () =>
        {
            CloseMenu(false);
            WindowManager.instance.GetMenu<MultipleWindowHorizontal>().OpenMenu(this, true);
        });
        AddButton("Multiple window example (Vertical)", systemWindow, () =>
        {
            CloseMenu(false);
            WindowManager.instance.GetMenu<MultipleWindowVertical>().OpenMenu(this, true);
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
        AddButton("Code Initialized SimpleMenus", systemWindow, () =>
        {
            CloseMenu(false);
            FindAnyObjectByType<SimpleMenuInstance>().OpenMenu();
        });
        AddButton("Context Menu Example", systemWindow, () =>
        {
            CloseMenu(false);
            FindAnyObjectByType<ContextMenuExample>().Open();
        });
        AddGap(systemWindow);
        AddButton("Quit", systemWindow, Quit);
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