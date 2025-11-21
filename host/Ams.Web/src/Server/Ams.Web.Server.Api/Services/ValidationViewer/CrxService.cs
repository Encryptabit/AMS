using Ams.Core.Artifacts;
using Ams.Web.Shared.ValidationViewer;
using Ams.Web.Server.Api.Models.ValidationViewer;
using DocumentFormat.OpenXml;
using Microsoft.Extensions.Options;
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Spreadsheet;

namespace Ams.Web.Server.Api.Services.ValidationViewer;

internal sealed class CrxService : ICrxService
{
    private readonly ValidationViewerWorkspaceState _state;
    private readonly IAudioStreamService _audio;
    private readonly WorkspaceResolver _resolver;
    private readonly ILogger<CrxService> _logger;
    private readonly IOptions<ValidationViewerOptions> _options;

    public CrxService(ValidationViewerWorkspaceState state, WorkspaceResolver resolver, IAudioStreamService audio, ILogger<CrxService> logger, IOptions<ValidationViewerOptions> options)
    {
        _state = state;
        _resolver = resolver;
        _audio = audio;
        _logger = logger;
        _options = options;
    }

    public async Task<object> ExportAsync(string bookId, string chapterId, double start, double end, CancellationToken ct = default)
    {
        var buffer = _audio.LoadBuffer(bookId, chapterId, "treated") ?? _audio.LoadBuffer(bookId, chapterId, "raw");
        if (buffer is null)
        {
            return new { error = "Audio buffer not found" };
        }

        var slice = _audio.Slice(buffer, start, end);
        var bookRoot = _resolver.ResolveBookRoot(bookId).FullName;
        var crxDir = new DirectoryInfo(Path.Combine(bookRoot, _state.CrxDirectoryName));
        crxDir.Create();

        var errorNum = NextErrorNumber(crxDir);
        var exportFile = new FileInfo(Path.Combine(crxDir.FullName, $"{errorNum:000}.wav"));
        await using var wav = _audio.ToWavStream(slice);
        await using var fs = exportFile.Open(FileMode.Create, FileAccess.Write, FileShare.Read);
        wav.Position = 0;
        await wav.CopyToAsync(fs, ct);

        return new
        {
            success = true,
            filename = exportFile.Name,
            errorNumber = errorNum,
            path = Path.GetRelativePath(bookRoot, exportFile.FullName)
        };
    }

    public async Task<object> AddToCrxAsync(string bookId, string chapterId, JsonElement payload, CancellationToken ct = default)
    {
        var start = payload.TryGetProperty("start", out var sVal) ? sVal.GetDouble() : 0;
        var end = payload.TryGetProperty("end", out var eVal) ? eVal.GetDouble() : start;
        var errorType = payload.TryGetProperty("errorType", out var etVal) ? etVal.GetString() : _state.DefaultErrorType;
        var comments = payload.TryGetProperty("comments", out var cVal) ? cVal.GetString() : string.Empty;
        var paddingMs = payload.TryGetProperty("paddingMs", out var pVal) ? pVal.GetInt32() : 50;

        var bookRoot = _resolver.ResolveBookRoot(bookId);
        var crxDir = new DirectoryInfo(Path.Combine(bookRoot.FullName, _state.CrxDirectoryName));
        crxDir.Create();

        var errorNum = NextErrorNumber(crxDir);
        var crxFile = new FileInfo(Path.Combine(crxDir.FullName, $"{bookRoot.Name}_CRX.xlsx"));

        await EnsureCrxTemplateAsync(crxFile);
        UpdateCrxWorkbook(crxFile, errorNum, chapterId, start, errorType ?? _state.DefaultErrorType, comments ?? string.Empty);

        var buffer = _audio.LoadBuffer(bookId, chapterId, "treated") ?? _audio.LoadBuffer(bookId, chapterId, "raw");
        if (buffer is not null)
        {
            var slice = _audio.Slice(buffer, start, end + paddingMs / 1000d);
            var exportFile = new FileInfo(Path.Combine(crxDir.FullName, $"{errorNum:000}.wav"));
            await using var wav = _audio.ToWavStream(slice);
            await using var fs = exportFile.Open(FileMode.Create, FileAccess.Write, FileShare.Read);
            wav.Position = 0;
            await wav.CopyToAsync(fs, ct);
        }

        return new
        {
            success = true,
            errorNumber = errorNum,
            crxFile = crxFile.Name,
            timecode = TimeSpan.FromSeconds(start).ToString("hh\\:mm\\:ss"),
            audioFile = $"{errorNum:000}.wav"
        };
    }

