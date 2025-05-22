using ChosenConcept.APFramework.UI.Utility;
using ChosenConcept.APFramework.UI.Window;

namespace ChosenConcept.APFramework.UI.Element
{
    public class TextUI : WindowElement<TextUI>
    {
        public TextUI(string name, WindowUI parent) : base(name, parent)
        {
        }
        public override string displayText => _available switch
        {
            true => formattedContent,
            false => StyleUtility.StringColored(formattedContent, StyleUtility.disabled)
        };
    }
}