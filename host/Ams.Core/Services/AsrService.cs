using System;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Ams.Core.Artifacts;
using Ams.Core.Asr;
using Ams.Core.Processors;
using Ams.Core.Runtime.Chapter;
using Ams.Core.Services.Integrations.FFmpeg;
using Ams.Core.Runtime.Audio;

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
        return PrepareForAsr(buffer);
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

    private static AudioBuffer PrepareForAsr(AudioBuffer buffer)
    {
        if (buffer.Channels == 1)
        {
            return buffer;
        }

        return FfFilterGraph
            .FromBuffer(buffer)
            .Custom(BuildMonoPanClause(buffer.Channels))
            .ToBuffer();
    }

    private static string BuildMonoPanClause(int channels)
    {
        if (channels <= 1)
        {
            return "pan=mono|c0=c0";
        }

        var weight = 1.0 / channels;
        var builder = new StringBuilder();
        for (var ch = 0; ch < channels; ch++)
        {
            if (ch > 0)
            {
                builder.Append('+');
            }

            builder.Append(FormattableString.Invariant($"{weight:F6}*c{ch}"));
        }

        return $"pan=mono|c0={builder}";
    }
}
