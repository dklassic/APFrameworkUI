using System;
using System.Collections.Generic;
using System.Linq;
using ChosenConcept.APFramework.UI.Utility;
using ChosenConcept.APFramework.UI.Window;
using Cysharp.Text;
using UnityEngine;

namespace ChosenConcept.APFramework.UI.Element
{
    public class SliderUI<T> : WindowElement<SliderUI<T>>, ISlider
    {
        bool _inInput;
        List<string> _choiceListContentCache = new();
        List<IStringLabel> _choiceList = new();
        List<T> _choiceValueList = new();
        Action<T> _action;

        (Vector2, Vector2) _cachedArrowPosition = (Vector2.zero, Vector2.zero);
        int ISlider.firstSliderArrowIndex => firstCharacterIndex + labelPrefix.Length;
        int ISlider.lastSliderArrowIndex => lastCharacterIndex;
        public (Vector2, Vector2) cachedArrowPosition => _cachedArrowPosition;

        public override int count
        {
            get => _count;
            set
            {
                _count = Mathf.Clamp(value, 0, _choiceList.Count - 1);
                if (_count != value)
                    return;
                _parentWindow?.InvokeUpdate();
                TriggerAction();
            }
        }

        public override string displayText
        {
            get
            {
                if (_inInput)
                    return ZString.Concat(labelPrefix, SliderText());
                return base.displayText;
            }
        }

        public string currentChoice => choiceListContent.Count > 0 ? choiceListContent[_count] : TextUtility.NA;

        public List<string> choiceListContent
        {
            get
            {
                if (_choiceListContentCache.Count == 0)
                {
                    if (_choiceList.Count == 0)
                    {
                        return _choiceListContentCache;
                    }

                    _choiceListContentCache.AddRange(_choiceList.Select(x => x.GetValue()));
                }

                return _choiceListContentCache;
            }
        }

        public override int getMaxLength
        {
            get
            {
                if (_choiceList.Count == 0)
                    return TextUtility.WidthSensitiveLength(labelPrefix) + 2;
                return TextUtility.WidthSensitiveLength(labelPrefix) + maxContentLength + 2;
            }
        }

        public override string formattedContent => ZString.Concat(labelPrefix, currentChoice);

        public int maxContentLength
        {
            get
            {
                if (choiceListContent.Count == 0)
                {
                    return 0;
                }

                int count = 0;
                foreach (string choice in choiceListContent)
                {
                    int choiceLength = TextUtility.WidthSensitiveLength(choice);
                    if (choiceLength > count)
                    {
                        count = choiceLength;
                    }
                }

                return count;
            }
        }

        public SliderUI(string label, WindowUI parent) : base(label, parent)
        {
        }

        public SliderUI<T> SetAction(Action<T> action)
        {
            _action = action;
            return this;
        }

        public SliderUI<T> SetActiveValue(T value)
        {
            int index = _choiceValueList.IndexOf(value);
            if (index < 0)
            {
                return this;
            }

            if (_count == index)
                return this;
            _count = index;
            _parentWindow?.InvokeUpdate();
            return this;
        }

        public void TriggerAction()
        {
            if (_action == null)
                return;
            _action.Invoke(_choiceValueList[_count]);
        }

        public void ClearChoice()
        {
            _choiceListContentCache.Clear();
            _choiceList.Clear();
        }

        public SliderUI<T> SetChoice(List<IStringLabel> choice, List<T> value)
        {
            if (choice.Count != value.Count)
            {
                Debug.LogError($"Mismatch amount of {choice.Count} and {value.Count}");
                return this;
            }

            ClearChoice();
            _choiceList.AddRange(choice);
            _choiceValueList.AddRange(value);
            return this;
        }

        public SliderUI<T> SetChoice(List<string> choice, List<T> value)
        {
            if (choice.Count != value.Count)
            {
                Debug.LogError($"Mismatch amount of {choice.Count} and {value.Count}");
                return this;
            }

            ClearChoice();
            for (int i = 0; i < choice.Count; i++)
            {
                AddChoice(choice[i], value[i]);
            }

            return this;
        }

        public SliderUI<T> SetChoiceByValue(IEnumerable<T> value)
        {
            ClearChoice();
            foreach (T choice in value)
            {
                AddChoice(choice.ToString(), choice);
            }

            return this;
        }

        public SliderUI<T> AddChoice(string choice, T value)
        {
            _choiceListContentCache.Clear();
            _choiceList.Add(new StringLabel(choice));
            _choiceValueList.Add(value);
            return this;
        }

        public void RemoveChoiceAt(int index)
        {
            _choiceListContentCache.Clear();
            _choiceList.RemoveAt(index);
            _choiceValueList.RemoveAt(index);
        }

        public SliderUI<T> AddChoiceByValue(T choice)
        {
            _choiceList.Add(new StringLabel(choice.ToString()));
            _choiceValueList.Add(choice);
            return this;
        }

        public void RemoveValue(T value)
        {
            _choiceListContentCache.Clear();
            int index = _choiceValueList.IndexOf(value);
            if (index < 0)
                return;
            _choiceList.RemoveAt(index);
            _choiceValueList.RemoveAt(index);
        }

        public string SliderText()
        {
            string optionString = currentChoice;
            if (_count == 0)
                return StyleUtility.StringColored(ZString.Concat(" ", OptionFillString(optionString), "›"),
                    StyleUtility.selected);
            if (_count == _choiceList.Count - 1)
                return StyleUtility.StringColored(ZString.Concat("‹", OptionFillString(optionString), " "),
                    StyleUtility.selected);
            return StyleUtility.StringColored(ZString.Concat("‹", OptionFillString(optionString), "›"),
                StyleUtility.selected);
        }

        public override void ClearCache()
        {
            base.ClearCache();
            _choiceListContentCache.Clear();
        }

        public virtual string OptionFillString(string activeOption)
        {
            int totalLengthRequired = maxContentLength - TextUtility.WidthSensitiveLength(activeOption);
            return ZString.Concat(TextUtility.Repeat(' ', totalLengthRequired - totalLengthRequired / 2), activeOption,
                TextUtility.Repeat(' ', totalLengthRequired / 2));
        }

        void ISlider.SetCachedArrowPosition((Vector2, Vector2) position) => _cachedArrowPosition = position;

        void ISlider.SetInput(bool inInput)
        {
            if (_inInput == inInput)
                return;
            _inInput = inInput;
            parentWindow?.InvokeUpdate();
        }

        public override void ClearCachedPosition()
        {
            base.ClearCachedPosition();
            _cachedArrowPosition = (Vector2.zero, Vector2.zero);
        }

        (bool, bool) ISlider.HoverOnArrow(Vector2 position)
        {
            bool hoverOnDecrease = false;
            bool hoverOnIncrease = false;
            float fontSize = _parentWindow.setup.fontSize;
            Vector2 leftArrowDelta = position - _cachedArrowPosition.Item1;
            Vector2 rightArrowDelta = position - _cachedArrowPosition.Item2;
            if (leftArrowDelta.sqrMagnitude < rightArrowDelta.sqrMagnitude &&
                Mathf.Abs(leftArrowDelta.x) < fontSize && Mathf.Abs(leftArrowDelta.y) < fontSize)
            {
                hoverOnDecrease = true;
            }
            else if (Mathf.Abs(rightArrowDelta.x) < fontSize && Mathf.Abs(rightArrowDelta.y) < fontSize)
            {
                hoverOnIncrease = true;
            }

            return (hoverOnDecrease, hoverOnIncrease);
        }
    }
}