using System;
using System.Threading;
using System.Threading.Tasks;
using Ams.Core.Asr;

namespace Ams.Core.Processors;

/// <summary>
/// Whisper.NET backed ASR primitives exposed as static helpers.
/// </summary>
public static class AsrProcessor
{
    public static Task<AsrResponse> TranscribeFileAsync(
        string audioPath,
        AsrOptions options,
        CancellationToken cancellationToken = default) =>
        throw new NotImplementedException("Pending Whisper.NET integration.");

    public static Task<AsrResponse> TranscribeBufferAsync(
        ReadOnlyMemory<float> monoAudio,
        AsrOptions options,
        CancellationToken cancellationToken = default) =>
        throw new NotImplementedException("Pending Whisper.NET integration.");

    public static Task<string> DetectLanguageAsync(
        string audioPath,
        AsrOptions options,
        CancellationToken cancellationToken = default) =>
        throw new NotImplementedException("Pending Whisper.NET integration.");
}

public sealed record AsrOptions(
    string ModelPath,
    string Language,
    int Threads = 0,
    bool UseGpu = true,
    bool EnableWordTimestamps = true,
    int BeamSize = 5,
    int BestOf = 1,
    float Temperature = 0.0f,
    bool NoSpeechBoost = true);
