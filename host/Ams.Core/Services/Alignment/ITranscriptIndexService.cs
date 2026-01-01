using Ams.Core.Artifacts;
using Ams.Core.Artifacts.Alignment;
using Ams.Core.Runtime.Chapter;

namespace Ams.Core.Services.Alignment;

/// <summary>
/// Service for building transcript index documents from book text and ASR output.
/// </summary>
public interface ITranscriptIndexService
{
    /// <summary>
    /// Builds a transcript index by aligning book text with ASR transcript.
    /// </summary>
    /// <param name="context">The chapter context containing book and ASR documents.</param>
    /// <param name="options">Optional build options.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>A transcript index containing word alignments and rollups.</returns>
    Task<TranscriptIndex> BuildTranscriptIndexAsync(
        ChapterContext context,
        TranscriptBuildOptions? options = null,
        CancellationToken cancellationToken = default);
}
