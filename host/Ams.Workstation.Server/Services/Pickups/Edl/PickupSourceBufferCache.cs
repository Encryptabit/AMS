using System.Security.Cryptography;
using System.Text;
using Ams.Core.Artifacts;
using Ams.Core.Processors;
using Ams.Workstation.Server.Models;

namespace Ams.Workstation.Server.Services.Pickups.Edl;

/// <summary>
/// Caches decoded pickup source buffers by canonical path + fingerprint,
/// allowing one decode with many zero-copy slice views.
/// </summary>
public sealed class PickupSourceBufferCache
{
    private readonly Func<string, AudioBuffer> _decoder;
    private readonly object _lock = new();
    private readonly Dictionary<string, CacheEntry> _entries = new(StringComparer.OrdinalIgnoreCase);
    private int _decodeCount;

    public PickupSourceBufferCache()
        : this(static path => AudioProcessor.Decode(path))
    {
    }

    internal PickupSourceBufferCache(Func<string, AudioBuffer> decoder)
    {
        _decoder = decoder ?? throw new ArgumentNullException(nameof(decoder));
    }

    /// <summary>
    /// Number of decode operations performed since cache construction.
    /// Useful for diagnostics/tests proving one-decode-many-slices behavior.
    /// </summary>
    public int DecodeCount
    {
        get
        {
            lock (_lock)
            {
                return _decodeCount;
            }
        }
    }

    /// <summary>
    /// Builds a source reference from an on-disk pickup file.
    /// Fingerprint is stable for (path + size + modifiedUtc).
    /// </summary>
    public PickupEdlSourceReference DescribeSource(string sourcePath)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(sourcePath);

        var file = new FileInfo(sourcePath.Trim());
        if (!file.Exists)
        {
            throw new FileNotFoundException(
                $"Pickup source file is missing: '{file.FullName}'.",
                file.FullName);
        }

        var modifiedUtc = file.LastWriteTimeUtc;
        var fingerprint = ComputeFingerprint(file.FullName, file.Length, modifiedUtc);

