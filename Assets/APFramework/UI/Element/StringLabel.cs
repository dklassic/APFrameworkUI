using System;

namespace ChosenConcept.APFramework.UI.Element
{
    [Serializable]
    public struct StringLabel : IStringLabel
    {
        string _content;
        
        public StringLabel(string value)
        {
            _content = value;
        }

        string IStringLabel.GetValue()
        {
            return _content;
        }
    }
}