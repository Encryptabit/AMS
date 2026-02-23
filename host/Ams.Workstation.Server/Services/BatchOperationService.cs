using System;
using System.Collections.Generic;
using System.Linq;
using Ams.Workstation.Server.Models;

namespace Ams.Workstation.Server.Services;

/// <summary>
/// Provides batch operations for the Polish workflow: rename, shift, and pre/post roll
/// standardization across multiple chapters. Operations are staged non-destructively
/// via the <see cref="StagingQueueService"/> pattern.
/// </summary>
public class BatchOperationService
{
    private readonly BlazorWorkspace _workspace;
    private readonly StagingQueueService _stagingQueue;
    private readonly List<BatchOperation> _history = new();

    public BatchOperationService(BlazorWorkspace workspace, StagingQueueService stagingQueue)
    {
        _workspace = workspace;
        _stagingQueue = stagingQueue;
    }

    /// <summary>
    /// Returns all available chapters from the workspace with selection state.
    /// </summary>
    public List<BatchTarget> GetAvailableChapters()
    {
        if (!_workspace.IsInitialized)
        {
            return new List<BatchTarget>();
        }

        return _workspace.AvailableChapters
            .Select(name =>
            {
                var stem = _workspace.GetStemForChapter(name) ?? name;
                return new BatchTarget(stem, Selected: false);
            })
            .ToList();
    }

    /// <summary>
    /// Creates a batch rename operation. Records the operation intent (naming pattern)
    /// for application at a later time. Actual rename logic is deferred to apply time.
    /// </summary>
    /// <param name="chapters">List of chapter stems to rename.</param>
    /// <param name="pattern">Naming pattern, e.g. "Chapter {N:D2}".</param>
    /// <returns>The created batch operation.</returns>
    public BatchOperation CreateBatchRename(IReadOnlyList<string> chapters, string pattern)
    {
        ArgumentNullException.ThrowIfNull(chapters);
        ArgumentException.ThrowIfNullOrWhiteSpace(pattern);

        var operation = new BatchOperation(
            Id: Guid.NewGuid().ToString("N"),
            Type: BatchOperationType.Rename,
            TargetChapters: chapters,
            Description: $"Rename {chapters.Count} chapter(s) using pattern: {pattern}",
            CreatedAtUtc: DateTime.UtcNow);

        _history.Add(operation);
        return operation;
    }

    /// <summary>
    /// Creates a batch timing shift operation. Shifting means adjusting the gap between
    /// chapter title reading and content start.
    /// </summary>
    /// <param name="chapters">List of chapter stems to shift.</param>
    /// <param name="shiftSeconds">Shift amount in seconds (positive = later, negative = earlier).</param>
    /// <returns>The created batch operation.</returns>
    public BatchOperation CreateBatchShift(IReadOnlyList<string> chapters, double shiftSeconds)
    {
        ArgumentNullException.ThrowIfNull(chapters);

        var direction = shiftSeconds >= 0 ? "forward" : "backward";
        var operation = new BatchOperation(
            Id: Guid.NewGuid().ToString("N"),
            Type: BatchOperationType.Shift,
            TargetChapters: chapters,
            Description: $"Shift {chapters.Count} chapter(s) {Math.Abs(shiftSeconds):F2}s {direction}",
            CreatedAtUtc: DateTime.UtcNow);

        _history.Add(operation);
        return operation;
    }

    /// <summary>
    /// Creates a batch pre/post roll standardization operation. When applied, this will
    /// re-run AudioTreatmentService with standardized TreatmentOptions.
    /// </summary>
    /// <param name="chapters">List of chapter stems to standardize.</param>
    /// <param name="preRollSec">Pre-roll duration in seconds.</param>
    /// <param name="postRollSec">Post-roll duration in seconds.</param>
    /// <returns>The created batch operation.</returns>
    public BatchOperation CreateBatchPrePostRoll(IReadOnlyList<string> chapters, double preRollSec, double postRollSec)
    {
        ArgumentNullException.ThrowIfNull(chapters);

        if (preRollSec < 0) throw new ArgumentOutOfRangeException(nameof(preRollSec), "Pre-roll must be >= 0");
        if (postRollSec < 0) throw new ArgumentOutOfRangeException(nameof(postRollSec), "Post-roll must be >= 0");

        var operation = new BatchOperation(
            Id: Guid.NewGuid().ToString("N"),
            Type: BatchOperationType.PrePostRoll,
            TargetChapters: chapters,
            Description: $"Standardize {chapters.Count} chapter(s): pre={preRollSec:F1}s, post={postRollSec:F1}s",
            CreatedAtUtc: DateTime.UtcNow);

        _history.Add(operation);
        return operation;
    }

    /// <summary>
    /// Placeholder for batch DSP operations. DSP pipeline is deferred per locked decision.
    /// </summary>
    // TODO: Implement batch DSP processing when DSP pipeline is ready
    public BatchOperation CreateBatchDsp(IReadOnlyList<string> chapters)
    {
        throw new NotSupportedException("DSP batch operations are not yet implemented. Deferred per locked decision.");
    }

    /// <summary>
    /// Returns recent batch operations history.
    /// </summary>
    public IReadOnlyList<BatchOperation> GetBatchHistory()
    {
        return _history.AsReadOnly();
    }
}