        return new PickupEdlSourceReference(
            path: file.FullName,
            fingerprint: fingerprint,
            fileSizeBytes: file.Length,
            modifiedAtUtc: modifiedUtc);
    }

    /// <summary>
    /// Returns the full decoded source buffer (cached).
    /// Validates source fingerprint before returning.
    /// </summary>
    public AudioBuffer GetSourceBuffer(
        PickupEdlSourceReference source,
        string chapterStem,
        string opId,
        CancellationToken ct = default)
    {
        ArgumentNullException.ThrowIfNull(source);
        ArgumentException.ThrowIfNullOrWhiteSpace(chapterStem);
        ArgumentException.ThrowIfNullOrWhiteSpace(opId);

        return GetBufferOrThrow(source, chapterStem, opId, ct);
    }

    /// <summary>
    /// Returns a zero-copy slice by time range (seconds) from cached source buffer.
    /// </summary>
    public AudioBuffer GetSliceByTime(
        PickupEdlSourceReference source,
        double startSec,
        double endSec,
        string chapterStem,
        string opId,
        CancellationToken ct = default)
    {
        ArgumentNullException.ThrowIfNull(source);
        ArgumentException.ThrowIfNullOrWhiteSpace(chapterStem);
        ArgumentException.ThrowIfNullOrWhiteSpace(opId);

        if (!double.IsFinite(startSec) || !double.IsFinite(endSec) || startSec < 0 || endSec <= startSec)
        {
            throw new InvalidOperationException(
                $"Invalid pickup slice range for op '{opId}' in chapter '{chapterStem}': " +
                $"[{startSec:F6}, {endSec:F6}] from source '{source.Path}' " +
                $"(fingerprint='{source.Fingerprint}').");
        }

        var buffer = GetBufferOrThrow(source, chapterStem, opId, ct);
        var startSample = Math.Max(0, (int)Math.Floor(startSec * buffer.SampleRate));
        var endSample = Math.Max(startSample + 1, (int)Math.Ceiling(endSec * buffer.SampleRate));

        return SliceOrThrow(
            buffer,
            startSample,
            endSample,
            source,
            chapterStem,
            opId);
    }

    /// <summary>
    /// Returns a zero-copy slice by sample range from cached source buffer.
    /// </summary>
    public AudioBuffer GetSliceBySamples(
        PickupEdlSourceReference source,
        int startSample,
        int endSample,
        string chapterStem,
        string opId,
        CancellationToken ct = default)
    {
        ArgumentNullException.ThrowIfNull(source);
        ArgumentException.ThrowIfNullOrWhiteSpace(chapterStem);
        ArgumentException.ThrowIfNullOrWhiteSpace(opId);

        var buffer = GetBufferOrThrow(source, chapterStem, opId, ct);
        return SliceOrThrow(
            buffer,
            startSample,
            endSample,
            source,
            chapterStem,
            opId);
    }

    /// <summary>
    /// Invalidates one cached source entry by path.
    /// </summary>
    public void Invalidate(string sourcePath)
    {
        if (string.IsNullOrWhiteSpace(sourcePath))
        {
            return;
        }

        var key = Path.GetFullPath(sourcePath.Trim());
        lock (_lock)
        {
            _entries.Remove(key);
        }
    }

    private AudioBuffer GetBufferOrThrow(
        PickupEdlSourceReference expected,
        string chapterStem,
        string opId,
        CancellationToken ct)
    {
        PickupEdlSourceReference actual;
        try
        {
            actual = DescribeSource(expected.Path);
        }
        catch (FileNotFoundException ex)
        {
            throw new FileNotFoundException(
                $"Pickup source file missing for op '{opId}' in chapter '{chapterStem}' " +
                $"(fingerprint='{expected.Fingerprint}', path='{expected.Path}').",
                expected.Path,
                ex);
        }

        if (!string.Equals(actual.Fingerprint, expected.Fingerprint, StringComparison.Ordinal))
        {
            throw new InvalidOperationException(
                $"Stale source fingerprint for op '{opId}' in chapter '{chapterStem}': " +
                $"expected='{expected.Fingerprint}', actual='{actual.Fingerprint}', path='{actual.Path}'.");
        }

        lock (_lock)
        {
            if (_entries.TryGetValue(actual.Path, out var cached) &&
                string.Equals(cached.Fingerprint, expected.Fingerprint, StringComparison.Ordinal))
            {
                return cached.Buffer;
            }
        }

        ct.ThrowIfCancellationRequested();
        var decoded = _decoder(actual.Path);

        lock (_lock)
        {
            if (_entries.TryGetValue(actual.Path, out var cached) &&
                string.Equals(cached.Fingerprint, expected.Fingerprint, StringComparison.Ordinal))
            {
                return cached.Buffer;
            }

            _entries[actual.Path] = new CacheEntry(expected.Fingerprint, decoded);
            _decodeCount++;
            return decoded;
        }
    }

    private static AudioBuffer SliceOrThrow(
        AudioBuffer sourceBuffer,
        int startSample,
        int endSample,
        PickupEdlSourceReference source,
        string chapterStem,
        string opId)
    {
        if (startSample < 0 || endSample <= startSample || endSample > sourceBuffer.Length)
        {
            throw new InvalidOperationException(
                $"Pickup source slice out of bounds for op '{opId}' in chapter '{chapterStem}': " +
                $"range=[{startSample},{endSample}), bufferLength={sourceBuffer.Length}, " +
                $"fingerprint='{source.Fingerprint}', path='{source.Path}'.");
        }

        return sourceBuffer.Slice(startSample, endSample - startSample);
    }

    private static string ComputeFingerprint(string fullPath, long length, DateTime modifiedUtc)
    {
        var payload = $"{fullPath}|{length}|{modifiedUtc.ToUniversalTime().Ticks}";
        var hash = SHA256.HashData(Encoding.UTF8.GetBytes(payload));
        return Convert.ToHexString(hash)[..24].ToLowerInvariant();
    }

    private sealed record CacheEntry(string Fingerprint, AudioBuffer Buffer);
}
