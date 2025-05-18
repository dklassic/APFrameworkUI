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

            _parentWindow?.InvokeUpdate();
        }

        public void SetActiveCount(int count)
        {
            _count = Mathf.Clamp(count, 0, _choiceList.Count);
            _parentWindow?.InvokeUpdate();
        }

        public void SetAction(Action<T> action) => _action = action;

        public override void TriggerAction()
        {
            if (_action == null)
                return;
            _action.Invoke(_choiceValueList[_count]);
        }

        public SingleSelectionUI(string label, WindowUI parent, List<LocalizedStringLabel> choice) : base(label, parent)
        {
            SetChoice(choice);
        }

        public void ClearChoice()
        {
            _choiceList.Clear();
        }

        public void SetChoice(List<LocalizedStringLabel> choice)
        {
            _choiceList.Clear();
            foreach (LocalizedStringLabel item in choice)
            {
                _choiceList.Add(item);
            }
        }

        public void SetChoice(List<string> choice)
        {
            ClearChoice();
            foreach (string c in choice)
            {
                AddChoice(c);
            }
        }

        public void AddChoice(string choice)
        {
            _choiceListContentCache.Clear();
            _choiceList.Add(new StringLabel(choice));
        }

        public void AddChoice(IStringLabel choice)
        {
            _choiceListContentCache.Clear();
            _choiceList.Add(choice);
        }

        public void RemoveChoiceAt(int index)
        {
            _choiceListContentCache.Clear();
            _choiceList.RemoveAt(index);
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

        public override void ClearCachedValue()
        {
            base.ClearCachedValue();
            _choiceListContentCache.Clear();
        }
    }
}