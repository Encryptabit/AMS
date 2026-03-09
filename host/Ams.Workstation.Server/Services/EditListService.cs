using System.Text.Json;
using Ams.Core.Audio;

namespace Ams.Workstation.Server.Services;

/// <summary>
/// Manages an immutable, append-only list of <see cref="ChapterEdit"/> records per chapter.
/// Provides the edit history used by <see cref="TimelineProjection"/> to map between
/// baseline and current timeline positions.
///
/// Singleton — shared across all Blazor circuits. Persists to
/// <c>{workDir}/.polish/edit-list.json</c> (same <c>.polish/</c> directory as
/// <see cref="StagingQueueService"/>).
///
/// Thread-safe via simple <c>lock</c> (single-user workstation app).
/// </summary>
public class EditListService
{
    private static readonly JsonSerializerOptions JsonOptions = new() { WriteIndented = true };

    private readonly BlazorWorkspace _workspace;
    private readonly object _lock = new();
    private Dictionary<string, List<ChapterEdit>>? _edits;

    public EditListService(BlazorWorkspace workspace)
    {
        _workspace = workspace;
    }

    /// <summary>
    /// Appends a <see cref="ChapterEdit"/> to the edit list for its chapter and persists immediately.
    /// </summary>
    public void Add(ChapterEdit edit)
    {
        ArgumentNullException.ThrowIfNull(edit);

        lock (_lock)
        {
            EnsureLoaded();
            if (!_edits!.TryGetValue(edit.ChapterStem, out var list))
            {
                list = new List<ChapterEdit>();
                _edits[edit.ChapterStem] = list;
            }

            list.Add(edit);
            Save();
        }
    }

    /// <summary>
    /// Removes a <see cref="ChapterEdit"/> by its ID and persists immediately.
    /// Used when reverting an edit — the record is removed rather than mutated.
    /// </summary>
    /// <returns>True if the edit was found and removed.</returns>
    public bool Remove(string editId)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(editId);

        lock (_lock)
        {
            EnsureLoaded();
            foreach (var list in _edits!.Values)
            {
                var index = list.FindIndex(e => e.Id == editId);
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
    /// Returns all edits for a specific chapter, sorted by <see cref="ChapterEdit.BaselineStartSec"/>
    /// (front-to-back in the original timeline). This ordering is required by
    /// <see cref="TimelineProjection.BaselineToCurrentTime"/> which walks edits sequentially.
    /// </summary>
    public IReadOnlyList<ChapterEdit> GetEdits(string chapterStem)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(chapterStem);

        lock (_lock)
        {
            EnsureLoaded();
            if (!_edits!.TryGetValue(chapterStem, out var list))
                return Array.Empty<ChapterEdit>();

            return list
                .OrderBy(e => e.BaselineStartSec)
                .ToList()
                .AsReadOnly();
        }
    }

    /// <summary>
    /// Returns all edits across all chapters.
    /// </summary>
    public IReadOnlyList<ChapterEdit> GetAllEdits()
    {
        lock (_lock)
        {
            EnsureLoaded();
            return _edits!.Values
                .SelectMany(list => list)
                .OrderBy(e => e.BaselineStartSec)
                .ToList()
                .AsReadOnly();
        }
    }

    /// <summary>
    /// Removes all edits for a specific chapter and persists immediately.
    /// </summary>
    public void Clear(string chapterStem)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(chapterStem);

        lock (_lock)
        {
            EnsureLoaded();
            if (_edits!.Remove(chapterStem))
            {
                Save();
            }
        }
    }

    /// <summary>
    /// Returns true if the specified chapter has any edits in the list.
    /// </summary>
    public bool HasEdits(string chapterStem)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(chapterStem);

        lock (_lock)
        {
            EnsureLoaded();
            return _edits!.TryGetValue(chapterStem, out var list) && list.Count > 0;
        }
    }

    private string GetFilePath()
    {
        var workDir = _workspace.WorkingDirectory
                      ?? throw new InvalidOperationException("Workspace not initialized.");
        return Path.Combine(workDir, ".polish", "edit-list.json");
    }

    private void EnsureLoaded()
    {
        if (_edits != null) return;

        _edits = new Dictionary<string, List<ChapterEdit>>(StringComparer.OrdinalIgnoreCase);

        try
        {
            var path = GetFilePath();
            if (!File.Exists(path)) return;

            var json = File.ReadAllText(path);
            var deserialized = JsonSerializer.Deserialize<Dictionary<string, List<ChapterEdit>>>(json, JsonOptions);
            if (deserialized != null)
            {
                foreach (var (key, value) in deserialized)
                {
                    _edits[key] = value;
                }
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Failed to load edit list: {ex.Message}");
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

            var json = JsonSerializer.Serialize(_edits, JsonOptions);
            File.WriteAllText(path, json);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Failed to save edit list: {ex.Message}");
        }
    }
}
