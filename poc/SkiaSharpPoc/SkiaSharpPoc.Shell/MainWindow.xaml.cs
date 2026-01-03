using System.Diagnostics;
using System.IO;
using System.Windows;
using System.Windows.Input;
using Microsoft.Win32;
using SkiaSharp;
using SkiaSharp.Views.Desktop;
using SkiaSharp.Views.WPF;

namespace SkiaSharpPoc.Shell;

/// <summary>
/// WPF main window hosting SkiaSharp GPU-accelerated waveform rendering.
/// </summary>
public partial class MainWindow : Window
{
    private readonly WaveformRenderer _waveformRenderer;
    private readonly FrameTimer _frameTimer;
    private readonly Stopwatch _uiUpdateTimer;
    private const int UiUpdateIntervalMs = 125;
    private string _statsOverlayText = string.Empty;
    private readonly SKPaint _statsBackgroundPaint;
    private readonly SKPaint _statsTextPaint;
    private readonly SKFont _statsFont;
    private AudioLoader? _audioLoader;

    // View transform state
    private float _offsetX;
    private float _scaleX = 1.0f;
    private const float MinScale = 0.001f;
    private const float MaxScale = 1000.0f;

    // Mouse interaction state
    private bool _isDragging;
    private Point _lastMousePos;

    // Stats overlay toggle
    private bool _statsOverlayVisible = true;

    public MainWindow()
    {
        InitializeComponent();
        _waveformRenderer = new WaveformRenderer();
        _frameTimer = new FrameTimer();
        _uiUpdateTimer = Stopwatch.StartNew();
        _statsBackgroundPaint = new SKPaint
        {
            Color = new SKColor(0, 0, 0, 180),
            Style = SKPaintStyle.Fill,
            IsAntialias = false
        };
        _statsTextPaint = new SKPaint
        {
            Color = SKColors.White,
            IsAntialias = true
        };
        _statsFont = new SKFont(SKTypeface.FromFamilyName("Consolas"), 14);
    }

    private void Window_Loaded(object sender, RoutedEventArgs e)
    {
        // Try to auto-load the sample audio file
        var samplePath = Path.Combine(AppContext.BaseDirectory, "..", "..", "..", "..", "audio_sample.wav");
        if (File.Exists(samplePath))
        {
            LoadAudioFile(samplePath);
        }
        else
        {
            // Try alternate path from solution root
            var altPath = Path.Combine(AppContext.BaseDirectory, "..", "..", "..", "..", "..", "audio_sample.wav");
            if (File.Exists(altPath))
            {
                LoadAudioFile(altPath);
            }
        }
    }

    private void LoadAudioFile(string path)
    {
        try
        {
            _audioLoader?.Dispose();
            _audioLoader = new AudioLoader(path);

            // Reset view
            _offsetX = 0;
            _scaleX = 1.0f;

            StatusText.Text = $"Loaded: {Path.GetFileName(path)} ({_audioLoader.Duration:F2}s, {_audioLoader.SampleRate}Hz, {_audioLoader.Channels}ch)";
            SkiaCanvas.InvalidateVisual();
        }
        catch (Exception ex)
        {
            StatusText.Text = $"Error loading audio: {ex.Message}";
        }
    }

    private void SkiaCanvas_PaintSurface(object sender, SKPaintSurfaceEventArgs e)
    {
        _frameTimer.BeginFrame();

        var canvas = e.Surface.Canvas;
        var width = e.Info.Width;
        var height = e.Info.Height;

        // Clear background
        canvas.Clear(new SKColor(30, 30, 46)); // Dark background matching reference

        float? startTime = null;
        float? endTime = null;
        if (_audioLoader != null)
        {
            // Calculate visible sample range based on view transform
            var samplesPerPixel = _audioLoader.TotalSamples / (float)width / _scaleX;
            var startSample = (long)Math.Max(0, -_offsetX * samplesPerPixel);
            var endSample = (long)Math.Min(_audioLoader.TotalSamples, startSample + (long)(width * samplesPerPixel));

            _waveformRenderer.Render(canvas, _audioLoader, width, height, startSample, endSample, _offsetX, _scaleX);

            // Capture time range for UI update (throttled below)
            startTime = startSample / (float)_audioLoader.SampleRate;
            endTime = endSample / (float)_audioLoader.SampleRate;
        }

        // Draw stats overlay if enabled
        if (_statsOverlayVisible)
        {
            DrawStatsOverlay(canvas, width);
        }

        _frameTimer.EndFrame();

        // Throttle UI updates to avoid per-frame allocations and UI churn
        if (_uiUpdateTimer.ElapsedMilliseconds >= UiUpdateIntervalMs)
        {
            _uiUpdateTimer.Restart();
            if (startTime.HasValue && endTime.HasValue)
            {
                TimeRangeText.Text = $"Time: {startTime.Value:F2}s - {endTime.Value:F2}s";
                ZoomText.Text = $"Zoom: {_scaleX:F2}x";
            }

            _statsOverlayText =
                $"FPS: {_frameTimer.CurrentFps:F1}\n" +
                $"Frame: {_frameTimer.AvgFrameTimeMs:F2}ms\n" +
                $"Min: {_frameTimer.MinFrameTimeMs:F2}ms\n" +
                $"Max: {_frameTimer.MaxFrameTimeMs:F2}ms";

            FpsText.Text =
                $"FPS: {_frameTimer.CurrentFps:F1} | Frame: {_frameTimer.AvgFrameTimeMs:F2}ms " +
                $"(min {_frameTimer.MinFrameTimeMs:F2} / max {_frameTimer.MaxFrameTimeMs:F2})";
        }
    }

