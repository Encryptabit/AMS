using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using Ams.Workstation.Server.Models;

namespace Ams.Workstation.Server.Services;

/// <summary>
/// CRX (correction) tracking service. Exports audio segments and records
/// error entries in a JSON file for the current book.
/// </summary>
public class CrxService
{
    private readonly BlazorWorkspace _workspace;
    private readonly AudioExportService _audioExportService;

    public CrxService(BlazorWorkspace workspace, AudioExportService audioExportService)
    {
        _workspace = workspace;
        _audioExportService = audioExportService;
    }

    /// <summary>
    /// Submit a CRX entry: export audio and record metadata.
    /// </summary>
    public CrxSubmitResult Submit(string chapterName, CrxSubmitRequest request)
    {
        try
        {
            var exportResult = _audioExportService.ExportSegment(
                request.Start,
                request.End,
                request.PaddingMs);

            var entry = new CrxEntry(
                ErrorNumber: exportResult.ErrorNumber,
                Chapter: chapterName,
                Timecode: FormatTimecode(request.Start),
                ErrorType: request.ErrorType,
                Comments: request.Comments,
                SentenceId: request.SentenceId,
                StartTime: request.Start,
                EndTime: request.End,
                AudioFile: exportResult.Filename,
                CreatedAt: DateTime.UtcNow
            );

            AppendCrxEntry(entry);

            return new CrxSubmitResult(
                Success: true,
                ErrorNumber: entry.ErrorNumber,
                Timecode: entry.Timecode,
                AudioFile: entry.AudioFile
            );
        }
        catch (Exception ex)
        {
            return new CrxSubmitResult(false, 0, "", "", ex.Message);
        }
    }

    public IReadOnlyList<CrxEntry> GetEntries()
    {
        var path = GetCrxJsonPath();
        if (!File.Exists(path)) return Array.Empty<CrxEntry>();

        try
        {
            var json = File.ReadAllText(path);
            return JsonSerializer.Deserialize<List<CrxEntry>>(json) ?? new();
        }
        catch
        {
            return Array.Empty<CrxEntry>();
        }
    }

    private void AppendCrxEntry(CrxEntry entry)
    {
        var entries = GetEntries().ToList();
        entries.Add(entry);

        var path = GetCrxJsonPath();
        var json = JsonSerializer.Serialize(entries, new JsonSerializerOptions { WriteIndented = true });
        File.WriteAllText(path, json);
    }

    private string GetCrxJsonPath()
    {
        var crxFolder = Path.Combine(_workspace.RootPath, "CRX");
        Directory.CreateDirectory(crxFolder);
        var bookName = Path.GetFileName(_workspace.RootPath.TrimEnd(Path.DirectorySeparatorChar));
        return Path.Combine(crxFolder, $"{bookName}_CRX.json");
    }

    private static string FormatTimecode(double seconds)
    {
        var ts = TimeSpan.FromSeconds(seconds);
        return $"{(int)ts.TotalHours:D2}:{ts.Minutes:D2}:{ts.Seconds:D2}";
    }
}
