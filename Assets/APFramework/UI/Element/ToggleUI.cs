using System;
using ChosenConcept.APFramework.UI.Utility;
using ChosenConcept.APFramework.UI.Window;
using Cysharp.Text;

namespace ChosenConcept.APFramework.UI.Element
{
    public class ToggleUI : WindowElement<ToggleUI>, IValueSyncTarget
    {
        protected bool _toggledOn;
        Action<bool> _action;
        Func<bool> _activeValueGetter;

        public override string displayText
        {
            get
            {
                if (_inFocus)
                    return StyleUtility.StringColored(formattedContent,
                        _available ? StyleUtility.selected : StyleUtility.disableSelected);
                return _available
                    ? formattedContent
                    : StyleUtility.StringColored(formattedContent, StyleUtility.disabled);
            }
        }

        public override string formattedContent => ZString.Concat((_toggledOn ? "■ " : "□ "), base.formattedContent);

        public ToggleUI(string label, WindowUI parent) : base(label, parent)
        {
        }

        public ToggleUI SetAction(Action<bool> action)
        {
            _action = action;
            return this;
        }


        public virtual ToggleUI SetActiveToggle(bool on)
        {
            if (_toggledOn == on)
                return this;
            _toggledOn = on;
            _parentWindow.InvokeUpdate();
            return this;
        }

        public virtual ToggleUI SetActiveToggle(Func<bool> valueGetter)
        {
            _activeValueGetter = valueGetter;
            bool value = _activeValueGetter();
            if (_toggledOn == value)
                return this;
            _toggledOn = value;
            _parentWindow.InvokeUpdate();
            return this;
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

        bool IValueSyncTarget.needSync => _activeValueGetter != null;

        void IValueSyncTarget.SyncValue()
        {
            SetActiveToggle(_activeValueGetter());
        }
    }
}