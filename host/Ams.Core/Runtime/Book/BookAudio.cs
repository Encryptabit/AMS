using System.Collections.Generic;
using System.IO;
using System.Linq;
using Ams.Core.Artifacts;
using Ams.Core.Processors;

namespace Ams.Core.Runtime.Book;

/// <summary>
/// Manages book-level audio assets (roomtone, etc.) with lazy loading.
/// </summary>
public sealed class BookAudio
{
    private readonly BookContext _book;
    private readonly object _pickupLock = new();
    private readonly Dictionary<string, PickupAudioDescriptor> _pickupsById =
        new(StringComparer.OrdinalIgnoreCase);
    private readonly Dictionary<string, string> _pickupIdByPath =
        new(StringComparer.OrdinalIgnoreCase);
    private readonly Dictionary<string, AudioBuffer> _pickupBuffersById =
        new(StringComparer.OrdinalIgnoreCase);
    private readonly object _playbackErrorAlertLock = new();
    private PlaybackErrorAlertDescriptor? _playbackErrorAlertSound;
    private AudioBuffer? _playbackErrorAlertBuffer;
    private AudioBuffer? _roomtone;
    private bool _roomtoneLoaded;

    internal BookAudio(BookContext book)
    {
        _book = book ?? throw new ArgumentNullException(nameof(book));
    }

    /// <summary>
    /// Gets the path to the roomtone file for this book.
    /// </summary>
    public string RoomtonePath => _book.ResolveArtifactFile("roomtone.wav").FullName;

    /// <summary>
    /// Gets whether a roomtone file exists for this book.
    /// </summary>
    public bool HasRoomtone => File.Exists(RoomtonePath);

    /// <summary>
    /// Gets the registered pickup descriptors for this book.
    /// Descriptors are book-level and independent of chapter context.
    /// </summary>
    public IReadOnlyList<PickupAudioDescriptor> Pickups
    {
        get
        {
            lock (_pickupLock)
            {
                return _pickupsById.Values
                    .OrderBy(p => p.RegisteredAtUtc)
                    .ToArray();
            }
        }
    }

    /// <summary>
    /// Gets the app-level playback-error alert sound descriptor currently loaded into this book context.
    /// </summary>
    public PlaybackErrorAlertDescriptor? PlaybackErrorAlertSound
    {
        get
        {
            lock (_playbackErrorAlertLock)
            {
                return _playbackErrorAlertSound;
            }
        }
    }

    /// <summary>
    /// Gets the roomtone audio buffer, loading it lazily if needed.
    /// Returns null if no roomtone file exists.
    /// </summary>
    public AudioBuffer? Roomtone
    {
        get
        {
            if (!_roomtoneLoaded)
            {
                _roomtone = LoadRoomtone();
                _roomtoneLoaded = true;
            }

            return _roomtone;
        }
    }

    /// <summary>
    /// Unloads the roomtone buffer to free memory.
    /// </summary>
    public void UnloadRoomtone()
    {
        _roomtone = null;
        _roomtoneLoaded = false;
        Log.Debug("BookAudio unloaded roomtone for {BookId}", _book.Descriptor.BookId);
    }

    /// <summary>
    /// Registers a pickup file in the book-level pickup collection.
    /// Re-registering the same path refreshes source metadata and invalidates any stale cached buffer.
    /// </summary>
    public PickupAudioDescriptor RegisterPickup(string pickupPath, string? pickupId = null)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(pickupPath);

        var sourceFile = new FileInfo(pickupPath.Trim());
        if (!sourceFile.Exists)
        {
            throw new FileNotFoundException($"Pickup file not found: {pickupPath}", pickupPath);
        }

        var canonicalPath = sourceFile.FullName;
        var sourceModifiedUtc = sourceFile.LastWriteTimeUtc;
        var sourceSizeBytes = sourceFile.Length;
        var resolvedPickupId = string.IsNullOrWhiteSpace(pickupId) ? canonicalPath : pickupId.Trim();

