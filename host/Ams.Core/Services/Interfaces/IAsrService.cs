using Ams.Core.Artifacts;
using Ams.Core.Processors;
using Ams.Core.Runtime.Chapter;

namespace Ams.Core.Services.Interfaces;

public interface IAsrService
{
    Task<AsrResponse> TranscribeAsync(
        ChapterContext chapter,
        AsrOptions options,
        CancellationToken cancellationToken = default);

    // Re-transcribes only the chunks at the given indices and splices the new tokens/segments
    // into the existing AsrResponse on the chapter. The chunk plan must be valid for the
    // current audio (the caller must catch the InvalidOperationException and fall back to a
    // full chapter re-transcribe). Tokens and segments outside the patched chunks' time ranges
    // are preserved verbatim. Returns the merged response; does NOT persist.
    Task<AsrResponse> TranscribeChunksAsync(
        ChapterContext chapter,
        IReadOnlyList<int> chunkIndices,
        AsrOptions options,
        CancellationToken cancellationToken = default);

    AudioBuffer ResolveAsrReadyBuffer(ChapterContext chapter);
}