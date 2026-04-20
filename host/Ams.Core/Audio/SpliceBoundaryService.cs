using Ams.Core.Artifacts;
using Ams.Core.Processors;

namespace Ams.Core.Audio;

/// <summary>
/// Configurable thresholds for splice boundary refinement.
/// </summary>
public sealed record SpliceBoundaryOptions
{
    /// <summary>Silence detection threshold in dB (default -55 dB).</summary>
    public double SilenceThresholdDb { get; init; } = -55.0;

    /// <summary>Minimum silence duration to consider as a valid gap (default 50ms).</summary>
    public TimeSpan MinSilenceDuration { get; init; } = TimeSpan.FromMilliseconds(50);

    /// <summary>How far before/after the rough boundary to search for silence (default 0.5s).</summary>
    public double SearchMarginSec { get; init; } = 0.5;

    /// <summary>
    /// Maximum non-silent burst duration to ignore when stitching neighboring silences
    /// (click immunity). Bursts at or below this duration are treated as transient clicks.
    /// </summary>
    public double ClickImmunityBurstSec { get; init; } = 0.030;

    /// <summary>Padding added before the refined start boundary (default 15ms).</summary>
    public double StartPaddingSec { get; init; } = 0.015;

    /// <summary>Padding added after the refined end boundary (default 25ms).</summary>
    public double EndPaddingSec { get; init; } = 0.025;

    /// <summary>Margin to back off from a snap-to-energy edge (default 30ms).</summary>
    public double EnergyBackoffSec { get; init; } = 0.030;

    /// <summary>Whether to run breath detection on boundary regions (default true).</summary>
    public bool EnableBreathDetection { get; init; } = true;

    /// <summary>Search window radius around each boundary for breath detection (default 0.2s).</summary>
    public double BreathSearchRadiusSec { get; init; } = 0.2;

    /// <summary>Small gap (seconds) to leave between the breath edge and the cut point (default 5ms).</summary>
    public double BreathGuardSec { get; init; } = 0.005;
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
    Original,

    /// <summary>Boundary shifted to avoid bisecting a detected breath.</summary>
    BreathAware
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

