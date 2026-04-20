using System.Text.Json;
using Ams.Core.Application.Benchmark;
using Ams.Core.Application.Runs;

namespace Ams.Tests.Application.Benchmark;

public sealed class BenchmarkCompareContractTests : IDisposable
{
    private readonly List<string> _tempDirectories = new();

    [Fact]
    public void BenchmarkCompareArtifact_SerializesStableTokensAndRoundTrips()
    {
        var artifact = new BenchmarkCompareArtifact(
            compareId: "cmp-001",
            comparedAtUtc: new DateTimeOffset(2026, 4, 15, 21, 0, 0, TimeSpan.Zero),
            baseline: new BenchmarkCompareManifestReference(
                runId: "baseline-001",
                chapterSetFingerprint: "base-fingerprint",
                deterministic: true,
                phase: BenchmarkRunPhase.Completed,
                state: RunState.Completed),
            candidate: new BenchmarkCompareManifestReference(
                runId: "candidate-001",
                chapterSetFingerprint: "candidate-fingerprint",
                deterministic: true,
                phase: BenchmarkRunPhase.Completed,
                state: RunState.Completed),
            compatibility: new BenchmarkCompareCompatibility(
                status: BenchmarkCompareCompatibilityStatus.Incompatible,
                reasons:
                [
                    new BenchmarkCompareCompatibilityReason(
                        code: BenchmarkCompareCompatibilityReasonCode.ChapterSetMismatch,
                        message: "Chapter sets differ.",
                        field: "chapterSet",
                        expected: "chapter-01,chapter-02",
                        actual: "chapter-01")
                ]),
            metricVerdicts:
            [
                new BenchmarkCompareMetricVerdict(
                    metric: "totalPipelineRuntimeMs",
                    verdict: BenchmarkCompareVerdict.Improved,
                    baseline: 180,
                    candidate: 150,
                    delta: -30,
                    threshold: new BenchmarkCompareMetricThreshold(
                        value: 5,
                        rationale: "Runtime deltas below 5ms are noise.",
                        unit: "ms"),
                    rationale: "Candidate reduced pipeline runtime above threshold."),
                new BenchmarkCompareMetricVerdict(
                    metric: "totalMismatchCount",
                    verdict: BenchmarkCompareVerdict.NoChange,
                    baseline: 3,
                    candidate: 3,
                    delta: 0,
                    threshold: new BenchmarkCompareMetricThreshold(
                        value: 1,
                        rationale: "Require at least one mismatch delta.",
                        unit: "count"),
                    rationale: "Mismatch delta stayed within threshold.")
            ]);

        var json = BenchmarkCompareArtifact.Serialize(artifact);
        using var document = JsonDocument.Parse(json);
        var root = document.RootElement;

        Assert.Equal("cmp-001", root.GetProperty("compareId").GetString());
        Assert.Equal(
            "Incompatible",
            root.GetProperty("compatibility").GetProperty("status").GetString());

        var reasonCode = root.GetProperty("compatibility")
            .GetProperty("reasons")
            .EnumerateArray()
            .Single()
            .GetProperty("code")
            .GetString();

        Assert.Equal("ChapterSetMismatch", reasonCode);

        var verdictTokens = root
            .GetProperty("metricVerdicts")
            .EnumerateArray()
            .Select(metricNode => metricNode.GetProperty("verdict").GetString() ?? string.Empty)
            .ToArray();

        Assert.Equal(["Improved", "NoChange"], verdictTokens);

        var roundTrip = BenchmarkCompareArtifact.Deserialize(json);
        Assert.Equal(artifact.CompareId, roundTrip.CompareId);
        Assert.Equal(artifact.Compatibility.Status, roundTrip.Compatibility.Status);
        Assert.Equal(artifact.MetricVerdicts.Select(metric => metric.Verdict), roundTrip.MetricVerdicts.Select(metric => metric.Verdict));
    }

