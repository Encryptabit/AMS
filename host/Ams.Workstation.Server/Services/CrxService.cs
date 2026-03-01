using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text.RegularExpressions;
using System.Text.Json;
using Ams.Workstation.Server.Models;
using ClosedXML.Excel;
using System.Xml.Linq;

namespace Ams.Workstation.Server.Services;

/// <summary>
/// CRX (correction) tracking service. Exports audio segments and records
/// error entries in an Excel file (.xlsx) for the current book.
/// Uses ClosedXML to read/write Excel, matching the Python validation-viewer
/// column layout from BASE_CRX.xlsx template.
/// </summary>
public class CrxService
{
    private const string CrxTemplatePath = @"C:\Aethon\BASE_CRX.xlsx";
    private const int CrxDataRowStart = 11;
    private static readonly JsonSerializerOptions JsonOptions = new() { WriteIndented = true };

    private readonly BlazorWorkspace _workspace;
    private readonly AudioExportService _audioExportService;

    public CrxService(BlazorWorkspace workspace, AudioExportService audioExportService)
    {
        _workspace = workspace;
        _audioExportService = audioExportService;
    }

    /// <summary>
    /// Submit a CRX entry: export audio and record metadata to JSON and Excel.
    /// </summary>
    public CrxSubmitResult Submit(string chapterName, CrxSubmitRequest request)
    {
        try
        {
            // Ensure Excel template/workbook is ready before exporting audio
            // to avoid orphan WAV files if the template is missing
            EnsureExcelReady();

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

            var jsonWritten = false;
            try
            {
                // JSON is the runtime source of truth for CRX pickup resolution.
                AppendOrUpdateJsonEntry(entry);
                jsonWritten = true;
                AppendCrxEntry(entry);
            }
            catch
            {
                if (jsonWritten)
                    TryRemoveJsonEntry(entry.ErrorNumber);

                // Keep artifacts in sync on submit failure.
                TryDeleteExportedFile(exportResult.Path);
                throw;
            }

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

    /// <summary>
    /// Read all CRX entries from the CRX JSON artifact.
    /// If JSON is missing but legacy Excel entries exist, seed JSON once from Excel.
    /// </summary>
    public IReadOnlyList<CrxEntry> GetEntries()
    {
        EnsureJsonSeededFromExcel();
        return TryReadJsonEntries();
    }

    private IReadOnlyList<CrxEntry> TryReadExcelEntries()
    {
        var path = GetCrxExcelPath(createDir: false);
        if (!File.Exists(path))
            return Array.Empty<CrxEntry>();

        try
        {
            using var workbook = new XLWorkbook(path);
            var worksheet = workbook.Worksheets.First();
            var entries = new List<CrxEntry>();

            for (var row = CrxDataRowStart; ; row++)
            {
                var errorNumCell = worksheet.Cell(row, 2).GetString();
                if (string.IsNullOrWhiteSpace(errorNumCell))
                    break;

                var errorNumber = int.TryParse(errorNumCell, out var num) ? num : 0;
                var chapter = worksheet.Cell(row, 4).GetString();
                var timecode = worksheet.Cell(row, 6).GetString();
                var errorType = worksheet.Cell(row, 7).GetString();
                var comments = worksheet.Cell(row, 8).GetString();

                entries.Add(new CrxEntry(
                    ErrorNumber: errorNumber,
                    Chapter: chapter,
                    Timecode: timecode,
                    ErrorType: errorType,
                    Comments: comments,
                    SentenceId: 0,
                    StartTime: 0.0,
                    EndTime: 0.0,
                    AudioFile: "",
                    CreatedAt: DateTime.MinValue
                ));
            }

            return entries;
        }
        catch
        {
            return TryReadExcelEntriesOpenXml(path);
        }
    }

    private static IReadOnlyList<CrxEntry> TryReadExcelEntriesOpenXml(string path)
    {
        try
        {
            using var stream = new FileStream(
                path,
                FileMode.Open,
                FileAccess.Read,
                FileShare.ReadWrite | FileShare.Delete);
            using var archive = new ZipArchive(stream, ZipArchiveMode.Read, leaveOpen: false);

            var shared = ReadSharedStrings(archive);
            var sheetEntry = archive.GetEntry("xl/worksheets/sheet1.xml");
            if (sheetEntry == null)
                return Array.Empty<CrxEntry>();

            using var wsStream = sheetEntry.Open();
            var doc = XDocument.Load(wsStream);
            XNamespace ns = "http://schemas.openxmlformats.org/spreadsheetml/2006/main";

            var entries = new List<CrxEntry>();
            var rows = doc.Descendants(ns + "row");
            foreach (var row in rows)
            {
                var rowNum = (int?)row.Attribute("r") ?? 0;
                if (rowNum < CrxDataRowStart)
                    continue;

                var valuesByCol = new Dictionary<int, string>();
                foreach (var cell in row.Elements(ns + "c"))
                {
                    var reference = (string?)cell.Attribute("r");
                    if (string.IsNullOrWhiteSpace(reference))
                        continue;

                    var col = ExtractColumnIndex(reference);
                    if (col <= 0)
                        continue;

                    valuesByCol[col] = GetCellText(cell, shared, ns);
                }

                var errorNumCell = valuesByCol.GetValueOrDefault(2, "").Trim();
                if (string.IsNullOrWhiteSpace(errorNumCell))
                    continue;

                var errorNumber = int.TryParse(errorNumCell, out var num) ? num : 0;
                var chapter = valuesByCol.GetValueOrDefault(4, "").Trim();
                var timecode = valuesByCol.GetValueOrDefault(6, "").Trim();
                var errorType = valuesByCol.GetValueOrDefault(7, "").Trim();
                var comments = valuesByCol.GetValueOrDefault(8, "").Trim();

                entries.Add(new CrxEntry(
                    ErrorNumber: errorNumber,
                    Chapter: chapter,
                    Timecode: timecode,
                    ErrorType: errorType,
                    Comments: comments,
                    SentenceId: 0,
                    StartTime: 0.0,
                    EndTime: 0.0,
                    AudioFile: "",
                    CreatedAt: DateTime.MinValue));
            }

            return entries;
        }
        catch
        {
            return Array.Empty<CrxEntry>();
        }
    }

    private static List<string> ReadSharedStrings(ZipArchive archive)
    {
        var entry = archive.GetEntry("xl/sharedStrings.xml");
        if (entry == null)
            return new List<string>();

        using var stream = entry.Open();
        var doc = XDocument.Load(stream);
        XNamespace ns = "http://schemas.openxmlformats.org/spreadsheetml/2006/main";

        return doc.Descendants(ns + "si")
            .Select(si => string.Concat(si.Descendants(ns + "t").Select(t => (string?)t ?? "")))
            .ToList();
    }

    private static int ExtractColumnIndex(string cellReference)
    {
        var match = Regex.Match(cellReference, "^[A-Z]+", RegexOptions.IgnoreCase);
        if (!match.Success)
            return 0;

        var letters = match.Value.ToUpperInvariant();
        var result = 0;
        foreach (var ch in letters)
        {
            result = (result * 26) + (ch - 'A' + 1);
        }

        return result;
    }

    private static string GetCellText(XElement cell, IReadOnlyList<string> shared, XNamespace ns)
    {
        var type = ((string?)cell.Attribute("t")) ?? "";
        if (type == "inlineStr")
        {
            return string.Concat(cell.Descendants(ns + "t").Select(t => (string?)t ?? ""));
        }

        var value = (string?)cell.Element(ns + "v") ?? "";
        if (type == "s")
        {
            if (int.TryParse(value, out var index) && index >= 0 && index < shared.Count)
                return shared[index];
            return "";
        }

        return value;
    }

    private IReadOnlyList<CrxEntry> TryReadJsonEntries()
    {
        var path = GetCrxJsonPath(createDir: false);
        if (!File.Exists(path))
            return Array.Empty<CrxEntry>();

        try
        {
            var json = File.ReadAllText(path);
            var entries = JsonSerializer.Deserialize<List<CrxEntry>>(json);
            if (entries is { Count: > 0 })
                return entries.OrderBy(e => e.ErrorNumber).ToList();

            return Array.Empty<CrxEntry>();
        }
        catch
        {
            return Array.Empty<CrxEntry>();
        }
    }

    private void EnsureJsonSeededFromExcel()
    {
        var jsonEntries = TryReadJsonEntries();
        if (jsonEntries.Count > 0)
            return;

        var excelEntries = TryReadExcelEntries();
        if (excelEntries.Count == 0)
            return;

        var seededAt = DateTime.UtcNow;
        var seeded = excelEntries
            .OrderBy(e => e.ErrorNumber)
            .Select(e => BuildSeededLegacyEntry(e, seededAt))
            .ToList();

        WriteJsonEntries(seeded);
    }

    private CrxEntry BuildSeededLegacyEntry(CrxEntry excelEntry, DateTime seededAt)
    {
        var seeded = excelEntry with
        {
            AudioFile = ResolveAudioFileForError(excelEntry.ErrorNumber),
            CreatedAt = seededAt
        };

        if (!_workspace.IsInitialized || !_workspace.HasBookIndex)
            return seeded;

        var chapterName = ResolveWorkspaceChapterName(seeded.Chapter);
        if (string.IsNullOrWhiteSpace(chapterName))
            return seeded;

        if (!_workspace.TryGetHydratedTranscript(chapterName, out var hydrated) || hydrated == null)
            return seeded with { Chapter = chapterName };

        var sentences = hydrated.Sentences;
        if (sentences.Count == 0)
            return seeded with { Chapter = chapterName };

        if (seeded.SentenceId > 0)
        {
            var direct = sentences.FirstOrDefault(s => s.Id == seeded.SentenceId);
            if (direct?.Timing != null && direct.Timing.EndSec > direct.Timing.StartSec)
            {
                return seeded with
                {
                    Chapter = chapterName,
                    StartTime = direct.Timing.StartSec,
                    EndTime = direct.Timing.EndSec
                };
            }
        }

        var timeSec = TryParseTimecode(seeded.Timecode);
        if (!timeSec.HasValue)
            return seeded with { Chapter = chapterName };

        var nearest = sentences
            .Where(s => s.Timing != null)
            .OrderBy(s => DistanceToSentenceCenter(s, timeSec.Value))
            .FirstOrDefault();

        if (nearest?.Timing == null || nearest.Timing.EndSec <= nearest.Timing.StartSec)
            return seeded with { Chapter = chapterName };

        return seeded with
        {
            Chapter = chapterName,
            SentenceId = nearest.Id,
            StartTime = nearest.Timing.StartSec,
            EndTime = nearest.Timing.EndSec
        };
    }

    private string ResolveAudioFileForError(int errorNumber)
    {
        var fileName = $"{errorNumber:D3}.wav";
        var crxFolder = Path.Combine(_workspace.RootPath, "CRX");
        return File.Exists(Path.Combine(crxFolder, fileName)) ? fileName : string.Empty;
    }

    private string? ResolveWorkspaceChapterName(string? chapterLabel)
    {
        if (string.IsNullOrWhiteSpace(chapterLabel))
            return null;

        foreach (var chapterName in _workspace.AvailableChapters)
        {
            if (ChapterMatches(chapterLabel, chapterName))
                return chapterName;
        }

        return null;
    }

    private static bool ChapterMatches(string? left, string? right)
    {
        if (string.IsNullOrWhiteSpace(left) || string.IsNullOrWhiteSpace(right))
            return false;

        var leftNumber = TryExtractChapterNumber(left);
        var rightNumber = TryExtractChapterNumber(right);
        if (leftNumber.HasValue && rightNumber.HasValue)
            return leftNumber.Value == rightNumber.Value;

        return string.Equals(NormalizeForCompare(left), NormalizeForCompare(right), StringComparison.Ordinal);
    }

    private static int? TryExtractChapterNumber(string chapterLabel)
    {
        var match = Regex.Match(chapterLabel, @"\d+");
        if (!match.Success)
            return null;

        return int.TryParse(match.Value, out var value) ? value : null;
    }

    private static string NormalizeForCompare(string? text)
    {
        if (string.IsNullOrWhiteSpace(text))
            return string.Empty;

        return Regex.Replace(
            Regex.Replace(text.ToLowerInvariant().Trim(), @"[^\w\s]", " "),
            @"\s+", " ").Trim();
    }

    private static double? TryParseTimecode(string? timecode)
    {
        if (string.IsNullOrWhiteSpace(timecode))
            return null;

        if (TimeSpan.TryParse(timecode, out var ts))
            return ts.TotalSeconds;

        return null;
    }

    private static double DistanceToSentenceCenter(dynamic sentence, double timeSec)
    {
        if (sentence.Timing == null)
            return double.MaxValue;

        var center = (sentence.Timing.StartSec + sentence.Timing.EndSec) / 2.0;
        return Math.Abs(center - timeSec);
    }

    private void AppendOrUpdateJsonEntry(CrxEntry entry)
    {
        var entries = TryReadJsonEntries().ToList();
        var existingIndex = entries.FindIndex(e => e.ErrorNumber == entry.ErrorNumber);
        if (existingIndex >= 0)
            entries[existingIndex] = entry;
        else
            entries.Add(entry);

        entries = entries
            .OrderBy(e => e.ErrorNumber)
            .ToList();

        WriteJsonEntries(entries);
    }

    private void TryRemoveJsonEntry(int errorNumber)
    {
        try
        {
            var path = GetCrxJsonPath(createDir: false);
            if (!File.Exists(path))
                return;

            var entries = TryReadJsonEntries()
                .Where(e => e.ErrorNumber != errorNumber)
                .OrderBy(e => e.ErrorNumber)
                .ToList();

            WriteJsonEntries(entries);
        }
        catch
        {
            // best effort rollback
        }
    }

    private void WriteJsonEntries(IReadOnlyList<CrxEntry> entries)
    {
        var path = GetCrxJsonPath();
        var tempPath = $"{path}.{Guid.NewGuid():N}.tmp";
        var payload = JsonSerializer.Serialize(entries, JsonOptions);

        File.WriteAllText(tempPath, payload);

        if (File.Exists(path))
            File.Replace(tempPath, path, destinationBackupFileName: null);
        else
            File.Move(tempPath, path);
    }

    /// <summary>
    /// Append a CRX entry to the Excel workbook.
    /// On first use, copies the BASE_CRX.xlsx template to preserve formatting.
    /// </summary>
    private void AppendCrxEntry(CrxEntry entry)
    {
        var excelPath = GetCrxExcelPath();

        // Copy template on first use if target does not exist
        if (!File.Exists(excelPath))
        {
            if (!File.Exists(CrxTemplatePath))
                throw new FileNotFoundException(
                    $"CRX template not found at {CrxTemplatePath}");

            File.Copy(CrxTemplatePath, excelPath);
        }

        // Write entry to Excel
        using var workbook = new XLWorkbook(excelPath);
        var worksheet = workbook.Worksheets.First();

        var targetRow = CrxDataRowStart + (entry.ErrorNumber - 1);

        // Column B (2): Error number as 3-digit string
        worksheet.Cell(targetRow, 2).Value = $"{entry.ErrorNumber:D3}";
        // Column C (3): Recording Day - leave blank (same as Python)
        // Column D (4): Chapter name
        worksheet.Cell(targetRow, 4).Value = entry.Chapter;
        // Column E (5): PDF/Word Page # - leave blank (same as Python)
        // Column F (6): Timecode in HH:MM:SS
        worksheet.Cell(targetRow, 6).Value = entry.Timecode;
        // Column G (7): Error type
        worksheet.Cell(targetRow, 7).Value = entry.ErrorType;
        // Column H (8): Comments
        worksheet.Cell(targetRow, 8).Value = entry.Comments;

        workbook.Save();
    }

    /// <summary>
    /// Validate that the Excel workbook is ready (template exists or file already created).
    /// Called before audio export to fail fast and avoid orphan WAV files.
    /// </summary>
    private void EnsureExcelReady()
    {
        var excelPath = GetCrxExcelPath();
        if (!File.Exists(excelPath) && !File.Exists(CrxTemplatePath))
            throw new FileNotFoundException(
                $"CRX template not found at {CrxTemplatePath}");
    }

    private static void TryDeleteExportedFile(string path)
    {
        try { if (File.Exists(path)) File.Delete(path); }
        catch { /* best-effort cleanup */ }
    }

    /// <summary>
    /// Get the path to the CRX Excel file for the current book.
    /// </summary>
    private string GetCrxExcelPath(bool createDir = true)
    {
        var crxFolder = Path.Combine(_workspace.RootPath, "CRX");
        if (createDir)
            Directory.CreateDirectory(crxFolder);
        var bookName = Path.GetFileName(_workspace.RootPath.TrimEnd(Path.DirectorySeparatorChar));
        return Path.Combine(crxFolder, $"{bookName}_CRX.xlsx");
    }

    private string GetCrxJsonPath(bool createDir = true)
    {
        var crxFolder = Path.Combine(_workspace.RootPath, "CRX");
        if (createDir)
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
