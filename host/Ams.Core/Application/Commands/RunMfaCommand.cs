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

        var outcome = await MfaWorkflow.RunChapterAsync(
                chapter,
                audioFile,
                hydrateFile,
                chapterStem,
                chapterDirectory,
                cancellationToken,
                useDedicatedProcess: options?.UseDedicatedProcess ?? false,
                workspaceRoot: options?.WorkspaceRoot,
                beamSettings: beamSettings,
                disableChunkedMfa: options?.DisableChunkedMfa ?? false,
                requireAsrChunkAudio: options?.RequireAsrChunkAudio ?? false,
                maxConsecutiveDelRun: options?.MaxConsecutiveDelRun ?? 3)
            .ConfigureAwait(false);

        var alignmentRoot =
            options?.AlignmentRootDirectory ?? new DirectoryInfo(Path.Combine(chapterRoot, "alignment"));
        var textGridFile = options?.TextGridFile
                           ?? chapter.Documents.GetTextGridFile()
                           ?? new FileInfo(Path.Combine(alignmentRoot.FullName, "mfa", $"{chapterStem}.TextGrid"));

        chapter.Documents.InvalidateTextGrid();

        return new RunMfaResult(textGridFile, outcome.ProblematicChunkIndices);
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

    /// <summary>
    /// When true, chunked MFA requires pre-sliced chunk audio emitted by ASR.
    /// If chunk-audio artifacts are missing or incompatible, MFA fails instead
    /// of silently regenerating chunk slices.
    /// </summary>
    public bool RequireAsrChunkAudio { get; init; }

    /// <summary>
    /// Maximum length of a consecutive Del-op book-word run that the lab builder
    /// will still splice back into the MFA corpus using book canonical text. Runs
    /// longer than this are dropped so MFA does not try to align audio against
    /// passages the narrator skipped.
    /// <para>
    /// Default <c>3</c> covers chapter-heading drops ("Chapter" missing) and small
    /// narrator slips while leaving room to skip whole missed sentences. Set to
    /// <c>0</c> for legacy behavior (drop every Del). Set to <see cref="int.MaxValue"/>
    /// to always include — this is unsafe when narrators omit sentences.
    /// </para>
    /// </summary>
    public int MaxConsecutiveDelRun { get; init; } = 3;
}

public sealed record RunMfaResult(FileInfo TextGridFile, IReadOnlyList<int> ProblematicChunkIndices)
{
    public RunMfaResult(FileInfo textGridFile)
        : this(textGridFile, Array.Empty<int>()) { }
}
