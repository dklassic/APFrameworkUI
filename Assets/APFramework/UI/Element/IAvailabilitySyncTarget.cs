namespace ChosenConcept.APFramework.UI.Element
{
    public interface IAvailabilitySyncTarget
    {
        bool needSync { get; }
        void SyncAvailability();
    }
}