using System.Collections.Concurrent;
using System.Text.Json;
using Ams.Core.Common;
using Ams.Workstation.Server.Models;

namespace Ams.Workstation.Server.Services.Pickups.Fit;

/// <summary>
/// Chapter-scoped durable Fit-plan persistence with canonical Pick-map drift checks.
/// </summary>
public sealed class PickupFitPlanStore
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        WriteIndented = true
    };

    private readonly Func<string> _workspaceRootResolver;
    private readonly Action<string, string, CancellationToken> _atomicWrite;
    private readonly ConcurrentDictionary<string, object> _documentLocks =
        new(StringComparer.OrdinalIgnoreCase);

    public PickupFitPlanStore(BlazorWorkspace workspace)
        : this(
            workspaceRootResolver: () => workspace.WorkingDirectory
                ?? throw new InvalidOperationException("Workspace not initialized for pickup fit-plan store."),
            atomicWrite: null)
    {
    }

    internal PickupFitPlanStore(
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
        if (string.IsNullOrWhiteSpace(root))
        {
            throw new InvalidOperationException("Workspace root resolver returned an empty path for pickup fit-plan store.");
        }

        return Path.Combine(root, ".polish", "pickups", "fit", $"{chapterStem.Trim()}.fit-plan.json");
    }

    public PickupFitPlanDocument? TryRead(string chapterStem, CancellationToken ct = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(chapterStem);

        var path = GetDocumentPath(chapterStem);
        var gate = GetDocumentGate(path);

        lock (gate)
        {
            return TryReadUnlocked(path, chapterStem, ct);
        }
    }

    public PickupFitPlanDocument LoadOrCreate(
        string chapterStem,
        PickupPickMapDocument pickMap,
        CancellationToken ct = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(chapterStem);
        ArgumentNullException.ThrowIfNull(pickMap);

        var path = GetDocumentPath(chapterStem);
        var gate = GetDocumentGate(path);

        lock (gate)
        {
            return LoadOrCreateUnlocked(path, chapterStem, pickMap, ct);
        }
    }

    public PickupFitPlanDocument Save(
        string chapterStem,
        PickupPickMapDocument pickMap,
        PickupFitPlanDocument document,
        CancellationToken ct = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(chapterStem);
        ArgumentNullException.ThrowIfNull(pickMap);
        ArgumentNullException.ThrowIfNull(document);

        var path = GetDocumentPath(chapterStem);
        var gate = GetDocumentGate(path);

        lock (gate)
        {
            var current = LoadOrCreateUnlocked(path, chapterStem, pickMap, ct);
            var previousJson = File.Exists(path)
                ? File.ReadAllText(path)
                : null;
            var next = NormalizeForWrite(
                chapterStem: chapterStem,
                pickMap: pickMap,
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
                        "Pickup fit-plan write committed: chapter={ChapterStem}, revision={Revision}, pickRevision={PickMapRevision}, assignmentFingerprint={PickAssignmentsFingerprint}, items={ItemCount}, lastOp={LastOperationId}",
                        next.ChapterStem,
                        next.Revision,
                        next.PickMapRevision,
                        next.PickAssignmentsFingerprint,
                        next.Items.Count,
                        next.LastOperationId ?? "none");

                    return next;
                }
                catch (Exception ex) when (attempt == 1 && IsRetriableIoFailure(ex))
                {
                    finalFailure = RestoreSnapshotAfterFailedWrite(path, previousJson, chapterStem, pickMap, ex, ct);
                    Log.Warn(
                        "Pickup fit-plan write retrying after {FailureType} attempt={Attempt}, chapter={ChapterStem}, path={Path}: {Message}",
                        ex.GetType().Name,
                        attempt,
                        chapterStem,
                        path,
                        ex.Message);
                }
                catch (Exception ex)
                {
                    finalFailure = RestoreSnapshotAfterFailedWrite(path, previousJson, chapterStem, pickMap, ex, ct);
                    break;
                }
            }

            throw new InvalidOperationException(
                $"Pickup fit-plan write failed after retry for chapter '{chapterStem}' " +
                $"(pickRevision='{pickMap.Revision}', assignmentFingerprint='{PickupFitPlanDocument.ComputePickAssignmentsFingerprint(chapterStem, pickMap)}', path='{path}').",
                finalFailure);
        }
    }

    private PickupFitPlanDocument LoadOrCreateUnlocked(
        string path,
        string chapterStem,
        PickupPickMapDocument pickMap,
        CancellationToken ct)
    {
        var existing = TryReadUnlocked(path, chapterStem, ct);
        if (existing is null)
        {
            return PickupFitPlanDocument.CreateInitial(chapterStem, pickMap);
        }

        var currentAssignmentFingerprint = PickupFitPlanDocument.ComputePickAssignmentsFingerprint(chapterStem, pickMap);
        var sourceMatches = string.Equals(existing.Source.Fingerprint, pickMap.Source.Fingerprint, StringComparison.Ordinal) &&
            string.Equals(existing.Source.CrxTargetsFingerprint, pickMap.Source.CrxTargetsFingerprint, StringComparison.Ordinal);
        var assignmentsMatch = existing.PickMapRevision == pickMap.Revision &&
            string.Equals(existing.PickAssignmentsFingerprint, currentAssignmentFingerprint, StringComparison.Ordinal);

        if (!sourceMatches || !assignmentsMatch)
        {
            if (existing.Items.Count > 0)
            {
                throw new InvalidOperationException(
                    "Pickup fit-plan canonical Pick truth mismatch: " +
                    $"chapter='{chapterStem}', documentSource='{existing.Source.Fingerprint}', requestedSource='{pickMap.Source.Fingerprint}', " +
                    $"documentCrx='{existing.Source.CrxTargetsFingerprint}', requestedCrx='{pickMap.Source.CrxTargetsFingerprint}', " +
                    $"documentPickRevision='{existing.PickMapRevision}', requestedPickRevision='{pickMap.Revision}', " +
                    $"documentAssignments='{existing.PickAssignmentsFingerprint}', requestedAssignments='{currentAssignmentFingerprint}', " +
                    $"documentPath='{path}'.");
            }

            return PickupFitPlanDocument.CreateInitial(chapterStem, pickMap, existing.LastOperationId);
        }

        return existing;
    }

    private PickupFitPlanDocument? TryReadUnlocked(string path, string chapterStem, CancellationToken ct)
    {
        ct.ThrowIfCancellationRequested();

        if (!File.Exists(path))
        {
            return null;
        }

        var json = File.ReadAllText(path);
        try
        {
            var document = JsonSerializer.Deserialize<PickupFitPlanDocument>(json, JsonOptions)
                ?? throw new InvalidOperationException(
                    $"Pickup fit-plan file at '{path}' deserialized to null.");

            if (!string.Equals(document.ChapterStem, chapterStem, StringComparison.OrdinalIgnoreCase))
            {
                throw new InvalidOperationException(
                    $"Pickup fit-plan chapter mismatch at '{path}': " +
                    $"expected='{chapterStem}', actual='{document.ChapterStem}'.");
            }

            return document;
        }
        catch (Exception ex) when (IsMalformedDocumentException(ex))
        {
            var quarantinePath = QuarantineMalformedFile(path);
            throw new InvalidOperationException(
                $"Malformed pickup fit-plan JSON for chapter '{chapterStem}' at '{path}'. " +
                $"File moved to '{quarantinePath}'.",
                ex);
        }
    }

    private static PickupFitPlanDocument NormalizeForWrite(
        string chapterStem,
        PickupPickMapDocument pickMap,
        int nextRevision,
        PickupFitPlanDocument document)
    {
        if (!string.Equals(document.ChapterStem, chapterStem, StringComparison.OrdinalIgnoreCase))
        {
            throw new InvalidOperationException(
                $"Pickup fit-plan save produced chapter mismatch: expected='{chapterStem}', actual='{document.ChapterStem}'.");
        }

        if (!string.Equals(document.Source.Fingerprint, pickMap.Source.Fingerprint, StringComparison.Ordinal))
        {
            throw new InvalidOperationException(
                "Pickup fit-plan save changed source fingerprint: " +
                $"expected='{pickMap.Source.Fingerprint}', actual='{document.Source.Fingerprint}', sourcePath='{pickMap.Source.Path}'.");
        }

        if (!string.Equals(document.Source.CrxTargetsFingerprint, pickMap.Source.CrxTargetsFingerprint, StringComparison.Ordinal))
        {
            throw new InvalidOperationException(
                "Pickup fit-plan save changed CRX target fingerprint: " +
                $"expected='{pickMap.Source.CrxTargetsFingerprint}', actual='{document.Source.CrxTargetsFingerprint}', sourcePath='{pickMap.Source.Path}'.");
        }

        var expectedAssignmentFingerprint = PickupFitPlanDocument.ComputePickAssignmentsFingerprint(chapterStem, pickMap);
        if (document.PickMapRevision != pickMap.Revision ||
            !string.Equals(document.PickAssignmentsFingerprint, expectedAssignmentFingerprint, StringComparison.Ordinal))
        {
            throw new InvalidOperationException(
                "Pickup fit-plan save used stale Pick assignment truth: " +
                $"expectedRevision='{pickMap.Revision}', actualRevision='{document.PickMapRevision}', " +
                $"expectedAssignments='{expectedAssignmentFingerprint}', actualAssignments='{document.PickAssignmentsFingerprint}'.");
        }

        ValidateCanonicalAssignmentAffinity(chapterStem, pickMap, document);

        return new PickupFitPlanDocument(
            schemaVersion: PickupFitPlanDocument.CurrentSchemaVersion,
            chapterStem: chapterStem,
            revision: nextRevision,
            source: pickMap.Source,
            pickMapRevision: pickMap.Revision,
            pickAssignmentsFingerprint: expectedAssignmentFingerprint,
            items: document.GetDeterministicItemOrder(),
            createdAtUtc: document.CreatedAtUtc,
            updatedAtUtc: DateTime.UtcNow,
            lastOperationId: document.LastOperationId,
            lastValidationError: document.LastValidationError,
            isDraft: document.IsDraft);
    }

    private static void ValidateCanonicalAssignmentAffinity(
        string chapterStem,
        PickupPickMapDocument pickMap,
        PickupFitPlanDocument document)
    {
        var canonicalAssignments = PickupFitPlanDocument.GetCanonicalAssignments(chapterStem, pickMap)
            .ToDictionary(assignment => assignment.Id, StringComparer.Ordinal);

        if (document.Items.Count == 0)
        {
            if (!document.IsDraft)
            {
                throw new InvalidOperationException(
                    $"Pickup fit-plan for chapter '{chapterStem}' has no items but is not draft.");
            }

            return;
        }

        if (document.Items.Count != canonicalAssignments.Count)
        {
            throw new InvalidOperationException(
                $"Pickup fit-plan for chapter '{chapterStem}' item count does not match canonical Pick assignments: " +
                $"document='{document.Items.Count}', pick='{canonicalAssignments.Count}'.");
        }

        foreach (var item in document.Items)
        {
            if (!canonicalAssignments.TryGetValue(item.PickAssignmentId, out var assignment))
            {
                throw new InvalidOperationException(
                    $"Pickup fit-plan item '{item.FitItemId}' references unknown Pick assignment '{item.PickAssignmentId}' " +
                    $"for chapter '{chapterStem}'.");
            }

            if (!string.Equals(item.PickupSegmentId, assignment.PickupSegmentId, StringComparison.Ordinal))
            {
                throw new InvalidOperationException(
                    $"Pickup fit-plan item '{item.FitItemId}' pickup segment mismatch: " +
                    $"document='{item.PickupSegmentId}', pick='{assignment.PickupSegmentId}'.");
            }

            var target = assignment.EffectiveTarget
                ?? throw new InvalidOperationException(
                    $"Canonical Pick assignment '{assignment.Id}' for chapter '{chapterStem}' has no target.");

            if (!string.Equals(item.Target.TargetKey, target.TargetKey, StringComparison.OrdinalIgnoreCase))
            {
                throw new InvalidOperationException(
                    $"Pickup fit-plan item '{item.FitItemId}' target mismatch: " +
                    $"document='{item.Target.TargetKey}', pick='{target.TargetKey}'.");
            }
        }
    }

    private static bool IsRetriableIoFailure(Exception ex)
        => ex is IOException || ex is TimeoutException;

    private static bool IsMalformedDocumentException(Exception ex)
        => ex is JsonException || ex is ArgumentException || ex is InvalidOperationException;

    private static Exception RestoreSnapshotAfterFailedWrite(
        string path,
        string? previousJson,
        string chapterStem,
        PickupPickMapDocument pickMap,
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
                ?? throw new InvalidOperationException($"Cannot resolve directory for pickup fit-plan file '{path}'.");
            Directory.CreateDirectory(directory);
            File.WriteAllText(path, previousJson);
            return writeFailure;
        }
        catch (Exception restoreFailure)
        {
            return new InvalidOperationException(
                "Pickup fit-plan write failed and last-good document restoration failed: " +
                $"chapter='{chapterStem}', path='{path}', sourcePath='{pickMap.Source.Path}', " +
                $"sourceFingerprint='{pickMap.Source.Fingerprint}', pickRevision='{pickMap.Revision}', " +
                $"writeFailure='{writeFailure.Message}', restoreFailure='{restoreFailure.Message}'.",
                new AggregateException(writeFailure, restoreFailure));
        }
    }

    private static void WriteAtomic(string path, string json, CancellationToken ct)
    {
        ct.ThrowIfCancellationRequested();

        var directory = Path.GetDirectoryName(path)
            ?? throw new InvalidOperationException($"Cannot resolve directory for pickup fit-plan file '{path}'.");
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
            ?? throw new InvalidOperationException($"Cannot resolve directory for malformed pickup fit-plan file '{path}'.");
        var fileName = Path.GetFileName(path);
        var quarantinePath = Path.Combine(
            directory,
            $"{fileName}.malformed.{DateTime.UtcNow:yyyyMMddHHmmssfff}.json");

        File.Move(path, quarantinePath, overwrite: false);
        return quarantinePath;
    }

    private object GetDocumentGate(string path)
        => _documentLocks.GetOrAdd(path, static _ => new object());

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
