using Ams.Core.Artifacts;
using Ams.Core.Processors;

namespace Ams.Tests.Audio;

public class AudioProcessorAnalysisTests
{
    [Fact]
    public void MeasureRms_EmitsStopwatchActivity()
    {
        var buffer = CreateConstantBuffer(amplitude: 0.5f, sampleRate: 16_000, durationSec: 0.5);
        var activities = new List<AudioProcessorActivity>();

        using (AudioProcessor.BeginActivityCapture(activity => activities.Add(activity)))
        {
            _ = AudioProcessor.MeasureRms(buffer, 0.0, 0.25);
        }

        var rmsActivity = Assert.Single(
            activities,
            activity => string.Equals(activity.Function, nameof(AudioProcessor.MeasureRms), StringComparison.Ordinal));

        Assert.True(rmsActivity.Succeeded);
        Assert.True(rmsActivity.DurationMs >= 0);
        Assert.True(rmsActivity.DurationUs >= 0);
        Assert.True(rmsActivity.DurationUs >= rmsActivity.DurationMs * 1000L);
    }

    [Fact]
    public void EncodeWavToStream_RepeatedFastCalls_AccumulateMicrosecondRuntime()
    {
        var buffer = CreateConstantBuffer(amplitude: 0.4f, sampleRate: 16_000, durationSec: 0.05);
        var activities = new List<AudioProcessorActivity>();

        using (AudioProcessor.BeginActivityCapture(activity => activities.Add(activity)))
        {
            for (var index = 0; index < 32; index++)
            {
                using var _ = AudioProcessor.EncodeWavToStream(buffer);
            }
        }

        var encodeActivities = activities
            .Where(activity => string.Equals(activity.Function, nameof(AudioProcessor.EncodeWavToStream), StringComparison.Ordinal))
            .ToArray();

        Assert.NotEmpty(encodeActivities);
        Assert.All(encodeActivities, activity => Assert.True(activity.DurationUs >= activity.DurationMs * 1000L));
        Assert.True(encodeActivities.Sum(activity => activity.DurationUs) > 0);
    }

    [Fact]
    public void AnalyzeLoudness_BufferOverload_EmitsStopwatchActivity()
    {
        var buffer = CreateConstantBuffer(amplitude: 0.25f, sampleRate: 16_000, durationSec: 0.25);
        var activities = new List<AudioProcessorActivity>();

        using (AudioProcessor.BeginActivityCapture(activity => activities.Add(activity)))
        {
            _ = AudioProcessor.AnalyzeLoudness(buffer, new AudioLoudnessAnalysisOptions
            {
                ComputeIntegratedLufs = false,
                WindowDuration = TimeSpan.FromMilliseconds(125)
            });
        }

        Assert.Contains(
            activities,
            activity => string.Equals(activity.Function, nameof(AudioProcessor.AnalyzeLoudness), StringComparison.Ordinal));
    }

    [Fact]
    public void AnalyzeLoudness_ConstantSignal_ComputesExpectedDbValues()
    {
        var buffer = CreateConstantBuffer(amplitude: 0.5f, sampleRate: 16_000, durationSec: 1.0);

        var metrics = AudioProcessor.AnalyzeLoudness(buffer, new AudioLoudnessAnalysisOptions
        {
            ComputeIntegratedLufs = false,
            WindowDuration = TimeSpan.FromMilliseconds(250)
        });

        const double expectedDb = -6.0206;

        Assert.Equal(1.0, metrics.DurationSec, 3);
        Assert.Equal(0.25, metrics.WindowDurationSec, 3);
        Assert.Equal(0.5, metrics.SamplePeakLinear, 6);
        Assert.Equal(0.5, metrics.TruePeakLinear, 6);
        Assert.Equal(0.5, metrics.OverallRmsLinear, 6);
        Assert.Equal(0.5, metrics.MinWindowRmsLinear, 6);
        Assert.Equal(0.5, metrics.MaxWindowRmsLinear, 6);

        Assert.NotNull(metrics.SamplePeakDbFs);
        Assert.NotNull(metrics.TruePeakDbFs);
        Assert.NotNull(metrics.OverallRmsDbFs);
        Assert.NotNull(metrics.MinWindowRmsDbFs);
        Assert.NotNull(metrics.MaxWindowRmsDbFs);

        Assert.Equal(expectedDb, metrics.SamplePeakDbFs!.Value, 3);
        Assert.Equal(expectedDb, metrics.TruePeakDbFs!.Value, 3);
        Assert.Equal(expectedDb, metrics.OverallRmsDbFs!.Value, 3);
        Assert.Equal(expectedDb, metrics.MinWindowRmsDbFs!.Value, 3);
        Assert.Equal(expectedDb, metrics.MaxWindowRmsDbFs!.Value, 3);
        Assert.Null(metrics.IntegratedLufs);
    }

