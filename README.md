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

# How to Use

Study example UIs within the Script folder should give you a good idea.

`GeneralUISystem` class can be inherited to use on UIs without any interactivity.

`GeneralUISystemWithNavigation` class can be inherited to use on UIs that takes input.

`UIManager` handles the initialization of all UI, you can bypass `GeneralUISystemNavigation` and create UI with this alone if you want. Currently the layout of UI displayed requires predetermined layout under UIManager prefab.

# Requirement

- Unity 2021 LTS (but this system's creation dated back to the days of Unity 2019 so should be widely compatible)
- Text Mesh Pro
- Input System

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
- ExclusiveToggle (cancels out other ExclusiveToggles when one is selected)

## Window

- Sophisticated auto resizing
- Tons of predefined text based window style
- Tons of window mask animations to be used upon opening and closing UI
- To be able to show Title and Subscript of a window


# Fonts Included

For demonstration purpose, two fonts are been included in this project:

- IBM - IBM Plex
- Google - Noto Sans CJK

Both of which are licensed under OFL.

Several magic numbers are specifically tuned around these two fonts and the need to display Chinese characters properly, therefore when coupled with different fonts several adjustments will have to be made.
