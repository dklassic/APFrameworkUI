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

- CompositeMenuMono
- SimpleMenu

Beware that since each relies on different navigation logic, best not to mix them together, unless one of them is not interactable.

## CompositeMenuMono

`CompositeMenuMono` is a self-contained class that inherits `MonoBehaviour`, which manages multiple instances of windows and the logic to navigate between them. When utilizing `CompositeMenuMono`, it is expected that all windows will be activated and deactivated together. When working with `CompositeMenuMono`, you should inherit the class and override several parts of it to initialize the menu, meanwhile this is much eacsier to work with if you want some custom navigation between windows.

In the meantime, you can also retrieve the menu via the `WindowManager.intance.GetSystem<T>`, which is really useful for quickly referencing between menus without hassle.

## SimpleMenu

`SimpleMenu` is a class that only contains a single window and the logic to navigate within this window. When multiple instances of `SimpleMenu` are active, the `WindowManager` will be responsible for cross window navigation.

This is most useful to create quick utility menus everywhere.

Study the scripts in [Script](https://github.com/dklassic/APFrameworkUI/tree/next/Assets/Script) folder to see how it instantiates each menu should give you a quick start.

# Requirement

- Unity 2021 LTS
- Text Mesh Pro
- Unity Input System
- [CySharp ZString](https://github.com/Cysharp/ZString)

# Features

## Input

Out of box the UI system directly access the following input through Unity Input System:

- Mouse input
- Mouse scroll
- Keyboard/Controller input

But if you're aiming to use it more than just a utility menu, best implement the input provider yourself for granular control.

## Window Elements

Every window element inherits from the `WindowElement` class, which features a `IStringLabel` label for display, as the element will cache the string value using `IStringLabel.GetValue()`, you can implement the interface in other ways such as returning a function value, or in my own use case, returning a localized value for localization. Each elements will also have a tag that directly represents the hierarchy path of the element, which should be useful as some localization string id.

Currently window elements implemented are all based on my own needs, which are:

- Text (Non-selectable text display)
- Button (Selectable and can trigger action when pressed)
- QuickSelection (Cycles between chioces upon confrim and optionally cancel inputs)
- Selection (Shows up a dedicated option screen for choosing)
- Slider (Capable of taking artibtrary types and triggering callbacks)
- Toggle
- TextInput (Also with predition and autocomplete available)
- ScrollableText (To be able to display given text within designated height with logic to scroll)

## Window

- Auto resizing
- Dragging
- Tons of predefined text based window style
- Tons of window mask animations to be used upon opening and closing UI

## Pre-made Menu Utility

The utilities below are all made by inheriting `CompositeMenuMono`.

### Confirmation Provider

Access through `WindowManager.instance.GetConfirm`, which will show a menu with confirm button and optionally a cancel button, with callbacks available.

### Context Menu Provider

Access through `WindowManager.instance.GetContextMenu`, which can show a context menu with quick access to actions at a designated position.

- A callback is available for when the context menu closes.
- The context menu will be closed when mouse clicked out of window.
- By default the context menu will close on execution of any button, this behavior can be changed.

## Document

For more detailed explaination of how the framework is structured, please refer to the [Document](https://github.com/dklassic/APFrameworkUI/blob/next/Document.md).

## Futurework

- Add option to show an icon to pin window when it is movable
- Add option to show an icon to close window directly, without the need to made an button for that
- Add option to allow the ability to resize window with mouse, maybe?
- Add ordering to menus for priority in detecting input, for now just don't stack them together
- Setup line break better when dealing with lengthy labeled elements
- Add the ability to reorder windows within the same layout with preview, and the ability to return window to the prespecified layout
- Fix layout element size calculation as it is still rather loose to my liking
- Add the ability to make SimpleMenu and CompositeMenu cross navigation possible.

# Fonts Included

For demonstration purpose, two fonts are included in this project:

- IBM - IBM Plex
- Google - Noto Sans CJK

Both of which are licensed under OFL.

Several magic numbers are specifically tuned around these two fonts to make sure each characters are always in width of 1 or 2, therefore when coupled with different fonts several adjustments will have to be made.
