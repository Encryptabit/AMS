using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using Ams.Core.Audio;
using Ams.Workstation.Server.Models;

namespace Ams.Workstation.Server.Services;

/// <summary>
/// Manages a non-destructive queue of <see cref="StagedReplacement"/> items per chapter.
/// Replacements can be staged, listed, removed, and cleared without modifying audio.
/// Persists queue to {workDir}/.polish/staging-queue.json.
/// Singleton -- shared across all Blazor circuits.
/// </summary>
public class StagingQueueService
{
    private static readonly JsonSerializerOptions JsonOptions = new() { WriteIndented = true };
    private const double BoundaryEpsilonSec = 0.001;

    private readonly BlazorWorkspace _workspace;
    private readonly EditListService _editListService;
    private readonly object _lock = new();
    private Dictionary<string, List<StagedReplacement>>? _queue;

    public StagingQueueService(BlazorWorkspace workspace, EditListService editListService)
    {
        _workspace = workspace;
        _editListService = editListService;
    }

    /// <summary>
    /// Adds a replacement to the staging queue for its chapter.
    /// </summary>
    public void Stage(StagedReplacement item)
    {
        if (!TryStage(item, out var validationError))
        {
            throw new InvalidOperationException(
                $"Unable to stage replacement '{item.Id}': {validationError ?? "validation failed"}");
        }
    }

    /// <summary>
    /// Adds a replacement to the staging queue for its chapter after validating overlap constraints.
    /// </summary>
    /// <param name="item">The replacement to stage.</param>
    /// <param name="validationError">Validation error if staging failed.</param>
    /// <returns>True when the item was staged; otherwise false.</returns>
    public bool TryStage(StagedReplacement item, out string? validationError)
    {
        ArgumentNullException.ThrowIfNull(item);

        lock (_lock)
        {
            EnsureLoaded();
            if (!_queue!.TryGetValue(item.ChapterStem, out var list))
            {
                list = new List<StagedReplacement>();
                _queue[item.ChapterStem] = list;
            }

            if (!IsValidRange(item.OriginalStartSec, item.OriginalEndSec))
            {
                validationError = "Replacement boundaries are invalid (end must be greater than start).";
                return false;
            }

            var conflicting = FindFirstOverlap(
                list,
                item.OriginalStartSec,
                item.OriginalEndSec,
                item.Id);
            if (conflicting is not null)
            {
                validationError = BuildOverlapError(conflicting);
                return false;
            }

            list.Add(item);
            Save();
            validationError = null;
            return true;
        }
    }

    /// <summary>
    /// Removes a staged replacement by ID. Only removes items with <see cref="ReplacementStatus.Staged"/> status.
    /// </summary>
    /// <returns>True if the item was found and removed.</returns>
    public bool Unstage(string replacementId)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(replacementId);

