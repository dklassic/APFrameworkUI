using ChosenConcept.APFramework.Interface.Framework;
using UnityEngine;

public class ExampleMenu : CompositeMenuMono
{
    protected override void InitializeUI()
    {
        LayoutAlignment layout = InitNewLayout();
        AddButton("All window elements available", layout, () =>
        {
            CloseMenu(false);
            WindowManager.instance.GetMenu<ElementsShowcase>().OpenMenu(this, true);
        });
        AddButton("Multiple window example (Horizontal)", layout, () =>
        {
            CloseMenu(false);
            WindowManager.instance.GetMenu<MultipleWindowHorizontal>().OpenMenu(this, true);
        });
        AddButton("Multiple window example (Vertical)", layout, () =>
        {
            CloseMenu(false);
            WindowManager.instance.GetMenu<MultipleWindowVertical>().OpenMenu(this, true);
        });
        AddButton("中文顯示", layout, () =>
        {
            CloseMenu(false);
            WindowManager.instance.GetMenu<ChineseDisplay>().OpenMenu(this, true);
        });
        AddButton("Setting", layout, () =>
        {
            CloseMenu(false);
            WindowManager.instance.GetMenu<ResolutionSetting>().OpenMenu(this, true);
        });
        AddButton("Code Initialized SimpleMenus", layout, () =>
        {
            CloseMenu(false);
            FindAnyObjectByType<SimpleMenuInstance>().OpenMenu();
        });
        AddButton("Context Menu Example", layout, () =>
        {
            CloseMenu(false);
            FindAnyObjectByType<ContextMenuExample>().Open();
        });
        AddButton("Draggable Window Example", layout, () =>
        {
            CloseMenu(false);
            FindAnyObjectByType<DraggableWindow>().OpenMenu(this);
        });
        AddButton("Quit", layout, Quit);
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