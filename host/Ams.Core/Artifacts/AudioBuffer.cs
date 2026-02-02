using Ams.Core.Processors;

namespace Ams.Core.Artifacts;

public sealed class AudioBuffer
{
    public int Channels { get; }
    public int SampleRate { get; }
    public int Length { get; }
    public float[][] Planar { get; }
    public AudioBufferMetadata Metadata { get; private set; }

    public AudioBuffer(int channels, int sampleRate, int length, AudioBufferMetadata? metadata = null)
    {
        Channels = channels;
        SampleRate = sampleRate;
        Length = length;
        Metadata = metadata ?? AudioBufferMetadata.CreateDefault(sampleRate, channels);

        Planar = new float[channels][];
        for (var ch = 0; ch < channels; ch++) Planar[ch] = new float[length];
    }

    public MemoryStream ToWavStream(AudioEncodeOptions? options = null)
        => AudioProcessor.EncodeWavToStream(this, options);

    public void UpdateMetadata(AudioBufferMetadata metadata)
    {
        Metadata = metadata;
    }

    /// <summary>
    /// Concatenates multiple AudioBuffer instances into a single new buffer.
    /// All buffers must have matching SampleRate and Channels.
    /// </summary>
    /// <param name="buffers">The buffers to concatenate in order.</param>
    /// <returns>A new AudioBuffer containing all samples sequentially.</returns>
    /// <exception cref="ArgumentException">Thrown if buffers is empty.</exception>
    /// <exception cref="InvalidOperationException">Thrown if buffers have mismatched SampleRate or Channels.</exception>
    public static AudioBuffer Concat(params AudioBuffer[] buffers)
        => Concat((IEnumerable<AudioBuffer>)buffers);

    /// <summary>
    /// Concatenates multiple AudioBuffer instances into a single new buffer.
    /// All buffers must have matching SampleRate and Channels.
    /// </summary>
    /// <param name="buffers">The buffers to concatenate in order.</param>
    /// <returns>A new AudioBuffer containing all samples sequentially.</returns>
    /// <exception cref="ArgumentException">Thrown if buffers is empty.</exception>
    /// <exception cref="InvalidOperationException">Thrown if buffers have mismatched SampleRate or Channels.</exception>
    public static AudioBuffer Concat(IEnumerable<AudioBuffer> buffers)
    {
        ArgumentNullException.ThrowIfNull(buffers);

        var bufferList = buffers as IList<AudioBuffer> ?? buffers.ToList();

        if (bufferList.Count == 0)
        {
            throw new ArgumentException("At least one buffer is required for concatenation.", nameof(buffers));
        }

        // Single buffer: return a clone
        if (bufferList.Count == 1)
        {
            var source = bufferList[0];
            var clone = new AudioBuffer(source.Channels, source.SampleRate, source.Length, source.Metadata);
            for (int ch = 0; ch < source.Channels; ch++)
            {
                Array.Copy(source.Planar[ch], clone.Planar[ch], source.Length);
            }
            return clone;
        }

        // Validate all buffers have matching format
        var first = bufferList[0];
        int sampleRate = first.SampleRate;
        int channels = first.Channels;

        for (int i = 1; i < bufferList.Count; i++)
        {
            var buf = bufferList[i];
            if (buf.SampleRate != sampleRate)
            {
                throw new InvalidOperationException(
                    $"Buffer at index {i} has SampleRate {buf.SampleRate}, expected {sampleRate}. " +
                    "All buffers must have matching SampleRate for concatenation.");
            }
            if (buf.Channels != channels)
            {
                throw new InvalidOperationException(
                    $"Buffer at index {i} has {buf.Channels} channels, expected {channels}. " +
                    "All buffers must have matching Channels for concatenation.");
            }
        }

        // Calculate total length
        long totalLength = 0;
        foreach (var buf in bufferList)
        {
            totalLength += buf.Length;
        }

        if (totalLength > int.MaxValue)
        {
            throw new InvalidOperationException(
                $"Combined buffer length ({totalLength} samples) exceeds maximum ({int.MaxValue}).");
        }

        // Allocate result buffer with metadata from first buffer
        var result = new AudioBuffer(channels, sampleRate, (int)totalLength, first.Metadata);

        // Copy samples from each buffer
        int offset = 0;
        foreach (var buf in bufferList)
        {
            for (int ch = 0; ch < channels; ch++)
            {
                Array.Copy(buf.Planar[ch], 0, result.Planar[ch], offset, buf.Length);
            }
            offset += buf.Length;
        }

        return result;
    }
}