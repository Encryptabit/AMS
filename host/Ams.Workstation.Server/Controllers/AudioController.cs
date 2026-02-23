using System;
using System.IO;
using Ams.Core.Processors;
using Ams.Workstation.Server.Services;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace Ams.Workstation.Server.Controllers;

/// <summary>
/// Serves audio files for the waveform player.
/// Streams audio from AudioBufferContext when a chapter is loaded.
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class AudioController : ControllerBase
{
    private readonly IWebHostEnvironment _environment;
    private readonly ILogger<AudioController> _logger;
    private readonly BlazorWorkspace _workspace;

    public AudioController(
        IWebHostEnvironment environment,
        ILogger<AudioController> logger,
        BlazorWorkspace workspace)
    {
        _environment = environment;
        _logger = logger;
        _workspace = workspace;
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
}
