using Ams.Core.Application.Mfa;
using Ams.Core.Runtime.Chapter;

namespace Ams.Core.Application.Commands;

public sealed class RunMfaCommand
{
    public async Task<RunMfaResult> ExecuteAsync(
        ChapterContext chapter,
        RunMfaOptions? options,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(chapter);

        var chapterRoot = chapter.Descriptor.RootPath ?? throw new InvalidOperationException("Chapter root path is not configured.");
        var chapterStem = chapter.Descriptor.ChapterId;

        var chapterDirectory = options?.ChapterDirectory ?? new DirectoryInfo(chapterRoot);
        var hydrateFile = options?.HydrateFile
                          ?? chapter.Documents.GetHydratedTranscriptFile()
                          ?? throw new InvalidOperationException("Hydrated transcript artifact path is not available.");
        var audioFile = options?.AudioFile ?? ResolveAudioFile(chapter, options);

        if (!hydrateFile.Exists)
        {
            throw new FileNotFoundException("Hydrated transcript not found.", hydrateFile.FullName);
        }

        if (!audioFile.Exists)
        {
            throw new FileNotFoundException("Audio file not found.", audioFile.FullName);
        }

        await MfaWorkflow.RunChapterAsync(chapter, audioFile, hydrateFile, chapterStem, chapterDirectory, cancellationToken)
            .ConfigureAwait(false);

        var alignmentRoot = options?.AlignmentRootDirectory ?? new DirectoryInfo(Path.Combine(chapterRoot, "alignment"));
        var textGridFile = options?.TextGridFile
                             ?? chapter.Documents.GetTextGridFile()
                             ?? new FileInfo(Path.Combine(alignmentRoot.FullName, "mfa", $"{chapterStem}.TextGrid"));

        chapter.Documents.InvalidateTextGrid();

        return new RunMfaResult(textGridFile);
    }

    private static FileInfo ResolveAudioFile(ChapterContext chapter, RunMfaOptions? options)
    {
        if (options?.AudioFile is { } audio)
        {
            return audio;
        }

        var descriptor = chapter.Descriptor.AudioBuffers.FirstOrDefault();
        if (descriptor is null)
        {
            throw new InvalidOperationException("No audio buffers are registered for this chapter.");
        }

        return new FileInfo(Path.GetFullPath(descriptor.Path));
    }
}

public sealed record RunMfaOptions
{
    public FileInfo? AudioFile { get; init; }
    public FileInfo? HydrateFile { get; init; }
    public FileInfo? TextGridFile { get; init; }
    public DirectoryInfo? AlignmentRootDirectory { get; init; }
    public DirectoryInfo? ChapterDirectory { get; init; }
}

public sealed record RunMfaResult(FileInfo TextGridFile);
