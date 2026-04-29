using System.Text.Json.Serialization;

namespace Ams.Workstation.Server.Models;

/// <summary>
/// Operator-facing assignment status for one pickup segment in the durable batch Pick map.
/// </summary>
public enum PickupPickMapAssignmentStatus
{
    Inferred = 0,
    Confirmed = 1,
    Override = 2,
    Rejected = 3,
    Deferred = 4,
    NotApplicable = 5,
    Unresolved = 6
}

/// <summary>
/// Source identity for a stitched pickup batch plus the CRX target set used during inference.
/// </summary>
public sealed record PickupPickMapSourceReference
{
    [JsonConstructor]
    public PickupPickMapSourceReference(
        string path,
        string fingerprint,
        long fileSizeBytes,
        DateTime modifiedAtUtc,
        string crxTargetsFingerprint)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(path);
        ArgumentException.ThrowIfNullOrWhiteSpace(fingerprint);
        ArgumentException.ThrowIfNullOrWhiteSpace(crxTargetsFingerprint);

        if (fileSizeBytes < 0)
        {
            throw new ArgumentOutOfRangeException(
                nameof(fileSizeBytes),
                fileSizeBytes,
                "Pickup pick-map source file size must be non-negative.");
        }

        Path = path.Trim();
        Fingerprint = fingerprint.Trim();
        FileSizeBytes = fileSizeBytes;
        ModifiedAtUtc = modifiedAtUtc.ToUniversalTime();
        CrxTargetsFingerprint = crxTargetsFingerprint.Trim();
    }

    public string Path { get; }

    public string Fingerprint { get; }

    public long FileSizeBytes { get; }

    public DateTime ModifiedAtUtc { get; }

    public string CrxTargetsFingerprint { get; }
}

/// <summary>
/// Durable reference to one CRX/chapter target that a pickup segment may satisfy.
/// </summary>
public sealed record PickupPickMapTargetReference
{
    private const double RangeEpsilonSec = 0.000_001;

    [JsonConstructor]
    public PickupPickMapTargetReference(
        string chapterStem,
        string chapterName,
        int errorNumber,
        int sentenceId,
        double originalStartSec,
        double originalEndSec)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(chapterStem);
        ArgumentException.ThrowIfNullOrWhiteSpace(chapterName);

        if (errorNumber <= 0)
        {
            throw new ArgumentOutOfRangeException(
                nameof(errorNumber),
                errorNumber,
                $"Pickup pick-map target for chapter '{chapterStem}' has invalid error number '{errorNumber}'.");
        }

        if (sentenceId < 0)
        {
            throw new ArgumentOutOfRangeException(
                nameof(sentenceId),
                sentenceId,
                $"Pickup pick-map target for chapter '{chapterStem}' and error '{errorNumber}' has invalid sentence id '{sentenceId}'.");
        }

        EnsureRange(
            startSec: originalStartSec,
            endSec: originalEndSec,
            chapterStem: chapterStem,
            errorNumber: errorNumber);

        ChapterStem = chapterStem.Trim();
        ChapterName = chapterName.Trim();
        ErrorNumber = errorNumber;
        SentenceId = sentenceId;
        OriginalStartSec = originalStartSec;
        OriginalEndSec = originalEndSec;
    }

    public string ChapterStem { get; }

    public string ChapterName { get; }

    public int ErrorNumber { get; }

    public int SentenceId { get; }

    public double OriginalStartSec { get; }

    public double OriginalEndSec { get; }

    [JsonIgnore]
    public string TargetKey => BuildTargetKey(ChapterStem, ErrorNumber, SentenceId);

    public static string BuildTargetKey(string chapterStem, int errorNumber, int sentenceId)
        => $"{chapterStem.Trim()}::{errorNumber:D8}::{sentenceId:D8}";

    private static void EnsureRange(double startSec, double endSec, string chapterStem, int errorNumber)
    {
        if (double.IsNaN(startSec) || double.IsInfinity(startSec) ||
            double.IsNaN(endSec) || double.IsInfinity(endSec))
        {
            throw new ArgumentOutOfRangeException(
                nameof(startSec),
                $"Pickup pick-map target for chapter '{chapterStem}' and error '{errorNumber}' has non-finite original range.");
        }

        if (startSec < 0)
        {
            throw new ArgumentOutOfRangeException(
                nameof(startSec),
                startSec,
                $"Pickup pick-map target for chapter '{chapterStem}' and error '{errorNumber}' has negative original start '{startSec}'.");
        }

        if (endSec <= startSec + RangeEpsilonSec)
        {
            throw new ArgumentOutOfRangeException(
                nameof(endSec),
                $"Pickup pick-map target for chapter '{chapterStem}' and error '{errorNumber}' has invalid original range " +
                $"[{startSec:F6}, {endSec:F6}] (end must be greater than start).");
        }
    }
}

