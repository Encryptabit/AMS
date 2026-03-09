using System.Globalization;
using System.Text.RegularExpressions;
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
    private const double DefaultMinimumStructuralGapDuration = 0.30;
    private static readonly Regex DecoratedChapterTitlePattern = new(
        @"^\s*chapter\b.+?[:\-–—]\s*.+\s*$",
        RegexOptions.Compiled | RegexOptions.IgnoreCase);
    private static readonly Regex DecoratedChapterTitlePartsPattern = new(
        @"^\s*(chapter\b.+?)\s*[:\-–—]\s*(.+?)\s*$",
        RegexOptions.Compiled | RegexOptions.IgnoreCase);

    /// <summary>
    /// Analyzes the structure of detected silence regions to identify head silence,
    /// title-body gap, and tail silence in audiobook chapter audio.
    /// </summary>
    /// <returns>
    /// A tuple of (HeadSilenceSec, TitleDurationSec, TitleBodyGapSec, TailSilenceSec).
    /// Title and gap are null when the structure cannot be identified (fewer than 3 silence regions
    /// after accounting for head and tail).
    /// </returns>
    public static (
        double HeadSilenceSec,
        double? TitleDurationSec,
        double? DecoratorGapSec,
        double? TitleBodyGapSec,
        double TailSilenceSec)
        AnalyzeStructure(
            IReadOnlyList<SilenceRegion> silences,
            double totalDurationSec,
            double minimumStructuralGapDurationSec = DefaultMinimumStructuralGapDuration,
            bool expectDecorator = false,
            double minimumDecoratorSpeechDurationSec = 0.0,
            double minimumTitleSpeechDurationSec = 0.0)
    {
        if (silences.Count == 0)
            return (0.0, null, null, null, 0.0);

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

        // Title-body gap: prefer the first plausible structural gap near the start of the
        // file, rather than the longest pause in the opening minutes. This avoids treating
        // an early paragraph pause as the title/body break on long chapters.
        double? titleDurationSec = null;
        double? decoratorGapSec = null;
        double? titleBodyGapSec = null;

        var searchLimit = Math.Max(60.0, totalDurationSec * 0.10);
        var candidateGapIndices = new List<int>(2);
        for (int i = 0; i < silences.Count; i++)
        {
            if (i == headIndex || i == tailIndex) continue;
            if (silences[i].Start > searchLimit) break;

            var dur = silences[i].Duration >= 0
                ? silences[i].Duration
                : totalDurationSec - silences[i].Start;
            if (dur >= minimumStructuralGapDurationSec)
            {
                candidateGapIndices.Add(i);
            }
        }

        // Fall back to the largest early gap when no structural candidate clears the
        // minimum duration, so very short title/body gaps can still be surfaced.
        double longestGapDuration = 0.0;
        int fallbackGapIndex = -1;
        for (int i = 0; candidateGapIndices.Count == 0 && i < silences.Count; i++)
        {
            if (i == headIndex || i == tailIndex) continue;
            if (silences[i].Start > searchLimit) break;
            var dur = silences[i].Duration >= 0
                ? silences[i].Duration
                : totalDurationSec - silences[i].Start;
            if (dur > longestGapDuration)
            {
                longestGapDuration = dur;
                fallbackGapIndex = i;
            }
        }

        var headEnd = headIndex >= 0
            ? (silences[headIndex].End >= 0 ? silences[headIndex].End : totalDurationSec)
            : 0.0;

        int gapIndex = -1;
        if (expectDecorator && candidateGapIndices.Count > 0)
        {
            var decoratorGapIndex = FindGapIndexMeetingMinimumSpeech(
                silences,
                candidateGapIndices,
                headEnd,
                minimumDecoratorSpeechDurationSec);
            decoratorGapIndex = decoratorGapIndex >= 0 ? decoratorGapIndex : candidateGapIndices[0];

            var decoratorGapEnd = silences[decoratorGapIndex].End >= 0
                ? silences[decoratorGapIndex].End
                : totalDurationSec;
            gapIndex = FindSecondaryGapIndex(
                silences,
                decoratorGapIndex,
                tailIndex,
                decoratorGapEnd,
                totalDurationSec,
                searchLimit,
                minimumStructuralGapDurationSec,
                minimumTitleSpeechDurationSec);

            if (gapIndex > decoratorGapIndex)
            {
                decoratorGapSec = silences[decoratorGapIndex].Duration >= 0
                    ? silences[decoratorGapIndex].Duration
                    : totalDurationSec - silences[decoratorGapIndex].Start;
            }
            else
            {
                gapIndex = decoratorGapIndex;
            }
        }
        else if (candidateGapIndices.Count > 0)
        {
            gapIndex = FindGapIndexMeetingMinimumSpeech(
                silences,
                candidateGapIndices,
                headEnd,
                minimumTitleSpeechDurationSec);
            gapIndex = gapIndex >= 0 ? gapIndex : candidateGapIndices[0];
        }
        else
        {
            gapIndex = fallbackGapIndex;
        }

        if (gapIndex >= 0 && headIndex >= 0)
        {
            var gapRegion = silences[gapIndex];
            titleBodyGapSec = gapRegion.Duration >= 0
                ? gapRegion.Duration
                : totalDurationSec - gapRegion.Start;

            // Title duration: speech between end of head silence and start of gap
            titleDurationSec = gapRegion.Start - headEnd;
        }

        return (headSilenceSec, titleDurationSec, decoratorGapSec, titleBodyGapSec, tailSilenceSec);
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
        QcThresholds thresholds,
        string? sectionTitle = null)
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
        var (headSilence, titleDuration, decoratorGap, titleBodyGap, tailSilence) =
            AnalyzeStructure(
                silences,
                durationSec,
                Math.Max(minSilenceDurationSec * 4.0, thresholds.MinTitleBodyGap * 0.5),
                HasDecorator(sectionTitle),
                GetMinimumDecoratorSpeechDurationSec(sectionTitle),
                GetMinimumTitleSpeechDurationSec(sectionTitle));

        var result = new ChapterQcResult
        {
            FileName = fileName,
            DurationSec = durationSec,
            Channels = info.Channels,
            SampleRate = info.SampleRate,
            HeadSilenceSec = headSilence,
            TitleDurationSec = titleDuration,
            DecoratorGapSec = decoratorGap,
            TitleBodyGapSec = titleBodyGap,
            TailSilenceSec = tailSilence,
            AllSilences = silences
        };

        // Flag anomalies
        var flags = FlagAnomalies(result, thresholds);
        result = result with { Flags = flags };

        return result;
    }

    private static bool HasDecorator(string? sectionTitle)
        => !string.IsNullOrWhiteSpace(sectionTitle) &&
           DecoratedChapterTitlePattern.IsMatch(sectionTitle);

    private static int FindSecondaryGapIndex(
        IReadOnlyList<SilenceRegion> silences,
        int decoratorGapIndex,
        int tailIndex,
        double decoratorGapEnd,
        double totalDurationSec,
        double searchLimit,
        double minimumStructuralGapDurationSec,
        double minimumTitleSpeechDurationSec)
    {
        int fallbackQualifiedIndex = -1;
        double fallbackQualifiedDuration = 0.0;
        int fallbackAnyIndex = -1;
        double fallbackAnyDuration = 0.0;

        for (int i = decoratorGapIndex + 1; i < silences.Count; i++)
        {
            if (i == tailIndex) continue;
            if (silences[i].Start > searchLimit) break;

            var dur = silences[i].Duration >= 0
                ? silences[i].Duration
                : totalDurationSec - silences[i].Start;

            if (dur > fallbackAnyDuration)
            {
                fallbackAnyDuration = dur;
                fallbackAnyIndex = i;
            }

            if (silences[i].Start - decoratorGapEnd < minimumTitleSpeechDurationSec)
            {
                continue;
            }

            if (dur >= minimumStructuralGapDurationSec)
            {
                return i;
            }

            if (dur > fallbackQualifiedDuration)
            {
                fallbackQualifiedDuration = dur;
                fallbackQualifiedIndex = i;
            }
        }

        return fallbackQualifiedIndex >= 0 ? fallbackQualifiedIndex : fallbackAnyIndex;
    }

    private static int FindGapIndexMeetingMinimumSpeech(
        IReadOnlyList<SilenceRegion> silences,
        IReadOnlyList<int> candidateGapIndices,
        double speechStartSec,
        double minimumSpeechDurationSec)
    {
        if (minimumSpeechDurationSec <= 0)
        {
            return candidateGapIndices.Count > 0 ? candidateGapIndices[0] : -1;
        }

        foreach (var gapIndex in candidateGapIndices)
        {
            if (silences[gapIndex].Start - speechStartSec >= minimumSpeechDurationSec)
            {
                return gapIndex;
            }
        }

        return -1;
    }

    private static double GetMinimumDecoratorSpeechDurationSec(string? sectionTitle)
    {
        if (!TrySplitDecoratorTitle(sectionTitle, out var decoratorText, out _))
        {
            return 0.0;
        }

        return EstimateMinimumSpeechDurationSec(CountWords(decoratorText));
    }

    private static double GetMinimumTitleSpeechDurationSec(string? sectionTitle)
    {
        if (TrySplitDecoratorTitle(sectionTitle, out _, out var titleText))
        {
            return EstimateMinimumSpeechDurationSec(CountWords(titleText));
        }

        return EstimateMinimumSpeechDurationSec(CountWords(sectionTitle));
    }

    private static bool TrySplitDecoratorTitle(string? sectionTitle, out string decoratorText, out string titleText)
    {
        decoratorText = string.Empty;
        titleText = string.Empty;
        if (string.IsNullOrWhiteSpace(sectionTitle))
        {
            return false;
        }

        var match = DecoratedChapterTitlePartsPattern.Match(sectionTitle);
        if (!match.Success)
        {
            return false;
        }

        decoratorText = match.Groups[1].Value.Trim();
        titleText = match.Groups[2].Value.Trim();
        return decoratorText.Length > 0 && titleText.Length > 0;
    }

    private static int CountWords(string? text)
    {
        if (string.IsNullOrWhiteSpace(text))
        {
            return 0;
        }

        return Regex.Matches(text, @"\S+").Count;
    }

    private static double EstimateMinimumSpeechDurationSec(int wordCount)
    {
        if (wordCount <= 0)
        {
            return 0.0;
        }

        return Math.Clamp(wordCount * 0.18, 0.35, 1.75);
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
