using System;

namespace ChosenConcept.APFramework.Interface.Framework.Element
{
    public class FunctionStringLabel : IStringLabel
    {
        Func<string> _func;
        public FunctionStringLabel(Func<string> func)
        {
            _func = func;
        }

        string IStringLabel.GetValue()
        {
            return _func();
        }
    }
}