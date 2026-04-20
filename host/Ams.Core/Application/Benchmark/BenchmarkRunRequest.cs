using Ams.Core.Application.Pipeline;
using Ams.Core.Application.Runs;

namespace Ams.Core.Application.Benchmark;

public sealed record BenchmarkRunChapterRequest
{
    public BenchmarkRunChapterRequest(
        string chapterId,
        FileInfo audioFile,
        DirectoryInfo? chapterDirectory = null)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(chapterId);
        ArgumentNullException.ThrowIfNull(audioFile);

        ChapterId = chapterId.Trim();
        AudioFile = audioFile;
        ChapterDirectory = chapterDirectory;
    }

    public string ChapterId { get; }

    public FileInfo AudioFile { get; }

    public DirectoryInfo? ChapterDirectory { get; }
}

public sealed record BenchmarkRunRequest
{
    public BenchmarkRunRequest(
        bool deterministic,
        string? requestedModel,
        PipelineRunOptions pipelineOptions,
        IReadOnlyList<BenchmarkRunChapterRequest> chapters,
        DirectoryInfo outputRoot,
        string? runId = null,
        ModuleId? moduleId = null,
        BenchmarkMetricsPolicySnapshot? metricsPolicy = null)
    {
        ArgumentNullException.ThrowIfNull(pipelineOptions);
        ArgumentNullException.ThrowIfNull(chapters);
        ArgumentNullException.ThrowIfNull(outputRoot);

        Deterministic = deterministic;
        RequestedModel = NormalizeOptionalText(requestedModel);
        PipelineOptions = pipelineOptions;
        Chapters = chapters.ToArray();
        OutputRoot = outputRoot;
        RunId = NormalizeOptionalText(runId);
        ModuleId = moduleId ?? ModuleIds.BenchmarkRun;
        MetricsPolicy = metricsPolicy ?? BenchmarkMetricsPolicySnapshot.Default;
    }

    public bool Deterministic { get; }

    public string? RequestedModel { get; }

    public PipelineRunOptions PipelineOptions { get; }

    public IReadOnlyList<BenchmarkRunChapterRequest> Chapters { get; }

    public DirectoryInfo OutputRoot { get; }

    public string? RunId { get; }

    public ModuleId ModuleId { get; }

    public BenchmarkMetricsPolicySnapshot MetricsPolicy { get; }

    public BenchmarkDeterminismGateRequest CreateGateRequest()
        => BenchmarkDeterminismGateRequest.FromPipelineOptions(RequestedModel, PipelineOptions);

    private static string? NormalizeOptionalText(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return null;
        }

        return value.Trim();
    }
}
