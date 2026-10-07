using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

using WpfColor = System.Windows.Media.Color;

namespace ShabarinathFloatingPenTool;

public partial class ToolbarWindow : Window
{
    private readonly OverlayWindow _overlay;

    public ToolbarWindow(OverlayWindow overlay)
    {
        InitializeComponent();
        _overlay = overlay;

        Left = SystemParameters.WorkArea.Left + 20;
        Top = SystemParameters.WorkArea.Top + 80;
    }

    public void SyncTool(DrawingTool tool)
    {
        ToolLabel.Text = tool.ToString();
        KeepOnTop();
    }

    public void KeepOnTop()
    {
        Topmost = false;
        Topmost = true;
    }

    private void DragHandle_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
    {
        try
        {
            DragMove();
        }
        catch
        {
        }
    }

    private void Tool_Click(object sender, RoutedEventArgs e)
    {
        if (sender is not Button btn || btn.Tag is not string tag)
            return;

        if (Enum.TryParse(tag, out DrawingTool tool))
            _overlay.SetTool(tool);

        KeepOnTop();
    }

    private void Undo_Click(object sender, RoutedEventArgs e) => _overlay.Undo();
    private void Redo_Click(object sender, RoutedEventArgs e) => _overlay.Redo();
    private void Clear_Click(object sender, RoutedEventArgs e) => _overlay.ClearAll();

    private void More_Click(object sender, RoutedEventArgs e)
    {
        MorePanel.Visibility =
            MorePanel.Visibility == Visibility.Visible
                ? Visibility.Collapsed
                : Visibility.Visible;
    }

    private void Close_Click(object sender, RoutedEventArgs e)
    {
        ((App)System.Windows.Application.Current).Quit();
    }

    private void Color_Click(object sender, RoutedEventArgs e)
    {
        using System.Windows.Forms.ColorDialog dialog = new()
        {
            FullOpen = true
        };

        if (dialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
        {
            System.Drawing.Color c = dialog.Color;
            WpfColor mediaColor = WpfColor.FromRgb(c.R, c.G, c.B);

            _overlay.SetColor(mediaColor);
            ColorButton.Foreground = new SolidColorBrush(mediaColor);
        }
    }

    private void SizeBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (SizeBox.SelectedItem is ComboBoxItem item &&
            double.TryParse(item.Content?.ToString(), out double value))
        {
            _overlay.SetWidth(value);
        }
    }
}