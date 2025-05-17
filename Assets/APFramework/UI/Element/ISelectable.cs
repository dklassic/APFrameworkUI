using System.Collections.Generic;

namespace ChosenConcept.APFramework.Interface.Framework.Element
{
    public interface ISelectable
    {
        List<string> values { get; }
        int activeCount { get; }
        void SetCount(int count);
    }
}