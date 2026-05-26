using Ams.Core.Artifacts;
using Ams.Core.Processors;

namespace Ams.Workstation.Server.Services;

/// <summary>
/// Exports audio segments to WAV files in the CRX folder.
/// Uses zero-copy AudioBuffer slices for segment extraction.
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
    /// Uses a clamped AudioBuffer slice for segment extraction,
    /// then encodes the segment buffer directly to the destination WAV.
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

        var endWithPadding = endSec + (paddingMs / 1000.0);
        var segment = audioBuffer.SliceClamped(
            TimeSpan.FromSeconds(startSec),
            TimeSpan.FromSeconds(endWithPadding));

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

        // Export segment to WAV file
        var filename = $"{nextNumber:D3}.wav";
        var outputPath = Path.Combine(crxFolder, filename);

        AudioProcessor.EncodeWav(outputPath, segment);

        return new ExportResult(true, filename, nextNumber, outputPath);
    }
}

public record ExportResult(bool Success, string Filename, int ErrorNumber, string Path);
