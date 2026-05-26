using System;
using System.IO;
using Ams.Core.Artifacts;
using Ams.Core.Audio;
using Ams.Core.Processors;
using Ams.Core.Runtime.Audio;
using Ams.Core.Runtime.Book;
using Ams.Workstation.Server.Services;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Primitives;

namespace Ams.Workstation.Server.Controllers;

/// <summary>
/// Serves audio files for the waveform player.
/// Streams audio from AudioBufferContext when a chapter is loaded.
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class AudioController : ControllerBase
{
    private const int DefaultPeakPxPerSec = 1200;
    private const int MaxWaveformBuckets = 500_000;
    private readonly IWebHostEnvironment _environment;
    private readonly ILogger<AudioController> _logger;
    private readonly BlazorWorkspace _workspace;
    private readonly PreviewBufferService _previewBuffer;

    public AudioController(
        IWebHostEnvironment environment,
        ILogger<AudioController> logger,
        BlazorWorkspace workspace,
        PreviewBufferService previewBuffer)
    {
        _environment = environment;
        _logger = logger;
        _workspace = workspace;
        _previewBuffer = previewBuffer;
    }

    /// <summary>
    /// Serves an audio file from the specified path.
    /// </summary>
    /// <param name="path">The file path (URL-encoded)</param>
    /// <returns>Audio file stream with appropriate content type</returns>
    [HttpGet]
    public IActionResult GetAudio([FromQuery] string? path)
    {
        if (string.IsNullOrEmpty(path))
        {
            // For testing, serve a sample audio from wwwroot/audio
            var samplePath = Path.Combine(_environment.WebRootPath, "audio", "sample.wav");
            if (System.IO.File.Exists(samplePath))
            {
                return ServeAudioFile(samplePath);
            }
            return NotFound("No path specified and no sample audio found");
        }

        // Decode the path
        var decodedPath = Uri.UnescapeDataString(path);

        // Security check: ensure the path exists and is accessible
        if (!System.IO.File.Exists(decodedPath))
        {
            _logger.LogWarning("Audio file not found: {Path}", decodedPath);
            return NotFound($"Audio file not found: {decodedPath}");
        }

        return ServeAudioFile(decodedPath);
    }

    /// <summary>
    /// Serves the configured app-level playback-error alert sound used by the proof playback view.
    /// </summary>
    [HttpGet("alerts/playback-error")]
    public IActionResult GetPlaybackErrorAlertAudio()
    {
        if (!_workspace.IsInitialized)
        {
            return NotFound("Workspace not initialized");
        }

        var descriptor = _workspace.Book.Audio.PlaybackErrorAlertSound;
        if (descriptor is null)
        {
            return NotFound("Playback error alert sound is not configured");
        }

        if (!System.IO.File.Exists(descriptor.SourcePath))
        {
            _workspace.Book.Audio.ClearPlaybackErrorAlertSound();
            _logger.LogWarning(
                "Playback error alert sound is configured but missing on disk at {Path}",
                descriptor.SourcePath);
            return NotFound("Playback error alert sound file is missing");
        }

        return ServeAudioFile(descriptor.SourcePath);
    }

    /// <summary>
    /// Gets the audio for a chapter from the workspace's AudioBufferContext.
    /// Streams the resolved audio artifact when no range is requested.
    /// When start/end query params are provided, streams an in-memory slice of that segment.
    /// </summary>
    [HttpGet("chapter/{chapterName}")]
    public IActionResult GetChapterAudio(string chapterName, [FromQuery] double? start = null, [FromQuery] double? end = null)
    {
        if (_workspace.CurrentChapterHandle is null)
        {
            _logger.LogWarning("GetChapterAudio called but no chapter is loaded");
            return NotFound("No chapter loaded");
        }

        try
        {
            var audioContext = _workspace.CurrentChapterHandle.Chapter.Audio.Current;

            // If start/end provided, stream the requested in-memory segment.
            if (start.HasValue && end.HasValue)
            {
                var buffer = audioContext.Buffer;
                if (buffer is null)
                {
                    _logger.LogWarning("Audio buffer not available for ranged chapter '{ChapterName}'", chapterName);
                    return NotFound("Audio buffer not available");
                }

                LogRangeDiagnostics(chapterName, audioContext, buffer, start, end);

                if (!buffer.TrySliceClamped(
                        TimeSpan.FromSeconds(start.Value),
                        TimeSpan.FromSeconds(end.Value),
                        out var segment))
                {
                    return BadRequest("Invalid chapter audio range");
                }

                return StreamAudioBuffer(segment);
            }

            _logger.LogDebug(
                "Streaming chapter audio file for chapter '{ChapterName}' from buffer '{BufferId}'",
                chapterName,
                audioContext.Descriptor.BufferId);

            return StreamAudioFile(audioContext, "Audio file not available");
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning(ex, "No audio buffers registered for chapter '{ChapterName}'", chapterName);
            return NotFound($"No audio buffers available for chapter '{chapterName}'");
        }
    }

    /// <summary>
    /// Returns precomputed min/max waveform peaks for the current chapter audio.
    /// Optional start/end values slice the chapter before peak extraction.
    /// </summary>
    [HttpGet("chapter/{chapterName}/peaks")]
    public IActionResult GetChapterPeaks(
        string chapterName,
        [FromQuery] int pxPerSec = DefaultPeakPxPerSec,
        [FromQuery] double? start = null,
        [FromQuery] double? end = null)
    {
        if (_workspace.CurrentChapterHandle is null)
        {
            return NotFound("No chapter loaded");
        }

        var audioContext = _workspace.CurrentChapterHandle.Chapter.Audio.Current;
        var buffer = audioContext.Buffer;
        if (buffer is null)
        {
            return NotFound("Audio buffer not available");
        }

        if (start.HasValue || end.HasValue)
        {
            if (!buffer.TrySliceClamped(
                    start.HasValue ? TimeSpan.FromSeconds(start.Value) : null,
                    end.HasValue ? TimeSpan.FromSeconds(end.Value) : null,
                    out var segment))
            {
                return BadRequest("Invalid chapter peak range");
            }

            return Ok(BuildWaveformPeaksPayload(segment, pxPerSec, $"chapter '{chapterName}' range"));
        }

        var bucketCount = ResolveWaveformBucketCount(buffer, pxPerSec, $"chapter '{chapterName}'");
        var peaks = audioContext.GetOrCreateWaveformPeaks(bucketCount);
        if (peaks is null)
        {
            return NotFound("Audio buffer not available");
        }

        return Ok(ToWaveformResponse(peaks));
    }

    /// <summary>
    /// Serves a partial region of a chapter's audio by decoding only the requested time range.
    /// Uses the current chapter audio descriptor and decodes directly from disk with start/duration
    /// parameters for memory-efficient partial loading.
    /// </summary>
    [HttpGet("chapter/{chapterName}/region")]
    public IActionResult GetChapterRegionAudio(string chapterName, [FromQuery] double start, [FromQuery] double end)
    {
        if (start < 0)
        {
            return BadRequest("Start must be >= 0");
        }

        if (end <= start)
        {
            return BadRequest("End must be greater than start");
        }

        if (_workspace.CurrentChapterHandle is null)
        {
            return NotFound("No chapter loaded");
        }

        try
        {
            var requestedChapter = Uri.UnescapeDataString(chapterName ?? string.Empty).Trim();
            if (string.IsNullOrWhiteSpace(requestedChapter))
            {
                return BadRequest("Chapter name is required");
            }

            var handle = _workspace.CurrentChapterHandle;
            var descriptor = handle.Chapter.Descriptor;
            var activeChapter = string.IsNullOrWhiteSpace(_workspace.CurrentChapterName)
                ? descriptor.ChapterId
                : _workspace.CurrentChapterName!.Trim();

            if (!MatchesRequestedChapter(requestedChapter, activeChapter, descriptor))
            {
                _logger.LogWarning(
                    "Region playback request rejected for chapter '{RequestedChapter}'. Active chapter='{ActiveChapter}', stem='{ChapterId}'.",
                    requestedChapter,
                    activeChapter,
                    descriptor.ChapterId);
                return NotFound($"Chapter '{requestedChapter}' is not active (active chapter '{activeChapter}').");
            }

            var audioContext = handle.Chapter.Audio.Current;
            var audioPath = audioContext.Descriptor.Path;

            if (!System.IO.File.Exists(audioPath))
            {
                _logger.LogWarning(
                    "Audio descriptor file not found for chapter '{ChapterName}' at '{Path}'",
                    requestedChapter,
                    audioPath);
                return NotFound($"Audio file not found for chapter '{requestedChapter}'");
            }

            // Validate end does not exceed duration
            var info = AudioProcessor.Probe(audioPath);
            var duration = info.Duration.TotalSeconds;

            if (start >= duration)
            {
                return BadRequest($"Start ({start:F2}s) exceeds audio duration ({duration:F2}s)");
            }

            var clampedEnd = Math.Min(end, duration);
            var regionDuration = clampedEnd - start;

            _logger.LogDebug(
                "Serving region audio for chapter '{ChapterName}': {Start:F2}s - {End:F2}s",
                chapterName, start, clampedEnd);

            // Decode only the requested region from disk
            var buffer = AudioProcessor.Decode(audioPath, new AudioDecodeOptions(
                Start: TimeSpan.FromSeconds(start),
                Duration: TimeSpan.FromSeconds(regionDuration)));

            return StreamAudioBuffer(buffer);
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning(ex, "No audio buffers registered for chapter '{ChapterName}'", chapterName);
            return NotFound($"No audio buffers available for chapter '{chapterName}'");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to serve region audio for chapter '{ChapterName}'", chapterName);
            return StatusCode(500, $"Failed to serve region audio: {ex.Message}");
        }
    }

    /// <summary>
    /// Serves the in-memory preview buffer from PreviewBufferService.
    /// Returns 404 if no preview has been generated yet.
    /// </summary>
    [HttpGet("preview")]
    public IActionResult GetPreviewAudio()
    {
        var buffer = _previewBuffer.Buffer;
        if (buffer is null)
        {
            return NotFound("No preview buffer available");
        }

        return StreamAudioBuffer(buffer);
    }

    /// <summary>
    /// Returns precomputed min/max waveform peaks for the transient in-memory preview buffer.
    /// </summary>
    [HttpGet("preview/peaks")]
    public IActionResult GetPreviewPeaks([FromQuery] int pxPerSec = DefaultPeakPxPerSec)
    {
        var buffer = _previewBuffer.Buffer;
        if (buffer is null)
        {
            return NotFound("No preview buffer available");
        }

        return Ok(BuildWaveformPeaksPayload(buffer, pxPerSec, "preview buffer"));
    }

    /// <summary>
    /// Serves corrected chapter audio using deterministic corrected→treated→current fallback.
    /// </summary>
    [HttpGet("chapter/{chapterName}/corrected")]
    public IActionResult GetCorrectedChapterAudio(string chapterName)
    {
        if (!TryResolveCorrectedPlayback(chapterName, out var resolved, out var failureResult))
        {
            return failureResult!;
        }

        _logger.LogDebug(
            "Serving corrected playback audio for chapter '{RequestedChapter}' from source '{Source}'",
            resolved.RequestedChapter,
            resolved.Source);

        return StreamAudioFile(
            resolved.Context,
            $"No audio available for chapter '{resolved.RequestedChapter}' (source '{resolved.Source}').");
    }

    /// <summary>
    /// Returns precomputed min/max waveform peaks for corrected chapter audio.
    /// Uses same corrected→treated→current fallback resolution as playback.
    /// </summary>
    [HttpGet("chapter/{chapterName}/corrected/peaks")]
    public IActionResult GetCorrectedChapterPeaks(string chapterName, [FromQuery] int pxPerSec = DefaultPeakPxPerSec)
    {
        if (!TryResolveCorrectedPlayback(chapterName, out var resolved, out var failureResult))
        {
            return failureResult!;
        }

        var buffer = resolved.Context.Buffer;
        if (buffer is null)
        {
            _logger.LogWarning(
                "Corrected waveform peaks unavailable because audio buffer failed to load for chapter '{RequestedChapter}' from source '{Source}'",
                resolved.RequestedChapter,
                resolved.Source);
            return NotFound($"No audio available for chapter '{resolved.RequestedChapter}' (source '{resolved.Source}').");
        }

        var bucketCount = ResolveWaveformBucketCount(
            buffer,
            pxPerSec,
            $"corrected chapter '{resolved.RequestedChapter}' source '{resolved.Source}'");
        var peaks = resolved.Context.GetOrCreateWaveformPeaks(bucketCount);
        if (peaks is null)
        {
            _logger.LogWarning(
                "Waveform peaks unavailable for chapter '{RequestedChapter}' from source '{Source}'",
                resolved.RequestedChapter,
                resolved.Source);
            return NotFound($"No audio available for chapter '{resolved.RequestedChapter}' (source '{resolved.Source}').");
        }

        return Ok(ToWaveformResponse(peaks));
    }

    /// <summary>
    /// Returns normalized RMS amplitude data for a segment of an audio file.
    /// Used by mini waveform thumbnails to render lightweight canvas-based visualizations
    /// without requiring a full wavesurfer.js instance.
    /// </summary>
    /// <param name="path">Absolute path to the audio file.</param>
    /// <param name="start">Optional start time in seconds.</param>
    /// <param name="end">Optional end time in seconds.</param>
    /// <param name="points">Number of amplitude data points to return (clamped to 20-500).</param>
    /// <returns>JSON array of normalized floats (0.0 to 1.0).</returns>
    [HttpGet("waveform-data")]
    public IActionResult GetWaveformData(
        [FromQuery] string path,
        [FromQuery] double? start = null,
        [FromQuery] double? end = null,
        [FromQuery] int points = 100)
    {
        if (string.IsNullOrEmpty(path))
        {
            return BadRequest("Path is required");
        }

        var decodedPath = Uri.UnescapeDataString(path);

        if (!System.IO.File.Exists(decodedPath))
        {
            return NotFound($"Audio file not found: {decodedPath}");
        }

        // Clamp points to safe range
        points = Math.Clamp(points, 20, 500);

        try
        {
            // Decode the audio segment
            AudioDecodeOptions? decodeOptions = null;

            if (start.HasValue && end.HasValue && end.Value > start.Value)
            {
                decodeOptions = new AudioDecodeOptions(
                    Start: TimeSpan.FromSeconds(start.Value),
                    Duration: TimeSpan.FromSeconds(end.Value - start.Value));
            }
            else if (start.HasValue)
            {
                decodeOptions = new AudioDecodeOptions(
                    Start: TimeSpan.FromSeconds(start.Value));
            }

            var buffer = AudioProcessor.Decode(decodedPath, decodeOptions);

            if (buffer.Length == 0)
            {
                return Ok(Array.Empty<float>());
            }

            // Compute RMS amplitude per block across all channels
            var samplesPerBlock = Math.Max(1, buffer.Length / points);
            var actualPoints = Math.Min(points, buffer.Length);
            var amplitudes = new float[actualPoints];
            var maxAmplitude = 0f;

            for (var i = 0; i < actualPoints; i++)
            {
                var blockStart = i * samplesPerBlock;
                var blockEnd = Math.Min(blockStart + samplesPerBlock, buffer.Length);
                var sumSquares = 0.0;
                var count = 0;

                for (var ch = 0; ch < buffer.Channels; ch++)
                {
                    for (var s = blockStart; s < blockEnd; s++)
                    {
                        var sample = buffer.GetChannel(ch).Span[s];
                        sumSquares += sample * sample;
                        count++;
                    }
                }

                var rms = count > 0 ? (float)Math.Sqrt(sumSquares / count) : 0f;
                amplitudes[i] = rms;

                if (rms > maxAmplitude)
                {
                    maxAmplitude = rms;
                }
            }

            // Normalize to 0.0 - 1.0
            if (maxAmplitude > 0f)
            {
                for (var i = 0; i < amplitudes.Length; i++)
                {
                    amplitudes[i] /= maxAmplitude;
                }
            }

            return Ok(amplitudes);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to compute waveform data for '{Path}'", decodedPath);
            return StatusCode(500, $"Failed to compute waveform data: {ex.Message}");
        }
    }

    private IActionResult ServeAudioFile(string filePath)
    {
        var extension = Path.GetExtension(filePath).ToLowerInvariant();
        var contentType = extension switch
        {
            ".wav" => "audio/wav",
            ".mp3" => "audio/mpeg",
            ".ogg" => "audio/ogg",
            ".flac" => "audio/flac",
            ".m4a" => "audio/mp4",
            _ => "application/octet-stream"
        };

        _logger.LogDebug("Serving audio file: {Path} as {ContentType}", filePath, contentType);

        var fileStream = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.Read);
        return File(fileStream, contentType, enableRangeProcessing: true);
    }

    private IActionResult StreamAudioFile(AudioBufferContext context, string notFoundMessage)
    {
        var filePath = context.Descriptor.Path;
        if (!System.IO.File.Exists(filePath))
        {
            _logger.LogWarning(
                "Audio descriptor file not found for buffer '{BufferId}' at '{Path}'",
                context.Descriptor.BufferId,
                filePath);
            return NotFound(notFoundMessage);
        }

        return ServeAudioFile(filePath);
    }

    private IActionResult StreamAudioBuffer(AudioBuffer buffer)
    {
        var stream = buffer.ToWavStream();
        return File(stream, "audio/wav", enableRangeProcessing: true);
    }

    private object BuildWaveformPeaksPayload(AudioBuffer buffer, int pxPerSec, string description)
    {
        var bucketCount = ResolveWaveformBucketCount(buffer, pxPerSec, description);
        var peaks = WaveformPeakExtractor.ComputeMonoMinMaxEnvelope(buffer, bucketCount);
        return ToWaveformResponse(peaks);
    }

    private object ToWaveformResponse(WaveformPeaks peaks)
    {
        return new
        {
            duration = peaks.DurationSeconds,
            peaks = new[] { peaks.Data }
        };
    }

    private int ResolveWaveformBucketCount(AudioBuffer buffer, int pxPerSec, string description)
    {
        var clampedPxPerSec = Math.Max(1, pxPerSec);
        var durationSeconds = buffer.SampleRate > 0
            ? buffer.Length / (double)buffer.SampleRate
            : 0d;

        var requestedBuckets = Math.Max(1, (int)Math.Ceiling(durationSeconds * clampedPxPerSec));
        var actualBuckets = Math.Min(requestedBuckets, MaxWaveformBuckets);

        if (actualBuckets != requestedBuckets)
        {
            _logger.LogInformation(
                "Clamped waveform peak bucket count for {Description} from {RequestedBuckets} to {ActualBuckets}",
                description,
                requestedBuckets,
                actualBuckets);
        }

        return actualBuckets;
    }

    private void LogRangeDiagnostics(string chapterName, AudioBufferContext context, AudioBuffer buffer, double? start, double? end)
    {
        if (!_logger.IsEnabled(LogLevel.Debug))
        {
            return;
        }

        var rangeHeader = Request.Headers.Range;
        if (StringValues.IsNullOrEmpty(rangeHeader))
        {
            return;
        }

        var rangeValue = rangeHeader.ToString();
        var approxSec = TryEstimateWavRangeStartSeconds(rangeValue, buffer, out var seconds)
            ? $"{seconds:F3}s"
            : "n/a";

        _logger.LogDebug(
            "Chapter audio request range: chapter={ChapterName} buffer={BufferId} range={RangeHeader} approxWavStart={ApproxStart} queryStart={QueryStart} queryEnd={QueryEnd} sampleRate={SampleRate} channels={Channels}",
            chapterName,
            context.Descriptor.BufferId,
            rangeValue,
            approxSec,
            start,
            end,
            buffer.SampleRate,
            buffer.Channels);
    }

    private static bool TryEstimateWavRangeStartSeconds(string rangeHeader, AudioBuffer buffer, out double seconds)
    {
        seconds = 0;
        if (string.IsNullOrWhiteSpace(rangeHeader))
        {
            return false;
        }

        const int wavHeaderBytes = 44;
        const string prefix = "bytes=";
        if (!rangeHeader.StartsWith(prefix, StringComparison.OrdinalIgnoreCase))
        {
            return false;
        }

        var dashIndex = rangeHeader.IndexOf('-', prefix.Length);
        if (dashIndex < 0)
        {
            return false;
        }

        var startToken = rangeHeader.Substring(prefix.Length, dashIndex - prefix.Length);
        if (!long.TryParse(startToken, out var startBytes) || startBytes < 0)
        {
            return false;
        }

        var bytesPerSampleFrame = buffer.Channels * sizeof(short);
        if (buffer.SampleRate <= 0 || bytesPerSampleFrame <= 0)
        {
            return false;
        }

        var audioBytes = Math.Max(0, startBytes - wavHeaderBytes);
        seconds = audioBytes / (double)(buffer.SampleRate * bytesPerSampleFrame);
        return true;
    }

    private sealed record CorrectedPlaybackResolution(
        string RequestedChapter,
        string ActiveChapter,
        string ChapterId,
        string Source,
        AudioBufferContext Context);

    private bool TryResolveCorrectedPlayback(
        string chapterName,
        out CorrectedPlaybackResolution resolved,
        out IActionResult? failureResult)
    {
        resolved = null!;
        failureResult = null;

        if (_workspace.CurrentChapterHandle is null)
        {
            failureResult = NotFound("No chapter loaded");
            return false;
        }

        var requestedChapter = Uri.UnescapeDataString(chapterName ?? string.Empty).Trim();
        if (string.IsNullOrWhiteSpace(requestedChapter))
        {
            _logger.LogWarning(
                "Corrected playback request rejected due to blank chapter token. Raw token='{ChapterToken}'",
                chapterName);
            failureResult = BadRequest("Chapter name is required");
            return false;
        }

        var handle = _workspace.CurrentChapterHandle;
        var descriptor = handle.Chapter.Descriptor;
        var activeChapter = string.IsNullOrWhiteSpace(_workspace.CurrentChapterName)
            ? descriptor.ChapterId
            : _workspace.CurrentChapterName!.Trim();

        if (!MatchesRequestedChapter(requestedChapter, activeChapter, descriptor))
        {
            _logger.LogWarning(
                "Corrected playback request rejected for chapter '{RequestedChapter}'. Active chapter='{ActiveChapter}', stem='{ChapterId}'.",
                requestedChapter,
                activeChapter,
                descriptor.ChapterId);
            failureResult = NotFound($"Chapter '{requestedChapter}' is not active (active chapter '{activeChapter}').");
            return false;
        }

        var audio = handle.Chapter.Audio;
        AudioBufferContext? currentContext;
        try
        {
            currentContext = audio.Current;
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning(
                ex,
                "Corrected playback request failed because current audio context is unavailable for chapter '{RequestedChapter}'",
                requestedChapter);
            currentContext = null;
        }

        foreach (var candidate in new[] { ("corrected", audio.Corrected), ("treated", audio.Treated), ("current", currentContext) })
        {
            var context = candidate.Item2;
            if (!IsAudioContextAvailable(context))
            {
                continue;
            }

            resolved = new CorrectedPlaybackResolution(
                RequestedChapter: requestedChapter,
                ActiveChapter: activeChapter,
                ChapterId: descriptor.ChapterId,
                Source: candidate.Item1,
                Context: context!);

            _logger.LogDebug(
                "Resolved corrected playback source '{Source}' for chapter '{RequestedChapter}' (active '{ActiveChapter}', stem '{ChapterId}')",
                resolved.Source,
                resolved.RequestedChapter,
                resolved.ActiveChapter,
                resolved.ChapterId);
            return true;
        }

        _logger.LogWarning(
            "No corrected playback source available for chapter '{RequestedChapter}' (active '{ActiveChapter}', stem '{ChapterId}'). Checked corrected, treated, current.",
            requestedChapter,
            activeChapter,
            descriptor.ChapterId);
        failureResult = NotFound($"No audio available for chapter '{requestedChapter}' (checked corrected, treated, current).");
        return false;
    }

    private static bool IsAudioContextAvailable(AudioBufferContext? context)
    {
        return context is not null && System.IO.File.Exists(context.Descriptor.Path);
    }

    private static bool MatchesRequestedChapter(
        string requestedChapter,
        string activeChapter,
        ChapterDescriptor descriptor)
    {
        if (string.Equals(requestedChapter, activeChapter, StringComparison.OrdinalIgnoreCase)
            || string.Equals(requestedChapter, descriptor.ChapterId, StringComparison.OrdinalIgnoreCase))
        {
            return true;
        }

        foreach (var alias in descriptor.Aliases)
        {
            if (string.Equals(requestedChapter, alias, StringComparison.OrdinalIgnoreCase))
            {
                return true;
            }
        }

        return false;
    }
}
