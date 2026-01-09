using System;
using System.Collections.Generic;
using System.IO;
using Ams.Workstation.Server.Models;
using Ams.Workstation.Server.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace Ams.Workstation.Server.Controllers;

/// <summary>
/// REST API controller for validation proof reports.
/// Provides chapter metrics, book overview, and detailed chapter reports.
/// </summary>
[ApiController]
[Route("api/proof")]
public class ProofApiController : ControllerBase
{
    private readonly BlazorWorkspace _workspace;
    private readonly ValidationMetricsService _metricsService;
    private readonly ProofReportService _reportService;
    private readonly ILogger<ProofApiController> _logger;

    public ProofApiController(
        BlazorWorkspace workspace,
        ValidationMetricsService metricsService,
        ProofReportService reportService,
        ILogger<ProofApiController> logger)
    {
        _workspace = workspace;
        _metricsService = metricsService;
        _reportService = reportService;
        _logger = logger;
    }

    /// <summary>
    /// GET /api/proof/chapters - List all chapters with metrics.
    /// </summary>
    [HttpGet("chapters")]
    public ActionResult<IEnumerable<ProofChapterInfo>> GetChapters()
    {
        if (!_workspace.IsInitialized)
        {
            return BadRequest("Workspace not initialized. Set working directory first.");
        }

        var chapters = new List<ProofChapterInfo>();

        foreach (var chapterName in _workspace.AvailableChapters)
        {
            try
            {
                var hydrate = LoadHydratedTranscript(chapterName);
                if (hydrate == null)
                {
                    _logger.LogDebug("No hydrate file for chapter '{ChapterName}'", chapterName);
                    continue;
                }

                var metrics = _metricsService.ComputeChapterMetrics(hydrate);
                chapters.Add(new ProofChapterInfo(chapterName, metrics));
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to load metrics for chapter '{ChapterName}'", chapterName);
            }
        }

        return Ok(chapters);
    }

    /// <summary>
    /// GET /api/proof/overview - Book-wide aggregate metrics.
    /// </summary>
    [HttpGet("overview")]
    public ActionResult<BookOverview> GetOverview()
    {
        if (!_workspace.IsInitialized)
        {
            return BadRequest("Workspace not initialized. Set working directory first.");
        }

        var chapterMetrics = new List<(string ChapterName, ChapterMetrics Metrics)>();

        foreach (var chapterName in _workspace.AvailableChapters)
        {
            try
            {
                var hydrate = LoadHydratedTranscript(chapterName);
                if (hydrate == null) continue;

                var metrics = _metricsService.ComputeChapterMetrics(hydrate);
                chapterMetrics.Add((chapterName, metrics));
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to load metrics for chapter '{ChapterName}'", chapterName);
            }
        }

        var overview = _metricsService.ComputeBookOverview(chapterMetrics);

        // Set book name from working directory
        var bookName = Path.GetFileName(_workspace.WorkingDirectory?.TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar));

        return Ok(overview with { BookName = bookName ?? "" });
    }

    /// <summary>
    /// GET /api/proof/report/{chapterName} - Detailed chapter report.
    /// </summary>
    [HttpGet("report/{chapterName}")]
    public ActionResult<ChapterReport> GetReport(string chapterName)
    {
        if (!_workspace.IsInitialized)
        {
            return BadRequest("Workspace not initialized. Set working directory first.");
        }

        if (string.IsNullOrEmpty(chapterName))
        {
            return BadRequest("Chapter name is required.");
        }

        // URL decode the chapter name
        var decodedName = Uri.UnescapeDataString(chapterName);

        try
        {
            var hydrate = LoadHydratedTranscript(decodedName);
            if (hydrate == null)
            {
                return NotFound($"Chapter '{decodedName}' not found or has no hydrate file.");
            }

            var report = _reportService.BuildReport(decodedName, hydrate);
            return Ok(report);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to build report for chapter '{ChapterName}'", decodedName);
            return NotFound($"Failed to load chapter '{decodedName}': {ex.Message}");
        }
    }

    /// <summary>
    /// Load HydratedTranscript for a chapter by selecting it in the workspace.
    /// </summary>
    /// <remarks>
    /// This temporarily switches the workspace's current chapter to load the hydrate file.
    /// The workspace maintains the chapter mapping internally.
    /// For performance, consider caching metrics rather than loading each chapter repeatedly.
    /// </remarks>
    private Ams.Core.Artifacts.Hydrate.HydratedTranscript? LoadHydratedTranscript(string chapterName)
    {
        // If the requested chapter is already selected, just return its hydrate
        if (_workspace.CurrentChapterName == chapterName && _workspace.CurrentChapterHandle != null)
        {
            return _workspace.CurrentChapterHandle.Chapter.Documents.HydratedTranscript;
        }

        // Save the current chapter selection to restore later
        var previousChapter = _workspace.CurrentChapterName;

        // Select the requested chapter
        if (!_workspace.SelectChapter(chapterName))
        {
            _logger.LogDebug("Failed to select chapter '{ChapterName}'", chapterName);
            return null;
        }

        // Get the hydrate from the now-selected chapter
        var hydrate = _workspace.CurrentChapterHandle?.Chapter.Documents.HydratedTranscript;

        // Restore previous chapter selection if it was different
        if (previousChapter != null && previousChapter != chapterName)
        {
            _workspace.SelectChapter(previousChapter);
        }

        return hydrate;
    }
}
