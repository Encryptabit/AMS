using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using Ams.Core.Artifacts;
using Ams.Core.Processors;
using Ams.Workstation.Server.Models;

namespace Ams.Workstation.Server.Services;

/// <summary>
/// Saves original audio segments to disk before replacement application and can restore them.
/// Uses versioned segment files on disk with a JSON manifest per chapter.
/// Persists undo data to {workDir}/.polish-undo/{chapterStem}/.
/// Singleton -- shared across all Blazor circuits.
/// </summary>
public class UndoService
{
    private static readonly JsonSerializerOptions JsonOptions = new() { WriteIndented = true };

    private readonly BlazorWorkspace _workspace;
    private readonly object _lock = new();
    private readonly Dictionary<string, List<UndoRecord>> _records = new(StringComparer.OrdinalIgnoreCase);
    private readonly HashSet<string> _loadedChapters = new(StringComparer.OrdinalIgnoreCase);

    public UndoService(BlazorWorkspace workspace)
    {
        _workspace = workspace;
    }

    /// <summary>
    /// Saves the original audio segment before a replacement is applied.
    /// Trims the segment from <paramref name="startSec"/> to <paramref name="endSec"/>,
    /// encodes as WAV, and creates an <see cref="UndoRecord"/>.
    /// </summary>
    /// <param name="chapterStem">The chapter stem identifier.</param>
    /// <param name="sentenceId">The sentence being replaced.</param>
    /// <param name="replacementId">The ID of the StagedReplacement being applied.</param>
    /// <param name="originalBuffer">The full chapter audio buffer.</param>
    /// <param name="startSec">Start time of the segment to back up.</param>
    /// <param name="endSec">End time of the segment to back up.</param>
    /// <param name="replacementDurationSec">Duration of the replacement audio (for shift tracking).</param>
    /// <returns>The created UndoRecord.</returns>
    public UndoRecord SaveOriginalSegment(
        string chapterStem,
        int sentenceId,
        string replacementId,
        AudioBuffer originalBuffer,
        double startSec,
        double endSec,
        double replacementDurationSec)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(chapterStem);
        ArgumentException.ThrowIfNullOrWhiteSpace(replacementId);
        ArgumentNullException.ThrowIfNull(originalBuffer);

