using Ams.Core.Artifacts;
using Ams.Core.Processors;
using Ams.Core.Runtime.Book;
using Ams.Core.Runtime.Common;
using Ams.Core.Runtime.Interfaces;

namespace Ams.Core.Runtime.Audio;

public sealed class AudioBufferManager : IAudioBufferManager
{
    private readonly object _sync = new();
    private readonly IReadOnlyList<AudioBufferDescriptor> _descriptors;
    private readonly Dictionary<string, AudioBufferContext> _cache;
    private readonly Func<AudioBufferDescriptor, AudioBuffer?> _loader;
    private readonly RuntimeCachePolicy _cachePolicy;
    private int _cursor;

    public AudioBufferManager(
        IReadOnlyList<AudioBufferDescriptor>? descriptors,
        Func<AudioBufferDescriptor, AudioBuffer?>? loader = null)
    {
        _descriptors = descriptors ?? Array.Empty<AudioBufferDescriptor>();
        _loader = loader ?? DefaultLoader;
        _cachePolicy = RuntimeLifetimePolicies.AudioBuffers;
        _cache = new Dictionary<string, AudioBufferContext>(StringComparer.OrdinalIgnoreCase);
        _cursor = 0;
    }

    public int Count => _descriptors.Count;

    internal RuntimeCachePolicy CachePolicy => _cachePolicy;

    /// <summary>
    /// Gets the chapter's raw buffer context without changing the current cursor.
    /// </summary>
    public AudioBufferContext? Raw => TryGetByBufferId("raw", moveCursor: false);

    /// <summary>
    /// Gets the chapter's treated buffer context without changing the current cursor.
    /// </summary>
    public AudioBufferContext? Treated => TryGetByBufferId("treated", moveCursor: false);

    /// <summary>
    /// Gets the chapter's corrected buffer context without changing the current cursor.
    /// </summary>
    public AudioBufferContext? Corrected => TryGetByBufferId("corrected", moveCursor: false);

    /// <summary>
    /// Gets the chapter's filtered buffer context without changing the current cursor.
    /// </summary>
    public AudioBufferContext? Filtered => TryGetByBufferId("filtered", moveCursor: false);

    public AudioBufferContext Current
    {
        get
        {
            lock (_sync)
            {
                if (_descriptors.Count == 0)
                {
                    throw new InvalidOperationException("No audio buffers have been registered for this chapter.");
                }

                return LoadCore(_cursor);
            }
        }
    }

    public AudioBufferContext Load(int index)
    {
        lock (_sync)
        {
            if ((uint)index >= (uint)_descriptors.Count)
            {
                throw new ArgumentOutOfRangeException(nameof(index));
            }

            return LoadCore(index);
        }
    }

    public AudioBufferContext Load(string bufferId)
    {
        return GetByBufferId(bufferId, moveCursor: true);
    }

    public bool TryMoveNext(out AudioBufferContext bufferContext)
    {
        lock (_sync)
        {
            if (_cursor + 1 >= _descriptors.Count)
            {
                bufferContext = _descriptors.Count == 0 ? null! : LoadCore(_cursor);
                return false;
            }

            bufferContext = LoadCore(_cursor + 1);
            return true;
        }
    }

    public bool TryMovePrevious(out AudioBufferContext bufferContext)
    {
        lock (_sync)
        {
            if (_cursor <= 0 || _descriptors.Count == 0)
            {
                bufferContext = _descriptors.Count == 0 ? null! : LoadCore(_cursor);
                return false;
            }

            bufferContext = LoadCore(_cursor - 1);
            return true;
        }
    }

    public void Reset()
    {
        lock (_sync)
        {
            _cursor = 0;
        }
    }

    public void Deallocate(string bufferId)
    {
        if (string.IsNullOrWhiteSpace(bufferId))
        {
            return;
        }

        lock (_sync)
        {
            if (_cache.Remove(bufferId, out var context))
            {
                context.Unload();
                Log.Debug(
                    "AudioBufferManager deallocated buffer {BufferId} (cache {CacheCount})",
                    bufferId,
                    _cache.Count);
            }
        }
    }

    public void DeallocateAll()
    {
        lock (_sync)
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
    }

    private AudioBufferContext LoadCore(int index)
    {
        _cursor = index;
        var descriptor = _descriptors[index];
        return GetOrCreate(descriptor);
    }

    private AudioBufferContext GetOrCreate(AudioBufferDescriptor descriptor)
    {
        if (!_cache.TryGetValue(descriptor.BufferId, out var context))
        {
            context = new AudioBufferContext(descriptor, _loader);
            _cache[descriptor.BufferId] = context;
            Log.Debug(
                "AudioBufferManager created buffer context {BufferId} (cache {CacheCount}, policy {Policy})",
                descriptor.BufferId,
                _cache.Count,
                _cachePolicy.Name);
        }
        else
        {
            Log.Debug(
                "AudioBufferManager reused buffer context {BufferId} (cache {CacheCount}, policy {Policy})",
                descriptor.BufferId,
                _cache.Count,
                _cachePolicy.Name);
        }

        return context;
    }

    private AudioBufferContext GetByBufferId(string bufferId, bool moveCursor)
    {
        return TryGetByBufferId(bufferId, moveCursor)
            ?? throw new KeyNotFoundException($"Buffer '{bufferId}' not found in chapter context.");
    }

    private AudioBufferContext? TryGetByBufferId(string bufferId, bool moveCursor)
    {
        ArgumentException.ThrowIfNullOrEmpty(bufferId);
        lock (_sync)
        {
            for (int i = 0; i < _descriptors.Count; i++)
            {
                if (!string.Equals(_descriptors[i].BufferId, bufferId, StringComparison.OrdinalIgnoreCase))
                {
                    continue;
                }

                if (moveCursor)
                {
                    _cursor = i;
                }

                return GetOrCreate(_descriptors[i]);
            }

            return null;
        }
    }

    private AudioBuffer? DefaultLoader(AudioBufferDescriptor descriptor)
    {
        if (!File.Exists(descriptor.Path))
        {
            return null;
        }

        try
        {
            return AudioProcessor.Decode(descriptor.Path);
        }
        catch (Exception ex)
        {
            Log.Warn("AudioBufferManager failed to decode {Path}: {Message}", descriptor.Path, ex.Message);
            return null;
        }
    }
}
