using System.Collections;
using ChosenConcept.APFramework.UI;
using UnityEngine;

public class StartScript : MonoBehaviour
{
    IEnumerator Start()
    {
        // Wait a frame for menus to be initialized
        yield return null;
        WindowManager.instance.GetMenu<ExampleMenu>().OpenMenu(true);
    }
}