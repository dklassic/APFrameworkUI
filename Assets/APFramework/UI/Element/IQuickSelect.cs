namespace ChosenConcept.APFramework.UI.Element
{
    public interface IQuickSelect
    {
        void CycleForward();
        void CycleBackward();
        bool canCycleBackward { get; }
    }
}