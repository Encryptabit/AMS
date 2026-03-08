using Ams.Core.Artifacts;

namespace Ams.Core.Audio;

/// <summary>
/// Splits an AudioBuffer at natural silence boundaries using a single O(n)
/// sliding-RMS pass. Split points land at the midpoint of each qualifying
/// silence region for clean ASR chunk boundaries.
/// </summary>
public static class SilenceChunker
{
    /// <summary>
    /// Represents a contiguous chunk of audio within a buffer.
    /// </summary>
    public readonly record struct ChunkBoundary(int StartSample, int Length);

    /// <summary>
    /// RMS window size in samples. 1024 samples at 16kHz = 64ms.
    /// Provides noise resilience vs per-sample comparison.
    /// </summary>
    private const int RmsWindowSize = 1024;

    /// <summary>
    /// Hop size for the sliding RMS window. 512 samples = 50% overlap.
    /// </summary>
    private const int RmsHopSize = 512;

    /// <summary>
    /// Default minimum chunk duration. Prevents excessive fragmentation
    /// on audiobooks with frequent pauses.
    /// </summary>
    private static readonly TimeSpan DefaultMinChunkDuration = TimeSpan.FromSeconds(15);

    /// <summary>
    /// Returns chunk boundaries for splitting an AudioBuffer at silence points.
    /// Uses a sliding RMS window to detect silence regions, then splits at their midpoints.
    /// Operates on channel 0 only (buffer is mono by the time it reaches ASR).
    /// </summary>
    /// <param name="buffer">The audio buffer to analyze.</param>
    /// <param name="silenceThresholdDb">Silence threshold in dB (default: AudioDefaults.SilenceThresholdDb).</param>
    /// <param name="minSilenceDuration">Minimum silence duration to qualify as a split point (default: AudioDefaults.MinimumSilenceDuration).</param>
    /// <param name="minChunkDuration">Minimum chunk duration to prevent excessive fragmentation (default: 30 seconds).</param>
    /// <returns>Contiguous chunk boundaries covering the entire buffer with no gaps.</returns>
    public static IReadOnlyList<ChunkBoundary> FindChunkBoundaries(
        AudioBuffer buffer,
        double silenceThresholdDb = AudioDefaults.SilenceThresholdDb,
        TimeSpan? minSilenceDuration = null,
        TimeSpan? minChunkDuration = null)
    {
        ArgumentNullException.ThrowIfNull(buffer);

        var effectiveMinSilence = minSilenceDuration ?? AudioDefaults.MinimumSilenceDuration;
        var effectiveMinChunk = minChunkDuration ?? DefaultMinChunkDuration;

        var minSilenceSamples = (int)(effectiveMinSilence.TotalSeconds * buffer.SampleRate);
        var minChunkSamples = (int)(effectiveMinChunk.TotalSeconds * buffer.SampleRate);

        // If buffer is shorter than minChunkDuration, return single chunk
        if (buffer.Length <= minChunkSamples)
        {
            return [new ChunkBoundary(0, buffer.Length)];
        }

        // Convert dB threshold to linear RMS amplitude
        var threshold = Math.Pow(10, silenceThresholdDb / 20.0);

        // Detect silence regions via single O(n) pass with sliding RMS window
        var silenceRegions = DetectSilenceRegions(buffer, threshold, minSilenceSamples);

        if (silenceRegions.Count == 0)
        {
            return [new ChunkBoundary(0, buffer.Length)];
        }

        // Extract midpoints as split candidates.
        // Skip regions that span the entire buffer (all-silence) or sit at the
        // very start/end with no audio on one side -- those aren't boundaries
        // between audio segments.
        var splitCandidates = new List<int>(silenceRegions.Count);
        foreach (var (start, length) in silenceRegions)
        {
            var midpoint = start + length / 2;
            // Skip if this silence region covers the full buffer
            if (start == 0 && start + length >= buffer.Length)
                continue;
            // Skip midpoints at the very edges (nothing useful to split)
            if (midpoint <= 0 || midpoint >= buffer.Length)
                continue;
            splitCandidates.Add(midpoint);
        }

        // Greedily select boundaries that respect minChunkDuration
        var selectedSplits = SelectSplitPoints(splitCandidates, buffer.Length, minChunkSamples);

        // Build contiguous chunk boundaries from selected split points
        return BuildChunkBoundaries(selectedSplits, buffer.Length);
    }

