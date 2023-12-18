public class InputUI : ButtonUI
{
    protected bool inputMode = false;
    public InputUI(string content) : base(content)
    {
        this.content = content;
    }
}