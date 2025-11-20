using System.Diagnostics;
using System.Text.Json;
using System.Text.Json.Serialization;
using Ams.Core.Artifacts.Hydrate;
using Ams.Core.Runtime.Artifacts;
using Ams.Core.Runtime.Chapter;
using Ams.Core.Runtime.Workspace;
using Ams.Web.Server.Api.Models.ValidationViewer;
using Ams.Web.Server.Api.Services;
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Spreadsheet;
using Microsoft.AspNetCore.Http;

namespace Ams.Web.Server.Api.Controllers;

[ApiController]
[Route("api")]
[AllowAnonymous]
public sealed partial class ValidationViewerController : AppControllerBase
{
    [AutoInject] private IOptions<ValidationViewerOptions> _optionsAccessor = default!;
    [AutoInject] private ValidationViewerWorkspaceState _state = default!;
    [AutoInject] private ILogger<ValidationViewerController> _logger = default!;

    private readonly JsonSerializerOptions _jsonOptions = new()
    {
        PropertyNameCaseInsensitive = true,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
    };

    private const string ReviewedFileName = "reviewed-status.json";

    private DirectoryInfo BookRoot
    {
        get
        {
            var path = _state.BookRoot;
            if (string.IsNullOrWhiteSpace(path))
            {
                throw new InvalidOperationException("ValidationViewer: BookRoot is not configured.");
            }

            return new DirectoryInfo(path);
        }
    }

    #region Reviewed state

    [HttpGet("reviewed")]
    public async Task<IActionResult> GetReviewedAsync()
    {
        var status = await LoadReviewedAsync();
        return Ok(status);
    }

    [HttpPost("reviewed/{chapter}")]
    public async Task<IActionResult> MarkReviewedAsync(string chapter, [FromBody] JsonElement payload)
    {
        var reviewed = payload.TryGetProperty("reviewed", out var reviewedProp) && reviewedProp.GetBoolean();
        var current = await LoadReviewedAsync();
        current[chapter] = new ReviewedStatusDto(reviewed, DateTime.UtcNow.ToString("o"));
        await SaveReviewedAsync(current);
        return Ok(new { success = true, chapter, reviewed });
    }

    [HttpPost("reset-reviews")]
    public async Task<IActionResult> ResetReviewsAsync()
    {
        await SaveReviewedAsync(new Dictionary<string, ReviewedStatusDto>());
        return Ok(new { success = true, message = "All review status reset" });
    }

    private async Task<Dictionary<string, ReviewedStatusDto>> LoadReviewedAsync()
    {
        try
        {
            var file = GetReviewedFile();
            if (!file.Exists)
            {
                return new();
            }

            await using var stream = file.OpenRead();
            var all = await JsonSerializer.DeserializeAsync<Dictionary<string, Dictionary<string, ReviewedStatusDto>>>(stream, _jsonOptions)
                      ?? new();
            return all.TryGetValue(BookRoot.Name, out var bookDict) ? new(bookDict) : new();
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to load reviewed status.");
            return new();
        }
    }

    private async Task SaveReviewedAsync(Dictionary<string, ReviewedStatusDto> current)
    {
        var file = GetReviewedFile();
        if (!file.Directory!.Exists)
        {
            file.Directory.Create();
        }

        Dictionary<string, Dictionary<string, ReviewedStatusDto>> payload;
        if (file.Exists)
        {
            try
            {
                await using var existing = file.OpenRead();
                payload = await JsonSerializer.DeserializeAsync<Dictionary<string, Dictionary<string, ReviewedStatusDto>>>(existing, _jsonOptions)
                           ?? new();
            }
            catch
            {
                payload = new();
            }
        }
        else
        {
            payload = new();
        }

        payload[BookRoot.Name] = current;

        await using var stream = file.Open(FileMode.Create, FileAccess.Write, FileShare.Read);
        await JsonSerializer.SerializeAsync(stream, payload, _jsonOptions);
    }

