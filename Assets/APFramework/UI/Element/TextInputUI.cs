using System;
using System.Collections.Generic;
using System.Linq;
using Cysharp.Text;
using UnityEngine;

namespace ChosenConcept.APFramework.Interface.Framework.Element
{
    public class TextInputUI : WindowElement<TextInputUI>
    {
        bool _inInput;
        string _inputContent = string.Empty;
        int _height = 1;
        int _caretPosition = -1;
        Vector2Int _selectionRange = Vector2Int.zero;
        Action<string> _setStringAction;
        List<string> _predictionCandidates = new();

        public string predictionString
        {
            get
            {
                if (string.IsNullOrEmpty(_inputContent))
                    return string.Empty;
                string prediction = GetPredictionString();
                if (string.IsNullOrEmpty(prediction))
                    return string.Empty;
                return prediction.Substring(_inputContent.Length);
            }
        }

        public string inputContent => _inputContent;
        protected bool hasSelection => _selectionRange.x != _selectionRange.y;

        public override string formattedContent => ZString.Concat(labelPrefix, _inputContent);

        public override string displayText
        {
            get
            {
                if (_inInput)
                {
                    if (hasSelection)
                    {
                        Vector2Int selectionRange = _selectionRange;
                        selectionRange.y++;

                        return ZString.Concat(labelPrefix, StyleUtility.StringColoredRange(
                            _inputContent
                                .Insert(Mathf.Min(_caretPosition, _inputContent.Length),
                                    "█"), StyleUtility.Selected, selectionRange.x, selectionRange.y));
                    }

                    if (_caretPosition >= 0)
                    {
                        return ZString.Concat(labelPrefix, _inputContent
                                .Insert(Mathf.Min(_caretPosition, _inputContent.Length),
                                    StyleUtility.StringColored("█",
                                        StyleUtility.Selected)),
                            StyleUtility.StringColored(predictionString,
                                StyleUtility.Disabled));
                    }

                    return labelPrefix + _inputContent;
                }

                return base.displayText;
            }
        }

        public TextInputUI(string content, WindowUI parent) : base(content, parent)
        {
        }

        public void SetPredictionCandidate(List<string> predictionCandidate)
        {
            _predictionCandidates = predictionCandidate.OrderBy(q => q).ToList();
        }

        public void AddPredictionCandidate(string predictionCandidate)
        {
            _predictionCandidates.Add(predictionCandidate);
            _predictionCandidates = _predictionCandidates.OrderBy(q => q).ToList();
        }

        public void SetInputContent(string value)
        {
            _inputContent = value;
            parentWindow?.InvokeUpdate();
            _setStringAction?.Invoke(value);
        }

        public TextInputUI SetActiveInputContent(string value)
        {
            _inputContent = value;
            parentWindow?.InvokeUpdate();
            return this;
        }

        public TextInputUI SetAction(Action<string> setStringAction)
        {
            _setStringAction = setStringAction;
            return this;
        }

        public void SetHeight(int height)
        {
            _height = height;
        }

        public void SetCaretPosition(int inputFieldCaretPosition)
        {
            _caretPosition = inputFieldCaretPosition;
            parentWindow?.InvokeUpdate();
        }

        public void SetSelectionRange(int first, int last)
        {
            _selectionRange.x = Mathf.Min(first, last);
            _selectionRange.y = Mathf.Max(first, last);
            parentWindow?.InvokeUpdate();
        }

        public void SetInput(bool value)
        {
            if (_inInput == value)
                return;
            _inInput = value;
            if (!value)
            {
                SetCaretPosition(-1);
                SetSelectionRange(-1, -1);
            }

            parentWindow?.InvokeUpdate();
        }

        public bool TriggerAutoComplete()
        {
            string prediction = GetPredictionString();
            if (string.IsNullOrEmpty(prediction))
                return false;
            _inputContent = prediction;
            return true;
        }

        public string GetPredictionString()
        {
            if (string.IsNullOrEmpty(_inputContent))
                return string.Empty;
            string inputLowerInvariant = _inputContent.ToLowerInvariant();
            for (int i = 0; i < _predictionCandidates.Count; i++)
            {
                // candidate is sorted so we can just return the first one
                if (_predictionCandidates[i].ToLowerInvariant().StartsWith(inputLowerInvariant))
                    return _predictionCandidates[i];
            }

            return string.Empty;
        }
    }
}