    [Fact]
    public void BenchmarkCompareArtifact_DeserializeRejectsInvalidEnumTokens()
    {
        const string malformedJson = """
                                     {
                                       "compareId": "cmp-invalid-enum",
                                       "comparedAtUtc": "2026-04-15T21:00:00Z",
                                       "baseline": {
                                         "runId": "baseline-001",
                                         "chapterSetFingerprint": "base",
                                         "deterministic": true,
                                         "phase": "Completed",
                                         "state": "Completed"
                                       },
                                       "candidate": {
                                         "runId": "candidate-001",
                                         "chapterSetFingerprint": "candidate",
                                         "deterministic": true,
                                         "phase": "Completed",
                                         "state": "Completed"
                                       },
                                       "compatibility": {
                                         "status": "Broken",
                                         "reasons": [
                                           {
                                             "code": "ChapterSetMismatch",
                                             "message": "bad"
                                           }
                                         ]
                                       },
                                       "metricVerdicts": []
                                     }
                                     """;

        var exception = Assert.ThrowsAny<Exception>(() => BenchmarkCompareArtifact.Deserialize(malformedJson));

        Assert.Contains("status", exception.ToString(), StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void BenchmarkCompareMetricThreshold_RejectsNegativeValues()
    {
        var exception = Assert.Throws<ArgumentOutOfRangeException>(() =>
            new BenchmarkCompareMetricThreshold(
                value: -0.1,
                rationale: "negative threshold"));

        Assert.Equal("value", exception.ParamName);
    }

    [Fact]
    public async Task BenchmarkRunArtifactStore_WriteCompareAsync_PersistsCompareArtifactWithStablePath()
    {
        var outputRoot = new DirectoryInfo(CreateTempDirectory());
        var store = new BenchmarkRunArtifactStore();
        var artifact = CreateCompatibleArtifact(compareId: "cmp-store-001");

        var artifactFile = await store.WriteCompareAsync(outputRoot, artifact);

        Assert.True(artifactFile.Exists);
        Assert.Equal("benchmark-compare-cmp-store-001.compare.json", artifactFile.Name);

        var persisted = await File.ReadAllTextAsync(artifactFile.FullName);
        var roundTrip = BenchmarkCompareArtifact.Deserialize(persisted);

        Assert.Equal("cmp-store-001", roundTrip.CompareId);
        Assert.True(roundTrip.Compatibility.IsCompatible);
    }

    [Fact]
    public async Task BenchmarkRunArtifactStore_WriteCompareAsync_FailsClosedWhenCompareArtifactAlreadyExists()
    {
        var trackedFiles = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        var writeCalls = 0;

        var store = new BenchmarkRunArtifactStore(
            ensureDirectory: _ => { },
            writeAllText: (path, _) =>
            {
                Interlocked.Increment(ref writeCalls);
                lock (trackedFiles)
                {
                    trackedFiles.Add(path);
                }
            },
            fileExists: path =>
            {
                lock (trackedFiles)
                {
                    return trackedFiles.Contains(path);
                }
            });

        var outputRoot = new DirectoryInfo(CreateTempDirectory());
        var artifact = CreateCompatibleArtifact(compareId: "cmp-lock-001");

        _ = await store.WriteCompareAsync(outputRoot, artifact);

        var duplicateException = await Assert.ThrowsAsync<IOException>(async () =>
            await store.WriteCompareAsync(outputRoot, artifact));

        Assert.Contains("already exists", duplicateException.Message, StringComparison.OrdinalIgnoreCase);
        Assert.Equal(1, writeCalls);
    }

    [Fact]
    public async Task BenchmarkRunArtifactStore_WriteCompareAsync_RejectsUnsafeCompareIdentifiers()
    {
        var store = new BenchmarkRunArtifactStore();
        var outputRoot = new DirectoryInfo(CreateTempDirectory());
        var artifact = CreateCompatibleArtifact(compareId: "../escape-attempt");

        var exception = await Assert.ThrowsAsync<ArgumentException>(async () =>
            await store.WriteCompareAsync(outputRoot, artifact));

        Assert.Contains("parent-directory", exception.Message, StringComparison.OrdinalIgnoreCase);
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
                // Best-effort cleanup for temporary test directories.
            }
        }
    }

    private static BenchmarkCompareArtifact CreateCompatibleArtifact(string compareId)
    {
        return new BenchmarkCompareArtifact(
            compareId: compareId,
            comparedAtUtc: new DateTimeOffset(2026, 4, 15, 21, 10, 0, TimeSpan.Zero),
            baseline: new BenchmarkCompareManifestReference(
                runId: "baseline-001",
                chapterSetFingerprint: "fingerprint-001",
                deterministic: false,
                phase: BenchmarkRunPhase.Completed,
                state: RunState.Completed),
            candidate: new BenchmarkCompareManifestReference(
                runId: "candidate-001",
                chapterSetFingerprint: "fingerprint-001",
                deterministic: false,
                phase: BenchmarkRunPhase.Completed,
                state: RunState.Completed),
            compatibility: new BenchmarkCompareCompatibility(
                status: BenchmarkCompareCompatibilityStatus.Compatible,
                reasons: []),
            metricVerdicts:
            [
                new BenchmarkCompareMetricVerdict(
                    metric: "totalPipelineRuntimeMs",
                    verdict: BenchmarkCompareVerdict.NoChange,
                    baseline: 100,
                    candidate: 100,
                    delta: 0,
                    threshold: new BenchmarkCompareMetricThreshold(5, "threshold"),
                    rationale: "No material runtime delta.")
            ]);
    }

    private string CreateTempDirectory()
    {
        var path = Path.Combine(Path.GetTempPath(), "ams-benchmark-compare-contract", Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(path);
        _tempDirectories.Add(path);
        return path;
    }
}
