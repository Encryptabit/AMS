using Ams.Core.Artifacts.Hydrate;
using Ams.Core.Runtime.Chapter;

namespace Ams.Core.Services.Alignment;

/// <summary>
/// Service for hydrating transcript index into a fully enriched transcript with diffs and metrics.
/// </summary>
public interface ITranscriptHydrationService
{
    /// <summary>
    /// Hydrates a transcript index by computing diffs, metrics, and building enriched sentence/paragraph data.
    /// </summary>
    /// <param name="context">The chapter context containing transcript index and related documents.</param>
    /// <param name="options">Optional hydration options.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>A fully hydrated transcript with computed metrics and diffs.</returns>
    Task<HydratedTranscript> HydrateTranscriptAsync(
        ChapterContext context,
        HydrationOptions? options = null,
        CancellationToken cancellationToken = default);
}
