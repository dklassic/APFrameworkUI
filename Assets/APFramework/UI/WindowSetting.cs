using System.Collections.Generic;
using UnityEngine;
public class WindowSetting : MonoBehaviour
{
    public List<WindowStyle> EmbeddedTitle = new List<WindowStyle>();
    public List<WindowStyle> Titlebar = new List<WindowStyle>();
    public List<WindowStyle> FrameOnly = new List<WindowStyle>();
    public List<WindowStyle> FullFrame = new List<WindowStyle>();
    public List<WindowStyle> NoOutline = new List<WindowStyle>();
    public List<WindowStyle> CornerSet = new List<WindowStyle>();
    public List<WindowStyle> LowerLeftSet = new List<WindowStyle>();
    public List<WindowStyle> LeftLine = new List<WindowStyle>();
    public List<WindowStyle> ThickenEdge = new List<WindowStyle>();
    public List<WindowStyle> ThickenCorner = new List<WindowStyle>();
    public List<WindowStyle> LeftLabel = new List<WindowStyle>();
    public List<WindowStyle> RightLabel = new List<WindowStyle>();
    public bool IsFullFrame(WindowStyle style) => FullFrame.Contains(style);
    public bool HasEmbeddedTitle(WindowStyle style) => EmbeddedTitle.Contains(style);
    public bool HasTitlebar(WindowStyle style) => Titlebar.Contains(style);
    public bool HasOutline(WindowStyle style) => !NoOutline.Contains(style);
    public bool HasTitle(WindowStyle style) => !FrameOnly.Contains(style);
    public bool IsCornerSet(WindowStyle style) => CornerSet.Contains(style);
    public bool IsLowerLeft(WindowStyle style) => LowerLeftSet.Contains(style);
    public bool IsLeftLine(WindowStyle style) => LeftLine.Contains(style);
    public bool HasThickenEdge(WindowStyle style) => ThickenEdge.Contains(style);
    public bool HasThickenCorner(WindowStyle style) => ThickenCorner.Contains(style);
    public bool HasLeftLabel(WindowStyle style) => LeftLabel.Contains(style);
    public bool HasRightLabel(WindowStyle style) => RightLabel.Contains(style);
}