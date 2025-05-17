using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace ChosenConcept.APFramework.Interface.Framework.Element
{
    public class TextInputUIWithPrediction : TextInputUI
    {
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
        public override string formattedContent => _inputContent;

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

                        return StyleUtility.StringColoredRange(
                            _inputContent
                                .Insert(Mathf.Min(_caretPosition, _inputContent.Length),
                                    "█"), StyleUtility.Selected, selectionRange.x, selectionRange.y);
                    }

                    if (_caretPosition >= 0)
                    {
                        return _inputContent
                                .Insert(Mathf.Min(_caretPosition, _inputContent.Length),
                                    StyleUtility.StringColored("█", StyleUtility.Selected))
                            + StyleUtility.StringColored(predictionString, StyleUtility.Disabled);
                    }

                    return _inputContent;
                }

                return base.displayText;
            }
        }

        public TextInputUIWithPrediction(string label, WindowUI parent) : base(label, parent)
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
