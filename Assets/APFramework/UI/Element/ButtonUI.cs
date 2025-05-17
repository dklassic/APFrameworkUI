using System;

namespace ChosenConcept.APFramework.Interface.Framework.Element
{
    public class ButtonUI : WindowElement
    {
        Action _action;
        protected bool _inFocus;
        Action _focusAction = null;
        public bool inFocus => _inFocus;
        public bool isSingleButtonWindow => _parentWindow.isSingleButtonWindow;

        public override string displayText
        {
            get
            {
                if (_inFocus)
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
            if (!isSingleButtonWindow)
            {
                _inFocus = setFocus;
                _parentWindow.InvokeUpdate();
            }

            _parentWindow.SetFocusAndAvailable(setFocus, _available);
            if (setFocus)
                _focusAction?.Invoke();
        }

        public override void SetAvailable(bool availability)
        {
            _available = availability;
            _parentWindow.SetFocusAndAvailable(_inFocus, _available);
            if (!isSingleButtonWindow)
                _parentWindow.InvokeUpdate();
        }
    }
}