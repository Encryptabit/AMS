using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using Ams.Core.Common;

namespace Ams.Workstation.Server.Services;

/// <summary>
/// Persists recently used workspace paths for quick switching in the header.
/// </summary>
public sealed class WorkspaceHistoryService
{
    private const int MaxEntries = 25;
    private static readonly JsonSerializerOptions JsonOptions = new() { WriteIndented = true };
    private static readonly StringComparer PathComparer = OperatingSystem.IsWindows()
        ? StringComparer.OrdinalIgnoreCase
        : StringComparer.Ordinal;

    private readonly object _lock = new();
    private List<string>? _entries;

    public IReadOnlyList<string> GetSavedWorkspaces(bool existingOnly = false)
    {
        lock (_lock)
        {
            EnsureLoaded();
            var entries = _entries ?? new List<string>();

            if (!existingOnly)
                return entries.ToList().AsReadOnly();

            var existing = entries.Where(Directory.Exists).ToList();
            if (existing.Count != entries.Count)
            {
                _entries = existing;
                Save();
            }

            return existing.AsReadOnly();
        }
    }

    public void RememberWorkspace(string path)
    {
        if (string.IsNullOrWhiteSpace(path)) return;

        var normalized = Path.GetFullPath(path.Trim());

        lock (_lock)
        {
            EnsureLoaded();
            _entries!.RemoveAll(p => PathComparer.Equals(p, normalized));
            _entries.Insert(0, normalized);

            if (_entries.Count > MaxEntries)
            {
                _entries.RemoveRange(MaxEntries, _entries.Count - MaxEntries);
            }

            Save();
        }
    }

    private void EnsureLoaded()
    {
        if (_entries is not null) return;

        _entries = new List<string>();
        var path = GetFilePath();
        if (!File.Exists(path)) return;

        try
        {
            var json = File.ReadAllText(path);
            var loaded = JsonSerializer.Deserialize<List<string>>(json) ?? new List<string>();

            foreach (var item in loaded)
            {
                if (string.IsNullOrWhiteSpace(item)) continue;
                var normalized = Path.GetFullPath(item.Trim());
                if (_entries.Any(existing => PathComparer.Equals(existing, normalized))) continue;
                _entries.Add(normalized);
            }
        }
        catch
        {
            // Ignore malformed history and continue with an empty list.
        }
    }

    private string GetFilePath() => AmsAppDataPaths.Resolve("workstation", "workspace-history.json");

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

            if (_entries is null || _entries.Count == 0)
            {
                if (File.Exists(path))
                {
                    File.Delete(path);
                }

                return;
            }

            var json = JsonSerializer.Serialize(_entries, JsonOptions);
            File.WriteAllText(path, json);
        }
        catch
        {
            // Best effort persistence only.
        }
    }
}
