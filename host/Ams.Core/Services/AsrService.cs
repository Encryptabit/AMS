using Ams.Core.Artifacts;
using Ams.Core.Audio;
using Ams.Core.Processors;
using Ams.Core.Runtime.Audio;
using Ams.Core.Runtime.Chapter;
using Ams.Core.Services.Interfaces;

namespace Ams.Core.Services;

/// <summary>
/// Orchestrates Whisper.NET transcription using the shared processor.
/// </summary>
public sealed class AsrService : IAsrService
{
    public Task<AsrResponse> TranscribeAsync(
        ChapterContext chapter,
        AsrOptions options,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(chapter);
        var buffer = ResolveAsrReadyBuffer(chapter);
        return AsrProcessor.TranscribeBufferAsync(buffer, options, cancellationToken);
    }

    public AudioBuffer ResolveAsrReadyBuffer(ChapterContext chapter)
    {
        ArgumentNullException.ThrowIfNull(chapter);
        var bufferContext = ResolveAudioBufferContext(chapter);
        var buffer = bufferContext.Buffer ?? throw new InvalidOperationException("Audio buffer could not be loaded.");
        return AsrAudioPreparer.PrepareForAsr(buffer);
    }

    private static AudioBufferContext ResolveAudioBufferContext(ChapterContext chapter)
    {
        if (chapter.Descriptor.AudioBuffers.Count == 0)
        {
            throw new InvalidOperationException("No audio buffers are registered for this chapter.");
        }

        var descriptor = chapter.Descriptor.AudioBuffers[0];
        return chapter.Audio.Load(descriptor.BufferId);
    }
}