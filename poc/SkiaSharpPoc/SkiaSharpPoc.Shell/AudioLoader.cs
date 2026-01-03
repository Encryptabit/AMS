using System.IO;
using System.IO.MemoryMappedFiles;
using System.Runtime.InteropServices;

namespace SkiaSharpPoc.Shell;

/// <summary>
/// Loads and provides sample data from WAV files using memory-mapping for large files.
/// Supports on-the-fly downsampling for zoomed-out views.
/// </summary>
public sealed class AudioLoader : IDisposable
{
    private readonly MemoryMappedFile _mappedFile;
    private readonly MemoryMappedViewAccessor _accessor;
    private readonly long _dataOffset;
    private readonly int _bytesPerSample;
    private bool _disposed;

    public int SampleRate { get; }
    public int Channels { get; }
    public int BitsPerSample { get; }
    public long TotalSamples { get; }
    public double Duration => TotalSamples / (double)SampleRate;

    public AudioLoader(string filePath)
    {
        if (!File.Exists(filePath))
            throw new FileNotFoundException("Audio file not found.", filePath);

        // Memory-map the file for efficient access to large files
        _mappedFile = MemoryMappedFile.CreateFromFile(filePath, FileMode.Open, null, 0, MemoryMappedFileAccess.Read);
        _accessor = _mappedFile.CreateViewAccessor(0, 0, MemoryMappedFileAccess.Read);

        // Parse WAV header
        var header = ParseWavHeader();
        SampleRate = header.SampleRate;
        Channels = header.Channels;
        BitsPerSample = header.BitsPerSample;
        _dataOffset = header.DataOffset;
        _bytesPerSample = BitsPerSample / 8 * Channels;

        var dataSize = header.DataSize;
        TotalSamples = dataSize / _bytesPerSample;
    }

    private WavHeader ParseWavHeader()
    {
        // Read RIFF header
        var riff = new byte[4];
        _accessor.ReadArray(0, riff, 0, 4);
        if (riff[0] != 'R' || riff[1] != 'I' || riff[2] != 'F' || riff[3] != 'F')
            throw new InvalidDataException("Not a valid WAV file: missing RIFF header.");

        // Skip file size (4 bytes), read WAVE marker
        var wave = new byte[4];
        _accessor.ReadArray(8, wave, 0, 4);
        if (wave[0] != 'W' || wave[1] != 'A' || wave[2] != 'V' || wave[3] != 'E')
            throw new InvalidDataException("Not a valid WAV file: missing WAVE marker.");

        // Find fmt chunk
        long pos = 12;
        int sampleRate = 44100;
        short channels = 1;
        short bitsPerSample = 16;
        long dataOffset = 0;
        long dataSize = 0;

        while (pos < _accessor.Capacity - 8)
        {
            var chunkId = new byte[4];
            _accessor.ReadArray(pos, chunkId, 0, 4);
            var chunkSize = _accessor.ReadInt32(pos + 4);

            var chunkName = System.Text.Encoding.ASCII.GetString(chunkId);

            if (chunkName == "fmt ")
            {
                var audioFormat = _accessor.ReadInt16(pos + 8);
                if (audioFormat != 1 && audioFormat != 3) // PCM or IEEE float
                    throw new NotSupportedException($"Only PCM and IEEE float WAV formats are supported. Found format: {audioFormat}");

                channels = _accessor.ReadInt16(pos + 10);
                sampleRate = _accessor.ReadInt32(pos + 12);
                // Skip byte rate (4) and block align (2)
                bitsPerSample = _accessor.ReadInt16(pos + 22);
            }
            else if (chunkName == "data")
            {
                dataOffset = pos + 8;
                dataSize = chunkSize;
                break;
            }

            pos += 8 + chunkSize;
            // Align to even boundary
            if (chunkSize % 2 != 0)
                pos++;
        }

        if (dataOffset == 0)
            throw new InvalidDataException("No data chunk found in WAV file.");

        return new WavHeader(sampleRate, channels, bitsPerSample, dataOffset, dataSize);
    }

    /// <summary>
    /// Gets downsampled min/max pairs for the specified sample range.
    /// Returns pairs of (min, max) values for each pixel bucket.
    /// </summary>
    public (float[] mins, float[] maxs) GetMinMaxSamples(long startSample, long endSample, int targetPixels)
    {
        startSample = Math.Max(0, startSample);
        endSample = Math.Min(TotalSamples, endSample);

        if (startSample >= endSample || targetPixels <= 0)
            return (Array.Empty<float>(), Array.Empty<float>());

        var sampleCount = endSample - startSample;
        var samplesPerPixel = (double)sampleCount / targetPixels;
        var actualPixels = (int)Math.Min(targetPixels, sampleCount);

        var mins = new float[actualPixels];
        var maxs = new float[actualPixels];

        // Initialize with extreme values
        for (int i = 0; i < actualPixels; i++)
        {
            mins[i] = float.MaxValue;
            maxs[i] = float.MinValue;
        }

        // Buffer for reading samples
        var bufferSamples = 4096;
        var buffer = new byte[bufferSamples * _bytesPerSample];

        long currentSample = startSample;
        while (currentSample < endSample)
        {
            var samplesToRead = (int)Math.Min(bufferSamples, endSample - currentSample);
            var bytesToRead = samplesToRead * _bytesPerSample;

            var offset = _dataOffset + currentSample * _bytesPerSample;
            _accessor.ReadArray(offset, buffer, 0, bytesToRead);

            for (int i = 0; i < samplesToRead; i++)
            {
                var sampleIndex = currentSample + i - startSample;
                var pixelIndex = (int)(sampleIndex / samplesPerPixel);
                if (pixelIndex >= actualPixels)
                    pixelIndex = actualPixels - 1;

                var sampleValue = ReadSample(buffer, i * _bytesPerSample);

                if (sampleValue < mins[pixelIndex])
                    mins[pixelIndex] = sampleValue;
                if (sampleValue > maxs[pixelIndex])
                    maxs[pixelIndex] = sampleValue;
            }

            currentSample += samplesToRead;
        }

        // Replace uninitialized values with 0
        for (int i = 0; i < actualPixels; i++)
        {
            if (mins[i] == float.MaxValue) mins[i] = 0;
            if (maxs[i] == float.MinValue) maxs[i] = 0;
        }

        return (mins, maxs);
    }

    private float ReadSample(byte[] buffer, int offset)
    {
        switch (BitsPerSample)
        {
            case 8:
                // 8-bit is unsigned, center at 128
                return (buffer[offset] - 128) / 128f;

            case 16:
                var sample16 = (short)(buffer[offset] | (buffer[offset + 1] << 8));
                return sample16 / 32768f;

            case 24:
                var sample24 = buffer[offset] | (buffer[offset + 1] << 8) | (buffer[offset + 2] << 16);
                // Sign extend from 24-bit
                if ((sample24 & 0x800000) != 0)
                    sample24 |= unchecked((int)0xFF000000);
                return sample24 / 8388608f;

            case 32:
                // Could be float or int32
                var sample32 = BitConverter.ToSingle(buffer, offset);
                return sample32;

            default:
                return 0f;
        }
    }

    public void Dispose()
    {
        if (_disposed)
            return;

        _disposed = true;
        _accessor.Dispose();
        _mappedFile.Dispose();
    }

    private readonly record struct WavHeader(int SampleRate, short Channels, short BitsPerSample, long DataOffset, long DataSize);
}
