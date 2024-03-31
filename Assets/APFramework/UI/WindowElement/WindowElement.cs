using UnityEngine;
[System.Serializable]
public class WindowElement
{
    protected string content = string.Empty;
    protected int count = 0;
    public virtual int Count
    {
        get => count; set
        {
            count = value;
            parentWindow.InvokeUpdate();
        }
    }
    public virtual string FormattedContent => content;
    public string RawContent => content;
    public virtual int GetLength => TextUtility.ActualLength(ToDisplay) + 1;
    // For the element to determine how it is displayed by window aka with rich text
    public virtual string ToDisplay => FormattedContent;
    protected bool flexible = false;
    public bool Flexible { get { return flexible; } set { flexible = value; } }
    protected bool available = true;
    public bool Available => available;
    Vector2Int characterIndex = new Vector2Int(-1, -1);
    public int FirstCharacterIndex
    {
        get => characterIndex[0];
        set
        {
            characterIndex[0] = value;
        }
    }
    public int LastCharacterIndex
    {
        get => characterIndex[1];
        set
        {
            characterIndex[1] = value;
        }
    }
    [SerializeField] Vector2 cachedPositionStart = Vector2.zero;
    [SerializeField] Vector2 cachedPositionEnd = Vector2.zero;
    public (Vector2, Vector2) CachedPosition => (cachedPositionStart, cachedPositionEnd);
    public void ClearCachedPosition()
    {
        cachedPositionStart = Vector2.zero;
        cachedPositionEnd = Vector2.zero;
    }
    public void SetCachedPosition((Vector2 startPosition, Vector2 endPosition) position)
    {
        cachedPositionStart = position.startPosition;
        cachedPositionEnd = position.endPosition;
    }
    protected WindowUI parentWindow;
    public WindowUI ParentWindow => parentWindow;
    public virtual void Activate() => _ = 0;
    public virtual void Deactivate() => _ = 0;
    public void SetContent(string content)
    {
        this.content = content;
        parentWindow?.InvokeUpdate();
    }
    public void SetParent(WindowUI window) => this.parentWindow = window;

    public void SetParentName(string name) => parentWindow?.SetName(name);
    public void SetParentSubscript(string name) => parentWindow?.SetSubscript(name);
    public void Remove() => parentWindow?.RemoveElement(this);
    public WindowElementType ElementType = WindowElementType.Text;
    public void TriggerGlitch() => parentWindow?.TriggerGlitch();
    public void AutoResize() => parentWindow?.AutoResize();
    public virtual void SetAvailable(bool availability)
    {
        this.available = availability;
        ParentWindow.InvokeUpdate();
    }
}