        lock (_lock)
        {
            EnsureLoaded();
            foreach (var list in _queue!.Values)
            {
                var index = list.FindIndex(r => r.Id == replacementId && r.Status == ReplacementStatus.Staged);
                if (index >= 0)
                {
                    list.RemoveAt(index);
                    Save();
                    return true;
                }
            }

            return false;
        }
    }

    /// <summary>
    /// Returns all staged replacements for a specific chapter.
    /// </summary>
    public IReadOnlyList<StagedReplacement> GetQueue(string chapterStem)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(chapterStem);

        lock (_lock)
        {
            EnsureLoaded();
            return _queue!.TryGetValue(chapterStem, out var list)
                ? list.AsReadOnly()
                : Array.Empty<StagedReplacement>();
        }
    }

    /// <summary>
    /// Returns all items across all chapters with <see cref="ReplacementStatus.Staged"/> status.
    /// </summary>
    public IReadOnlyList<StagedReplacement> GetAllQueued()
    {
        lock (_lock)
        {
            EnsureLoaded();
            return _queue!.Values
                .SelectMany(list => list)
                .Where(r => r.Status == ReplacementStatus.Staged)
                .ToList()
                .AsReadOnly();
        }
    }

    /// <summary>
    /// Transitions the status of a replacement (e.g., Staged -> Applied, Applied -> Reverted).
    /// When transitioning to Applied, creates a <see cref="ChapterEdit"/> record in the edit list.
    /// When transitioning to Reverted, removes the corresponding edit record.
    /// </summary>
    /// <returns>True if the item was found and updated.</returns>
    public bool UpdateStatus(string replacementId, ReplacementStatus newStatus)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(replacementId);

        lock (_lock)
        {
            EnsureLoaded();
            foreach (var (chapterStem, list) in _queue!)
            {
                var index = list.FindIndex(r => r.Id == replacementId);
                if (index >= 0)
                {
                    var replacement = list[index];
                    list[index] = replacement with { Status = newStatus };
                    Save();

                    // Track in edit list for timeline projection
                    if (newStatus == ReplacementStatus.Applied)
                    {
                        var edit = new ChapterEdit(
                            Id: replacement.Id,
                            ChapterStem: replacement.ChapterStem,
                            Operation: EditOperation.PickupReplace,
                            BaselineStartSec: replacement.OriginalStartSec,
                            BaselineEndSec: replacement.OriginalEndSec,
                            ReplacementDurationSec: replacement.PickupDuration(),
                            SentenceId: replacement.SentenceId,
                            ErrorNumber: null,
                            PickupAssetId: null,
                            CrossfadeDurationSec: replacement.CrossfadeDurationSec,
                            CrossfadeCurve: replacement.CrossfadeCurve,
                            AppliedAtUtc: DateTime.UtcNow);
                        _editListService.Add(edit);
                    }
                    else if (newStatus == ReplacementStatus.Reverted)
                    {
                        _editListService.Remove(replacement.Id);
                    }

                    return true;
                }
            }

            return false;
        }
    }

    /// <summary>
    /// Updates the original (chapter-side) splice boundaries for a staged replacement.
    /// Called when the user drags region handles on the waveform.
    /// Only updates items with <see cref="ReplacementStatus.Staged"/> status.
    /// </summary>
    /// <param name="replacementId">The replacement to update.</param>
    /// <param name="newStartSec">New start time in seconds.</param>
    /// <param name="newEndSec">New end time in seconds.</param>
    /// <returns>True if the item was found and updated.</returns>
    public bool UpdateBoundaries(string replacementId, double newStartSec, double newEndSec)
        => TryUpdateBoundaries(replacementId, newStartSec, newEndSec, out _);

    /// <summary>
    /// Updates splice boundaries for a staged replacement, rejecting invalid or overlapping ranges.
    /// </summary>
    /// <param name="replacementId">The replacement to update.</param>
    /// <param name="newStartSec">New start time in seconds.</param>
    /// <param name="newEndSec">New end time in seconds.</param>
    /// <param name="validationError">Validation error if update failed.</param>
    /// <returns>True when updated; otherwise false.</returns>
    public bool TryUpdateBoundaries(
        string replacementId,
        double newStartSec,
        double newEndSec,
        out string? validationError)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(replacementId);

        lock (_lock)
        {
            EnsureLoaded();
            if (!IsValidRange(newStartSec, newEndSec))
            {
                validationError = "Region must have a positive duration.";
                return false;
            }

            foreach (var list in _queue!.Values)
            {
                var index = list.FindIndex(r => r.Id == replacementId && r.Status == ReplacementStatus.Staged);
                if (index >= 0)
                {
                    var conflicting = FindFirstOverlap(list, newStartSec, newEndSec, replacementId);
                    if (conflicting is not null)
                    {
                        validationError = BuildOverlapError(conflicting);
                        return false;
                    }

                    list[index] = list[index] with
                    {
                        OriginalStartSec = newStartSec,
                        OriginalEndSec = newEndSec
                    };
                    Save();
                    validationError = null;
                    return true;
                }
            }

            validationError = $"Replacement '{replacementId}' was not found or is not staged.";
            return false;
        }
    }

    /// <summary>
    /// Maps a baseline time position to the current (post-edit) timeline for a chapter
    /// by delegating to <see cref="TimelineProjection.BaselineToCurrentTime"/>.
    /// </summary>
    /// <param name="chapterStem">The chapter whose edit list to use.</param>
    /// <param name="baselineTimeSec">A time in the original, unedited audio.</param>
    /// <returns>The equivalent time in the current post-edit audio.</returns>
    public double GetCurrentTime(string chapterStem, double baselineTimeSec)
    {
        var edits = _editListService.GetEdits(chapterStem);
        return TimelineProjection.BaselineToCurrentTime(baselineTimeSec, edits);
    }

    /// <summary>
    /// Checks whether the given range overlaps any active (staged/applied) replacement
    /// in a chapter, excluding <paramref name="replacementId"/> when provided.
    /// </summary>
    public bool TryGetActiveOverlap(
        string chapterStem,
        string? replacementId,
        double startSec,
        double endSec,
        out StagedReplacement? conflicting)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(chapterStem);

        lock (_lock)
        {
            EnsureLoaded();
            conflicting = null;

            if (!_queue!.TryGetValue(chapterStem, out var list))
            {
                return false;
            }

            conflicting = FindFirstOverlap(list, startSec, endSec, replacementId);
            return conflicting is not null;
        }
    }

    /// <summary>
    /// Clears all staged items for a specific chapter.
    /// </summary>
    public void Clear(string chapterStem)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(chapterStem);

        lock (_lock)
        {
            EnsureLoaded();
            if (_queue!.Remove(chapterStem))
            {
                Save();
            }
        }
    }

    /// <summary>
    /// Clears all staging queues across all chapters.
    /// </summary>
    public void ClearAll()
    {
        lock (_lock)
        {
            EnsureLoaded();
            _queue!.Clear();
            Save();
        }
    }

    private string GetFilePath()
    {
        var workDir = _workspace.WorkingDirectory
                      ?? throw new InvalidOperationException("Workspace not initialized.");
        return Path.Combine(workDir, ".polish", "staging-queue.json");
    }

    private void EnsureLoaded()
    {
        if (_queue != null) return;

        _queue = new Dictionary<string, List<StagedReplacement>>(StringComparer.OrdinalIgnoreCase);

        try
        {
            var path = GetFilePath();
            if (!File.Exists(path)) return;

            var json = File.ReadAllText(path);
            var deserialized = JsonSerializer.Deserialize<Dictionary<string, List<StagedReplacement>>>(json, JsonOptions);
            if (deserialized != null)
            {
                foreach (var (key, value) in deserialized)
                {
                    _queue[key] = value;
                }
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Failed to load staging queue: {ex.Message}");
        }
    }

    private void Save()
    {
        try
        {
            var path = GetFilePath();
            var dir = Path.GetDirectoryName(path);
            if (!string.IsNullOrEmpty(dir) && !Directory.Exists(dir))
            {
                Directory.CreateDirectory(dir);
            }

            var json = JsonSerializer.Serialize(_queue, JsonOptions);
            File.WriteAllText(path, json);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Failed to save staging queue: {ex.Message}");
        }
    }

    private static bool IsValidRange(double startSec, double endSec)
        => !double.IsNaN(startSec)
           && !double.IsNaN(endSec)
           && !double.IsInfinity(startSec)
           && !double.IsInfinity(endSec)
           && endSec > startSec + BoundaryEpsilonSec;

    private static bool Overlaps(double aStart, double aEnd, double bStart, double bEnd)
        => aStart < bEnd - BoundaryEpsilonSec && bStart < aEnd - BoundaryEpsilonSec;

    private static bool IsActive(StagedReplacement item)
        => item.Status == ReplacementStatus.Staged || item.Status == ReplacementStatus.Applied;

    private static StagedReplacement? FindFirstOverlap(
        IReadOnlyList<StagedReplacement> list,
        double startSec,
        double endSec,
        string? excludeReplacementId)
    {
        for (var i = 0; i < list.Count; i++)
        {
            var item = list[i];
            if (!IsActive(item))
            {
                continue;
            }

            if (!string.IsNullOrWhiteSpace(excludeReplacementId) &&
                string.Equals(item.Id, excludeReplacementId, StringComparison.Ordinal))
            {
                continue;
            }

            if (Overlaps(startSec, endSec, item.OriginalStartSec, item.OriginalEndSec))
            {
                return item;
            }
        }

        return null;
    }

    private static string BuildOverlapError(StagedReplacement conflicting)
        => $"Overlaps sentence {conflicting.SentenceId} ({conflicting.Status}) at " +
           $"{conflicting.OriginalStartSec:F3}s-{conflicting.OriginalEndSec:F3}s.";
}
