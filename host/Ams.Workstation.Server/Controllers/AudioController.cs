using System;
using System.IO;
using Ams.Core.Artifacts;
using Ams.Core.Audio;
using Ams.Core.Processors;
using Ams.Core.Runtime.Audio;
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
    /// Gets the audio for a chapter from the workspace's AudioBufferContext.
    /// Streams WAV data from the loaded AudioBuffer.
    /// When start/end query params are provided, trims to that segment.
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
            var buffer = audioContext.Buffer;

            if (buffer is null)
            {
                _logger.LogWarning("Audio buffer not available for chapter '{ChapterName}'", chapterName);
                return NotFound("Audio buffer not available");
            }

            LogRangeDiagnostics(chapterName, buffer, start, end);

            // If start/end provided, trim to segment
            if (start.HasValue && end.HasValue)
            {
                var maxDuration = (double)buffer.Length / buffer.SampleRate;
                var trimmed = Ams.Core.Processors.AudioProcessor.Trim(
                    buffer,
                    TimeSpan.FromSeconds(Math.Max(0, start.Value)),
                    TimeSpan.FromSeconds(Math.Min(end.Value, maxDuration)));

                var trimStream = trimmed.ToWavStream();
                return File(trimStream, "audio/wav", enableRangeProcessing: true);
            }

            _logger.LogDebug(
                "Streaming audio for chapter '{ChapterName}': {Channels}ch, {SampleRate}Hz, {Length} samples",
                chapterName, buffer.Channels, buffer.SampleRate, buffer.Length);

            var stream = buffer.ToWavStream();
            return File(stream, "audio/wav", enableRangeProcessing: true);
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning(ex, "No audio buffers registered for chapter '{ChapterName}'", chapterName);
            return NotFound($"No audio buffers available for chapter '{chapterName}'");
        }
    }

    /// <summary>
    /// Returns precomputed min/max waveform peaks for the current chapter audio.
    /// Optional start/end values trim the chapter before peak extraction.
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
            var segment = TrimBuffer(buffer, start, end);
            return segment is null
                ? BadRequest("Invalid chapter peak range")
                : Ok(BuildWaveformPeaksPayload(segment, pxPerSec, $"chapter '{chapterName}' range"));
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
    /// Uses the workspace to resolve the chapter's audio file path and decodes directly from disk
    /// with start/duration parameters for memory-efficient partial loading.
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

        if (!_workspace.IsInitialized || string.IsNullOrEmpty(_workspace.WorkingDirectory))
        {
            return NotFound("Workspace not initialized");
        }

        try
        {
            // Resolve chapter name to stem and audio file path
            var stem = _workspace.GetStemForChapter(chapterName) ?? chapterName;
            var audioPath = Path.Combine(_workspace.WorkingDirectory, $"{stem}.wav");

            if (!System.IO.File.Exists(audioPath))
            {
                // Try treated audio path
                var treatedPath = Path.Combine(_workspace.WorkingDirectory, stem, $"{stem}.treated.wav");
                if (System.IO.File.Exists(treatedPath))
                {
                    audioPath = treatedPath;
                }
                else
                {
                    _logger.LogWarning("Audio file not found for chapter '{ChapterName}' at '{Path}'", chapterName, audioPath);
                    return NotFound($"Audio file not found for chapter '{chapterName}'");
                }
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

            var stream = buffer.ToWavStream();
            return File(stream, "audio/wav", enableRangeProcessing: true);
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

        var stream = buffer.ToWavStream();
        return File(stream, "audio/wav", enableRangeProcessing: true);
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
    /// Serves the corrected chapter audio from disk.
    /// Falls back to treated.wav, then raw audio if corrected.wav does not exist.
    /// </summary>
    [HttpGet("chapter/{chapterName}/corrected")]
    public IActionResult GetCorrectedChapterAudio(string chapterName)
    {
        if (_workspace.CurrentChapterHandle is null)
        {
            return NotFound("No chapter loaded");
        }

        var descriptor = _workspace.CurrentChapterHandle.Chapter.Descriptor;
        var correctedPath = Path.Combine(descriptor.RootPath, $"{descriptor.ChapterId}.corrected.wav");

        if (System.IO.File.Exists(correctedPath))
        {
            _logger.LogDebug("Serving corrected audio for chapter '{ChapterName}'", chapterName);
            return ServeAudioFile(correctedPath);
        }

        var treatedPath = Path.Combine(descriptor.RootPath, $"{descriptor.ChapterId}.treated.wav");
        if (System.IO.File.Exists(treatedPath))
        {
            _logger.LogDebug("Corrected not found, falling back to treated for chapter '{ChapterName}'", chapterName);
            return ServeAudioFile(treatedPath);
        }

        // Fall back to raw buffer
        try
        {
            var buffer = _workspace.CurrentChapterHandle.Chapter.Audio.Current.Buffer;
            if (buffer is null)
                return NotFound("No audio available");

            var stream = buffer.ToWavStream();
            return File(stream, "audio/wav", enableRangeProcessing: true);
        }
        catch (InvalidOperationException)
        {
            return NotFound("No audio available");
        }
    }

    /// <summary>
    /// Returns precomputed min/max waveform peaks for corrected chapter audio.
    /// Falls back to treated/current audio using the same resolution logic as playback.
    /// </summary>
    [HttpGet("chapter/{chapterName}/corrected/peaks")]
    public IActionResult GetCorrectedChapterPeaks(string chapterName, [FromQuery] int pxPerSec = DefaultPeakPxPerSec)
    {
        if (_workspace.CurrentChapterHandle is null)
        {
            return NotFound("No chapter loaded");
        }

        var audioContext = ResolveCorrectedPlaybackContext();
        var buffer = audioContext?.Buffer;
        if (audioContext is null || buffer is null)
        {
            return NotFound("No audio available");
        }

        var bucketCount = ResolveWaveformBucketCount(buffer, pxPerSec, $"corrected chapter '{chapterName}'");
        var peaks = audioContext.GetOrCreateWaveformPeaks(bucketCount);
        if (peaks is null)
        {
            return NotFound("No audio available");
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

    private void LogRangeDiagnostics(string chapterName, AudioBuffer buffer, double? start, double? end)
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
            _workspace.CurrentChapterHandle?.Chapter.Audio.Current.Descriptor.BufferId,
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

    private static AudioBuffer? TrimBuffer(AudioBuffer buffer, double? start, double? end)
    {
        var maxDuration = buffer.SampleRate > 0
            ? buffer.Length / (double)buffer.SampleRate
            : 0d;

        var clampedStart = Math.Max(0d, start ?? 0d);
        var clampedEnd = end.HasValue ? Math.Min(end.Value, maxDuration) : maxDuration;
        if (clampedEnd <= clampedStart)
        {
            return null;
        }

        return AudioProcessor.Trim(
            buffer,
            TimeSpan.FromSeconds(clampedStart),
            TimeSpan.FromSeconds(clampedEnd));
    }

    private AudioBufferContext? ResolveCorrectedPlaybackContext()
    {
        if (_workspace.CurrentChapterHandle is null)
        {
            return null;
        }

        var audio = _workspace.CurrentChapterHandle.Chapter.Audio;
        foreach (var candidate in new[] { audio.Corrected, audio.Treated, audio.Current })
        {
            if (candidate?.Buffer is not null)
            {
                return candidate;
            }
        }

        return null;
    }
}
