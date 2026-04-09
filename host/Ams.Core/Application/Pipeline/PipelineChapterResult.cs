using System.Linq;
using Ams.Core.Application.Commands;
using Ams.Core.Application.Runs;

namespace Ams.Core.Application.Pipeline;

public sealed record PipelineStageResult
{
    public PipelineStageResult(
        PipelineStage stage,
        RunState state,
        bool executed,
        string message,
        IReadOnlyList<RunArtifact>? artifacts = null,
        RunFailure? failure = null)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(message);

        if (state == RunState.Failed && failure is null)
        {
            throw new ArgumentException("Failed stage results must include a failure.", nameof(failure));
        }

        if (state != RunState.Failed && failure is not null)
        {
            throw new ArgumentException("Failures may only be attached to failed stage results.", nameof(failure));
        }

        Stage = stage;
        State = state;
        Executed = executed;
        Message = message;
        Artifacts = artifacts?.ToArray() ?? [];
        Failure = failure;
    }

    public PipelineStage Stage { get; }

    public RunState State { get; }

    public bool Executed { get; }

    public string Message { get; }

    public IReadOnlyList<RunArtifact> Artifacts { get; }

    public RunFailure? Failure { get; }

    public bool UsedCachedOutput => State == RunState.Completed && !Executed && Failure is null;

    public string StageName => PipelineRunContract.ToStageName(Stage);
}

public sealed record PipelineChapterResult
{
    public PipelineChapterResult(
        string chapterId,
        bool bookIndexBuilt,
        bool asrRan,
        bool anchorsRan,
        bool transcriptRan,
        bool hydrateRan,
        bool mfaRan,
        FileInfo bookIndexFile,
        FileInfo asrFile,
        FileInfo anchorFile,
        FileInfo transcriptFile,
        FileInfo hydrateFile,
        FileInfo textGridFile,
        FileInfo treatedAudioFile,
        bool promptlessAsrRecoveryRequested = false,
        BuildBookIndexResult? bookIndexResult = null,
        ModuleId? moduleId = null,
        RunState state = RunState.Completed,
        RunFailure? failure = null,
        IReadOnlyList<RunArtifact>? artifacts = null,
        IReadOnlyList<RunProgressUpdate>? progressUpdates = null,
        IReadOnlyList<PipelineStageResult>? stageResults = null)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(chapterId);
        ArgumentNullException.ThrowIfNull(bookIndexFile);
        ArgumentNullException.ThrowIfNull(asrFile);
        ArgumentNullException.ThrowIfNull(anchorFile);
        ArgumentNullException.ThrowIfNull(transcriptFile);
        ArgumentNullException.ThrowIfNull(hydrateFile);
        ArgumentNullException.ThrowIfNull(textGridFile);
        ArgumentNullException.ThrowIfNull(treatedAudioFile);

        if (state == RunState.Failed && failure is null)
        {
            throw new ArgumentException("Failed pipeline results must include a failure.", nameof(failure));
        }

        if (state != RunState.Failed && failure is not null)
        {
            throw new ArgumentException("Failures may only be attached to failed pipeline results.", nameof(failure));
        }

        ChapterId = chapterId;
        BookIndexBuilt = bookIndexBuilt;
        AsrRan = asrRan;
        AnchorsRan = anchorsRan;
        TranscriptRan = transcriptRan;
        HydrateRan = hydrateRan;
        MfaRan = mfaRan;
        BookIndexFile = bookIndexFile;
        AsrFile = asrFile;
        AnchorFile = anchorFile;
        TranscriptFile = transcriptFile;
        HydrateFile = hydrateFile;
        TextGridFile = textGridFile;
        TreatedAudioFile = treatedAudioFile;
        PromptlessAsrRecoveryRequested = promptlessAsrRecoveryRequested;
        BookIndexResult = bookIndexResult;
        ModuleId = moduleId ?? ModuleIds.PipelineRun;
        State = state;
        Failure = failure;
        Artifacts = artifacts?.ToArray() ?? [];
        ProgressUpdates = progressUpdates?.ToArray() ?? [];
        StageResults = stageResults?.ToArray() ?? [];
    }

    public string ChapterId { get; }

    public bool BookIndexBuilt { get; }

    public bool AsrRan { get; }

    public bool AnchorsRan { get; }

    public bool TranscriptRan { get; }

    public bool HydrateRan { get; }

    public bool MfaRan { get; }

    public FileInfo BookIndexFile { get; }

    public FileInfo AsrFile { get; }

    public FileInfo AnchorFile { get; }

    public FileInfo TranscriptFile { get; }

    public FileInfo HydrateFile { get; }

    public FileInfo TextGridFile { get; }

    public FileInfo TreatedAudioFile { get; }

    public bool PromptlessAsrRecoveryRequested { get; }

    public BuildBookIndexResult? BookIndexResult { get; }

    public ModuleId ModuleId { get; }

    public RunState State { get; }

    public RunFailure? Failure { get; }

    public IReadOnlyList<RunArtifact> Artifacts { get; }

    public IReadOnlyList<RunProgressUpdate> ProgressUpdates { get; }

    public IReadOnlyList<PipelineStageResult> StageResults { get; }

    public PipelineStageResult? GetStageResult(PipelineStage stage)
        => StageResults.FirstOrDefault(result => result.Stage == stage);
}

