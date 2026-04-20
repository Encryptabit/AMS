namespace Ams.Workstation.Server.Services.Prep;

public enum PrepRuntimeReadinessState
{
    Ready,
    Warning,
    Failed,
    Unknown
}

public enum PrepModelProvenanceKind
{
    PinnedPath,
    MissingExplicitModel,
    AliasOnly,
    MissingModelFile,
    InvalidModelInput
}

public sealed record PrepRuntimeDependencyReadiness(
    string Dependency,
    PrepRuntimeReadinessState State,
    string Summary,
    string? Detail = null,
    int? ExitCode = null,
    long? DurationMs = null);

public sealed record PrepRuntimeModelProvenance(
    PrepRuntimeReadinessState State,
    PrepModelProvenanceKind SourceKind,
    string? RequestedModel,
    string? NormalizedModelPath,
    bool IsDeterministic,
    string Summary,
    string Guidance);

public sealed record PrepRuntimeReadinessSnapshot
{
    public DateTimeOffset CapturedAtUtc { get; init; }

    public string? ChapterDisplayTitle { get; init; }

    public string? ChapterId { get; init; }

    public PrepRuntimeModelProvenance ModelProvenance { get; init; } = new(
        PrepRuntimeReadinessState.Unknown,
        PrepModelProvenanceKind.InvalidModelInput,
        null,
        null,
        IsDeterministic: false,
        Summary: "Model provenance was not captured.",
        Guidance: "Retry readiness probing before dispatching Prep.");

    public PrepRuntimeDependencyReadiness Ffmpeg { get; init; } = new(
        "FFmpeg",
        PrepRuntimeReadinessState.Unknown,
        "FFmpeg readiness was not captured.");

    public PrepRuntimeDependencyReadiness Mfa { get; init; } = new(
        "MFA",
        PrepRuntimeReadinessState.Unknown,
        "MFA readiness was not captured.");

    public bool IsReady { get; init; }

    public bool IsDeterministic { get; init; }

    public bool ReusedCachedProbe { get; init; }

    public IReadOnlyList<string> Notes { get; init; } = [];
}
