using System;
using System.Windows;
using System.Windows.Interop;

namespace HybridVelloPoc.Shell;

public partial class MainWindow : Window
{
    private VelloHostController? _velloController;
    private bool _isInitialized;

    public MainWindow()
    {
        InitializeComponent();
    }

    private void Window_Loaded(object sender, RoutedEventArgs e)
    {
        var helper = new WindowInteropHelper(this);
        var wpfHwnd = helper.Handle;

        StatusText.Text = $"WPF HWND: 0x{wpfHwnd:X8}";
        UpdateDpiDisplay();

        try
        {
            _velloController = new VelloHostController(wpfHwnd, this, ContentHost);
            _velloController.SceneChanged += OnSceneChanged;
            _velloController.FpsUpdated += OnFpsUpdated;
            _velloController.StatusUpdated += OnStatusUpdated;

            _velloController.Start();
            _isInitialized = true;
        }
        catch (Exception ex)
        {
            StatusText.Text = $"Error: {ex.Message}";
            MessageBox.Show($"Failed to initialize Vello: {ex.Message}\n\n{ex.StackTrace}",
                "Initialization Error", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
    {
        _velloController?.Stop();
        _velloController?.Dispose();
        _velloController = null;
    }

    private void Window_LocationChanged(object sender, EventArgs e)
    {
        if (_isInitialized)
        {
            _velloController?.SyncVelloWindowPosition();
        }
    }

    private void Window_SizeChanged(object sender, SizeChangedEventArgs e)
    {
        if (_isInitialized)
        {
            _velloController?.SyncVelloWindowPosition();
            UpdateDpiDisplay();
        }
    }

    private void Window_StateChanged(object sender, EventArgs e)
    {
        if (_isInitialized)
        {
            _velloController?.HandleWindowStateChanged(WindowState);
        }
    }

    private void Window_Activated(object sender, EventArgs e)
    {
        if (_isInitialized)
        {
            _velloController?.HandleWindowActivated(true);
        }
    }

    private void Window_Deactivated(object sender, EventArgs e)
    {
        if (_isInitialized)
        {
            _velloController?.HandleWindowActivated(false);
        }
    }

    private void UpdateDpiDisplay()
    {
        var source = PresentationSource.FromVisual(this);
        if (source?.CompositionTarget != null)
        {
            var dpiX = source.CompositionTarget.TransformToDevice.M11 * 96;
            var dpiY = source.CompositionTarget.TransformToDevice.M22 * 96;
            DpiText.Text = $"DPI: {dpiX:F0}x{dpiY:F0}";
        }
    }

    private void OnSceneChanged(string sceneName, int sceneIndex, int totalScenes)
    {
        Dispatcher.InvokeAsync(() =>
        {
            SceneText.Text = $"Scene: {sceneName} ({sceneIndex + 1}/{totalScenes})";
        });
    }

    private void OnFpsUpdated(double fps)
    {
        Dispatcher.InvokeAsync(() =>
        {
            FpsText.Text = $"FPS: {fps:F1}";
        });
    }

    private void OnStatusUpdated(string status)
    {
        Dispatcher.InvokeAsync(() =>
        {
            StatusText.Text = status;
        });
    }

    // Menu handlers
    private void MenuItem_Exit_Click(object sender, RoutedEventArgs e)
    {
        Close();
    }

    private void MenuItem_PrevScene_Click(object sender, RoutedEventArgs e)
    {
        _velloController?.ChangeScene(-1);
    }

    private void MenuItem_NextScene_Click(object sender, RoutedEventArgs e)
    {
        _velloController?.ChangeScene(1);
    }

    private void MenuItem_ToggleStats_Click(object sender, RoutedEventArgs e)
    {
        _velloController?.ToggleStats();
    }

    private void MenuItem_ResetView_Click(object sender, RoutedEventArgs e)
    {
        _velloController?.ResetView();
    }

    private void MenuItem_About_Click(object sender, RoutedEventArgs e)
    {
        MessageBox.Show(
            "Hybrid Vello POC\n\n" +
            "This proof-of-concept demonstrates hosting a GPU-accelerated Vello/Winit window " +
            "as an owned window of a WPF shell.\n\n" +
            "The Vello window is positioned over the ContentHost area and synchronized " +
            "with the WPF window's position, size, and state.\n\n" +
            "Controls:\n" +
            "- Left/Right: Change scene\n" +
            "- Up/Down: Adjust complexity\n" +
            "- Mouse drag: Pan\n" +
            "- Mouse wheel: Zoom\n" +
            "- Q/E: Rotate\n" +
            "- S: Toggle stats\n" +
            "- Space: Reset view",
            "About Hybrid Vello POC",
            MessageBoxButton.OK,
            MessageBoxImage.Information);
    }
}
