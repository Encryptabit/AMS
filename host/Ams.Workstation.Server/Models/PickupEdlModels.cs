using System.Text.Json.Serialization;
using Ams.Core.Audio;

namespace Ams.Workstation.Server.Models;

/// <summary>
/// Type of pickup EDL operation. T01 locks only replacement semantics.
/// </summary>
public enum PickupEdlOperationType
{
    PickupReplace = 0
}

/// <summary>
/// Lifecycle state for pickup EDL operations.
/// </summary>
public enum PickupEdlOperationState
{
    Staged = 0,
    Applied = 1,
    Reverted = 2,
    Failed = 3
}

/// <summary>
/// Source-file metadata used to validate operation/source affinity.
/// </summary>
public sealed record PickupEdlSourceReference
{
    [JsonConstructor]
    public PickupEdlSourceReference(
        string path,
        string fingerprint,
        long fileSizeBytes,
        DateTime modifiedAtUtc)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(path);
        ArgumentException.ThrowIfNullOrWhiteSpace(fingerprint);

        if (fileSizeBytes < 0)
        {
            throw new ArgumentOutOfRangeException(
                nameof(fileSizeBytes),
                fileSizeBytes,
                "Source file size must be non-negative.");
        }

        Path = path;
        Fingerprint = fingerprint;
        FileSizeBytes = fileSizeBytes;
        ModifiedAtUtc = modifiedAtUtc;
    }

    public string Path { get; }

    public string Fingerprint { get; }

    public long FileSizeBytes { get; }

    public DateTime ModifiedAtUtc { get; }
}

/// <summary>
/// Immutable pickup edit operation stored inside a chapter-scoped EDL document.
/// </summary>
public sealed record PickupEdlOperation
{
    private const double RangeEpsilonSec = 0.000_001;

    [JsonConstructor]
    public PickupEdlOperation(
        string id,
        string chapterStem,
        PickupEdlOperationType kind,
        PickupEdlOperationState state,
        double baselineStartSec,
        double baselineEndSec,
        double sourceStartSec,
        double sourceEndSec,
        string sourceFingerprint,
        int? sentenceId,
        int? errorNumber,
        string? pickupAssetId,
        double crossfadeDurationSec,
        string crossfadeCurve,
        DateTime updatedAtUtc)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(id);
        ArgumentException.ThrowIfNullOrWhiteSpace(chapterStem);
        ArgumentException.ThrowIfNullOrWhiteSpace(sourceFingerprint);
        ArgumentException.ThrowIfNullOrWhiteSpace(crossfadeCurve);

        if (!Enum.IsDefined(typeof(PickupEdlOperationType), kind))
        {
            throw new ArgumentOutOfRangeException(
                nameof(kind),
                kind,
                $"Pickup EDL op '{id}' for chapter '{chapterStem}' has unknown kind value '{(int)kind}'.");
        }

        if (!Enum.IsDefined(typeof(PickupEdlOperationState), state))
        {
            throw new ArgumentOutOfRangeException(
                nameof(state),
                state,
                $"Pickup EDL op '{id}' for chapter '{chapterStem}' has unknown state value '{(int)state}'.");
        }

        EnsureRange(
            rangeName: "baseline",
            startSec: baselineStartSec,
            endSec: baselineEndSec,
            opId: id,
            chapterStem: chapterStem);

        EnsureRange(
            rangeName: "source",
            startSec: sourceStartSec,
            endSec: sourceEndSec,
            opId: id,
            chapterStem: chapterStem);

        if (double.IsNaN(crossfadeDurationSec) ||
            double.IsInfinity(crossfadeDurationSec) ||
            crossfadeDurationSec < 0)
        {
            throw new ArgumentOutOfRangeException(
                nameof(crossfadeDurationSec),
                crossfadeDurationSec,
                $"Pickup EDL op '{id}' for chapter '{chapterStem}' has invalid crossfade duration '{crossfadeDurationSec}'.");
        }

