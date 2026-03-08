using Ams.Core.Artifacts;
using Ams.Core.Audio;

namespace Ams.Tests.Audio;

public class SilenceChunkerTests
{
    private const int SampleRate = 16000;

    /// <summary>
    /// Threshold amplitude for -55dB: 10^(-55/20) ~= 0.001778
    /// We use values well below and above this for clarity.
    /// </summary>
    private const float SilenceAmplitude = 0.0005f;  // well below -55dB threshold
    private const float AudioAmplitude = 0.1f;       // well above -55dB threshold

    [Fact]
    public void AllSilence_ReturnsSingleChunk()
    {
        var buffer = CreateBuffer(SampleRate * 60); // 60 seconds of silence
        FillSilence(buffer, 0, buffer.Length);

        var chunks = SilenceChunker.FindChunkBoundaries(buffer);

        Assert.Single(chunks);
        Assert.Equal(0, chunks[0].StartSample);
        Assert.Equal(buffer.Length, chunks[0].Length);
    }

    [Fact]
    public void NoSilence_ReturnsSingleChunk()
    {
        var buffer = CreateBuffer(SampleRate * 60); // 60 seconds of audio
        FillAudio(buffer, 0, buffer.Length);

        var chunks = SilenceChunker.FindChunkBoundaries(buffer);

        Assert.Single(chunks);
        Assert.Equal(0, chunks[0].StartSample);
        Assert.Equal(buffer.Length, chunks[0].Length);
    }

    [Fact]
    public void SingleSilenceRegion_SplitsAtMidpoint()
    {
        // 90s audio, 300ms silence at 45s mark, then more audio
        // Both resulting chunks (~45s each) are well above the 15s minChunkDuration
        var silenceDurationSamples = (int)(0.3 * SampleRate); // 300ms = 4800 samples
        var silenceStart = SampleRate * 45;
        var totalLength = SampleRate * 90;

        var buffer = CreateBuffer(totalLength);
        FillAudio(buffer, 0, silenceStart);
        FillSilence(buffer, silenceStart, silenceDurationSamples);
        FillAudio(buffer, silenceStart + silenceDurationSamples, totalLength - silenceStart - silenceDurationSamples);

        var chunks = SilenceChunker.FindChunkBoundaries(buffer);

        Assert.Equal(2, chunks.Count);
        // Split point should be near the midpoint of the silence region
        var expectedMidpoint = silenceStart + silenceDurationSamples / 2;
        Assert.Equal(0, chunks[0].StartSample);
        // Allow some tolerance due to RMS window hop size
        Assert.InRange(chunks[0].Length, expectedMidpoint - 1024, expectedMidpoint + 1024);
        Assert.Equal(chunks[0].Length, chunks[1].StartSample); // no gap
        Assert.Equal(totalLength - chunks[0].Length, chunks[1].Length);
    }

    [Fact]
    public void MultipleSilenceRegions_ReturnsNPlusOneChunks()
    {
        // 3 minutes audio with silence at 1m and 2m marks
        var totalLength = SampleRate * 180; // 3 minutes
        var silenceDuration = (int)(0.5 * SampleRate); // 500ms silence regions
        var silence1Start = SampleRate * 60;
        var silence2Start = SampleRate * 120;

        var buffer = CreateBuffer(totalLength);
        FillAudio(buffer, 0, totalLength);
        FillSilence(buffer, silence1Start, silenceDuration);
        FillSilence(buffer, silence2Start, silenceDuration);

        var chunks = SilenceChunker.FindChunkBoundaries(buffer);

        Assert.Equal(3, chunks.Count);
    }

    [Fact]
    public void ShortBuffer_ReturnsSingleChunkRegardlessOfSilence()
    {
        // Buffer shorter than minChunkDuration (default 15s)
        var totalLength = SampleRate * 10; // 10 seconds
        var buffer = CreateBuffer(totalLength);
        FillAudio(buffer, 0, totalLength / 2);
        FillSilence(buffer, totalLength / 2, (int)(0.5 * SampleRate)); // 500ms silence
        FillAudio(buffer, totalLength / 2 + (int)(0.5 * SampleRate),
            totalLength - totalLength / 2 - (int)(0.5 * SampleRate));

        var chunks = SilenceChunker.FindChunkBoundaries(buffer);

        Assert.Single(chunks);
        Assert.Equal(0, chunks[0].StartSample);
        Assert.Equal(totalLength, chunks[0].Length);
    }

    [Fact]
    public void SubThresholdSilence_IsIgnored()
    {
        // Silence shorter than MinimumSilenceDuration (200ms) should be ignored
        var shortSilenceDuration = (int)(0.1 * SampleRate); // 100ms - below 200ms threshold
        var totalLength = SampleRate * 60;
        var silenceStart = SampleRate * 30;

        var buffer = CreateBuffer(totalLength);
        FillAudio(buffer, 0, silenceStart);
        FillSilence(buffer, silenceStart, shortSilenceDuration);
        FillAudio(buffer, silenceStart + shortSilenceDuration, totalLength - silenceStart - shortSilenceDuration);

        var chunks = SilenceChunker.FindChunkBoundaries(buffer);

        Assert.Single(chunks);
    }

