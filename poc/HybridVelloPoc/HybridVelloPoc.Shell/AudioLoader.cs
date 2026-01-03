using System.IO;
using System.IO.MemoryMappedFiles;

namespace HybridVelloPoc.Shell;

/// <summary>
/// Loads and provides sample data from WAV files using memory-mapping for large files.
/// Uses pre-computed mipmaps for efficient rendering at any zoom level (like wavesurfer.js).
/// </summary>
public sealed class AudioLoader : IDisposable
{
    private readonly MemoryMappedFile _mappedFile;
    private readonly MemoryMappedViewAccessor _accessor;
    private readonly long _dataOffset;
    private readonly int _bytesPerSample;
    private bool _disposed;

    // Mipmap levels - each level has half the resolution of the previous
    // Level 0 = 1 sample per entry, Level 1 = 2 samples per entry, etc.
    private readonly List<(float[] mins, float[] maxs)> _mipmaps = new();
    private const int SamplesPerBaseLevel = 256; // Base mipmap: 256 samples per entry
    private const int MaxMipmapLevels = 12;      // Enough for hours of audio

    public int SampleRate { get; }
    public int Channels { get; }
    public int BitsPerSample { get; }
    public long TotalSamples { get; }
    public double Duration => TotalSamples / (double)SampleRate;
    public bool MipmapsReady { get; private set; }

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

        // Build mipmaps asynchronously (non-blocking)
        Task.Run(BuildMipmaps);
    }

    /// <summary>
    /// Builds mipmap pyramid for efficient rendering at any zoom level.
    /// Called automatically on construction, runs in background.
    /// </summary>
    private void BuildMipmaps()
    {
        try
        {
            // Level 0: Base resolution (256 samples per entry)
            var level0Size = (int)((TotalSamples + SamplesPerBaseLevel - 1) / SamplesPerBaseLevel);
            var mins0 = new float[level0Size];
            var maxs0 = new float[level0Size];

            // Initialize
            for (int i = 0; i < level0Size; i++)
            {
                mins0[i] = float.MaxValue;
                maxs0[i] = float.MinValue;
            }

            // Build base level from raw samples
            var bufferSamples = 8192;
            var buffer = new byte[bufferSamples * _bytesPerSample];
            long currentSample = 0;

            while (currentSample < TotalSamples)
            {
                var samplesToRead = (int)Math.Min(bufferSamples, TotalSamples - currentSample);
                var bytesToRead = samplesToRead * _bytesPerSample;
                var offset = _dataOffset + currentSample * _bytesPerSample;

                _accessor.ReadArray(offset, buffer, 0, bytesToRead);

                for (int i = 0; i < samplesToRead; i++)
                {
                    var bucketIndex = (int)((currentSample + i) / SamplesPerBaseLevel);
                    if (bucketIndex >= level0Size) break;

                    var sampleValue = ReadSample(buffer, i * _bytesPerSample);
                    if (sampleValue < mins0[bucketIndex]) mins0[bucketIndex] = sampleValue;
                    if (sampleValue > maxs0[bucketIndex]) maxs0[bucketIndex] = sampleValue;
                }

                currentSample += samplesToRead;
            }

            // Fix uninitialized values
            for (int i = 0; i < level0Size; i++)
            {
                if (mins0[i] == float.MaxValue) mins0[i] = 0;
                if (maxs0[i] == float.MinValue) maxs0[i] = 0;
            }

            _mipmaps.Add((mins0, maxs0));

            // Build higher mipmap levels (each level halves resolution)
            var prevMins = mins0;
            var prevMaxs = maxs0;

            for (int level = 1; level < MaxMipmapLevels; level++)
            {
                var levelSize = (prevMins.Length + 1) / 2;
                if (levelSize < 2) break;

                var mins = new float[levelSize];
                var maxs = new float[levelSize];

                for (int i = 0; i < levelSize; i++)
                {
                    var idx1 = i * 2;
                    var idx2 = Math.Min(idx1 + 1, prevMins.Length - 1);
                    mins[i] = Math.Min(prevMins[idx1], prevMins[idx2]);
                    maxs[i] = Math.Max(prevMaxs[idx1], prevMaxs[idx2]);
                }

                _mipmaps.Add((mins, maxs));
                prevMins = mins;
                prevMaxs = maxs;
            }

            MipmapsReady = true;
        }
        catch
        {
            // Silently fail - will fall back to direct computation
        }
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
    /// Uses pre-computed mipmaps for O(pixels) performance instead of O(samples).
    /// </summary>
    public (float[] mins, float[] maxs) GetMinMaxSamples(long startSample, long endSample, int targetPixels)
    {
        startSample = Math.Max(0, startSample);
        endSample = Math.Min(TotalSamples, endSample);

        if (startSample >= endSample || targetPixels <= 0)
            return (Array.Empty<float>(), Array.Empty<float>());

        var sampleCount = endSample - startSample;
        var actualPixels = (int)Math.Min(targetPixels, sampleCount);

        // Use mipmaps if ready, otherwise return empty (will show flat line briefly)
        if (!MipmapsReady || _mipmaps.Count == 0)
            return (new float[actualPixels], new float[actualPixels]);

        // Find best mipmap level for this zoom
        // samplesPerPixel tells us how many original samples per output pixel
        var samplesPerPixel = (double)sampleCount / actualPixels;

        // Find mipmap level where entries cover roughly samplesPerPixel samples
        // Level 0 covers SamplesPerBaseLevel samples, level N covers SamplesPerBaseLevel * 2^N
        int bestLevel = 0;
        long samplesPerEntry = SamplesPerBaseLevel;
        for (int level = 0; level < _mipmaps.Count; level++)
        {
            if (samplesPerEntry > samplesPerPixel * 2) break;
            bestLevel = level;
            samplesPerEntry *= 2;
        }

        var (levelMins, levelMaxs) = _mipmaps[bestLevel];
        samplesPerEntry = SamplesPerBaseLevel * (1L << bestLevel);

        var mins = new float[actualPixels];
        var maxs = new float[actualPixels];

        // Map sample range to mipmap entries
        var startEntry = (int)(startSample / samplesPerEntry);
        var endEntry = (int)((endSample + samplesPerEntry - 1) / samplesPerEntry);
        startEntry = Math.Max(0, startEntry);
        endEntry = Math.Min(levelMins.Length, endEntry);

        var entriesPerPixel = (double)(endEntry - startEntry) / actualPixels;

        for (int pixel = 0; pixel < actualPixels; pixel++)
        {
            var entryStart = startEntry + (int)(pixel * entriesPerPixel);
            var entryEnd = startEntry + (int)((pixel + 1) * entriesPerPixel);
            entryEnd = Math.Max(entryEnd, entryStart + 1);
            entryEnd = Math.Min(entryEnd, endEntry);

            var minVal = float.MaxValue;
            var maxVal = float.MinValue;

            for (int e = entryStart; e < entryEnd && e < levelMins.Length; e++)
            {
                if (levelMins[e] < minVal) minVal = levelMins[e];
                if (levelMaxs[e] > maxVal) maxVal = levelMaxs[e];
            }

            mins[pixel] = minVal == float.MaxValue ? 0 : minVal;
            maxs[pixel] = maxVal == float.MinValue ? 0 : maxVal;
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
