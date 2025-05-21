using System;
using System.Collections.Generic;
using System.Linq;
using Cysharp.Text;
using UnityEngine;

namespace ChosenConcept.APFramework.Interface.Framework.Element
{
    public class QuickSelectionUI<T> : WindowElement<QuickSelectionUI<T>>, IQuickSelect
    {
        bool _canSetPrevious;
        Action<T> _action;
        List<string> _choiceListContentCache = new();
        List<IStringLabel> _choiceList = new();
        List<T> _choiceValueList = new();

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

        public string currentChoice => choiceListContent.Count > 0 ? choiceListContent[_count] : "N/A";
        public override int getMaxLength => TextUtility.WidthSensitiveLength(formattedContent) + 2;
        public override string formattedContent => ZString.Concat(labelPrefix, currentChoice);

        public override int count
        {
            get => _count;
            set
            {
                _count = Mathf.Clamp(value, 0, _choiceList.Count);
                if (_count != value)
                    return;
                _parentWindow?.InvokeUpdate();
                TriggerAction();
            }
        }

        public QuickSelectionUI(string label, WindowUI parent) : base(label, parent)
        {
        }

        public QuickSelectionUI<T> SetActiveValue(T value)
        {
            int index = _choiceValueList.IndexOf(value);
            if (index < 0)
            {
                return this;
            }

            if (index == count)
                return this;
            count = index;
            _parentWindow?.InvokeUpdate();
            return this;
        }

        public void SetCount(int count)
        {
            this.count = count;
        }

        public QuickSelectionUI<T> SetCanPrevious(bool canPrevious)
        {
            _canSetPrevious = canPrevious;
            return this;
        }

        public QuickSelectionUI<T> SetAction(Action<T> action)
        {
            _action = action;
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
            _choiceList.Clear();
            _choiceValueList.Clear();
        }

        public QuickSelectionUI<T> SetChoice(List<IStringLabel> choice, List<T> value)
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

        public QuickSelectionUI<T> SetChoice(List<string> choice, List<T> value)
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

        public QuickSelectionUI<T> SetChoiceByValue(List<T> value)
        {
            ClearChoice();
            foreach (T item in value)
            {
                AddChoice(item.ToString(), item);
            }

            return this;
        }

        public QuickSelectionUI<T> AddChoice(string choice, T value)
        {
            _choiceListContentCache.Clear();
            _choiceList.Add(new StringLabel(choice));
            _choiceValueList.Add(value);
            return this;
        }

        public QuickSelectionUI<T> AddChoice(IStringLabel choice, T value)
        {
            _choiceListContentCache.Clear();
            _choiceList.Add(choice);
            _choiceValueList.Add(value);
            return this;
        }

        public void RemoveChoiceAt(int index)
        {
            _choiceListContentCache.Clear();
            _choiceList.RemoveAt(index);
            _choiceValueList.RemoveAt(index);
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

        public override void ClearCachedValue()
        {
            base.ClearCachedValue();
            _choiceListContentCache.Clear();
        }

        void IQuickSelect.SetNextChoice()
        {
            SetCount((_count + 1) % _choiceList.Count);
        }

        void IQuickSelect.SetPreviousChoice()
        {
            SetCount((_count - 1) % _choiceList.Count);
        }

        bool IQuickSelect.canSetPrevious => _canSetPrevious;
    }
}