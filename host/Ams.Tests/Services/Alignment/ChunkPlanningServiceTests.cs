using Ams.Core.Artifacts;
using Ams.Core.Artifacts.Alignment;
using Ams.Core.Audio;
using Ams.Core.Services.Alignment;

namespace Ams.Tests.Services.Alignment;

public class ChunkPlanningServiceTests
{
    private const int SampleRate = 16000;
    private const float SilenceAmplitude = 0.0005f;
    private const float AudioAmplitude = 0.1f;
    private readonly ChunkPlanningService _service = new();

    [Fact]
    public void GeneratePlan_Deterministic_IdenticalInputYieldsIdenticalOutput()
    {
        var buffer = CreateBufferWithSilence(SampleRate * 120, SampleRate * 60, (int)(0.5 * SampleRate));
        var policy = ChunkPlanningPolicy.Default;

        var plan1 = _service.GeneratePlan(buffer, "/audio/chapter01.wav", policy);
        var plan2 = _service.GeneratePlan(buffer, "/audio/chapter01.wav", policy);

        Assert.Equal(plan1.Chunks.Count, plan2.Chunks.Count);
        for (int i = 0; i < plan1.Chunks.Count; i++)
        {
            Assert.Equal(plan1.Chunks[i].ChunkId, plan2.Chunks[i].ChunkId);
            Assert.Equal(plan1.Chunks[i].StartSample, plan2.Chunks[i].StartSample);
            Assert.Equal(plan1.Chunks[i].LengthSamples, plan2.Chunks[i].LengthSamples);
            Assert.Equal(plan1.Chunks[i].StartSec, plan2.Chunks[i].StartSec);
            Assert.Equal(plan1.Chunks[i].EndSec, plan2.Chunks[i].EndSec);
        }

        Assert.Equal(plan1.SourceAudioFingerprint, plan2.SourceAudioFingerprint);
    }

    [Fact]
    public void GeneratePlan_ChunkIdsAreSequentialFromZero()
    {
        var buffer = CreateBufferWithMultipleSilences(SampleRate * 180, new[]
        {
            (SampleRate * 60, (int)(0.5 * SampleRate)),
            (SampleRate * 120, (int)(0.5 * SampleRate))
        });

        var plan = _service.GeneratePlan(buffer, "/audio/test.wav");

        for (int i = 0; i < plan.Chunks.Count; i++)
        {
            Assert.Equal(i, plan.Chunks[i].ChunkId);
        }
    }

    [Fact]
    public void GeneratePlan_ChunksAreSortedByStartSample()
    {
        var buffer = CreateBufferWithMultipleSilences(SampleRate * 180, new[]
        {
            (SampleRate * 60, (int)(0.5 * SampleRate)),
            (SampleRate * 120, (int)(0.5 * SampleRate))
        });

        var plan = _service.GeneratePlan(buffer, "/audio/test.wav");

        for (int i = 1; i < plan.Chunks.Count; i++)
        {
            Assert.True(plan.Chunks[i].StartSample > plan.Chunks[i - 1].StartSample,
                $"Chunk {i} StartSample ({plan.Chunks[i].StartSample}) should be > chunk {i - 1} StartSample ({plan.Chunks[i - 1].StartSample})");
        }
    }

    [Fact]
    public void GeneratePlan_ChunksCoverEntireBuffer()
    {
        var totalLength = SampleRate * 120;
        var buffer = CreateBufferWithSilence(totalLength, SampleRate * 60, (int)(0.5 * SampleRate));

        var plan = _service.GeneratePlan(buffer, "/audio/test.wav");

        // First chunk starts at 0
        Assert.Equal(0, plan.Chunks[0].StartSample);

        // Contiguous coverage
        for (int i = 1; i < plan.Chunks.Count; i++)
        {
            var prevEnd = plan.Chunks[i - 1].StartSample + plan.Chunks[i - 1].LengthSamples;
            Assert.Equal(prevEnd, plan.Chunks[i].StartSample);
        }

        // Total length matches buffer
        var totalCoverage = plan.Chunks.Sum(c => c.LengthSamples);
        Assert.Equal(totalLength, totalCoverage);
    }

