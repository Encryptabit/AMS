using System.Collections.Concurrent;
using System.Text.Json;
using Ams.Core.Common;
using Ams.Workstation.Server.Models;

namespace Ams.Workstation.Server.Services.Pickups.Edl;

/// <summary>
/// Chapter-scoped pickup EDL persistence with atomic temp+replace writes.
/// </summary>
public sealed class PickupEdlStore
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        WriteIndented = true
    };

    private readonly Func<string> _workspaceRootResolver;
    private readonly Action<string, string, CancellationToken> _atomicWrite;
    private readonly ConcurrentDictionary<string, object> _chapterLocks =
        new(StringComparer.OrdinalIgnoreCase);

    public PickupEdlStore(BlazorWorkspace workspace)
        : this(
            workspaceRootResolver: () => workspace.WorkingDirectory
                ?? throw new InvalidOperationException("Workspace not initialized."),
            atomicWrite: null)
    {
    }

    internal PickupEdlStore(
        Func<string> workspaceRootResolver,
        Action<string, string, CancellationToken>? atomicWrite = null)
    {
        _workspaceRootResolver = workspaceRootResolver
            ?? throw new ArgumentNullException(nameof(workspaceRootResolver));
        _atomicWrite = atomicWrite ?? WriteAtomic;
    }

    public string GetDocumentPath(string chapterStem)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(chapterStem);

        var root = _workspaceRootResolver();
        return Path.Combine(root, ".polish", "edl", $"{chapterStem}.edl.json");
    }

    public PickupEdlDocument? TryRead(string chapterStem, CancellationToken ct = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(chapterStem);

        var path = GetDocumentPath(chapterStem);
        var gate = GetChapterGate(chapterStem);

        lock (gate)
        {
            return TryReadUnlocked(path, chapterStem, ct);
        }
    }

    public PickupEdlDocument LoadOrCreate(
        string chapterStem,
        PickupEdlSourceReference source,
        CancellationToken ct = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(chapterStem);
        ArgumentNullException.ThrowIfNull(source);

        var path = GetDocumentPath(chapterStem);
        var gate = GetChapterGate(chapterStem);

        lock (gate)
        {
            return LoadOrCreateUnlocked(path, chapterStem, source, ct);
        }
    }

    public PickupEdlDocument Mutate(
        string chapterStem,
        PickupEdlSourceReference source,
        Func<PickupEdlDocument, PickupEdlDocument> mutation,
        CancellationToken ct = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(chapterStem);
        ArgumentNullException.ThrowIfNull(source);
        ArgumentNullException.ThrowIfNull(mutation);

        var path = GetDocumentPath(chapterStem);
        var gate = GetChapterGate(chapterStem);

        for (var attempt = 1; attempt <= 2; attempt++)
        {
            ct.ThrowIfCancellationRequested();

            try
            {
                lock (gate)
                {
                    var current = LoadOrCreateUnlocked(path, chapterStem, source, ct);
                    var mutated = mutation(current)
                        ?? throw new InvalidOperationException(
                            $"Pickup EDL mutation returned null for chapter '{chapterStem}'.");

                    var next = NormalizeForWrite(
                        chapterStem: chapterStem,
                        source: current.Source,
                        nextRevision: current.Revision + 1,
                        mutated: mutated);

                    var json = JsonSerializer.Serialize(next, JsonOptions);
                    _atomicWrite(path, json, ct);

                    Log.Debug(
                        "Pickup EDL write committed: chapter={ChapterStem}, revision={Revision}, opCount={OperationCount}",
                        next.ChapterStem,
                        next.Revision,
                        next.Operations.Count);

                    return next;
                }
            }
            catch (Exception ex) when (attempt == 1 && IsRetriableIoFailure(ex))
            {
                Log.Warn(
                    "Pickup EDL write retrying after {FailureType} for chapter={ChapterStem} attempt={Attempt}: {Message}",
                    ex.GetType().Name,
                    chapterStem,
                    attempt,
                    ex.Message);
            }
        }

        throw new InvalidOperationException(
            $"Pickup EDL write failed after retry for chapter '{chapterStem}'.");
    }

    private PickupEdlDocument LoadOrCreateUnlocked(
        string path,
        string chapterStem,
        PickupEdlSourceReference source,
        CancellationToken ct)
    {
        var existing = TryReadUnlocked(path, chapterStem, ct);
        if (existing is null)
        {
            return new PickupEdlDocument(
                schemaVersion: PickupEdlDocument.CurrentSchemaVersion,
                chapterStem: chapterStem,
                revision: 0,
                source: source,
                operations: []);
        }

        if (!string.Equals(existing.Source.Fingerprint, source.Fingerprint, StringComparison.Ordinal))
        {
            if (existing.Operations.Count > 0)
            {
                throw new InvalidOperationException(
                    $"Pickup EDL source fingerprint mismatch for chapter '{chapterStem}': " +
                    $"document='{existing.Source.Fingerprint}', requested='{source.Fingerprint}', " +
                    $"path='{existing.Source.Path}'.");
            }

            return new PickupEdlDocument(
                schemaVersion: existing.SchemaVersion,
                chapterStem: existing.ChapterStem,
                revision: existing.Revision,
                source: source,
                operations: []);
        }

        return existing;
    }

    private PickupEdlDocument? TryReadUnlocked(string path, string chapterStem, CancellationToken ct)
    {
        ct.ThrowIfCancellationRequested();

        if (!File.Exists(path))
        {
            return null;
        }

        var json = File.ReadAllText(path);
        try
        {
            var document = JsonSerializer.Deserialize<PickupEdlDocument>(json, JsonOptions)
                ?? throw new InvalidOperationException(
                    $"Pickup EDL file at '{path}' deserialized to null.");

            if (!string.Equals(document.ChapterStem, chapterStem, StringComparison.OrdinalIgnoreCase))
            {
                throw new InvalidOperationException(
                    $"Pickup EDL chapter mismatch at '{path}': " +
                    $"expected='{chapterStem}', actual='{document.ChapterStem}'.");
            }

            return document;
        }
        catch (Exception ex) when (IsMalformedDocumentException(ex))
        {
            var quarantinePath = QuarantineMalformedFile(path);
            throw new InvalidOperationException(
                $"Malformed pickup EDL JSON for chapter '{chapterStem}' at '{path}'. " +
                $"File moved to '{quarantinePath}'.",
                ex);
        }
    }

    private static PickupEdlDocument NormalizeForWrite(
        string chapterStem,
        PickupEdlSourceReference source,
        int nextRevision,
        PickupEdlDocument mutated)
    {
        if (!string.Equals(mutated.ChapterStem, chapterStem, StringComparison.OrdinalIgnoreCase))
        {
            throw new InvalidOperationException(
                $"Pickup EDL mutation produced chapter mismatch: expected='{chapterStem}', actual='{mutated.ChapterStem}'.");
        }

        if (!string.Equals(mutated.Source.Fingerprint, source.Fingerprint, StringComparison.Ordinal))
        {
            throw new InvalidOperationException(
                $"Pickup EDL mutation changed source fingerprint for chapter '{chapterStem}': " +
                $"expected='{source.Fingerprint}', actual='{mutated.Source.Fingerprint}'.");
        }

        return new PickupEdlDocument(
            schemaVersion: PickupEdlDocument.CurrentSchemaVersion,
            chapterStem: chapterStem,
            revision: nextRevision,
            source: source,
            operations: mutated.Operations);
    }

    private static bool IsRetriableIoFailure(Exception ex)
        => ex is IOException || ex is TimeoutException;

    private static bool IsMalformedDocumentException(Exception ex)
        => ex is JsonException || ex is ArgumentException || ex is InvalidOperationException;

    private static void WriteAtomic(string path, string json, CancellationToken ct)
    {
        ct.ThrowIfCancellationRequested();

        var directory = Path.GetDirectoryName(path)
            ?? throw new InvalidOperationException($"Cannot resolve directory for '{path}'.");
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
            ?? throw new InvalidOperationException($"Cannot resolve directory for malformed file '{path}'.");
        var fileName = Path.GetFileName(path);
        var quarantinePath = Path.Combine(
            directory,
            $"{fileName}.malformed.{DateTime.UtcNow:yyyyMMddHHmmssfff}.json");

        File.Move(path, quarantinePath, overwrite: false);
        return quarantinePath;
    }

    private object GetChapterGate(string chapterStem)
        => _chapterLocks.GetOrAdd(chapterStem, static _ => new object());

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
