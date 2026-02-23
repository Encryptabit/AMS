namespace Ams.Workstation.Server.Models;

/// <summary>
/// Status lifecycle for a staged audio replacement in the Polish workflow.
/// Transitions: Staged -> Applied -> (optionally) Reverted. Failed if application errors.
/// </summary>
public enum ReplacementStatus
{
    /// <summary>Replacement is staged and ready to be applied.</summary>
    Staged,

    /// <summary>Replacement has been successfully applied to the chapter audio.</summary>
    Applied,

    /// <summary>Replacement was applied but has been reverted to the original audio.</summary>
    Reverted,

    /// <summary>Replacement failed during splice application.</summary>
    Failed
}

/// <summary>
/// A staged replacement that captures the full intent to replace a time range in the
/// original chapter audio with audio from a pickup recording. Immutable record that
/// flows through the Polish pipeline from staging through application or revert.
/// Status transitions create new instances via <c>with</c>.
/// </summary>
public sealed record StagedReplacement(
    string Id,
    string ChapterStem,
    int SentenceId,
    double OriginalStartSec,
    double OriginalEndSec,
    string PickupSourcePath,
    double PickupStartSec,
    double PickupEndSec,
    double CrossfadeDurationSec,
    string CrossfadeCurve,
    DateTime StagedAtUtc,
    ReplacementStatus Status);

/// <summary>
/// Result of matching a pickup recording segment to a sentence in the original audio.
/// Contains the matched time boundaries and a confidence score (0.0-1.0) indicating
/// how well the pickup text aligns with the target sentence.
/// </summary>
public sealed record PickupMatch(
    int SentenceId,
    double PickupStartSec,
    double PickupEndSec,
    double Confidence,
    string RecognizedText);

/// <summary>
/// A discrete segment within a pickup recording, identified by its time boundaries
/// and the text transcribed from that segment via ASR.
/// </summary>
public sealed record PickupSegment(
    double StartSec,
    double EndSec,
    string TranscribedText);

/// <summary>
/// Tracks a backup of the original audio segment before a replacement was applied.
/// Used by the undo system to restore the original audio if a replacement is reverted.
/// The <see cref="ReplacementId"/> links back to the originating <see cref="StagedReplacement"/>.
/// </summary>
public sealed record UndoRecord(
    string ReplacementId,
    string ChapterStem,
    int SentenceId,
    string OriginalSegmentPath,
    double OriginalStartSec,
    double OriginalEndSec,
    double OriginalDurationSec,
    double ReplacementDurationSec,
    DateTime AppliedAtUtc);

/// <summary>
/// Type of batch operation that can be applied across multiple chapters in the Polish workflow.
/// </summary>
public enum BatchOperationType
{
    Rename,
    Shift,
    PrePostRoll,
    PickupReplacement
}

/// <summary>
/// A batch operation definition that targets multiple chapters with a single
/// editing action (rename, shift, pre/post roll adjustment, or pickup replacement).
/// </summary>
public sealed record BatchOperation(
    string Id,
    BatchOperationType Type,
    IReadOnlyList<string> TargetChapters,
    string Description,
    DateTime CreatedAtUtc);

/// <summary>
/// Represents a chapter's selection state within a batch operation target list.
/// </summary>
public sealed record BatchTarget(
    string ChapterStem,
    bool Selected);
