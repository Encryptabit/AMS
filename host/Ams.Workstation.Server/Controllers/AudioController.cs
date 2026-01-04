using Microsoft.AspNetCore.Mvc;

namespace Ams.Workstation.Server.Controllers;

/// <summary>
/// Serves audio files for the waveform player.
/// Note: In Plan 4, this will integrate with Ams.Core for proper chapter audio paths.
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class AudioController : ControllerBase
{
    private readonly IWebHostEnvironment _environment;
    private readonly ILogger<AudioController> _logger;

    public AudioController(IWebHostEnvironment environment, ILogger<AudioController> logger)
    {
        _environment = environment;
        _logger = logger;
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
    /// Gets the audio URL for a chapter.
    /// In Plan 4, this will resolve the chapter name to its actual audio path via Ams.Core.
    /// </summary>
    [HttpGet("chapter/{chapterName}")]
    public IActionResult GetChapterAudio(string chapterName)
    {
        // TODO: In Plan 4, integrate with workspace to get actual chapter audio path
        // For now, return sample audio for testing
        var samplePath = Path.Combine(_environment.WebRootPath, "audio", "sample.wav");
        if (System.IO.File.Exists(samplePath))
        {
            return ServeAudioFile(samplePath);
        }

        return NotFound($"Audio for chapter '{chapterName}' not found (sample audio not configured)");
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
