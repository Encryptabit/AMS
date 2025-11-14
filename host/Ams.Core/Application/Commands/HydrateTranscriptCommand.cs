using Ams.Core.Runtime.Chapter;
using Ams.Core.Services.Alignment;

namespace Ams.Core.Application.Commands;

public sealed class HydrateTranscriptCommand
{
    private readonly IAlignmentService _alignmentService;

    public HydrateTranscriptCommand(IAlignmentService alignmentService)
    {
        _alignmentService = alignmentService ?? throw new ArgumentNullException(nameof(alignmentService));
    }

    public async Task ExecuteAsync(
        ChapterContext chapter,
        HydrationOptions? options,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(chapter);

        await _alignmentService
            .HydrateTranscriptAsync(chapter, options, cancellationToken)
            .ConfigureAwait(false);

        chapter.Save();
    }
}