    private int NextErrorNumber(DirectoryInfo crxDir)
    {
        var max = 0;
        foreach (var file in crxDir.EnumerateFiles("*.wav"))
        {
            if (int.TryParse(Path.GetFileNameWithoutExtension(file.Name), out var n))
            {
                max = Math.Max(max, n);
            }
        }

        return max + 1;
    }

    private async Task EnsureCrxTemplateAsync(FileInfo crxFile)
    {
        if (crxFile.Exists) return;

        var template = _state.CrxTemplatePath ?? _options.Value.CrxTemplatePath;
        if (!string.IsNullOrWhiteSpace(template) && File.Exists(template))
        {
            crxFile.Directory?.Create();
            File.Copy(template!, crxFile.FullName, overwrite: true);
            return;
        }

        crxFile.Directory?.Create();
        await using var stream = crxFile.Open(FileMode.Create, FileAccess.ReadWrite, FileShare.None);
        using var doc = SpreadsheetDocument.Create(stream, SpreadsheetDocumentType.Workbook);
        var workbookPart = doc.AddWorkbookPart();
        workbookPart.Workbook = new Workbook();
        var worksheetPart = workbookPart.AddNewPart<WorksheetPart>();
        worksheetPart.Worksheet = new Worksheet(new SheetData());
        var sheets = workbookPart.Workbook.AppendChild(new Sheets());
        sheets.Append(new Sheet { Id = workbookPart.GetIdOfPart(worksheetPart), SheetId = 1, Name = "Sheet1" });
    }

    private void UpdateCrxWorkbook(FileInfo crxFile, int errorNum, string chapter, double startSeconds, string errorType, string comments)
    {
        using var doc = SpreadsheetDocument.Open(crxFile.FullName, true);
        var workbookPart = doc.WorkbookPart ?? throw new InvalidOperationException("WorkbookPart missing");
        var worksheetPart = workbookPart.WorksheetParts.First();
        var sheetData = worksheetPart.Worksheet.GetFirstChild<SheetData>() ?? worksheetPart.Worksheet.AppendChild(new SheetData());

        uint targetRow = (uint)(11 + (errorNum - 1));

        SetCell(sheetData, "B", targetRow, errorNum.ToString("000"));
        SetCell(sheetData, "D", targetRow, chapter);
        SetCell(sheetData, "F", targetRow, TimeSpan.FromSeconds(startSeconds).ToString("hh\\:mm\\:ss"));
        SetCell(sheetData, "G", targetRow, errorType);
        SetCell(sheetData, "H", targetRow, comments);

        worksheetPart.Worksheet.Save();
        workbookPart.Workbook.Save();
    }

    private static void SetCell(SheetData sheetData, string columnName, uint rowIndex, string value)
    {
        var row = sheetData.Elements<Row>().FirstOrDefault(r => r.RowIndex == rowIndex);
        if (row is null)
        {
            row = new Row { RowIndex = rowIndex };
            sheetData.Append(row);
        }

        var cellReference = columnName + rowIndex;
        var cell = row.Elements<Cell>().FirstOrDefault(c => string.Equals(c.CellReference?.Value, cellReference, StringComparison.OrdinalIgnoreCase));
        if (cell is null)
        {
            cell = new Cell { CellReference = cellReference };
            row.Append(cell);
        }

        cell.DataType = CellValues.String;
        cell.CellValue = new CellValue(value);
    }

}
