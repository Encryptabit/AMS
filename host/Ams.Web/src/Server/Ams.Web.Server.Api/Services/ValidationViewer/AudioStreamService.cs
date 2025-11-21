using Ams.Core.Artifacts;
using Ams.Core.Processors;
using Ams.Core.Runtime.Chapter;

namespace Ams.Web.Server.Api.Services.ValidationViewer;

internal sealed class AudioStreamService : IAudioStreamService
{
    private readonly ValidationViewerWorkspaceState _state;
    private readonly ILogger<AudioStreamService> _logger;

    private readonly WorkspaceResolver _resolver;

    public AudioStreamService(ValidationViewerWorkspaceState state, WorkspaceResolver resolver, ILogger<AudioStreamService> logger)
    {
        _state = state;
        _resolver = resolver;
        _logger = logger;
    }

    public AudioBuffer? LoadBuffer(string bookId, string chapterId, string variant)
    {
        variant = string.IsNullOrWhiteSpace(variant) ? "raw" : variant.ToLowerInvariant();

        using var handle = _resolver.OpenChapter(bookId, chapterId);
        if (handle is null)
        {
            return null;
        }

        var chapter = handle.Chapter;
        try
        {
            var bufferId = variant switch
            {
                "treated" => "treated",
                "filtered" => "filtered",
                _ => "raw"
            };

            var audioCtx = chapter.Audio.Load(bufferId);
            var buffer = audioCtx.Buffer;
            if (buffer is not null)
            {
                return buffer;
            }
        }
        catch
        {
            // ignore and return null below
        }

        return null;
    }

    public AudioBuffer Slice(AudioBuffer buffer, double? startSec, double? endSec)
    {
        if (startSec is null || endSec is null || endSec <= startSec)
        {
            return buffer;
        }

        var startSample = (int)Math.Clamp(startSec.Value * buffer.SampleRate, 0, buffer.Length);
        var endSample = (int)Math.Clamp(endSec.Value * buffer.SampleRate, startSample, buffer.Length);
        var length = endSample - startSample;
        if (length <= 0)
        {
            return buffer;
        }

        var slice = new AudioBuffer(buffer.Channels, buffer.SampleRate, length, buffer.Metadata);
        for (var ch = 0; ch < buffer.Channels; ch++)
        {
            Array.Copy(buffer.Planar[ch], startSample, slice.Planar[ch], 0, length);
        }

        return slice;
    }

    public Stream ToWavStream(AudioBuffer buffer)
        => buffer.ToWavStream();

    #region helpers

    private DirectoryInfo GetBookRoot(string bookId) => _resolver.ResolveBookRoot(bookId);

    private FileInfo? GetBookIndex(string bookId) => _resolver.ResolveBookIndex(bookId);

    #endregion
}
