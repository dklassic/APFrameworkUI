using System;
using System.Collections.Generic;
using ChosenConcept.APFramework.Interface.Framework;
using UnityEngine;
using UnityEngine.InputSystem;

public class ContextMenuExample : MonoBehaviour
{
    [SerializeField] SimpleMenu _returnButton;
    bool _contextMenuOpen;
    bool _exampleOpen;
    bool _exampleMenuInstantiated;

    public void Open()
    {
        _exampleOpen = true;
        if (!_exampleMenuInstantiated)
        {
            _exampleMenuInstantiated = true;
            _returnButton = new SimpleMenu("Return");
            _returnButton.AddText("Mouse right click to open a context menu");
            _returnButton.AddButton("Return", () =>
            {
                _exampleOpen = false;
                _returnButton.CloseMenu();
                WindowManager.instance.GetMenu<ExampleMenu>().OpenMenu(true);
            });
            WindowManager.instance.RegisterMenu(_returnButton);
        }

        _returnButton.OpenMenu(true);
    }

    void Update()
    {
        if (!_exampleOpen)
            return;
        Mouse mouse = Mouse.current;
        if (mouse != null)
        {
            if (!_contextMenuOpen && mouse.rightButton.wasPressedThisFrame)
            {
                _contextMenuOpen = true;
                List<string> choices = new List<string>();
                List<Action> actions = new List<Action>();
                for (int i = 0; i <= 5; i++)
                {
                    choices.Add(i.ToString());
                    actions.Add(() => Debug.Log("Clicked"));
                }

                WindowManager.instance.GetContextMenu(choices, actions, mouse.position.ReadValue(),
                    () => { _contextMenuOpen = false; });
            }
        }
    }
}