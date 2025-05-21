namespace ChosenConcept.APFramework.Interface.Framework
{
    public interface IQuickSelect
    {
        void SetNextChoice();
        void SetPreviousChoice();
        bool canSetPrevious { get; }
    }
}