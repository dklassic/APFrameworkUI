using System;
using System.Collections.Generic;
using ChosenConcept.APFramework.Interface.Framework.Element;
using UnityEngine;

namespace ChosenConcept.APFramework.Interface.Framework
{
    public class ContextMenuProvider : MonoBehaviour
    {
        [SerializeField] MenuSetup _menuSetup;
        [SerializeField] bool _contextMenuInstantiated = false;
        [SerializeField] SimpleMenu _contextMenu;
        [SerializeField] WindowUI _window;

        public void SetupMenu(List<string> choices, List<Action> actions, Vector2 position, Action onClose)
        {
            if (!_contextMenuInstantiated)
            {
                _contextMenuInstantiated = true;
                _contextMenu = new SimpleMenu("ContextMenuProvider", _menuSetup);
                _window = _contextMenu.NewWindow("ContextMenu");
                WindowManager.instance.RegisterMenu(_contextMenu);
            }

            _window.RevertAlignment();
            if (choices.Count != actions.Count)
            {
                Debug.LogError("Mismatch amount of choices and actions");
                return;
            }

            _window.ClearElements();
            _window.ClearCachedPosition();
            for (int i = 0; i <= choices.Count - 1; i++)
            {
                _window.AddButton(choices[i], actions[i]);
            }
            _window.AutoResize();

            _window.GetComponent<RectTransform>().sizeDelta =
                new Vector2(_window.layout.minWidth, _window.layout.minHeight);
            _contextMenu.SetMenuCloseAction(onClose);
            _contextMenu.OpenMenu(true);
            _window.MoveTo(position);
        }
    }
}