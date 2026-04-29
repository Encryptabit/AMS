using System.Collections.Concurrent;
using System.Text.Json;
using Ams.Core.Common;
using Ams.Workstation.Server.Models;

namespace Ams.Workstation.Server.Services.Pickups.Pick;

/// <summary>
/// Batch-scoped durable Pick map persistence with atomic temp+replace writes.
/// </summary>
public sealed class PickupPickMapStore
{
    private const string DocumentFileName = "pick-map.json";

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        WriteIndented = true
    };

    private readonly Func<string> _workspaceRootResolver;
    private readonly Action<string, string, CancellationToken> _atomicWrite;
    private readonly ConcurrentDictionary<string, object> _mapLocks =
        new(StringComparer.OrdinalIgnoreCase);

    public PickupPickMapStore(BlazorWorkspace workspace)
        : this(
            workspaceRootResolver: () => workspace.WorkingDirectory
                ?? throw new InvalidOperationException("Workspace not initialized for pickup pick-map store."),
            atomicWrite: null)
    {
    }

    internal PickupPickMapStore(
        Func<string> workspaceRootResolver,
        Action<string, string, CancellationToken>? atomicWrite = null)
    {
        _workspaceRootResolver = workspaceRootResolver
            ?? throw new ArgumentNullException(nameof(workspaceRootResolver));
        _atomicWrite = atomicWrite ?? WriteAtomic;
    }

    public string GetDocumentPath()
    {
        var root = _workspaceRootResolver();
        if (string.IsNullOrWhiteSpace(root))
        {
            throw new InvalidOperationException("Workspace root resolver returned an empty path for pickup pick-map store.");
        }

        return Path.Combine(root, ".polish", "pickups", DocumentFileName);
    }

    public PickupPickMapDocument? TryRead(CancellationToken ct = default)
    {
        var path = GetDocumentPath();
        var gate = GetMapGate(path);

        lock (gate)
        {
            return TryReadUnlocked(path, ct);
        }
    }

    public PickupPickMapDocument LoadOrCreate(
        PickupPickMapSourceReference source,
        CancellationToken ct = default)
    {
        ArgumentNullException.ThrowIfNull(source);

        var path = GetDocumentPath();
        var gate = GetMapGate(path);

        lock (gate)
        {
            return LoadOrCreateUnlocked(path, source, ct);
        }
    }

    public PickupPickMapDocument Save(
        PickupPickMapSourceReference source,
        PickupPickMapDocument document,
        CancellationToken ct = default)
    {
        ArgumentNullException.ThrowIfNull(source);
        ArgumentNullException.ThrowIfNull(document);

        var path = GetDocumentPath();
        var gate = GetMapGate(path);

        lock (gate)
        {
            var current = LoadOrCreateUnlocked(path, source, ct);
            var previousJson = File.Exists(path)
                ? File.ReadAllText(path)
                : null;
            var next = NormalizeForWrite(
                source: source,
                nextRevision: current.Revision + 1,
                document: document);
            var json = JsonSerializer.Serialize(next, JsonOptions);

            Exception? finalFailure = null;
            for (var attempt = 1; attempt <= 2; attempt++)
            {
                ct.ThrowIfCancellationRequested();

                try
                {
                    _atomicWrite(path, json, ct);

                    Log.Info(
                        "Pickup pick-map write committed: revision={Revision}, source={SourceFingerprint}, crx={CrxTargetsFingerprint}, assignments={AssignmentCount}",
                        next.Revision,
                        next.Source.Fingerprint,
                        next.Source.CrxTargetsFingerprint,
                        next.Assignments.Count);

                    return next;
                }
                catch (Exception ex) when (attempt == 1 && IsRetriableIoFailure(ex))
                {
                    finalFailure = RestoreSnapshotAfterFailedWrite(path, previousJson, source, ex, ct);
                    Log.Warn(
                        "Pickup pick-map write retrying after {FailureType} attempt={Attempt}, path={Path}, source={SourceFingerprint}: {Message}",
                        ex.GetType().Name,
                        attempt,
                        path,
                        source.Fingerprint,
                        ex.Message);
                }
                catch (Exception ex)
                {
                    finalFailure = RestoreSnapshotAfterFailedWrite(path, previousJson, source, ex, ct);
                    break;
                }
            }

            throw new InvalidOperationException(
                $"Pickup pick-map write failed after retry for source '{source.Path}' " +
                $"(fingerprint='{source.Fingerprint}', crxTargets='{source.CrxTargetsFingerprint}', path='{path}').",
                finalFailure);
        }
    }

    private PickupPickMapDocument LoadOrCreateUnlocked(
        string path,
        PickupPickMapSourceReference source,
        CancellationToken ct)
    {
        var existing = TryReadUnlocked(path, ct);
        if (existing is null)
        {
            return CreateDraft(source);
        }

        if (!string.Equals(existing.Source.Fingerprint, source.Fingerprint, StringComparison.Ordinal) ||
            !string.Equals(existing.Source.CrxTargetsFingerprint, source.CrxTargetsFingerprint, StringComparison.Ordinal))
        {
            if (existing.Assignments.Count > 0)
            {
                throw new InvalidOperationException(
                    "Pickup pick-map source fingerprint mismatch: " +
                    $"documentSource='{existing.Source.Fingerprint}', requestedSource='{source.Fingerprint}', " +
                    $"documentCrx='{existing.Source.CrxTargetsFingerprint}', requestedCrx='{source.CrxTargetsFingerprint}', " +
                    $"documentPath='{path}', sourcePath='{existing.Source.Path}'.");
            }

            return new PickupPickMapDocument(
                schemaVersion: PickupPickMapDocument.CurrentSchemaVersion,
                revision: existing.Revision,
                source: source,
                assignments: [],
                createdAtUtc: existing.CreatedAtUtc,
                updatedAtUtc: DateTime.UtcNow,
                lastOperationId: existing.LastOperationId,
                lastValidationError: null,
                isDraft: true);
        }

        return existing;
    }

    private static PickupPickMapDocument CreateDraft(PickupPickMapSourceReference source)
    {
        var now = DateTime.UtcNow;
        return new PickupPickMapDocument(
            schemaVersion: PickupPickMapDocument.CurrentSchemaVersion,
            revision: 0,
            source: source,
            assignments: [],
            createdAtUtc: now,
            updatedAtUtc: now,
            lastOperationId: null,
            lastValidationError: null,
            isDraft: true);
    }

    private PickupPickMapDocument? TryReadUnlocked(string path, CancellationToken ct)
    {
        ct.ThrowIfCancellationRequested();

        if (!File.Exists(path))
        {
            return null;
        }

        var json = File.ReadAllText(path);
        try
        {
            return JsonSerializer.Deserialize<PickupPickMapDocument>(json, JsonOptions)
                ?? throw new InvalidOperationException(
                    $"Pickup pick-map file at '{path}' deserialized to null.");
        }
        catch (Exception ex) when (IsMalformedDocumentException(ex))
        {
            var quarantinePath = QuarantineMalformedFile(path);
            throw new InvalidOperationException(
                $"Malformed pickup pick-map JSON at '{path}'. File moved to '{quarantinePath}'.",
                ex);
        }
    }

    private static PickupPickMapDocument NormalizeForWrite(
        PickupPickMapSourceReference source,
        int nextRevision,
        PickupPickMapDocument document)
    {
        if (!string.Equals(document.Source.Fingerprint, source.Fingerprint, StringComparison.Ordinal))
        {
            throw new InvalidOperationException(
                "Pickup pick-map save changed source fingerprint: " +
                $"expected='{source.Fingerprint}', actual='{document.Source.Fingerprint}', sourcePath='{source.Path}'.");
        }

        if (!string.Equals(document.Source.CrxTargetsFingerprint, source.CrxTargetsFingerprint, StringComparison.Ordinal))
        {
            throw new InvalidOperationException(
                "Pickup pick-map save changed CRX target fingerprint: " +
                $"expected='{source.CrxTargetsFingerprint}', actual='{document.Source.CrxTargetsFingerprint}', sourcePath='{source.Path}'.");
        }

        return new PickupPickMapDocument(
            schemaVersion: PickupPickMapDocument.CurrentSchemaVersion,
            revision: nextRevision,
            source: source,
            assignments: document.GetDeterministicAssignmentOrder(),
            createdAtUtc: document.CreatedAtUtc,
            updatedAtUtc: DateTime.UtcNow,
            lastOperationId: document.LastOperationId,
            lastValidationError: document.LastValidationError,
            isDraft: document.IsDraft);
    }

    private static bool IsRetriableIoFailure(Exception ex)
        => ex is IOException || ex is TimeoutException;

    private static bool IsMalformedDocumentException(Exception ex)
        => ex is JsonException || ex is ArgumentException || ex is InvalidOperationException;

    private static Exception RestoreSnapshotAfterFailedWrite(
        string path,
        string? previousJson,
        PickupPickMapSourceReference source,
        Exception writeFailure,
        CancellationToken ct)
    {
        try
        {
            ct.ThrowIfCancellationRequested();

            if (previousJson is null)
            {
                if (File.Exists(path))
                {
                    File.Delete(path);
                }

                return writeFailure;
            }

            var currentJson = File.Exists(path)
                ? File.ReadAllText(path)
                : null;
            if (string.Equals(currentJson, previousJson, StringComparison.Ordinal))
            {
                return writeFailure;
            }

            var directory = Path.GetDirectoryName(path)
                ?? throw new InvalidOperationException($"Cannot resolve directory for pickup pick-map file '{path}'.");
            Directory.CreateDirectory(directory);
            File.WriteAllText(path, previousJson);
            return writeFailure;
        }
        catch (Exception restoreFailure)
        {
            return new InvalidOperationException(
                "Pickup pick-map write failed and last-good document restoration failed: " +
                $"path='{path}', sourcePath='{source.Path}', fingerprint='{source.Fingerprint}', " +
                $"writeFailure='{writeFailure.Message}', restoreFailure='{restoreFailure.Message}'.",
                new AggregateException(writeFailure, restoreFailure));
        }
    }

    private static void WriteAtomic(string path, string json, CancellationToken ct)
    {
        ct.ThrowIfCancellationRequested();

        var directory = Path.GetDirectoryName(path)
            ?? throw new InvalidOperationException($"Cannot resolve directory for pickup pick-map file '{path}'.");
        Directory.CreateDirectory(directory);

        var tempPath = Path.Combine(directory, $"{Path.GetFileName(path)}.{Guid.NewGuid():N}.tmp");
        File.WriteAllText(tempPath, json);

        try
        {
            if (File.Exists(path))
            {
                File.Replace(tempPath, path, destinationBackupFileName: null, ignoreMetadataErrors: true);
            }
            else
            {
                File.Move(tempPath, path);
            }
        }
        catch
        {
            TryDeleteTemp(tempPath);
            throw;
        }
    }

    private static string QuarantineMalformedFile(string path)
    {
        var directory = Path.GetDirectoryName(path)
            ?? throw new InvalidOperationException($"Cannot resolve directory for malformed pickup pick-map file '{path}'.");
        var fileName = Path.GetFileName(path);
        var quarantinePath = Path.Combine(
            directory,
            $"{fileName}.malformed.{DateTime.UtcNow:yyyyMMddHHmmssfff}.json");

        File.Move(path, quarantinePath, overwrite: false);
        return quarantinePath;
    }

    private object GetMapGate(string path)
        => _mapLocks.GetOrAdd(path, static _ => new object());

    private static void TryDeleteTemp(string tempPath)
    {
        try
        {
            if (File.Exists(tempPath))
            {
                File.Delete(tempPath);
            }
        }
        catch
        {
            // best-effort cleanup only
        }
    }
}
