using System;

namespace ChosenConcept.APFramework.Interface.Framework.Element
{
    public class ButtonUIWithContent : ButtonUI
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

        public override string formattedContent => labelPrefix + content;

        public ButtonUIWithContent(string content, WindowUI parent) : base(content, parent)
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

        public void SetContent(Func<string> context)
        {
            _contentCache = null;
            _content = new FuncStringLabel(context);
        }
        public override void ClearCachedValue()
        {
            _contentCache = null;
            base.ClearCachedValue();
        }
    }
}