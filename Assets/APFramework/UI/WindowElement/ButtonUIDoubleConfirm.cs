public class ButtonUIDoubleConfirm : ButtonUI
{
    public override string FormattedContent => awaitConfirm ? "> " + base.FormattedContent : base.FormattedContent;
    protected bool awaitConfirm = false;
    public override int GetLength => base.GetLength + 2;
    System.Action selectAction = null;
    public void SetSelectAction(System.Action selectAction) => this.selectAction = selectAction;
    public void SetConfirm(bool confirm)
    {
        awaitConfirm = confirm;
    }
    public override void TriggerAction()
    {
        if (!awaitConfirm)
        {
            awaitConfirm = true;
            selectAction?.Invoke();
            parentWindow.CancelOthers(this);
            parentWindow.InvokeUpdate();
            return;
        }
        awaitConfirm = false;
        base.TriggerAction();
    }
    public override void SetAvailable(bool availability)
    {
        this.available = availability;
        if (!availability)
            awaitConfirm = false;
        if (IsSingleUIWindow)
            parentWindow.SetFocusAndAvailable(inFocus, available);
        else
            parentWindow.InvokeUpdate();
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
    public ButtonUIDoubleConfirm(string content) : base(content)
    {
        SetContent(content);
        ElementType = WindowElementType.Button;
    }
}