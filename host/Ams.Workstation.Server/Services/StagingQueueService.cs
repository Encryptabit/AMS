using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
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

    private readonly BlazorWorkspace _workspace;
    private readonly object _lock = new();
    private Dictionary<string, List<StagedReplacement>>? _queue;

    public StagingQueueService(BlazorWorkspace workspace)
    {
        _workspace = workspace;
    }

    /// <summary>
    /// Adds a replacement to the staging queue for its chapter.
    /// </summary>
    public void Stage(StagedReplacement item)
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

            list.Add(item);
            Save();
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
                    list[index] = list[index] with { Status = newStatus };
                    Save();
                    return true;
                }
            }

            return false;
        }
    }

    /// <summary>
    /// Shifts OriginalStartSec and OriginalEndSec for all downstream items in the chapter
    /// whose SentenceId is greater than <paramref name="pivotSentenceId"/>.
    /// Applies to both Staged and Applied items so that revert/preview targets the
    /// correct region even when upstream replacements change duration.
    /// Call after apply/revert to cascade timing changes to downstream items.
    /// </summary>
    public void ShiftDownstream(string chapterStem, int pivotSentenceId, double deltaSec)
    {
        if (Math.Abs(deltaSec) < 0.001) return;
        ArgumentException.ThrowIfNullOrWhiteSpace(chapterStem);

        lock (_lock)
        {
            EnsureLoaded();
            if (!_queue!.TryGetValue(chapterStem, out var list))
                return;

            bool changed = false;
            for (int i = 0; i < list.Count; i++)
            {
                var item = list[i];
                if (item.SentenceId > pivotSentenceId
                    && (item.Status == ReplacementStatus.Staged || item.Status == ReplacementStatus.Applied))
                {
                    list[i] = item with
                    {
                        OriginalStartSec = item.OriginalStartSec + deltaSec,
                        OriginalEndSec = item.OriginalEndSec + deltaSec
                    };
                    changed = true;
                    Console.WriteLine(
                        $"[CascadeOffset] Shifted sentence {item.SentenceId} ({item.Status}): " +
                        $"{item.OriginalStartSec:F3}s → {list[i].OriginalStartSec:F3}s (delta {deltaSec:+0.000;-0.000}s)");
                }
            }

            if (changed) Save();
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
}
