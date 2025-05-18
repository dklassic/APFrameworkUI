using ChosenConcept.APFramework.Interface.Framework;
using UnityEngine;

public class StartScript : MonoBehaviour
{
    void Start()
    {
        CompositeMenuMono[] menus =
            FindObjectsByType<CompositeMenuMono>(FindObjectsInactive.Include, FindObjectsSortMode.None);
        foreach (CompositeMenuMono menu in menus)
        {
            menu.Initialize();
            WindowManager.instance.RegisterMenu(menu);
        }

        WindowManager.instance.GetMenu<ExampleMenu>().OpenMenu(true);
    }
}