    /// <summary>
    /// Single O(n) pass: slides an RMS window across channel 0, tracking silence regions.
    /// </summary>
    private static List<(int Start, int Length)> DetectSilenceRegions(
        AudioBuffer buffer,
        double threshold,
        int minSilenceSamples)
    {
        var regions = new List<(int Start, int Length)>();
        var samples = buffer.GetChannel(0).Span;
        var totalSamples = buffer.Length;

        // Handle edge case: buffer smaller than RMS window
        if (totalSamples < RmsWindowSize)
        {
            // Compute single RMS for the entire buffer
            var rms = ComputeRms(samples, 0, totalSamples);
            if (rms < threshold && totalSamples >= minSilenceSamples)
            {
                regions.Add((0, totalSamples));
            }
            return regions;
        }

        int silenceStart = -1; // -1 means not currently in a silence region

        for (int pos = 0; pos + RmsWindowSize <= totalSamples; pos += RmsHopSize)
        {
            var rms = ComputeRms(samples, pos, RmsWindowSize);

            if (rms < threshold)
            {
                // In silence
                if (silenceStart == -1)
                {
                    silenceStart = pos;
                }
            }
            else
            {
                // Exiting silence
                if (silenceStart >= 0)
                {
                    var silenceLength = pos - silenceStart;
                    if (silenceLength >= minSilenceSamples)
                    {
                        regions.Add((silenceStart, silenceLength));
                    }
                    silenceStart = -1;
                }
            }
        }

        // Handle trailing silence (silence that extends to end of buffer)
        if (silenceStart >= 0)
        {
            var silenceLength = totalSamples - silenceStart;
            if (silenceLength >= minSilenceSamples)
            {
                regions.Add((silenceStart, silenceLength));
            }
        }

        return regions;
    }

    /// <summary>
    /// Computes RMS amplitude for a window of samples.
    /// </summary>
    private static double ComputeRms(ReadOnlySpan<float> samples, int offset, int length)
    {
        double sumSquares = 0;
        var end = offset + length;
        for (int i = offset; i < end; i++)
        {
            double s = samples[i];
            sumSquares += s * s;
        }
        return Math.Sqrt(sumSquares / length);
    }

    /// <summary>
    /// Greedily selects split points that keep chunks at or above minChunkSamples.
    /// Iterates through candidates in order, only accepting a split if both the
    /// preceding chunk and the remaining buffer would be large enough.
    /// </summary>
    private static List<int> SelectSplitPoints(
        List<int> candidates,
        int totalLength,
        int minChunkSamples)
    {
        var selected = new List<int>();
        int lastSplit = 0;

        foreach (var candidate in candidates)
        {
            var chunkBefore = candidate - lastSplit;
            var remaining = totalLength - candidate;

            if (chunkBefore >= minChunkSamples && remaining >= minChunkSamples)
            {
                selected.Add(candidate);
                lastSplit = candidate;
            }
        }

        return selected;
    }

    /// <summary>
    /// Converts a sorted list of split points into contiguous chunk boundaries.
    /// </summary>
    private static IReadOnlyList<ChunkBoundary> BuildChunkBoundaries(
        List<int> splitPoints,
        int totalLength)
    {
        if (splitPoints.Count == 0)
        {
            return [new ChunkBoundary(0, totalLength)];
        }

        var chunks = new List<ChunkBoundary>(splitPoints.Count + 1);
        int start = 0;

        foreach (var split in splitPoints)
        {
            chunks.Add(new ChunkBoundary(start, split - start));
            start = split;
        }

        // Final chunk from last split to end
        chunks.Add(new ChunkBoundary(start, totalLength - start));

        return chunks;
    }
}
