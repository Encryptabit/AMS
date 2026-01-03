using System.Numerics;
using VelloSharp;
using VelloSharp.Scenes;

namespace HybridVelloPoc.Shell;

/// <summary>
/// Renders waveform visualization using VelloSharp scene API.
/// Supports efficient rendering of large audio files through downsampling.
/// </summary>
public sealed class WaveformRenderer
{
    // Colors matching SkiaSharpPoc for visual parity
    private static readonly RgbaColor WaveformColor = RgbaColor.FromBytes(30, 144, 255, 255); // Dodger Blue
    private static readonly RgbaColor CenterLineColor = RgbaColor.FromBytes(88, 91, 112, 255); // Muted gray
    private static readonly RgbaColor BackgroundColor = RgbaColor.FromBytes(30, 30, 46, 255); // Dark background

    /// <summary>
    /// Renders the waveform to the Vello scene.
    /// </summary>
    /// <param name="scene">The Vello scene to draw to.</param>
    /// <param name="audioLoader">The audio data source.</param>
    /// <param name="width">Canvas width in pixels.</param>
    /// <param name="height">Canvas height in pixels.</param>
    /// <param name="startSample">First sample to render.</param>
    /// <param name="endSample">Last sample to render (exclusive).</param>
    /// <param name="transform">The view transform matrix.</param>
    public void Render(
        Scene scene,
        AudioLoader audioLoader,
        int width,
        int height,
        long startSample,
        long endSample,
        Matrix3x2 transform)
    {
        // Draw background
        DrawBackground(scene, width, height, transform);

        var centerY = height / 2f;
        var amplitude = height / 2f * 0.9f; // Leave some margin

        // Draw center line
        DrawCenterLine(scene, width, centerY, transform);

        // Get min/max samples for visible range
        var (mins, maxs) = audioLoader.GetMinMaxSamples(startSample, endSample, width);

        if (mins.Length == 0)
            return;

        // Draw waveform as filled polygon
        DrawWaveform(scene, mins, maxs, centerY, amplitude, transform);
    }

    private void DrawBackground(Scene scene, int width, int height, Matrix3x2 transform)
    {
        var bgPath = new PathBuilder()
            .MoveTo(0, 0)
            .LineTo(width, 0)
            .LineTo(width, height)
            .LineTo(0, height)
            .Close();

        scene.FillPath(bgPath, FillRule.NonZero, transform, BackgroundColor);
    }

    private void DrawCenterLine(Scene scene, int width, float centerY, Matrix3x2 transform)
    {
        var linePath = new PathBuilder()
            .MoveTo(0, centerY)
            .LineTo(width, centerY);

        var strokeStyle = new StrokeStyle { Width = 1.0f };
        scene.StrokePath(linePath, strokeStyle, transform, CenterLineColor);
    }

    private void DrawWaveform(Scene scene, float[] mins, float[] maxs, float centerY, float amplitude, Matrix3x2 transform)
    {
        // Build path for waveform - filled polygon matching SkiaSharp approach
        var pathBuilder = new PathBuilder();

        // Draw as filled polygon - top half
        pathBuilder.MoveTo(0, centerY + mins[0] * amplitude);

        for (int i = 0; i < mins.Length; i++)
        {
            var x = (float)i;
            pathBuilder.LineTo(x, centerY - maxs[i] * amplitude);
        }

        // Bottom half (reverse)
        for (int i = mins.Length - 1; i >= 0; i--)
        {
            var x = (float)i;
            pathBuilder.LineTo(x, centerY - mins[i] * amplitude);
        }

        pathBuilder.Close();

        scene.FillPath(pathBuilder, FillRule.NonZero, transform, WaveformColor);
    }

    /// <summary>
    /// Renders a timing overlay showing frame statistics.
    /// </summary>
    /// <param name="scene">The Vello scene to draw to.</param>
    /// <param name="width">Canvas width in pixels.</param>
    /// <param name="frameTimer">The frame timer with current stats.</param>
    /// <param name="transform">The view transform matrix.</param>
    public void RenderStatsOverlay(Scene scene, int width, FrameTimer frameTimer, Matrix3x2 transform)
    {
        // Draw stats background box
        const int boxWidth = 150;
        const int boxHeight = 80;
        var boxX = width - boxWidth - 10;
        const int boxY = 10;

        var bgPath = new PathBuilder()
            .MoveTo(boxX, boxY)
            .LineTo(boxX + boxWidth, boxY)
            .LineTo(boxX + boxWidth, boxY + boxHeight)
            .LineTo(boxX, boxY + boxHeight)
            .Close();

        var bgColor = RgbaColor.FromBytes(0, 0, 0, 180);
        scene.FillPath(bgPath, FillRule.NonZero, transform, bgColor);

        // Note: VelloSharp text rendering would require HarfBuzzSharp integration.
        // For the POC, the FPS is displayed in the WPF status bar instead.
    }
}
