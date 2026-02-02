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
    /// Uses roomtone from the book's audio context.
    /// </summary>
    /// <param name="chapter">The chapter context containing the audio buffer.</param>
    /// <param name="outputPath">Path for the output treated.wav file.</param>
    /// <param name="options">Treatment options (timing durations, thresholds).</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Treatment result with timing information.</returns>
    public async Task<TreatmentResult> TreatChapterAsync(
        ChapterContext chapter,
        string outputPath,
        TreatmentOptions? options = null,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(chapter);
        ArgumentException.ThrowIfNullOrWhiteSpace(outputPath);

        var roomtoneBuffer = chapter.Book.Audio.Roomtone
            ?? throw new InvalidOperationException(
                $"Roomtone not found at {chapter.Book.Audio.RoomtonePath}. " +
                "Create a roomtone.wav file in the book directory.");

        return await TreatChapterCoreAsync(chapter, roomtoneBuffer, outputPath, options, cancellationToken);
    }

    /// <summary>
    /// Treats a chapter audio by assembling:
    /// [preroll roomtone] -> [title segment] -> [gap roomtone] -> [content segment] -> [postroll roomtone]
    /// Uses an explicit roomtone file path.
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

        var roomtoneBuffer = AudioProcessor.Decode(roomtonePath);
        return await TreatChapterCoreAsync(chapter, roomtoneBuffer, outputPath, options, cancellationToken);
    }

    private async Task<TreatmentResult> TreatChapterCoreAsync(
        ChapterContext chapter,
        AudioBuffer roomtoneBuffer,
        string outputPath,
        TreatmentOptions? options,
        CancellationToken cancellationToken)
    {
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

        Log.Debug(
            "Speech boundaries: title={TitleStart:F3}s-{TitleEnd:F3}s, content={ContentStart:F3}s-{ContentEnd:F3}s",
            titleStart, titleEnd, contentStart, contentEnd);

        // Create temp directory for intermediate files
        var tempDir = Path.Combine(Path.GetTempPath(), "ams", "treat", Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(tempDir);

        try
        {
            // Check if we have a separate title segment (titleStart >= 0 AND has positive duration)
            bool hasTitle = titleStart >= 0 && titleEnd > titleStart;
            var segmentFiles = new List<string>();

            // Preroll (always present)
            var prerollPath = Path.Combine(tempDir, "01-preroll.wav");
            var prerollBuffer = PrepareRoomtoneSegment(roomtoneBuffer, opts.PrerollSeconds);
            AudioProcessor.EncodeWav(prerollPath, prerollBuffer);
            segmentFiles.Add(prerollPath);

            double titleDuration = 0;
            if (hasTitle)
            {
                // Extract title segment
                Log.Debug("Extracting title segment: {Start:F3}s - {End:F3}s", titleStart, titleEnd);
                var titlePath = Path.Combine(tempDir, "02-title.wav");
                var titleBuffer = AudioProcessor.Trim(
                    chapterBuffer,
                    TimeSpan.FromSeconds(titleStart),
                    TimeSpan.FromSeconds(titleEnd));
                AudioProcessor.EncodeWav(titlePath, titleBuffer);
                segmentFiles.Add(titlePath);
                titleDuration = titleEnd - titleStart;

                // Gap between title and content
                var gapPath = Path.Combine(tempDir, "03-gap.wav");
                var gapBuffer = PrepareRoomtoneSegment(roomtoneBuffer, opts.ChapterToContentGapSeconds);
                AudioProcessor.EncodeWav(gapPath, gapBuffer);
                segmentFiles.Add(gapPath);
            }

            // Validate content segment has positive duration
            if (contentEnd <= contentStart)
            {
                throw new InvalidOperationException(
                    $"Content segment has zero or negative duration: {contentStart:F3}s - {contentEnd:F3}s");
            }

            // Extract content segment
            Log.Debug("Extracting content segment: {Start:F3}s - {End:F3}s", contentStart, contentEnd);
            var contentPath = Path.Combine(tempDir, "04-content.wav");
            var contentBuffer = AudioProcessor.Trim(
                chapterBuffer,
                TimeSpan.FromSeconds(contentStart),
                TimeSpan.FromSeconds(contentEnd));
            AudioProcessor.EncodeWav(contentPath, contentBuffer);
            segmentFiles.Add(contentPath);

            // Postroll (always present)
            var postrollPath = Path.Combine(tempDir, "05-postroll.wav");
            var postrollBuffer = PrepareRoomtoneSegment(roomtoneBuffer, opts.PostrollSeconds);
            AudioProcessor.EncodeWav(postrollPath, postrollBuffer);
            segmentFiles.Add(postrollPath);

            // Use FFmpeg concat demuxer to assemble final audio
            await ConcatAudioFilesAsync(
                segmentFiles,
                outputPath,
                tempDir,
                cancellationToken);

            // Calculate total duration
            var totalDuration = opts.PrerollSeconds
                + titleDuration
                + (hasTitle ? opts.ChapterToContentGapSeconds : 0)
                + (contentEnd - contentStart)
                + opts.PostrollSeconds;

            return new TreatmentResult(
                outputPath,
                hasTitle ? titleStart : -1,
                hasTitle ? titleEnd : -1,
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
            // Use negative titleEnd to signal "no title segment"
            return (-1.0, -1.0, 0.0, audioDuration);
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
        if (roomtone.Length == 0)
        {
            throw new InvalidOperationException("Roomtone buffer is empty (0 samples).");
        }

        if (durationSeconds <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(durationSeconds),
                $"Duration must be positive, got {durationSeconds}");
        }

        double roomtoneDuration = roomtone.Length / (double)roomtone.SampleRate;
        Log.Debug(
            "PrepareRoomtoneSegment: roomtone={RoomtoneDuration:F3}s ({Samples} samples), target={TargetDuration:F3}s",
            roomtoneDuration,
            roomtone.Length,
            durationSeconds);

        // Always loop to target duration - simpler and avoids FFmpeg trim edge cases
        int targetSamples = (int)(durationSeconds * roomtone.SampleRate);
        if (targetSamples <= 0)
        {
            throw new InvalidOperationException(
                $"Target samples is {targetSamples} (duration={durationSeconds}s, sampleRate={roomtone.SampleRate})");
        }

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

        // Must read stdout/stderr asynchronously to prevent deadlock when buffers fill
        var stderrTask = process.StandardError.ReadToEndAsync(cancellationToken);
        var stdoutTask = process.StandardOutput.ReadToEndAsync(cancellationToken);

        await process.WaitForExitAsync(cancellationToken);

        var stderr = await stderrTask;
        await stdoutTask; // Discard stdout but ensure it's drained

        if (process.ExitCode != 0)
        {
            throw new InvalidOperationException(
                $"FFmpeg concat failed with exit code {process.ExitCode}: {stderr}");
        }
    }
}
