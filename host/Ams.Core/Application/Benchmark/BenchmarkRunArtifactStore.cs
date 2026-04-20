using System.Collections.Concurrent;
using System.IO;

namespace Ams.Core.Application.Benchmark;

public sealed class BenchmarkRunArtifactStore
{
    private readonly Action<string> _ensureDirectory;
    private readonly Action<string, string> _writeAllText;
    private readonly Func<string, bool> _fileExists;
    private readonly ConcurrentDictionary<string, object> _pathLocks = new(StringComparer.OrdinalIgnoreCase);

    public BenchmarkRunArtifactStore(
        Action<string>? ensureDirectory = null,
        Action<string, string>? writeAllText = null,
        Func<string, bool>? fileExists = null)
    {
        _ensureDirectory = ensureDirectory ?? (path => Directory.CreateDirectory(path));
        _writeAllText = writeAllText ?? File.WriteAllText;
        _fileExists = fileExists ?? File.Exists;
    }

    public Task<FileInfo> WriteManifestAsync(
        DirectoryInfo outputRoot,
        BenchmarkRunManifest manifest,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(manifest);
        cancellationToken.ThrowIfCancellationRequested();

        var path = BuildRunArtifactPath(outputRoot, manifest.RunId, "manifest");
        Persist(path, BenchmarkRunManifest.Serialize(manifest), cancellationToken);
        return Task.FromResult(new FileInfo(path));
    }

    public Task<FileInfo> WriteInvalidRunAsync(
        DirectoryInfo outputRoot,
        BenchmarkInvalidRunArtifact invalidRun,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(invalidRun);
        cancellationToken.ThrowIfCancellationRequested();

        var path = BuildRunArtifactPath(outputRoot, invalidRun.RunId, "invalid-run");
        Persist(path, BenchmarkInvalidRunArtifact.Serialize(invalidRun), cancellationToken);
        return Task.FromResult(new FileInfo(path));
    }

    public Task<FileInfo> WriteCompareAsync(
        DirectoryInfo outputRoot,
        BenchmarkCompareArtifact compareArtifact,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(compareArtifact);
        cancellationToken.ThrowIfCancellationRequested();

        var path = BuildCompareArtifactPath(outputRoot, compareArtifact.CompareId);
        Persist(path, BenchmarkCompareArtifact.Serialize(compareArtifact), cancellationToken, failIfExists: true);
        return Task.FromResult(new FileInfo(path));
    }

    private void Persist(
        string path,
        string payload,
        CancellationToken cancellationToken,
        bool failIfExists = false)
    {
        cancellationToken.ThrowIfCancellationRequested();

        var pathLock = _pathLocks.GetOrAdd(path, static _ => new object());

        lock (pathLock)
        {
            cancellationToken.ThrowIfCancellationRequested();

            if (failIfExists && _fileExists(path))
            {
                throw new IOException($"Benchmark artifact already exists and is locked: {path}");
            }

            var directoryPath = Path.GetDirectoryName(path);
            if (!string.IsNullOrWhiteSpace(directoryPath))
            {
                _ensureDirectory(directoryPath);
            }

            _writeAllText(path, payload);

            if (!_fileExists(path))
            {
                throw new IOException($"Benchmark artifact write did not produce file: {path}");
            }
        }
    }

    private static string BuildRunArtifactPath(DirectoryInfo outputRoot, string runId, string artifactKind)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(runId);
        ArgumentException.ThrowIfNullOrWhiteSpace(artifactKind);

        var normalizedOutputRoot = NormalizeOutputRoot(outputRoot);
        var safeRunId = SanitizeForFileName(runId);
        var normalizedRunToken = safeRunId.StartsWith("run-", StringComparison.OrdinalIgnoreCase)
            ? safeRunId
            : $"run-{safeRunId}";

        var fileName = $"benchmark-{normalizedRunToken}.{artifactKind}.json";
        return Path.Combine(normalizedOutputRoot, fileName);
    }

    private static string BuildCompareArtifactPath(DirectoryInfo outputRoot, string compareId)
    {
        ValidateSafeArtifactIdentifier(compareId, nameof(compareId));

        var normalizedOutputRoot = NormalizeOutputRoot(outputRoot);
        var safeCompareId = SanitizeForFileName(compareId);
        var fileName = $"benchmark-compare-{safeCompareId}.compare.json";
        return Path.Combine(normalizedOutputRoot, fileName);
    }

    private static string NormalizeOutputRoot(DirectoryInfo outputRoot)
    {
        ArgumentNullException.ThrowIfNull(outputRoot);

        var normalizedOutputRoot = Path.GetFullPath(outputRoot.FullName);
        if (string.IsNullOrWhiteSpace(normalizedOutputRoot))
        {
            throw new ArgumentException("Benchmark output root cannot be blank.", nameof(outputRoot));
        }

        if (normalizedOutputRoot.IndexOfAny(Path.GetInvalidPathChars()) >= 0)
        {
            throw new ArgumentException("Benchmark output root contains invalid path characters.", nameof(outputRoot));
        }

        return normalizedOutputRoot;
    }

    private static void ValidateSafeArtifactIdentifier(string value, string parameterName)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(value);

        var normalized = value.Trim();
        if (normalized.Contains("..", StringComparison.Ordinal))
        {
            throw new ArgumentException("Artifact identifiers cannot include parent-directory segments.", parameterName);
        }

        if (normalized.IndexOf(Path.DirectorySeparatorChar) >= 0
            || normalized.IndexOf(Path.AltDirectorySeparatorChar) >= 0
            || normalized.IndexOf(Path.VolumeSeparatorChar) >= 0)
        {
            throw new ArgumentException("Artifact identifiers cannot include path separators.", parameterName);
        }

        if (normalized.IndexOfAny(Path.GetInvalidFileNameChars()) >= 0)
        {
            throw new ArgumentException("Artifact identifiers contain invalid file-name characters.", parameterName);
        }
    }

    private static string SanitizeForFileName(string value)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(value);

        var chars = value
            .Select(ch => char.IsLetterOrDigit(ch) || ch is '-' or '_' ? ch : '-')
            .ToArray();

        var sanitized = new string(chars).Trim('-');
        return string.IsNullOrWhiteSpace(sanitized) ? "run" : sanitized;
    }
}
