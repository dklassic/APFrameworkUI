using System;
using Cysharp.Text;
using UnityEngine;

namespace ChosenConcept.APFramework.Interface.Framework.Element
{
    public class SliderUI : InputUI
    {
        protected int _min;
        protected int _max = 10;
        Action<int> _action;
        protected int _maxLength => Mathf.Max(_min.ToString().Length, _max.ToString().Length);
        public virtual int firstSliderArrowIndex => firstCharacterIndex + labelPrefix.Length;
        public virtual int lastSliderArrowIndex => lastCharacterIndex;

        public virtual int maxContentLength
        {
            get
            {
                int minCount = Mathf.FloorToInt(Mathf.Log10(Mathf.Abs(_min))) + 1;
                int maxCount = Mathf.FloorToInt(Mathf.Log10(Mathf.Abs(_max))) + 1;
                if (minCount > maxCount)
                {
                    if (_min < 0)
                        return minCount + 1;
                    return minCount;
                }

                if (_max < 0)
                    return maxCount + 1;
                return maxCount;
            }
        }

        (Vector2, Vector2) _cachedArrowPosition = (Vector2.zero, Vector2.zero);
        public (Vector2, Vector2) cachedArrowPosition => _cachedArrowPosition;
        public override int getMaxLength => TextUtility.ActualLength(labelPrefix) + _maxLength + 3;
        public override string formattedContent => labelPrefix + _count;

        public override int count
        {
            get => _count;
            set
            {
                _count = Mathf.Clamp(value, _min, _max);
                _parentWindow?.InvokeUpdate();
                TriggerAction();
            }
        }

        public override string displayText
        {
            get
            {
                if (_inInput)
                    return labelPrefix + SliderText();
                return base.displayText;
            }
        }
        
        public void SetActiveCount(int count)
        {
            _count = Mathf.Clamp(count, _min, _max);
            _parentWindow?.InvokeUpdate();
        }

        public virtual void SetAction(Action<int> action) => _action = action;

        public override void TriggerAction()
        {
            if (_action == null)
                return;
            _action.Invoke(_count);
        }

        public virtual string OptionFillString(string activeOption)
        {
            int totalLengthRequired = maxContentLength - TextUtility.ActualLength(activeOption);
            return TextUtility.Repeat(' ', totalLengthRequired - totalLengthRequired / 2) + activeOption +
                   TextUtility.Repeat(' ', totalLengthRequired / 2);
        }

        public void SetCachedArrowPosition((Vector2, Vector2) position) => _cachedArrowPosition = position;
        public void ClearCachedArrowPosition() => _cachedArrowPosition = (Vector2.zero, Vector2.zero);

        public SliderUI(string label, WindowUI parent) : base(label, parent)
        {
        }

        public SliderUI(string label, WindowUI parent, int min, int max) : base(label, parent)
        {
            SetLimit(min, max);
        }

        public virtual string SliderText()
        {
            if (_count == _min)
                return StyleUtility.StringColored(ZString.Concat(" ", OptionFillString(_count.ToString()), "›"),
                    StyleUtility.Selected);
            if (_count == _max)
                return StyleUtility.StringColored(ZString.Concat("‹", OptionFillString(_count.ToString()), " "),
                    StyleUtility.Selected);
            return StyleUtility.StringColored(ZString.Concat("‹", OptionFillString(_count.ToString()), "›"),
                StyleUtility.Selected);
        }

        public void SetLimit(int min, int max)
        {
            _min = min;
            _max = max;
            count = Mathf.Clamp(_count, min, max);
        }
    }
}