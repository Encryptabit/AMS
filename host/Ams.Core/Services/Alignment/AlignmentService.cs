using Ams.Core.Artifacts;
using Ams.Core.Artifacts.Alignment;
using Ams.Core.Artifacts.Hydrate;
using Ams.Core.Runtime.Book;
using Ams.Core.Runtime.Chapter;
using Ams.Core.Services.Interfaces;

namespace Ams.Core.Services.Alignment;

/// <summary>
/// Facade service that delegates to focused alignment services.
/// Provides backward compatibility with the IAlignmentService contract.
/// </summary>
public sealed class AlignmentService : IAlignmentService
{
    private readonly IAnchorComputeService _anchorService;
    private readonly ITranscriptIndexService _transcriptService;
    private readonly ITranscriptHydrationService _hydrationService;

    public AlignmentService(
        IPronunciationProvider? pronunciationProvider = null,
        IAnchorComputeService? anchorService = null,
        ITranscriptIndexService? transcriptService = null,
        ITranscriptHydrationService? hydrationService = null)
    {
        var provider = pronunciationProvider ?? NullPronunciationProvider.Instance;
        _anchorService = anchorService ?? new AnchorComputeService();
        _transcriptService = transcriptService ?? new TranscriptIndexService(provider);
        _hydrationService = hydrationService ?? new TranscriptHydrationService();
    }

    public Task<AnchorDocument> ComputeAnchorsAsync(
        ChapterContext context,
        AnchorComputationOptions? options = null,
        CancellationToken cancellationToken = default)
    {
        return _anchorService.ComputeAnchorsAsync(context, options, cancellationToken);
    }

    public Task<TranscriptIndex> BuildTranscriptIndexAsync(
        ChapterContext context,
        TranscriptBuildOptions? options = null,
        CancellationToken cancellationToken = default)
    {
        return _transcriptService.BuildTranscriptIndexAsync(context, options, cancellationToken);
    }

    public Task<HydratedTranscript> HydrateTranscriptAsync(
        ChapterContext context,
        HydrationOptions? options = null,
        CancellationToken cancellationToken = default)
    {
        return _hydrationService.HydrateTranscriptAsync(context, options, cancellationToken);
    }
}
