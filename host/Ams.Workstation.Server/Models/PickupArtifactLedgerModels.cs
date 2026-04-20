using System.Text.Json.Serialization;

namespace Ams.Workstation.Server.Models;

/// <summary>
/// Rollback verdict captured in artifact-ledger terminal/failure entries.
/// </summary>
public enum PickupArtifactLedgerRollbackVerdict
{
    NotRequired = 0,
    Succeeded = 1,
    Failed = 2,
    NotAttempted = 3
}

/// <summary>
/// Canonical lifecycle transition names for pickup commit/revert provenance entries.
/// </summary>
public static class PickupArtifactLedgerTransitions
{
    public const string CommitAttempt = "commit.attempt";
    public const string CommitSuccess = "commit.success";
    public const string CommitFailure = "commit.failure";
    public const string CommitCancelled = "commit.cancelled";
    public const string RevertAttempt = "revert.attempt";
    public const string RevertSuccess = "revert.success";
    public const string RevertFailure = "revert.failure";
    public const string RevertCancelled = "revert.cancelled";
    public const string RollbackSucceeded = "rollback.succeeded";
    public const string RollbackFailed = "rollback.failed";

    private static readonly HashSet<string> KnownValues =
    [
        CommitAttempt,
        CommitSuccess,
        CommitFailure,
        CommitCancelled,
        RevertAttempt,
        RevertSuccess,
        RevertFailure,
        RevertCancelled,
        RollbackSucceeded,
        RollbackFailed
    ];

    public static bool IsKnown(string transition)
        => !string.IsNullOrWhiteSpace(transition) && KnownValues.Contains(transition.Trim());

    public static string DescribeKnownValues()
        => string.Join(", ",
            KnownValues
                .OrderBy(value => value, StringComparer.Ordinal));
}

/// <summary>
/// Draft payload appended by runtime hooks; store assigns sequence and timestamp.
/// </summary>
public sealed record PickupArtifactLedgerEntryDraft
{
    [JsonConstructor]
    public PickupArtifactLedgerEntryDraft(
        string operationId,
        string transition,
        string phase,
        int edlRevision,
        ReplacementStatus queueStatus,
        PickupEdlOperationState edlState,
        PickupArtifactLedgerRollbackVerdict rollbackVerdict,
        IReadOnlyList<string>? artifactRefs,
        string? failureReason = null,
        DateTime? occurredAtUtc = null)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(operationId);
        ArgumentException.ThrowIfNullOrWhiteSpace(transition);
        ArgumentException.ThrowIfNullOrWhiteSpace(phase);

        if (!PickupArtifactLedgerTransitions.IsKnown(transition))
        {
            throw new ArgumentException(
                $"Pickup artifact ledger op '{operationId}' has unknown transition '{transition}'. " +
                $"Known transitions: {PickupArtifactLedgerTransitions.DescribeKnownValues()}.",
                nameof(transition));
        }

        if (edlRevision < 0)
        {
            throw new ArgumentOutOfRangeException(
                nameof(edlRevision),
                edlRevision,
                $"Pickup artifact ledger op '{operationId}' has invalid EDL revision '{edlRevision}'.");
        }

        if (!Enum.IsDefined(typeof(ReplacementStatus), queueStatus))
        {
            throw new ArgumentOutOfRangeException(
                nameof(queueStatus),
                queueStatus,
                $"Pickup artifact ledger op '{operationId}' has unknown queue status '{(int)queueStatus}'.");
        }

        if (!Enum.IsDefined(typeof(PickupEdlOperationState), edlState))
        {
            throw new ArgumentOutOfRangeException(
                nameof(edlState),
                edlState,
                $"Pickup artifact ledger op '{operationId}' has unknown EDL state '{(int)edlState}'.");
        }

        if (!Enum.IsDefined(typeof(PickupArtifactLedgerRollbackVerdict), rollbackVerdict))
        {
            throw new ArgumentOutOfRangeException(
                nameof(rollbackVerdict),
                rollbackVerdict,
                $"Pickup artifact ledger op '{operationId}' has unknown rollback verdict '{(int)rollbackVerdict}'.");
        }

        var normalizedRefs = NormalizeArtifactRefs(operationId, artifactRefs);

