using Ams.Core.Application.Commands;
using Ams.Core.Services.Alignment;

namespace Ams.Core.Application.Pipeline;

public sealed record PipelineRunOptions
{
    public FileInfo BookFile { get; init; } = null!;
    public FileInfo BookIndexFile { get; init; } = null!;
    public FileInfo AudioFile { get; init; } = null!;
    public DirectoryInfo? ChapterDirectory { get; init; }
    public string ChapterId { get; init; } = string.Empty;
    public bool Force { get; init; }
    public bool ForceIndex { get; init; }
    public PipelineStage StartStage { get; init; } = PipelineStage.BookIndex;
    public PipelineStage EndStage { get; init; } = PipelineStage.Mfa;
    public double AverageWordsPerMinute { get; init; } = 155.0;
    public GenerateTranscriptOptions? TranscriptOptions { get; init; }
    public AnchorComputationOptions? AnchorOptions { get; init; }
    public BuildTranscriptIndexOptions? TranscriptIndexOptions { get; init; }
    public HydrationOptions? HydrationOptions { get; init; }
    public RunMfaOptions? MfaOptions { get; init; }
    public MergeTimingsOptions? MergeOptions { get; init; }
    public bool SkipTreatedCopy { get; init; }
    public FileInfo? TreatedCopyFile { get; init; }
    public PipelineConcurrencyControl? Concurrency { get; init; }

    /// <summary>
    /// Optional chunk planning policy for ASR and MFA stages. When null, stages
    /// use <see cref="ChunkPlanningPolicy.Default"/>. Explicit policy enables
    /// callers to override silence threshold, minimum silence duration, or minimum
    /// chunk duration for specific pipeline runs.
    /// </summary>
    /// <remarks>
    /// Invalidation rule: a stored chunk plan should be regenerated when the source
    /// audio fingerprint or the chunk policy differs from the stored plan. Use
    /// <see cref="ChunkPlanningService.IsValid"/> to check before recomputing.
    /// </remarks>
    public ChunkPlanningPolicy? ChunkPlanningPolicy { get; init; }

    /// <summary>
    /// When true, suppresses shared chunk plan generation and usage during ASR.
    /// ASR will process the full audio buffer as a single pass (legacy behavior).
    /// Use this to quickly revert to pre-chunking ASR behavior without code changes.
    /// </summary>
    public bool DisableChunkPlan { get; init; }

    /// <summary>
    /// When true, forces MFA to use the legacy single-utterance corpus path even
    /// when a chunk plan is available. ASR chunk planning is unaffected.
    /// Use this to isolate MFA chunking behavior during rollout without affecting ASR.
    /// </summary>
    public bool DisableChunkedMfa { get; init; }
}