    [Fact]
    public void AnalyzeLoudness_EmptyBuffer_ReturnsNullDbMetrics()
    {
        var buffer = new AudioBuffer(channels: 1, sampleRate: 16_000, length: 0);

        var metrics = AudioProcessor.AnalyzeLoudness(buffer, new AudioLoudnessAnalysisOptions
        {
            ComputeIntegratedLufs = false
        });

        Assert.Equal(0, metrics.DurationSec);
        Assert.Equal(0, metrics.SamplePeakLinear);
        Assert.Equal(0, metrics.TruePeakLinear);
        Assert.Equal(0, metrics.OverallRmsLinear);
        Assert.Equal(0, metrics.MinWindowRmsLinear);
        Assert.Equal(0, metrics.MaxWindowRmsLinear);
        Assert.Null(metrics.SamplePeakDbFs);
        Assert.Null(metrics.TruePeakDbFs);
        Assert.Null(metrics.OverallRmsDbFs);
        Assert.Null(metrics.MinWindowRmsDbFs);
        Assert.Null(metrics.MaxWindowRmsDbFs);
        Assert.Null(metrics.IntegratedLufs);
    }

    [Fact]
    public void TryParseIntegratedLufs_UsesMostRecentIntegratedReading()
    {
        var logs = new[]
        {
            "[Parsed_ebur128_0 @ 0x123] t: 0.40 TARGET:-23 LUFS M:-20.2 S:-20.1 I: -21.6 LUFS LRA: 1.1 LU",
            "[Parsed_ebur128_0 @ 0x123] Integrated loudness:",
            "[Parsed_ebur128_0 @ 0x123] I:         -18.4 LUFS"
        };

        var parsed = AudioProcessor.TryParseIntegratedLufs(logs, out var integratedLufs);

        Assert.True(parsed);
        Assert.Equal(-18.4, integratedLufs, 3);
    }

    [Fact]
    public void TryParseIntegratedLufs_NoIntegratedReading_ReturnsFalse()
    {
        var logs = new[]
        {
            "[Parsed_ebur128_0 @ 0x123] Summary:",
            "[Parsed_ebur128_0 @ 0x123] Loudness range:",
            "[Parsed_ebur128_0 @ 0x123] LRA: 4.2 LU"
        };

        var parsed = AudioProcessor.TryParseIntegratedLufs(logs, out var integratedLufs);

        Assert.False(parsed);
        Assert.Equal(0, integratedLufs);
    }

    private static AudioBuffer CreateConstantBuffer(float amplitude, int sampleRate, double durationSec, int channels = 1)
    {
        var totalSamples = (int)Math.Round(durationSec * sampleRate);
        var buffer = new AudioBuffer(channels, sampleRate, totalSamples);

        for (int channel = 0; channel < channels; channel++)
        {
            var span = buffer.GetChannelSpan(channel);
            span.Fill(amplitude);
        }

        return buffer;
    }
}
