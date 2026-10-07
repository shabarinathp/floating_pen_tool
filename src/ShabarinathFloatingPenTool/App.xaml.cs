using System.Windows;

namespace ShabarinathFloatingPenTool;

public partial class App : Application
{
    private OverlayWindow? _overlay;
    private ToolbarWindow? _toolbar;

    protected override void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);

        _overlay = new OverlayWindow();
        _toolbar = new ToolbarWindow(_overlay);

        _overlay.AttachToolbar(_toolbar);

        _overlay.Show();
        _toolbar.Show();
        _toolbar.Activate();

        _overlay.SetTool(DrawingTool.Cursor);
    }

    public void Quit()
    {
        _toolbar?.Close();
        _overlay?.Close();
        Shutdown();
    }
}