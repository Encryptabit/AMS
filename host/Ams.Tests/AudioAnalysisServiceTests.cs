using System;
using System.IO;
using System.Threading.Tasks;
using Ams.Core.Asr.Pipeline;
using Ams.Tests.TestHelpers;
using Xunit;

namespace Ams.Tests.Asr;

public sealed class AudioAnalysisServiceTests : IAsyncLifetime
{
    private readonly string _audioPath;

    public AudioAnalysisServiceTests()
    {
        _audioPath = Path.Combine(Path.GetTempPath(), "ams-audio-test-" + Guid.NewGuid().ToString("N") + ".wav");
        File.WriteAllBytes(_audioPath, Array.Empty<byte>());
    }

    public Task InitializeAsync() => Task.CompletedTask;

    public Task DisposeAsync()
    {
        if (File.Exists(_audioPath))
        {
            File.Delete(_audioPath);
        }
        return Task.CompletedTask;
    }

    [Fact]
    public async Task GetVolumeAnalysis_HotBoundaries_ReturnsNudgedStart()
    {
        var runner = new TestAudioProcessRunner(simulateHot: true);
        var service = new AudioAnalysisService(runner);
        var parameters = new VolumeAnalysisParams(-45.0, -35.0, 0.080, 0.050, 3500.0, 12000.0, 5.0, 2.5, 0.003, 8, 3, 0.012, 0.015);

        var result = await service.GetVolumeAnalysis(_audioPath, 0.0, 1.0, parameters);

        Assert.True(result.LeftNudges > 0);
        Assert.True(result.SuggestedStart > 0.0);
        Assert.True(result.SuggestedEnd >= 1.0);
        Assert.True(result.LeftEdgeHot);
    }

    [Fact]
    public async Task GetVolumeAnalysis_QuietSegment_DoesNotAdjust()
    {
        var runner = new TestAudioProcessRunner(simulateHot: false);
        var service = new AudioAnalysisService(runner);
        var parameters = new VolumeAnalysisParams(-45.0, -35.0, 0.080, 0.050, 3500.0, 12000.0, 5.0, 2.5, 0.003, 8, 3, 0.012, 0.015);

        var result = await service.GetVolumeAnalysis(_audioPath, 0.5, 1.0, parameters);

        Assert.False(result.LeftEdgeHot);
        Assert.Equal(0.5, result.SuggestedStart, 3);
        Assert.Equal(1.5, result.SuggestedEnd, 3);
    }
}
