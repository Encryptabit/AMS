using Ams.Core.Application.Benchmark;

namespace Ams.Tests.Application.Benchmark;

public sealed class BenchmarkDeterminismGateTests
{
    [Fact]
    public async Task EvaluateAsync_PinnedModelAndDependenciesReady_AcceptsDeterministicRun()
    {
        var pinnedPath = Path.GetFullPath(Path.Combine(Path.GetTempPath(), "models", "ggml-large-v3.bin"));
        var gate = CreateGate(
            fileExists: candidate => string.Equals(Path.GetFullPath(candidate), pinnedPath, StringComparison.OrdinalIgnoreCase));

        var result = await gate.EvaluateAsync(CreateRequest(pinnedPath));

        Assert.Equal(BenchmarkDeterminismVerdict.Valid, result.Verdict);
        Assert.Empty(result.ReasonCodes);
        Assert.Equal(BenchmarkModelProvenanceKind.PinnedPath, result.ModelProvenance.SourceKind);
        Assert.True(result.ModelProvenance.IsDeterministic);
        Assert.Equal(BenchmarkReadinessState.Ready, result.Ffmpeg.State);
        Assert.Equal(BenchmarkReadinessState.Ready, result.Mfa.State);
        Assert.True(result.CachePolicy.AllowsCachedPipelineArtifacts);
        Assert.False(result.ChunkPolicy.DisableChunkPlan);
    }

    [Fact]
    public async Task EvaluateAsync_BlankModelPath_RejectsDeterministicRun()
    {
        var gate = CreateGate();

        var result = await gate.EvaluateAsync(CreateRequest("   "));

        Assert.Equal(BenchmarkDeterminismVerdict.Invalid, result.Verdict);
        Assert.Equal([BenchmarkDeterminismReasonCode.MissingExplicitModel], result.ReasonCodes);
        Assert.Equal(BenchmarkModelProvenanceKind.MissingExplicitModel, result.ModelProvenance.SourceKind);
    }

    [Fact]
    public async Task EvaluateAsync_AliasOnlyModel_RejectsDeterministicRun()
    {
        var gate = CreateGate();

        var result = await gate.EvaluateAsync(CreateRequest("large-v3"));

        Assert.Equal(BenchmarkDeterminismVerdict.Invalid, result.Verdict);
        Assert.Equal([BenchmarkDeterminismReasonCode.AliasOnlyModel], result.ReasonCodes);
        Assert.Equal(BenchmarkModelProvenanceKind.AliasOnly, result.ModelProvenance.SourceKind);
    }

    [Fact]
    public async Task EvaluateAsync_MalformedModelPath_RejectsDeterministicRun()
    {
        var gate = CreateGate();

        var result = await gate.EvaluateAsync(CreateRequest("bad\0model.bin"));

        Assert.Equal(BenchmarkDeterminismVerdict.Invalid, result.Verdict);
        Assert.Equal([BenchmarkDeterminismReasonCode.InvalidModelInput], result.ReasonCodes);
        Assert.Equal(BenchmarkModelProvenanceKind.InvalidModelInput, result.ModelProvenance.SourceKind);
    }

    [Fact]
    public async Task EvaluateAsync_NonExistentPinnedPath_RejectsDeterministicRun()
    {
        var missingPath = Path.GetFullPath(Path.Combine(Path.GetTempPath(), "missing", "ggml-large-v3.bin"));
        var gate = CreateGate(fileExists: _ => false);

        var result = await gate.EvaluateAsync(CreateRequest(missingPath));

        Assert.Equal(BenchmarkDeterminismVerdict.Invalid, result.Verdict);
        Assert.Equal([BenchmarkDeterminismReasonCode.MissingModelFile], result.ReasonCodes);
        Assert.Equal(BenchmarkModelProvenanceKind.MissingModelFile, result.ModelProvenance.SourceKind);
    }

    [Fact]
    public async Task EvaluateAsync_DependencyProbeThrows_RejectsWithDependencyFailureReason()
    {
        var gate = CreateGate(
            probe: _ => throw new InvalidOperationException("probe blew up"),
            fileExists: _ => true);

        var result = await gate.EvaluateAsync(CreateRequest("/tmp/models/pinned.bin"));

        Assert.Equal(BenchmarkDeterminismVerdict.Invalid, result.Verdict);
        Assert.Equal([BenchmarkDeterminismReasonCode.DependencyProbeFailed], result.ReasonCodes);
        Assert.Equal(BenchmarkReadinessState.Unknown, result.Ffmpeg.State);
        Assert.Equal(BenchmarkReadinessState.Unknown, result.Mfa.State);
    }

