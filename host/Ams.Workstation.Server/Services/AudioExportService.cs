using Ams.Core.Artifacts;
using Ams.Core.Processors;

namespace Ams.Workstation.Server.Services;

/// <summary>
/// Exports audio segments to WAV files in the CRX folder.
/// Uses AudioProcessor.Trim (FFmpeg atrim) for segment extraction.
/// </summary>
public class AudioExportService
{
    private readonly BlazorWorkspace _workspace;

    public AudioExportService(BlazorWorkspace workspace)
    {
        _workspace = workspace;
    }

    /// <summary>
    /// Export an audio segment to the CRX folder.
    /// Uses AudioProcessor.Trim (FFmpeg atrim) for segment extraction,
    /// then ToWavStream() on the trimmed buffer.
    /// </summary>
    public ExportResult ExportSegment(double startSec, double endSec, int paddingMs = 0)
    {
        if (_workspace.CurrentChapterHandle == null)
            throw new InvalidOperationException("No chapter loaded");

        var audioCtx = _workspace.CurrentChapterHandle.Chapter.Audio;
        if (audioCtx.Count == 0)
            throw new InvalidOperationException("No audio registered for chapter");

        var audioBuffer = audioCtx.Current.Buffer;
        if (audioBuffer == null)
            throw new InvalidOperationException("Audio buffer not loaded");

        // Trim segment using AudioProcessor (FFmpeg atrim filter)
        var endWithPadding = endSec + (paddingMs / 1000.0);
        var maxDuration = (double)audioBuffer.Length / audioBuffer.SampleRate;
        var trimmed = AudioProcessor.Trim(
            audioBuffer,
            TimeSpan.FromSeconds(Math.Max(0, startSec)),
            TimeSpan.FromSeconds(Math.Min(endWithPadding, maxDuration)));

        // Create CRX folder
        var crxFolder = Path.Combine(_workspace.RootPath, "CRX");
        Directory.CreateDirectory(crxFolder);

        // Determine next error number
        var existingFiles = Directory.GetFiles(crxFolder, "*.wav");
        var nextNumber = 1;
        foreach (var file in existingFiles)
        {
            var name = Path.GetFileNameWithoutExtension(file);
            if (int.TryParse(name, out var num) && num >= nextNumber)
                nextNumber = num + 1;
        }

        // Export trimmed segment to WAV file
        var filename = $"{nextNumber:D3}.wav";
        var outputPath = Path.Combine(crxFolder, filename);

        using var wavStream = trimmed.ToWavStream();
        using var outputStream = File.Create(outputPath);
        wavStream.CopyTo(outputStream);

        return new ExportResult(true, filename, nextNumber, outputPath);
    }
}

public record ExportResult(bool Success, string Filename, int ErrorNumber, string Path);
