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
    string RecognizedText,
    int? ErrorNumber = null,
    bool IsLowConfidence = false);

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
/// For rebuild-based revert, also tracks the replacement audio segment path so the
/// rebuild process can re-apply edits to the baseline audio.
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
    DateTime AppliedAtUtc,
    string? ReplacementSegmentPath = null);

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

/// <summary>
/// Wraps a <see cref="PickupMatch"/> with a chapter stem to form a composite key,
/// preventing sentence ID collisions when matching pickups across multiple chapters.
/// </summary>
public sealed record CrossChapterPickupMatch(
    string ChapterStem,
    PickupMatch Match)
{
    /// <summary>Composite key: chapterStem:sentenceId to avoid cross-chapter ID collisions.</summary>
    public string CompositeKey => $"{ChapterStem}:{Match.SentenceId}";
}

/// <summary>
/// The type of roomtone editing operation to perform on an audio region.
/// </summary>
public enum RoomtoneOperation
{
    /// <summary>Insert roomtone at a point, pushing content apart.</summary>
    Insert,

    /// <summary>Replace selected region with looped roomtone.</summary>
    Replace,

    /// <summary>Delete selected region, pulling content together with crossfade.</summary>
    Delete
}

/// <summary>
/// Parameters for a roomtone editing operation specifying the target region,
/// crossfade duration, and crossfade curve shape.
/// </summary>
public sealed record RoomtoneRequest(
    RoomtoneOperation Operation,
    double StartSec,
    double EndSec,
    double CrossfadeDurationSec = 0.070,
    string CrossfadeCurve = "hsin");

/// <summary>
/// State of a pickup match box in the three-column pipeline (Match -> Stage -> Commit).
/// </summary>
public enum PickupBoxState
{
    /// <summary>Pickup has been matched but not yet staged for replacement.</summary>
    Matched,

    /// <summary>Pickup replacement is staged and ready for review.</summary>
    Staged,

    /// <summary>Pickup replacement has been committed to the chapter audio.</summary>
    Committed
}

public sealed record CrxPickupTarget(
    int ErrorNumber,
    string ChapterStem,
    string ChapterName,
    int SentenceId,
    string ShouldBeText,
    string BookText,
    double OriginalStartSec,
    double OriginalEndSec);

public sealed record PickupArtifacts(
    string PickupFilePath,
    DateTime ProcessedAtUtc,
    long PickupFileSizeBytes,
    DateTime PickupFileModifiedUtc,
    string CrxTargetsFingerprint,
    Dictionary<string, List<CrossChapterPickupMatch>> MatchesByChapter);

/// <summary>
/// Identifies the source type of a pickup asset — either a segment within
/// a multi-pickup session file or an individual standalone WAV file.
/// </summary>
public enum PickupSourceType
{
    /// <summary>A segment extracted from a multi-pickup session recording.</summary>
    SessionSegment,

    /// <summary>An individual standalone pickup WAV file.</summary>
    IndividualFile
}

/// <summary>
/// A normalized pickup asset that unifies both session-file segments and individual
/// pickup files into a single shape. Created during import processing and cached
/// for reuse. Each asset references a source file with trim boundaries and
/// carries ASR transcription and matching metadata.
/// </summary>
public sealed record PickupAsset(
    string Id,
    PickupSourceType SourceType,
    string SourceFilePath,
    double TrimStartSec,
    double TrimEndSec,
    string TranscribedText,
    double Confidence,
    int? MatchedErrorNumber,
    int? MatchedSentenceId,
    string? MatchedChapterStem,
    DateTime ImportedAtUtc);

/// <summary>
/// Cached results of pickup asset processing for a source file. Invalidated
/// when the source file changes (path + size + modified timestamp) or when
/// the CRX target set changes.
/// </summary>
public sealed record PickupAssetCache(
    string SourceFilePath,
    long SourceFileSizeBytes,
    DateTime SourceFileModifiedUtc,
    string CrxTargetsFingerprint,
    IReadOnlyList<PickupAsset> Assets,
    DateTime ProcessedAtUtc);

public static class StagedReplacementExtensions
{
    public static double PickupDuration(this StagedReplacement r) =>
        r.PickupEndSec - r.PickupStartSec;

    public static double OriginalDuration(this StagedReplacement r) =>
        r.OriginalEndSec - r.OriginalStartSec;

    /// <summary>
    /// Where the replacement actually ends in the post-splice audio:
    /// original start + pickup duration (not the original end).
    /// </summary>
    public static double ActualReplacedEndSec(this StagedReplacement r) =>
        r.OriginalStartSec + r.PickupDuration();
}
