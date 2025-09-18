using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using System.Threading.Tasks;
using Ams.Core;
using Ams.Core.Asr.Pipeline;
using Ams.Core.Pipeline;
using Ams.Tests.TestHelpers;
using Xunit;

namespace Ams.Tests.Pipeline;

public sealed class ChunkAudioStageTests : IAsyncLifetime
{
    private readonly string _workDir;
    private readonly string _audioPath;

    public ChunkAudioStageTests()
    {
        _workDir = Path.Combine(Path.GetTempPath(), "ams-chunk-test-" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(_workDir);
        _audioPath = Path.Combine(_workDir, "input.wav");
        File.WriteAllBytes(_audioPath, Array.Empty<byte>());
    }

    public Task InitializeAsync()
    {
        var planDir = Path.Combine(_workDir, "plan");
        Directory.CreateDirectory(planDir);
        var plan = new WindowPlanV2(
            new List<ChunkSpan> { new ChunkSpan(0.0, 1.0) },
            new WindowPlanningParams(30.0, 60.0, 45.0, true),
            0.0,
            false);
        var planJson = JsonSerializer.Serialize(plan, new JsonSerializerOptions { WriteIndented = true });
        File.WriteAllText(Path.Combine(planDir, "windows.json"), planJson);
        return Task.CompletedTask;
    }

    public Task DisposeAsync()
    {
        if (Directory.Exists(_workDir))
        {
            Directory.Delete(_workDir, true);
        }
        return Task.CompletedTask;
    }

    [Fact]
    public async Task RunAsync_ProducesNudgedChunk()
    {
        var runner = new TestAudioProcessRunner(simulateHot: true);
        var stage = new ChunkAudioStage(_workDir, runner, new ChunkingParams("wav", 44100, new VolumeAnalysisParams(-45.0, -35.0, 0.080, 0.050, 3500.0, 12000.0, 5.0, 2.5, 0.003, 8, 3, 0.012, 0.015)));

        var manifest = ManifestV2.CreateNew(new InputMetadata(_audioPath, "sha", 10.0, 0, DateTime.UtcNow));
        var ok = await stage.RunAsync(manifest);
        Assert.True(ok);

        var indexPath = Path.Combine(_workDir, "chunks", "index.json");
        Assert.True(File.Exists(indexPath));
        var chunkIndex = JsonSerializer.Deserialize<ChunkIndex>(await File.ReadAllTextAsync(indexPath));
        Assert.NotNull(chunkIndex);
        Assert.NotEmpty(chunkIndex!.Chunks);
        Assert.True(chunkIndex!.Chunks[0].Span.Start > 0.0);
    }
}
