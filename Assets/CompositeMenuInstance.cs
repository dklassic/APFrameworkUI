using ChosenConcept.APFramework.Interface;
using ChosenConcept.APFramework.Interface.Framework;
using UnityEngine;

public class CompositeMenuInstance : MonoBehaviour
{
    [SerializeField] CompositeMenu _compositeMenu;
    void Start()
    {
        _compositeMenu = new CompositeMenu("CompositeMenuInstance", MenuSetup.defaultSetup, WindowSetup.defaultSetup,
            LayoutSetup.defaultLayout);
        LayoutAlignment layout = _compositeMenu.InitializeNewLayout();
        WindowUI window1 = _compositeMenu.NewWindow("Window1", layout, WindowSetup.defaultSetup);
        window1.AddButton("Test");
        window1.AddButton("Test");
        window1.AddButton("Test");
        window1.AutoResize();
        WindowUI window2 = _compositeMenu.NewWindow("Window2", layout, WindowSetup.defaultSetup);
        window2.AddButton("Test");
        window2.AddButton("Test");
        window2.AddButton("Test");
        window2.AutoResize();
        WindowUI window3 = _compositeMenu.NewWindow("Window2", layout, WindowSetup.defaultSetup);
        window3.AddButton("Return", () =>
        {
            _compositeMenu.CloseMenu();
            WindowManager.instance.GetMenu<ExampleMenu>().OpenMenu(true);
        });
        window3.AutoResize();
        WindowManager.instance.RegisterMenu(_compositeMenu);
    }

    public void OpenMenu()
    {
        _compositeMenu.OpenMenu(true);
    }
}