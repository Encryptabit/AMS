namespace Ams.Core.Application.Runs;

public static class ModuleIds
{
    public static ModuleId BuildBookIndex { get; } = new("prep.book_index.build");

    public static ModuleId PipelineRun { get; } = new("prep.pipeline.run");

    public static ModuleId BenchmarkRun { get; } = new("prep.benchmark.run");

    public static ModuleId BenchmarkCompare { get; } = new("prep.benchmark.compare");
}