        lock (_lock)
        {
            EnsureLoaded(chapterStem);

            // Determine next version number for this sentence
            var version = GetNextVersion(chapterStem, sentenceId);

            // Build backup file path
            var dir = GetChapterUndoDir(chapterStem);
            var fileName = $"sent{sentenceId}.v{version}.original.wav";
            var filePath = Path.Combine(dir, fileName);

            // Trim the original segment and encode to disk
            var segment = AudioProcessor.Trim(
                originalBuffer,
                TimeSpan.FromSeconds(startSec),
                TimeSpan.FromSeconds(endSec));

            AudioProcessor.EncodeWav(filePath, segment);

            // Create the undo record
            var record = new UndoRecord(
                ReplacementId: replacementId,
                ChapterStem: chapterStem,
                SentenceId: sentenceId,
                OriginalSegmentPath: filePath,
                OriginalStartSec: startSec,
                OriginalEndSec: endSec,
                OriginalDurationSec: endSec - startSec,
                ReplacementDurationSec: replacementDurationSec,
                AppliedAtUtc: DateTime.UtcNow);

            if (!_records.TryGetValue(chapterStem, out var list))
            {
                list = new List<UndoRecord>();
                _records[chapterStem] = list;
            }

            list.Add(record);
            SaveManifest(chapterStem);

            return record;
        }
    }

    /// <summary>
    /// Returns all undo records for a chapter.
    /// </summary>
    public IReadOnlyList<UndoRecord> GetUndoRecords(string chapterStem)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(chapterStem);

        lock (_lock)
        {
            EnsureLoaded(chapterStem);
            return _records.TryGetValue(chapterStem, out var list)
                ? list.AsReadOnly()
                : Array.Empty<UndoRecord>();
        }
    }

    /// <summary>
    /// Returns a single undo record by replacement ID, or null if not found.
    /// </summary>
    public UndoRecord? GetUndoRecord(string replacementId)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(replacementId);

        lock (_lock)
        {
            foreach (var list in _records.Values)
            {
                var record = list.FirstOrDefault(r => r.ReplacementId == replacementId);
                if (record != null) return record;
            }

            return null;
        }
    }

    /// <summary>
    /// Decodes the backup WAV file back to an AudioBuffer.
    /// </summary>
    /// <returns>The original audio segment, or null if the backup file is missing.</returns>
    public AudioBuffer? LoadOriginalSegment(string replacementId)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(replacementId);

        lock (_lock)
        {
            var record = GetUndoRecordInternal(replacementId);
            if (record == null) return null;

            if (!File.Exists(record.OriginalSegmentPath))
            {
                Console.WriteLine($"Undo backup file missing: {record.OriginalSegmentPath}");
                return null;
            }

            return AudioProcessor.Decode(record.OriginalSegmentPath);
        }
    }

    /// <summary>
    /// Deletes the backup file and removes the undo record.
    /// </summary>
    /// <returns>True if the record was found and removed.</returns>
    public bool RemoveRecord(string replacementId)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(replacementId);

        lock (_lock)
        {
            foreach (var (chapterStem, list) in _records)
            {
                var index = list.FindIndex(r => r.ReplacementId == replacementId);
                if (index >= 0)
                {
                    var record = list[index];

                    // Delete backup file if it exists
                    try
                    {
                        if (File.Exists(record.OriginalSegmentPath))
                        {
                            File.Delete(record.OriginalSegmentPath);
                        }
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Failed to delete undo backup: {ex.Message}");
                    }

                    list.RemoveAt(index);
                    SaveManifest(chapterStem);
                    return true;
                }
            }

            return false;
        }
    }

    /// <summary>
    /// Checks if an undo record exists for the given replacement ID.
    /// </summary>
    public bool HasUndo(string replacementId)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(replacementId);

        lock (_lock)
        {
            return GetUndoRecordInternal(replacementId) != null;
        }
    }

    #region Private Helpers

    private string GetWorkDir()
    {
        return _workspace.WorkingDirectory
               ?? throw new InvalidOperationException("Workspace not initialized.");
    }

    private string GetChapterUndoDir(string chapterStem)
    {
        var dir = Path.Combine(GetWorkDir(), ".polish-undo", chapterStem);
        if (!Directory.Exists(dir))
        {
            Directory.CreateDirectory(dir);
        }

        return dir;
    }

    private string GetManifestPath(string chapterStem)
    {
        return Path.Combine(GetChapterUndoDir(chapterStem), "manifest.json");
    }

    private int GetNextVersion(string chapterStem, int sentenceId)
    {
        if (!_records.TryGetValue(chapterStem, out var list))
            return 1;

        var maxVersion = list
            .Where(r => r.SentenceId == sentenceId)
            .Count();

        // Also scan existing files to determine next version (in case of manifest/file mismatch)
        var dir = GetChapterUndoDir(chapterStem);
        var pattern = $"sent{sentenceId}.v*.original.wav";
        var existingFiles = Directory.GetFiles(dir, pattern);
        var maxFileVersion = 0;

        foreach (var file in existingFiles)
        {
            var fileName = Path.GetFileName(file);
            // Parse version from "sent5.v3.original.wav"
            var parts = fileName.Split('.');
            if (parts.Length >= 2 && parts[1].StartsWith('v') &&
                int.TryParse(parts[1].AsSpan(1), out var ver))
            {
                maxFileVersion = Math.Max(maxFileVersion, ver);
            }
        }

        return Math.Max(maxVersion, maxFileVersion) + 1;
    }

    private UndoRecord? GetUndoRecordInternal(string replacementId)
    {
        foreach (var list in _records.Values)
        {
            var record = list.FirstOrDefault(r => r.ReplacementId == replacementId);
            if (record != null) return record;
        }

        return null;
    }

    private void EnsureLoaded(string chapterStem)
    {
        if (_loadedChapters.Contains(chapterStem)) return;

        _loadedChapters.Add(chapterStem);

        try
        {
            var manifestPath = GetManifestPath(chapterStem);
            if (!File.Exists(manifestPath)) return;

            var json = File.ReadAllText(manifestPath);
            var records = JsonSerializer.Deserialize<List<UndoRecord>>(json, JsonOptions);
            if (records != null && records.Count > 0)
            {
                _records[chapterStem] = records;
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Failed to load undo manifest for '{chapterStem}': {ex.Message}");
        }
    }

    private void SaveManifest(string chapterStem)
    {
        try
        {
            var manifestPath = GetManifestPath(chapterStem);
            var records = _records.TryGetValue(chapterStem, out var list)
                ? list
                : new List<UndoRecord>();

            var json = JsonSerializer.Serialize(records, JsonOptions);
            File.WriteAllText(manifestPath, json);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Failed to save undo manifest for '{chapterStem}': {ex.Message}");
        }
    }

    #endregion
}
