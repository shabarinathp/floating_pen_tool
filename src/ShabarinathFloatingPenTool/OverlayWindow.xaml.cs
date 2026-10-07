using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shapes;

using WpfColor = System.Windows.Media.Color;
using WpfPoint = System.Windows.Point;
using WpfMouseEventArgs = System.Windows.Input.MouseEventArgs;
using WpfKeyEventArgs = System.Windows.Input.KeyEventArgs;

namespace ShabarinathFloatingPenTool;

public partial class OverlayWindow : Window
{
    private DrawingTool _tool = DrawingTool.Cursor;
    private WpfColor _color = Colors.Red;
    private double _width = 4;

    private ToolbarWindow? _toolbar;
    private DrawAction? _active;
    private readonly List<DrawAction> _actions = new();
    private readonly Stack<DrawAction> _redo = new();

    public OverlayWindow()
    {
        InitializeComponent();

        Left = SystemParameters.VirtualScreenLeft;
        Top = SystemParameters.VirtualScreenTop;
        Width = SystemParameters.VirtualScreenWidth;
        Height = SystemParameters.VirtualScreenHeight;

        PreviewKeyDown += OverlayWindow_PreviewKeyDown;
    }

    public void AttachToolbar(ToolbarWindow toolbar) => _toolbar = toolbar;

    public void SetTool(DrawingTool tool)
    {
        _tool = tool;

        bool cursorMode = tool == DrawingTool.Cursor;
        NativeMethods.SetClickThrough(this, cursorMode);

        Cursor = cursorMode ? Cursors.Arrow : Cursors.Cross;

        if (!cursorMode)
            Focus();

        _toolbar?.SyncTool(tool);
        _toolbar?.KeepOnTop();
    }

    public void SetColor(WpfColor color) => _color = color;
    public void SetWidth(double width) => _width = width;

    public void Undo()
    {
        if (_actions.Count == 0)
            return;

        DrawAction last = _actions[^1];
        _actions.RemoveAt(_actions.Count - 1);
        _redo.Push(last);
        Redraw();
    }

    public void Redo()
    {
        if (_redo.Count == 0)
            return;

        _actions.Add(_redo.Pop());
        Redraw();
    }

    public void ClearAll()
    {
        foreach (DrawAction action in _actions)
            _redo.Push(action);

        _actions.Clear();
        Redraw();
    }

    private void OverlayWindow_PreviewKeyDown(object sender, WpfKeyEventArgs e)
    {
        if ((Keyboard.Modifiers & ModifierKeys.Control) != 0 && e.Key == Key.Z)
        {
            Undo();
            e.Handled = true;
        }
        else if ((Keyboard.Modifiers & ModifierKeys.Control) != 0 && e.Key == Key.Y)
        {
            Redo();
            e.Handled = true;
        }
        else if (e.Key == Key.Escape)
        {
            SetTool(DrawingTool.Cursor);
            e.Handled = true;
        }
        else if ((Keyboard.Modifiers & (ModifierKeys.Control | ModifierKeys.Shift)) ==
                 (ModifierKeys.Control | ModifierKeys.Shift) && e.Key == Key.C)
        {
            ClearAll();
            e.Handled = true;
        }
        else if ((Keyboard.Modifiers & (ModifierKeys.Control | ModifierKeys.Shift)) ==
                 (ModifierKeys.Control | ModifierKeys.Shift) && e.Key == Key.Q)
        {
            ((App)System.Windows.Application.Current).Quit();
            e.Handled = true;
        }
    }

    private void RootCanvas_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
    {
        if (_tool == DrawingTool.Cursor)
            return;

        WpfPoint p = e.GetPosition(RootCanvas);

        _active = new DrawAction
        {
            Tool = _tool,
            Color = _color,
            Width = _tool == DrawingTool.Brush ? Math.Max(_width, 12)
                  : _tool == DrawingTool.Eraser ? Math.Max(_width, 20)
                  : _width
        };

        _active.Points.Add(p);

        if (_tool is DrawingTool.Line or DrawingTool.Rectangle or DrawingTool.Square or DrawingTool.Circle)
            _active.Points.Add(p);

        _redo.Clear();
        RootCanvas.CaptureMouse();

        Redraw(_active);
        _toolbar?.KeepOnTop();
    }