/// <summary>
/// One durable canonical row from pickup segment to inferred/selected CRX target.
/// </summary>
public sealed record PickupPickMapAssignment
{
    private const double RangeEpsilonSec = 0.000_001;

    [JsonConstructor]
    public PickupPickMapAssignment(
        string id,
        string pickupSegmentId,
        double sourceStartSec,
        double sourceEndSec,
        PickupPickMapAssignmentStatus status,
        PickupPickMapTargetReference? inferredTarget,
        PickupPickMapTargetReference? selectedTarget,
        double? confidence,
        string? note,
        string? validationError,
        DateTime updatedAtUtc)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(id);
        ArgumentException.ThrowIfNullOrWhiteSpace(pickupSegmentId);

        if (!Enum.IsDefined(typeof(PickupPickMapAssignmentStatus), status))
        {
            throw new ArgumentOutOfRangeException(
                nameof(status),
                status,
                $"Pickup pick-map assignment '{id}' has unknown status value '{(int)status}'.");
        }

        EnsureRange(
            startSec: sourceStartSec,
            endSec: sourceEndSec,
            assignmentId: id);

        if (confidence is < 0 or > 1 ||
            confidence is not null && (double.IsNaN(confidence.Value) || double.IsInfinity(confidence.Value)))
        {
            throw new ArgumentOutOfRangeException(
                nameof(confidence),
                confidence,
                $"Pickup pick-map assignment '{id}' has invalid confidence '{confidence}'.");
        }

        ValidateTargetAffinity(id, status, inferredTarget, selectedTarget);

        Id = id.Trim();
        PickupSegmentId = pickupSegmentId.Trim();
        SourceStartSec = sourceStartSec;
        SourceEndSec = sourceEndSec;
        Status = status;
        InferredTarget = inferredTarget;
        SelectedTarget = selectedTarget;
        Confidence = confidence;
        Note = string.IsNullOrWhiteSpace(note) ? null : note.Trim();
        ValidationError = string.IsNullOrWhiteSpace(validationError) ? null : validationError.Trim();
        UpdatedAtUtc = updatedAtUtc.ToUniversalTime();
    }

    public string Id { get; }

    public string PickupSegmentId { get; }

    public double SourceStartSec { get; }

    public double SourceEndSec { get; }

    public PickupPickMapAssignmentStatus Status { get; }

    public PickupPickMapTargetReference? InferredTarget { get; }

    public PickupPickMapTargetReference? SelectedTarget { get; }

    public double? Confidence { get; }

    public string? Note { get; }

    public string? ValidationError { get; }

    public DateTime UpdatedAtUtc { get; }

    [JsonIgnore]
    public PickupPickMapTargetReference? EffectiveTarget => SelectedTarget ?? InferredTarget;

    [JsonIgnore]
    public bool RequiresUniqueTarget
        => Status is PickupPickMapAssignmentStatus.Inferred
            or PickupPickMapAssignmentStatus.Confirmed
            or PickupPickMapAssignmentStatus.Override;

    private static void ValidateTargetAffinity(
        string assignmentId,
        PickupPickMapAssignmentStatus status,
        PickupPickMapTargetReference? inferredTarget,
        PickupPickMapTargetReference? selectedTarget)
    {
        switch (status)
        {
            case PickupPickMapAssignmentStatus.Inferred:
                if (selectedTarget is null && inferredTarget is null)
                {
                    throw new InvalidOperationException(
                        $"Pickup pick-map assignment '{assignmentId}' is inferred but has no target reference.");
                }
                break;

            case PickupPickMapAssignmentStatus.Confirmed:
            case PickupPickMapAssignmentStatus.Override:
                if (selectedTarget is null)
                {
                    throw new InvalidOperationException(
                        $"Pickup pick-map assignment '{assignmentId}' has status '{status}' but no selected target reference.");
                }
                break;

            case PickupPickMapAssignmentStatus.Rejected:
            case PickupPickMapAssignmentStatus.Deferred:
            case PickupPickMapAssignmentStatus.NotApplicable:
            case PickupPickMapAssignmentStatus.Unresolved:
                if (selectedTarget is not null)
                {
                    throw new InvalidOperationException(
                        $"Pickup pick-map assignment '{assignmentId}' has terminal/manual status '{status}' but still has a selected target.");
                }
                break;
        }
    }

    private static void EnsureRange(double startSec, double endSec, string assignmentId)
    {
        if (double.IsNaN(startSec) || double.IsInfinity(startSec) ||
            double.IsNaN(endSec) || double.IsInfinity(endSec))
        {
            throw new ArgumentOutOfRangeException(
                nameof(startSec),
                $"Pickup pick-map assignment '{assignmentId}' has non-finite source segment range.");
        }

        if (startSec < 0)
        {
            throw new ArgumentOutOfRangeException(
                nameof(startSec),
                startSec,
                $"Pickup pick-map assignment '{assignmentId}' has negative source start '{startSec}'.");
        }

        if (endSec <= startSec + RangeEpsilonSec)
        {
            throw new ArgumentOutOfRangeException(
                nameof(endSec),
                $"Pickup pick-map assignment '{assignmentId}' has invalid source range " +
                $"[{startSec:F6}, {endSec:F6}] (end must be greater than start).");
        }
    }
}

