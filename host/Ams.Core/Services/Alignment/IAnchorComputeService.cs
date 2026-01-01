using Ams.Core.Artifacts.Alignment;
using Ams.Core.Runtime.Chapter;

namespace Ams.Core.Services.Alignment;

/// <summary>
/// Service for computing anchor points between book text and ASR output.
/// </summary>
public interface IAnchorComputeService
{
    /// <summary>
    /// Computes anchor points between book text and ASR transcript.
    /// </summary>
    /// <param name="context">The chapter context containing book and ASR documents.</param>
    /// <param name="options">Optional computation options.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>An anchor document containing computed anchors.</returns>
    Task<AnchorDocument> ComputeAnchorsAsync(
        ChapterContext context,
        AnchorComputationOptions? options = null,
        CancellationToken cancellationToken = default);
}
