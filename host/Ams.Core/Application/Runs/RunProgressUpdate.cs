using System.Text.Json.Serialization;

namespace Ams.Core.Application.Runs;

public sealed record RunProgressUpdate
{
    [JsonConstructor]
    public RunProgressUpdate(
        ModuleId moduleId,
        RunState state,
        string? stage,
        string message,
        double? progress,
        RunFailure? failure,
        IReadOnlyList<RunArtifact>? artifacts,
        string? itemId = null)
    {
        ArgumentNullException.ThrowIfNull(moduleId);
        ArgumentException.ThrowIfNullOrWhiteSpace(message);

        if (stage is not null && string.IsNullOrWhiteSpace(stage))
        {
            throw new ArgumentException("Stage cannot be blank when provided.", nameof(stage));
        }

        if (itemId is not null && string.IsNullOrWhiteSpace(itemId))
        {
            throw new ArgumentException("ItemId cannot be blank when provided.", nameof(itemId));
        }

        if (progress is < 0 or > 1)
        {
            throw new ArgumentOutOfRangeException(nameof(progress), "Progress must be between 0.0 and 1.0.");
        }

        if (state == RunState.Failed && failure is null)
        {
            throw new ArgumentException("Failed updates must include a failure.", nameof(failure));
        }

        if (state != RunState.Failed && failure is not null)
        {
            throw new ArgumentException("Failures may only be attached to failed updates.", nameof(failure));
        }

        ModuleId = moduleId;
        State = state;
        Stage = stage;
        Message = message;
        Progress = progress;
        Failure = failure;
        Artifacts = artifacts?.ToArray() ?? [];
        ItemId = itemId;
    }

    public ModuleId ModuleId { get; }

    public RunState State { get; }

    public string? Stage { get; }

    public string Message { get; }

    public double? Progress { get; }

    public RunFailure? Failure { get; }

    public IReadOnlyList<RunArtifact> Artifacts { get; }

    /// <summary>
    /// Host-defined item identifier associated with this update. Pipeline execution uses
    /// the chapter id here so UIs can render multi-item progress without inventing a
    /// second transport-specific key.
    /// </summary>
    public string? ItemId { get; }

    public static RunProgressUpdate CreateStatus(
        ModuleId moduleId,
        RunState state,
        string message,
        double? progress = null,
        string? stage = null,
        IReadOnlyList<RunArtifact>? artifacts = null,
        string? itemId = null)
    {
        if (state == RunState.Failed)
        {
            throw new ArgumentException("Use CreateFailure for failed progress updates.", nameof(state));
        }

        return new RunProgressUpdate(moduleId, state, stage, message, progress, failure: null, artifacts, itemId);
    }

    public static RunProgressUpdate CreateFailure(
        ModuleId moduleId,
        RunFailure failure,
        string? message = null,
        double? progress = null,
        IReadOnlyList<RunArtifact>? artifacts = null,
        string? itemId = null)
    {
        ArgumentNullException.ThrowIfNull(failure);

        return new RunProgressUpdate(
            moduleId,
            RunState.Failed,
            failure.Stage,
            string.IsNullOrWhiteSpace(message) ? failure.Message : message,
            progress,
            failure,
            artifacts,
            itemId);
    }
}
