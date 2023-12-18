using UnityEngine;
using UnityEngine.InputSystem;

public class UIInput : MonoBehaviour
{
    InputMaster controls;
    public InputMaster Controls => controls;
    [SerializeField] Vector2 move = Vector2.zero;
    public System.Action OnConfirm;
    public System.Action OnCancel;
    public System.Action<Vector2> OnMove;
    public System.Action OnMouseConfirm;
    public System.Action OnMouseCancel;
    public System.Action OnDebug;
    public System.Action OnMenu;
    public System.Action OnKeyboardMenu;
    public System.Action OnQuickTest;
    void Awake()
    {
        controls = new InputMaster();
        controls.MenuNav.Confirm.performed += ConfirmPerformed;
        controls.MenuNav.Cancel.performed += CancelPerformed;
        // controls.MenuNav.QuickConfirm.performed += SelectionPerformed;
        // controls.MenuNav.QuickCancel.performed += SelectionCancelled;
        controls.MenuNav.Navigation.performed += ctx => MovePerformed(new Vector2(ctx.ReadValue<Vector2>().x, ctx.ReadValue<Vector2>().y));
        controls.MenuNav.Navigation.canceled += _ => MovePerformed(Vector2.zero);
        controls.MenuNav.NavigationLeftStick.performed += ctx => LeftStickMovePerformed(new Vector2(ctx.ReadValue<Vector2>().x, ctx.ReadValue<Vector2>().y));
        controls.MenuNav.NavigationLeftStick.canceled += _ => LeftStickMovePerformed(Vector2.zero);
        controls.MenuNav.NavigationRightStick.performed += ctx => RightStickMovePerformed(new Vector2(ctx.ReadValue<Vector2>().x, ctx.ReadValue<Vector2>().y));
        controls.MenuNav.NavigationRightStick.canceled += _ => RightStickMovePerformed(Vector2.zero);
        controls.MenuNav.MouseConfirm.performed += WrappedMouseConfirmPerformed;
        controls.MenuNav.MouseCancel.performed += WrappedMouseCancelPerformed;
        controls.MenuNav.Menu.performed += MenuPerformed;
        controls.MenuNav.KeyboardMenu.performed += KeyboardMenuPerformed;
    }
    void OnEnable()
    {
        controls.Enable();
    }
    void QuickTestTrigger(InputAction.CallbackContext context)
    {
        Debug.Log("QuickTestTriggered");
        OnQuickTest?.Invoke();
    }
    void WrappedMouseCancelPerformed(InputAction.CallbackContext context)
    {
        OnMouseCancel?.Invoke();
    }

    void WrappedMouseConfirmPerformed(InputAction.CallbackContext context)
    {
        OnMouseConfirm?.Invoke();
    }

    void MovePerformed(Vector2 vector2)
    {
        move = vector2;
        OnMove?.Invoke(vector2);
    }
    void LeftStickMovePerformed(Vector2 vector2)
    {
        MovePerformed(vector2);
    }
    void RightStickMovePerformed(Vector2 vector2)
    {
        MovePerformed(vector2);
    }

    void CancelPerformed(InputAction.CallbackContext context)
    {
        OnCancel?.Invoke();
    }

    void ConfirmPerformed(InputAction.CallbackContext context)
    {
        OnConfirm?.Invoke();
    }
    void MenuPerformed(InputAction.CallbackContext context)
    {
        OnMenu?.Invoke();
    }
    void KeyboardMenuPerformed(InputAction.CallbackContext context)
    {
        OnKeyboardMenu?.Invoke();
    }
}
