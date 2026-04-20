using Ams.Core.Artifacts;
using Ams.Core.Audio;
using Ams.Core.Runtime.Book;

namespace Ams.Core.Runtime.Audio;

public sealed class AudioBufferContext
{
    private readonly AudioBufferDescriptor _descriptor;
    private readonly Func<AudioBufferDescriptor, AudioBuffer?> _loader;
    private readonly object _waveformLock = new();
    private readonly Dictionary<int, WaveformPeaks> _waveformPeaksCache = new();
    private AudioBuffer? _buffer;
    private bool _loaded;

    internal AudioBufferContext(AudioBufferDescriptor descriptor, Func<AudioBufferDescriptor, AudioBuffer?> loader)
    {
        _descriptor = descriptor;
        _loader = loader;
    }

    public AudioBufferDescriptor Descriptor => _descriptor;

    public AudioBuffer? Buffer
    {
        get
        {
            if (!_loaded)
            {
                _buffer = _loader(_descriptor);
                _loaded = true;
                Log.Debug(
                    "AudioBufferContext loaded buffer {BufferId} (has data: {HasData})",
                    _descriptor.BufferId,
                    _buffer is not null);
            }

            return _buffer;
        }
    }

    public bool IsLoaded => _loaded && _buffer != null;

    public WaveformPeaks? GetOrCreateWaveformPeaks(int bucketCount)
    {
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(bucketCount);

        lock (_waveformLock)
        {
            if (_waveformPeaksCache.TryGetValue(bucketCount, out var cached))
            {
                return cached;
            }

            var buffer = Buffer;
            if (buffer is null)
            {
                return null;
            }

            var peaks = WaveformPeakExtractor.ComputeMonoMinMaxEnvelope(buffer, bucketCount);
            _waveformPeaksCache[bucketCount] = peaks;
            return peaks;
        }
    }

    public void Unload()
    {
        _buffer = null;
        lock (_waveformLock)
        {
            _waveformPeaksCache.Clear();
        }
        _loaded = false;
        Log.Debug("AudioBufferContext unloaded buffer {BufferId}", _descriptor.BufferId);
    }
}
