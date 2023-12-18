public class ToggleUI : ButtonUI
{
    protected bool set = false;
    public bool Set { get => set; set => set = value; }
    System.Action<bool> action = null;
    public void SetAction(System.Action<bool> action) => this.action = action;
    public new void TriggerAction()
    {
        if (action == null)
            return;
        action.Invoke(set);
    }
    public ToggleUI(string content) : base(content)
    {
        SetContent(content);
        ElementType = WindowElementType.Toggle;
    }
    public override string ToDisplay
    {
        get
        {
            if (inFocus)
                return StyleUtility.StringColored(FormattedContent, available ? StyleUtility.Selected : StyleUtility.DisableSelected);
            else
                return available ? FormattedContent : StyleUtility.StringColored(FormattedContent, StyleUtility.Disabled);
        }
    }
    public override string FormattedContent => content != string.Empty ? (set ? "■ " : "□ ") + content : (set ? " ■ " : " □ ");
    public virtual void SetToggle(bool on)
    {
        this.set = on;
        parentWindow.InvokeUpdate();
    }
    public virtual void Toggle()
    {
        set = !set;
        TriggerAction();
        parentWindow.InvokeUpdate();
    }
}
