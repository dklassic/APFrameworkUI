using System.Collections;
using UnityEngine;

public static class LayoutUtility
{
    public static readonly LayoutSetting UpperLeftVertical = new LayoutSetting(LayoutLocation.TopLeft, LayoutDirection.Vertical);
    public static readonly LayoutSetting UpperCenterVertical = new LayoutSetting(LayoutLocation.TopCenter, LayoutDirection.Vertical);
    public static readonly LayoutSetting UpperRightVertical = new LayoutSetting(LayoutLocation.TopRight, LayoutDirection.Vertical);
    public static readonly LayoutSetting MiddleLeftVertical = new LayoutSetting(LayoutLocation.MiddleLeft, LayoutDirection.Vertical);
    public static readonly LayoutSetting MiddleCenterVertical = new LayoutSetting(LayoutLocation.MiddleCenter, LayoutDirection.Vertical);
    public static readonly LayoutSetting MiddleRightVertical = new LayoutSetting(LayoutLocation.MiddleRight, LayoutDirection.Vertical);
    public static readonly LayoutSetting LowerLeftVertical = new LayoutSetting(LayoutLocation.BottomLeft, LayoutDirection.Vertical);
    public static readonly LayoutSetting LowerCenterVertical = new LayoutSetting(LayoutLocation.BottomCenter, LayoutDirection.Vertical);
    public static readonly LayoutSetting LowerRightVertical = new LayoutSetting(LayoutLocation.BottomRight, LayoutDirection.Vertical);
    public static readonly LayoutSetting UpperLeftHorizontal = new LayoutSetting(LayoutLocation.TopLeft, LayoutDirection.Horizontal);
    public static readonly LayoutSetting UpperCenterHorizontal = new LayoutSetting(LayoutLocation.TopCenter, LayoutDirection.Horizontal);
    public static readonly LayoutSetting UpperRightHorizontal = new LayoutSetting(LayoutLocation.TopRight, LayoutDirection.Horizontal);
    public static readonly LayoutSetting MiddleLeftHorizontal = new LayoutSetting(LayoutLocation.MiddleLeft, LayoutDirection.Horizontal);
    public static readonly LayoutSetting MiddleCenterHorizontal = new LayoutSetting(LayoutLocation.MiddleCenter, LayoutDirection.Horizontal);
    public static readonly LayoutSetting MiddleRightHorizontal = new LayoutSetting(LayoutLocation.MiddleRight, LayoutDirection.Horizontal);
    public static readonly LayoutSetting LowerLeftHorizontal = new LayoutSetting(LayoutLocation.BottomLeft, LayoutDirection.Horizontal);
    public static readonly LayoutSetting LowerCenterHorizontal = new LayoutSetting(LayoutLocation.BottomCenter, LayoutDirection.Horizontal);
    public static readonly LayoutSetting LowerRightHorizontal = new LayoutSetting(LayoutLocation.BottomRight, LayoutDirection.Horizontal);


    public static LayoutSetting GetSettingByPreset(LayoutPreset preset) => preset switch
    {
        LayoutPreset.UpperLeftVertical => UpperLeftVertical,
        LayoutPreset.UpperCenterVertical => UpperCenterVertical,
        LayoutPreset.UpperRightVertical => UpperRightVertical,
        LayoutPreset.MiddleLeftVertical => MiddleLeftVertical,
        LayoutPreset.MiddleCenterVertical => MiddleCenterVertical,
        LayoutPreset.MiddleRightVertical => MiddleRightVertical,
        LayoutPreset.LowerLeftVertical => LowerLeftVertical,
        LayoutPreset.LowerCenterVertical => LowerCenterVertical,
        LayoutPreset.LowerRightVertical => LowerRightVertical,
        LayoutPreset.UpperLeftHorizontal => UpperLeftHorizontal,
        LayoutPreset.UpperCenterHorizontal => UpperCenterHorizontal,
        LayoutPreset.UpperRightHorizontal => UpperRightHorizontal,
        LayoutPreset.MiddleLeftHorizontal => MiddleLeftHorizontal,
        LayoutPreset.MiddleCenterHorizontal => MiddleCenterHorizontal,
        LayoutPreset.MiddleRightHorizontal => MiddleRightHorizontal,
        LayoutPreset.LowerLeftHorizontal => LowerLeftHorizontal,
        LayoutPreset.LowerCenterHorizontal => LowerCenterHorizontal,
        LayoutPreset.LowerRightHorizontal => LowerRightHorizontal,
        _ => MiddleCenterVertical,
    };
}

public enum LayoutPreset
{
    UpperLeftVertical,
    UpperCenterVertical,
    UpperRightVertical,
    MiddleLeftVertical,
    MiddleCenterVertical,
    MiddleRightVertical,
    LowerLeftVertical,
    LowerCenterVertical,
    LowerRightVertical,
    UpperLeftHorizontal,
    UpperCenterHorizontal,
    UpperRightHorizontal,
    MiddleLeftHorizontal,
    MiddleCenterHorizontal,
    MiddleRightHorizontal,
    LowerLeftHorizontal,
    LowerCenterHorizontal,
    LowerRightHorizontal,
}