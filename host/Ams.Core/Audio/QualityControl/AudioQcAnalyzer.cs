using System.Globalization;
using Ams.Core.Processors;

namespace Ams.Core.Audio.QualityControl;

/// <summary>
/// Analyzes audiobook chapter audio files for QC using in-process FFmpeg (via AudioProcessor).
/// Pure analysis functions (AnalyzeStructure, FlagAnomalies)
/// are static and testable without FFmpeg. AnalyzeFile requires FFmpeg native libraries.
/// </summary>
public static class AudioQcAnalyzer
{
    /// <summary>
    /// Tolerance in seconds for considering a silence region as starting "near" 0.0
    /// or ending "near" the total file duration.
    /// </summary>
    private const double NearBoundaryTolerance = 0.05;

    /// <summary>
    /// Analyzes the structure of detected silence regions to identify head silence,
    /// title-body gap, and tail silence in audiobook chapter audio.
    /// </summary>
    /// <returns>
    /// A tuple of (HeadSilenceSec, TitleDurationSec, TitleBodyGapSec, TailSilenceSec).
    /// Title and gap are null when the structure cannot be identified (fewer than 3 silence regions
    /// after accounting for head and tail).
    /// </returns>
    public static (double HeadSilenceSec, double? TitleDurationSec, double? TitleBodyGapSec, double TailSilenceSec)
        AnalyzeStructure(IReadOnlyList<SilenceRegion> silences, double totalDurationSec)
    {
        if (silences.Count == 0)
            return (0.0, null, null, 0.0);

        double headSilenceSec = 0.0;
        double tailSilenceSec = 0.0;
        int headIndex = -1;
        int tailIndex = -1;

        // Identify head silence: first region starting near 0.0
        if (silences[0].Start < NearBoundaryTolerance)
        {
            headIndex = 0;
            headSilenceSec = silences[0].Duration >= 0
                ? silences[0].Duration
                : totalDurationSec - silences[0].Start;
        }

        // Identify tail silence: last region ending near totalDuration
        var lastRegion = silences[^1];
        var lastEnd = lastRegion.End < 0 ? totalDurationSec : lastRegion.End;
        if (lastEnd > totalDurationSec - NearBoundaryTolerance)
        {
            tailIndex = silences.Count - 1;
            tailSilenceSec = lastRegion.Duration >= 0
                ? lastRegion.Duration
                : totalDurationSec - lastRegion.Start;
        }

        // Title-body gap: the longest non-head, non-tail silence in the first portion
        // of the file (first 60s or 10% of duration, whichever is larger). The structural
        // gap between title narration and body text is typically the most prominent silence
        // near the start — much longer than inter-word pauses.
        double? titleDurationSec = null;
        double? titleBodyGapSec = null;

        var searchLimit = Math.Max(60.0, totalDurationSec * 0.10);
        int gapIndex = -1;
        double longestGapDuration = 0.0;
        for (int i = 0; i < silences.Count; i++)
        {
            if (i == headIndex || i == tailIndex) continue;
            if (silences[i].Start > searchLimit) break;
            var dur = silences[i].Duration >= 0
                ? silences[i].Duration
                : totalDurationSec - silences[i].Start;
            if (dur > longestGapDuration)
            {
                longestGapDuration = dur;
                gapIndex = i;
            }
        }

        if (gapIndex >= 0 && headIndex >= 0)
        {
            var gapRegion = silences[gapIndex];
            titleBodyGapSec = gapRegion.Duration >= 0
                ? gapRegion.Duration
                : totalDurationSec - gapRegion.Start;

            // Title duration: speech between end of head silence and start of gap
            var headEnd = silences[headIndex].End >= 0
                ? silences[headIndex].End
                : totalDurationSec;
            titleDurationSec = gapRegion.Start - headEnd;
        }

        return (headSilenceSec, titleDurationSec, titleBodyGapSec, tailSilenceSec);
    }

