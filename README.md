# APFramework UI System

> A Text Mesh Pro based text only UI system for Unity

APFramework is a framework created by [DK Liao](https://twitter.com/RandomDevDK) used in [Autopanic](https://store.steampowered.com/app/1274830) and [Autopanic Zero](https://store.steampowered.com/app/1423670).

The UI System for APFramework is designed with two goals in mind:

- To be a text only UI system
- To be able to quickly initialize UI using code only

This really helps due to the solo nature of both games made with APFramework, allowing multitude of menus of different functions to be created with ease.

The UI system is heavily coupled with Text Mesh Pro at the moment but should be easily modifiable to suit other packages or game engine.

![showcase](https://blog.chosenconcept.dev/images/posts/autopanic-devlog/0007/3.gif)

The creation of this UI system is heavily inspired by [PhiOS](https://github.com/pblca/PhiOS) (mirror repo) made by [phi6](https://twitter.com/phi6).

# Getting Started

APFramework's UI system is a very code based system, it is best used when you want to have a quick debug menu instantiated everywhere without the need to commit to making UIs for every random usage. That being said, you can also try to make use of APFramework's UI logic and rework the presentation yourself.

There are two styles of APFramework usage:

- CompositeMenu
- SimpleMenu

Beware that since each relies on different navigation logic, best not to mix them together, unless one of them is not interactable.

## CompositeMenu

`CompositeMenu` is a class that manages multiple instances of windows and the logic to navigate between them. When utilizing `CompositeMenu`, it is expected that all windows will be activated and deactivated together.

When utilizing `CompositeMenu`, it is expected that you'll attempt to inherit 

## SimpleMenu

`SimpleMenu` is a class that only contains a single window and the logic to navigate within this window. When multiple instances of `SimpleMenu` is active, the `WindowManager` instance will be responsible for navigation between the windows.

This is most useful 

A quick UI code would look like this:
```C#
public class SuperQuickMenu : GeneralUISystemWithNavigation
{
    protected override void InitializeUI()
    {
        AddButton("This is a quick single Button", ButtonPressedAction);
    }
    void ButtonPressedAction() => _ = 0;
}
```
By inheriting the `GeneralUISystemWithNavigation` class, you can quickly initialize UI by overriding `InitializeUI()`. The quickest way to setup a UI is to simply create an UI initialized with a single `WindowElement`. That wouldn't be much useful at all so usually you'd want to initialize a `WindowUI` then add in all the elements:
```C#
public class BiggerWindow : GeneralUISystemWithNavigation
{
    protected override void InitializeUI()
    {
        WindowUI systemWindow = NewWindow("A New Window", DefaultSetup);
        AddText("This is a non-selectable text", systemWindow);
        AddToggle("This is a Toggle", systemWindow);
        AddButton("This is a Button", systemWindow);
        SliderUI slider = AddSlider("This is a simple Slider with range", systemWindow);
        slider.SetLimit(-10, 10);
        SliderUIChoice sliderChoice = AddSliderWithChoice("This is a Slider that takes string options", systemWindow);
        sliderChoice.AddChoice("Low");
        sliderChoice.AddChoice("Medium");
        sliderChoice.AddChoice("High");
        systemWindow.AutoResize();
    }
}
```
Study example UIs within the Script folder should give you a good idea:
- [`ElementsShowcase`](https://github.com/dklassic/APFrameworkUI/blob/main/Assets/Script/ElementsShowcase.cs) shows all `WindowElement` available
- [`MultipleWindow`](https://github.com/dklassic/APFrameworkUI/blob/main/Assets/Script/MultipleWindow.cs) is a demonstration on how to initialize multiple `WindowUI` sharing the same layout
- [`ExampleMenu`](https://github.com/dklassic/APFrameworkUI/blob/main/Assets/Script/ExampleMenu.cs) is an example of SubMenu interaction if you want to use that
- [`ChineseDisplay`](https://github.com/dklassic/APFrameworkUI/blob/main/Assets/Script/ChineseDisplay.cs) is a showcase on the displaying of Chinese
- [`ResolutionSetting`](https://github.com/dklassic/APFrameworkUI/blob/main/Assets/Script/ResolutionSetting.cs) shows how to handle resolution change and how to change the overlay mode

Some other quick notes:
- [`GeneralUISystem`](https://github.com/dklassic/APFrameworkUI/blob/main/Assets/APFramework/UI/GeneralUISystem.cs) class can be inherited to use on UIs without any interactivity.
- [`GeneralUISystemWithNavigation`](https://github.com/dklassic/APFrameworkUI/blob/main/Assets/APFramework/UI/GeneralUISystemWithNavigation.cs) class can be inherited to use on UIs that takes input.
- [`UIManager`](https://github.com/dklassic/APFrameworkUI/blob/main/Assets/APFramework/UI/UIManager.cs) handles the initialization of all UI, you can bypass `GeneralUISystemWithNavigation` and create UI with this alone if you want.
- Currently the layout of UI displayed requires predetermined layout under UIManager prefab.
- [`UIInput`](https://github.com/dklassic/APFrameworkUI/blob/main/Assets/APFramework/UI/UIInput.cs) handles the input.
- [`StyleUtility`](https://github.com/dklassic/APFrameworkUI/blob/main/Assets/APFramework/UI/StyleUtility.cs) handles all the color choice.
- [`TextUtility`](https://github.com/dklassic/APFrameworkUI/blob/main/Assets/APFramework/UI/TextUtility.cs) contains several text related utility function.
- [`CoroutineUtility`](https://github.com/dklassic/APFrameworkUI/blob/main/Assets/APFramework/UI/CoroutineUtility.cs) is a utility for GC-less coroutine, can change the implementation to Unitask instead if you want to.

# Detailed Design Dive (WIP)

# Requirement

- Unity 2021 LTS (but this system's creation dated back to the days of Unity 2019 so should be widely compatible)
- Text Mesh Pro
- Input System
- ZString

# Features

## Input

- Mouse input handling
- Mouse scroll handling
- Keyboard/Controller input handling

## Window Elements

- Text
- Button
- ButtonWithCount (Used for a Enemy Debugger in Autopanic to quickly setup enemy amount, increase upon confirm and decrease upon cancel)
- DoubleConfirmButton (Takes a second confirm to trigger action assigned)
- Slider (Integer choice only)
- SliderWithChoice (String based choice)
- Toggle

## Window

- Sophisticated auto resizing
- Tons of predefined text based window style
- Tons of window mask animations to be used upon opening and closing UI
- To be able to show Title and Subscript of a window

# Fonts Included

For demonstration purpose, two fonts are included in this project:

- IBM - IBM Plex
- Google - Noto Sans CJK

Both of which are licensed under OFL.

Several magic numbers are specifically tuned around these two fonts and the need to display Chinese characters properly, therefore when coupled with different fonts several adjustments will have to be made.
