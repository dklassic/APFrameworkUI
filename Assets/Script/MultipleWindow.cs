using ChosenConcept.APFramework.Interface.Framework;

public class MultipleWindow : CompositeMenuMono
{
    protected override void InitializeUI()
    {
        LayoutAlignment layout = InitNewLayout();

        WindowUI window1 = NewWindow("Window 1", layout, WindowSetup.defaultSetup);
        AddButton("Button from Window 1", window1);
        AddButton("Button from Window 1", window1);
        AddButton("Button from Window 1", window1);
        window1.AutoResize();

        WindowUI window2 = NewWindow("Window 2", layout, WindowSetup.defaultSetup);
        AddButton("Button from Window 2", window2);
        AddButton("Button from Window 2", window2);
        AddButton("Button from Window 2", window2);
        window2.AutoResize();

        WindowUI window3 = NewWindow("Window 3", layout, WindowSetup.defaultSetup);
        AddButton("Button from Window 3", window3);
        AddButton("Button from Window 3", window3);
        AddButton("Button from Window 3", window3);
        window3.AutoResize();
    }
}
