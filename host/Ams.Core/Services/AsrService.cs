using System.Threading;
using System.Threading.Tasks;
using Ams.Core.Asr;
using Ams.Core.Processors;

namespace Ams.Core.Services;

/// <summary>
/// Orchestrates Whisper.NET transcription using the shared processor.
/// </summary>
public sealed class AsrService
{
    public Task<AsrResponse> TranscribeFileAsync(
        string audioPath,
        AsrOptions options,
        CancellationToken cancellationToken = default) =>
        AsrProcessor.TranscribeFileAsync(audioPath, options, cancellationToken);
}
