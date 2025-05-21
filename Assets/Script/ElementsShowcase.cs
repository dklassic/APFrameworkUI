using System;
using System.Collections.Generic;
using System.Linq;
using ChosenConcept.APFramework.Interface.Framework;
using ChosenConcept.APFramework.Interface.Framework.Element;

public class ElementsShowcase : CompositeMenuMono
{
    public enum Quality
    {
        Low,
        Medium,
        High,
        Ultra
    }

    protected override void InitializeUI()
    {
        WindowUI systemWindow = NewWindow("Elements Showcase", WindowSetup.defaultSetup);
        AddToggle("This is a Toggle", systemWindow);
        AddButton("This is a Button", systemWindow);
        AddQuickSelectionUI<int>("This is a Button that increases per confirm and decreases per cancel", systemWindow)
            .SetChoiceByValue(new List<int> { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10 });
        AddGap(systemWindow);

        AddSlider<int>("This is a simple Slider using numeric value", systemWindow)
            .SetChoiceByValue(new List<int> { 1, 2, 3 })
            .SetActiveValue(1);
        List<Quality> result = Enum.GetValues(typeof(Quality)).Cast<Quality>().ToList();
        AddSlider<Quality>("This is a Slider that takes a Enum as value", systemWindow)
            .SetChoiceByValue(result)
            .SetActiveValue(Quality.High);

        AddGap(systemWindow);
        AddText("Here's a scrollable text:", systemWindow);
        const string lorem =
            "Lorem ipsum dolor sit amet, consectetur adipiscing elit. Nunc in tempor ante. Morbi eget odio interdum, cursus tortor sit amet, volutpat turpis. Proin eu odio turpis. Nulla in sapien felis. Nulla porttitor, purus eget venenatis lobortis, magna orci blandit enim, et volutpat sapien justo vel erat. Duis tempus porttitor enim, quis finibus dui luctus a. Donec eget erat vestibulum, eleifend ligula non, ultrices lectus. Ut elementum aliquet fermentum. Vestibulum diam purus, dapibus quis libero ut, aliquet vehicula felis. Ut finibus velit fermentum lectus consectetur convallis. Suspendisse scelerisque in dolor non faucibus. Donec volutpat eros ut nunc malesuada elementum. Ut et odio auctor, auctor dolor a, vehicula tellus. Nam elementum sem sit amet pellentesque mollis. Nullam condimentum tellus id pellentesque suscipit.\n\nDuis varius, risus ac fringilla pharetra, magna diam malesuada enim, a varius orci tellus vel eros. Pellentesque quis euismod eros. Aliquam posuere sodales turpis, quis ullamcorper leo tempor quis. Donec justo sem, elementum in scelerisque in, tincidunt sit amet dui. Etiam vitae elit vel mauris tempor fermentum. Interdum et malesuada fames ac ante ipsum primis in faucibus. Maecenas malesuada neque urna, et volutpat est laoreet a. Ut aliquam quis dui nec mattis. Sed ut maximus felis, non feugiat felis. Praesent ullamcorper orci eu odio convallis mattis. Duis tempor enim nec elit porttitor mattis. Quisque sit amet magna non tellus dapibus vulputate eu vel urna. Cras rutrum, metus nec scelerisque aliquet, urna leo suscipit dolor, vitae rhoncus nisl metus in leo. Cras lacinia, ante in commodo maximus, nunc nunc convallis sem, vitae interdum nulla erat nec eros.\n\n";
        AddScrollableText("Scrollable", systemWindow)
            .SetContentHeight(4)
            .SetLabel(lorem);
        AddGap(systemWindow);
        AddTextInput("This is a Text Input", systemWindow)
            .SetInputContent("Also with pre-entered text!");
        AddGap(systemWindow);
        AddSingleSelection<Choices>("This is a single selection element", systemWindow)
            .SetChoiceByValue(Enum.GetValues(typeof(Choices)).Cast<Choices>().ToList());
        AddGap(systemWindow);
        AddText("You can hide labels for some elements:", systemWindow);
        AddSingleSelection<Choices>("This is a single selection element", systemWindow)
            .SetChoiceByValue(Enum.GetValues(typeof(Choices)).Cast<Choices>().ToList())
            .ShowLabel(false);

        AddGap(systemWindow);
        AddButton("You can also disable interaction", systemWindow)
            .SetAvailable(false);
        AddGap(systemWindow);
        AddText("Here is a Button that takes double confirm to trigger:", systemWindow);
        AddButton("Double confirm to return", systemWindow, () => CloseMenu(true)).SetConfirmText("Confirm");

        systemWindow.Resize(50);
    }

    public enum Choices
    {
        HereAreSomeChoices,
        ThatYouCanPasThrough,
        WithAGenericType
    }
}