        OperationId = operationId.Trim();
        Transition = transition.Trim();
        Phase = phase.Trim();
        EdlRevision = edlRevision;
        QueueStatus = queueStatus;
        EdlState = edlState;
        RollbackVerdict = rollbackVerdict;
        ArtifactRefs = normalizedRefs;
        FailureReason = string.IsNullOrWhiteSpace(failureReason) ? null : failureReason.Trim();
        OccurredAtUtc = (occurredAtUtc ?? DateTime.UtcNow).ToUniversalTime();
    }

    public string OperationId { get; }

    public string Transition { get; }

    public string Phase { get; }

    public int EdlRevision { get; }

    public ReplacementStatus QueueStatus { get; }

    public PickupEdlOperationState EdlState { get; }

    public PickupArtifactLedgerRollbackVerdict RollbackVerdict { get; }

    public IReadOnlyList<string> ArtifactRefs { get; }

    public string? FailureReason { get; }

    public DateTime OccurredAtUtc { get; }

    public PickupArtifactLedgerEntry ToEntry(long sequence)
        => new(
            sequence: sequence,
            operationId: OperationId,
            transition: Transition,
            phase: Phase,
            edlRevision: EdlRevision,
            queueStatus: QueueStatus,
            edlState: EdlState,
            rollbackVerdict: RollbackVerdict,
            artifactRefs: ArtifactRefs,
            failureReason: FailureReason,
            occurredAtUtc: OccurredAtUtc);

    private static IReadOnlyList<string> NormalizeArtifactRefs(string operationId, IReadOnlyList<string>? artifactRefs)
    {
        if (artifactRefs is null || artifactRefs.Count == 0)
        {
            throw new ArgumentException(
                $"Pickup artifact ledger op '{operationId}' is missing artifact refs.",
                nameof(artifactRefs));
        }

        var normalized = artifactRefs
            .Select(reference => reference?.Trim())
            .Where(reference => !string.IsNullOrWhiteSpace(reference))
            .Select(reference => reference!.Replace('\\', '/'))
            .Distinct(StringComparer.Ordinal)
            .OrderBy(reference => reference, StringComparer.Ordinal)
            .ToArray();

        if (normalized.Length == 0)
        {
            throw new ArgumentException(
                $"Pickup artifact ledger op '{operationId}' is missing artifact refs.",
                nameof(artifactRefs));
        }

        foreach (var reference in normalized)
        {
            if (Path.IsPathRooted(reference))
            {
                throw new ArgumentException(
                    $"Pickup artifact ledger op '{operationId}' artifact ref '{reference}' must be workspace-relative.",
                    nameof(artifactRefs));
            }

            var segments = reference.Split('/', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
            if (segments.Any(segment => string.Equals(segment, "..", StringComparison.Ordinal)))
            {
                throw new ArgumentException(
                    $"Pickup artifact ledger op '{operationId}' artifact ref '{reference}' cannot escape workspace root.",
                    nameof(artifactRefs));
            }
        }

        return normalized;
    }
}

/// <summary>
/// Durable, chapter-scoped pickup artifact ledger entry.
/// </summary>
public sealed record PickupArtifactLedgerEntry
{
    [JsonConstructor]
    public PickupArtifactLedgerEntry(
        long sequence,
        string operationId,
        string transition,
        string phase,
        int edlRevision,
        ReplacementStatus queueStatus,
        PickupEdlOperationState edlState,
        PickupArtifactLedgerRollbackVerdict rollbackVerdict,
        IReadOnlyList<string>? artifactRefs,
        string? failureReason,
        DateTime occurredAtUtc)
    {
        if (sequence <= 0)
        {
            throw new ArgumentOutOfRangeException(
                nameof(sequence),
                sequence,
                "Pickup artifact ledger sequence must be greater than zero.");
        }

        var draft = new PickupArtifactLedgerEntryDraft(
            operationId: operationId,
            transition: transition,
            phase: phase,
            edlRevision: edlRevision,
            queueStatus: queueStatus,
            edlState: edlState,
            rollbackVerdict: rollbackVerdict,
            artifactRefs: artifactRefs,
            failureReason: failureReason,
            occurredAtUtc: occurredAtUtc);

        Sequence = sequence;
        OperationId = draft.OperationId;
        Transition = draft.Transition;
        Phase = draft.Phase;
        EdlRevision = draft.EdlRevision;
        QueueStatus = draft.QueueStatus;
        EdlState = draft.EdlState;
        RollbackVerdict = draft.RollbackVerdict;
        ArtifactRefs = draft.ArtifactRefs;
        FailureReason = draft.FailureReason;
        OccurredAtUtc = draft.OccurredAtUtc;
    }

    public long Sequence { get; }

    public string OperationId { get; }

    public string Transition { get; }

    public string Phase { get; }

    public int EdlRevision { get; }

    public ReplacementStatus QueueStatus { get; }

    public PickupEdlOperationState EdlState { get; }

    public PickupArtifactLedgerRollbackVerdict RollbackVerdict { get; }

    public IReadOnlyList<string> ArtifactRefs { get; }