    [Fact]
    public void GeneratePlan_TimeConversionsAreConsistent()
    {
        var totalLength = SampleRate * 120;
        var buffer = CreateBufferWithSilence(totalLength, SampleRate * 60, (int)(0.5 * SampleRate));

        var plan = _service.GeneratePlan(buffer, "/audio/test.wav");

        foreach (var chunk in plan.Chunks)
        {
            var expectedStartSec = (double)chunk.StartSample / SampleRate;
            var expectedEndSec = (double)(chunk.StartSample + chunk.LengthSamples) / SampleRate;

            Assert.Equal(expectedStartSec, chunk.StartSec);
            Assert.Equal(expectedEndSec, chunk.EndSec);
        }
    }

    [Fact]
    public void GeneratePlan_StoresPolicyInDocument()
    {
        var buffer = CreateAudioBuffer(SampleRate * 60);
        var policy = new ChunkPlanningPolicy
        {
            SilenceThresholdDb = -50.0,
            MinSilenceDuration = TimeSpan.FromMilliseconds(300),
            MinChunkDuration = TimeSpan.FromSeconds(20),
            MaxChunkDuration = TimeSpan.FromSeconds(27)
        };

        var plan = _service.GeneratePlan(buffer, "/audio/test.wav", policy);

        Assert.Equal(-50.0, plan.Policy.SilenceThresholdDb);
        Assert.Equal(300.0, plan.Policy.MinSilenceDurationMs);
        Assert.Equal(20.0, plan.Policy.MinChunkDurationSec);
        Assert.Equal(27.0, plan.Policy.MaxChunkDurationSec);
        Assert.Equal(SampleRate, plan.Policy.SampleRate);
    }

    [Fact]
    public void GeneratePlan_StoresVersionAndMetadata()
    {
        var buffer = CreateAudioBuffer(SampleRate * 60);

        var plan = _service.GeneratePlan(buffer, "/audio/chapter01.wav");

        Assert.Equal(ChunkPlanDocument.CurrentVersion, plan.Version);
        Assert.Equal("/audio/chapter01.wav", plan.SourceAudioPath);
        Assert.True(plan.CreatedAtUtc <= DateTime.UtcNow);
        Assert.True(plan.CreatedAtUtc > DateTime.UtcNow.AddMinutes(-1));
    }

    [Fact]
    public void GeneratePlan_FingerprintNormalizesPathSeparators()
    {
        var buffer = CreateAudioBuffer(SampleRate * 60);

        var plan1 = _service.GeneratePlan(buffer, @"C:\Audio\chapter01.wav");
        var plan2 = _service.GeneratePlan(buffer, "C:/Audio/chapter01.wav");

        Assert.Equal(plan1.SourceAudioFingerprint, plan2.SourceAudioFingerprint);
    }

    [Fact]
    public void GeneratePlan_DefaultPolicyMatchesAudioDefaults()
    {
        var buffer = CreateAudioBuffer(SampleRate * 60);

        var plan = _service.GeneratePlan(buffer, "/audio/test.wav");

        Assert.Equal(AudioDefaults.SilenceThresholdDb, plan.Policy.SilenceThresholdDb);
        Assert.Equal(AudioDefaults.MinimumSilenceDuration.TotalMilliseconds, plan.Policy.MinSilenceDurationMs);
        Assert.Equal(15.0, plan.Policy.MinChunkDurationSec);
        Assert.Equal(29.5, plan.Policy.MaxChunkDurationSec);
    }

    [Fact]
    public void IsValid_ReturnsTrueForMatchingInputs()
    {
        var buffer = CreateAudioBuffer(SampleRate * 60);
        var policy = ChunkPlanningPolicy.Default;
        var plan = _service.GeneratePlan(buffer, "/audio/test.wav", policy);

        Assert.True(_service.IsValid(plan, buffer, "/audio/test.wav", policy));
    }

    [Fact]
    public void IsValid_ReturnsFalseForDifferentAudioLength()
    {
        var buffer1 = CreateAudioBuffer(SampleRate * 60);
        var buffer2 = CreateAudioBuffer(SampleRate * 90);
        var plan = _service.GeneratePlan(buffer1, "/audio/test.wav");

        Assert.False(_service.IsValid(plan, buffer2, "/audio/test.wav"));
    }

