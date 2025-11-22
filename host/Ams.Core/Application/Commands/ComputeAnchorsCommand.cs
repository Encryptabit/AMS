using Ams.Core.Runtime.Chapter;
using Ams.Core.Services.Alignment;
using Ams.Core.Services.Interfaces;

namespace Ams.Core.Application.Commands;

public sealed class ComputeAnchorsCommand
{
    private readonly IAlignmentService _alignmentService;

    public ComputeAnchorsCommand(IAlignmentService alignmentService)
    {
        _alignmentService = alignmentService ?? throw new ArgumentNullException(nameof(alignmentService));
    }

    public async Task ExecuteAsync(
        ChapterContext chapter,
        AnchorComputationOptions? options,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(chapter);

        var anchorOptions = options ?? new AnchorComputationOptions();
        var anchors = await _alignmentService
            .ComputeAnchorsAsync(chapter, anchorOptions, cancellationToken)
            .ConfigureAwait(false);

        chapter.Documents.Anchors = anchors;
        chapter.Save();
    }
}