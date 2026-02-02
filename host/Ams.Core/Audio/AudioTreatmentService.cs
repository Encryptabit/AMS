using System.Globalization;
using System.Text;
using Ams.Core.Artifacts;
using Ams.Core.Processors;
using Ams.Core.Runtime.Chapter;

namespace Ams.Core.Audio;

/// <summary>
/// Service for treating chapter audio with roomtone padding.
/// </summary>
public sealed class AudioTreatmentService
{
    /// <summary>
    /// Result of audio treatment processing.
    /// </summary>
    public sealed record TreatmentResult(
        string OutputPath,
        double TitleStartSec,
        double TitleEndSec,
        double ContentStartSec,
        double ContentEndSec,
        double TotalDurationSec);

    /// <summary>
    /// Treats a chapter audio by assembling:
    /// [preroll roomtone] -> [title segment] -> [gap roomtone] -> [content segment] -> [postroll roomtone]
    /// </summary>
    /// <param name="chapter">The chapter context containing the audio buffer.</param>
    /// <param name="roomtonePath">Path to the roomtone.wav file.</param>
    /// <param name="outputPath">Path for the output treated.wav file.</param>
    /// <param name="options">Treatment options (timing durations, thresholds).</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Treatment result with timing information.</returns>
    public async Task<TreatmentResult> TreatChapterAsync(
        ChapterContext chapter,
        string roomtonePath,
        string outputPath,
        TreatmentOptions? options = null,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(chapter);
        ArgumentException.ThrowIfNullOrWhiteSpace(roomtonePath);
        ArgumentException.ThrowIfNullOrWhiteSpace(outputPath);

        if (!File.Exists(roomtonePath))
        {
            throw new FileNotFoundException($"Roomtone file not found: {roomtonePath}", roomtonePath);
        }

        var opts = options ?? new TreatmentOptions();

        // Get chapter audio from the AudioBufferManager
        var audioContext = chapter.Audio.Current;
        var chapterBuffer = audioContext.Buffer
            ?? throw new InvalidOperationException(
                $"Failed to load audio buffer for chapter '{chapter.Descriptor.ChapterId}'");

        // Detect silence intervals to find speech boundaries
        var silenceOpts = new SilenceDetectOptions
        {
            NoiseDb = opts.SilenceThresholdDb,
            MinimumDuration = TimeSpan.FromSeconds(opts.MinimumSilenceDuration)
        };
        var silenceIntervals = AudioProcessor.DetectSilence(chapterBuffer, silenceOpts);

        // Find title and content boundaries
        var (titleStart, titleEnd, contentStart, contentEnd) = FindSpeechBoundaries(
            chapterBuffer,
            silenceIntervals,
            opts.TitleContentGapThreshold);

        // Create temp directory for intermediate files
        var tempDir = Path.Combine(Path.GetTempPath(), "ams", "treat", Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(tempDir);

        try
        {
            // Extract segments and prepare roomtone segments
            var prerollPath = Path.Combine(tempDir, "01-preroll.wav");
            var titlePath = Path.Combine(tempDir, "02-title.wav");
            var gapPath = Path.Combine(tempDir, "03-gap.wav");
            var contentPath = Path.Combine(tempDir, "04-content.wav");
            var postrollPath = Path.Combine(tempDir, "05-postroll.wav");

            // Extract title segment
            var titleBuffer = AudioProcessor.Trim(
                chapterBuffer,
                TimeSpan.FromSeconds(titleStart),
                TimeSpan.FromSeconds(titleEnd));
            AudioProcessor.EncodeWav(titlePath, titleBuffer);

            // Extract content segment
            var contentBuffer = AudioProcessor.Trim(
                chapterBuffer,
                TimeSpan.FromSeconds(contentStart),
                TimeSpan.FromSeconds(contentEnd));
            AudioProcessor.EncodeWav(contentPath, contentBuffer);

            // Load and prepare roomtone segments
            var roomtoneBuffer = AudioProcessor.Decode(roomtonePath);

            // Ensure roomtone is long enough; loop if needed
            var prerollBuffer = PrepareRoomtoneSegment(roomtoneBuffer, opts.PrerollSeconds);
            AudioProcessor.EncodeWav(prerollPath, prerollBuffer);

            var gapBuffer = PrepareRoomtoneSegment(roomtoneBuffer, opts.ChapterToContentGapSeconds);
            AudioProcessor.EncodeWav(gapPath, gapBuffer);

            var postrollBuffer = PrepareRoomtoneSegment(roomtoneBuffer, opts.PostrollSeconds);
            AudioProcessor.EncodeWav(postrollPath, postrollBuffer);

            // Use FFmpeg concat demuxer to assemble final audio
            await ConcatAudioFilesAsync(
                new[] { prerollPath, titlePath, gapPath, contentPath, postrollPath },
                outputPath,
                tempDir,
                cancellationToken);

            // Calculate total duration
            var totalDuration = opts.PrerollSeconds
                + (titleEnd - titleStart)
                + opts.ChapterToContentGapSeconds
                + (contentEnd - contentStart)
                + opts.PostrollSeconds;

            return new TreatmentResult(
                outputPath,
                titleStart,
                titleEnd,
                contentStart,
                contentEnd,
                totalDuration);
        }
        finally
        {
            // Cleanup temp directory
            try
            {
                if (Directory.Exists(tempDir))
                {
                    Directory.Delete(tempDir, recursive: true);
                }
            }
            catch
            {
                // Best-effort cleanup
            }
        }
    }

    /// <summary>
    /// Finds speech boundaries: title start/end and content start/end.
    /// Title ends when there's a significant silence gap (>threshold).
    /// Content starts after that gap and ends at the final speech offset.
    /// </summary>
    private static (double TitleStart, double TitleEnd, double ContentStart, double ContentEnd) FindSpeechBoundaries(
        AudioBuffer buffer,
        IReadOnlyList<SilenceInterval> silenceIntervals,
        double gapThreshold)
    {
        double audioDuration = buffer.Length / (double)buffer.SampleRate;

        if (silenceIntervals.Count == 0)
        {
            // No silence detected - treat entire audio as content, no separate title
            return (0.0, 0.0, 0.0, audioDuration);
        }

        // Find first speech onset (end of first silence if it starts at 0, else 0)
        double titleStart = 0.0;
        if (silenceIntervals[0].Start.TotalSeconds < 0.1)
        {
            titleStart = silenceIntervals[0].End.TotalSeconds;
        }

        // Find the first significant gap after title starts (>gapThreshold seconds)
        double titleEnd = audioDuration;
        double contentStart = audioDuration;
        bool foundTitleContentGap = false;

        foreach (var interval in silenceIntervals)
        {
            // Skip silences before title starts
            if (interval.End.TotalSeconds <= titleStart)
            {
                continue;
            }

            // Look for significant gap indicating title/content boundary
            if (interval.Duration.TotalSeconds >= gapThreshold)
            {
                titleEnd = interval.Start.TotalSeconds;
                contentStart = interval.End.TotalSeconds;
                foundTitleContentGap = true;
                break;
            }
        }

        // If no significant gap found, assume first sentence is title
        // Use a heuristic: first ~10 seconds is title
        if (!foundTitleContentGap)
        {
            // Find first gap after at least some speech
            foreach (var interval in silenceIntervals)
            {
                if (interval.Start.TotalSeconds > titleStart + 1.0 &&
                    interval.Duration.TotalSeconds >= 0.3)
                {
                    titleEnd = interval.Start.TotalSeconds;
                    contentStart = interval.End.TotalSeconds;
                    break;
                }
            }
        }

        // Find content end (last speech offset)
        double contentEnd = audioDuration;
        var lastSilence = silenceIntervals.LastOrDefault();
        if (lastSilence != default &&
            Math.Abs(lastSilence.End.TotalSeconds - audioDuration) < 0.1)
        {
            contentEnd = lastSilence.Start.TotalSeconds;
        }

        // Ensure valid boundaries
        titleEnd = Math.Max(titleStart, titleEnd);
        contentStart = Math.Max(titleEnd, contentStart);
        contentEnd = Math.Max(contentStart, contentEnd);

        return (titleStart, titleEnd, contentStart, contentEnd);
    }

    /// <summary>
    /// Prepares a roomtone segment of the specified duration.
    /// If roomtone is shorter than needed, loops it.
    /// </summary>
    private static AudioBuffer PrepareRoomtoneSegment(AudioBuffer roomtone, double durationSeconds)
    {
        double roomtoneDuration = roomtone.Length / (double)roomtone.SampleRate;

        if (roomtoneDuration >= durationSeconds)
        {
            // Roomtone is long enough, just trim
            return AudioProcessor.Trim(roomtone, TimeSpan.Zero, TimeSpan.FromSeconds(durationSeconds));
        }

        // Need to loop roomtone
        int targetSamples = (int)(durationSeconds * roomtone.SampleRate);
        var buffer = new AudioBuffer(roomtone.Channels, roomtone.SampleRate, targetSamples);

        for (int ch = 0; ch < roomtone.Channels; ch++)
        {
            var source = roomtone.Planar[ch];
            var target = buffer.Planar[ch];
            int sourceLen = source.Length;

            for (int i = 0; i < targetSamples; i++)
            {
                target[i] = source[i % sourceLen];
            }
        }

        return buffer;
    }

    /// <summary>
    /// Concatenates multiple audio files using FFmpeg concat demuxer.
    /// </summary>
    private static async Task ConcatAudioFilesAsync(
        IReadOnlyList<string> inputFiles,
        string outputPath,
        string tempDir,
        CancellationToken cancellationToken)
    {
        // Create concat list file
        var concatListPath = Path.Combine(tempDir, "concat.txt");
        var listBuilder = new StringBuilder();
        foreach (var file in inputFiles)
        {
            // FFmpeg concat demuxer requires forward slashes and escaping
            var escaped = file.Replace("\\", "/").Replace("'", "'\\''");
            listBuilder.AppendLine(CultureInfo.InvariantCulture, $"file '{escaped}'");
        }
        await File.WriteAllTextAsync(concatListPath, listBuilder.ToString(), cancellationToken);

        // Ensure output directory exists
        var outputDir = Path.GetDirectoryName(outputPath);
        if (!string.IsNullOrEmpty(outputDir))
        {
            Directory.CreateDirectory(outputDir);
        }

        // Run FFmpeg concat demuxer
        var psi = new System.Diagnostics.ProcessStartInfo
        {
            FileName = "ffmpeg",
            UseShellExecute = false,
            CreateNoWindow = true,
            RedirectStandardOutput = true,
            RedirectStandardError = true
        };

        psi.ArgumentList.Add("-y");
        psi.ArgumentList.Add("-f");
        psi.ArgumentList.Add("concat");
        psi.ArgumentList.Add("-safe");
        psi.ArgumentList.Add("0");
        psi.ArgumentList.Add("-i");
        psi.ArgumentList.Add(concatListPath);
        psi.ArgumentList.Add("-c");
        psi.ArgumentList.Add("copy");
        psi.ArgumentList.Add(outputPath);

        using var process = System.Diagnostics.Process.Start(psi);
        if (process == null)
        {
            throw new InvalidOperationException("Failed to start FFmpeg process.");
        }

        await process.WaitForExitAsync(cancellationToken);

        if (process.ExitCode != 0)
        {
            var stderr = await process.StandardError.ReadToEndAsync(cancellationToken);
            throw new InvalidOperationException(
                $"FFmpeg concat failed with exit code {process.ExitCode}: {stderr}");
        }
    }
}
