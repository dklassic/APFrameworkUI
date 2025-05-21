using System;
using System.Collections;
using System.Collections.Generic;
using ChosenConcept.APFramework.Interface.Framework.Element;
using UnityEngine;

namespace ChosenConcept.APFramework.Interface.Framework
{
    public class ContextMenuProvider : MonoBehaviour
    {
        [SerializeField] MenuSetup _menuSetup;
        [SerializeField] MenuStyling _menuStyling;
        [SerializeField] bool _contextMenuInstantiated = false;
        [SerializeField] SimpleMenu _contextMenu;
        [SerializeField] WindowUI _window;
        [SerializeField] bool _active;
        public bool active => _active;

        public void Initialize()
        {
            _contextMenu = new SimpleMenu("ContextMenuProvider", _menuSetup, _menuStyling);
            _window = _contextMenu.NewWindow("ContextMenu");
            WindowManager.instance.RegisterMenu(_contextMenu);
            _contextMenuInstantiated = true;
        }

        public void SetupMenu(List<string> choices, List<Action> actions, Vector2 position, Action onClose)
        {
            if (!_contextMenuInstantiated)
                return;

            if (choices.Count != actions.Count)
            {
                Debug.LogError("Mismatch amount of choices and actions");
                return;
            }

            _active = true;
            _window.RevertAlignment();
            _window.ClearElements();
            _window.ClearCachedPosition();
            for (int i = 0; i <= choices.Count - 1; i++)
            {
                _window.AddButton(choices[i], actions[i]);
            }

            _contextMenu.SetMenuCloseAction(() =>
            {
                _active = false;
                WindowManager.instance.EndContextMenu();
                onClose.Invoke();
            });
            _contextMenu.OpenMenu(true);
            _window.MoveTo(position);
        }

        public void SetupMenu(List<string> choices, List<Action> actions, Action onClose)
        {
            if (!_contextMenuInstantiated)
                return;

            if (choices.Count != actions.Count)
            {
                Debug.LogError("Mismatch amount of choices and actions");
                return;
            }

            _active = true;
            _window.RevertAlignment();
            _window.ClearElements();
            _window.ClearCachedPosition();
            for (int i = 0; i <= choices.Count - 1; i++)
            {
                _window.AddButton(choices[i], actions[i]);
            }

            _contextMenu.SetMenuCloseAction(() =>
            {
                _active = false;
                WindowManager.instance.EndContextMenu();
                onClose.Invoke();
            });
            _contextMenu.OpenMenu(true);
        }

        public void UpdateMenu()
        {
            _contextMenu.UpdateMenu();
        }
    }
}