    [Fact]
    public async Task EvaluateAsync_DependencyProbeTimesOut_RejectsWithTimeoutReason()
    {
        var gate = CreateGate(
            probe: _ => throw new TimeoutException("probe timeout"),
            fileExists: _ => true);

        var result = await gate.EvaluateAsync(CreateRequest("/tmp/models/pinned.bin"));

        Assert.Equal(BenchmarkDeterminismVerdict.Invalid, result.Verdict);
        Assert.Equal([BenchmarkDeterminismReasonCode.DependencyProbeTimeout], result.ReasonCodes);
        Assert.Equal(BenchmarkReadinessState.Unknown, result.Ffmpeg.State);
        Assert.Equal(BenchmarkReadinessState.Unknown, result.Mfa.State);
    }

    [Fact]
    public async Task EvaluateAsync_MalformedDependencySnapshot_RejectsWithMalformedReason()
    {
        var malformedSnapshot = new BenchmarkDependencyReadinessSnapshot(
            capturedAtUtc: DateTimeOffset.UtcNow,
            ffmpeg: new BenchmarkDependencyReadiness(
                dependency: "ffmpeg-runtime",
                state: BenchmarkReadinessState.Ready,
                summary: "FFmpeg ready."),
            mfa: new BenchmarkDependencyReadiness(
                dependency: "MFA",
                state: BenchmarkReadinessState.Ready,
                summary: "MFA ready."));

        var gate = CreateGate(
            probe: _ => Task.FromResult(malformedSnapshot),
            fileExists: _ => true);

        var result = await gate.EvaluateAsync(CreateRequest("/tmp/models/pinned.bin"));

        Assert.Equal(BenchmarkDeterminismVerdict.Invalid, result.Verdict);
        Assert.Equal([BenchmarkDeterminismReasonCode.DependencyProbeMalformed], result.ReasonCodes);
    }

    [Fact]
    public async Task EvaluateAsync_MultipleRejections_AreOrderedAndDeduplicated()
    {
        var nonReady = new BenchmarkDependencyReadinessSnapshot(
            capturedAtUtc: DateTimeOffset.UtcNow,
            ffmpeg: new BenchmarkDependencyReadiness(
                dependency: "FFmpeg",
                state: BenchmarkReadinessState.Failed,
                summary: "FFmpeg failed."),
            mfa: new BenchmarkDependencyReadiness(
                dependency: "MFA",
                state: BenchmarkReadinessState.Warning,
                summary: "MFA warming."));

        var gate = CreateGate(
            probe: _ => Task.FromResult(nonReady),
            fileExists: _ => false);

        var result = await gate.EvaluateAsync(CreateRequest("   "));

        Assert.Equal(BenchmarkDeterminismVerdict.Invalid, result.Verdict);
        Assert.Equal(
        [
            BenchmarkDeterminismReasonCode.MissingExplicitModel,
            BenchmarkDeterminismReasonCode.FfmpegNotReady,
            BenchmarkDeterminismReasonCode.MfaNotReady
        ], result.ReasonCodes);

        Assert.Equal(result.ReasonCodes.Count, result.ReasonCodes.Distinct().Count());
    }

    private static BenchmarkDeterminismGate CreateGate(
        Func<CancellationToken, Task<BenchmarkDependencyReadinessSnapshot>>? probe = null,
        Func<string, bool>? fileExists = null,
        Func<DateTimeOffset>? utcNow = null)
    {
        var effectiveProbe = probe ?? (_ => Task.FromResult(CreateReadySnapshot()));

        return new BenchmarkDeterminismGate(
            new DelegatingDependencyProbe(effectiveProbe),
            fileExists ?? (_ => false),
            utcNow);
    }

    private static BenchmarkDeterminismGateRequest CreateRequest(string? model)
    {
        return new BenchmarkDeterminismGateRequest(
            requestedModel: model,
            cachePolicy: BenchmarkCachePolicy.Default,
            chunkPolicy: BenchmarkChunkPolicy.Default);
    }

    private static BenchmarkDependencyReadinessSnapshot CreateReadySnapshot()
    {
        return new BenchmarkDependencyReadinessSnapshot(
            capturedAtUtc: DateTimeOffset.UtcNow,
            ffmpeg: new BenchmarkDependencyReadiness(
                dependency: "FFmpeg",
                state: BenchmarkReadinessState.Ready,
                summary: "FFmpeg ready."),
            mfa: new BenchmarkDependencyReadiness(
                dependency: "MFA",
                state: BenchmarkReadinessState.Ready,
                summary: "MFA ready."));
    }

    private sealed class DelegatingDependencyProbe(
        Func<CancellationToken, Task<BenchmarkDependencyReadinessSnapshot>> callback)
        : IBenchmarkDependencyReadinessProbe
    {
        private readonly Func<CancellationToken, Task<BenchmarkDependencyReadinessSnapshot>> _callback = callback;

        public Task<BenchmarkDependencyReadinessSnapshot> CaptureAsync(CancellationToken cancellationToken = default)
            => _callback(cancellationToken);
    }
}