    private FileInfo GetReviewedFile()
    {
        var opt = _optionsAccessor.Value;
        if (!string.IsNullOrWhiteSpace(_state.ReviewedStatusPath ?? opt.ReviewedStatusPath))
        {
            return new FileInfo(_state.ReviewedStatusPath ?? opt.ReviewedStatusPath!);
        }

        var root = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
        if (string.IsNullOrWhiteSpace(root))
        {
            root = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), ".config");
        }

        return new FileInfo(Path.Combine(root, "AMS", "validation-viewer", ReviewedFileName));
    }

    #endregion

    #region Chapters & overview

    [HttpGet("chapters")]
    public IActionResult GetChapters()
    {
        var summaries = EnumerateChapters();
        return Ok(summaries);
    }

    [HttpGet("overview")]
    public IActionResult GetOverview()
    {
        var chapters = EnumerateChapters();

        var totalSentences = chapters.Sum(c => c.Metrics.SentenceCount);
        var totalFlaggedSentences = chapters.Sum(c => c.Metrics.SentenceFlagged);
        var totalParagraphs = chapters.Sum(c => c.Metrics.ParagraphCount);
        var totalFlaggedParagraphs = chapters.Sum(c => c.Metrics.ParagraphFlagged);

        var avgSentenceWer = totalSentences > 0
            ? chapters.Where(c => c.Metrics.SentenceCount > 0)
                .Sum(c => c.Metrics.SentenceCount * double.Parse(c.Metrics.SentenceAvgWer.TrimEnd('%'), System.Globalization.CultureInfo.InvariantCulture))
                / totalSentences
            : 0;

        var avgParagraphWer = totalParagraphs > 0
            ? chapters.Where(c => c.Metrics.ParagraphCount > 0)
                .Sum(c => c.Metrics.ParagraphCount * double.Parse(c.Metrics.ParagraphAvgWer.TrimEnd('%'), System.Globalization.CultureInfo.InvariantCulture))
                / totalParagraphs
            : 0;

        var response = new ValidationOverviewResponse(
            BookRoot.Name,
            chapters.Count,
            totalSentences,
            totalFlaggedSentences,
            $"{avgSentenceWer:F2}%",
            totalParagraphs,
            totalFlaggedParagraphs,
            $"{avgParagraphWer:F2}%",
            chapters);

        return Ok(response);
    }

    private List<ValidationChapterSummary> EnumerateChapters()
    {
        var baseDir = BookRoot;
        if (!baseDir.Exists)
        {
            throw new DirectoryNotFoundException($"Book root not found: {baseDir.FullName}");
        }

        var summaries = new List<ValidationChapterSummary>();
        foreach (var dir in baseDir.EnumerateDirectories())
        {
            var hydrate = dir.GetFiles($"{dir.Name}.align.hydrate.json").FirstOrDefault();
            if (hydrate is null || !hydrate.Exists)
            {
                continue;
            }

            var metrics = BuildMetrics(hydrate);
            summaries.Add(new ValidationChapterSummary(
                dir.Name,
                Path.GetRelativePath(baseDir.FullName, dir.FullName),
                metrics));
        }

        return summaries
            .OrderBy(c => NaturalSortKey(c.Name))
            .ToList();
    }

    private static (int Primary, string Value) NaturalSortKey(string name)
    {
        var nums = System.Text.RegularExpressions.Regex.Matches(name, "\\d+")
            .Select(m => int.Parse(m.Value))
            .ToList();
        return nums.Count > 0 ? (nums[0], name.ToLowerInvariant()) : (int.MaxValue, name.ToLowerInvariant());
    }

    private ValidationChapterMetrics BuildMetrics(FileInfo hydrateFile)
    {
        var hydrate = LoadHydrate(hydrateFile.FullName);
        var sentences = hydrate.Sentences ?? Array.Empty<HydratedSentence>();
        var paragraphs = hydrate.Paragraphs ?? Array.Empty<HydratedParagraph>();

        var flaggedSentences = sentences.Count(s => !string.Equals(s.Status, "ok", StringComparison.OrdinalIgnoreCase));
        var avgWer = sentences.Count > 0 ? sentences.Average(s => s.Metrics.Wer) * 100 : 0;

        var flaggedParagraphs = paragraphs.Count(p => !string.Equals(p.Status, "ok", StringComparison.OrdinalIgnoreCase));
        var avgParagraphWer = paragraphs.Count > 0 ? paragraphs.Average(p => p.Metrics.Wer) * 100 : 0;

        return new ValidationChapterMetrics(
            sentences.Count,
            flaggedSentences,
            $"{avgWer:F2}%",
            paragraphs.Count,
            flaggedParagraphs,
            $"{avgParagraphWer:F2}%");
    }

    #endregion

    #region Report

    [HttpGet("report/{chapter}")]
    public IActionResult GetReport(string chapter)
    {
        var handle = OpenChapterHandle(chapter);
        if (handle is null)
        {
            return NotFound(new { error = "Hydrate file not found", chapter });
        }

        using (handle)
        {
            var hydrate = handle.Chapter.Documents.HydratedTranscript;
            if (hydrate is null)
            {
                return NotFound(new { error = "Hydrate file not found", chapter });
            }

            var report = BuildReport(handle.Chapter.Descriptor.ChapterId, hydrate);
            return Ok(report);
        }
    }

    private ValidationReportResponse BuildReport(string chapterName, HydratedTranscript hydrate)
    {
        var sentences = hydrate.Sentences ?? Array.Empty<HydratedSentence>();
        var paragraphs = hydrate.Paragraphs ?? Array.Empty<HydratedParagraph>();

        var flagged = sentences.Where(s => !string.Equals(s.Status, "ok", StringComparison.OrdinalIgnoreCase)).ToList();
        var avgWer = sentences.Count > 0 ? sentences.Average(s => s.Metrics.Wer) * 100 : 0;
        var maxWer = sentences.Count > 0 ? sentences.Max(s => s.Metrics.Wer) * 100 : 0;

        var sentenceToParagraph = new Dictionary<int, int>();
        foreach (var para in paragraphs)
        {
            foreach (var sid in para.SentenceIds)
            {
                sentenceToParagraph[sid] = para.Id;
            }
        }

        var wordOpsBySentence = BuildWordOpsBySentence(hydrate);

        var sentenceDtos = sentences.Select(s =>
        {
            var timing = s.Timing;
            var timingText = timing is null
                ? string.Empty
                : $"{timing.StartSec:0.000}s → {timing.EndSec:0.000}s (Δ {timing.Duration:0.000}s)";

            var bookRange = s.BookRange;
            var scriptRange = s.ScriptRange;

            return new ValidationSentenceResponse(
                s.Id,
                $"{s.Metrics.Wer * 100:0.1}%",
                $"{s.Metrics.Cer * 100:0.1}%",
                s.Status,
                $"{bookRange.Start}-{bookRange.End}",
                scriptRange is null ? string.Empty : $"{scriptRange.Start?.ToString() ?? "0"}-{scriptRange.End?.ToString() ?? "0"}",
                timingText,
                s.BookText,
                s.ScriptText,
                s.BookText.Length > 100 ? s.BookText[..100] : s.BookText,
                s.Diff,
                timing?.StartSec,
                timing?.EndSec,
                bookRange.Start,
                bookRange.End,
                sentenceToParagraph.TryGetValue(s.Id, out var pid) ? pid : null,
                wordOpsBySentence.TryGetValue(s.Id, out var ops) ? ops : null
            );
        }).ToList();

        var paragraphDtos = new List<ValidationParagraphResponse>();

        var stats = new ValidationReportStats(
            sentences.Count.ToString(),
            $"{avgWer:0.2}%",
            $"{maxWer:0.2}%",
            flagged.Count.ToString(),
            paragraphs.Count.ToString(),
            paragraphs.Count > 0 ? $"{paragraphs.Average(p => p.Metrics.Wer) * 100:0.2}%" : "0.00%",
            paragraphs.Count > 0 ? $"{paragraphs.Average(p => p.Metrics.Coverage) * 100:0.2}%" : "0.00%"
        );

        return new ValidationReportResponse(
            chapterName,
            hydrate.AudioPath,
            hydrate.ScriptPath,
            hydrate.BookIndexPath,
            DateTime.UtcNow.ToString("o"),
            stats,
            sentenceDtos,
            paragraphDtos);
    }

    private static Dictionary<int, IReadOnlyList<Dictionary<string, string?>>> BuildWordOpsBySentence(HydratedTranscript hydrate)
    {
        var result = new Dictionary<int, IReadOnlyList<Dictionary<string, string?>>>();
        if (hydrate.Words is null || hydrate.Words.Count == 0)
        {
            return result;
        }

        var sentences = hydrate.Sentences ?? Array.Empty<HydratedSentence>();
        var sentenceByBookIdx = new Dictionary<int, int>();
        foreach (var s in sentences)
        {
            var start = Math.Min(s.BookRange.Start, s.BookRange.End);
            var end = Math.Max(s.BookRange.Start, s.BookRange.End);
            for (var i = start; i <= end; i++)
            {
                sentenceByBookIdx[i] = s.Id;
            }
        }

        int? lastSentence = null;
        foreach (var word in hydrate.Words)
        {
            var sentenceId = word.BookIdx.HasValue && sentenceByBookIdx.TryGetValue(word.BookIdx.Value, out var sid)
                ? sid
                : lastSentence;

            if (sentenceId is null)
            {
                continue;
            }

            var entry = new Dictionary<string, string?>
            {
                ["op"] = word.Op,
                ["reason"] = word.Reason,
                ["bookWord"] = word.BookWord?.Trim(),
                ["asrWord"] = word.AsrWord?.Trim()
            };

            if (!result.TryGetValue(sentenceId.Value, out var list))
            {
                result[sentenceId.Value] = new List<Dictionary<string, string?>> { entry };
            }
            else if (list is List<Dictionary<string, string?>> mutable)
            {
                mutable.Add(entry);
            }
            else
            {
                result[sentenceId.Value] = new List<Dictionary<string, string?>>(list) { entry };
            }

            lastSentence = sentenceId;
        }

        return result;
    }

    private ChapterContextHandle? OpenChapterHandle(string chapterId)
    {
        var bookIndexPath = _state.BookIndexPath ?? Path.Combine(BookRoot.FullName, "book-index.json");
        var bookIndex = new FileInfo(bookIndexPath);
        if (!bookIndex.Exists)
        {
            _logger.LogWarning("book-index.json not found at {Path}", bookIndex.FullName);
            return null;
        }

        var chapterDir = new DirectoryInfo(Path.Combine(BookRoot.FullName, chapterId));
        var audio = new FileInfo(Path.Combine(chapterDir.FullName, $"{chapterId}.wav"));
        if (!audio.Exists)
        {
            var rootAudio = new FileInfo(Path.Combine(BookRoot.FullName, $"{chapterId}.wav"));
            audio = rootAudio.Exists ? rootAudio : audio;
        }

        var hydrate = new FileInfo(Path.Combine(chapterDir.FullName, $"{chapterId}.align.hydrate.json"));
        if (!hydrate.Exists)
        {
            return null;
        }

        var asr = new FileInfo(Path.Combine(chapterDir.FullName, "asr.json"));
        var transcript = new FileInfo(Path.Combine(chapterDir.FullName, $"{chapterId}.align.tx.json"));

        return ChapterContextHandle.Create(
            bookIndex,
            asr.Exists ? asr : null,
            transcript.Exists ? transcript : null,
            hydrate,
            audio.Exists ? audio : null,
            chapterDir.Exists ? chapterDir : null,
            chapterId);
    }

    private HydratedTranscript LoadHydrate(string path)
    {
        var json = System.IO.File.ReadAllText(path);
        return JsonSerializer.Deserialize<HydratedTranscript>(json, _jsonOptions)
               ?? throw new InvalidOperationException($"Failed to read hydrate from {path}");
    }

    #endregion

    #region Audio endpoints

    [HttpGet("audio/{chapter}")]
    public async Task<IActionResult> GetAudioAsync(string chapter, [FromQuery] double? start = null, [FromQuery] double? end = null, [FromQuery] string? source = "raw")
    {
        var audioPath = ResolveAudioPath(chapter, source);
        if (audioPath is null)
        {
            return NotFound(new { error = "Audio file not found" });
        }

        if (start is null || end is null)
        {
            var stream = System.IO.File.OpenRead(audioPath.FullName);
            return File(stream, "audio/wav", enableRangeProcessing: true);
        }

        if (end <= start)
        {
            return BadRequest(new { error = "end must be greater than start" });
        }

        var duration = end.Value - start.Value;
        var temp = Path.GetTempFileName() + ".wav";

        try
        {
            var psi = new ProcessStartInfo
            {
                FileName = "ffmpeg",
                ArgumentList =
                {
                    "-i", audioPath.FullName,
                    "-ss", start.Value.ToString(System.Globalization.CultureInfo.InvariantCulture),
                    "-t", duration.ToString(System.Globalization.CultureInfo.InvariantCulture),
                    "-c", "copy",
                    "-y",
                    temp
                },
                RedirectStandardError = true,
                RedirectStandardOutput = true,
                UseShellExecute = false
            };

            var proc = Process.Start(psi) ?? throw new InvalidOperationException("Failed to start ffmpeg");
            await proc.WaitForExitAsync();
            if (proc.ExitCode != 0)
            {
                var err = await proc.StandardError.ReadToEndAsync();
                _logger.LogWarning("ffmpeg exit {Code}: {Error}", proc.ExitCode, err);
                return StatusCode(StatusCodes.Status500InternalServerError, new { error = "ffmpeg failed", details = err });
            }

            var bytes = await System.IO.File.ReadAllBytesAsync(temp);
            return File(bytes, "audio/wav");
        }
        finally
        {
            try { System.IO.File.Delete(temp); } catch { }
        }
    }

    private FileInfo? ResolveAudioPath(string chapter, string? source)
    {
        source = string.IsNullOrWhiteSpace(source) ? "raw" : source.ToLowerInvariant();
        var chapterDir = new DirectoryInfo(Path.Combine(BookRoot.FullName, chapter));

        string stem = source switch
        {
            "treated" => $"{chapter}.treated.wav",
            "filtered" => $"{chapter}.filtered.wav",
            _ => $"{chapter}.wav"
        };

        var candidate = new FileInfo(Path.Combine(chapterDir.FullName, stem));
        if (!candidate.Exists && source == "raw")
        {
            // fallback to book root
            candidate = new FileInfo(Path.Combine(BookRoot.FullName, stem));
        }

        return candidate.Exists ? candidate : null;
    }

    [HttpPost("export/{chapter}")]
    public async Task<IActionResult> ExportAudioAsync(string chapter, [FromBody] JsonElement payload)
    {
        if (!payload.TryGetProperty("start", out var startEl) || !payload.TryGetProperty("end", out var endEl))
        {
            return BadRequest(new { error = "start and end are required" });
        }

        var start = startEl.GetDouble();
        var end = endEl.GetDouble();

        var audioPath = ResolveAudioPath(chapter, "treated") ?? ResolveAudioPath(chapter, "raw");
        if (audioPath is null)
        {
            return NotFound(new { error = "Audio file not found" });
        }

        var crxDir = new DirectoryInfo(Path.Combine(BookRoot.FullName, _state.CrxDirectoryName));
        if (!crxDir.Exists)
        {
            crxDir.Create();
        }

        var errorNum = NextErrorNumber(crxDir);
        var exportFile = new FileInfo(Path.Combine(crxDir.FullName, $"{errorNum:000}.wav"));
        var duration = Math.Max(0.1, end - start);

        var psi = new ProcessStartInfo
        {
            FileName = "ffmpeg",
            ArgumentList =
            {
                "-i", audioPath.FullName,
                "-ss", start.ToString(System.Globalization.CultureInfo.InvariantCulture),
                "-t", duration.ToString(System.Globalization.CultureInfo.InvariantCulture),
                "-c", "copy",
                "-y",
                exportFile.FullName
            },
            RedirectStandardError = true,
            RedirectStandardOutput = true,
            UseShellExecute = false
        };

        var proc = Process.Start(psi) ?? throw new InvalidOperationException("Failed to start ffmpeg");
        await proc.WaitForExitAsync();
        if (proc.ExitCode != 0)
        {
            var err = await proc.StandardError.ReadToEndAsync();
            return StatusCode(StatusCodes.Status500InternalServerError, new { error = "ffmpeg failed", details = err });
        }

        return Ok(new
        {
            success = true,
            filename = exportFile.Name,
            errorNumber = errorNum,
            path = Path.GetRelativePath(BookRoot.FullName, exportFile.FullName)
        });
    }

    [HttpPost("crx/{chapter}")]
    public async Task<IActionResult> AddToCrxAsync(string chapter, [FromBody] JsonElement payload)
    {
        var start = payload.TryGetProperty("start", out var sVal) ? sVal.GetDouble() : 0;
        var end = payload.TryGetProperty("end", out var eVal) ? eVal.GetDouble() : start;
        var sentenceId = payload.TryGetProperty("sentenceId", out var sidVal) ? sidVal.GetInt32() : -1;
        var errorType = payload.TryGetProperty("errorType", out var etVal) ? etVal.GetString() : _state.DefaultErrorType;
        var comments = payload.TryGetProperty("comments", out var cVal) ? cVal.GetString() : string.Empty;
        var paddingMs = payload.TryGetProperty("paddingMs", out var pVal) ? pVal.GetInt32() : 50;

        var crxDir = new DirectoryInfo(Path.Combine(BookRoot.FullName, _state.CrxDirectoryName));
        if (!crxDir.Exists)
        {
            crxDir.Create();
        }

        var errorNum = NextErrorNumber(crxDir);
        var crxFile = new FileInfo(Path.Combine(crxDir.FullName, $"{BookRoot.Name}_CRX.xlsx"));

        await EnsureCrxTemplateAsync(crxFile);
        UpdateCrxWorkbook(crxFile, errorNum, chapter, start, errorType ?? _state.DefaultErrorType, comments ?? string.Empty);

        // export audio with padding
        var audioPath = ResolveAudioPath(chapter, "treated") ?? ResolveAudioPath(chapter, "raw");
        if (audioPath is not null)
        {
            var duration = Math.Max(0.1, (end - start) + paddingMs / 1000d);
            var exportFile = new FileInfo(Path.Combine(crxDir.FullName, $"{errorNum:000}.wav"));

            var psi = new ProcessStartInfo
            {
                FileName = "ffmpeg",
                ArgumentList =
                {
                    "-i", audioPath.FullName,
                    "-ss", start.ToString(System.Globalization.CultureInfo.InvariantCulture),
                    "-t", duration.ToString(System.Globalization.CultureInfo.InvariantCulture),
                    "-c", "copy",
                    "-y",
                    exportFile.FullName
                },
                RedirectStandardError = true,
                RedirectStandardOutput = true,
                UseShellExecute = false
            };

            var proc = Process.Start(psi) ?? throw new InvalidOperationException("Failed to start ffmpeg");
            await proc.WaitForExitAsync();
            if (proc.ExitCode != 0)
            {
                _logger.LogWarning("ffmpeg failed when exporting CRX audio: {Code}", proc.ExitCode);
            }
        }

        return Ok(new
        {
            success = true,
            errorNumber = errorNum,
            crxFile = crxFile.Name,
            timecode = TimeSpan.FromSeconds(start).ToString("hh\\:mm\\:ss"),
            audioFile = $"{errorNum:000}.wav"
        });
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
        if (crxFile.Exists)
        {
            return;
        }

        var template = _state.CrxTemplatePath ?? _optionsAccessor.Value.CrxTemplatePath;
        if (!string.IsNullOrWhiteSpace(template) && System.IO.File.Exists(template))
        {
            crxFile.Directory?.Create();
            System.IO.File.Copy(template!, crxFile.FullName, overwrite: true);
            return;
        }

        // create a minimal workbook
        crxFile.Directory?.Create();
        await using var stream = crxFile.Open(FileMode.Create, FileAccess.ReadWrite, FileShare.None);
        using var doc = SpreadsheetDocument.Create(stream, DocumentFormat.OpenXml.SpreadsheetDocumentType.Workbook);
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

        uint targetRow = (uint)(11 + (errorNum - 1)); // row 11 is first data row in python tool

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

    #endregion

    #region Workspace endpoints

    public sealed record WorkspaceRequest(
        string? WorkspaceRoot,
        string? BookIndexPath,
        string? CrxTemplatePath,
        string? CrxDirectoryName,
        string? DefaultErrorType);

    [HttpGet("workspace")]
    public IActionResult GetWorkspace()
    {
        return Ok(new
        {
            workspaceRoot = _state.BookRoot,
            bookIndexPath = _state.BookIndexPath,
            crxTemplatePath = _state.CrxTemplatePath,
            crxDirectoryName = _state.CrxDirectoryName,
            defaultErrorType = _state.DefaultErrorType
        });
    }

    [HttpPost("workspace")]
    public IActionResult SetWorkspace([FromBody] WorkspaceRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.WorkspaceRoot) && string.IsNullOrWhiteSpace(request.BookIndexPath))
        {
            return BadRequest(new { error = "workspaceRoot or bookIndexPath must be provided" });
        }

        _state.Update(
            bookRoot: request.WorkspaceRoot,
            bookIndexPath: request.BookIndexPath,
            crxTemplatePath: request.CrxTemplatePath,
            crxDirectoryName: request.CrxDirectoryName,
            defaultErrorType: request.DefaultErrorType);

        return GetWorkspace();
    }

    #endregion
}