        Id = id;
        ChapterStem = chapterStem;
        Kind = kind;
        State = state;
        BaselineStartSec = baselineStartSec;
        BaselineEndSec = baselineEndSec;
        SourceStartSec = sourceStartSec;
        SourceEndSec = sourceEndSec;
        SourceFingerprint = sourceFingerprint;
        SentenceId = sentenceId;
        ErrorNumber = errorNumber;
        PickupAssetId = pickupAssetId;
        CrossfadeDurationSec = crossfadeDurationSec;
        CrossfadeCurve = crossfadeCurve;
        UpdatedAtUtc = updatedAtUtc;
    }

    public string Id { get; }

    public string ChapterStem { get; }

    public PickupEdlOperationType Kind { get; }

    public PickupEdlOperationState State { get; }

    public double BaselineStartSec { get; }

    public double BaselineEndSec { get; }

    public double SourceStartSec { get; }

    public double SourceEndSec { get; }

    public string SourceFingerprint { get; }

    public int? SentenceId { get; }

    public int? ErrorNumber { get; }

    public string? PickupAssetId { get; }

    public double CrossfadeDurationSec { get; }

    public string CrossfadeCurve { get; }

    public DateTime UpdatedAtUtc { get; }

    [JsonIgnore]
    public bool IsActive => State is PickupEdlOperationState.Staged or PickupEdlOperationState.Applied;

    [JsonIgnore]
    public double ReplacementDurationSec => SourceEndSec - SourceStartSec;

    public ChapterEdit ToChapterEdit()
    {
        var editOperation = Kind switch
        {
            PickupEdlOperationType.PickupReplace => EditOperation.PickupReplace,
            _ => throw new InvalidOperationException(
                $"Pickup EDL op '{Id}' for chapter '{ChapterStem}' cannot map unknown kind '{Kind}'.")
        };

        return new ChapterEdit(
            Id: Id,
            ChapterStem: ChapterStem,
            Operation: editOperation,
            BaselineStartSec: BaselineStartSec,
            BaselineEndSec: BaselineEndSec,
            ReplacementDurationSec: ReplacementDurationSec,
            SentenceId: SentenceId,
            ErrorNumber: ErrorNumber,
            PickupAssetId: PickupAssetId,
            CrossfadeDurationSec: CrossfadeDurationSec,
            CrossfadeCurve: CrossfadeCurve,
            AppliedAtUtc: UpdatedAtUtc);
    }

    private static void EnsureRange(
        string rangeName,
        double startSec,
        double endSec,
        string opId,
        string chapterStem)
    {
        if (double.IsNaN(startSec) || double.IsInfinity(startSec) ||
            double.IsNaN(endSec) || double.IsInfinity(endSec))
        {
            throw new ArgumentOutOfRangeException(
                rangeName,
                $"Pickup EDL op '{opId}' for chapter '{chapterStem}' has non-finite {rangeName} range.");
        }

        if (startSec < 0)
        {
            throw new ArgumentOutOfRangeException(
                rangeName,
                startSec,
                $"Pickup EDL op '{opId}' for chapter '{chapterStem}' has negative {rangeName} start '{startSec}'.");
        }

        if (endSec <= startSec + RangeEpsilonSec)
        {
            throw new ArgumentOutOfRangeException(
                rangeName,
                $"Pickup EDL op '{opId}' for chapter '{chapterStem}' has invalid {rangeName} range " +
                $"[{startSec:F6}, {endSec:F6}] (end must be greater than start). ");
        }
    }
}

/// <summary>
/// Chapter-scoped pickup EDL document contract.
/// </summary>
public sealed record PickupEdlDocument
{
    private const double OverlapEpsilonSec = 0.001;

    /// <summary>
    /// Stable schema marker for migration-aware loading.
    /// </summary>
    public const string CurrentSchemaVersion = "pickup-edl/v1";

    [JsonConstructor]
    public PickupEdlDocument(
        string schemaVersion,
        string chapterStem,
        int revision,
        PickupEdlSourceReference source,
        IReadOnlyList<PickupEdlOperation>? operations)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(schemaVersion);
        ArgumentException.ThrowIfNullOrWhiteSpace(chapterStem);
        ArgumentNullException.ThrowIfNull(source);

        if (!string.Equals(schemaVersion, CurrentSchemaVersion, StringComparison.Ordinal))
        {
            throw new ArgumentException(
                $"Unsupported pickup EDL schema version '{schemaVersion}'. Expected '{CurrentSchemaVersion}'.",
                nameof(schemaVersion));
        }

        if (revision < 0)
        {
            throw new ArgumentOutOfRangeException(
                nameof(revision),
                revision,
                $"Pickup EDL revision for chapter '{chapterStem}' must be non-negative.");
        }

        var normalizedOps = operations?.ToArray() ?? [];
        ValidateOperations(
            chapterStem: chapterStem,
            documentFingerprint: source.Fingerprint,
            operations: normalizedOps);

