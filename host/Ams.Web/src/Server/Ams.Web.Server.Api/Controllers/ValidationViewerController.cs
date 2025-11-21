using Ams.Web.Server.Api.Models.ValidationViewer;
using Ams.Web.Server.Api.Services;
using Ams.Web.Server.Api.Services.ValidationViewer;
using Ams.Web.Shared.ValidationViewer;

namespace Ams.Web.Server.Api.Controllers;

[ApiController]
[Route("api")]
[AllowAnonymous]
public sealed partial class ValidationViewerController : AppControllerBase
{
    [AutoInject] private IValidationViewerService _validation = default!;
    [AutoInject] private IAudioStreamService _audioService = default!;
    [AutoInject] private IReviewedStateService _reviewed = default!;
    [AutoInject] private ValidationViewerWorkspaceState _state = default!;
    [AutoInject] private ICrxService _crx = default!;
    [AutoInject] private IOptions<ValidationViewerOptions> _optionsAccessor = default!;
    [AutoInject] private ILogger<ValidationViewerController> _logger = default!;

    private DirectoryInfo GetBookRoot(string bookId)
    {
        var path = _state.BookRoot;
        if (string.IsNullOrWhiteSpace(path))
        {
            throw new InvalidOperationException("ValidationViewer: BookRoot is not configured.");
        }

        var dir = new DirectoryInfo(path);
        // If caller specifies a book ID that doesn't match, we still use the configured root but log.
        if (!dir.Name.Equals(bookId, StringComparison.OrdinalIgnoreCase))
        {
            _logger.LogDebug("Requested bookId {BookId} differs from configured book root {Root}; using configured root.", bookId, dir.Name);
        }

        return dir;
    }

    #region Reviewed state

    [HttpGet("validation/books/{bookId}/reviewed")]
    public async Task<IActionResult> GetReviewedAsync(string bookId)
    {
        var status = await _reviewed.GetAsync(bookId);
        return Ok(status);
    }

    [HttpPost("validation/books/{bookId}/reviewed/{chapter}")]
    public async Task<IActionResult> MarkReviewedAsync(string bookId, string chapter, [FromBody] JsonElement payload)
    {
        var reviewed = payload.TryGetProperty("reviewed", out var reviewedProp) && reviewedProp.GetBoolean();
        var current = await _reviewed.SetAsync(bookId, chapter, reviewed);
        return Ok(new { success = true, chapter, reviewed });
    }

    [HttpPost("validation/books/{bookId}/reset-reviews")]
    public async Task<IActionResult> ResetReviewsAsync(string bookId)
    {
        await _reviewed.ResetAsync(bookId);
        return Ok(new { success = true, message = "All review status reset" });
    }

    #endregion

    #region Chapters & overview

    [HttpGet("validation/books/{bookId}/chapters")]
    public IActionResult GetChapters(string bookId)
    {
        var summaries = _validation.GetChapters(bookId);
        return Ok(summaries);
    }

    [HttpGet("validation/books/{bookId}/overview")]
    public IActionResult GetOverview(string bookId)
    {
        return Ok(_validation.GetOverview(bookId));
    }

    #endregion

    #region Report

    [HttpGet("validation/books/{bookId}/report/{chapter}")]
    public IActionResult GetReport(string bookId, string chapter)
    {
        var report = _validation.GetReport(bookId, chapter);
        if (report is null) return NotFound(new { error = "Hydrate file not found", chapter });
        return Ok(report);
    }
    #endregion
    
    #region Audio endpoints

    [HttpGet("audio/books/{bookId}/chapters/{chapter}")]
    public IActionResult GetAudioAsync(string bookId, string chapter, [FromQuery] double? start = null, [FromQuery] double? end = null, [FromQuery] string? source = "raw")
    {
        var buffer = _audioService.LoadBuffer(bookId, chapter, source ?? "raw");
        if (buffer is null)
        {
            return NotFound(new { error = "Audio buffer not found" });
        }

        var slice = _audioService.Slice(buffer, start, end);
        var stream = _audioService.ToWavStream(slice);
        return File(stream, "audio/wav");
    }

    [HttpPost("audio/books/{bookId}/chapters/{chapter}/export")]
    public async Task<IActionResult> ExportAudioAsync(string bookId, string chapter, [FromBody] JsonElement payload)
    {
        if (!payload.TryGetProperty("start", out var startEl) || !payload.TryGetProperty("end", out var endEl))
        {
            return BadRequest(new { error = "start and end are required" });
        }

        var start = startEl.GetDouble();
        var end = endEl.GetDouble();

        var result = await _crx.ExportAsync(bookId, chapter, start, end);
        return Ok(result);
    }

    [HttpPost("validation/books/{bookId}/crx/{chapter}")]
    public async Task<IActionResult> AddToCrxAsync(string bookId, string chapter, [FromBody] JsonElement payload)
    {
        var result = await _crx.AddToCrxAsync(bookId, chapter, payload);
        return Ok(result);
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
