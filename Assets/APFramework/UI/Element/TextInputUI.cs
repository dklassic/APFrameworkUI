using System;
using UnityEngine;

namespace ChosenConcept.APFramework.Interface.Framework.Element
{
    public class TextInputUI : InputUI
    {
        protected string _inputContent = string.Empty;
        protected int _height = 1;
        protected int _caretPosition = -1;
        protected Vector2Int _selectionRange = Vector2Int.zero;
        Action<string> _setStringAction;
        public string inputContent => _inputContent;
        protected bool hasSelection => _selectionRange.x != _selectionRange.y;

        public override string formattedContent => labelPrefix + _inputContent;

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

                        return labelPrefix + StyleUtility.StringColoredRange(
                            _inputContent
                                .Insert(Mathf.Min(_caretPosition, _inputContent.Length),
                                    "█"), StyleUtility.Selected, selectionRange.x, selectionRange.y);
                    }

                    if (_caretPosition >= 0)
                    {
                        return labelPrefix + _inputContent
                            .Insert(Mathf.Min(_caretPosition, _inputContent.Length),
                                StyleUtility.StringColored("█", StyleUtility.Selected));
                    }

                    return labelPrefix + _inputContent;
                }

                return base.displayText;
            }
        }
        
        public TextInputUI(string content, WindowUI parent) : base(content, parent)
        {
        }

        public void SetInputContent(string value)
        {
            _inputContent = value;
            parentWindow?.InvokeUpdate();
            _setStringAction?.Invoke(value);
        }

        public void SetActiveInputContent(string value)
        {
            _inputContent = value;
            parentWindow?.InvokeUpdate();
        }

        public void SetAction(Action<string> setStringAction)
        {
            _setStringAction = setStringAction;
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

        public override bool SetInput(bool value)
        {
            if (base.SetInput(value))
            {
                if (!value)
                {
                    SetCaretPosition(-1);
                    SetSelectionRange(-1, -1);
                }
            }

            return false;
        }
    }
}