    private void DrawStatsOverlay(SKCanvas canvas, int width)
    {
        if (string.IsNullOrEmpty(_statsOverlayText))
            return;

        var lines = _statsOverlayText.Split('\n');
        var lineHeight = _statsFont.Size + 4;
        var boxWidth = 150;
        var boxHeight = lines.Length * lineHeight + 10;
        var boxX = width - boxWidth - 10;
        var boxY = 10;

        canvas.DrawRect(boxX, boxY, boxWidth, boxHeight, _statsBackgroundPaint);

        for (int i = 0; i < lines.Length; i++)
        {
            canvas.DrawText(lines[i], boxX + 8, boxY + 18 + i * lineHeight, SKTextAlign.Left, _statsFont, _statsTextPaint);
        }
    }

    private void SkiaCanvas_MouseDown(object sender, MouseButtonEventArgs e)
    {
        if (e.LeftButton == MouseButtonState.Pressed)
        {
            _isDragging = true;
            _lastMousePos = e.GetPosition(SkiaCanvas);
            SkiaCanvas.CaptureMouse();
        }
    }

    private void SkiaCanvas_MouseUp(object sender, MouseButtonEventArgs e)
    {
        if (e.LeftButton == MouseButtonState.Released)
        {
            _isDragging = false;
            SkiaCanvas.ReleaseMouseCapture();
        }
    }

    private void SkiaCanvas_MouseMove(object sender, MouseEventArgs e)
    {
        if (_isDragging)
        {
            var currentPos = e.GetPosition(SkiaCanvas);
            var deltaX = (float)(currentPos.X - _lastMousePos.X);

            _offsetX += deltaX;
            _lastMousePos = currentPos;

            SkiaCanvas.InvalidateVisual();
        }
    }

    private void SkiaCanvas_MouseWheel(object sender, MouseWheelEventArgs e)
    {
        var mousePos = e.GetPosition(SkiaCanvas);
        var zoomFactor = e.Delta > 0 ? 1.1f : 1f / 1.1f;

        // Zoom around mouse position
        var oldScale = _scaleX;
        _scaleX = Math.Clamp(_scaleX * zoomFactor, MinScale, MaxScale);

        // Adjust offset to zoom around mouse position
        var scaleRatio = _scaleX / oldScale;
        _offsetX = (float)(mousePos.X - (mousePos.X - _offsetX) * scaleRatio);

        SkiaCanvas.InvalidateVisual();
    }

    private void Window_KeyDown(object sender, KeyEventArgs e)
    {
        switch (e.Key)
        {
            case Key.S:
                _statsOverlayVisible = !_statsOverlayVisible;
                SkiaCanvas.InvalidateVisual();
                break;
            case Key.Space:
                ResetView();
                break;
        }
    }

    private void ResetView()
    {
        _offsetX = 0;
        _scaleX = 1.0f;
        SkiaCanvas.InvalidateVisual();
    }

    private void MenuItem_OpenAudio_Click(object sender, RoutedEventArgs e)
    {
        var dialog = new OpenFileDialog
        {
            Filter = "WAV Files (*.wav)|*.wav|All Files (*.*)|*.*",
            Title = "Open Audio File"
        };

        if (dialog.ShowDialog() == true)
        {
            LoadAudioFile(dialog.FileName);
        }
    }

    private void MenuItem_Exit_Click(object sender, RoutedEventArgs e)
    {
        Close();
    }

    private void MenuItem_ToggleStats_Click(object sender, RoutedEventArgs e)
    {
        _statsOverlayVisible = !_statsOverlayVisible;
        SkiaCanvas.InvalidateVisual();
    }

    private void MenuItem_ResetView_Click(object sender, RoutedEventArgs e)
    {
        ResetView();
    }

    private void MenuItem_About_Click(object sender, RoutedEventArgs e)
    {
        MessageBox.Show(
            "SkiaSharp GPU Waveform POC\n\nA proof-of-concept for GPU-accelerated waveform rendering using SkiaSharp.\n\nControls:\n- Mouse drag: Pan\n- Mouse wheel: Zoom\n- S: Toggle stats overlay\n- Space: Reset view",
            "About",
            MessageBoxButton.OK,
            MessageBoxImage.Information);
    }

    protected override void OnClosed(EventArgs e)
    {
        base.OnClosed(e);
        _audioLoader?.Dispose();
        _statsFont.Dispose();
        _statsTextPaint.Dispose();
        _statsBackgroundPaint.Dispose();
    }
}
