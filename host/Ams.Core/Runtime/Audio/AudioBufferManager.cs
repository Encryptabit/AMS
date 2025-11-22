using Ams.Core.Artifacts;
using Ams.Core.Processors;
using Ams.Core.Runtime.Book;
using Ams.Core.Runtime.Interfaces;

namespace Ams.Core.Runtime.Audio;

public sealed class AudioBufferManager : IAudioBufferManager
{
    private readonly IReadOnlyList<AudioBufferDescriptor> _descriptors;
    private readonly Dictionary<string, AudioBufferContext> _cache;
    private readonly Func<AudioBufferDescriptor, AudioBuffer?> _loader;
    private int _cursor;

    public AudioBufferManager(
        IReadOnlyList<AudioBufferDescriptor>? descriptors,
        Func<AudioBufferDescriptor, AudioBuffer?>? loader = null)
    {
        _descriptors = descriptors ?? Array.Empty<AudioBufferDescriptor>();
        _loader = loader ?? DefaultLoader;
        _cache = new Dictionary<string, AudioBufferContext>(StringComparer.OrdinalIgnoreCase);
        _cursor = 0;
    }

    public int Count => _descriptors.Count;

    public AudioBufferContext Current
    {
        get
        {
            if (_descriptors.Count == 0)
            {
                throw new InvalidOperationException("No audio buffers have been registered for this chapter.");
            }

            return Load(_cursor);
        }
    }

    public AudioBufferContext Load(int index)
    {
        if ((uint)index >= (uint)_descriptors.Count)
        {
            throw new ArgumentOutOfRangeException(nameof(index));
        }

        _cursor = index;
        var descriptor = _descriptors[index];
        return GetOrCreate(descriptor);
    }

    public AudioBufferContext Load(string bufferId)
    {
        ArgumentException.ThrowIfNullOrEmpty(bufferId);
        for (int i = 0; i < _descriptors.Count; i++)
        {
            if (string.Equals(_descriptors[i].BufferId, bufferId, StringComparison.OrdinalIgnoreCase))
            {
                _cursor = i;
                return GetOrCreate(_descriptors[i]);
            }
        }

        throw new KeyNotFoundException($"Buffer '{bufferId}' not found in chapter context.");
    }

    public bool TryMoveNext(out AudioBufferContext bufferContext)
    {
        if (_cursor + 1 >= _descriptors.Count)
        {
            bufferContext = _descriptors.Count == 0 ? null! : Current;
            return false;
        }

        bufferContext = Load(_cursor + 1);
        return true;
    }

    public bool TryMovePrevious(out AudioBufferContext bufferContext)
    {
        if (_cursor <= 0 || _descriptors.Count == 0)
        {
            bufferContext = _descriptors.Count == 0 ? null! : Current;
            return false;
        }

        bufferContext = Load(_cursor - 1);
        return true;
    }

    public void Reset()
    {
        _cursor = 0;
    }

    public void Deallocate(string bufferId)
    {
        if (string.IsNullOrWhiteSpace(bufferId))
        {
            return;
        }

        if (_cache.Remove(bufferId, out var context))
        {
            context.Unload();
            Log.Debug(
                "AudioBufferManager deallocated buffer {BufferId} (cache {CacheCount})",
                bufferId,
                _cache.Count);
        }
    }

    public void DeallocateAll()
    {
        foreach (var context in _cache.Values)
        {
            context.Unload();
        }

        if (_cache.Count > 0)
        {
            Log.Debug("AudioBufferManager flushed {Count} buffer(s)", _cache.Count);
        }

        _cache.Clear();
    }

    private AudioBufferContext GetOrCreate(AudioBufferDescriptor descriptor)
    {
        if (!_cache.TryGetValue(descriptor.BufferId, out var context))
        {
            context = new AudioBufferContext(descriptor, _loader);
            _cache[descriptor.BufferId] = context;
            Log.Debug(
                "AudioBufferManager created buffer context {BufferId} (cache {CacheCount})",
                descriptor.BufferId,
                _cache.Count);
        }
        else
        {
            Log.Debug(
                "AudioBufferManager reused buffer context {BufferId} (cache {CacheCount})",
                descriptor.BufferId,
                _cache.Count);
        }

        return context;
    }

    private AudioBuffer? DefaultLoader(AudioBufferDescriptor descriptor)
    {
        if (!File.Exists(descriptor.Path))
        {
            return null;
        }

        try
        {
            return AudioProcessor.Decode(descriptor.Path, new AudioDecodeOptions
            {
                TargetSampleRate = descriptor.SampleRate,
                TargetChannels = descriptor.Channels
            });
        }
        catch (Exception ex)
        {
            Log.Warn("AudioBufferManager failed to decode {Path}: {Message}", descriptor.Path, ex.Message);
            return null;
        }
    }
}