        SchemaVersion = schemaVersion;
        ChapterStem = chapterStem;
        Revision = revision;
        Source = source;
        Operations = normalizedOps;
    }

    public string SchemaVersion { get; }

    public string ChapterStem { get; }

    public int Revision { get; }

    public PickupEdlSourceReference Source { get; }

    public IReadOnlyList<PickupEdlOperation> Operations { get; }

    /// <summary>
    /// Deterministic order of all operations for diagnostics and snapshot diffs.
    /// </summary>
    public IReadOnlyList<PickupEdlOperation> GetDeterministicOperationOrder()
        => Operations
            .OrderBy(op => op.BaselineStartSec)
            .ThenBy(op => op.BaselineEndSec)
            .ThenBy(op => op.Id, StringComparer.Ordinal)
            .ToArray();

    /// <summary>
    /// Deterministic apply order independent of insertion order.
    /// Back-to-front ordering preserves baseline coordinates during rebuild.
    /// </summary>
    public IReadOnlyList<PickupEdlOperation> GetDeterministicApplyOrder()
        => Operations
            .Where(op => op.IsActive)
            .OrderByDescending(op => op.BaselineStartSec)
            .ThenByDescending(op => op.BaselineEndSec)
            .ThenBy(op => op.Id, StringComparer.Ordinal)
            .ToArray();

    /// <summary>
    /// Builds timeline-projection edit list with canonical front-to-back ordering.
    /// </summary>
    public IReadOnlyList<ChapterEdit> BuildProjectionEdits()
        => Operations
            .Where(op => op.IsActive)
            .OrderBy(op => op.BaselineStartSec)
            .ThenBy(op => op.BaselineEndSec)
            .ThenBy(op => op.Id, StringComparer.Ordinal)
            .Select(op => op.ToChapterEdit())
            .ToArray();

    private static void ValidateOperations(
        string chapterStem,
        string documentFingerprint,
        IReadOnlyList<PickupEdlOperation> operations)
    {
        var ids = new HashSet<string>(StringComparer.Ordinal);

        for (var i = 0; i < operations.Count; i++)
        {
            var op = operations[i] ?? throw new InvalidOperationException(
                $"Pickup EDL chapter '{chapterStem}' contains null operation at index {i}.");

            if (!ids.Add(op.Id))
            {
                throw new InvalidOperationException(
                    $"Pickup EDL chapter '{chapterStem}' contains duplicate op id '{op.Id}'.");
            }

            if (!string.Equals(op.ChapterStem, chapterStem, StringComparison.OrdinalIgnoreCase))
            {
                throw new InvalidOperationException(
                    $"Pickup EDL chapter mismatch for op '{op.Id}': document='{chapterStem}', op='{op.ChapterStem}'.");
            }

            if (!string.Equals(op.SourceFingerprint, documentFingerprint, StringComparison.Ordinal))
            {
                throw new InvalidOperationException(
                    $"Pickup EDL source fingerprint mismatch in chapter '{chapterStem}' for op '{op.Id}': " +
                    $"document='{documentFingerprint}', op='{op.SourceFingerprint}'.");
            }

            if (!Enum.IsDefined(typeof(PickupEdlOperationState), op.State))
            {
                throw new InvalidOperationException(
                    $"Pickup EDL chapter '{chapterStem}' op '{op.Id}' has unknown state value '{(int)op.State}'.");
            }
        }

        var active = operations
            .Where(op => op.IsActive)
            .OrderBy(op => op.BaselineStartSec)
            .ThenBy(op => op.BaselineEndSec)
            .ThenBy(op => op.Id, StringComparer.Ordinal)
            .ToArray();

        for (var i = 1; i < active.Length; i++)
        {
            var left = active[i - 1];
            var right = active[i];
            if (RangesOverlap(left.BaselineStartSec, left.BaselineEndSec, right.BaselineStartSec, right.BaselineEndSec))
            {
                throw new InvalidOperationException(
                    $"Pickup EDL overlap in chapter '{chapterStem}' between op '{left.Id}' " +
                    $"[{left.BaselineStartSec:F3}, {left.BaselineEndSec:F3}] and op '{right.Id}' " +
                    $"[{right.BaselineStartSec:F3}, {right.BaselineEndSec:F3}].");
            }
        }
    }

    private static bool RangesOverlap(double leftStart, double leftEnd, double rightStart, double rightEnd)
        => leftStart < rightEnd - OverlapEpsilonSec && rightStart < leftEnd - OverlapEpsilonSec;
}
