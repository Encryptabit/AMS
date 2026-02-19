using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Ams.Core.Artifacts.Hydrate;
using Ams.Workstation.Server.Models;

namespace Ams.Workstation.Server.Services;

/// <summary>
/// Service for loading chapter data including sentences with timing information.
/// Reads from HydratedTranscript in the workspace.
/// </summary>
public class ChapterDataService
{
    private readonly BlazorWorkspace _workspace;

    public ChapterDataService(BlazorWorkspace workspace)
    {
        _workspace = workspace;
    }

    /// <summary>
    /// Gets the list of sentences for a chapter from the HydratedTranscript.
    /// </summary>
    /// <param name="chapterName">The name of the chapter to load.</param>
    /// <returns>A list of sentences with timing information.</returns>
    public Task<List<SentenceViewModel>> GetSentencesAsync(string chapterName)
    {
        if (_workspace.CurrentChapterHandle is null)
        {
            return Task.FromResult(new List<SentenceViewModel>());
        }

        var hydrate = _workspace.CurrentChapterHandle.Chapter.Documents.HydratedTranscript;
        if (hydrate is null)
        {
            return Task.FromResult(new List<SentenceViewModel>());
        }

        var sentences = hydrate.Sentences.Select(s => new SentenceViewModel
        {
            Id = s.Id,
            Text = s.BookText,
            StartTime = s.Timing?.StartSec ?? 0,
            EndTime = s.Timing?.EndSec ?? 0,
            Status = s.Status ?? "ok",
            HasDiff = s.Diff?.Ops?.Any(op => op.Operation != "equal") == true,
            DiffHtml = BuildDiffHtml(s.Diff)
        }).ToList();

        return Task.FromResult(sentences);
    }

    private static string? BuildDiffHtml(HydratedDiff? diff)
    {
        if (diff?.Ops is null) return null;

        // Build HTML from diff ops: equal=plain, delete=strikethrough, insert=underline
        var sb = new StringBuilder();
        foreach (var op in diff.Ops)
        {
            var tokens = string.Join(" ", op.Tokens);
            sb.Append(op.Operation switch
            {
                "delete" => $"<span class=\"diff-delete\">{tokens}</span>",
                "insert" => $"<span class=\"diff-insert\">{tokens}</span>",
                _ => tokens
            });
            sb.Append(' ');
        }
        return sb.ToString().Trim();
    }

    /// <summary>
    /// Gets a specific sentence by ID.
    /// </summary>
    /// <param name="chapterName">The chapter name.</param>
    /// <param name="sentenceId">The sentence ID.</param>
    /// <returns>The sentence, or null if not found.</returns>
    public async Task<SentenceViewModel?> GetSentenceAsync(string chapterName, int sentenceId)
    {
        var sentences = await GetSentencesAsync(chapterName);
        return sentences.FirstOrDefault(s => s.Id == sentenceId);
    }

    /// <summary>
    /// Gets the total duration of a chapter based on its sentences.
    /// </summary>
    /// <param name="chapterName">The chapter name.</param>
    /// <returns>The total duration in seconds.</returns>
    public async Task<double> GetChapterDurationAsync(string chapterName)
    {
        var sentences = await GetSentencesAsync(chapterName);
        return sentences.Any() ? sentences.Max(s => s.EndTime) : 0;
    }
}
