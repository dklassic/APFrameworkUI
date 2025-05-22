using ChosenConcept.APFramework.UI.Menu;
using UnityEngine;
using UnityEngine.InputSystem;

namespace ChosenConcept.APFramework.UI.Input
{
    public class UnityInputProvider : IInputProvider
    {
        bool _inputEnabled;
        bool _isController;
        Vector2 _lastMovement;
        Vector2 _mouseDelta;
        Vector2 _lastMousePosition;
        Vector2 _lastLeftStickInput;

        public bool hasMouse => Mouse.current != null;
        public Vector2 mouseDelta => _mouseDelta;
        public Vector2 mousePosition => Mouse.current.position.ReadValue();
        public bool inputEnabled => _inputEnabled;
        IMenuInputTarget _activeTarget;
        void IInputProvider.SetTarget(IMenuInputTarget target)
        {
            _activeTarget = target;
        }
        void IInputProvider.EnableInput(bool enable)
        {
            _inputEnabled = enable;
        }

        void IInputProvider.Update()
        {
            if (!_inputEnabled)
                return;
            Vector2 movement = Vector2.zero;
            Gamepad gamepad = Gamepad.current;
            if (gamepad != null)
            {
                Vector2 leftStickValue = gamepad.leftStick.ReadValue();
                Vector2 dpadValue = gamepad.dpad.ReadValue();
                if (leftStickValue.sqrMagnitude > dpadValue.sqrMagnitude)
                    movement = leftStickValue;
                else
                    movement = dpadValue;

                if (gamepad.buttonSouth.wasPressedThisFrame)
                    _activeTarget?.OnConfirm();

                if (gamepad.buttonEast.wasPressedThisFrame)
                    _activeTarget?.OnCancel();
            }

            Keyboard keyboard = Keyboard.current;
            if (keyboard != null)
            {
                if (keyboard.upArrowKey.isPressed || keyboard.wKey.isPressed)
                    movement.y = 1.0f;
                if (keyboard.downArrowKey.isPressed || keyboard.sKey.isPressed)
                    movement.y = -1.0f;
                if (keyboard.leftArrowKey.isPressed || keyboard.aKey.isPressed)
                    movement.x = -1.0f;
                if (keyboard.rightArrowKey.isPressed || keyboard.dKey.isPressed)
                    movement.x = 1.0f;

                if (keyboard.spaceKey.wasPressedThisFrame || keyboard.enterKey.wasPressedThisFrame)
                    _activeTarget?.OnConfirm();
                if (keyboard.escapeKey.wasPressedThisFrame)
                    _activeTarget?.OnCancel();
            }

            Mouse mouse = Mouse.current;
            if (mouse != null)
            {
                _mouseDelta = mouse.position.ReadValue() - _lastMousePosition;
                _lastMousePosition = mouse.position.ReadValue();
                if (mouse.leftButton.wasPressedThisFrame)
                    _activeTarget?.OnMouseConfirmPressed();
                if (mouse.leftButton.wasReleasedThisFrame)
                    _activeTarget?.OnMouseConfirmReleased();
                if (mouse.rightButton.wasPressedThisFrame)
                    _activeTarget?.OnMouseCancel();
                if (mouse.scroll.ReadValue().sqrMagnitude > 0)
                {
                    Vector2 mouseScroll = new(0, Mouse.current.scroll.ReadValue().normalized.y);
                    _activeTarget?.OnScroll(mouseScroll);
                }
            }
            if(_lastMovement != movement)
            {
                if (movement.magnitude > 0.5f)
                {
                    _activeTarget?.OnMove(movement);
                }
                else
                {
                    _activeTarget?.OnMove(Vector2.zero);
                }
                _lastMovement = movement;
            }
        }
    }
}