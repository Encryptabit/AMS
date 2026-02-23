using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;

namespace Ams.Workstation.Server.Services;

/// <summary>
/// Tracks which error patterns are ignored (acceptable variations).
/// Persists ignored pattern keys per book to %LOCALAPPDATA%\AMS\workstation\ignored-patterns.json.
/// Singleton — shared across all Blazor circuits.
/// </summary>
public class IgnoredPatternsService
{
    private static readonly string BasePath = Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
        "AMS", "workstation");

    private readonly BlazorWorkspace _workspace;
    private HashSet<string> _ignoredKeys = new();
    private string? _currentBookId;

    public IgnoredPatternsService(BlazorWorkspace workspace)
    {
        _workspace = workspace;
    }

    public IReadOnlySet<string> GetIgnoredKeys()
    {
        EnsureLoaded();
        return _ignoredKeys;
    }

    public bool IsIgnored(string key)
    {
        EnsureLoaded();
        return _ignoredKeys.Contains(key);
    }

    public void SetIgnored(string key, bool ignored)
    {
        EnsureLoaded();
        if (ignored)
            _ignoredKeys.Add(key);
        else
            _ignoredKeys.Remove(key);
        Save();
    }

    public bool ToggleIgnored(string key)
    {
        EnsureLoaded();
        if (_ignoredKeys.Contains(key))
        {
            _ignoredKeys.Remove(key);
            Save();
            return false;
        }
        else
        {
            _ignoredKeys.Add(key);
            Save();
            return true;
        }
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

    private string GetFilePath() => Path.Combine(BasePath, "ignored-patterns.json");

    private void Load()
    {
        _ignoredKeys.Clear();
        var path = GetFilePath();
        if (!File.Exists(path)) return;

        try
        {
            var json = File.ReadAllText(path);
            using var doc = JsonDocument.Parse(json);
            if (doc.RootElement.TryGetProperty(_currentBookId ?? "", out var bookElement))
            {
                foreach (var item in bookElement.EnumerateArray())
                {
                    _ignoredKeys.Add(item.GetString() ?? "");
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

            var allBooks = new Dictionary<string, List<string>>();
            var path = GetFilePath();
            if (File.Exists(path))
            {
                var existing = File.ReadAllText(path);
                allBooks = JsonSerializer.Deserialize<Dictionary<string, List<string>>>(existing)
                           ?? new();
            }

            if (!string.IsNullOrEmpty(_currentBookId))
            {
                allBooks[_currentBookId] = _ignoredKeys.ToList();
            }

            var json = JsonSerializer.Serialize(allBooks, new JsonSerializerOptions { WriteIndented = true });
            File.WriteAllText(path, json);
        }
        catch { }
    }
}
