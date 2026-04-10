using Ams.Workstation.Server.Services.Prep;

namespace Ams.Tests.Workstation.Prep;

public sealed class WorkstationPrepRuntimeReadinessTests
{
    [Fact]
    public async Task CaptureAsync_PinnedModelAndDependenciesReady_IsDeterministicAndReady()
    {
        var existing = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        {
            Path.GetFullPath("/tmp/models/ggml-large-v3.bin")
        };

        var probe = CreateProbe(
            fileExists: path => existing.Contains(Path.GetFullPath(path)));

        var snapshot = await probe.CaptureAsync(
            new PrepPipelineRunRequest
            {
                Asr = new PrepPipelineAsrRequest
                {
                    Model = "/tmp/models/ggml-large-v3.bin"
                }
            },
            chapterDisplayTitle: "Chapter 1",
            chapterId: "chapter-1");

        Assert.True(snapshot.IsReady);
        Assert.True(snapshot.IsDeterministic);
        Assert.Equal(PrepRuntimeReadinessState.Ready, snapshot.ModelProvenance.State);
        Assert.Equal(PrepModelProvenanceKind.PinnedPath, snapshot.ModelProvenance.SourceKind);
        Assert.Equal(PrepRuntimeReadinessState.Ready, snapshot.Ffmpeg.State);
        Assert.Equal(PrepRuntimeReadinessState.Ready, snapshot.Mfa.State);
    }

