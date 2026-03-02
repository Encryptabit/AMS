using System.Diagnostics;
using System.Globalization;
using System.Text.Json;
using System.Text.RegularExpressions;
using Ams.Core.Common;

namespace Ams.Core.Audio.QualityControl;

/// <summary>
/// Analyzes audiobook chapter audio files for QC using ffmpeg silencedetect.
/// Pure analysis functions (ParseSilenceRegions, AnalyzeStructure, FlagAnomalies)
/// are static and testable without ffmpeg. AnalyzeFileAsync requires ffmpeg/ffprobe.
/// </summary>
public static class AudioQcAnalyzer
{
    private static readonly Regex SilenceStartRegex = new(@"silence_start:\s*([\d.]+)", RegexOptions.Compiled);
    private static readonly Regex SilenceEndRegex = new(@"silence_end:\s*([\d.]+)", RegexOptions.Compiled);
    private static readonly Regex SilenceDurationRegex = new(@"silence_duration:\s*([\d.]+)", RegexOptions.Compiled);

    /// <summary>
    /// Tolerance in seconds for considering a silence region as starting "near" 0.0
    /// or ending "near" the total file duration.
    /// </summary>
    private const double NearBoundaryTolerance = 0.05;

    /// <summary>
    /// Parses silence regions from ffmpeg silencedetect stderr output.
    /// Handles trailing silence_start with no matching silence_end by returning
    /// a sentinel region with End = -1 and Duration = -1.
    /// Uses InvariantCulture for all double parsing.
    /// </summary>
    public static SilenceRegion[] ParseSilenceRegions(string ffmpegStderr)
    {
        if (string.IsNullOrEmpty(ffmpegStderr))
            return [];

        var regions = new List<SilenceRegion>();
        double? currentStart = null;

        foreach (var line in ffmpegStderr.Split('\n'))
        {
            if (line.Contains("silence_start:"))
            {
                var m = SilenceStartRegex.Match(line);
                if (m.Success)
                {
                    currentStart = double.Parse(m.Groups[1].Value, CultureInfo.InvariantCulture);
                }
            }
            else if (line.Contains("silence_end:") && currentStart.HasValue)
            {
                var endMatch = SilenceEndRegex.Match(line);
                var durMatch = SilenceDurationRegex.Match(line);
                if (endMatch.Success)
                {
                    var end = double.Parse(endMatch.Groups[1].Value, CultureInfo.InvariantCulture);
                    var duration = durMatch.Success
                        ? double.Parse(durMatch.Groups[1].Value, CultureInfo.InvariantCulture)
                        : end - currentStart.Value;
                    regions.Add(new SilenceRegion(currentStart.Value, end, duration));
                    currentStart = null;
                }
            }
        }

        // Handle trailing silence_start with no matching silence_end
        if (currentStart.HasValue)
        {
            regions.Add(new SilenceRegion(currentStart.Value, -1.0, -1.0));
        }

        return regions.ToArray();
    }

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

        // Title-body gap: after head silence, the next silence region that is NOT the tail
        // We need at least: head + gap + tail (3 regions) or head + gap (2+ regions where gap != tail)
        double? titleDurationSec = null;
        double? titleBodyGapSec = null;

