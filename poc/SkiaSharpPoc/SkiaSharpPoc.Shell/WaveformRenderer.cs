using SkiaSharp;

namespace SkiaSharpPoc.Shell;

/// <summary>
/// Renders waveform visualization using SkiaSharp.
/// Supports efficient rendering of large audio files through downsampling.
/// </summary>
public sealed class WaveformRenderer
{
    // Colors
    private readonly SKColor _waveformColor = new(30, 144, 255); // Dodger Blue
    private readonly SKColor _centerLineColor = new(88, 91, 112); // Muted gray

    // Cached paint objects to avoid allocations in render loop
    private readonly SKPaint _waveformPaint;
    private readonly SKPaint _centerLinePaint;

    public WaveformRenderer()
    {
        _waveformPaint = new SKPaint
        {
            Color = _waveformColor,
            Style = SKPaintStyle.Fill,
            IsAntialias = true
        };

        _centerLinePaint = new SKPaint
        {
            Color = _centerLineColor,
            Style = SKPaintStyle.Stroke,
            StrokeWidth = 1,
            IsAntialias = false
        };
    }

    /// <summary>
    /// Renders the waveform to the canvas.
    /// </summary>
    /// <param name="canvas">The SkiaSharp canvas to draw on.</param>
    /// <param name="audioLoader">The audio data source.</param>
    /// <param name="width">Canvas width in pixels.</param>
    /// <param name="height">Canvas height in pixels.</param>
    /// <param name="startSample">First sample to render.</param>
    /// <param name="endSample">Last sample to render (exclusive).</param>
    /// <param name="offsetX">Horizontal pan offset in pixels.</param>
    /// <param name="scaleX">Horizontal zoom scale (1.0 = fit all).</param>
    public void Render(
        SKCanvas canvas,
        AudioLoader audioLoader,
        int width,
        int height,
        long startSample,
        long endSample,
        float offsetX,
        float scaleX)
    {
        var centerY = height / 2f;
        var amplitude = height / 2f * 0.9f; // Leave some margin

        // Draw center line
        canvas.DrawLine(0, centerY, width, centerY, _centerLinePaint);

        // Get min/max samples for visible range
        var (mins, maxs) = audioLoader.GetMinMaxSamples(startSample, endSample, width);

        if (mins.Length == 0)
            return;

        // Build path for waveform
        using var path = new SKPath();

        // Draw as filled polygon - top half
        path.MoveTo(0, centerY + mins[0] * amplitude);

        for (int i = 0; i < mins.Length; i++)
        {
            var x = i;
            path.LineTo(x, centerY - maxs[i] * amplitude);
        }

        // Bottom half (reverse)
        for (int i = mins.Length - 1; i >= 0; i--)
        {
            var x = i;
            path.LineTo(x, centerY - mins[i] * amplitude);
        }

        path.Close();
        canvas.DrawPath(path, _waveformPaint);
    }

    /// <summary>
    /// Disposes of cached resources.
    /// </summary>
    public void Dispose()
    {
        _waveformPaint.Dispose();
        _centerLinePaint.Dispose();
    }
}
