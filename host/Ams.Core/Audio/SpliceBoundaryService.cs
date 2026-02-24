using Ams.Core.Artifacts;
using Ams.Core.Processors;

namespace Ams.Core.Audio;

/// <summary>
/// Configurable thresholds for splice boundary refinement.
/// </summary>
public sealed record SpliceBoundaryOptions
{
    /// <summary>Silence detection threshold in dB (default -50 dB, below breaths).</summary>
    public double SilenceThresholdDb { get; init; } = -50.0;

    /// <summary>Minimum silence duration to consider as a valid gap (default 80ms).</summary>
    public TimeSpan MinSilenceDuration { get; init; } = TimeSpan.FromMilliseconds(80);

    /// <summary>How far before/after the rough boundary to search for silence (default 0.5s).</summary>
    public double SearchMarginSec { get; init; } = 0.5;

    /// <summary>Margin to back off from a snap-to-energy edge (default 30ms).</summary>
    public double EnergyBackoffSec { get; init; } = 0.030;
}

/// <summary>
/// Describes how a single boundary (start or end) was refined.
/// </summary>
public enum BoundaryMethod
{
    /// <summary>Cut placed at center of a detected silence interval.</summary>
    SilenceCenter,

    /// <summary>Boundary snapped to energy edge with small backoff.</summary>
    SnapEnergy,

    /// <summary>No refinement possible; original boundary used as-is.</summary>
    Original
}

/// <summary>
/// Result of refining splice boundaries for a sentence replacement.
/// </summary>
public sealed record SpliceBoundaryResult(
    double RefinedStartSec,
    double RefinedEndSec,
    BoundaryMethod StartMethod,
    BoundaryMethod EndMethod,
    double OriginalStartSec,
    double OriginalEndSec);

/// <summary>
/// Pure-function service that refines rough sentence boundaries using silence detection
/// and energy snapping. Sits alongside <see cref="AudioSpliceService"/> in the audio layer.
/// </summary>
public static class SpliceBoundaryService
{
    private static readonly SpliceBoundaryOptions DefaultOptions = new();

    /// <summary>
    /// Refines rough sentence boundaries by finding natural silence gaps near
    /// the start and end of the target sentence.
    /// </summary>
    /// <param name="chapterBuffer">The full chapter audio buffer.</param>
    /// <param name="roughStartSec">ASR/MFA-derived start time of the sentence.</param>
    /// <param name="roughEndSec">ASR/MFA-derived end time of the sentence.</param>
    /// <param name="prevSentenceEndSec">End time of the previous sentence (null if first).</param>
    /// <param name="nextSentenceStartSec">Start time of the next sentence (null if last).</param>
    /// <param name="options">Optional refinement thresholds.</param>
    /// <returns>Refined start/end boundaries with method annotations.</returns>
    public static SpliceBoundaryResult RefineBoundaries(
        AudioBuffer chapterBuffer,
        double roughStartSec,
        double roughEndSec,
        double? prevSentenceEndSec,
        double? nextSentenceStartSec,
        SpliceBoundaryOptions? options = null)
    {
        ArgumentNullException.ThrowIfNull(chapterBuffer);

        var opts = options ?? DefaultOptions;
        var bufferDurationSec = (double)chapterBuffer.Length / chapterBuffer.SampleRate;

        // Refine start boundary
        var (refinedStart, startMethod) = RefineBoundary(
            chapterBuffer, bufferDurationSec, roughStartSec,
            searchLeftSec: prevSentenceEndSec ?? Math.Max(0, roughStartSec - opts.SearchMarginSec),
            searchRightSec: roughStartSec + 0.1,
            isStartBoundary: true,
            opts);

        // Refine end boundary
        var (refinedEnd, endMethod) = RefineBoundary(
            chapterBuffer, bufferDurationSec, roughEndSec,
            searchLeftSec: roughEndSec - 0.1,
            searchRightSec: nextSentenceStartSec ?? Math.Min(bufferDurationSec, roughEndSec + opts.SearchMarginSec),
            isStartBoundary: false,
            opts);

        // Sanity: ensure start < end
        if (refinedStart >= refinedEnd)
        {
            refinedStart = roughStartSec;
            refinedEnd = roughEndSec;
            startMethod = BoundaryMethod.Original;
            endMethod = BoundaryMethod.Original;
        }

        return new SpliceBoundaryResult(
            refinedStart, refinedEnd,
            startMethod, endMethod,
            roughStartSec, roughEndSec);
    }

    private static (double position, BoundaryMethod method) RefineBoundary(
        AudioBuffer buffer,
        double bufferDurationSec,
        double roughSec,
        double searchLeftSec,
        double searchRightSec,
        bool isStartBoundary,
        SpliceBoundaryOptions opts)
    {
        // Clamp search window to buffer
        searchLeftSec = Math.Max(0, searchLeftSec);
        searchRightSec = Math.Min(bufferDurationSec, searchRightSec);

        if (searchRightSec <= searchLeftSec)
            return (roughSec, BoundaryMethod.Original);

        // 1. Trim the search region from the chapter buffer
        var regionBuffer = AudioProcessor.Trim(
            buffer,
            TimeSpan.FromSeconds(searchLeftSec),
            TimeSpan.FromSeconds(searchRightSec));

        if (regionBuffer.Length == 0)
            return (roughSec, BoundaryMethod.Original);

        // 2. Run silence detection with lower thresholds (below breaths)
        var silenceOptions = new SilenceDetectOptions
        {
            NoiseDb = opts.SilenceThresholdDb,
            MinimumDuration = opts.MinSilenceDuration
        };

        var silences = AudioProcessor.DetectSilence(regionBuffer, silenceOptions);

        if (silences.Count > 0)
        {
            // 3. Find the silence interval closest to the rough boundary
            // (positions in silences are relative to the trimmed region)
            double roughRelative = roughSec - searchLeftSec;
            SilenceInterval? closest = null;
            double closestDist = double.MaxValue;

            foreach (var si in silences)
            {
                double center = (si.Start.TotalSeconds + si.End.TotalSeconds) / 2.0;
                double dist = Math.Abs(center - roughRelative);
                if (dist < closestDist)
                {
                    closestDist = dist;
                    closest = si;
                }
            }

            if (closest is not null)
            {
                // 4. Place cut at the CENTER of the silence
                double centerRelative = (closest.Start.TotalSeconds + closest.End.TotalSeconds) / 2.0;
                double centerAbsolute = searchLeftSec + centerRelative;
                return (centerAbsolute, BoundaryMethod.SilenceCenter);
            }
        }

        // 5. Fallback: snap to energy edge and back off
        var seed = new TimingRange(roughSec, roughSec);
        var snapped = AudioProcessor.SnapToEnergy(
            buffer, seed,
            searchWindowSec: searchRightSec - searchLeftSec);

        // For start boundaries, use the start edge (where speech begins);
        // for end boundaries, use the end edge (where speech ends).
        double snappedPos = isStartBoundary ? snapped.StartSec : snapped.EndSec;

        // Back off from the speech edge
        double backed = isStartBoundary
            ? snappedPos - opts.EnergyBackoffSec
            : snappedPos + opts.EnergyBackoffSec;

        // Only use the snapped result if it differs meaningfully from rough
        if (Math.Abs(backed - roughSec) > 0.005)
        {
            backed = Math.Clamp(backed, searchLeftSec, searchRightSec);
            return (backed, BoundaryMethod.SnapEnergy);
        }

        // 6. Final fallback: use original
        return (roughSec, BoundaryMethod.Original);
    }
}
