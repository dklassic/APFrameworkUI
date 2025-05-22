using System;
using ChosenConcept.APFramework.UI.Utility;
using ChosenConcept.APFramework.UI.Window;
using Cysharp.Text;

namespace ChosenConcept.APFramework.UI.Element
{
    public class ButtonUI : WindowElement<ButtonUI>
    {
        bool _needConfirm;
        bool _awaitConfirm;
        string _confirmTextCache;
        IStringLabel _confirmText;
        Action _action;
        Action _onAwaitAction;

        public override string formattedContent => _awaitConfirm
            ? ZString.Concat("> ",
                _confirmText.GetValue() == string.Empty ? base.formattedContent : ConfirmTextCache)
            : base.formattedContent;

        public bool awaitConfirm => _awaitConfirm;

        public string ConfirmTextCache
        {
            get
            {
                if (_confirmTextCache == null)
                    _confirmTextCache = _confirmText.GetValue();
                return _confirmTextCache;
            }
        }

        public override int getMaxLength => base.getMaxLength + 2;

        public override string displayText
        {
            get
            {
                if (_inFocus)
                    return StyleUtility.StringColored(TextUtility.StripRichTagsFromStr(formattedContent),
                        _available ? StyleUtility.selected : StyleUtility.disableSelected);
                return _available
                    ? formattedContent
                    : StyleUtility.StringColored(formattedContent, StyleUtility.disabled);
            }
        }

        public ButtonUI(string content, WindowUI parent) : base(content, parent)
        {
        }

        public ButtonUI SetOnAwaitAction(Action action)
        {
            _onAwaitAction = action;
            return this;
        }

        public void SetConfirm(bool confirm)
        {
            if (_awaitConfirm == confirm)
                return;
            _awaitConfirm = confirm;
            parentWindow.UpdateElementPosition(this);
            parentWindow.InvokeUpdate();
        }

        public ButtonUI SetConfirmText(IStringLabel confirmText)
        {
            _needConfirm = true;
            _confirmText = confirmText;
            return this;
        }

        public ButtonUI SetConfirmText(string confirmText)
        {
            return SetConfirmText(new StringLabel(confirmText));
        }

        public ButtonUI SetConfirmText(Func<string> confirmTextFunc)
        {
            return SetConfirmText(new FunctionStringLabel(confirmTextFunc));
        }

        public void CancelAwait()
        {
            SetConfirm(false);
        }

        public override void Reset() => SetConfirm(false);

        public override void SetFocus(bool v)
        {
            if (!v && _needConfirm)
                CancelAwait();
            base.SetFocus(v);
        }

        public void TriggerAction()
        {
            if (_action == null)
                return;
            if (!awaitConfirm && _needConfirm)
            {
                _awaitConfirm = true;
                _onAwaitAction?.Invoke();
                _parentWindow.InvokeUpdate();
                // only if the content becomes longer we will resize the position
                // so that when the text shrinks, the confirm click can still happen in place
                if (TextUtility.WidthSensitiveLength(base.formattedContent) <
                    TextUtility.WidthSensitiveLength(ConfirmTextCache))
                    _parentWindow.UpdateElementPosition(this);
                return;
            }

            CancelAwait();
            _action.Invoke();
        }

        public new ButtonUI SetAvailable(bool availability)
        {
            if (availability != _available)
            {
                _available = availability;
                if (!availability)
                    _awaitConfirm = false;
                if (parentWindow.isSingleButtonWindow)
                    _parentWindow.SetAvailable(_available);
                _parentWindow.InvokeUpdate();
            }

            return this;
        }

        public ButtonUI SetAction(Action action)
        {
            _action = action;
            return this;
        }
    }
}