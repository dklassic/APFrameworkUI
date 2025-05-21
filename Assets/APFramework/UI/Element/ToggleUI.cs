using System;

namespace ChosenConcept.APFramework.Interface.Framework.Element
{
    public class ToggleUI : WindowElement<ToggleUI>
    {
        protected bool _toggledOn;
        Action<bool> _action;

        public override string displayText
        {
            get
            {
                if (_inFocus)
                    return StyleUtility.StringColored(formattedContent,
                        _available ? StyleUtility.Selected : StyleUtility.DisableSelected);
                return _available
                    ? formattedContent
                    : StyleUtility.StringColored(formattedContent, StyleUtility.Disabled);
            }
        }

        public override string formattedContent => (_toggledOn ? "■ " : "□ ") + base.formattedContent;

        public ToggleUI(string label, WindowUI parent) : base(label, parent)
        {
        }

        public ToggleUI SetAction(Action<bool> action)
        {
            _action = action;
            return this;
        }


        public virtual void SetToggle(bool on)
        {
            if (_toggledOn == on)
                return;
            _toggledOn = on;
            _parentWindow.InvokeUpdate();
        }

        public virtual void Toggle()
        {
            _toggledOn = !_toggledOn;
            TriggerAction();
            _parentWindow.InvokeUpdate();
        }

        public void TriggerAction()
        {
            if (_action == null)
                return;
            _action.Invoke(_toggledOn);
        }
    }
}