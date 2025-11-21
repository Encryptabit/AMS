using Ams.Core.Runtime.Chapter;
using Ams.Core.Services.Alignment;
using Ams.Core.Services.Interfaces;

namespace Ams.Core.Application.Commands;

public sealed class BuildTranscriptIndexCommand
{
    private readonly IAlignmentService _alignmentService;

    public BuildTranscriptIndexCommand(IAlignmentService alignmentService)
    {
        _alignmentService = alignmentService ?? throw new ArgumentNullException(nameof(alignmentService));
    }

    public async Task ExecuteAsync(
        ChapterContext chapter,
        BuildTranscriptIndexOptions? options,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(chapter);

        var effective = options ?? BuildTranscriptIndexOptions.Default;
        var audioPath = (effective.AudioFile ?? ResolveAudioFile(chapter)).FullName;

        var asrFile = effective.AsrFile
                     ?? chapter.Documents.GetAsrFile()
                     ?? throw new InvalidOperationException("ASR artifact path could not be resolved.");

        var bookIndexFile = effective.BookIndexFile
                            ?? chapter.Book.Documents.GetBookIndexFile()
                            ?? throw new InvalidOperationException("Book index artifact path could not be resolved.");

        var asrPath = asrFile.FullName;
        var bookIndexPath = bookIndexFile.FullName;
        var anchorOptions = effective.AnchorOptions ?? new AnchorComputationOptions();

        var transcript = await _alignmentService.BuildTranscriptIndexAsync(
            chapter,
            new TranscriptBuildOptions
            {
                AudioPath = audioPath,
                ScriptPath = asrPath,
                BookIndexPath = bookIndexPath,
                AnchorOptions = anchorOptions
            },
            cancellationToken).ConfigureAwait(false);

        chapter.Documents.Transcript = transcript;
        chapter.Save();
    }

    private static FileInfo ResolveAudioFile(ChapterContext chapter)
    {
        var descriptor = chapter.Descriptor.AudioBuffers.FirstOrDefault()
                         ?? throw new InvalidOperationException("No audio buffers are registered for this chapter.");
        return new FileInfo(Path.GetFullPath(descriptor.Path));
    }
}

public sealed record BuildTranscriptIndexOptions
{
    public static BuildTranscriptIndexOptions Default { get; } = new();

    public FileInfo? AudioFile { get; init; }
    public FileInfo? AsrFile { get; init; }
    public FileInfo? BookIndexFile { get; init; }
    public AnchorComputationOptions? AnchorOptions { get; init; }
}