    /// <summary>
    /// Flags anomalies based on threshold comparison against the QC result.
    /// Returns formatted flag strings like "HEAD_SILENCE_SHORT (0.32s &lt; 0.50s min)".
    /// </summary>
    public static IReadOnlyList<string> FlagAnomalies(ChapterQcResult result, QcThresholds thresholds)
    {
        var flags = new List<string>();

        // Head silence checks
        if (result.HeadSilenceSec < thresholds.MinHeadSilence)
        {
            flags.Add(string.Format(CultureInfo.InvariantCulture,
                "HEAD_SILENCE_SHORT ({0:F2}s < {1:F2}s min)",
                result.HeadSilenceSec, thresholds.MinHeadSilence));
        }
        else if (result.HeadSilenceSec > thresholds.MaxHeadSilence)
        {
            flags.Add(string.Format(CultureInfo.InvariantCulture,
                "HEAD_SILENCE_LONG ({0:F2}s > {1:F2}s max)",
                result.HeadSilenceSec, thresholds.MaxHeadSilence));
        }

        // Tail silence checks
        if (result.TailSilenceSec < thresholds.MinTailSilence)
        {
            flags.Add(string.Format(CultureInfo.InvariantCulture,
                "TAIL_SILENCE_SHORT ({0:F2}s < {1:F2}s min)",
                result.TailSilenceSec, thresholds.MinTailSilence));
        }
        else if (result.TailSilenceSec > thresholds.MaxTailSilence)
        {
            flags.Add(string.Format(CultureInfo.InvariantCulture,
                "TAIL_SILENCE_LONG ({0:F2}s > {1:F2}s max)",
                result.TailSilenceSec, thresholds.MaxTailSilence));
        }

        // Title-body gap checks (only if gap is present)
        if (result.TitleBodyGapSec.HasValue)
        {
            if (result.TitleBodyGapSec.Value < thresholds.MinTitleBodyGap)
            {
                flags.Add(string.Format(CultureInfo.InvariantCulture,
                    "TITLE_GAP_SHORT ({0:F2}s < {1:F2}s min)",
                    result.TitleBodyGapSec.Value, thresholds.MinTitleBodyGap));
            }
            else if (result.TitleBodyGapSec.Value > thresholds.MaxTitleBodyGap)
            {
                flags.Add(string.Format(CultureInfo.InvariantCulture,
                    "TITLE_GAP_LONG ({0:F2}s > {1:F2}s max)",
                    result.TitleBodyGapSec.Value, thresholds.MaxTitleBodyGap));
            }
        }

        return flags;
    }

    /// <summary>
    /// Analyzes a single audio file for QC using in-process FFmpeg (AudioProcessor).
    /// </summary>
    public static ChapterQcResult AnalyzeFile(
        string filePath,
        double noiseDb,
        double minSilenceDurationSec,
        QcThresholds thresholds)
    {
        var fileName = Path.GetFileName(filePath);

        // Probe file metadata via in-process FFmpeg
        var info = AudioProcessor.Probe(filePath);
        var durationSec = info.Duration.TotalSeconds;

        // Decode and run silence detection via in-process libavfilter
        var buffer = AudioProcessor.Decode(filePath);
        var intervals = AudioProcessor.DetectSilence(buffer, new SilenceDetectOptions
        {
            NoiseDb = noiseDb,
            MinimumDuration = TimeSpan.FromSeconds(minSilenceDurationSec)
        });

        // Map SilenceInterval → SilenceRegion for structure analysis
        var silences = MapToRegions(intervals, durationSec);

        // Analyze structure
        var (headSilence, titleDuration, titleBodyGap, tailSilence) =
            AnalyzeStructure(silences, durationSec);

        var result = new ChapterQcResult
        {
            FileName = fileName,
            DurationSec = durationSec,
            Channels = info.Channels,
            SampleRate = info.SampleRate,
            HeadSilenceSec = headSilence,
            TitleDurationSec = titleDuration,
            TitleBodyGapSec = titleBodyGap,
            TailSilenceSec = tailSilence,
            AllSilences = silences
        };

        // Flag anomalies
        var flags = FlagAnomalies(result, thresholds);
        result = result with { Flags = flags };

        return result;
    }

    /// <summary>
    /// Maps <see cref="SilenceInterval"/> (from AudioProcessor.DetectSilence) to
    /// <see cref="SilenceRegion"/> (used by QC structure analysis).
    /// A trailing interval whose End matches the file duration is mapped with
    /// sentinel End/Duration = -1 to signal open-ended tail silence.
    /// </summary>
    private static SilenceRegion[] MapToRegions(IReadOnlyList<SilenceInterval> intervals, double totalDurationSec)
    {
        var regions = new SilenceRegion[intervals.Count];
        for (int i = 0; i < intervals.Count; i++)
        {
            var iv = intervals[i];
            regions[i] = new SilenceRegion(
                iv.Start.TotalSeconds,
                iv.End.TotalSeconds,
                iv.Duration.TotalSeconds);
        }
        return regions;
    }
}
