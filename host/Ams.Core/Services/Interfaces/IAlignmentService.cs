using Ams.Core.Artifacts;
using Ams.Core.Artifacts.Alignment;
using Ams.Core.Artifacts.Hydrate;
using Ams.Core.Runtime.Chapter;
using Ams.Core.Services.Alignment;

namespace Ams.Core.Services.Interfaces;

public interface IAlignmentService
{
    Task<AnchorDocument> ComputeAnchorsAsync(
        ChapterContext context,
        AnchorComputationOptions? options = null,
        CancellationToken cancellationToken = default);

    Task<TranscriptIndex> BuildTranscriptIndexAsync(
        ChapterContext context,
        TranscriptBuildOptions? options = null,
        CancellationToken cancellationToken = default);

    Task<HydratedTranscript> HydrateTranscriptAsync(
        ChapterContext context,
        HydrationOptions? options = null,
        CancellationToken cancellationToken = default);
}
