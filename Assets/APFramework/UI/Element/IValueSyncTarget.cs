namespace ChosenConcept.APFramework.UI.Element
{
    public interface IValueSyncTarget
    {
        bool needSync { get; }
        void SyncValue();
    }
}