namespace ChosenConcept.APFramework.Interface.Framework.Element
{
    public class InputUI : ButtonUI
    {
        protected bool _inInput;
        public InputUI(string label, WindowUI parent) : base(label, parent)
        {
        }
        public virtual bool SetInput(bool value)
        {
            if (!_available)
                return false;
            _inInput = value;
            _parentWindow.SetInput(value);
            _parentWindow.InvokeUpdate();
            return true;
        }
    }
}