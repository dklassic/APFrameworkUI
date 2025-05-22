using System;
using System.Collections.Generic;
using System.Linq;
using ChosenConcept.APFramework.Interface.Framework;
using UnityEngine;

public class ElementsShowcase : CompositeMenuMono
{
    public enum Quality
    {
        Low,
        Medium,
        High,
        Ultra
    }

    public enum Choices
    {
        HereAreSomeChoices,
        ThatYouCanPasThrough,
        WithAGenericType
    }


    protected override void InitializeMenu()
    {
        WindowUI systemWindow = NewWindow("Elements Showcase", WindowSetup.defaultSetup);
        systemWindow.AddToggle("This is a Toggle");
        systemWindow.AddButton("This is a Button");
        systemWindow
            .AddQuickSelectionUI<int>("QuickSelection allows cycling through values with confirm or cancel presses.")
            .SetChoiceByValue(new List<int> { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10 })
            .SetCanCycleBackward(true);
        systemWindow.AddGap();

        systemWindow.AddSlider<int>("This is a simple Slider using numeric value")
            .SetChoiceByValue(new List<int> { 1, 2, 3 })
            .SetActiveValue(1);
        systemWindow.AddSlider<Quality>("This is a Slider that takes a Enum as value")
            .SetChoiceByValue(Enum.GetValues(typeof(Quality)).Cast<Quality>())
            .SetActiveValue(Quality.High);

        systemWindow.AddGap();
        systemWindow.AddText("Here's a scrollable text:");
        const string lorem =
            "Lorem ipsum dolor sit amet, consectetur adipiscing elit. Nunc in tempor ante. Morbi eget odio interdum, cursus tortor sit amet, volutpat turpis. Proin eu odio turpis. Nulla in sapien felis. Nulla porttitor, purus eget venenatis lobortis, magna orci blandit enim, et volutpat sapien justo vel erat. Duis tempus porttitor enim, quis finibus dui luctus a. Donec eget erat vestibulum, eleifend ligula non, ultrices lectus. Ut elementum aliquet fermentum. Vestibulum diam purus, dapibus quis libero ut, aliquet vehicula felis. Ut finibus velit fermentum lectus consectetur convallis. Suspendisse scelerisque in dolor non faucibus. Donec volutpat eros ut nunc malesuada elementum. Ut et odio auctor, auctor dolor a, vehicula tellus. Nam elementum sem sit amet pellentesque mollis. Nullam condimentum tellus id pellentesque suscipit.\n\nDuis varius, risus ac fringilla pharetra, magna diam malesuada enim, a varius orci tellus vel eros. Pellentesque quis euismod eros. Aliquam posuere sodales turpis, quis ullamcorper leo tempor quis. Donec justo sem, elementum in scelerisque in, tincidunt sit amet dui. Etiam vitae elit vel mauris tempor fermentum. Interdum et malesuada fames ac ante ipsum primis in faucibus. Maecenas malesuada neque urna, et volutpat est laoreet a. Ut aliquam quis dui nec mattis. Sed ut maximus felis, non feugiat felis. Praesent ullamcorper orci eu odio convallis mattis. Duis tempor enim nec elit porttitor mattis. Quisque sit amet magna non tellus dapibus vulputate eu vel urna. Cras rutrum, metus nec scelerisque aliquet, urna leo suscipit dolor, vitae rhoncus nisl metus in leo. Cras lacinia, ante in commodo maximus, nunc nunc convallis sem, vitae interdum nulla erat nec eros.\n\n";
        systemWindow.AddScrollableText("Scrollable")
            .SetContentHeight(4)
            .SetLabel(lorem);
        systemWindow.AddGap();
        systemWindow.AddTextInput("This is a Text Input")
            .SetInputContent("Also with pre-entered text!");
        systemWindow.AddGap();
        systemWindow.AddSingleSelection<Choices>("This is a single selection element")
            .SetChoiceByValue(Enum.GetValues(typeof(Choices)).Cast<Choices>());
        systemWindow.AddGap();
        systemWindow.AddText("You can hide labels for some elements:");
        systemWindow.AddSingleSelection<Choices>("This is a single selection element")
            .SetChoiceByValue(Enum.GetValues(typeof(Choices)).Cast<Choices>())
            .ShowLabel(false);

        systemWindow.AddGap();
        systemWindow.AddButton("You can also disable interaction")
            .SetAvailable(false);
        systemWindow.AddGap();
        systemWindow.AddText("Here is a Button that takes double confirm to trigger:");
        systemWindow.AddButton("Double confirm to return", () => CloseMenu(true))
            .SetConfirmText("Confirm");
    }
}