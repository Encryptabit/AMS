namespace Ams.Core.Application.Benchmark;

public sealed record BenchmarkMetricsCollectionRequest
{
    public BenchmarkMetricsCollectionRequest(
        string chapterId,
        FileInfo rawAudioFile,
        FileInfo treatedAudioFile,
        FileInfo hydrateFile,
        FileInfo? treatedHydrateFile = null,
        BenchmarkMetricsPolicySnapshot? policy = null,
        long? pipelineRuntimeMs = null,
        string? sectionTitle = null)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(chapterId);
        ArgumentNullException.ThrowIfNull(rawAudioFile);
        ArgumentNullException.ThrowIfNull(treatedAudioFile);
        ArgumentNullException.ThrowIfNull(hydrateFile);

        if (pipelineRuntimeMs.HasValue && pipelineRuntimeMs.Value < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(pipelineRuntimeMs), "Runtime values cannot be negative.");
        }

        ChapterId = chapterId.Trim();
        RawAudioFile = rawAudioFile;
        TreatedAudioFile = treatedAudioFile;
        HydrateFile = hydrateFile;
        TreatedHydrateFile = treatedHydrateFile ?? hydrateFile;
        Policy = policy ?? BenchmarkMetricsPolicySnapshot.Default;
        PipelineRuntimeMs = pipelineRuntimeMs;
        SectionTitle = NormalizeOptionalText(sectionTitle);
    }

    public string ChapterId { get; }

    public FileInfo RawAudioFile { get; }

    public FileInfo TreatedAudioFile { get; }

    public FileInfo HydrateFile { get; }

    public FileInfo TreatedHydrateFile { get; }

    public BenchmarkMetricsPolicySnapshot Policy { get; }

    public long? PipelineRuntimeMs { get; }

    public string? SectionTitle { get; }

    private static string? NormalizeOptionalText(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return null;
        }

        return value.Trim();
    }
}

public interface IBenchmarkMetricsCollector
{
    Task<BenchmarkChapterMetrics> CollectAsync(
        BenchmarkMetricsCollectionRequest request,
        CancellationToken cancellationToken = default);
}
