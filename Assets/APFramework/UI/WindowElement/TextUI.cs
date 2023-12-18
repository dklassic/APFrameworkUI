public class TextUI : WindowElement
{
    public TextUI(string content)
    {
        SetContent(content);
        ElementType = WindowElementType.Text;
    }
    public override string ToDisplay => available switch
    {
        true => FormattedContent,
        false => StyleUtility.StringColored(FormattedContent, StyleUtility.Disabled)
    };
}