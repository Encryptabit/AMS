using System.Text.Json;
using Ams.Core.Application.Runs;

namespace Ams.Tests.Application.Runs;

public class RunContractTests
{
    [Theory]
    [InlineData("prep.book_index.build")]
    [InlineData("prep.pipeline.run")]
    [InlineData("prep.stage_2.verify")]
    public void ModuleId_AcceptsStableLowerCaseDottedIds(string value)
    {
        var moduleId = new ModuleId(value);

        Assert.Equal(value, moduleId.Value);
        Assert.Equal(value, moduleId.ToString());
    }

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData("prep pipeline run")]
    [InlineData("Prep.pipeline.run")]
    [InlineData("prep.pipeline.run!")]
    [InlineData(".prep.pipeline.run")]
    [InlineData("prep..pipeline.run")]
    [InlineData("prep.pipeline.run.")]
    public void ModuleId_RejectsInvalidIds(string value)
    {
        Assert.Throws<ArgumentException>(() => new ModuleId(value));
    }

    [Fact]
    public void ModuleIds_DefineStableKnownModules()
    {
        Assert.Equal("prep.book_index.build", ModuleIds.BuildBookIndex.Value);
        Assert.Equal("prep.pipeline.run", ModuleIds.PipelineRun.Value);
        Assert.Equal("prep.benchmark.run", ModuleIds.BenchmarkRun.Value);
    }

    [Theory]
    [InlineData(RunState.Pending, 0.0)]
    [InlineData(RunState.Running, 0.5)]
    [InlineData(RunState.Completed, 1.0)]
    public void RunProgressUpdate_AllowsNonFailedStatesWithoutFailure(RunState state, double progress)
    {
        var update = new RunProgressUpdate(
            ModuleIds.PipelineRun,
            state,
            stage: "pipeline",
            message: "Progress updated",
            progress: progress,
            failure: null,
            artifacts: null);

        Assert.Equal(state, update.State);
        Assert.Equal(progress, update.Progress);
        Assert.Empty(update.Artifacts);
        Assert.Null(update.Failure);
    }

    [Fact]
    public void RunProgressUpdate_RequiresFailureForFailedState()
    {
        var exception = Assert.Throws<ArgumentException>(() =>
            new RunProgressUpdate(
                ModuleIds.PipelineRun,
                RunState.Failed,
                stage: "pipeline",
                message: "Run failed",
                progress: 0.5,
                failure: null,
                artifacts: null));

        Assert.Contains("failure", exception.Message, StringComparison.OrdinalIgnoreCase);
    }

    [Theory]
    [InlineData(RunState.Pending)]
    [InlineData(RunState.Running)]
    [InlineData(RunState.Completed)]
    public void RunProgressUpdate_RejectsFailureOutsideFailedState(RunState state)
    {
        var failure = new RunFailure(RunFailureKind.Execution, "Something broke", stage: "pipeline");

        var exception = Assert.Throws<ArgumentException>(() =>
            new RunProgressUpdate(
                ModuleIds.PipelineRun,
                state,
                stage: "pipeline",
                message: "Invalid state",
                progress: 0.2,
                failure: failure,
                artifacts: null));

        Assert.Contains("failed", exception.Message, StringComparison.OrdinalIgnoreCase);
    }

    [Theory]
    [InlineData(-0.01)]
    [InlineData(1.01)]
    public void RunProgressUpdate_RejectsProgressOutsideNormalizedRange(double progress)
    {
        Assert.Throws<ArgumentOutOfRangeException>(() =>
            new RunProgressUpdate(
                ModuleIds.PipelineRun,
                RunState.Running,
                stage: "pipeline",
                message: "Invalid progress",
                progress: progress,
                failure: null,
                artifacts: null));
    }

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    public void RunFailure_RejectsBlankMessages(string message)
    {
        Assert.Throws<ArgumentException>(() => new RunFailure(RunFailureKind.Validation, message));
    }

    [Fact]
    public void RunArtifact_RejectsBlankPath()
    {
        Assert.Throws<ArgumentException>(() =>
            new RunArtifact(
                name: "book-index",
                kind: RunArtifactKind.Output,
                path: " ",
                exists: false));
    }

    [Fact]
    public void RunProgressUpdate_RoundTripsThroughJson()
    {
        var update = new RunProgressUpdate(
            ModuleIds.BuildBookIndex,
            RunState.Failed,
            stage: "book_index",
            message: "Book index build failed",
            progress: 0.25,
            failure: new RunFailure(
                RunFailureKind.Dependency,
                "Pronunciation dictionary is unavailable",
                stage: "book_index"),
            artifacts:
            [
                new RunArtifact(
                    name: "book-index",
                    kind: RunArtifactKind.Output,
                    path: "/tmp/chapter/book-index.json",
                    exists: false),
                new RunArtifact(
                    name: "build-log",
                    kind: RunArtifactKind.Log,
                    path: "/tmp/chapter/build-index.log",
                    exists: true)
            ]);

        var json = JsonSerializer.Serialize(update);
        var roundTrip = JsonSerializer.Deserialize<RunProgressUpdate>(json);

        Assert.NotNull(roundTrip);
        Assert.Equal(update.ModuleId.Value, roundTrip.ModuleId.Value);
        Assert.Equal(update.State, roundTrip.State);
        Assert.Equal(update.Stage, roundTrip.Stage);
        Assert.Equal(update.Message, roundTrip.Message);
        Assert.Equal(update.Progress, roundTrip.Progress);
        Assert.NotNull(roundTrip.Failure);
        Assert.Equal(update.Failure!.Kind, roundTrip.Failure!.Kind);
        Assert.Equal(update.Failure.Message, roundTrip.Failure.Message);
        Assert.Equal(update.Failure.Stage, roundTrip.Failure.Stage);
        Assert.Equal(2, roundTrip.Artifacts.Count);
        Assert.Equal("book-index", roundTrip.Artifacts[0].Name);
        Assert.Equal(RunArtifactKind.Output, roundTrip.Artifacts[0].Kind);
        Assert.Equal("/tmp/chapter/book-index.json", roundTrip.Artifacts[0].Path);
        Assert.False(roundTrip.Artifacts[0].Exists);
        Assert.Equal("build-log", roundTrip.Artifacts[1].Name);
        Assert.Equal(RunArtifactKind.Log, roundTrip.Artifacts[1].Kind);
        Assert.Equal("/tmp/chapter/build-index.log", roundTrip.Artifacts[1].Path);
        Assert.True(roundTrip.Artifacts[1].Exists);
    }
}
