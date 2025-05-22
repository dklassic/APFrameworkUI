namespace ChosenConcept.APFramework.Interface.Framework
{
    public interface IQuickSelect
    {
        void CycleForward();
        void CycleBackward();
        bool canCycleBackward { get; }
    }
}