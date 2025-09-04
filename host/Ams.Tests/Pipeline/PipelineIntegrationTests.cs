using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Ams.Core;
using Ams.Core.Pipeline;
using Xunit;

namespace Ams.Tests.Pipeline;

public class PipelineIntegrationTests : IDisposable
{
    private readonly string _tempDir;
    private readonly string _testWavPath;

    public PipelineIntegrationTests()
    {
        _tempDir = Path.Combine(Path.GetTempPath(), "ams_test_" + Guid.NewGuid().ToString("N")[..8]);
        Directory.CreateDirectory(_tempDir);
        _testWavPath = Path.Combine(_tempDir, "test.wav");
        CreateTestWavFile(_testWavPath);
    }

    public void Dispose()
    {
        try { if (Directory.Exists(_tempDir)) Directory.Delete(_tempDir, true); } catch { }
    }

    [Fact]
    public async Task DetectSilence_CreatesArtifacts()
    {
        var workDir = Path.Combine(_tempDir, "test.wav.ams");
        var stage = new DetectSilenceStage(workDir, new FfmpegSilenceDetector(), new DefaultProcessRunner(), new SilenceDetectionParams(-30.0, 0.3));
        var inputMeta = new InputMetadata(_testWavPath, "test-sha256", 5.0, 1024, DateTime.UtcNow);
        var manifest = ManifestV2.CreateNew(inputMeta);
        var ok = await stage.RunAsync(manifest);
        Assert.True(ok);
        Assert.True(File.Exists(Path.Combine(workDir, "timeline", "silence.json")));
        Assert.Equal("completed", manifest.Stages["timeline"].Status.Status);
    }

    [Fact]
    public async Task DetectThenPlan_EndToEnd()
    {
        var workDir = Path.Combine(_tempDir, "test.wav.ams");
        var manifest = ManifestV2.CreateNew(new InputMetadata(_testWavPath, "test-sha256", 300.0, 1024, DateTime.UtcNow));
        // Mock ffmpeg to avoid dependency in CI
        var mock = new MockProcessRunner();
        var detect = new DetectSilenceStage(workDir, new FfmpegSilenceDetector(), mock, new SilenceDetectionParams(-30.0, 0.3));
        var plan = new PlanWindowsStage(workDir, new SilenceWindowPlanner(), new WindowPlanningParams(60.0, 90.0, 75.0, false));
        Assert.True(await detect.RunAsync(manifest));
        Assert.True(await plan.RunAsync(manifest));
        var windowsPath = Path.Combine(workDir, "plan", "windows.json");
        Assert.True(File.Exists(windowsPath));
        var planJson = await File.ReadAllTextAsync(windowsPath);
        var planV2 = JsonSerializer.Deserialize<WindowPlanV2>(planJson);
        Assert.NotNull(planV2);
        Assert.True(planV2!.Windows.Count > 0);
    }

    private void CreateTestWavFile(string path)
    {
        var sampleRate = 44100;
        var channels = 1;
        var samples = sampleRate;
        var buffer = new AudioBuffer(channels, sampleRate, samples);
        WavIo.WriteInt16Pcm(path, buffer);
    }
}

public class MockProcessRunner : IProcessRunner
{
    public Task<ProcessResult> RunAsync(string fileName, string arguments, CancellationToken ct = default)
    {
        if (fileName == "ffmpeg")
        {
            if (arguments.Contains("-version"))
            {
                return Task.FromResult(new ProcessResult(0, "ffmpeg version 6.1.1-static", ""));
            }
            else if (arguments.Contains("silencedetect"))
            {
                // Provide several silence windows across a 5-minute audio to enable planning
                var stderr = @"
[silencedetect @ 0x123] silence_start: 74.5
[silencedetect @ 0x123] silence_end: 75.8 | silence_duration: 1.3
[silencedetect @ 0x123] silence_start: 149.9
[silencedetect @ 0x123] silence_end: 151.2 | silence_duration: 1.3
[silencedetect @ 0x123] silence_start: 224.7
[silencedetect @ 0x123] silence_end: 226.0 | silence_duration: 1.3
";
                return Task.FromResult(new ProcessResult(0, "", stderr));
            }
        }
        else if (fileName == "git" && arguments == "rev-parse HEAD")
        {
            return Task.FromResult(new ProcessResult(0, "abc123def456", ""));
        }
        return Task.FromResult(new ProcessResult(1, "", "Unknown command"));
    }
}
