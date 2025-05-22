using ChosenConcept.APFramework.Interface.Framework;

public class MultipleWindowVertical : CompositeMenuMono
{
    protected override void InitializeMenu()
    {
        LayoutAlignment layout = InitNewLayout();

        WindowUI window1 = NewWindow("Window 1", layout, WindowSetup.defaultSetup);
        window1.AddButton("Button from Window 1");
        window1.AddButton("Button from Window 1");
        window1.AddButton("Button from Window 1");

        WindowUI window2 = NewWindow("Window 2", layout, WindowSetup.defaultSetup);
        window2.AddButton("Button from Window 2");
        window2.AddButton("Button from Window 2");
        window2.AddButton("Button from Window 2");

        WindowUI window3 = NewWindow("Window 3", layout, WindowSetup.defaultSetup);
        window3.AddButton("Button from Window 3");
        window3.AddButton("Button from Window 3");
        window3.AddButton("Button from Window 3");

        AddButton("Return", layout, () => { CloseMenu(true); });
    }
}