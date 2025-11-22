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
}