    [Fact]
    public async Task CaptureAsync_BlankModelPath_IsNonDeterministicWarning()
    {
        var probe = CreateProbe();

        var snapshot = await probe.CaptureAsync(
            new PrepPipelineRunRequest
            {
                Asr = new PrepPipelineAsrRequest
                {
                    Model = "   "
                }
            },
            chapterDisplayTitle: "Chapter 1",
            chapterId: "chapter-1");

        Assert.False(snapshot.IsReady);
        Assert.False(snapshot.IsDeterministic);
        Assert.Equal(PrepRuntimeReadinessState.Warning, snapshot.ModelProvenance.State);
        Assert.Equal(PrepModelProvenanceKind.MissingExplicitModel, snapshot.ModelProvenance.SourceKind);
        Assert.Contains("explicit", snapshot.ModelProvenance.Guidance, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task CaptureAsync_AliasOnlyModel_IsNonDeterministicWarning()
    {
        var probe = CreateProbe();

        var snapshot = await probe.CaptureAsync(
            new PrepPipelineRunRequest
            {
                Asr = new PrepPipelineAsrRequest
                {
                    Model = "large-v3"
                }
            },
            chapterDisplayTitle: "Chapter 1",
            chapterId: "chapter-1");

        Assert.False(snapshot.IsReady);
        Assert.False(snapshot.IsDeterministic);
        Assert.Equal(PrepRuntimeReadinessState.Warning, snapshot.ModelProvenance.State);
        Assert.Equal(PrepModelProvenanceKind.AliasOnly, snapshot.ModelProvenance.SourceKind);
    }

    [Fact]
    public async Task CaptureAsync_NonexistentPinnedModelPath_FailsDeterminism()
    {
        var probe = CreateProbe(fileExists: _ => false);

        var snapshot = await probe.CaptureAsync(
            new PrepPipelineRunRequest
            {
                Asr = new PrepPipelineAsrRequest
                {
                    Model = "/missing/models/custom.bin"
                }
            },
            chapterDisplayTitle: "Chapter 1",
            chapterId: "chapter-1");

        Assert.False(snapshot.IsReady);
        Assert.False(snapshot.IsDeterministic);
        Assert.Equal(PrepRuntimeReadinessState.Failed, snapshot.ModelProvenance.State);
        Assert.Equal(PrepModelProvenanceKind.MissingModelFile, snapshot.ModelProvenance.SourceKind);
        Assert.Contains("does not exist", snapshot.ModelProvenance.Summary, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task CaptureAsync_MalformedModelPathInput_SurfacesValidationWarning()
    {
        var probe = CreateProbe();

        var snapshot = await probe.CaptureAsync(
            new PrepPipelineRunRequest
            {
                Asr = new PrepPipelineAsrRequest
                {
                    Model = "bad\0model.bin"
                }
            },
            chapterDisplayTitle: "Chapter 1",
            chapterId: "chapter-1");

        Assert.False(snapshot.IsReady);
        Assert.Equal(PrepRuntimeReadinessState.Warning, snapshot.ModelProvenance.State);
        Assert.Equal(PrepModelProvenanceKind.InvalidModelInput, snapshot.ModelProvenance.SourceKind);
        Assert.Contains("invalid", snapshot.ModelProvenance.Summary, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task CaptureAsync_DependencyFailures_AreRecordedWithoutThrowing()
    {
        var probe = CreateProbe(
            ffmpegProbe: _ => Task.FromResult(new PrepRuntimeDependencyReadiness(
                "FFmpeg",
                PrepRuntimeReadinessState.Failed,
                "FFmpeg check failed (exit 2).",
                Detail: "stderr: binaries missing",
                ExitCode: 2)),
            mfaProbe: _ => Task.FromResult(new PrepRuntimeDependencyReadiness(
                "MFA",
                PrepRuntimeReadinessState.Failed,
                "MFA readiness timed out.",
                Detail: "Timed out after 30s.")));

        var snapshot = await probe.CaptureAsync(
            new PrepPipelineRunRequest
            {
                Asr = new PrepPipelineAsrRequest
                {
                    Model = "/tmp/models/pinned.bin"
                }
            },
            chapterDisplayTitle: "Chapter 1",
            chapterId: "chapter-1");

        Assert.False(snapshot.IsReady);
        Assert.Equal(PrepRuntimeReadinessState.Failed, snapshot.Ffmpeg.State);
        Assert.Equal(PrepRuntimeReadinessState.Failed, snapshot.Mfa.State);
        Assert.Contains(snapshot.Notes, note => note.Contains("FFmpeg", StringComparison.OrdinalIgnoreCase));
        Assert.Contains(snapshot.Notes, note => note.Contains("MFA", StringComparison.OrdinalIgnoreCase));
    }

    [Fact]
    public async Task CaptureAsync_ReusesCachedProbeWithinTtl()
    {
        var now = new DateTimeOffset(2026, 4, 10, 9, 0, 0, TimeSpan.Zero);
        var ffmpegCalls = 0;
        var mfaCalls = 0;

        var probe = CreateProbe(
            ffmpegProbe: _ =>
            {
                ffmpegCalls++;
                return Task.FromResult(new PrepRuntimeDependencyReadiness(
                    "FFmpeg",
                    PrepRuntimeReadinessState.Ready,
                    "FFmpeg ready."));
            },
            mfaProbe: _ =>
            {
                mfaCalls++;
                return Task.FromResult(new PrepRuntimeDependencyReadiness(
                    "MFA",
                    PrepRuntimeReadinessState.Ready,
                    "MFA ready."));
            },
            utcNow: () => now,
            fileExists: _ => true,
            cacheTtl: TimeSpan.FromMinutes(1));

        var request = new PrepPipelineRunRequest
        {
            Asr = new PrepPipelineAsrRequest
            {
                Model = "/tmp/models/pinned.bin"
            }
        };

        var first = await probe.CaptureAsync(request, "Chapter 1", "chapter-1");
        now = now.AddSeconds(5);
        var second = await probe.CaptureAsync(request, "Chapter 1", "chapter-1");

        Assert.False(first.ReusedCachedProbe);
        Assert.True(second.ReusedCachedProbe);
        Assert.Equal(1, ffmpegCalls);
        Assert.Equal(1, mfaCalls);
    }

    [Fact]
    public async Task CaptureAsync_MalformedDependencySnapshot_Throws()
    {
        var probe = CreateProbe(
            ffmpegProbe: _ => Task.FromResult(new PrepRuntimeDependencyReadiness(
                "FFmpeg",
                PrepRuntimeReadinessState.Ready,
                string.Empty)),
            mfaProbe: _ => Task.FromResult(new PrepRuntimeDependencyReadiness(
                "MFA",
                PrepRuntimeReadinessState.Ready,
                "MFA ready.")));

        await Assert.ThrowsAsync<InvalidOperationException>(() => probe.CaptureAsync(
            new PrepPipelineRunRequest
            {
                Asr = new PrepPipelineAsrRequest
                {
                    Model = "/tmp/models/pinned.bin"
                }
            },
            "Chapter 1",
            "chapter-1"));
    }

    private static PrepRuntimeReadinessProbe CreateProbe(
        Func<CancellationToken, Task<PrepRuntimeDependencyReadiness>>? ffmpegProbe = null,
        Func<CancellationToken, Task<PrepRuntimeDependencyReadiness>>? mfaProbe = null,
        Func<string, bool>? fileExists = null,
        Func<DateTimeOffset>? utcNow = null,
        TimeSpan? cacheTtl = null)
    {
        return new PrepRuntimeReadinessProbe(
            ffmpegProbe: ffmpegProbe ?? (_ => Task.FromResult(new PrepRuntimeDependencyReadiness(
                "FFmpeg",
                PrepRuntimeReadinessState.Ready,
                "FFmpeg ready."))),
            mfaProbe: mfaProbe ?? (_ => Task.FromResult(new PrepRuntimeDependencyReadiness(
                "MFA",
                PrepRuntimeReadinessState.Ready,
                "MFA ready."))),
            fileExists: fileExists ?? (_ => false),
            utcNow: utcNow,
            ffmpegScriptPath: Path.Combine(Path.GetTempPath(), "setup_ffmpeg.py"),
            cacheTtl: cacheTtl);
    }
}