    [Fact]
    public void ChunkBoundaries_CoverEntireBuffer()
    {
        // Verify no gaps or overlaps
        var totalLength = SampleRate * 180; // 3 minutes
        var silenceDuration = (int)(0.5 * SampleRate);

        var buffer = CreateBuffer(totalLength);
        FillAudio(buffer, 0, totalLength);
        FillSilence(buffer, SampleRate * 45, silenceDuration);
        FillSilence(buffer, SampleRate * 90, silenceDuration);
        FillSilence(buffer, SampleRate * 135, silenceDuration);

        var chunks = SilenceChunker.FindChunkBoundaries(buffer);

        // No gaps
        for (int i = 1; i < chunks.Count; i++)
        {
            Assert.Equal(chunks[i - 1].StartSample + chunks[i - 1].Length, chunks[i].StartSample);
        }

        // Total coverage
        var totalCoverage = chunks.Sum(c => c.Length);
        Assert.Equal(totalLength, totalCoverage);

        // Starts at 0
        Assert.Equal(0, chunks[0].StartSample);
    }

    [Fact]
    public void RmsThreshold_MatchesDecibels()
    {
        // -55dB threshold means RMS ~= 10^(-55/20) ~= 0.001778
        // Samples just above threshold should NOT be treated as silence
        var amplitude = 0.003f; // above -55dB threshold
        var totalLength = SampleRate * 60;

        var buffer = CreateBuffer(totalLength);
        FillWithAmplitude(buffer, 0, totalLength, amplitude);

        var chunks = SilenceChunker.FindChunkBoundaries(buffer);

        // No silence detected, single chunk
        Assert.Single(chunks);
    }

    [Fact]
    public void MinChunkDuration_PreventsExcessiveFragmentation()
    {
        // Many silence regions close together should not create tiny chunks
        var totalLength = SampleRate * 120; // 2 minutes
        var buffer = CreateBuffer(totalLength);
        FillAudio(buffer, 0, totalLength);

        // Place silence every 5 seconds (well under default 15s minChunkDuration)
        for (int sec = 5; sec < 120; sec += 5)
        {
            var silStart = sec * SampleRate;
            var silLen = (int)(0.3 * SampleRate);
            if (silStart + silLen < totalLength)
                FillSilence(buffer, silStart, silLen);
        }

        var chunks = SilenceChunker.FindChunkBoundaries(buffer);

        // Should have fewer chunks than silence regions due to minChunkDuration filtering
        // With 120s and 15s min, we still expect far fewer chunks than silence regions.
        Assert.InRange(chunks.Count, 4, 9);
    }

    [Fact]
    public void CustomMinChunkDuration_IsRespected()
    {
        var totalLength = SampleRate * 60;
        var buffer = CreateBuffer(totalLength);
        FillAudio(buffer, 0, totalLength);
        // Silence at 10s, 20s, 30s, 40s, 50s
        for (int sec = 10; sec <= 50; sec += 10)
        {
            FillSilence(buffer, sec * SampleRate, (int)(0.3 * SampleRate));
        }

        // With 5s minChunkDuration, should split at more points
        var chunks5s = SilenceChunker.FindChunkBoundaries(buffer,
            minChunkDuration: TimeSpan.FromSeconds(5));

        // With 25s minChunkDuration, should split at fewer points
        var chunks25s = SilenceChunker.FindChunkBoundaries(buffer,
            minChunkDuration: TimeSpan.FromSeconds(25));

        Assert.True(chunks5s.Count > chunks25s.Count,
            $"5s min should produce more chunks ({chunks5s.Count}) than 25s min ({chunks25s.Count})");
    }

    // --- Helper methods ---

    private static AudioBuffer CreateBuffer(int length)
    {
        return new AudioBuffer(1, SampleRate, length);
    }

    private static void FillSilence(AudioBuffer buffer, int offset, int length)
    {
        FillWithAmplitude(buffer, offset, length, SilenceAmplitude);
    }

    private static void FillAudio(AudioBuffer buffer, int offset, int length)
    {
        FillWithAmplitude(buffer, offset, length, AudioAmplitude);
    }

    private static void FillWithAmplitude(AudioBuffer buffer, int offset, int length, float amplitude)
    {
        var span = buffer.GetChannelSpan(0);
        for (int i = offset; i < offset + length && i < buffer.Length; i++)
        {
            // Use a simple sine wave to provide realistic signal
            span[i] = amplitude * MathF.Sin(2 * MathF.PI * 440 * i / buffer.SampleRate);
        }
    }
}
