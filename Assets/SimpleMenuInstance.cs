using System.Collections.Generic;
using System.Linq;
using ChosenConcept.APFramework.Interface;
using ChosenConcept.APFramework.Interface.Framework;
using UnityEngine;

public class SimpleMenuInstance : MonoBehaviour
{
    [SerializeField] List<LayoutSetup> _layoutSetups = new();
    [SerializeField] List<SimpleMenu> _simpleMenus = new();
    bool _active;

    void Start()
    {
        int i = 0;
        foreach (var layout in _layoutSetups)
        {
            MenuSetup setup = MenuSetup.defaultSetup;
            setup.cancelOutAllowed = true;
            SimpleMenu menu = new(i.ToString(), MenuSetup.defaultSetup, WindowSetup.defaultSetup, layout);
            _simpleMenus.Add(menu);
            menu.AddText("Close all menu to quit");
            menu.AddSlider("slider");
            menu.AddButton("close", () => menu.CloseMenu());
            menu.AutoResizeWindow();
            WindowManager.instance.RegisterMenu(menu);
            i++;
        }
    }

    void Update()
    {
        if (!_active)
            return;
        if (!_simpleMenus.Any(x => x.isDisplayActive))
        {
            _active = false;
            WindowManager.instance.GetMenu<ExampleMenu>().OpenMenu(true);
        }
    }

    public void OpenMenu()
    {
        _active = true;
        foreach (SimpleMenu menu in _simpleMenus)
        {
            menu.OpenMenu(true);
        }
    }
}