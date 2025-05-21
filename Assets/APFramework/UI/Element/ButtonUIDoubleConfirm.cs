using System;
using Cysharp.Text;

namespace ChosenConcept.APFramework.Interface.Framework.Element
{
    public class ButtonUIDoubleConfirm : ButtonUI
    {
        protected bool _awaitConfirm;
        protected string _confirmTextContent;
        protected IStringLabel _confirmText;
        Action _onAwaitAction;

        public override string formattedContent => _awaitConfirm
            ? ZString.Concat("> ",
                _confirmText.GetValue() == string.Empty ? base.formattedContent : confirmTextContent)
            : base.formattedContent;

        public bool awaitConfirm => _awaitConfirm;

        public string confirmTextContent
        {
            get
            {
                if (_confirmTextContent == null)
                    _confirmTextContent = _confirmText.GetValue();
                return _confirmTextContent;
            }
        }

        public override int getMaxLength => base.getMaxLength + 2;

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

        public ButtonUIDoubleConfirm(string content, WindowUI parent) : base(content, parent)
        {
            _confirmText = new StringLabel("Confirm");
        }

        public void SetOnAwaitAction(Action action) => _onAwaitAction = action;

        public void SetConfirm(bool confirm)
        {
            _awaitConfirm = confirm;
            parentWindow.InvokeUpdate();
        }

        public void SetConfirmText(IStringLabel confirmText)
        {
            _confirmText = confirmText;
        }

        public void SetConfirmText(string confirmText)
        {
            SetConfirmText(new StringLabel(confirmText));
        }

        public void SetConfirmText(Func<string> confirmTextFunc)
        {
            SetConfirmText(new FuncStringLabel(confirmTextFunc));
        }

        public void CancelAwait()
        {
            SetConfirm(false);
        }

        public override void Reset() => SetConfirm(false);

        public override void SetFocus(bool v)
        {
            if (!v)
                CancelAwait();
            base.SetFocus(v);
        }

        public override void TriggerAction()
        {
            if (!awaitConfirm)
            {
                _awaitConfirm = true;
                _onAwaitAction?.Invoke();
                _parentWindow.InvokeUpdate();
                // only if the content becomes longer we resize the position
                // so that when the text shrinks, the confirm click can still happen in place
                if (TextUtility.WidthSensitiveLength(base.formattedContent) <
                    TextUtility.WidthSensitiveLength(confirmTextContent))
                    _parentWindow.UpdateElementPosition(this);
                return;
            }

            CancelAwait();
            base.TriggerAction();
            _parentWindow.UpdateElementPosition(this);
        }

        public override void SetAvailable(bool availability)
        {
            _available = availability;
            if (!availability)
                _awaitConfirm = false;
            if (parentWindow.isSingleButtonWindow)
                _parentWindow.SetAvailable(_available);
            _parentWindow.InvokeUpdate();
        }
    }
}