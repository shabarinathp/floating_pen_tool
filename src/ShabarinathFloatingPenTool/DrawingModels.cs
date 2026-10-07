using System.Collections.Generic;
using System.Windows;
using System.Windows.Media;

namespace ShabarinathFloatingPenTool;

public enum DrawingTool
{
    Cursor,
    Pen,
    Brush,
    Eraser,
    Line,
    Rectangle,
    Square,
    Circle
}

public sealed class DrawAction
{
    public DrawingTool Tool { get; set; }
    public Color Color { get; set; }
    public double Width { get; set; }
    public List<Point> Points { get; } = new();
}