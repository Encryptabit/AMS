using System.Collections.Concurrent;
using System.Text.Json;
using Ams.Core.Common;
using Ams.Workstation.Server.Models;

namespace Ams.Workstation.Server.Services.Pickups.Edl;

/// <summary>
/// Chapter-scoped pickup artifact-ledger persistence with atomic temp+replace writes.
/// </summary>
public sealed class PickupArtifactLedgerStore
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        WriteIndented = true
    };

    private readonly Func<string> _workspaceRootResolver;
    private readonly Action<string, string, CancellationToken> _atomicWrite;
    private readonly ConcurrentDictionary<string, object> _chapterLocks =
        new(StringComparer.OrdinalIgnoreCase);

    public PickupArtifactLedgerStore(BlazorWorkspace workspace)
        : this(
            workspaceRootResolver: () => workspace.WorkingDirectory
                ?? throw new InvalidOperationException("Workspace not initialized."),
            atomicWrite: null)
    {
    }

    internal PickupArtifactLedgerStore(
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
        return Path.Combine(root, ".polish", "edl", $"{chapterStem}.artifact-ledger.json");
    }

    public PickupArtifactLedgerDocument? TryRead(string chapterStem, CancellationToken ct = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(chapterStem);

        var path = GetDocumentPath(chapterStem);
        var gate = GetChapterGate(chapterStem);

        lock (gate)
        {
            return TryReadUnlocked(path, chapterStem, ct);
        }
    }

    public PickupArtifactLedgerDocument LoadOrCreate(string chapterStem, CancellationToken ct = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(chapterStem);

        var path = GetDocumentPath(chapterStem);
        var gate = GetChapterGate(chapterStem);

        lock (gate)
        {
            return LoadOrCreateUnlocked(path, chapterStem, ct);
        }
    }

    public PickupArtifactLedgerDocument Append(
        string chapterStem,
        PickupArtifactLedgerEntryDraft entry,
        CancellationToken ct = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(chapterStem);
        ArgumentNullException.ThrowIfNull(entry);

        var path = GetDocumentPath(chapterStem);
        var gate = GetChapterGate(chapterStem);

        Exception? finalFailure = null;
        for (var attempt = 1; attempt <= 2; attempt++)
        {
            ct.ThrowIfCancellationRequested();

            try
            {
                lock (gate)
                {
                    var current = LoadOrCreateUnlocked(path, chapterStem, ct);
                    var nextEntry = entry.ToEntry(current.LastSequence + 1);
                    var next = current.Append(nextEntry);

                    var json = JsonSerializer.Serialize(next, JsonOptions);
                    _atomicWrite(path, json, ct);

                    Log.Info(
                        "Pickup artifact ledger append committed: chapter={ChapterStem}, revision={Revision}, sequence={Sequence}, transition={Transition}, op={OperationId}, rollback={RollbackVerdict}",
                        next.ChapterStem,
                        next.Revision,
                        nextEntry.Sequence,
                        nextEntry.Transition,
                        nextEntry.OperationId,
                        nextEntry.RollbackVerdict);

                    return next;
                }
            }
            catch (Exception ex) when (attempt == 1 && IsRetriableIoFailure(ex))
            {
                finalFailure = ex;
                Log.Warn(
                    "Pickup artifact ledger write retrying after {FailureType} for chapter={ChapterStem} attempt={Attempt}: {Message}",
                    ex.GetType().Name,
                    chapterStem,
                    attempt,
                    ex.Message);
            }
            catch (Exception ex)
            {
                finalFailure = ex;
                break;
            }
        }

        throw new InvalidOperationException(
            $"Pickup artifact ledger write failed after retry for chapter '{chapterStem}'.",
            finalFailure);
    }

    private PickupArtifactLedgerDocument LoadOrCreateUnlocked(
        string path,
        string chapterStem,
        CancellationToken ct)
    {
        var existing = TryReadUnlocked(path, chapterStem, ct);
        if (existing is not null)
        {
            return existing;
        }

        return new PickupArtifactLedgerDocument(
            schemaVersion: PickupArtifactLedgerDocument.CurrentSchemaVersion,
            chapterStem: chapterStem,
            revision: 0,
            lastSequence: 0,
            entries: []);
    }

    private PickupArtifactLedgerDocument? TryReadUnlocked(string path, string chapterStem, CancellationToken ct)
    {
        ct.ThrowIfCancellationRequested();

        if (!File.Exists(path))
        {
            return null;
        }

        var json = File.ReadAllText(path);
        try
        {
            var document = JsonSerializer.Deserialize<PickupArtifactLedgerDocument>(json, JsonOptions)
                ?? throw new InvalidOperationException(
                    $"Pickup artifact ledger file at '{path}' deserialized to null.");

            if (!string.Equals(document.ChapterStem, chapterStem, StringComparison.OrdinalIgnoreCase))
            {
                throw new InvalidOperationException(
                    $"Pickup artifact ledger chapter mismatch at '{path}': " +
                    $"expected='{chapterStem}', actual='{document.ChapterStem}'.");
            }

            return document;
        }
        catch (Exception ex) when (IsMalformedDocumentException(ex))
        {
            var quarantinePath = QuarantineMalformedFile(path);
            throw new InvalidOperationException(
                $"Malformed pickup artifact ledger JSON for chapter '{chapterStem}' at '{path}'. " +
                $"File moved to '{quarantinePath}'.",
                ex);
        }
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
