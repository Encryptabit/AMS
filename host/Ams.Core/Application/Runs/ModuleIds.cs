namespace Ams.Core.Application.Runs;

public static class ModuleIds
{
    public static ModuleId BuildBookIndex { get; } = new("prep.book_index.build");

    public static ModuleId PipelineRun { get; } = new("prep.pipeline.run");
}
