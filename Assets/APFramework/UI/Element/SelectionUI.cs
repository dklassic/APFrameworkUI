using System;
using System.Collections.Generic;
using System.Linq;
using ChosenConcept.APFramework.UI.Utility;
using ChosenConcept.APFramework.UI.Window;
using Cysharp.Text;
using UnityEngine;

namespace ChosenConcept.APFramework.UI.Element
{
    public class SelectionUI<T> : WindowElement<SelectionUI<T>>, ISelectable
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

        public string currentChoice => choiceListContent.Count > 0 ? choiceListContent[_count] : TextUtility.NA;
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

        List<string> ISelectable.values => choiceListContent;
        int ISelectable.activeCount => count;

        public SelectionUI(string label, WindowUI parent) : base(label, parent)
        {
        }

        void ISelectable.SetCount(int count)
        {
            SetCount(count);
        }

        public SelectionUI<T> SetActiveValue(T value)
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

        public SelectionUI<T> SetCount(int count)
        {
            this.count = count;
            return this;
        }

        public SelectionUI<T> SetAction(Action<T> action)
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

        public SelectionUI<T> SetChoice(List<IStringLabel> choice, List<T> value)
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

        public SelectionUI<T> SetChoice(List<string> choice, List<T> value)
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

        public SelectionUI<T> SetChoiceByValue(IEnumerable<T> value)
        {
            ClearChoice();
            foreach (T item in value)
            {
                AddChoice(item.ToString(), item);
            }

            return this;
        }

        public SelectionUI<T> AddChoice(string choice, T value)
        {
            _choiceListContentCache.Clear();
            _choiceList.Add(new StringLabel(choice));
            _choiceValueList.Add(value);
            return this;
        }

        public SelectionUI<T> AddChoice(IStringLabel choice, T value)
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

        public override void ClearCache()
        {
            base.ClearCache();
            _choiceListContentCache.Clear();
        }
    }
}