    public string? FailureReason { get; }

    public DateTime OccurredAtUtc { get; }
}

/// <summary>
/// Chapter-scoped durable artifact ledger for pickup lifecycle provenance.
/// </summary>
public sealed record PickupArtifactLedgerDocument
{
    public const string CurrentSchemaVersion = "pickup-artifact-ledger/v1";

    [JsonConstructor]
    public PickupArtifactLedgerDocument(
        string schemaVersion,
        string chapterStem,
        int revision,
        long lastSequence,
        IReadOnlyList<PickupArtifactLedgerEntry>? entries)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(schemaVersion);
        ArgumentException.ThrowIfNullOrWhiteSpace(chapterStem);

        if (!string.Equals(schemaVersion, CurrentSchemaVersion, StringComparison.Ordinal))
        {
            throw new ArgumentException(
                $"Unsupported pickup artifact ledger schema version '{schemaVersion}'. " +
                $"Expected '{CurrentSchemaVersion}'.",
                nameof(schemaVersion));
        }

        if (revision < 0)
        {
            throw new ArgumentOutOfRangeException(
                nameof(revision),
                revision,
                $"Pickup artifact ledger revision for chapter '{chapterStem}' must be non-negative.");
        }

        if (lastSequence < 0)
        {
            throw new ArgumentOutOfRangeException(
                nameof(lastSequence),
                lastSequence,
                $"Pickup artifact ledger last sequence for chapter '{chapterStem}' must be non-negative.");
        }

        var normalizedEntries = (entries ?? Array.Empty<PickupArtifactLedgerEntry>())
            .OrderBy(entry => entry.Sequence)
            .ThenBy(entry => entry.OccurredAtUtc)
            .ThenBy(entry => entry.OperationId, StringComparer.Ordinal)
            .ToArray();

        ValidateEntries(chapterStem, lastSequence, normalizedEntries);

        SchemaVersion = schemaVersion;
        ChapterStem = chapterStem;
        Revision = revision;
        LastSequence = lastSequence;
        Entries = normalizedEntries;
    }

    public string SchemaVersion { get; }

    public string ChapterStem { get; }

    public int Revision { get; }

    public long LastSequence { get; }

    public IReadOnlyList<PickupArtifactLedgerEntry> Entries { get; }

    public IReadOnlyList<PickupArtifactLedgerEntry> GetDeterministicOrder()
        => Entries
            .OrderBy(entry => entry.Sequence)
            .ThenBy(entry => entry.OccurredAtUtc)
            .ThenBy(entry => entry.OperationId, StringComparer.Ordinal)
            .ToArray();

    public PickupArtifactLedgerDocument Append(PickupArtifactLedgerEntry entry)
    {
        ArgumentNullException.ThrowIfNull(entry);

        var expectedSequence = LastSequence + 1;
        if (entry.Sequence != expectedSequence)
        {
            throw new InvalidOperationException(
                $"Pickup artifact ledger append sequence mismatch for chapter '{ChapterStem}': " +
                $"expected='{expectedSequence}', actual='{entry.Sequence}'.");
        }

        var nextEntries = Entries.Concat([entry]).ToArray();

        return new PickupArtifactLedgerDocument(
            schemaVersion: CurrentSchemaVersion,
            chapterStem: ChapterStem,
            revision: Revision + 1,
            lastSequence: entry.Sequence,
            entries: nextEntries);
    }

    private static void ValidateEntries(
        string chapterStem,
        long lastSequence,
        IReadOnlyList<PickupArtifactLedgerEntry> entries)
    {
        if (entries.Count == 0)
        {
            if (lastSequence != 0)
            {
                throw new InvalidOperationException(
                    $"Pickup artifact ledger chapter '{chapterStem}' has lastSequence='{lastSequence}' with no entries.");
            }

            return;
        }

        long expectedSequence = 1;
        foreach (var entry in entries)
        {
            if (entry is null)
            {
                throw new InvalidOperationException(
                    $"Pickup artifact ledger chapter '{chapterStem}' contains null entry.");
            }

            if (entry.Sequence != expectedSequence)
            {
                throw new InvalidOperationException(
                    $"Pickup artifact ledger chapter '{chapterStem}' has non-deterministic sequence ordering: " +
                    $"expected='{expectedSequence}', actual='{entry.Sequence}'.");
            }

            expectedSequence++;
        }

        var observedLast = entries[^1].Sequence;
        if (observedLast != lastSequence)
        {
            throw new InvalidOperationException(
                $"Pickup artifact ledger chapter '{chapterStem}' has lastSequence mismatch: " +
                $"document='{lastSequence}', observed='{observedLast}'.");
        }
    }
}
