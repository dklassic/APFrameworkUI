public class ToggleUIExclusive : ToggleUI
{
    public ToggleUIExclusive(string content) : base(content)
    {
        SetContent(content);
        ElementType = WindowElementType.Toggle;
    }
    public override void SetToggle(bool on)
    {
        if (!on)
            return;
        this.set = on;
        parentWindow.ToggleOffOthers(this);
        parentWindow.InvokeUpdate();
    }
    public override void Toggle()
    {
        if (set)
            return;
        set = !set;
        parentWindow.ToggleOffOthers(this);
        parentWindow.InvokeUpdate();
        TriggerAction();
    }
}