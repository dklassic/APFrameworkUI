using ChosenConcept.APFramework.UI;
using ChosenConcept.APFramework.UI.Layout;
using ChosenConcept.APFramework.UI.Menu;
using UnityEngine;

public class ExampleMenu : CompositeMenuMono
{
    protected override void InitializeMenu()
    {
        LayoutAlignment layout = InitNewLayout();
        AddButton("All window elements available", layout, OpenSubMenu<ElementsShowcase>);
        AddButton("Update value via function", layout, OpenSubMenu<FunctionWindow>);
        AddButton("Multiple window example (Horizontal)", layout, OpenSubMenu<MultipleWindowHorizontal>);
        AddButton("Multiple window example (Vertical)", layout, OpenSubMenu<MultipleWindowVertical>);
        AddButton("Chinese 中文顯示", layout, OpenSubMenu<ChineseDisplay>);
        AddButton("Setting", layout, OpenSubMenu<ResolutionSetting>);
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
        AddButton("Draggable Window Example", layout, OpenSubMenu<DraggableWindow>);
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