/// <summary>
/// Deterministic chapter bucket for Pick diagnostics and UI grouping.
/// </summary>
public sealed record PickupPickMapChapterGroup(string ChapterStem, IReadOnlyList<PickupPickMapAssignment> Assignments);

/// <summary>
/// Durable canonical batch Pick map persisted under .polish/pickups/pick-map.json.
/// </summary>
public sealed record PickupPickMapDocument
{
    public const string CurrentSchemaVersion = "pickup-pick-map/v1";
    public const string UnassignedChapterGroup = "__unassigned";

    [JsonConstructor]
    public PickupPickMapDocument(
        string schemaVersion,
        int revision,
        PickupPickMapSourceReference source,
        IReadOnlyList<PickupPickMapAssignment>? assignments,
        DateTime createdAtUtc,
        DateTime updatedAtUtc,
        string? lastOperationId,
        string? lastValidationError,
        bool isDraft = false)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(schemaVersion);
        ArgumentNullException.ThrowIfNull(source);

        if (!string.Equals(schemaVersion, CurrentSchemaVersion, StringComparison.Ordinal))
        {
            throw new ArgumentException(
                $"Unsupported pickup pick-map schema version '{schemaVersion}'. Expected '{CurrentSchemaVersion}'.",
                nameof(schemaVersion));
        }

        if (revision < 0)
        {
            throw new ArgumentOutOfRangeException(
                nameof(revision),
                revision,
                "Pickup pick-map revision must be non-negative.");
        }

        var normalizedAssignments = OrderAssignments(assignments ?? Array.Empty<PickupPickMapAssignment>());

        ValidateAssignments(source, normalizedAssignments, isDraft);

