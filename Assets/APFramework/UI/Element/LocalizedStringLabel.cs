using System;

namespace ChosenConcept.APFramework.Interface.Framework.Element
{
    [Serializable]
    // This is an example setup for using the structure of the menu to localize 
    public struct LocalizedStringLabel : IStringLabel
    {
        string _tag;
        public string tag => _tag;
        public LocalizedStringLabel(string value)
        {
            _tag = value;
        }
        public LocalizedStringLabel(string value1, string value2)
        {
            _tag = value1 + "." + value2;
        }

        public LocalizedStringLabel(string value1, string value2, string value3)
        {
            _tag = value1 + "." + value2 + "." + value3;
        }

        string IStringLabel.GetValue()
        {
            if (_tag == null)
                return string.Empty;
            return _tag;
        }
        public string GetLocalizationTag() => _tag;
    }
}