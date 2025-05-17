using System;
using System.Collections.Generic;
using System.Linq;
using Cysharp.Text;
using UnityEngine;

namespace ChosenConcept.APFramework.Interface.Framework.Element
{
    public class SliderUIChoice<T> : SliderUI
    {
        List<string> _choiceListContentCache = new();
        List<IStringLabel> _choiceList = new();
        List<T> _choiceValueList = new();
        Action<T> _action;
        public string currentChoice => choiceListContent.Count > 0 ? choiceListContent[_count] : "N/A";

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
                    return TextUtility.ActualLength(labelPrefix) + 2;
                return TextUtility.ActualLength(labelPrefix) + maxContentLength + 2;
            }
        }

        public override string formattedContent => labelPrefix + currentChoice;

        public override int maxContentLength
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
                    int choiceLength = TextUtility.ActualLength(choice);
                    if (choiceLength > count)
                    {
                        count = choiceLength;
                    }
                }

                return count;
            }
        }

        public SliderUIChoice(string label, WindowUI parent) : base(label, parent)
        {
        }

        public SliderUIChoice(string label, WindowUI parent, List<LocalizedStringLabel> choice) : base(label, parent)
        {
            SetChoice(choice);
        }
        public void SetAction(Action<T> action) => _action = action;
        public void SetActiveValue(T value)
        {
            int index = _choiceValueList.IndexOf(value);
            if (index < 0)
            {
                return;
            }
            _count = Mathf.Clamp(index, _min, _max);
            _parentWindow?.InvokeUpdate();
        }

        public override void TriggerAction()
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

        public void SetChoice(List<LocalizedStringLabel> choice)
        {
            ClearChoice();
            foreach (LocalizedStringLabel item in choice)
            {
                _choiceList.Add(item);
            }

            SetLimit(0, choice.Count - 1);
        }

        public void SetChoice(List<string> choice)
        {
            ClearChoice();
            foreach (string c in choice)
            {
                AddChoice(new StringLabel(c));
            }

            SetLimit(0, choice.Count - 1);
        }

        public void AddChoice(IStringLabel choice)
        {
            _choiceListContentCache.Clear();
            _choiceList.Add(choice);
            _max = _choiceList.Count - 1;
        }

        public void AddChoice(string choice)
        {
            _choiceListContentCache.Clear();
            _choiceList.Add(new StringLabel(choice));
            _max = _choiceList.Count - 1;
        }

        public void RemoveChoiceAt(int index)
        {
            _choiceListContentCache.Clear();
            _choiceList.RemoveAt(index);
            _max = _choiceList.Count - 1;
        }

        public void SetChoiceValue(List<T> list)
        {
            _choiceValueList.Clear();
            _choiceValueList.AddRange(list);
        }

        public void AddChoiceValue(T choice)
        {
            _choiceValueList.Add(choice);
        }

        public override string SliderText()
        {
            string optionString = currentChoice;
            if (_count == _min)
                return StyleUtility.StringColored(ZString.Concat(" ", OptionFillString(optionString), "›"),
                    StyleUtility.Selected);
            if (_count == _max)
                return StyleUtility.StringColored(ZString.Concat("‹", OptionFillString(optionString), " "),
                    StyleUtility.Selected);
            return StyleUtility.StringColored(ZString.Concat("‹", OptionFillString(optionString), "›"),
                StyleUtility.Selected);
        }

        public override void ClearCachedValue()
        {
            base.ClearCachedValue();
            _choiceListContentCache.Clear();
        }
    }
}