using System.CommandLine;
using System.CommandLine.IO;
using Ams.Cli.Commands;
using CliProgram = Ams.Cli.Program;
using Ams.Core.Application.Benchmark;
using Ams.Core.Application.Pipeline;
using Ams.Core.Application.Runs;
using Ams.Core.Processors;
using Ams.Core.Runtime.Workspace;

namespace Ams.Tests.Cli;

public sealed class BenchmarkCommandContractTests : IDisposable
{
    private readonly List<string> _tempDirectories = new();

    [Fact]
    public void ProgramRootCommand_RegistersBenchmarkRunCompareAndListCommands()
    {
        using var host = CliProgram.BuildHost(Array.Empty<string>());
        var root = CliProgram.BuildRootCommand(host.Services);

        var benchmark = Assert.Single(
            root.Subcommands,
            command => string.Equals(command.Name, "benchmark", StringComparison.OrdinalIgnoreCase));

        Assert.Contains(
            benchmark.Subcommands,
            command => string.Equals(command.Name, "run", StringComparison.OrdinalIgnoreCase));
        Assert.Contains(
            benchmark.Subcommands,
            command => string.Equals(command.Name, "compare", StringComparison.OrdinalIgnoreCase));
        Assert.Contains(
            benchmark.Subcommands,
            command => string.Equals(command.Name, "list", StringComparison.OrdinalIgnoreCase));
    }

    [Fact]
    public async Task BenchmarkRun_MissingRequiredSources_ReturnsNonZeroWithoutDispatch()
    {
        var root = CreateRootCommand(
            executeRunAsync: (_, _, _) =>
            {
                throw new InvalidOperationException("Execute should not be called when required args are missing.");
            });

        var console = new TestConsole();
        var exitCode = await root.InvokeAsync(["benchmark", "run"], console);

        Assert.NotEqual(0, exitCode);
        Assert.False(string.IsNullOrWhiteSpace(console.Error.ToString()));
    }

    [Fact]
    public async Task BenchmarkRun_OmittedBook_ResolvesFromCurrentDirectory()
    {
        var fixture = CreateInputFixture();
        var capturedRequests = new List<BenchmarkRunRequest>();

        var root = CreateRootCommand(
            executeRunAsync: (_, request, _) =>
            {
                capturedRequests.Add(request);
                return Task.FromResult(CreateCompletedResult(fixture.WorkspaceRoot, fixture.ManifestFile, deterministic: false));
            });

        var console = new TestConsole();
        var originalDirectory = Directory.GetCurrentDirectory();

        try
        {
            Directory.SetCurrentDirectory(fixture.BookFile.Directory!.FullName);

            var exitCode = await root.InvokeAsync(
                [
                    "benchmark",
                    "run",
                    "--audio", fixture.AudioFiles[0].FullName,
                    "--book-index", fixture.BookIndexFile.FullName
                ],
                console);

            Assert.Equal(0, exitCode);
            var captured = Assert.Single(capturedRequests);
            Assert.Equal(fixture.BookFile.FullName, captured.PipelineOptions.BookFile.FullName);
        }
        finally
        {
            Directory.SetCurrentDirectory(originalDirectory);
        }
    }

    [Fact]
    public async Task BenchmarkRun_AudioStemWithSpaces_PreservesChapterIdAndDirectoryName()
    {
        var fixture = CreateInputFixture();
        var spacedAudio = WriteBinaryFile(
            Path.Combine(fixture.WorkDir.FullName, "Chapter 1.wav"),
            [0x52, 0x49, 0x46, 0x46]);

        var capturedRequests = new List<BenchmarkRunRequest>();

        var root = CreateRootCommand(
            executeRunAsync: (_, request, _) =>
            {
                capturedRequests.Add(request);
                return Task.FromResult(CreateCompletedResult(fixture.WorkspaceRoot, fixture.ManifestFile, deterministic: false));
            });

        var console = new TestConsole();
        var exitCode = await root.InvokeAsync(
            [
                "benchmark",
                "run",
                "--book", fixture.BookFile.FullName,
                "--audio", spacedAudio.FullName,
                "--book-index", fixture.BookIndexFile.FullName
            ],
            console);

        Assert.Equal(0, exitCode);

        var request = Assert.Single(capturedRequests);
        var chapter = Assert.Single(request.Chapters);

        Assert.Equal("Chapter 1", chapter.ChapterId);
        Assert.Equal("Chapter 1", chapter.ChapterDirectory?.Name);
    }