        lock (_pickupLock)
        {
            if (_pickupIdByPath.TryGetValue(canonicalPath, out var existingId) &&
                _pickupsById.TryGetValue(existingId, out var existingDescriptor))
            {
                var sourceChanged = existingDescriptor.SourceModifiedUtc != sourceModifiedUtc
                    || existingDescriptor.SourceSizeBytes != sourceSizeBytes;
                if (sourceChanged)
                {
                    _pickupBuffersById.Remove(existingId);
                    existingDescriptor = existingDescriptor with
                    {
                        SourceModifiedUtc = sourceModifiedUtc,
                        SourceSizeBytes = sourceSizeBytes
                    };
                    _pickupsById[existingId] = existingDescriptor;
                }

                return existingDescriptor;
            }

            if (_pickupsById.TryGetValue(resolvedPickupId, out var descriptorWithSameId))
            {
                if (!string.Equals(descriptorWithSameId.SourcePath, canonicalPath, StringComparison.OrdinalIgnoreCase))
                {
                    throw new InvalidOperationException(
                        $"Pickup id '{resolvedPickupId}' is already mapped to '{descriptorWithSameId.SourcePath}'.");
                }

                var sourceChanged = descriptorWithSameId.SourceModifiedUtc != sourceModifiedUtc
                    || descriptorWithSameId.SourceSizeBytes != sourceSizeBytes;
                if (sourceChanged)
                {
                    _pickupBuffersById.Remove(resolvedPickupId);
                    descriptorWithSameId = descriptorWithSameId with
                    {
                        SourceModifiedUtc = sourceModifiedUtc,
                        SourceSizeBytes = sourceSizeBytes
                    };
                    _pickupsById[resolvedPickupId] = descriptorWithSameId;
                }

                _pickupIdByPath[canonicalPath] = resolvedPickupId;
                return descriptorWithSameId;
            }

            var descriptor = new PickupAudioDescriptor(
                PickupId: resolvedPickupId,
                SourcePath: canonicalPath,
                SourceModifiedUtc: sourceModifiedUtc,
                SourceSizeBytes: sourceSizeBytes,
                RegisteredAtUtc: DateTime.UtcNow);

            _pickupsById[descriptor.PickupId] = descriptor;
            _pickupIdByPath[canonicalPath] = descriptor.PickupId;

            return descriptor;
        }
    }

    /// <summary>
    /// Loads a pickup buffer lazily by pickup id and caches it for reuse.
    /// </summary>
    public AudioBuffer LoadPickup(string pickupId)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(pickupId);

        lock (_pickupLock)
        {
            if (!_pickupsById.TryGetValue(pickupId, out var descriptor))
            {
                throw new KeyNotFoundException($"Pickup '{pickupId}' is not registered.");
            }

            var sourceFile = new FileInfo(descriptor.SourcePath);
            if (!sourceFile.Exists)
            {
                throw new FileNotFoundException($"Pickup file not found: {descriptor.SourcePath}", descriptor.SourcePath);
            }

            var sourceChanged = descriptor.SourceModifiedUtc != sourceFile.LastWriteTimeUtc
                || descriptor.SourceSizeBytes != sourceFile.Length;
            if (sourceChanged)
            {
                _pickupBuffersById.Remove(descriptor.PickupId);
                descriptor = descriptor with
                {
                    SourceModifiedUtc = sourceFile.LastWriteTimeUtc,
                    SourceSizeBytes = sourceFile.Length
                };
                _pickupsById[descriptor.PickupId] = descriptor;
            }

            if (_pickupBuffersById.TryGetValue(descriptor.PickupId, out var cachedBuffer))
            {
                return cachedBuffer;
            }

            var buffer = AudioProcessor.Decode(descriptor.SourcePath);
            _pickupBuffersById[descriptor.PickupId] = buffer;
            Log.Debug(
                "BookAudio loaded pickup {PickupId} for {BookId} ({Duration:F2}s, {SampleRate}Hz)",
                descriptor.PickupId,
                _book.Descriptor.BookId,
                buffer.Length / (double)buffer.SampleRate,
                buffer.SampleRate);
            return buffer;
        }
    }

    /// <summary>
    /// Registers a pickup by source path (if needed), then returns the lazy-cached buffer.
    /// </summary>
    public AudioBuffer LoadPickupByPath(string pickupPath)
    {
        var descriptor = RegisterPickup(pickupPath);
        return LoadPickup(descriptor.PickupId);
    }

    /// <summary>
    /// Releases a cached pickup buffer while keeping the descriptor registration.
    /// </summary>
    public void DeallocatePickup(string pickupId)
    {
        if (string.IsNullOrWhiteSpace(pickupId))
        {
            return;
        }

        lock (_pickupLock)
        {
            if (_pickupBuffersById.Remove(pickupId))
            {
                Log.Debug("BookAudio deallocated pickup buffer {PickupId} for {BookId}", pickupId, _book.Descriptor.BookId);
            }
        }
    }

    /// <summary>
    /// Clears all cached pickup buffers while preserving pickup descriptors.
    /// </summary>
    public void DeallocateAllPickups()
    {
        lock (_pickupLock)
        {
            var count = _pickupBuffersById.Count;
            _pickupBuffersById.Clear();
            if (count > 0)
            {
                Log.Debug("BookAudio deallocated {Count} pickup buffer(s) for {BookId}", count, _book.Descriptor.BookId);
            }
        }
    }

    /// <summary>
    /// Clears all registered pickup descriptors and cached pickup buffers.
    /// </summary>
    public void ClearPickups()
    {
        lock (_pickupLock)
        {
            var descriptorCount = _pickupsById.Count;
            var bufferCount = _pickupBuffersById.Count;
            _pickupsById.Clear();
            _pickupIdByPath.Clear();
            _pickupBuffersById.Clear();
            if (descriptorCount > 0 || bufferCount > 0)
            {
                Log.Debug(
                    "BookAudio cleared pickups for {BookId} (descriptors={DescriptorCount}, buffers={BufferCount})",
                    _book.Descriptor.BookId,
                    descriptorCount,
                    bufferCount);
            }
        }
    }

    /// <summary>
    /// Registers the app-level playback-error alert sound in this book context.
    /// Re-registering refreshes metadata and invalidates stale cached buffers when the source changes.
    /// </summary>
    public PlaybackErrorAlertDescriptor RegisterPlaybackErrorAlertSound(string soundPath)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(soundPath);

        var sourceFile = new FileInfo(soundPath.Trim());
        if (!sourceFile.Exists)
        {
            throw new FileNotFoundException($"Playback alert sound file not found: {soundPath}", soundPath);
        }

        var canonicalPath = sourceFile.FullName;
        var sourceModifiedUtc = sourceFile.LastWriteTimeUtc;
        var sourceSizeBytes = sourceFile.Length;

        lock (_playbackErrorAlertLock)
        {
            if (_playbackErrorAlertSound is not null
                && string.Equals(_playbackErrorAlertSound.SourcePath, canonicalPath, StringComparison.OrdinalIgnoreCase)
                && _playbackErrorAlertSound.SourceModifiedUtc == sourceModifiedUtc
                && _playbackErrorAlertSound.SourceSizeBytes == sourceSizeBytes)
            {
                return _playbackErrorAlertSound;
            }

            _playbackErrorAlertBuffer = null;
            _playbackErrorAlertSound = new PlaybackErrorAlertDescriptor(
                SourcePath: canonicalPath,
                SourceModifiedUtc: sourceModifiedUtc,
                SourceSizeBytes: sourceSizeBytes,
                RegisteredAtUtc: DateTime.UtcNow);

            Log.Debug(
                "BookAudio registered playback error alert sound for {BookId} from {Path}",
                _book.Descriptor.BookId,
                canonicalPath);

            return _playbackErrorAlertSound;
        }
    }

    /// <summary>
    /// Loads and caches the app-level playback-error alert buffer.
    /// Returns null when no alert sound is configured or when decoding fails.
    /// </summary>
    public AudioBuffer? LoadPlaybackErrorAlertSound()
    {
        lock (_playbackErrorAlertLock)
        {
            if (_playbackErrorAlertSound is null)
            {
                return null;
            }

            var sourceFile = new FileInfo(_playbackErrorAlertSound.SourcePath);
            if (!sourceFile.Exists)
            {
                Log.Warn(
                    "BookAudio playback alert sound missing for {BookId} at {Path}",
                    _book.Descriptor.BookId,
                    _playbackErrorAlertSound.SourcePath);
                _playbackErrorAlertSound = null;
                _playbackErrorAlertBuffer = null;
                return null;
            }

            var sourceChanged = _playbackErrorAlertSound.SourceModifiedUtc != sourceFile.LastWriteTimeUtc
                || _playbackErrorAlertSound.SourceSizeBytes != sourceFile.Length;
            if (sourceChanged)
            {
                _playbackErrorAlertBuffer = null;
                _playbackErrorAlertSound = _playbackErrorAlertSound with
                {
                    SourceModifiedUtc = sourceFile.LastWriteTimeUtc,
                    SourceSizeBytes = sourceFile.Length
                };
            }

            if (_playbackErrorAlertBuffer is not null)
            {
                return _playbackErrorAlertBuffer;
            }

            try
            {
                _playbackErrorAlertBuffer = AudioProcessor.Decode(sourceFile.FullName);
                Log.Debug(
                    "BookAudio loaded playback alert sound for {BookId} ({Duration:F2}s, {SampleRate}Hz)",
                    _book.Descriptor.BookId,
                    _playbackErrorAlertBuffer.Length / (double)_playbackErrorAlertBuffer.SampleRate,
                    _playbackErrorAlertBuffer.SampleRate);
                return _playbackErrorAlertBuffer;
            }
            catch (Exception ex)
            {
                Log.Warn(
                    "BookAudio failed to decode playback alert sound for {BookId} from {Path}: {Message}",
                    _book.Descriptor.BookId,
                    sourceFile.FullName,
                    ex.Message);
                return null;
            }
        }
    }

    /// <summary>
    /// Releases the cached playback-error alert buffer while keeping its descriptor.
    /// </summary>
    public void UnloadPlaybackErrorAlertSound()
    {
        lock (_playbackErrorAlertLock)
        {
            _playbackErrorAlertBuffer = null;
        }
    }

    /// <summary>
    /// Clears the playback-error alert descriptor and cached buffer from this book context.
    /// </summary>
    public void ClearPlaybackErrorAlertSound()
    {
        lock (_playbackErrorAlertLock)
        {
            _playbackErrorAlertSound = null;
            _playbackErrorAlertBuffer = null;
        }
    }

    /// <summary>
    /// Unloads all book-level audio caches (roomtone and pickups).
    /// </summary>
    public void UnloadAll()
    {
        UnloadRoomtone();
        DeallocateAllPickups();
        UnloadPlaybackErrorAlertSound();
    }

    private AudioBuffer? LoadRoomtone()
    {
        var path = RoomtonePath;
        if (!File.Exists(path))
        {
            Log.Debug("BookAudio roomtone not found at {Path}", path);
            return null;
        }

        try
        {
            var buffer = AudioProcessor.Decode(path);
            Log.Debug(
                "BookAudio loaded roomtone for {BookId} ({Duration:F2}s, {SampleRate}Hz)",
                _book.Descriptor.BookId,
                buffer.Length / (double)buffer.SampleRate,
                buffer.SampleRate);
            return buffer;
        }
        catch (Exception ex)
        {
            Log.Warn("BookAudio failed to load roomtone from {Path}: {Message}", path, ex.Message);
            return null;
        }
    }
}

/// <summary>
/// Book-level pickup metadata used by <see cref="BookAudio"/> for lazy pickup buffer loading.
/// </summary>
public sealed record PickupAudioDescriptor(
    string PickupId,
    string SourcePath,
    DateTime SourceModifiedUtc,
    long SourceSizeBytes,
    DateTime RegisteredAtUtc);

/// <summary>
/// App-level playback-error alert metadata loaded into <see cref="BookAudio"/>.
/// </summary>
public sealed record PlaybackErrorAlertDescriptor(
    string SourcePath,
    DateTime SourceModifiedUtc,
    long SourceSizeBytes,
    DateTime RegisteredAtUtc);