    [Fact]
    public void IsValid_ReturnsFalseForDifferentPath()
    {
        var buffer = CreateAudioBuffer(SampleRate * 60);
        var plan = _service.GeneratePlan(buffer, "/audio/chapter01.wav");

        Assert.False(_service.IsValid(plan, buffer, "/audio/chapter02.wav"));
    }

    [Fact]
    public void IsValid_ReturnsFalseForDifferentPolicy()
    {
        var buffer = CreateAudioBuffer(SampleRate * 60);
        var plan = _service.GeneratePlan(buffer, "/audio/test.wav");

        var differentPolicy = new ChunkPlanningPolicy
        {
            SilenceThresholdDb = -45.0
        };

        Assert.False(_service.IsValid(plan, buffer, "/audio/test.wav", differentPolicy));
    }

    [Fact]
    public void IsValid_ReturnsFalseForOlderPlanVersion()
    {
        var buffer = CreateAudioBuffer(SampleRate * 60);
        var plan = _service.GeneratePlan(buffer, "/audio/test.wav") with { Version = 1 };

        Assert.False(_service.IsValid(plan, buffer, "/audio/test.wav"));
    }

    [Fact]
    public void GeneratePlan_EnforcesDefaultMaxChunkDuration()
    {
        var totalLength = (int)(45.121375 * SampleRate);
        var buffer = CreateAudioBuffer(totalLength);
        FillWithAmplitude(buffer, SampleRate * 15, (int)(0.3 * SampleRate), SilenceAmplitude);
        FillWithAmplitude(buffer, (int)(40.044125 * SampleRate),
            totalLength - (int)(40.044125 * SampleRate), SilenceAmplitude);

        var plan = _service.GeneratePlan(buffer, "/audio/test.wav");
        var maxChunkSamples = (int)(29.5 * SampleRate);

        Assert.All(plan.Chunks, chunk => Assert.InRange(chunk.LengthSamples, 1, maxChunkSamples));
    }

    [Fact]
    public void GeneratePlan_NullBuffer_ThrowsArgumentNullException()
    {
        Assert.Throws<ArgumentNullException>(() =>
            _service.GeneratePlan((AudioBuffer)null!, "/audio/test.wav"));
    }

    [Fact]
    public void GeneratePlan_EmptyPath_ThrowsArgumentException()
    {
        var buffer = CreateAudioBuffer(SampleRate * 60);

        Assert.Throws<ArgumentException>(() =>
            _service.GeneratePlan(buffer, ""));
    }

    // --- Helper methods ---

    private static AudioBuffer CreateAudioBuffer(int length)
    {
        var buffer = new AudioBuffer(1, SampleRate, length);
        FillWithAmplitude(buffer, 0, length, AudioAmplitude);
        return buffer;
    }

    private static AudioBuffer CreateBufferWithSilence(int totalLength, int silenceStart, int silenceDuration)
    {
        var buffer = new AudioBuffer(1, SampleRate, totalLength);
        FillWithAmplitude(buffer, 0, totalLength, AudioAmplitude);
        FillWithAmplitude(buffer, silenceStart, silenceDuration, SilenceAmplitude);
        return buffer;
    }

    private static AudioBuffer CreateBufferWithMultipleSilences(int totalLength, (int start, int duration)[] silences)
    {
        var buffer = new AudioBuffer(1, SampleRate, totalLength);
        FillWithAmplitude(buffer, 0, totalLength, AudioAmplitude);
        foreach (var (start, duration) in silences)
        {
            FillWithAmplitude(buffer, start, duration, SilenceAmplitude);
        }
        return buffer;
    }

    private static void FillWithAmplitude(AudioBuffer buffer, int offset, int length, float amplitude)
    {
        var span = buffer.GetChannelSpan(0);
        for (int i = offset; i < offset + length && i < buffer.Length; i++)
        {
            span[i] = amplitude * MathF.Sin(2 * MathF.PI * 440 * i / buffer.SampleRate);
        }
    }
}
