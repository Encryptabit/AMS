using Ams.Core.Processors;

namespace Ams.Core.Artifacts;

public sealed class AudioBuffer
{
    private readonly float[] _backing;
    private readonly int _strideLength; // backing length per channel (may differ from Length for slices)
    private readonly int _offset;       // sample offset into backing per-channel stride

    public int Channels { get; }
    public int SampleRate { get; }
    public int Length { get; }
    public AudioBufferMetadata Metadata { get; private set; }

    /// <summary>
    /// Creates a new AudioBuffer with a freshly allocated contiguous backing store.
    /// </summary>
    public AudioBuffer(int channels, int sampleRate, int length, AudioBufferMetadata? metadata = null)
    {
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(channels);
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(sampleRate);
        ArgumentOutOfRangeException.ThrowIfNegative(length);

        Channels = channels;
        SampleRate = sampleRate;
        Length = length;
        _strideLength = length;
        _offset = 0;
        _backing = length > 0 ? new float[channels * length] : Array.Empty<float>();
        Metadata = metadata ?? AudioBufferMetadata.CreateDefault(sampleRate, channels);
    }

    /// <summary>
    /// Internal slice constructor: shares parent backing array with offset/length views.
    /// </summary>
    internal AudioBuffer(float[] backing, int channels, int sampleRate,
        int strideLength, int offset, int length, AudioBufferMetadata? metadata)
    {
        _backing = backing;
        Channels = channels;
        SampleRate = sampleRate;
        _strideLength = strideLength;
        _offset = offset;
        Length = length;
        Metadata = metadata ?? AudioBufferMetadata.CreateDefault(sampleRate, channels);
    }

    /// <summary>
    /// Returns a read-only memory view of the specified channel's samples.
    /// </summary>
    public ReadOnlyMemory<float> GetChannel(int channel)
        => new(_backing, channel * _strideLength + _offset, Length);

    /// <summary>
    /// Returns a writable span for producers to populate the buffer.
    /// Internal to prevent external mutation of buffer internals.
    /// </summary>
    internal Span<float> GetChannelSpan(int channel)
        => _backing.AsSpan(channel * _strideLength + _offset, Length);

    /// <summary>
    /// Sample indexer for convenient per-sample access.
    /// </summary>
    public float this[int channel, int sample]
    {
        get => _backing[channel * _strideLength + _offset + sample];
        internal set => _backing[channel * _strideLength + _offset + sample] = value;
    }

    /// <summary>
    /// Returns the underlying backing array for pinning in unmanaged interop (e.g., FFmpeg).
    /// SAFETY: Callers must ensure the backing array is not moved by GC while pinned.
    /// </summary>
    internal float[] GetBackingArray() => _backing;

    /// <summary>
    /// Returns the byte offset for the specified channel within the backing array.
    /// Used with GetBackingArray() for computing per-channel pointers during pinning.
    /// </summary>
    internal int GetChannelOffset(int channel) => channel * _strideLength + _offset;

    /// <summary>
    /// Returns a zero-copy slice of this buffer sharing the same backing store.
    /// </summary>
    public AudioBuffer Slice(int startSample, int length)
    {
        ArgumentOutOfRangeException.ThrowIfNegative(startSample);
        ArgumentOutOfRangeException.ThrowIfNegative(length);
        if (startSample + length > Length)
            throw new ArgumentOutOfRangeException(nameof(length),
                $"Slice [{startSample}..{startSample + length}) exceeds buffer length {Length}");

        return new AudioBuffer(_backing, Channels, SampleRate,
            _strideLength, _offset + startSample, length, Metadata);
    }

    /// <summary>
    /// Returns a zero-copy slice of this buffer for the given time range.
    /// </summary>
    public AudioBuffer Slice(TimeSpan start, TimeSpan end)
    {
        var startSample = (int)(start.TotalSeconds * SampleRate);
        var endSample = Math.Min((int)(end.TotalSeconds * SampleRate), Length);
        return Slice(startSample, endSample - startSample);
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

        // Validate all buffers have matching format
        var first = bufferList[0];
        int sampleRate = first.SampleRate;
        int channels = first.Channels;
        int totalLength = first.Length;

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
            totalLength += buf.Length;
        }

        // Direct managed memory copy -- all samples are already float[] in managed memory,
        // so FFmpeg filter graph overhead is unnecessary.
        var result = new AudioBuffer(channels, sampleRate, totalLength, first.Metadata);

        for (int ch = 0; ch < channels; ch++)
        {
            var dest = result.GetChannelSpan(ch);
            int offset = 0;
            for (int i = 0; i < bufferList.Count; i++)
            {
                var source = bufferList[i];
                source.GetChannel(ch).Span.CopyTo(dest.Slice(offset, source.Length));
                offset += source.Length;
            }
        }

        return result;
    }
}
