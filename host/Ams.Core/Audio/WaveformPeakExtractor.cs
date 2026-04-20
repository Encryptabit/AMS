using Ams.Core.Artifacts;

namespace Ams.Core.Audio;

public static class WaveformPeakExtractor
{
    public static WaveformPeaks ComputeMonoMinMaxEnvelope(AudioBuffer buffer, int bucketCount)
    {
        ArgumentNullException.ThrowIfNull(buffer);

        if (bucketCount <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(bucketCount), "Bucket count must be greater than zero.");
        }

        var durationSeconds = buffer.SampleRate > 0
            ? buffer.Length / (double)buffer.SampleRate
            : 0d;

        var data = new float[bucketCount * 2];
        if (buffer.Length == 0)
        {
            return new WaveformPeaks
            {
                BucketCount = bucketCount,
                DurationSeconds = durationSeconds,
                Data = data
            };
        }

        if (buffer.Channels == 1)
        {
            ComputeSingleChannelEnvelope(buffer.GetChannel(0).Span, data, bucketCount);
        }
        else
        {
            ComputeMultiChannelEnvelope(buffer, data, bucketCount);
        }

        return new WaveformPeaks
        {
            BucketCount = bucketCount,
            DurationSeconds = durationSeconds,
            Data = data
        };
    }

    private static void ComputeSingleChannelEnvelope(ReadOnlySpan<float> samples, Span<float> data, int bucketCount)
    {
        var sampleCount = samples.Length;
        for (var bucket = 0; bucket < bucketCount; bucket++)
        {
            var start = (int)((long)bucket * sampleCount / bucketCount);
            var end = (int)((long)(bucket + 1) * sampleCount / bucketCount);
            if (end <= start)
            {
                end = Math.Min(sampleCount, start + 1);
            }

            var max = 0f;
            var min = 0f;

            for (var i = start; i < end; i++)
            {
                var sample = samples[i];
                if (sample > max)
                {
                    max = sample;
                }
                if (sample < min)
                {
                    min = sample;
                }
            }

            data[bucket * 2] = max;
            data[bucket * 2 + 1] = min;
        }
    }

    private static void ComputeMultiChannelEnvelope(AudioBuffer buffer, Span<float> data, int bucketCount)
    {
        var sampleCount = buffer.Length;
        for (var bucket = 0; bucket < bucketCount; bucket++)
        {
            var start = (int)((long)bucket * sampleCount / bucketCount);
            var end = (int)((long)(bucket + 1) * sampleCount / bucketCount);
            if (end <= start)
            {
                end = Math.Min(sampleCount, start + 1);
            }

            var max = 0f;
            var min = 0f;

            for (var sampleIndex = start; sampleIndex < end; sampleIndex++)
            {
                for (var channel = 0; channel < buffer.Channels; channel++)
                {
                    var sample = buffer[channel, sampleIndex];
                    if (sample > max)
                    {
                        max = sample;
                    }

                    if (sample < min)
                    {
                        min = sample;
                    }
                }
            }

            data[bucket * 2] = max;
            data[bucket * 2 + 1] = min;
        }
    }
}
