using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using Ams.Core.Common;

namespace Ams.Workstation.Server.Services;

/// <summary>
/// Tracks which chapters have been reviewed by the user.
/// Persists reviewed status per book to %LOCALAPPDATA%\AMS\workstation\reviewed-status.json.
/// Singleton — shared across all Blazor circuits.
/// </summary>
public class ReviewedStatusService
{
    private static readonly string BasePath = AmsAppDataPaths.Resolve("workstation");

    private readonly BlazorWorkspace _workspace;
    private Dictionary<string, ReviewedEntry> _status = new();
    private string? _currentBookId;

    public ReviewedStatusService(BlazorWorkspace workspace)
    {
        _workspace = workspace;
    }

    public IReadOnlyDictionary<string, ReviewedEntry> GetAll()
    {
        EnsureLoaded();
        return _status;
    }

    public bool IsReviewed(string chapterName)
    {
        EnsureLoaded();
        return _status.TryGetValue(chapterName, out var entry) && entry.Reviewed;
    }

    public void SetReviewed(string chapterName, bool reviewed)
    {
        EnsureLoaded();
        _status[chapterName] = new ReviewedEntry(reviewed, DateTime.UtcNow);
        Save();
    }

    public void ResetAll()
    {
        ResetCurrentBook();
    }

    public void ResetCurrentBook()
    {
        EnsureLoaded();
        _status.Clear();
        Save();
    }

    private void EnsureLoaded()
    {
        var bookId = GetCurrentBookId();
        if (bookId != _currentBookId)
        {
            _currentBookId = bookId;
            Load();
        }
    }

    private string GetCurrentBookId()
    {
        if (!_workspace.IsInitialized) return "";
        return Path.GetFileName(_workspace.RootPath.TrimEnd(Path.DirectorySeparatorChar));
    }

    private string GetFilePath() => Path.Combine(BasePath, "reviewed-status.json");

    private void Load()
    {
        _status.Clear();
        var path = GetFilePath();
        if (!File.Exists(path)) return;

        try
        {
            var json = File.ReadAllText(path);
            using var doc = JsonDocument.Parse(json);
            if (doc.RootElement.TryGetProperty(_currentBookId ?? "", out var bookElement))
            {
                foreach (var prop in bookElement.EnumerateObject())
                {
                    var reviewed = prop.Value.GetProperty("reviewed").GetBoolean();
                    var timestamp = prop.Value.GetProperty("timestamp").GetDateTime();
                    _status[prop.Name] = new ReviewedEntry(reviewed, timestamp);
                }
            }
        }
        catch { }
    }

    private void Save()
    {
        try
        {
            var dir = Path.GetDirectoryName(GetFilePath());
            if (!string.IsNullOrEmpty(dir) && !Directory.Exists(dir))
                Directory.CreateDirectory(dir);

            // Load existing file to preserve other books
            var allBooks = new Dictionary<string, Dictionary<string, ReviewedEntry>>();
            var path = GetFilePath();
            if (File.Exists(path))
            {
                var existing = File.ReadAllText(path);
                allBooks = JsonSerializer.Deserialize<Dictionary<string, Dictionary<string, ReviewedEntry>>>(existing)
                           ?? new();
            }

            if (!string.IsNullOrEmpty(_currentBookId))
            {
                if (_status.Count == 0)
                {
                    allBooks.Remove(_currentBookId);
                }
                else
                {
                    allBooks[_currentBookId] = new Dictionary<string, ReviewedEntry>(_status);
                }
            }

            var json = JsonSerializer.Serialize(allBooks, new JsonSerializerOptions { WriteIndented = true });
            File.WriteAllText(path, json);
        }
        catch { }
    }
}

public record ReviewedEntry(bool Reviewed, DateTime Timestamp);
