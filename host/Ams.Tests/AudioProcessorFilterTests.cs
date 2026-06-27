using Ams.Core.Artifacts;
using Ams.Core.Processors;
using Ams.Core.Services.Integrations.FFmpeg;

namespace Ams.Tests;

public class AudioProcessorFilterTests
{
    private static bool FiltersUnavailable()
    {
        try
        {
            if (!Ams.Core.Services.Integrations.FFmpeg.FfSession.FiltersAvailable)
            {
                Console.WriteLine(
                    "Skipping FFmpeg filter tests because avfilter is not available in this environment.");
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
                buffer.GetChannelSpan(0)[cursor + i] = (float)value;
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
    public void FadeIn_GraduallyIncreasesAmplitude()
    {
        if (FiltersUnavailable()) return;
        var buffer = CreateBuffer((440, 1.0));
        var faded = AudioProcessor.FadeIn(buffer, TimeSpan.FromMilliseconds(100));
        int fadeSamples = (int)(0.1 * buffer.SampleRate);

        // First 1% of fade duration should be near silence (amplitude ramping from 0)
        int earlySamples = fadeSamples / 100;
        var fadedChannel = faded.GetChannel(0).ToArray();
        var earlyMax = fadedChannel.Take(earlySamples).Max(Math.Abs);
        Assert.True(earlyMax < 0.05f, $"Early samples should be near zero, but max was {earlyMax}");

        // Samples after fade should be at full amplitude (matching original)
        int postFadeStart = fadeSamples + 100;
        var postFadeMax = fadedChannel.Skip(postFadeStart).Take(100).Max(Math.Abs);
        Assert.True(postFadeMax > 0.4f, $"Post-fade samples should be at full amplitude, but max was {postFadeMax}");
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

    [Fact]
    public void ALimiter_DefaultsToNoAutoLevel()
    {
        if (FiltersUnavailable()) return;
        var buffer = CreateBuffer((440, 1.0));
        var samples = buffer.GetChannelSpan(0);
        for (var i = 0; i < samples.Length; i++)
        {
            samples[i] *= 1.8f;
        }

        var limited = FfFilterGraph
            .FromBuffer(buffer)
            .ALimiter(new ALimiterFilterParams(LimitDb: -6, AttackMilliseconds: 2, ReleaseMilliseconds: 80))
            .ToBuffer();

        var peak = limited.GetChannel(0).Span.ToArray().Max(sample => Math.Abs(sample));

        Assert.InRange(peak, 0.45f, 0.55f);
    }

    [Fact]
    public void FftDenoiseParameters_SerializeConfigKnobs()
    {
        var json = System.Text.Json.JsonSerializer.Serialize(
            new FftDenoiseFilterParams(NoiseReductionDb: 6),
            new System.Text.Json.JsonSerializerOptions
            {
                DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull
            });

        Assert.Contains("\"NoiseReductionDb\":6", json);
        Assert.Contains("\"NoiseFloorDb\":-50", json);
        Assert.Contains("\"ResidualFloorDb\":-38", json);
        Assert.Contains("\"TrackNoise\":false", json);
        Assert.Contains("\"BandNoise\":\"\"", json);
        Assert.Contains("\"OutputMode\":\"output\"", json);
        Assert.Contains("\"Adaptivity\":0.5", json);
        Assert.Contains("\"FloorOffset\":1", json);
        Assert.Contains("\"NoiseLink\":\"min\"", json);
        Assert.Contains("\"BandMultiplier\":1.25", json);
        Assert.Contains("\"SampleNoise\":\"none\"", json);
        Assert.Contains("\"GainSmooth\":0", json);
    }

    [Fact]
    public void FftDenoise_BuildsAfftdnFilterWithConfiguredKnobs()
    {
        var buffer = CreateBuffer((440, 1.0));

        var spec = FfFilterGraph
            .FromBuffer(buffer)
            .FftDenoise(new FftDenoiseFilterParams(
                NoiseReductionDb: 18,
                NoiseFloorDb: -55,
                NoiseType: "custom",
                BandNoise: "0 0 0",
                ResidualFloorDb: -42,
                TrackNoise: true,
                TrackResidual: true,
                OutputMode: "noise",
                Adaptivity: 0.25,
                FloorOffset: 0.75,
                NoiseLink: "average",
                BandMultiplier: 2,
                SampleNoise: "start",
                GainSmooth: 12))
            .BuildSpec();

        Assert.Contains(
            "afftdn=nr=18:nf=-55:nt=custom:bn=0 0 0:rf=-42:tn=1:tr=1:om=noise:ad=0.25:fo=0.75:nl=average:bm=2:sn=start:gs=12",
            spec);
    }

    [Fact]
    public void DeClick_BuildsNamedAdeclickFilter()
    {
        var buffer = CreateBuffer((440, 1.0));

        var spec = FfFilterGraph
            .FromBuffer(buffer)
            .DeClick(new DeClickFilterParams(
                Window: 40,
                Overlap: 70,
                AutoRegressionOrder: 4,
                Threshold: 3,
                Burst: 1,
                Method: "save"))
            .BuildSpec();

        Assert.Contains("adeclick=window=40:overlap=70:arorder=4:threshold=3:burst=1:method=save", spec);
    }

    [Fact]
    public void DeClick_RunsWithDefaults()
    {
        if (FiltersUnavailable()) return;
        var buffer = CreateBuffer((440, 1.0));
        buffer.GetChannelSpan(0)[buffer.SampleRate / 2] = 1.0f;

        var declicked = FfFilterGraph
            .FromBuffer(buffer)
            .DeClick()
            .ToBuffer();

        Assert.Equal(buffer.SampleRate, declicked.SampleRate);
        Assert.Equal(buffer.Channels, declicked.Channels);
        Assert.InRange(declicked.Length, buffer.Length - 32, buffer.Length + 32);
    }

    [Fact]
    public void LoudNormMeasurement_BuildsJsonStatsClause()
    {
        var buffer = CreateBuffer((440, 1.0));

        var spec = FfFilterGraph
            .FromBuffer(buffer)
            .LoudNormMeasurement(new LoudNormFilterParams(
                TargetI: -19,
                TargetLra: 7,
                TargetTp: -3,
                DualMono: true,
                TwoPass: true))
            .BuildSpec();

        Assert.Contains("loudnorm=I=-19:LRA=7:TP=-3:dual_mono=1:print_format=json", spec);
    }

    [Fact]
    public void LoudNorm_BuildsSampleRateRestoreClause()
    {
        var buffer = CreateBuffer((440, 1.0));

        var spec = FfFilterGraph
            .FromBuffer(buffer)
            .LoudNorm(new LoudNormFilterParams(TargetI: -19, TargetLra: 7, TargetTp: -3))
            .BuildSpec();

        Assert.Contains("loudnorm=I=-19:LRA=7:TP=-3:dual_mono=0,aresample=16000", spec);
    }

    [Fact]
    public void LoudNorm_AfterExplicitResample_RestoresExplicitSampleRate()
    {
        var buffer = CreateBuffer((440, 1.0));

        var spec = FfFilterGraph
            .FromBuffer(buffer)
            .Resample(new ResampleFilterParams(48000))
            .LoudNorm(new LoudNormFilterParams(TargetI: -19, TargetLra: 7, TargetTp: -3))
            .BuildSpec();

        Assert.Contains("aresample=48000,loudnorm=I=-19:LRA=7:TP=-3:dual_mono=0,aresample=48000", spec);
    }

    [Fact]
    public void LoudNormMeasured_BuildsSecondPassClause()
    {
        var buffer = CreateBuffer((440, 1.0));

        var spec = FfFilterGraph
            .FromBuffer(buffer)
            .LoudNormMeasured(
                new LoudNormFilterParams(TargetI: -19, TargetLra: 7, TargetTp: -3, DualMono: true, TwoPass: true),
                new LoudNormMeasuredStats(
                    MeasuredI: -26.1,
                    MeasuredLra: 3.2,
                    MeasuredTp: -2.4,
                    MeasuredThreshold: -36.8,
                    Offset: 0.5))
            .BuildSpec();

        Assert.Contains(
            "loudnorm=I=-19:LRA=7:TP=-3:measured_I=-26.1:measured_LRA=3.2:measured_TP=-2.4:measured_thresh=-36.8:offset=0.5:linear=1:dual_mono=1,aresample=16000",
            spec);
    }
}
