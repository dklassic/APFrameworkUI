using System;

namespace ChosenConcept.APFramework.Interface.Framework.Element
{
    public class FuncStringLabel : IStringLabel
    {
        Func<string> _func;
        public FuncStringLabel(Func<string> func)
        {
            _func = func;
        }

        string IStringLabel.GetValue()
        {
            return _func();
        }
    }
}