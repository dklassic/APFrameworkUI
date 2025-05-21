using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace ChosenConcept.APFramework.Interface.Framework.Element
{
    public class SingleSelectionUI<T> : InputUI, ISelectable
    {
        Action<T> _action;
        protected List<string> _choiceListContentCache = new();
        protected List<IStringLabel> _choiceList = new();
        protected List<T> _choiceValueList = new();

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
        public override string formattedContent => labelPrefix + currentChoice;

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

        List<string> ISelectable.values => choiceListContent;
        int ISelectable.activeCount => count;

        public SingleSelectionUI(string label, WindowUI parent) : base(label, parent)
        {
        }

        void ISelectable.SetCount(int count)
        {
            this.count = count;
        }

        public void SetActiveValue(T value)
        {
            int index = _choiceValueList.IndexOf(value);
            if (index < 0)
            {
                return;
            }

            if (index == count)
                return;
            count = index;
            _parentWindow?.InvokeUpdate();
        }

        public void SetActiveCount(int count)
        {
            _count = Mathf.Clamp(count, 0, _choiceList.Count);
            if (_count != count)
                return;
            _parentWindow?.InvokeUpdate();
        }

        public void SetAction(Action<T> action) => _action = action;

        public override void TriggerAction()
        {
            if (_action == null)
                return;
            _action.Invoke(_choiceValueList[_count]);
        }

        public SingleSelectionUI(string label, WindowUI parent, List<IStringLabel> choice, List<T> value) : base(label,
            parent)
        {
            SetChoice(choice, value);
        }

        public SingleSelectionUI(string label, WindowUI parent, List<string> choice, List<T> value) : base(label,
            parent)
        {
            SetChoice(choice, value);
        }


        public void ClearChoice()
        {
            _choiceList.Clear();
            _choiceValueList.Clear();
        }

        public void SetChoice(List<IStringLabel> choice, List<T> value)
        {
            if (choice.Count != value.Count)
            {
                Debug.LogError($"Mismatch amount of {choice.Count} and {value.Count}");
                return;
            }

            ClearChoice();
            _choiceList.AddRange(choice);
            _choiceValueList.AddRange(value);
        }

        public void SetChoice(List<string> choice, List<T> value)
        {
            if (choice.Count != value.Count)
            {
                Debug.LogError($"Mismatch amount of {choice.Count} and {value.Count}");
                return;
            }

            ClearChoice();
            for (int i = 0; i < choice.Count; i++)
            {
                AddChoice(choice[i], value[i]);
            }
        }

        public void SetChoiceByValue(List<T> value)
        {
            ClearChoice();
            foreach (T item in value)
            {
                AddChoice(item.ToString(), item);
            }
        }

        public void AddChoice(string choice, T value)
        {
            _choiceListContentCache.Clear();
            _choiceList.Add(new StringLabel(choice));
            _choiceValueList.Add(value);
        }

        public void AddChoice(IStringLabel choice, T value)
        {
            _choiceListContentCache.Clear();
            _choiceList.Add(choice);
            _choiceValueList.Add(value);
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
    }
}