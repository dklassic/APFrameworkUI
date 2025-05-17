namespace ChosenConcept.APFramework.Interface.Framework.Element
{
    public class TextUI : WindowElement
    {
        public TextUI(string name, WindowUI parent) : base(name, parent)
        {
        }
        public override string displayText => _available switch
        {
            true => formattedContent,
            false => StyleUtility.StringColored(formattedContent, StyleUtility.Disabled)
        };
    }
}