    private void RootCanvas_MouseMove(object sender, WpfMouseEventArgs e)
    {
        if (_active == null || e.LeftButton != MouseButtonState.Pressed)
            return;

        WpfPoint p = e.GetPosition(RootCanvas);

        if (_active.Tool is DrawingTool.Pen or DrawingTool.Brush or DrawingTool.Eraser)
            _active.Points.Add(p);
        else
            _active.Points[1] = p;

        Redraw(_active);
        _toolbar?.KeepOnTop();
    }

    private void RootCanvas_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
    {
        if (_active == null)
            return;

        _actions.Add(_active);
        _active = null;

        RootCanvas.ReleaseMouseCapture();
        Redraw();
        _toolbar?.KeepOnTop();
    }

    private void Redraw(DrawAction? preview = null)
    {
        RootCanvas.Children.Clear();

        foreach (DrawAction action in _actions)
            DrawActionToCanvas(action);

        if (preview != null)
            DrawActionToCanvas(preview);
    }

    private void DrawActionToCanvas(DrawAction action)
    {
        if (action.Points.Count == 0)
            return;

        if (action.Tool is DrawingTool.Pen or DrawingTool.Brush)
        {
            Polyline poly = new()
            {
                StrokeThickness = action.Width,
                StrokeStartLineCap = PenLineCap.Round,
                StrokeEndLineCap = PenLineCap.Round,
                StrokeLineJoin = PenLineJoin.Round
            };

            WpfColor c = action.Color;

            if (action.Tool == DrawingTool.Brush)
                c = WpfColor.FromArgb(110, c.R, c.G, c.B);

            poly.Stroke = new SolidColorBrush(c);

            foreach (WpfPoint p in action.Points)
                poly.Points.Add(p);

            RootCanvas.Children.Add(poly);
            return;
        }

        if (action.Tool == DrawingTool.Eraser)
        {
            // V5.1 keeps the eraser predictable by removing the nearest
            // completed annotation action instead of painting opaque pixels.
            RemoveNearestAction(action.Points[^1], action.Width);
            return;
        }

        if (action.Points.Count < 2)
            return;

        WpfPoint p1 = action.Points[0];
        WpfPoint p2 = action.Points[1];
        SolidColorBrush brush = new(action.Color);

        if (action.Tool == DrawingTool.Line)
        {
            RootCanvas.Children.Add(new Line
            {
                X1 = p1.X,
                Y1 = p1.Y,
                X2 = p2.X,
                Y2 = p2.Y,
                Stroke = brush,
                StrokeThickness = action.Width
            });
            return;
        }

        double x = Math.Min(p1.X, p2.X);
        double y = Math.Min(p1.Y, p2.Y);
        double w = Math.Abs(p2.X - p1.X);
        double h = Math.Abs(p2.Y - p1.Y);

        if (action.Tool == DrawingTool.Square)
        {
            double side = Math.Min(w, h);
            w = side;
            h = side;
        }

        Shape shape = action.Tool == DrawingTool.Circle
            ? new Ellipse()
            : new Rectangle();

        shape.Width = w;
        shape.Height = h;
        shape.Stroke = brush;
        shape.StrokeThickness = action.Width;
        shape.Fill = Brushes.Transparent;

        Canvas.SetLeft(shape, x);
        Canvas.SetTop(shape, y);
        RootCanvas.Children.Add(shape);
    }

    private void RemoveNearestAction(WpfPoint point, double radius)
    {
        if (_actions.Count == 0)
            return;

        double threshold = Math.Max(radius * 2.0, 30.0);

        for (int i = _actions.Count - 1; i >= 0; i--)
        {
            DrawAction candidate = _actions[i];

            foreach (WpfPoint p in candidate.Points)
            {
                double dx = p.X - point.X;
                double dy = p.Y - point.Y;

                if ((dx * dx) + (dy * dy) <= threshold * threshold)
                {
                    _redo.Push(candidate);
                    _actions.RemoveAt(i);
                    return;
                }
            }
        }
    }
}