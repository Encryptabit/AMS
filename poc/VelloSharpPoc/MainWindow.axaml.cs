using System.Numerics;
using Avalonia.Controls;
using Avalonia.Threading;
using VelloSharp;
using VelloSharp.Avalonia.Controls;

namespace VelloSharpPoc;

public partial class MainWindow : Window
{
    private int _frameCount;
    private DateTime _lastFpsUpdate = DateTime.Now;
    private double _currentFps;
    private DispatcherTimer? _forceRedrawTimer;
    private DateTime _startTime = DateTime.Now;

    public MainWindow()
    {
        InitializeComponent();

        // Force redraw with our own timer as workaround
        _forceRedrawTimer = new DispatcherTimer(DispatcherPriority.Render)
        {
            Interval = TimeSpan.FromMilliseconds(16) // ~60 FPS
        };
        _forceRedrawTimer.Tick += (s, e) => VelloCanvas.InvalidateVisual();
        _forceRedrawTimer.Start();
    }

    private void OnDraw(object? sender, VelloDrawEventArgs e)
    {
        var scene = e.Scene;
        var bounds = e.Bounds;

        // Use our own time tracking since TotalTime might not work
        var time = (DateTime.Now - _startTime).TotalSeconds;

        // Update FPS counter
        _frameCount++;
        var now = DateTime.Now;
        if ((now - _lastFpsUpdate).TotalSeconds >= 0.5)
        {
            _currentFps = _frameCount / (now - _lastFpsUpdate).TotalSeconds;
            _frameCount = 0;
            _lastFpsUpdate = now;

            Dispatcher.UIThread.Post(() =>
            {
                FpsText.Text = $"FPS: {_currentFps:F0}";
            });
        }

        // Fill background with dark blue
        var bgPath = new PathBuilder()
            .MoveTo(0, 0)
            .LineTo((float)bounds.Width, 0)
            .LineTo((float)bounds.Width, (float)bounds.Height)
            .LineTo(0, (float)bounds.Height)
            .Close();

        scene.FillPath(bgPath, FillRule.NonZero, e.GlobalTransform,
            RgbaColor.FromBytes(20, 25, 45, 255));

        // Draw animated waveform
        var waveformPath = new PathBuilder();
        var centerY = bounds.Height / 2;

        waveformPath.MoveTo(0, (float)centerY);

        for (int i = 0; i <= 400; i++)
        {
            var x = (float)(i * bounds.Width / 400);
            var y = (float)(centerY
                + Math.Sin(time * 2.0 + i * 0.02) * 60
                + Math.Sin(time * 3.5 + i * 0.04) * 30
                + Math.Sin(time * 1.2 + i * 0.01) * 20);
            waveformPath.LineTo(x, y);
        }

        scene.StrokePath(waveformPath, new StrokeStyle { Width = 2.5f },
            e.GlobalTransform, RgbaColor.FromBytes(0, 255, 160, 255));

        // Draw spectrogram-like bars at bottom
        var barCount = 64;
        var barWidth = (float)(bounds.Width / barCount);
        var barMaxHeight = 100f;

        for (int i = 0; i < barCount; i++)
        {
            var barHeight = (float)(barMaxHeight * (0.3 + 0.7 * Math.Abs(Math.Sin(time * 1.5 + i * 0.3))));
            var x = i * barWidth;
            var y = (float)(bounds.Height - barHeight - 40);

            var barPath = new PathBuilder()
                .MoveTo(x + 1, (float)bounds.Height - 40)
                .LineTo(x + barWidth - 1, (float)bounds.Height - 40)
                .LineTo(x + barWidth - 1, y)
                .LineTo(x + 1, y)
                .Close();

            var r = (byte)(100 + i * 2);
            var g = (byte)(50);
            var b = (byte)(200);

            scene.FillPath(barPath, FillRule.NonZero, e.GlobalTransform,
                RgbaColor.FromBytes(r, g, b, 200));
        }

        // Draw playhead line
        var playheadX = (float)((time * 50) % bounds.Width);
        var playheadPath = new PathBuilder()
            .MoveTo(playheadX, 0)
            .LineTo(playheadX, (float)bounds.Height);

        scene.StrokePath(playheadPath, new StrokeStyle { Width = 2f },
            e.GlobalTransform, RgbaColor.FromBytes(255, 100, 100, 180));
    }
}
