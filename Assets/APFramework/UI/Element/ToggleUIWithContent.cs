using System;

namespace ChosenConcept.APFramework.Interface.Framework.Element
{
    public class ToggleUIWithContent : ToggleUI
    {
        string _contentCache;
        protected IStringLabel _content;

        public string content
        {
            get
            {
                if (_contentCache == null)
                {
                    _contentCache = _content.GetValue();
                }

                return _contentCache;
            }
        }

        public override string formattedContent => (_toggledOn ? "■ " : "□ ") + labelPrefix + content;

        public ToggleUIWithContent(string label, WindowUI parent) : base(label, parent)
        {
        }

        public void SetContent(IStringLabel content)
        {
            _contentCache = null;
            _content = content;
        }

        public void SetContent(string content)
        {
            _contentCache = null;
            _content = new StringLabel(content);
        }

        public void SetContent(Func<string> func)
        {
            _contentCache = null;
            _content = new FuncStringLabel(func);
        }

        public override void ClearCachedValue()
        {
            base.ClearCachedValue();
            _contentCache = null;
        }
    }
}