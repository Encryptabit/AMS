using Ams.Core.Application.Mfa;
using Ams.Core.Application.Mfa.Models;
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

        var chapterRoot = chapter.Descriptor.RootPath ??
                          throw new InvalidOperationException("Chapter root path is not configured.");
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

        var beamSettings = MfaBeamSettings.Resolve(
            options?.BeamProfile,
            options?.Beam,
            options?.RetryBeam);

        await MfaWorkflow.RunChapterAsync(
                chapter,
                audioFile,
                hydrateFile,
                chapterStem,
                chapterDirectory,
                cancellationToken,
                useDedicatedProcess: options?.UseDedicatedProcess ?? false,
                workspaceRoot: options?.WorkspaceRoot,
                beamSettings: beamSettings,
                disableChunkedMfa: options?.DisableChunkedMfa ?? false)
            .ConfigureAwait(false);

        var alignmentRoot =
            options?.AlignmentRootDirectory ?? new DirectoryInfo(Path.Combine(chapterRoot, "alignment"));
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
    public bool UseDedicatedProcess { get; init; }
    public string? WorkspaceRoot { get; init; }

    /// <summary>
    /// Beam search profile preset. When null, defaults to <see cref="MfaBeamProfile.Balanced"/>.
    /// Explicit <see cref="Beam"/>/<see cref="RetryBeam"/> values override profile defaults.
    /// </summary>
    public MfaBeamProfile? BeamProfile { get; init; }

    /// <summary>Explicit beam width override (supersedes profile default).</summary>
    public int? Beam { get; init; }

    /// <summary>Explicit retry beam width override (supersedes profile default).</summary>
    public int? RetryBeam { get; init; }

    /// <summary>
    /// When true, forces MFA to use the legacy single-utterance corpus path even
    /// when a shared chunk plan exists. Use for rollout control to isolate MFA
    /// chunking behavior independently from ASR chunk planning.
    /// </summary>
    public bool DisableChunkedMfa { get; init; }
}

public sealed record RunMfaResult(FileInfo TextGridFile);