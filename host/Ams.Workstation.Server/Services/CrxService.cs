using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Ams.Workstation.Server.Models;
using ClosedXML.Excel;

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

    private readonly BlazorWorkspace _workspace;
    private readonly AudioExportService _audioExportService;

    public CrxService(BlazorWorkspace workspace, AudioExportService audioExportService)
    {
        _workspace = workspace;
        _audioExportService = audioExportService;
    }

    /// <summary>
    /// Submit a CRX entry: export audio and record metadata to Excel.
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

    /// <summary>
    /// Read all CRX entries from the Excel file.
    /// </summary>
    public IReadOnlyList<CrxEntry> GetEntries()
    {
        var path = GetCrxExcelPath(createDir: false);
        if (!File.Exists(path)) return Array.Empty<CrxEntry>();

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
            return Array.Empty<CrxEntry>();
        }
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

    private static string FormatTimecode(double seconds)
    {
        var ts = TimeSpan.FromSeconds(seconds);
        return $"{(int)ts.TotalHours:D2}:{ts.Minutes:D2}:{ts.Seconds:D2}";
    }
}