        SchemaVersion = schemaVersion;
        Revision = revision;
        Source = source;
        Assignments = normalizedAssignments;
        CreatedAtUtc = createdAtUtc.ToUniversalTime();
        UpdatedAtUtc = updatedAtUtc.ToUniversalTime();
        LastOperationId = string.IsNullOrWhiteSpace(lastOperationId) ? null : lastOperationId.Trim();
        LastValidationError = string.IsNullOrWhiteSpace(lastValidationError) ? null : lastValidationError.Trim();
        IsDraft = isDraft;
    }

    public string SchemaVersion { get; }

    public int Revision { get; }

    public PickupPickMapSourceReference Source { get; }

    public IReadOnlyList<PickupPickMapAssignment> Assignments { get; }

    public DateTime CreatedAtUtc { get; }

    public DateTime UpdatedAtUtc { get; }

    public string? LastOperationId { get; }

    public string? LastValidationError { get; }

    public bool IsDraft { get; }

    public IReadOnlyList<PickupPickMapAssignment> GetDeterministicAssignmentOrder()
        => OrderAssignments(Assignments);

    public IReadOnlyList<PickupPickMapChapterGroup> GetDeterministicChapterGroups()
        => GetDeterministicAssignmentOrder()
            .GroupBy(
                assignment => assignment.EffectiveTarget?.ChapterStem ?? UnassignedChapterGroup,
                StringComparer.OrdinalIgnoreCase)
            .OrderBy(group => string.Equals(group.Key, UnassignedChapterGroup, StringComparison.OrdinalIgnoreCase) ? 1 : 0)
            .ThenBy(group => group.Key, StringComparer.OrdinalIgnoreCase)
            .Select(group => new PickupPickMapChapterGroup(group.Key, group.ToArray()))
            .ToArray();

    public IReadOnlyDictionary<PickupPickMapAssignmentStatus, int> GetAssignmentCountsByStatus()
        => Assignments
            .GroupBy(assignment => assignment.Status)
            .OrderBy(group => group.Key)
            .ToDictionary(group => group.Key, group => group.Count());

    public IReadOnlyDictionary<string, int> GetAssignmentCountsByChapter()
        => GetDeterministicChapterGroups()
            .ToDictionary(group => group.ChapterStem, group => group.Assignments.Count, StringComparer.OrdinalIgnoreCase);

    private static PickupPickMapAssignment[] OrderAssignments(IEnumerable<PickupPickMapAssignment> assignments)
        => assignments
            .OrderBy(assignment => assignment.EffectiveTarget is null ? 1 : 0)
            .ThenBy(assignment => assignment.EffectiveTarget?.ChapterStem ?? string.Empty, StringComparer.OrdinalIgnoreCase)
            .ThenBy(assignment => assignment.EffectiveTarget?.ErrorNumber ?? int.MaxValue)
            .ThenBy(assignment => assignment.EffectiveTarget?.SentenceId ?? int.MaxValue)
            .ThenBy(assignment => assignment.SourceStartSec)
            .ThenBy(assignment => assignment.SourceEndSec)
            .ThenBy(assignment => assignment.Id, StringComparer.Ordinal)
            .ToArray();

    private static void ValidateAssignments(
        PickupPickMapSourceReference source,
        IReadOnlyList<PickupPickMapAssignment> assignments,
        bool isDraft)
    {
        if (assignments.Count == 0)
        {
            if (!isDraft)
            {
                throw new InvalidOperationException(
                    $"Pickup pick-map for source '{source.Path}' has no assignments. Empty maps are allowed only when IsDraft is true.");
            }

            return;
        }

        var ids = new HashSet<string>(StringComparer.Ordinal);
        var segmentIds = new HashSet<string>(StringComparer.Ordinal);
        var selectedTargets = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        for (var i = 0; i < assignments.Count; i++)
        {
            var assignment = assignments[i] ?? throw new InvalidOperationException(
                $"Pickup pick-map for source '{source.Path}' contains null assignment at index {i}.");

            if (!ids.Add(assignment.Id))
            {
                throw new InvalidOperationException(
                    $"Pickup pick-map for source '{source.Path}' contains duplicate assignment id '{assignment.Id}'.");
            }

            if (!segmentIds.Add(assignment.PickupSegmentId))
            {
                throw new InvalidOperationException(
                    $"Pickup pick-map for source '{source.Path}' contains duplicate pickup segment id '{assignment.PickupSegmentId}'.");
            }

            if (!Enum.IsDefined(typeof(PickupPickMapAssignmentStatus), assignment.Status))
            {
                throw new InvalidOperationException(
                    $"Pickup pick-map assignment '{assignment.Id}' has unknown status value '{(int)assignment.Status}'.");
            }

            if (!assignment.RequiresUniqueTarget)
            {
                continue;
            }

            var target = assignment.EffectiveTarget
                ?? throw new InvalidOperationException(
                    $"Pickup pick-map assignment '{assignment.Id}' has status '{assignment.Status}' but no target reference.");

            if (!selectedTargets.Add(target.TargetKey))
            {
                throw new InvalidOperationException(
                    $"Pickup pick-map for source '{source.Path}' contains duplicate selected target " +
                    $"chapter='{target.ChapterStem}', error='{target.ErrorNumber}', sentence='{target.SentenceId}'.");
            }
        }
    }
}
