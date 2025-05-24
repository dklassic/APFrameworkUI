using System;
using System.Collections.Generic;
using System.Linq;
using ChosenConcept.APFramework.UI.Utility;
using ChosenConcept.APFramework.UI.Window;
using Cysharp.Text;
using UnityEngine;

namespace ChosenConcept.APFramework.UI.Element
{
    public class QuickSelectionUI<T> : WindowElement<QuickSelectionUI<T>>, IQuickSelect, IValueSyncTarget
    {
        bool _canCycleBackward;
        Action<T> _action;
        List<string> _choiceListContentCache = new();
        List<IStringLabel> _choiceList = new();
        List<T> _choiceValueList = new();
        Func<T> _activeValueGetter;

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

        public int maxContentLength
        {
            get
            {
                int maxLength = 0;
                foreach (string choice in choiceListContent)
                {
                    int length = TextUtility.WidthSensitiveLength(choice);
                    if (length > maxLength)
                        maxLength = length;
                }

                if (maxLength == 0)
                    return TextUtility.WidthSensitiveLength(TextUtility.NA);

                return maxLength;
            }
        }

        public override int getMaxLength => TextUtility.WidthSensitiveLength(labelPrefix) + maxContentLength + +2;
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

        public QuickSelectionUI<T> SetActiveValue(Func<T> valueGetter)
        {
            _activeValueGetter = valueGetter;
            int index = _choiceValueList.IndexOf(_activeValueGetter());
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

        public QuickSelectionUI<T> SetCanCycleBackward(bool canPrevious)
        {
            _canCycleBackward = canPrevious;
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

        public QuickSelectionUI<T> SetChoiceByValue(IEnumerable<T> value)
        {
            ClearChoice();
            foreach (T item in value)
            {
                AddChoice(item.ToString(), item);
            }

            return this;
        }

        public QuickSelectionUI<T> SetLocalizedChoiceByValue(IEnumerable<T> value)
        {
            ClearChoice();
            foreach (T item in value)
            {
                AddChoice(new LocalizedStringLabel(_tag, item.ToString()), item);
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

        public override void ClearCache()
        {
            base.ClearCache();
            _choiceListContentCache.Clear();
        }

        void IQuickSelect.CycleForward()
        {
            SetCount((_count + 1) % _choiceList.Count);
        }

        void IQuickSelect.CycleBackward()
        {
            SetCount((_count - 1) % _choiceList.Count);
        }

        bool IQuickSelect.canCycleBackward => _canCycleBackward;

        public override void ContextLanguageChange()
        {
            base.ContextLanguageChange();
            _choiceListContentCache.Clear();
        }

        public override IEnumerable<string> ExportLocalizationTag()
        {
            List<string> tags = new();
            tags.AddRange(base.ExportLocalizationTag());
            foreach (IStringLabel item in _choiceList)
            {
                if (item is LocalizedStringLabel label)
                    tags.Add(label.tag);
            }

            return tags;
        }

        bool IValueSyncTarget.needSync => _activeValueGetter != null;

        void IValueSyncTarget.SyncValue()
        {
            SetActiveValue(_activeValueGetter());
        }
    }
}