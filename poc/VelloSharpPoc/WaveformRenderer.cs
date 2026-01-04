using System.Numerics;
using Avalonia;
using VelloSharp;

namespace VelloSharpPoc;

/// <summary>
/// Renders test patterns to evaluate VelloSharp GPU performance.
/// Generates animated sine waves, 1000+ line segments, and fill rectangles.
/// </summary>
public class WaveformRenderer
{
    private const int WaveformLineCount = 1000;
    private const int SpectrogramRectCount = 64;
    private const int SpectrogramBandCount = 32;

    public record RenderStats(int LineCount, int RectCount);

    /// <summary>
    /// Renders test patterns to the Vello scene.
    /// </summary>
    /// <param name="scene">The Vello scene to draw to.</param>
    /// <param name="bounds">The available drawing bounds.</param>
    /// <param name="transform">The global transform matrix.</param>
    /// <param name="time">Total elapsed time in seconds for animation.</param>
    /// <returns>Statistics about what was rendered.</returns>
    public RenderStats Render(Scene scene, Rect bounds, Matrix3x2 transform, double time)
    {
        int lineCount = 0;
        int rectCount = 0;

        // Draw dark background
        DrawBackground(scene, bounds, transform);

        // Draw animated waveform (main test - 1000+ line segments)
        lineCount += DrawWaveform(scene, bounds, transform, time);

        // Draw spectrogram-style rectangles
        rectCount += DrawSpectrogram(scene, bounds, transform, time);

        // Draw secondary waveform overlay
        lineCount += DrawSecondaryWaveform(scene, bounds, transform, time);

        return new RenderStats(lineCount, rectCount);
    }

    private void DrawBackground(Scene scene, Rect bounds, Matrix3x2 transform)
    {
        var bgPath = new PathBuilder()
            .MoveTo((float)bounds.Left, (float)bounds.Top)
            .LineTo((float)bounds.Right, (float)bounds.Top)
            .LineTo((float)bounds.Right, (float)bounds.Bottom)
            .LineTo((float)bounds.Left, (float)bounds.Bottom)
            .Close();

        var bgColor = RgbaColor.FromBytes(18, 18, 24, 255);
        scene.FillPath(bgPath, FillRule.NonZero, transform, bgColor);
    }

    private int DrawWaveform(Scene scene, Rect bounds, Matrix3x2 transform, double time)
    {
        // Main waveform: 1000+ line segments animated sine wave
        float waveHeight = (float)bounds.Height * 0.3f;
        float centerY = (float)bounds.Height * 0.35f;
        float width = (float)bounds.Width;

        var waveformPath = new PathBuilder();
        bool first = true;

        for (int i = 0; i <= WaveformLineCount; i++)
        {
            float x = (float)bounds.Left + (i / (float)WaveformLineCount) * width;

            // Composite sine wave with multiple frequencies for more realistic audio look
            float phase = (float)(time * 2.0);
            float t = i / (float)WaveformLineCount;

            float y = centerY +
                MathF.Sin(t * 20 + phase) * waveHeight * 0.5f +
                MathF.Sin(t * 47 + phase * 1.3f) * waveHeight * 0.25f +
                MathF.Sin(t * 89 + phase * 0.7f) * waveHeight * 0.15f +
                MathF.Sin(t * 137 + phase * 2.1f) * waveHeight * 0.1f;

            if (first)
            {
                waveformPath.MoveTo(x, y);
                first = false;
            }
            else
            {
                waveformPath.LineTo(x, y);
            }
        }

        // Create gradient brush for waveform
        var waveformBrush = new LinearGradientBrush(
            new Vector2((float)bounds.Left, centerY - waveHeight),
            new Vector2((float)bounds.Left, centerY + waveHeight),
            new[]
            {
                new GradientStop(0f, RgbaColor.FromBytes(0, 255, 128, 255)),
                new GradientStop(0.5f, RgbaColor.FromBytes(0, 200, 255, 255)),
                new GradientStop(1f, RgbaColor.FromBytes(128, 0, 255, 255))
            });

        var strokeStyle = new StrokeStyle { Width = 2.0f };
        scene.StrokePath(waveformPath, strokeStyle, transform, waveformBrush);

        return WaveformLineCount;
    }

    private int DrawSecondaryWaveform(Scene scene, Rect bounds, Matrix3x2 transform, double time)
    {
        // Secondary waveform: different phase and position
        float waveHeight = (float)bounds.Height * 0.15f;
        float centerY = (float)bounds.Height * 0.75f;
        float width = (float)bounds.Width;

        var waveformPath = new PathBuilder();
        bool first = true;
        int lineCount = 500;

        for (int i = 0; i <= lineCount; i++)
        {
            float x = (float)bounds.Left + (i / (float)lineCount) * width;
            float phase = (float)(time * 3.0);
            float t = i / (float)lineCount;

            float y = centerY +
                MathF.Sin(t * 31 + phase) * waveHeight * 0.6f +
                MathF.Sin(t * 67 + phase * 1.5f) * waveHeight * 0.3f +
                MathF.Sin(t * 113 + phase * 0.9f) * waveHeight * 0.1f;

            if (first)
            {
                waveformPath.MoveTo(x, y);
                first = false;
            }
            else
            {
                waveformPath.LineTo(x, y);
            }
        }

        var color = RgbaColor.FromBytes(255, 100, 50, 180);
        var strokeStyle = new StrokeStyle { Width = 1.5f };
        scene.StrokePath(waveformPath, strokeStyle, transform, color);

        return lineCount;
    }

    private int DrawSpectrogram(Scene scene, Rect bounds, Matrix3x2 transform, double time)
    {
        // Spectrogram-style visualization (like frequency bands)
        float spectrogramHeight = (float)bounds.Height * 0.2f;
        float spectrogramTop = (float)bounds.Height * 0.55f;
        float width = (float)bounds.Width;
        float bandWidth = width / SpectrogramRectCount;

        int rectCount = 0;

        for (int i = 0; i < SpectrogramRectCount; i++)
        {
            float x = (float)bounds.Left + i * bandWidth;
            float phase = (float)(time * 2.5);

            // Simulate frequency band levels with multiple sine components
            float level =
                0.3f + 0.3f * MathF.Sin(i * 0.3f + phase) +
                0.2f * MathF.Sin(i * 0.7f + phase * 1.3f) +
                0.1f * MathF.Sin(i * 1.1f + phase * 0.6f);

            level = Math.Clamp(level, 0.05f, 1.0f);
            float barHeight = level * spectrogramHeight;

            var rectPath = new PathBuilder()
                .MoveTo(x + 1, spectrogramTop + spectrogramHeight - barHeight)
                .LineTo(x + bandWidth - 2, spectrogramTop + spectrogramHeight - barHeight)
                .LineTo(x + bandWidth - 2, spectrogramTop + spectrogramHeight)
                .LineTo(x + 1, spectrogramTop + spectrogramHeight)
                .Close();

            // Color based on frequency band and level
            byte r = (byte)(100 + 155 * (i / (float)SpectrogramRectCount));
            byte g = (byte)(255 * level);
            byte b = (byte)(200 - 100 * (i / (float)SpectrogramRectCount));

            var color = RgbaColor.FromBytes(r, g, b, 220);
            scene.FillPath(rectPath, FillRule.NonZero, transform, color);
            rectCount++;
        }

        return rectCount;
    }
}