        // Find the gap region: first non-head, non-tail silence region
        int gapIndex = -1;
        for (int i = 0; i < silences.Count; i++)
        {
            if (i == headIndex || i == tailIndex) continue;
            gapIndex = i;
            break;
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
    /// Analyzes a single audio file for QC by running ffmpeg silencedetect and ffprobe.
    /// </summary>
    public static async Task<ChapterQcResult> AnalyzeFileAsync(
        string filePath,
        double noiseDb,
        double minSilenceDurationSec,
        QcThresholds thresholds,
        CancellationToken ct)
    {
        var fileName = Path.GetFileName(filePath);

        // Get file metadata via ffprobe
        var (durationSec, channels, sampleRate) = await ProbeFileAsync(filePath, ct);

        // Run silence detection
        var silenceStderr = await RunSilenceDetectAsync(filePath, noiseDb, minSilenceDurationSec, ct);
        var silences = ParseSilenceRegions(silenceStderr);

        // Analyze structure
        var (headSilence, titleDuration, titleBodyGap, tailSilence) =
            AnalyzeStructure(silences, durationSec);

        var result = new ChapterQcResult
        {
            FileName = fileName,
            DurationSec = durationSec,
            Channels = channels,
            SampleRate = sampleRate,
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

    private static async Task<(double DurationSec, int Channels, int SampleRate)> ProbeFileAsync(
        string filePath, CancellationToken ct)
    {
        var ffprobeExe = Environment.GetEnvironmentVariable("FFPROBE_EXE");
        if (string.IsNullOrWhiteSpace(ffprobeExe))
        {
            // Try ffprobe next to ffmpeg
            var ffmpegExe = Environment.GetEnvironmentVariable("FFMPEG_EXE");
            if (!string.IsNullOrWhiteSpace(ffmpegExe))
            {
                var dir = Path.GetDirectoryName(ffmpegExe);
                if (dir is not null)
                {
                    var candidate = Path.Combine(dir, "ffprobe");
                    if (File.Exists(candidate)) ffprobeExe = candidate;
                    candidate = Path.Combine(dir, "ffprobe.exe");
                    if (File.Exists(candidate)) ffprobeExe = candidate;
                }
            }
            ffprobeExe ??= "ffprobe";
        }

        var psi = new ProcessStartInfo
        {
            FileName = ffprobeExe,
            Arguments = $"-v quiet -print_format json -show_format -show_streams \"{filePath}\"",
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            UseShellExecute = false,
            CreateNoWindow = true
        };

        using var p = Process.Start(psi) ?? throw new InvalidOperationException("Failed to start ffprobe");
        var stdout = await p.StandardOutput.ReadToEndAsync(ct);
        await p.WaitForExitAsync(ct);

        using var doc = JsonDocument.Parse(stdout);
        var root = doc.RootElement;

        double duration = 0;
        int channels = 0;
        int sampleRate = 0;

        // Try format.duration first
        if (root.TryGetProperty("format", out var format) &&
            format.TryGetProperty("duration", out var fmtDuration))
        {
            duration = double.Parse(fmtDuration.GetString()!, CultureInfo.InvariantCulture);
        }

        // Get stream info
        if (root.TryGetProperty("streams", out var streams) && streams.GetArrayLength() > 0)
        {
            var audioStream = streams[0];
            if (audioStream.TryGetProperty("channels", out var ch))
                channels = ch.GetInt32();
            if (audioStream.TryGetProperty("sample_rate", out var sr))
                sampleRate = int.Parse(sr.GetString()!, CultureInfo.InvariantCulture);
            // Fallback: get duration from stream if not in format
            if (duration == 0 && audioStream.TryGetProperty("duration", out var streamDuration))
                duration = double.Parse(streamDuration.GetString()!, CultureInfo.InvariantCulture);
        }

        return (duration, channels, sampleRate);
    }

    private static async Task<string> RunSilenceDetectAsync(
        string filePath, double noiseDb, double minSilenceDurationSec, CancellationToken ct)
    {
        var ffmpegExe = Environment.GetEnvironmentVariable("FFMPEG_EXE");
        if (string.IsNullOrWhiteSpace(ffmpegExe)) ffmpegExe = "ffmpeg";

        var noiseStr = noiseDb.ToString(CultureInfo.InvariantCulture);
        var durationStr = minSilenceDurationSec.ToString(CultureInfo.InvariantCulture);

        var psi = new ProcessStartInfo
        {
            FileName = ffmpegExe,
            Arguments = $"-i \"{filePath}\" -af silencedetect=noise={noiseStr}dB:duration={durationStr} -f null -",
            RedirectStandardError = true,
            RedirectStandardOutput = true,
            UseShellExecute = false,
            CreateNoWindow = true
        };

        using var p = Process.Start(psi) ?? throw new InvalidOperationException("Failed to start ffmpeg");
        var stderr = await p.StandardError.ReadToEndAsync(ct);
        await p.WaitForExitAsync(ct);

        return stderr;
    }
}
