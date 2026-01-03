using System.Diagnostics;

namespace SkiaSharpPoc.Shell;

/// <summary>
/// Tracks frame timing for performance monitoring.
/// Provides rolling FPS calculation and min/max/avg frame times.
/// </summary>
public sealed class FrameTimer
{
    private const int MaxFrameSamples = 60;

    private readonly Stopwatch _stopwatch = new();
    private readonly Queue<double> _frameTimes = new();
    private double _frameTimeSum;

    private double _currentFrameTime;
    private double _minFrameTime = double.MaxValue;
    private double _maxFrameTime = double.MinValue;

    /// <summary>
    /// Gets the current FPS based on rolling average.
    /// </summary>
    public double CurrentFps
    {
        get
        {
            if (_frameTimes.Count == 0)
                return 0;

            var avgMs = _frameTimeSum / _frameTimes.Count;
            return avgMs > 0 ? 1000.0 / avgMs : 0;
        }
    }

    /// <summary>
    /// Gets the average frame time in milliseconds.
    /// </summary>
    public double AvgFrameTimeMs => _frameTimes.Count > 0 ? _frameTimeSum / _frameTimes.Count : 0;

    /// <summary>
    /// Gets the minimum frame time in milliseconds (over sample window).
    /// </summary>
    public double MinFrameTimeMs => _minFrameTime == double.MaxValue ? 0 : _minFrameTime;

    /// <summary>
    /// Gets the maximum frame time in milliseconds (over sample window).
    /// </summary>
    public double MaxFrameTimeMs => _maxFrameTime == double.MinValue ? 0 : _maxFrameTime;

    /// <summary>
    /// Call at the start of each frame render.
    /// </summary>
    public void BeginFrame()
    {
        _stopwatch.Restart();
    }

    /// <summary>
    /// Call at the end of each frame render.
    /// </summary>
    public void EndFrame()
    {
        _stopwatch.Stop();
        _currentFrameTime = _stopwatch.Elapsed.TotalMilliseconds;

        // Add to rolling average
        _frameTimes.Enqueue(_currentFrameTime);
        _frameTimeSum += _currentFrameTime;

        while (_frameTimes.Count > MaxFrameSamples)
        {
            _frameTimeSum -= _frameTimes.Dequeue();
        }

        // Update min/max (recalculate over current window)
        _minFrameTime = double.MaxValue;
        _maxFrameTime = double.MinValue;

        foreach (var time in _frameTimes)
        {
            if (time < _minFrameTime) _minFrameTime = time;
            if (time > _maxFrameTime) _maxFrameTime = time;
        }
    }

    /// <summary>
    /// Resets all timing statistics.
    /// </summary>
    public void Reset()
    {
        _frameTimes.Clear();
        _frameTimeSum = 0;
        _minFrameTime = double.MaxValue;
        _maxFrameTime = double.MinValue;
    }
}