public sealed class PipelineRunException : Exception
{
    public PipelineRunException(PipelineChapterResult result, Exception? innerException = null)
        : base(result?.Failure?.Message ?? "Pipeline run failed.", innerException)
    {
        Result = result ?? throw new ArgumentNullException(nameof(result));

        if (result.State != RunState.Failed || result.Failure is null)
        {
            throw new ArgumentException("PipelineRunException requires a failed pipeline result.", nameof(result));
        }
    }

    public PipelineChapterResult Result { get; }

    public ModuleId ModuleId => Result.ModuleId;

    public RunFailure Failure => Result.Failure!;

    public IReadOnlyList<RunArtifact> Artifacts => Result.Artifacts;

    public IReadOnlyList<RunProgressUpdate> ProgressUpdates => Result.ProgressUpdates;

    public IReadOnlyList<PipelineStageResult> StageResults => Result.StageResults;
}

internal static class PipelineRunContract
{
    internal const string PipelineStageName = "pipeline";

    internal static string ToStageName(PipelineStage stage)
        => stage switch
        {
            PipelineStage.BookIndex => "book_index",
            PipelineStage.Asr => "asr",
            PipelineStage.Anchors => "anchors",
            PipelineStage.Transcript => "transcript",
            PipelineStage.Hydrate => "hydrate",
            PipelineStage.Mfa => "mfa",
            _ => PipelineStageName
        };

    internal static bool TryParseStage(string? stage, out PipelineStage pipelineStage)
    {
        switch (stage?.Trim().ToLowerInvariant())
        {
            case "book_index":
                pipelineStage = PipelineStage.BookIndex;
                return true;
            case "asr":
                pipelineStage = PipelineStage.Asr;
                return true;
            case "anchors":
                pipelineStage = PipelineStage.Anchors;
                return true;
            case "transcript":
                pipelineStage = PipelineStage.Transcript;
                return true;
            case "hydrate":
                pipelineStage = PipelineStage.Hydrate;
                return true;
            case "mfa":
                pipelineStage = PipelineStage.Mfa;
                return true;
            default:
                pipelineStage = PipelineStage.Pending;
                return false;
        }
    }

    internal static double StageProgressBefore(PipelineStage stage)
        => Math.Clamp(Math.Max((int)stage - 1, 0) / (double)(int)PipelineStage.Complete, 0d, 1d);

    internal static double StageProgress(PipelineStage stage)
        => Math.Clamp((int)stage / (double)(int)PipelineStage.Complete, 0d, 1d);

    internal static RunFailure CreateMissingArtifactFailure(PipelineStage stage, RunArtifact artifact)
    {
        ArgumentNullException.ThrowIfNull(artifact);

        return new RunFailure(
            RunFailureKind.Execution,
            $"{FormatStageLabel(stage)} output is missing: {artifact.Path}",
            ToStageName(stage));
    }

    internal static string DescribeStageStatus(PipelineStageResult? stageResult, string skippedMessage = "Skipped")
    {
        return stageResult?.Message ?? skippedMessage;
    }

    internal static string FormatStageLabel(PipelineStage stage)
        => stage switch
        {
            PipelineStage.BookIndex => "Book index",
            PipelineStage.Asr => "ASR",
            PipelineStage.Anchors => "Anchors",
            PipelineStage.Transcript => "Transcript",
            PipelineStage.Hydrate => "Hydrate",
            PipelineStage.Mfa => "MFA",
            PipelineStage.Complete => "Pipeline",
            _ => "Pipeline"
        };
}
