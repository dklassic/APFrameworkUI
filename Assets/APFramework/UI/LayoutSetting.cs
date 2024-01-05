using System.Numerics;

public struct LayoutSetting
{
    public LayoutLocation Alignment;
    public LayoutDirection Direction;
    public Vector4 Padding;
    public float Spacing;
    public LayoutSetting(LayoutLocation location, LayoutDirection direction, Vector4 padding, float spacing = 0)
    {
        Alignment = location;
        Direction = direction;
        Padding = padding;
        Spacing = spacing;
    }
    public LayoutSetting(LayoutLocation location, LayoutDirection direction)
    {
        Alignment = location;
        Direction = direction;
        Padding = Vector4.Zero;
        Spacing = 0;
    }
}