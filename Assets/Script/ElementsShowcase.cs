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
        AddButtonWithCount("This is a Button that increases per confirm and decreases per cancel", systemWindow);
        AddGap(systemWindow);

        SliderUI slider = AddSlider("This is a simple Slider with range", systemWindow);
        slider.SetLimit(-10, 10);
        SliderUIChoice<Quality> sliderChoice =
            AddSliderWithChoice<Quality>("This is a Slider that takes string options", systemWindow);
        List<Quality> result = Enum.GetValues(typeof(Quality)).Cast<Quality>().ToList();
        sliderChoice.SetChoice(result.Select(x => x.ToString()).ToList());
        sliderChoice.SetChoiceValue(result);
        sliderChoice.SetActiveValue(Quality.High);

        AddGap(systemWindow);
        AddText("Here's a scrollable text:", systemWindow);
        ScrollableTextUI text = AddScrollableText("Scrollable", systemWindow);
        text.SetContentHeight(4);
        const string lorem =
            "Lorem ipsum dolor sit amet, consectetur adipiscing elit. Nunc in tempor ante. Morbi eget odio interdum, cursus tortor sit amet, volutpat turpis. Proin eu odio turpis. Nulla in sapien felis. Nulla porttitor, purus eget venenatis lobortis, magna orci blandit enim, et volutpat sapien justo vel erat. Duis tempus porttitor enim, quis finibus dui luctus a. Donec eget erat vestibulum, eleifend ligula non, ultrices lectus. Ut elementum aliquet fermentum. Vestibulum diam purus, dapibus quis libero ut, aliquet vehicula felis. Ut finibus velit fermentum lectus consectetur convallis. Suspendisse scelerisque in dolor non faucibus. Donec volutpat eros ut nunc malesuada elementum. Ut et odio auctor, auctor dolor a, vehicula tellus. Nam elementum sem sit amet pellentesque mollis. Nullam condimentum tellus id pellentesque suscipit.\n\nDuis varius, risus ac fringilla pharetra, magna diam malesuada enim, a varius orci tellus vel eros. Pellentesque quis euismod eros. Aliquam posuere sodales turpis, quis ullamcorper leo tempor quis. Donec justo sem, elementum in scelerisque in, tincidunt sit amet dui. Etiam vitae elit vel mauris tempor fermentum. Interdum et malesuada fames ac ante ipsum primis in faucibus. Maecenas malesuada neque urna, et volutpat est laoreet a. Ut aliquam quis dui nec mattis. Sed ut maximus felis, non feugiat felis. Praesent ullamcorper orci eu odio convallis mattis. Duis tempor enim nec elit porttitor mattis. Quisque sit amet magna non tellus dapibus vulputate eu vel urna. Cras rutrum, metus nec scelerisque aliquet, urna leo suscipit dolor, vitae rhoncus nisl metus in leo. Cras lacinia, ante in commodo maximus, nunc nunc convallis sem, vitae interdum nulla erat nec eros.\n\n";
        text.SetLabel(lorem);
        AddGap(systemWindow);
        AddTextInput("This is a Text Input", systemWindow);

        SingleSelectionUI<int> choice = AddSingleSelection<int>("This is a single selection element", systemWindow);
        choice.SetChoice(new List<string>() { "Here are", "Some", "Options" });

        AddGap(systemWindow);
        ButtonUI disabledButton = AddButton("You can also disable interaction", systemWindow);
        disabledButton.SetAvailable(false);
        AddGap(systemWindow);
        AddText("Here is a Button that takes double confirm to trigger:", systemWindow);
        AddDoubleConfirmButton("Double confirm to return", systemWindow, () => CloseMenu(true));
        systemWindow.Resize(50);
    }
}