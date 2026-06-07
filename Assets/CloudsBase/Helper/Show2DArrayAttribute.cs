using UnityEngine;

public class Show2DArrayAttribute : PropertyAttribute
{
    public readonly int Width;
    public readonly int Height;

    public Show2DArrayAttribute(int height, int width)
    {
        Width = width;
        Height = height;
    }
}
