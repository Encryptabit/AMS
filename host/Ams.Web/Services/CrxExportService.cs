using System.Globalization;
using Ams.Core.Artifacts.Hydrate;
using Ams.Web.Configuration;
using Ams.Web.Requests;
using ClosedXML.Excel;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Ams.Web.Services;

public sealed class CrxExportService
{
    private readonly CrxOptions _options;
    private readonly WorkspaceState _workspaceState;
    private readonly ChapterDataService _chapterData;
    private readonly IAudioSegmentExporter _segmentExporter;
    private readonly ILogger<CrxExportService> _logger;

    public CrxExportService(
        IOptions<AmsOptions> options,
        WorkspaceState workspaceState,
        ChapterDataService chapterData,
        IAudioSegmentExporter segmentExporter,
        ILogger<CrxExportService> logger)
    {
        ArgumentNullException.ThrowIfNull(options);
        _workspaceState = workspaceState ?? throw new ArgumentNullException(nameof(workspaceState));
        _chapterData = chapterData ?? throw new ArgumentNullException(nameof(chapterData));
        _segmentExporter = segmentExporter ?? throw new ArgumentNullException(nameof(segmentExporter));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));

        _options = options.Value.Crx ?? throw new InvalidOperationException("CRX options are not configured.");
    }

    public async Task<CrxExportResult> ExportAsync(
        ChapterSummary summary,
        HydratedSentence sentence,
        ExportSentenceRequest request,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(summary);
        ArgumentNullException.ThrowIfNull(sentence);
        request ??= new ExportSentenceRequest();

        if (sentence.Timing is null)
        {
            throw new InvalidOperationException($"Sentence {sentence.Id} does not contain timing data.");
        }

        var errorType = string.IsNullOrWhiteSpace(request.ErrorType)
            ? _options.DefaultErrorType
            : request.ErrorType!.Trim().ToUpperInvariant();

        var templatePath = _workspaceState.TemplatePath;
        if (!File.Exists(templatePath))
        {
            throw new FileNotFoundException("CRX template was not found.", templatePath);
        }

        var crxRoot = Path.Combine(_workspaceState.WorkspaceRoot, "CRX");
        Directory.CreateDirectory(crxRoot);

        var chapterCrxDirectory = Path.Combine(crxRoot, summary.Id);
        Directory.CreateDirectory(chapterCrxDirectory);

        var audioFile = await _chapterData.ResolveAudioFileAsync(summary, cancellationToken).ConfigureAwait(false);
        var clipFileName = $"{summary.Id}_S{sentence.Id:D4}_{FormatTimeStamp(sentence.Timing.StartSec)}-{FormatTimeStamp(sentence.Timing.EndSec)}.wav";
        var clipFile = await _segmentExporter.ExportAsync(
            audioFile.FullName,
            chapterCrxDirectory,
            clipFileName,
            sentence.Timing.StartSec,
            sentence.Timing.EndSec,
            cancellationToken).ConfigureAwait(false);

        var workbookPath = Path.Combine(chapterCrxDirectory, $"{summary.Id}_CRX.xlsx");
        await EnsureWorkbookAsync(workbookPath, templatePath, cancellationToken).ConfigureAwait(false);

        var rowNumber = await AppendRowAsync(workbookPath, summary, sentence, errorType, request.Comment, clipFile, cancellationToken)
            .ConfigureAwait(false);

        return new CrxExportResult(
            clipFile.FullName,
            workbookPath,
            rowNumber,
            errorType,
            request.Comment ?? string.Empty,
            sentence.Timing.StartSec,
            sentence.Timing.EndSec);
    }

    private async Task EnsureWorkbookAsync(string workbookPath, string templatePath, CancellationToken cancellationToken)
    {
        if (File.Exists(workbookPath))
        {
            return;
        }

        cancellationToken.ThrowIfCancellationRequested();
        _logger.LogDebug("Creating CRX workbook {Workbook}", workbookPath);
        await Task.Run(() => File.Copy(templatePath, workbookPath, overwrite: false), cancellationToken).ConfigureAwait(false);
    }

    private async Task<int> AppendRowAsync(
        string workbookPath,
        ChapterSummary summary,
        HydratedSentence sentence,
        string errorType,
        string? comment,
        FileInfo clipFile,
        CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        return await Task.Run(() =>
        {
            using var workbook = new XLWorkbook(workbookPath);
            var worksheet = workbook.Worksheets.First();
            var rowNumber = _options.FirstDataRow;

            while (!worksheet.Row(rowNumber).IsEmpty())
            {
                rowNumber++;
            }

            worksheet.Cell(rowNumber, 1).Value = summary.DisplayName;
            worksheet.Cell(rowNumber, 2).Value = sentence.Id;
            worksheet.Cell(rowNumber, 3).Value = FormatTime(sentence.Timing!.StartSec);
            worksheet.Cell(rowNumber, 4).Value = FormatTime(sentence.Timing!.EndSec);
            worksheet.Cell(rowNumber, 5).Value = errorType;
            worksheet.Cell(rowNumber, 6).Value = comment ?? string.Empty;
            worksheet.Cell(rowNumber, 7).Value = clipFile.Name;
            worksheet.Cell(rowNumber, 8).Value = sentence.BookText;

            workbook.Save();
            return rowNumber;
        }, cancellationToken).ConfigureAwait(false);
    }

    private static string FormatTime(double seconds)
        => TimeSpan.FromSeconds(seconds).ToString(@"hh\:mm\:ss\.fff", CultureInfo.InvariantCulture);

    private static string FormatTimeStamp(double seconds)
        => TimeSpan.FromSeconds(seconds).ToString("hhmmss-fff", CultureInfo.InvariantCulture);
}

public sealed record CrxExportResult(
    string SegmentPath,
    string WorkbookPath,
    int RowNumber,
    string ErrorType,
    string Comment,
    double StartSeconds,
    double EndSeconds);