        // Apply small safety padding around boundaries to avoid clipped consonants/fricatives.
        var minStartSec = Math.Max(0.0, prevSentenceEndSec ?? 0.0);
        var maxEndSec = Math.Min(bufferDurationSec, nextSentenceStartSec ?? bufferDurationSec);
        (refinedStart, refinedEnd) = ApplyBoundaryPadding(
            refinedStart,
            refinedEnd,
            minStartSec,
            maxEndSec,
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

    /// <summary>
    /// Refines boundaries with breath awareness: calls <see cref="RefineBoundaries"/> first,
    /// then checks for breath overlap at each refined boundary. If a proposed cut point
    /// bisects a detected breath, the cut is shifted to avoid splitting it.
    /// </summary>
    /// <param name="chapterBuffer">The full chapter audio buffer.</param>
    /// <param name="roughStartSec">ASR/MFA-derived start time of the sentence.</param>
    /// <param name="roughEndSec">ASR/MFA-derived end time of the sentence.</param>
    /// <param name="prevSentenceEndSec">End time of the previous sentence (null if first).</param>
    /// <param name="nextSentenceStartSec">Start time of the next sentence (null if last).</param>
    /// <param name="options">Optional refinement thresholds (includes breath detection settings).</param>
    /// <returns>Refined start/end boundaries with method annotations.</returns>
    public static SpliceBoundaryResult RefineBoundariesBreathAware(
        AudioBuffer chapterBuffer,
        double roughStartSec,
        double roughEndSec,
        double? prevSentenceEndSec,
        double? nextSentenceStartSec,
        SpliceBoundaryOptions? options = null)
    {
        ArgumentNullException.ThrowIfNull(chapterBuffer);

        var opts = options ?? DefaultOptions;

        // 1. Get the initial refinement from existing logic
        var baseResult = RefineBoundaries(
            chapterBuffer, roughStartSec, roughEndSec,
            prevSentenceEndSec, nextSentenceStartSec, opts);

        // If breath detection is disabled, return the base result as-is
        if (!opts.EnableBreathDetection)
            return baseResult;

        var bufferDurationSec = (double)chapterBuffer.Length / chapterBuffer.SampleRate;
        var breathOpts = new FrameBreathDetectorOptions();

        var refinedStart = baseResult.RefinedStartSec;
        var startMethod = baseResult.StartMethod;
        var refinedEnd = baseResult.RefinedEndSec;
        var endMethod = baseResult.EndMethod;

        // 2. Check for breath overlap at start boundary
        {
            double searchStart = Math.Max(0, refinedStart - opts.BreathSearchRadiusSec);
            double searchEnd = Math.Min(bufferDurationSec, refinedStart + opts.BreathSearchRadiusSec);

            if (searchEnd > searchStart)
            {
                var breaths = FeatureExtraction.Detect(chapterBuffer, searchStart, searchEnd, breathOpts);
                foreach (var breath in breaths)
                {
                    // If a breath straddles the proposed start cut point, shift after the breath
                    if (breath.StartSec <= refinedStart && refinedStart <= breath.EndSec)
                    {
                        refinedStart = breath.EndSec + opts.BreathGuardSec;
                        startMethod = BoundaryMethod.BreathAware;
                        break; // Only adjust for the first straddling breath
                    }
                }
            }
        }

        // 3. Check for breath overlap at end boundary
        {
            double searchStart = Math.Max(0, refinedEnd - opts.BreathSearchRadiusSec);
            double searchEnd = Math.Min(bufferDurationSec, refinedEnd + opts.BreathSearchRadiusSec);

            if (searchEnd > searchStart)
            {
                var breaths = FeatureExtraction.Detect(chapterBuffer, searchStart, searchEnd, breathOpts);
                foreach (var breath in breaths)
                {
                    // If a breath straddles the proposed end cut point, shift before the breath
                    if (breath.StartSec <= refinedEnd && refinedEnd <= breath.EndSec)
                    {
                        refinedEnd = breath.StartSec - opts.BreathGuardSec;
                        endMethod = BoundaryMethod.BreathAware;
                        break; // Only adjust for the first straddling breath
                    }
                }
            }
        }

        // 4. Re-apply boundary padding with breath-adjusted positions
        var minStartSec = Math.Max(0.0, prevSentenceEndSec ?? 0.0);
        var maxEndSec = Math.Min(bufferDurationSec, nextSentenceStartSec ?? bufferDurationSec);
        (refinedStart, refinedEnd) = ApplyBoundaryPadding(
            refinedStart, refinedEnd,
            minStartSec, maxEndSec,
            opts);

        // 5. Sanity: ensure start < end, fall back to originals if not
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

        var silences = MergeSilencesAcrossTransientBursts(
            AudioProcessor.DetectSilence(regionBuffer, silenceOptions),
            opts.ClickImmunityBurstSec);

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

    internal static (double StartSec, double EndSec) ApplyBoundaryPadding(
        double startSec,
        double endSec,
        double minStartSec,
        double maxEndSec,
        SpliceBoundaryOptions opts)
    {
        var paddedStart = Math.Max(minStartSec, startSec - Math.Max(0.0, opts.StartPaddingSec));
        var paddedEnd = Math.Min(maxEndSec, endSec + Math.Max(0.0, opts.EndPaddingSec));
        return (paddedStart, paddedEnd);
    }

    internal static IReadOnlyList<SilenceInterval> MergeSilencesAcrossTransientBursts(
        IReadOnlyList<SilenceInterval> silences,
        double maxBurstSec)
    {
        if (silences.Count < 2 || maxBurstSec <= 0)
        {
            return silences;
        }

        var merged = new List<SilenceInterval>(silences.Count);
        var currentStart = silences[0].Start;
        var currentEnd = silences[0].End;

        for (int i = 1; i < silences.Count; i++)
        {
            var next = silences[i];
            var burstDurationSec = (next.Start - currentEnd).TotalSeconds;

            if (burstDurationSec <= maxBurstSec)
            {
                currentEnd = next.End;
                continue;
            }

            merged.Add(new SilenceInterval(
                currentStart,
                currentEnd,
                currentEnd - currentStart));
            currentStart = next.Start;
            currentEnd = next.End;
        }

        merged.Add(new SilenceInterval(
            currentStart,
            currentEnd,
            currentEnd - currentStart));

        return merged;
    }
}
