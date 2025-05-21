using System;
using UnityEngine;

namespace ChosenConcept.APFramework.Interface.Framework.Element
{
    public class ButtonUI : WindowElement
    {
        Action _action;
        Action _focusAction = null;
        protected bool _inFocus;
        public bool inFocus => _inFocus;

        public override string displayText
        {
            get
            {
                if (_inFocus && !_parentWindow.isSingleButtonWindow)
                    return StyleUtility.StringColored(TextUtility.StripRichTagsFromStr(formattedContent),
                        _available ? StyleUtility.Selected : StyleUtility.DisableSelected);
                return _available
                    ? formattedContent
                    : StyleUtility.StringColored(formattedContent, StyleUtility.Disabled);
            }
        }

        public ButtonUI(string content, WindowUI parent) : base(content, parent)
        {
        }

        public void ClearFocus()
        {
            if (!_inFocus)
                return;
            _inFocus = false;
            _parentWindow.InvokeUpdate();
        }

        public void SetAction(Action action) => _action = action;

        public virtual void TriggerAction()
        {
            if (_action == null)
                return;
            _action.Invoke();
        }

        public virtual void SetFocus(bool setFocus)
        {
            if (_inFocus == setFocus)
                return;
            _inFocus = setFocus;
            _parentWindow.InvokeUpdate();

            if (setFocus)
                _focusAction?.Invoke();
        }

        public override void SetAvailable(bool availability)
        {
            if(availability == _available)
                return;
            _available = availability;
            _parentWindow.InvokeUpdate();
        }
    }
}