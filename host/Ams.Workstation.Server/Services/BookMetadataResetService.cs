using System;
using System.IO;
using System.Text.Json;
using System.Text.Json.Nodes;
using Ams.Core.Common;

namespace Ams.Workstation.Server.Services;

public sealed class BookMetadataResetService
{
    private static readonly JsonSerializerOptions JsonOptions = new() { WriteIndented = true };
    private static readonly StringComparer PathComparer = OperatingSystem.IsWindows()
        ? StringComparer.OrdinalIgnoreCase
        : StringComparer.Ordinal;

    private readonly BlazorWorkspace _workspace;
    private readonly ReviewedStatusService _reviewedStatus;
    private readonly IgnoredPatternsService _ignoredPatterns;
    private readonly StagingQueueService _stagingQueue;
    private readonly PreviewBufferService _previewBuffer;

    public BookMetadataResetService(
        BlazorWorkspace workspace,
        ReviewedStatusService reviewedStatus,
        IgnoredPatternsService ignoredPatterns,
        StagingQueueService stagingQueue,
        PreviewBufferService previewBuffer)
    {
        _workspace = workspace;
        _reviewedStatus = reviewedStatus;
        _ignoredPatterns = ignoredPatterns;
        _stagingQueue = stagingQueue;
        _previewBuffer = previewBuffer;
    }

    public MetadataResetResult ResetCurrentBook()
    {
        if (!_workspace.IsInitialized || string.IsNullOrWhiteSpace(_workspace.WorkingDirectory))
        {
            return new MetadataResetResult(
                Success: false,
                BookId: null,
                ReviewedEntriesCleared: 0,
                IgnoredPatternsCleared: 0,
                BookScopedFilesTouched: 0,
                ClearedCurrentChapterState: false,
                Message: "Set a workspace before resetting metadata.");
        }

        var workingDirectory = _workspace.WorkingDirectory!;
        var bookId = Path.GetFileName(workingDirectory.TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar));

        var reviewedCount = _reviewedStatus.GetAll().Count;
        var ignoredCount = _ignoredPatterns.GetIgnoredKeys().Count;

        _reviewedStatus.ResetCurrentBook();
        _ignoredPatterns.ResetCurrentBook();

        // Clear polish state: staging queue (in-memory + on-disk) and preview buffer
        _stagingQueue.ClearAll();
        _previewBuffer.Clear();
        var clearedPolishDir = ClearPolishDirectory(workingDirectory);

        var touchedFiles = RemoveBookScopedEntries(bookId);
        var clearedCurrentChapter = ClearCurrentChapterState(workingDirectory);

        var message = touchedFiles > 0 || clearedCurrentChapter || reviewedCount > 0 || ignoredCount > 0 || clearedPolishDir
            ? $"Reset metadata for '{bookId}': reviewed={reviewedCount}, ignored={ignoredCount}, files={touchedFiles}, polish cleared={clearedPolishDir}."
            : $"No metadata found to reset for '{bookId}'.";

        return new MetadataResetResult(
            Success: true,
            BookId: bookId,
            ReviewedEntriesCleared: reviewedCount,
            IgnoredPatternsCleared: ignoredCount,
            BookScopedFilesTouched: touchedFiles,
            ClearedCurrentChapterState: clearedCurrentChapter,
            Message: message);
    }

    private static int RemoveBookScopedEntries(string bookId)
    {
        if (string.IsNullOrWhiteSpace(bookId)) return 0;

        var workstationDir = AmsAppDataPaths.Resolve("workstation");
        if (!Directory.Exists(workstationDir)) return 0;

        var touched = 0;

        foreach (var path in Directory.EnumerateFiles(workstationDir, "*.json", SearchOption.TopDirectoryOnly))
        {
            try
            {
                var json = File.ReadAllText(path);
                if (string.IsNullOrWhiteSpace(json)) continue;

                if (JsonNode.Parse(json) is not JsonObject obj) continue;
                if (!obj.Remove(bookId)) continue;

                var rewritten = obj.ToJsonString(JsonOptions);
                File.WriteAllText(path, rewritten);
                touched++;
            }
            catch
            {
                // Best-effort cleanup only.
            }
        }

        return touched;
    }

    /// <summary>
    /// Removes the .polish/ directory (staging queue JSON + undo backups) from the workspace.
    /// </summary>
    private static bool ClearPolishDirectory(string workingDirectory)
    {
        var polishDir = Path.Combine(workingDirectory, ".polish");
        if (!Directory.Exists(polishDir)) return false;

        try
        {
            Directory.Delete(polishDir, recursive: true);
            return true;
        }
        catch
        {
            return false;
        }
    }

    private static bool ClearCurrentChapterState(string workingDirectory)
    {
        var statePath = AmsAppDataPaths.Resolve("workstation-state.json");
        if (!File.Exists(statePath)) return false;

        try
        {
            var json = File.ReadAllText(statePath);
            if (string.IsNullOrWhiteSpace(json)) return false;

            if (JsonNode.Parse(json) is not JsonObject obj) return false;

            var persistedWorkingDir = obj["workingDirectory"]?.GetValue<string>();
            if (string.IsNullOrWhiteSpace(persistedWorkingDir)) return false;

            var normalizedCurrent = Path.GetFullPath(workingDirectory.Trim());
            var normalizedPersisted = Path.GetFullPath(persistedWorkingDir.Trim());
            if (!PathComparer.Equals(normalizedCurrent, normalizedPersisted)) return false;

            if (obj["currentChapter"] is null) return false;
            if (obj["currentChapter"]?.GetValue<string>() is not { Length: > 0 }) return false;

            obj["currentChapter"] = null;
            File.WriteAllText(statePath, obj.ToJsonString(JsonOptions));
            return true;
        }
        catch
        {
            return false;
        }
    }
}

public sealed record MetadataResetResult(
    bool Success,
    string? BookId,
    int ReviewedEntriesCleared,
    int IgnoredPatternsCleared,
    int BookScopedFilesTouched,
    bool ClearedCurrentChapterState,
    string Message);
