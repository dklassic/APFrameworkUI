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

`CompositeMenu` is a self-contained class that manages multiple instances of windows and the logic to navigate between them. When utilizing `CompositeMenu`, it is expected that all windows will be activated and deactivated together. When working with `CompositeMenu`, you can attempt to inherit the class to create custom navigation logic yourself.

Alternatively, you can use `CompositeMenuMono`, which provides exposed property on inspector to setup, while also allows getting the menu instance through the class name.

## SimpleMenu

`SimpleMenu` is a class that only contains a single window and the logic to navigate within this window. When multiple instances of `SimpleMenu` are active, the `WindowManager` will be responsible for cross window navigation.

This is most useful to create quick utility menus everywhere.

# Requirement

- Unity 2021 LTS
- Text Mesh Pro
- Unity Input System
- CySharp ZString

# Features

## Input

Out of box the UI system directly access the following input through Unity Input System:

- Mouse input
- Mouse scroll
- Keyboard/Controller input

But if you're aiming to use it more than just a utility menu, best implement the input provider yourself.

## Window Elements

Every window element inherits from the `WindowElement` class, which features a `IStringLabel` label for display, as the element will cache the string value using `IStringLabel.GetValue()`, you can implement the interface in other ways such as returning a function value, or in my own use case, returning a localized value for localization. Each elements will also have a tag that directly represents the hierarchy path of the element, which should be useful as some string id.

Currently window elements implemented are all based on my own needs, which are:

- Text (Non-selectable text display)
- Button (Selectable and can trigger action when pressed)
- ButtonWithCount (increase upon confirm and decrease upon cancel)
- DoubleConfirmButton (Takes a second confirm to trigger action assigned)
- Slider (Integer choice only)
- SliderWithChoice (String based choice that takes generic type value)
- Toggle
- TextInput
- ScrollableText (To be able to display given text within designated height with logic to scroll)

## Window

- Auto resizing
- Dragging
- Tons of predefined text based window style
- Tons of window mask animations to be used upon opening and closing UI
- 

# Fonts Included

For demonstration purpose, two fonts are included in this project:

- IBM - IBM Plex
- Google - Noto Sans CJK

Both of which are licensed under OFL.

Several magic numbers are specifically tuned around these two fonts and the need to display Chinese characters properly, therefore when coupled with different fonts several adjustments will have to be made.
