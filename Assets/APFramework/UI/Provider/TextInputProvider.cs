using UnityEngine;
using ChosenConcept.APFramework.Interface.Framework.Element;
using TMPro;
using UnityEngine.InputSystem;

namespace ChosenConcept.APFramework.Interface.Framework
{
    public class TextInputProvider : MonoBehaviour, IMenuInputTarget
    {
        [SerializeField] TMP_InputField _inputField;
        IMenuInputTarget _target;
        TextInputUI _textInputUI;
        string _originalText = string.Empty;
        bool _active = false;
        public bool active => _active;

        public void GetTextInput(IMenuInputTarget sourceUI, TextInputUI text)
        {
            _target = sourceUI;
            GetTextInput(text);
        }

        public void GetTextInput(TextInputUI text)
        {
            _active = true;
            _textInputUI = text;
            _originalText = _textInputUI.rawContent;
            // take away the input from the sourceUI
            WindowManager.instance.LinkInputTarget(null);

            // show the text UI
            _inputField.text = text.inputContent;
            _inputField.gameObject.SetActive(true);
            _inputField.onValueChanged.AddListener(OnValueChanged);
            _inputField.onSubmit.AddListener(OnSubmit);
            _inputField.onTextSelection.AddListener(OnTextSelection);
            _inputField.onEndTextSelection.AddListener(OnEndTextSelection);

            // focus on Unity's text UI
            _inputField.ActivateInputField();
            _textInputUI.SetCaretPosition(_inputField.caretPosition);
#if AUTOPANIC_STEAMWORK
        bool success = GameContext.platform.steam.ShowGamepadTextInput(this,
            GameContext.localization.GetLocalizedValue(text.rawContent),
            text.inputContent);
        if (success)
            return;
#endif
            WindowManager.instance.LinkInputTarget(this);
        }

        void OnEndTextSelection(string arg0, int arg1, int arg2)
        {
            _textInputUI.SetSelectionRange(0, 0);
        }

        void OnTextSelection(string arg0, int arg1, int arg2)
        {
            _textInputUI.SetSelectionRange(arg1, arg2);
        }

        // XXX: to don't use Update
        void Update()
        {
            if (_textInputUI == null)
                return;
            _textInputUI.SetCaretPosition(_inputField.caretPosition);
            // This naive solution is required because InputSystem isn't triggered properly
            if (Keyboard.current.tabKey.wasPressedThisFrame)
                TriggerAutoComplete();
        }

        void OnSubmit(string arg0)
        {
            CompleteInput();
        }

        void OnValueChanged(string value)
        {
            _textInputUI.SetActiveInputContent(value);
            _textInputUI.SetCaretPosition(_inputField.caretPosition);
            _textInputUI.SetSelectionRange(0, 0);
        }

        void CompleteInput()
        {
            _active = false;
            _inputField.onValueChanged.RemoveAllListeners();
            _inputField.onSubmit.RemoveAllListeners();
            _inputField.onTextSelection.RemoveAllListeners();
            _inputField.onEndTextSelection.RemoveAllListeners();

            // remove focus from Unity's text UI
            _inputField.DeactivateInputField();

            // close the text UI
            _inputField.gameObject.SetActive(false);

            // give back the input to the target
            WindowManager.instance.LinkInputTarget(null);
            _target.SetTextInput(_inputField.text);
            _target = null;
            _textInputUI = null;
        }

        public void SetTextAndConfirm(string submittedText)
        {
            _inputField.text = submittedText;
            _textInputUI.SetActiveInputContent(submittedText);
            _textInputUI.SetCaretPosition(_inputField.caretPosition);
            _textInputUI.SetSelectionRange(0, 0);
            CompleteInput();
        }

        public void CancelInput()
        {
            SetTextAndConfirm(_originalText);
        }

        void TriggerAutoComplete()
        {
            if (_textInputUI.TriggerAutoComplete())
            {
                _inputField.text = _textInputUI.inputContent;
                _inputField.MoveTextEnd(false);
            }
        }


        void IMenuInputTarget.OnConfirm()
        {
        }

        void IMenuInputTarget.OnCancel()
        {
            CompleteInput();
        }

        void IMenuInputTarget.OnMove(Vector2 move)
        {
        }

        void IMenuInputTarget.OnScroll(Vector2 scroll)
        {
        }

        void IMenuInputTarget.OnMouseConfirmPressed()
        {
        }

        void IMenuInputTarget.OnMouseConfirmReleased()
        {
        }

        void IMenuInputTarget.OnMouseCancel()
        {
            CompleteInput();
        }

        void IMenuInputTarget.OnKeyboardEscape()
        {
            CompleteInput();
        }

        void IMenuInputTarget.SetSelection(int i)
        {
        }

        void IMenuInputTarget.SetTextInput(string inputFieldText)
        {
        }
    }
}