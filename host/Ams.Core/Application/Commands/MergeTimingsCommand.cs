using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Ams.Core.Common;
using Ams.Core.Processors.Alignment.Mfa;
using Ams.Core.Runtime.Chapter;

namespace Ams.Core.Application.Commands;

public sealed class MergeTimingsCommand
{
    public Task ExecuteAsync(
        ChapterContext chapter,
        MergeTimingsOptions? options,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(chapter);

        var textGridFile = options?.TextGridFile ?? ResolveTextGridFile(chapter, options);

        var updateHydrate = options?.ApplyToHydrate ?? true;
        var updateTranscript = options?.ApplyToTranscript ?? true;
        if (!updateHydrate && !updateTranscript)
        {
            Log.Debug("merge-timings invoked with both hydrate/transcript targets disabled; skipping.");
            return Task.CompletedTask;
        }

        MfaTimingMerger.MergeTimings(chapter, textGridFile, updateHydrate, updateTranscript);

        return Task.CompletedTask;
    }

    private static FileInfo ResolveTextGridFile(ChapterContext chapter, MergeTimingsOptions? options)
    {
        if (options?.TextGridFile is { } overrideFile)
        {
            return overrideFile;
        }

        var chapterRoot = chapter.Descriptor.RootPath ?? throw new InvalidOperationException("Chapter root path is not configured.");
        var alignmentRoot = options?.AlignmentRootDirectory ?? new DirectoryInfo(Path.Combine(chapterRoot, "alignment", "mfa"));
        return new FileInfo(Path.Combine(alignmentRoot.FullName, $"{chapter.Descriptor.ChapterId}.TextGrid"));
    }
}

public sealed record MergeTimingsOptions
{
    public FileInfo? HydrateFile { get; init; }
    public FileInfo? TranscriptFile { get; init; }
    public FileInfo? TextGridFile { get; init; }
    public DirectoryInfo? AlignmentRootDirectory { get; init; }
    public bool ApplyToHydrate { get; init; } = true;
    public bool ApplyToTranscript { get; init; } = true;
}
