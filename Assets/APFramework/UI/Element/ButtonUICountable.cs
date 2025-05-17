using System;
using UnityEngine;

namespace ChosenConcept.APFramework.Interface.Framework.Element
{
    public class ButtonUICountable : ButtonUI
    {
        protected int _min = 0;
        protected int _max = 10;
        Action<int> _action;
        public override int getMaxLength => TextUtility.ActualLength(formattedContent) + 2;
        public override string formattedContent => labelPrefix + _count;

        public override int count
        {
            get => _count;
            set
            {
                _count = Mathf.Clamp(value, _min, _max);
                _action?.Invoke(_count);
                _parentWindow.InvokeUpdate();
            }
        }

        public ButtonUICountable(string content, WindowUI parent) : base(content, parent)
        {
        }

        public void SetAction(Action<int> action)
        {
            _action = action;
        }
    }
}