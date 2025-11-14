using Ams.Core.Artifacts;
using Ams.Core.Processors;

namespace Ams.Tests;

public class AudioProcessorFilterTests
{
    private static bool FiltersUnavailable()
    {
        try
        {
            if (!Ams.Core.Services.Integrations.FFmpeg.FfSession.FiltersAvailable)
            {
                Console.WriteLine("Skipping FFmpeg filter tests because avfilter is not available in this environment.");
                return true;
            }

            return false;
        }
        catch (InvalidOperationException ex)
        {
            Console.WriteLine($"Skipping FFmpeg filter tests: {ex.Message}");
            return true;
        }
        catch (NotSupportedException ex)
        {
            Console.WriteLine($"Skipping FFmpeg filter tests: {ex.Message}");
            return true;
        }
    }

    private static AudioBuffer CreateBuffer(params (double frequency, double seconds)[] segments)
    {
        const int sampleRate = 16000;
        int channelCount = 1;
        int totalSamples = (int)Math.Round(segments.Sum(seg => seg.seconds) * sampleRate);
        var buffer = new AudioBuffer(channelCount, sampleRate, totalSamples);
        int cursor = 0;
        foreach (var (frequency, seconds) in segments)
        {
            int segSamples = (int)Math.Round(seconds * sampleRate);
            double scale = frequency == 0 ? 0 : 0.5;
            for (int i = 0; i < segSamples; i++)
            {
                double value = frequency == 0
                    ? 0.0
                    : scale * Math.Sin(2 * Math.PI * frequency * i / sampleRate);
                buffer.Planar[0][cursor + i] = (float)value;
            }

            cursor += segSamples;
        }

        return buffer;
    }

    [Fact]
    public void Trim_ReturnsExpectedSegment()
    {
        if (FiltersUnavailable()) return;
        var buffer = CreateBuffer((0, 0.5), (440, 0.5));
        var trimmed = AudioProcessor.Trim(buffer, TimeSpan.FromMilliseconds(400), TimeSpan.FromMilliseconds(700));
        Assert.Equal(buffer.SampleRate, trimmed.SampleRate);
        Assert.Equal(buffer.Channels, trimmed.Channels);
        Assert.InRange(trimmed.Length, 0.28 * trimmed.SampleRate, 0.32 * trimmed.SampleRate);
    }

    [Fact]
    public void FadeIn_SetsLeadingSamplesToZero()
    {
        if (FiltersUnavailable()) return;
        var buffer = CreateBuffer((440, 1.0));
        var faded = AudioProcessor.FadeIn(buffer, TimeSpan.FromMilliseconds(100));
        int fadeSamples = (int)(0.1 * buffer.SampleRate);
        Assert.True(faded.Planar[0].Take(fadeSamples).All(sample => Math.Abs(sample) < 1e-3f));
    }

    [Fact]
    public void DetectSilence_FindsInitialGap()
    {
        if (FiltersUnavailable()) return;
        var buffer = CreateBuffer((0, 0.4), (440, 0.6));
        var intervals = AudioProcessor.DetectSilence(buffer, new SilenceDetectOptions
        {
            NoiseDb = -40,
            MinimumDuration = TimeSpan.FromMilliseconds(200)
        });

        Assert.NotEmpty(intervals);
        var first = intervals[0];
        Assert.True(first.Start < TimeSpan.FromMilliseconds(50));
        Assert.True(first.End > TimeSpan.FromMilliseconds(350));
    }
}
