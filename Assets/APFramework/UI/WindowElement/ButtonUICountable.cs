using UnityEngine;
// This is a quick access UI for enemy debugger
public class ButtonUICountable : ButtonUI
{
    public override int GetLength => TextUtility.ActualLength(FormattedContent) + 2;
    public override int Count
    {
        get => count; set
        {
            count = (int)Mathf.Clamp(value, min, max);
            parentWindow.InvokeUpdate();
            parentWindow.InvokeValueUpdate();
        }
    }
    protected int min = 0;
    protected int max = 10;

    public ButtonUICountable(string content) : base(content)
    {
        SetContent(content);
        ElementType = WindowElementType.Button;
    }
    public override string FormattedContent { get => content + TextUtility.ColumnWithSpace + count; }
    System.Action<int> action = null;
    public void SetAction(System.Action<int> action) => this.action = action;
}