    [Fact]
    public async Task BenchmarkRun_DeterministicAliasModel_ReturnsNonZeroWithoutDispatch()
    {
        var fixture = CreateInputFixture();
        var dispatchCount = 0;

        var root = CreateRootCommand(
            executeRunAsync: (_, _, _) =>
            {
                dispatchCount++;
                return Task.FromResult(CreateCompletedResult(fixture.WorkspaceRoot, fixture.ManifestFile, deterministic: true));
            });

        var console = new TestConsole();
        var exitCode = await root.InvokeAsync(
            [
                "benchmark",
                "run",
                "--book", fixture.BookFile.FullName,
                "--audio", fixture.AudioFiles[0].FullName,
                "--deterministic",
                "--asr-model", "tiny"
            ],
            console);

        Assert.NotEqual(0, exitCode);
        Assert.Equal(0, dispatchCount);
        Assert.Contains("alias-only", console.Error.ToString(), StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task BenchmarkRun_DeterministicRejection_ReturnsExplicitExitAndInvalidArtifactSummary()
    {
        var fixture = CreateInputFixture();
        var capturedRequests = new List<BenchmarkRunRequest>();

        var root = CreateRootCommand(
            executeRunAsync: (_, request, _) =>
            {
                capturedRequests.Add(request);
                return Task.FromResult(CreateInvalidResult(fixture.WorkspaceRoot, fixture.InvalidRunFile));
            });

        var console = new TestConsole();
        var exitCode = await root.InvokeAsync(
            [
                "benchmark",
                "run",
                "--book", fixture.BookFile.FullName,
                "--audio", fixture.AudioFiles[0].FullName,
                "--deterministic",
                "--asr-model", fixture.ModelFile.FullName,
                "--book-index", fixture.BookIndexFile.FullName,
                "--output-root", fixture.OutputRoot.FullName
            ],
            console);

        Assert.Equal(3, exitCode);
        var stdout = console.Out.ToString();
        Assert.Contains("Benchmark run ID: run-invalid-001", stdout, StringComparison.Ordinal);
        Assert.Contains("Deterministic verdict: invalid", stdout, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("Artifact path:", stdout, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("Deterministic reason codes:", stdout, StringComparison.OrdinalIgnoreCase);

        var captured = Assert.Single(capturedRequests);
        Assert.True(captured.Deterministic);
        Assert.Equal(fixture.ModelFile.FullName, captured.RequestedModel);
        Assert.Single(captured.Chapters);
    }

    [Fact]
    public async Task BenchmarkRun_MultiChapterInvocation_PreservesBoundarySemanticsAndUsesSingleDispatch()
    {
        var fixture = CreateInputFixture();
        var capturedRequests = new List<BenchmarkRunRequest>();

        var root = CreateRootCommand(
            executeRunAsync: (_, request, _) =>
            {
                capturedRequests.Add(request);
                return Task.FromResult(CreateCompletedResult(fixture.WorkspaceRoot, fixture.ManifestFile, deterministic: false));
            });

        var console = new TestConsole();
        var exitCode = await root.InvokeAsync(
            [
                "benchmark",
                "run",
                "--book", fixture.BookFile.FullName,
                "--audio", fixture.AudioFiles[0].FullName,
                "--audio", fixture.AudioFiles[1].FullName,
                "--book-index", fixture.BookIndexFile.FullName,
                "--work-dir", fixture.WorkDir.FullName,
                "--output-root", fixture.OutputRoot.FullName
            ],
            console);

        Assert.Equal(0, exitCode);
        var captured = Assert.Single(capturedRequests);
        Assert.Equal(2, captured.Chapters.Count);
        Assert.Equal(new[] { "chapter-01", "chapter-02" }, captured.Chapters.Select(chapter => chapter.ChapterId));

        var stdout = console.Out.ToString();
        Assert.Contains("Deterministic verdict: not-requested", stdout, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("Run phase: completed", stdout, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("Aggregate chapter-state counts:", stdout, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("Aggregate metrics-state counts:", stdout, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("Aggregate runtime-ms:", stdout, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("Aggregate quality totals:", stdout, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("Artifact path:", stdout, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task BenchmarkRun_SubMillisecondAudioActivities_PrintFractionalRuntimeMilliseconds()
    {
        var fixture = CreateInputFixture();

        var root = CreateRootCommand(
            executeRunAsync: (_, _, _) =>
                Task.FromResult(CreateCompletedResultWithAudioActivityMetrics(fixture.WorkspaceRoot, fixture.ManifestFile)));

        var console = new TestConsole();
        var exitCode = await root.InvokeAsync(
            [
                "benchmark",
                "run",
                "--book", fixture.BookFile.FullName,
                "--audio", fixture.AudioFiles[0].FullName,
                "--book-index", fixture.BookIndexFile.FullName
            ],
            console);

        Assert.Equal(0, exitCode);

        var stdout = console.Out.ToString();
        Assert.Contains(
            "Audio processing activity totals: count=2, runtime-ms=0.175, failures=0",
            stdout,
            StringComparison.OrdinalIgnoreCase);
        Assert.Contains(
            "Audio processing activity: function=EncodeWavToStream, calls=2, runtime-ms=0.175, failures=0",
            stdout,
            StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task BenchmarkRun_ServiceCancellation_ReturnsCancelledExitCodeAndFailureSummary()
    {
        var fixture = CreateInputFixture();

        var root = CreateRootCommand(
            executeRunAsync: (_, _, _) =>
                Task.FromResult(CreateCancelledResult(fixture.WorkspaceRoot, fixture.ManifestFile)));

        var console = new TestConsole();
        var exitCode = await root.InvokeAsync(
            [
                "benchmark",
                "run",
                "--book", fixture.BookFile.FullName,
                "--audio", fixture.AudioFiles[0].FullName,
                "--book-index", fixture.BookIndexFile.FullName
            ],
            console);

        Assert.Equal(4, exitCode);
        var stdout = console.Out.ToString();
        Assert.Contains("Run state: failed", stdout, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("Aggregate chapter-state counts:", stdout, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("Aggregate metrics-state counts:", stdout, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("Failure: kind=cancelled", stdout, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("Artifact path:", stdout, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task BenchmarkRun_PartialFailureRun_PrintsAggregateCountersAndFailureDiagnostics()
    {
        var fixture = CreateInputFixture();

        var root = CreateRootCommand(
            executeRunAsync: (_, _, _) =>
                Task.FromResult(CreatePartialFailureResult(fixture.WorkspaceRoot, fixture.ManifestFile)));

        var console = new TestConsole();
        var exitCode = await root.InvokeAsync(
            [
                "benchmark",
                "run",
                "--book", fixture.BookFile.FullName,
                "--audio", fixture.AudioFiles[0].FullName,
                "--book-index", fixture.BookIndexFile.FullName
            ],
            console);

        Assert.Equal(5, exitCode);
        var stdout = console.Out.ToString();
        Assert.Contains(
            "Aggregate chapter-state counts: pending=0, running=0, failed=1, completed=1, total=2",
            stdout,
            StringComparison.OrdinalIgnoreCase);
        Assert.Contains(
            "Aggregate metrics-state counts: not-run=0, completed=1, partial=1, failed=0, total=2",
            stdout,
            StringComparison.OrdinalIgnoreCase);
        Assert.Contains("Aggregate runtime-ms: pipeline=180, analysis=65, total=245", stdout, StringComparison.OrdinalIgnoreCase);
        Assert.Contains(
            "Aggregate quality totals: mismatch-count=3, missing-speech-sec=0.5, extra-speech-sec=0.15, qc-flag-count=4",
            stdout,
            StringComparison.OrdinalIgnoreCase);
        Assert.Contains("Failure: kind=execution", stdout, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task BenchmarkRun_MalformedAggregatePayload_ReturnsContractError()
    {
        var fixture = CreateInputFixture();

        var root = CreateRootCommand(
            executeRunAsync: (_, _, _) =>
                Task.FromResult(CreateMalformedAggregateResult(fixture.WorkspaceRoot, fixture.ManifestFile)));

        var console = new TestConsole();
        var exitCode = await root.InvokeAsync(
            [
                "benchmark",
                "run",
                "--book", fixture.BookFile.FullName,
                "--audio", fixture.AudioFiles[0].FullName,
                "--book-index", fixture.BookIndexFile.FullName
            ],
            console);

        Assert.Equal(2, exitCode);
        Assert.Contains("aggregate", console.Error.ToString(), StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task BenchmarkRun_NullArtifactPath_ReturnsContractError()
    {
        var fixture = CreateInputFixture();

        var root = CreateRootCommand(
            executeRunAsync: (_, _, _) =>
                Task.FromResult(CreateMissingArtifactResult()));

        var console = new TestConsole();
        var exitCode = await root.InvokeAsync(
            [
                "benchmark",
                "run",
                "--book", fixture.BookFile.FullName,
                "--audio", fixture.AudioFiles[0].FullName,
                "--book-index", fixture.BookIndexFile.FullName
            ],
            console);

        Assert.Equal(2, exitCode);
        Assert.Contains("artifact", console.Error.ToString(), StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task BenchmarkRun_MalformedServicePayload_ReturnsContractError()
    {
        var fixture = CreateInputFixture();

        var root = CreateRootCommand(
            executeRunAsync: (_, _, _) => Task.FromResult(CreateMalformedDeterministicResult()));

        var console = new TestConsole();
        var exitCode = await root.InvokeAsync(
            [
                "benchmark",
                "run",
                "--book", fixture.BookFile.FullName,
                "--audio", fixture.AudioFiles[0].FullName,
                "--deterministic",
                "--asr-model", fixture.ModelFile.FullName,
                "--book-index", fixture.BookIndexFile.FullName
            ],
            console);

        Assert.Equal(2, exitCode);
        Assert.Contains("contract error", console.Error.ToString(), StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task BenchmarkCompare_MissingRequiredSources_ReturnsNonZeroWithoutDispatch()
    {
        var dispatchCount = 0;
        var root = CreateRootCommand(
            executeCompareAsync: (_, _) =>
            {
                dispatchCount++;
                throw new InvalidOperationException("Compare should not dispatch when required args are missing.");
            });

        var console = new TestConsole();
        var exitCode = await root.InvokeAsync(["benchmark", "compare"], console);

        Assert.NotEqual(0, exitCode);
        Assert.Equal(0, dispatchCount);
        Assert.False(string.IsNullOrWhiteSpace(console.Error.ToString()));
    }

    [Fact]
    public async Task BenchmarkList_DefaultDirectory_EnumeratesArtifactsFromBenchmarkRuns()
    {
        var fixture = CreateInputFixture();
        File.SetLastWriteTimeUtc(fixture.ManifestFile.FullName, new DateTime(2026, 4, 15, 22, 0, 0, DateTimeKind.Utc));
        File.SetLastWriteTimeUtc(fixture.InvalidRunFile.FullName, new DateTime(2026, 4, 15, 21, 59, 0, DateTimeKind.Utc));

        var root = CreateRootCommand();
        var console = new TestConsole();

        var originalDirectory = Directory.GetCurrentDirectory();
        try
        {
            Directory.SetCurrentDirectory(fixture.WorkDir.FullName);

            var exitCode = await root.InvokeAsync(["benchmark", "ls"], console);

            Assert.Equal(0, exitCode);
            var stdout = console.Out.ToString();
            Assert.Contains("Benchmark artifacts:", stdout, StringComparison.OrdinalIgnoreCase);
            Assert.Contains("[0]", stdout, StringComparison.OrdinalIgnoreCase);
            Assert.Contains(fixture.ManifestFile.Name, stdout, StringComparison.OrdinalIgnoreCase);
            Assert.Contains(fixture.InvalidRunFile.Name, stdout, StringComparison.OrdinalIgnoreCase);
        }
        finally
        {
            Directory.SetCurrentDirectory(originalDirectory);
        }
    }

    [Fact]
    public async Task BenchmarkCompare_PositionalIndexes_ResolveFromDefaultBenchmarkRunsDirectory()
    {
        var fixture = CreateInputFixture();
        var capturedRequests = new List<BenchmarkCompareRequest>();

        File.SetLastWriteTimeUtc(fixture.ManifestFile.FullName, new DateTime(2026, 4, 15, 22, 0, 0, DateTimeKind.Utc));
        File.SetLastWriteTimeUtc(fixture.InvalidRunFile.FullName, new DateTime(2026, 4, 15, 21, 59, 0, DateTimeKind.Utc));

        var root = CreateRootCommand(
            executeCompareAsync: (request, _) =>
            {
                capturedRequests.Add(request);
                return Task.FromResult(CreateCompatibleCompareResult(fixture.OutputRoot));
            });

        var console = new TestConsole();
        var originalDirectory = Directory.GetCurrentDirectory();

        try
        {
            Directory.SetCurrentDirectory(fixture.WorkDir.FullName);

            var exitCode = await root.InvokeAsync(["benchmark", "compare", "0", "1"], console);

            Assert.Equal(0, exitCode);
            var request = Assert.Single(capturedRequests);
            Assert.Equal(fixture.ManifestFile.FullName, request.BaselineArtifact.FullName);
            Assert.Equal(fixture.InvalidRunFile.FullName, request.CandidateArtifact.FullName);
        }
        finally
        {
            Directory.SetCurrentDirectory(originalDirectory);
        }
    }

    [Fact]
    public async Task BenchmarkCompare_PositionalIndexes_ResolveFromExplicitDirOverride()
    {
        var fixture = CreateInputFixture();
        var alternateRoot = new DirectoryInfo(Path.Combine(fixture.WorkDir.FullName, "alt-benchmark-runs"));
        Directory.CreateDirectory(alternateRoot.FullName);

        var baseline = WriteTextFile(Path.Combine(alternateRoot.FullName, "alt-baseline.manifest.json"), "{}", registerPath: false);
        var candidate = WriteTextFile(Path.Combine(alternateRoot.FullName, "alt-candidate.manifest.json"), "{}", registerPath: false);

        File.SetLastWriteTimeUtc(baseline.FullName, new DateTime(2026, 4, 15, 21, 59, 0, DateTimeKind.Utc));
        File.SetLastWriteTimeUtc(candidate.FullName, new DateTime(2026, 4, 15, 22, 0, 0, DateTimeKind.Utc));

        var capturedRequests = new List<BenchmarkCompareRequest>();
        var root = CreateRootCommand(
            executeCompareAsync: (request, _) =>
            {
                capturedRequests.Add(request);
                return Task.FromResult(CreateCompatibleCompareResult(alternateRoot));
            });

        var console = new TestConsole();
        var originalDirectory = Directory.GetCurrentDirectory();

        try
        {
            Directory.SetCurrentDirectory(fixture.WorkDir.FullName);

            var exitCode = await root.InvokeAsync(
                ["benchmark", "compare", "0", "1", "--dir", alternateRoot.FullName],
                console);

            Assert.Equal(0, exitCode);
            var request = Assert.Single(capturedRequests);
            Assert.Equal(candidate.FullName, request.BaselineArtifact.FullName);
            Assert.Equal(baseline.FullName, request.CandidateArtifact.FullName);
        }
        finally
        {
            Directory.SetCurrentDirectory(originalDirectory);
        }
    }

    [Fact]
    public async Task BenchmarkCompare_NonExistentBaseline_ReturnsNonZeroWithoutDispatch()
    {
        var fixture = CreateInputFixture();
        var dispatchCount = 0;

        var root = CreateRootCommand(
            executeCompareAsync: (_, _) =>
            {
                dispatchCount++;
                return Task.FromResult(CreateCompatibleCompareResult(fixture.OutputRoot));
            });

        var missingBaseline = Path.Combine(fixture.OutputRoot.FullName, "missing-baseline.manifest.json");
        var console = new TestConsole();
        var exitCode = await root.InvokeAsync(
            [
                "benchmark",
                "compare",
                "--baseline", missingBaseline,
                "--candidate", fixture.ManifestFile.FullName
            ],
            console);

        Assert.Equal(2, exitCode);
        Assert.Equal(0, dispatchCount);
        Assert.Contains("not found", console.Error.ToString(), StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task BenchmarkCompare_UnsupportedArtifactKind_ReturnsNonZeroWithoutDispatch()
    {
        var fixture = CreateInputFixture();
        var dispatchCount = 0;
        var unsupported = WriteTextFile(Path.Combine(fixture.OutputRoot.FullName, "baseline.txt"), "unsupported-artifact");

        var root = CreateRootCommand(
            executeCompareAsync: (_, _) =>
            {
                dispatchCount++;
                return Task.FromResult(CreateCompatibleCompareResult(fixture.OutputRoot));
            });

        var console = new TestConsole();
        var exitCode = await root.InvokeAsync(
            [
                "benchmark",
                "compare",
                "--baseline", unsupported.FullName,
                "--candidate", fixture.ManifestFile.FullName
            ],
            console);

        Assert.Equal(2, exitCode);
        Assert.Equal(0, dispatchCount);
        Assert.Contains(".manifest.json", console.Error.ToString(), StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task BenchmarkCompare_CompatibleResult_PrintsStableMetricVerdictsAndArtifactPath()
    {
        var fixture = CreateInputFixture();
        var capturedRequests = new List<BenchmarkCompareRequest>();

        var root = CreateRootCommand(
            executeCompareAsync: (request, _) =>
            {
                capturedRequests.Add(request);
                return Task.FromResult(CreateCompatibleCompareResult(fixture.OutputRoot));
            });

        var console = new TestConsole();
        var exitCode = await root.InvokeAsync(
            [
                "benchmark",
                "compare",
                "--baseline", fixture.ManifestFile.FullName,
                "--candidate", fixture.ManifestFile.FullName,
                "--output-root", fixture.OutputRoot.FullName,
                "--compare-id", "cmp-cli-001"
            ],
            console);

        Assert.Equal(0, exitCode);

        var capturedRequest = Assert.Single(capturedRequests);
        Assert.Equal(ModuleIds.BenchmarkCompare, capturedRequest.ModuleId);
        Assert.Equal("cmp-cli-001", capturedRequest.CompareId);

        var stdout = console.Out.ToString();
        Assert.Contains("Compare compatibility: compatible", stdout, StringComparison.OrdinalIgnoreCase);
        Assert.Contains(
            "Metric verdict: metric=totalPipelineRuntimeMs, verdict=no-change",
            stdout,
            StringComparison.OrdinalIgnoreCase);
        Assert.Contains("delta=25", stdout, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("threshold=25 ms", stdout, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("rationale=Delta landed on threshold boundary.", stdout, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("Compare artifact path:", stdout, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task BenchmarkCompare_IncompatibleResult_PrintsReasonsBeforeSkippingVerdictScoring()
    {
        var fixture = CreateInputFixture();

        var root = CreateRootCommand(
            executeCompareAsync: (_, _) => Task.FromResult(CreateIncompatibleCompareResult(fixture.OutputRoot)));

        var console = new TestConsole();
        var exitCode = await root.InvokeAsync(
            [
                "benchmark",
                "compare",
                "--baseline", fixture.ManifestFile.FullName,
                "--candidate", fixture.ManifestFile.FullName,
                "--output-root", fixture.OutputRoot.FullName
            ],
            console);

        Assert.Equal(5, exitCode);

        var stdout = console.Out.ToString() ?? string.Empty;
        var reasonIndex = stdout.IndexOf("Compare compatibility reason:", StringComparison.OrdinalIgnoreCase);
        var scoringIndex = stdout.IndexOf("Metric verdict scoring skipped:", StringComparison.OrdinalIgnoreCase);

        Assert.True(reasonIndex >= 0);
        Assert.True(scoringIndex > reasonIndex);
        Assert.Contains("chapter-set mismatch", stdout, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("Metric verdict: metric=", stdout, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task BenchmarkCompare_MissingArtifactPathFromService_ReturnsContractError()
    {
        var fixture = CreateInputFixture();

        var root = CreateRootCommand(
            executeCompareAsync: (_, _) => Task.FromResult(CreateCompareResultMissingArtifactPath()));

        var console = new TestConsole();
        var exitCode = await root.InvokeAsync(
            [
                "benchmark",
                "compare",
                "--baseline", fixture.ManifestFile.FullName,
                "--candidate", fixture.ManifestFile.FullName,
                "--output-root", fixture.OutputRoot.FullName
            ],
            console);

        Assert.Equal(2, exitCode);
        Assert.Contains("artifact", console.Error.ToString(), StringComparison.OrdinalIgnoreCase);
    }

    public void Dispose()
    {
        foreach (var directory in _tempDirectories)
        {
            try
            {
                if (Directory.Exists(directory))
                {
                    Directory.Delete(directory, recursive: true);
                }
            }
            catch
            {
                // Best-effort temp cleanup.
            }
        }
    }

    private RootCommand CreateRootCommand(
        Func<IWorkspace, BenchmarkRunRequest, CancellationToken, Task<BenchmarkRunResult>>? executeRunAsync = null,
        Func<BenchmarkCompareRequest, CancellationToken, Task<BenchmarkCompareResult>>? executeCompareAsync = null)
    {
        var benchmarkRunService = CreatePlaceholderRunService();
        var benchmarkCompareService = CreatePlaceholderCompareService();
        var benchmarkCommand = BenchmarkCommand.Create(
            benchmarkRunService,
            benchmarkCompareService,
            executeRunAsync,
            executeCompareAsync);

        var root = new RootCommand("test-root");
        root.AddCommand(benchmarkCommand);
        return root;
    }

    private BenchmarkRunService CreatePlaceholderRunService()
    {
        return new BenchmarkRunService(
            runPipelineChapterAsync: (_, _, _) =>
                throw new InvalidOperationException("Placeholder BenchmarkRunService should not execute pipeline in command-contract tests."),
            evaluateDeterminismAsync: (_, _) =>
                throw new InvalidOperationException("Placeholder BenchmarkRunService should not evaluate gate in command-contract tests."),
            artifactStore: new BenchmarkRunArtifactStore(),
            utcNow: () => new DateTimeOffset(2026, 4, 15, 22, 0, 0, TimeSpan.Zero),
            runIdFactory: () => "test-run");
    }

    private static BenchmarkCompareService CreatePlaceholderCompareService()
    {
        return new BenchmarkCompareService(
            artifactStore: new BenchmarkRunArtifactStore(),
            manifestValidator: new BenchmarkRunManifestValidator(),
            utcNow: () => new DateTimeOffset(2026, 4, 15, 22, 0, 0, TimeSpan.Zero),
            compareIdFactory: () => "cmp-test");
    }

    private BenchmarkInputFixture CreateInputFixture()
    {
        var root = CreateTempDirectory();
        var workDir = new DirectoryInfo(Path.Combine(root, "work"));
        Directory.CreateDirectory(workDir.FullName);

        var outputRoot = new DirectoryInfo(Path.Combine(workDir.FullName, "benchmark-runs"));
        Directory.CreateDirectory(outputRoot.FullName);

        var bookFile = WriteTextFile(Path.Combine(root, "book.md"), "# Test Book\n\nSample manuscript content.");
        var bookIndexFile = WriteTextFile(Path.Combine(workDir.FullName, "book-index.json"), "{\"book\":\"test\"}");
        var modelFile = WriteTextFile(Path.Combine(root, "models", "ggml-large-v3.bin"), "fake-model");

        var chapterDir = new DirectoryInfo(Path.Combine(workDir.FullName, "chapters"));
        Directory.CreateDirectory(chapterDir.FullName);

        var audio1 = WriteBinaryFile(Path.Combine(chapterDir.FullName, "chapter-01.wav"), [0x52, 0x49, 0x46, 0x46]);
        var audio2 = WriteBinaryFile(Path.Combine(chapterDir.FullName, "chapter-02.wav"), [0x52, 0x49, 0x46, 0x46]);

        var manifestFile = WriteTextFile(
            Path.Combine(outputRoot.FullName, "benchmark-run-run-valid-001.manifest.json"),
            "{}",
            registerPath: false);

        var invalidRunFile = WriteTextFile(
            Path.Combine(outputRoot.FullName, "benchmark-run-run-invalid-001.invalid-run.json"),
            "{}",
            registerPath: false);

        return new BenchmarkInputFixture(
            WorkspaceRoot: workDir.FullName,
            WorkDir: workDir,
            OutputRoot: outputRoot,
            BookFile: bookFile,
            BookIndexFile: bookIndexFile,
            ModelFile: modelFile,
            AudioFiles: [audio1, audio2],
            ManifestFile: manifestFile,
            InvalidRunFile: invalidRunFile);
    }

    private BenchmarkRunResult CreateCompletedResult(
        string workspaceRoot,
        FileInfo manifestFile,
        bool deterministic)
    {
        return new BenchmarkRunResult(
            runId: "run-valid-001",
            moduleId: ModuleIds.BenchmarkRun,
            deterministic: deterministic,
            phase: BenchmarkRunPhase.Completed,
            state: RunState.Completed,
            determinism: deterministic ? CreateValidDeterminismContract() : null,
            chapterSet: ["chapter-01"],
            chapterSetFingerprint: "fingerprint-valid",
            chapterOutcomes: [],
            manifestFile: new FileInfo(Path.Combine(workspaceRoot, manifestFile.Name)),
            invalidRunFile: null,
            failure: null,
            phaseTransitions:
            [
                new BenchmarkRunPhaseTransition(BenchmarkRunPhase.Completed, DateTimeOffset.UtcNow, "done")
            ]);
    }

    private static BenchmarkRunResult CreateCompletedResultWithAudioActivityMetrics(string workspaceRoot, FileInfo manifestFile)
    {
        var audioActivities = new[]
        {
            new BenchmarkAudioProcessingActivity(
                function: nameof(AudioProcessor.EncodeWavToStream),
                startedAtUtc: DateTimeOffset.UtcNow,
                durationMs: 0,
                succeeded: true,
                durationUs: 125),
            new BenchmarkAudioProcessingActivity(
                function: nameof(AudioProcessor.EncodeWavToStream),
                startedAtUtc: DateTimeOffset.UtcNow,
                durationMs: 0,
                succeeded: true,
                durationUs: 50)
        };

        var chapterOutcomes = new[]
        {
            new BenchmarkRunChapterOutcome(
                chapterId: "chapter-01",
                state: RunState.Completed,
                summary: "Pipeline chapter completed.",
                stageSummaries: [],
                artifacts: [],
                metrics: CreateCompletedMetrics(
                    pipelineRuntimeMs: 100,
                    analysisRuntimeMs: 20,
                    mismatchCount: 1,
                    missingSpeechSec: 0.1,
                    extraSpeechSec: 0.05,
                    rawQcFlags: 1,
                    treatedQcFlags: 0,
                    audioProcessingActivities: audioActivities))
        };

        return new BenchmarkRunResult(
            runId: "run-valid-audio-activity-001",
            moduleId: ModuleIds.BenchmarkRun,
            deterministic: false,
            phase: BenchmarkRunPhase.Completed,
            state: RunState.Completed,
            determinism: null,
            chapterSet: ["chapter-01"],
            chapterSetFingerprint: "fingerprint-valid-audio-activity",
            chapterOutcomes: chapterOutcomes,
            manifestFile: new FileInfo(Path.Combine(workspaceRoot, manifestFile.Name)),
            invalidRunFile: null,
            failure: null,
            phaseTransitions:
            [
                new BenchmarkRunPhaseTransition(BenchmarkRunPhase.Completed, DateTimeOffset.UtcNow, "done")
            ]);
    }

    private static BenchmarkRunResult CreateInvalidResult(string workspaceRoot, FileInfo invalidRunFile)
    {
        return new BenchmarkRunResult(
            runId: "run-invalid-001",
            moduleId: ModuleIds.BenchmarkRun,
            deterministic: true,
            phase: BenchmarkRunPhase.Invalid,
            state: RunState.Completed,
            determinism: CreateInvalidDeterminismContract(),
            chapterSet: ["chapter-01"],
            chapterSetFingerprint: "fingerprint-invalid",
            chapterOutcomes: [],
            manifestFile: null,
            invalidRunFile: new FileInfo(Path.Combine(workspaceRoot, invalidRunFile.Name)),
            failure: null,
            phaseTransitions:
            [
                new BenchmarkRunPhaseTransition(BenchmarkRunPhase.Invalid, DateTimeOffset.UtcNow, "rejected")
            ]);
    }

    private static BenchmarkRunResult CreateCancelledResult(string workspaceRoot, FileInfo manifestFile)
    {
        var failure = new RunFailure(RunFailureKind.Cancelled, "Run cancelled by token.", "running");

        return new BenchmarkRunResult(
            runId: "run-cancelled-001",
            moduleId: ModuleIds.BenchmarkRun,
            deterministic: false,
            phase: BenchmarkRunPhase.Failed,
            state: RunState.Failed,
            determinism: null,
            chapterSet: ["chapter-01"],
            chapterSetFingerprint: "fingerprint-cancelled",
            chapterOutcomes: [],
            manifestFile: new FileInfo(Path.Combine(workspaceRoot, manifestFile.Name)),
            invalidRunFile: null,
            failure: failure,
            phaseTransitions:
            [
                new BenchmarkRunPhaseTransition(BenchmarkRunPhase.Failed, DateTimeOffset.UtcNow, "cancelled")
            ]);
    }

    private static BenchmarkRunResult CreatePartialFailureResult(string workspaceRoot, FileInfo manifestFile)
    {
        var runFailure = new RunFailure(RunFailureKind.Execution, "One or more chapters failed.", "running");
        var chapterFailure = new RunFailure(RunFailureKind.Execution, "Hydrate stage failed.", "hydrate");

        var chapterOutcomes = new[]
        {
            new BenchmarkRunChapterOutcome(
                chapterId: "chapter-01",
                state: RunState.Completed,
                summary: "Pipeline chapter completed.",
                stageSummaries: [],
                artifacts: [],
                metrics: CreateCompletedMetrics(
                    pipelineRuntimeMs: 100,
                    analysisRuntimeMs: 25,
                    mismatchCount: 2,
                    missingSpeechSec: 0.30,
                    extraSpeechSec: 0.10,
                    rawQcFlags: 1,
                    treatedQcFlags: 2)),
            new BenchmarkRunChapterOutcome(
                chapterId: "chapter-02",
                state: RunState.Failed,
                summary: "Hydrate stage failed.",
                failure: chapterFailure,
                stageSummaries: [],
                artifacts: [],
                metrics: CreatePartialMetrics(
                    chapterId: "chapter-02",
                    pipelineRuntimeMs: 80,
                    analysisRuntimeMs: 40,
                    mismatchCount: 1,
                    missingSpeechSec: 0.20,
                    extraSpeechSec: 0.05,
                    rawQcFlags: 0,
                    treatedQcFlags: 1))
        };

        return new BenchmarkRunResult(
            runId: "run-partial-failure-001",
            moduleId: ModuleIds.BenchmarkRun,
            deterministic: false,
            phase: BenchmarkRunPhase.Failed,
            state: RunState.Failed,
            determinism: null,
            chapterSet: ["chapter-01", "chapter-02"],
            chapterSetFingerprint: "fingerprint-partial-failure",
            chapterOutcomes: chapterOutcomes,
            manifestFile: new FileInfo(Path.Combine(workspaceRoot, manifestFile.Name)),
            invalidRunFile: null,
            failure: runFailure,
            phaseTransitions:
            [
                new BenchmarkRunPhaseTransition(BenchmarkRunPhase.Failed, DateTimeOffset.UtcNow, "failed")
            ]);
    }

    private static BenchmarkRunResult CreateMalformedAggregateResult(string workspaceRoot, FileInfo manifestFile)
    {
        var chapterOutcomes = new[]
        {
            new BenchmarkRunChapterOutcome(
                chapterId: "chapter-01",
                state: RunState.Completed,
                summary: "Pipeline chapter completed.",
                stageSummaries: [],
                artifacts: [],
                metrics: CreateCompletedMetrics(
                    pipelineRuntimeMs: 120,
                    analysisRuntimeMs: 30,
                    mismatchCount: 1,
                    missingSpeechSec: 0.20,
                    extraSpeechSec: 0.05,
                    rawQcFlags: 1,
                    treatedQcFlags: 0))
        };

        var malformedAggregate = new BenchmarkRunMetricsAggregate(
            chapterStates: new BenchmarkRunChapterStateCounts(
                pending: 0,
                running: 0,
                failed: 0,
                completed: 0),
            metricsStates: new BenchmarkMetricsStateCounts(
                notRun: 1,
                completed: 0,
                partial: 0,
                failed: 0),
            totalPipelineRuntimeMs: 0,
            totalAnalysisRuntimeMs: 0,
            totalMismatchCount: 0,
            totalMissingSpeechSec: 0,
            totalExtraSpeechSec: 0,
            totalQcFlags: 0);

        return new BenchmarkRunResult(
            runId: "run-malformed-aggregate-001",
            moduleId: ModuleIds.BenchmarkRun,
            deterministic: false,
            phase: BenchmarkRunPhase.Completed,
            state: RunState.Completed,
            determinism: null,
            chapterSet: ["chapter-01"],
            chapterSetFingerprint: "fingerprint-malformed-aggregate",
            chapterOutcomes: chapterOutcomes,
            manifestFile: new FileInfo(Path.Combine(workspaceRoot, manifestFile.Name)),
            invalidRunFile: null,
            failure: null,
            phaseTransitions:
            [
                new BenchmarkRunPhaseTransition(BenchmarkRunPhase.Completed, DateTimeOffset.UtcNow, "completed")
            ],
            aggregateMetrics: malformedAggregate);
    }

    private static BenchmarkRunResult CreateMissingArtifactResult()
    {
        return new BenchmarkRunResult(
            runId: "run-missing-artifact-001",
            moduleId: ModuleIds.BenchmarkRun,
            deterministic: false,
            phase: BenchmarkRunPhase.Completed,
            state: RunState.Completed,
            determinism: null,
            chapterSet: ["chapter-01"],
            chapterSetFingerprint: "fingerprint-missing-artifact",
            chapterOutcomes:
            [
                new BenchmarkRunChapterOutcome(
                    chapterId: "chapter-01",
                    state: RunState.Completed,
                    summary: "Pipeline chapter completed.",
                    stageSummaries: [],
                    artifacts: [],
                    metrics: CreateCompletedMetrics(
                        pipelineRuntimeMs: 90,
                        analysisRuntimeMs: 18,
                        mismatchCount: 1,
                        missingSpeechSec: 0.10,
                        extraSpeechSec: 0.05,
                        rawQcFlags: 0,
                        treatedQcFlags: 1))
            ],
            manifestFile: null,
            invalidRunFile: null,
            failure: null,
            phaseTransitions:
            [
                new BenchmarkRunPhaseTransition(BenchmarkRunPhase.Completed, DateTimeOffset.UtcNow, "completed")
            ]);
    }

    private static BenchmarkRunResult CreateMalformedDeterministicResult()
    {
        return new BenchmarkRunResult(
            runId: "run-malformed-001",
            moduleId: ModuleIds.BenchmarkRun,
            deterministic: true,
            phase: BenchmarkRunPhase.Completed,
            state: RunState.Completed,
            determinism: null,
            chapterSet: ["chapter-01"],
            chapterSetFingerprint: "fingerprint-malformed",
            chapterOutcomes: [],
            manifestFile: null,
            invalidRunFile: null,
            failure: null,
            phaseTransitions:
            [
                new BenchmarkRunPhaseTransition(BenchmarkRunPhase.Completed, DateTimeOffset.UtcNow, "completed")
            ]);
    }

    private static BenchmarkCompareResult CreateCompatibleCompareResult(DirectoryInfo outputRoot)
    {
        var artifactFile = new FileInfo(Path.Combine(outputRoot.FullName, "benchmark-compare-cmp-cli-001.compare.json"));
        Directory.CreateDirectory(outputRoot.FullName);
        File.WriteAllText(artifactFile.FullName, "{}");

        return new BenchmarkCompareResult(
            compareId: "cmp-cli-001",
            moduleId: ModuleIds.BenchmarkCompare,
            compatibility: new BenchmarkCompareCompatibility(
                status: BenchmarkCompareCompatibilityStatus.Compatible,
                reasons: []),
            metricVerdicts:
            [
                new BenchmarkCompareMetricVerdict(
                    metric: "totalPipelineRuntimeMs",
                    verdict: BenchmarkCompareVerdict.NoChange,
                    baseline: 100,
                    candidate: 125,
                    delta: 25,
                    threshold: new BenchmarkCompareMetricThreshold(
                        value: 25,
                        rationale: "Ignore runtime noise under 25ms.",
                        unit: "ms"),
                    rationale: "Delta landed on threshold boundary.")
            ],
            artifactFile: artifactFile,
            failure: null);
    }

    private static BenchmarkCompareResult CreateIncompatibleCompareResult(DirectoryInfo outputRoot)
    {
        var artifactFile = new FileInfo(Path.Combine(outputRoot.FullName, "benchmark-compare-cmp-cli-incompatible.compare.json"));
        Directory.CreateDirectory(outputRoot.FullName);
        File.WriteAllText(artifactFile.FullName, "{}");

        return new BenchmarkCompareResult(
            compareId: "cmp-cli-incompatible",
            moduleId: ModuleIds.BenchmarkCompare,
            compatibility: new BenchmarkCompareCompatibility(
                status: BenchmarkCompareCompatibilityStatus.Incompatible,
                reasons:
                [
                    new BenchmarkCompareCompatibilityReason(
                        code: BenchmarkCompareCompatibilityReasonCode.ChapterSetMismatch,
                        message: "Chapter-set mismatch between baseline and candidate.",
                        field: "chapterSet",
                        expected: "chapter-01,chapter-02",
                        actual: "chapter-01")
                ]),
            metricVerdicts: [],
            artifactFile: artifactFile,
            failure: null);
    }

    private static BenchmarkCompareResult CreateCompareResultMissingArtifactPath()
    {
        return new BenchmarkCompareResult(
            compareId: "cmp-cli-missing-artifact",
            moduleId: ModuleIds.BenchmarkCompare,
            compatibility: new BenchmarkCompareCompatibility(
                status: BenchmarkCompareCompatibilityStatus.Compatible,
                reasons: []),
            metricVerdicts:
            [
                new BenchmarkCompareMetricVerdict(
                    metric: "totalPipelineRuntimeMs",
                    verdict: BenchmarkCompareVerdict.Improved,
                    baseline: 200,
                    candidate: 150,
                    delta: -50,
                    threshold: new BenchmarkCompareMetricThreshold(
                        value: 25,
                        rationale: "Ignore runtime noise under 25ms.",
                        unit: "ms"),
                    rationale: "Candidate runtime improved.")
            ],
            artifactFile: null,
            failure: null);
    }

    private static BenchmarkChapterMetrics CreateCompletedMetrics(
        long pipelineRuntimeMs,
        long analysisRuntimeMs,
        int mismatchCount,
        double missingSpeechSec,
        double extraSpeechSec,
        int rawQcFlags,
        int treatedQcFlags,
        IReadOnlyList<BenchmarkAudioProcessingActivity>? audioProcessingActivities = null)
    {
        return new BenchmarkChapterMetrics(
            status: BenchmarkMetricsStatus.Completed,
            runtime: new BenchmarkChapterRuntimeMetrics(
                pipelineRuntimeMs: pipelineRuntimeMs,
                analysisRuntimeMs: analysisRuntimeMs),
            quality: CreateQualityMetrics(
                mismatchCount,
                missingSpeechSec,
                extraSpeechSec,
                rawQcFlags,
                treatedQcFlags),
            metricsFailure: null,
            audioProcessingActivities: audioProcessingActivities);
    }

    private static BenchmarkChapterMetrics CreatePartialMetrics(
        string chapterId,
        long pipelineRuntimeMs,
        long analysisRuntimeMs,
        int mismatchCount,
        double missingSpeechSec,
        double extraSpeechSec,
        int rawQcFlags,
        int treatedQcFlags,
        IReadOnlyList<BenchmarkAudioProcessingActivity>? audioProcessingActivities = null)
    {
        return new BenchmarkChapterMetrics(
            status: BenchmarkMetricsStatus.Partial,
            runtime: new BenchmarkChapterRuntimeMetrics(
                pipelineRuntimeMs: pipelineRuntimeMs,
                analysisRuntimeMs: analysisRuntimeMs),
            quality: CreateQualityMetrics(
                mismatchCount,
                missingSpeechSec,
                extraSpeechSec,
                rawQcFlags,
                treatedQcFlags),
            metricsFailure: new BenchmarkMetricsFailure(
                kind: RunFailureKind.Timeout,
                message: "QC analyzer timed out.",
                operation: "qc-analysis",
                chapterId: chapterId,
                resourcePath: "chapter.treated.wav"),
            audioProcessingActivities: audioProcessingActivities);
    }

    private static BenchmarkChapterQualityMetrics CreateQualityMetrics(
        int mismatchCount,
        double missingSpeechSec,
        double extraSpeechSec,
        int rawQcFlags,
        int treatedQcFlags)
    {
        return new BenchmarkChapterQualityMetrics(
            integrity: new BenchmarkAudioIntegrityMetrics(
                durationSec: 12.5,
                rawSpeechSec: 10.0,
                treatedSpeechSec: 9.6,
                missingSpeechSec: missingSpeechSec,
                extraSpeechSec: extraSpeechSec,
                mismatchCount: mismatchCount),
            rawQc: new BenchmarkQcMetrics(
                headSilenceSec: 0.7,
                titleBodyGapSec: 1.1,
                tailSilenceSec: 2.4,
                flagCount: rawQcFlags,
                flags: Enumerable.Range(1, rawQcFlags).Select(index => $"RAW_FLAG_{index}").ToArray()),
            treatedQc: new BenchmarkQcMetrics(
                headSilenceSec: 0.8,
                titleBodyGapSec: 1.2,
                tailSilenceSec: 2.2,
                flagCount: treatedQcFlags,
                flags: Enumerable.Range(1, treatedQcFlags).Select(index => $"TREATED_FLAG_{index}").ToArray()),
            rawLoudness: new BenchmarkLoudnessMetrics(
                durationSec: 12.5,
                samplePeakDbFs: -1.1,
                truePeakDbFs: -0.9,
                overallRmsDbFs: -21.0,
                integratedLufs: -18.0),
            treatedLoudness: new BenchmarkLoudnessMetrics(
                durationSec: 12.4,
                samplePeakDbFs: -1.0,
                truePeakDbFs: -0.8,
                overallRmsDbFs: -20.8,
                integratedLufs: -17.8));
    }

    private static BenchmarkDeterminismContract CreateValidDeterminismContract()
    {
        return new BenchmarkDeterminismContract(
            evaluatedAtUtc: DateTimeOffset.UtcNow,
            verdict: BenchmarkDeterminismVerdict.Valid,
            reasonCodes: [],
            modelProvenance: new BenchmarkModelProvenance(
                BenchmarkReadinessState.Ready,
                BenchmarkModelProvenanceKind.PinnedPath,
                requestedModel: "/models/ggml-large-v3.bin",
                normalizedModelPath: "/models/ggml-large-v3.bin",
                isDeterministic: true,
                summary: "Pinned model path verified.",
                guidance: "Proceed."),
            ffmpeg: new BenchmarkDependencyReadiness("FFmpeg", BenchmarkReadinessState.Ready, "FFmpeg ready."),
            mfa: new BenchmarkDependencyReadiness("MFA", BenchmarkReadinessState.Ready, "MFA ready."),
            cachePolicy: BenchmarkCachePolicy.Default,
            chunkPolicy: BenchmarkChunkPolicy.Default,
            guidance: ["Determinism contract valid."]);
    }

    private static BenchmarkDeterminismContract CreateInvalidDeterminismContract()
    {
        return new BenchmarkDeterminismContract(
            evaluatedAtUtc: DateTimeOffset.UtcNow,
            verdict: BenchmarkDeterminismVerdict.Invalid,
            reasonCodes: [BenchmarkDeterminismReasonCode.MissingModelFile],
            modelProvenance: new BenchmarkModelProvenance(
                BenchmarkReadinessState.Failed,
                BenchmarkModelProvenanceKind.MissingModelFile,
                requestedModel: "/models/missing.bin",
                normalizedModelPath: "/models/missing.bin",
                isDeterministic: false,
                summary: "Model file missing.",
                guidance: "Fix model path."),
            ffmpeg: new BenchmarkDependencyReadiness("FFmpeg", BenchmarkReadinessState.Ready, "FFmpeg ready."),
            mfa: new BenchmarkDependencyReadiness("MFA", BenchmarkReadinessState.Ready, "MFA ready."),
            cachePolicy: BenchmarkCachePolicy.Default,
            chunkPolicy: BenchmarkChunkPolicy.Default,
            guidance: ["Determinism contract invalid."]);
    }

    private string CreateTempDirectory()
    {
        var path = Path.Combine(Path.GetTempPath(), "ams-benchmark-command-contract", Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(path);
        _tempDirectories.Add(path);
        return path;
    }

    private FileInfo WriteTextFile(string path, string content, bool registerPath = true)
    {
        var directory = Path.GetDirectoryName(path);
        if (!string.IsNullOrWhiteSpace(directory))
        {
            Directory.CreateDirectory(directory);
        }

        File.WriteAllText(path, content);

        if (registerPath)
        {
            RegisterPath(path);
        }

        return new FileInfo(path);
    }

    private FileInfo WriteBinaryFile(string path, byte[] content)
    {
        var directory = Path.GetDirectoryName(path);
        if (!string.IsNullOrWhiteSpace(directory))
        {
            Directory.CreateDirectory(directory);
        }

        File.WriteAllBytes(path, content);
        RegisterPath(path);
        return new FileInfo(path);
    }

    private void RegisterPath(string path)
    {
        var directory = Path.GetDirectoryName(path);
        if (string.IsNullOrWhiteSpace(directory))
        {
            return;
        }

        if (!_tempDirectories.Contains(directory, StringComparer.OrdinalIgnoreCase))
        {
            _tempDirectories.Add(directory);
        }
    }

    private sealed record BenchmarkInputFixture(
        string WorkspaceRoot,
        DirectoryInfo WorkDir,
        DirectoryInfo OutputRoot,
        FileInfo BookFile,
        FileInfo BookIndexFile,
        FileInfo ModelFile,
        IReadOnlyList<FileInfo> AudioFiles,
        FileInfo ManifestFile,
        FileInfo InvalidRunFile);
}
