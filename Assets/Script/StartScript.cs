using System;
using System.Collections;
using System.Collections.Generic;
using ChosenConcept.APFramework.Interface.Framework;
using UnityEngine;
using UnityEngine.InputSystem;

public class StartScript : MonoBehaviour
{
    bool _contextMenuOpen = false;
    IEnumerator Start()
    {
        CompositeMenuMono[] menus =
            FindObjectsByType<CompositeMenuMono>(FindObjectsInactive.Include, FindObjectsSortMode.None);
        foreach (CompositeMenuMono menu in menus)
        {
            menu.Initialize();
            WindowManager.instance.RegisterMenu(menu);
        }

        yield return null;
        WindowManager.instance.GetMenu<ExampleMenu>().OpenMenu(true);
    }
}