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

    AudioBuffer ResolveAsrReadyBuffer(ChapterContext chapter);
}