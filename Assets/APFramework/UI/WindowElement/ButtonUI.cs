public class ButtonUI : WindowElement
{
    protected bool selectable = true;
    protected bool inFocus = false;
    public void ClearFocus()
    {
        inFocus = false;
        parentWindow.InvokeUpdate();
    }
    public bool IsSingleUIWindow => parentWindow != null && parentWindow.Elements.Count == 1 && !parentWindow.SingleWindowOverride;
    System.Action action = null;
    public void SetAction(System.Action action) => this.action = action;
    public virtual void TriggerAction()
    {
        if (action == null)
            return;
        action.Invoke();
    }
    internal void SetFocus(bool v)
    {
        if (!IsSingleUIWindow)
        {
            inFocus = v;
            parentWindow.InvokeUpdate();
        }
        else
        {
            parentWindow.SetFocusAndAvailable(v, available);
        }
    }
    public override void SetAvailable(bool availability)
    {
        this.available = availability;
        if (IsSingleUIWindow)
            parentWindow.SetFocusAndAvailable(inFocus, available);
        else
            parentWindow.InvokeUpdate();
    }
    public ButtonUI(string content)
    {
        SetContent(content);
        ElementType = WindowElementType.Button;
    }
    public override string ToDisplay
    {
        get
        {
            if (inFocus)
                return StyleUtility.StringColored(TextUtility.StripRichTagsFromStr(FormattedContent), available ? StyleUtility.Selected : StyleUtility.DisableSelected);
            else
                return available ? FormattedContent : StyleUtility.StringColored(FormattedContent, StyleUtility.Disabled);
        }
    }
}