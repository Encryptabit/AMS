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
    public Task<List<SentenceViewModel>> GetSentencesAsync(string chapterName, IReadOnlySet<string>? ignoredKeys = null)
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

        var sentences = hydrate.Sentences.Select(s =>
        {
            var diffHtml = BuildDiffHtml(s.Diff, ignoredKeys);
            return new SentenceViewModel
            {
                Id = s.Id,
                Text = s.BookText,
                StartTime = s.Timing?.StartSec ?? 0,
                EndTime = s.Timing?.EndSec ?? 0,
                Status = s.Status ?? "ok",
                HasDiff = HasVisibleDiff(s.Diff, ignoredKeys),
                DiffHtml = diffHtml
            };
        }).ToList();

        return Task.FromResult(sentences);
    }

    private static bool HasVisibleDiff(HydratedDiff? diff, IReadOnlySet<string>? ignoredKeys)
    {
        if (diff?.Ops is null) return false;
        if (ignoredKeys is null || ignoredKeys.Count == 0)
            return diff.Ops.Any(op => op.Operation != "equal");

        var ops = diff.Ops.ToList();
        for (int i = 0; i < ops.Count; i++)
        {
            var op = ops[i];
            if (op.Operation == "equal") continue;

            if (op.Operation == "delete")
            {
                var bookText = string.Join(" ", op.Tokens);
                if (i + 1 < ops.Count && ops[i + 1].Operation == "insert")
                {
                    var scriptText = string.Join(" ", ops[i + 1].Tokens);
                    if (!ignoredKeys.Contains(ErrorPatternService.BuildKey("sub", bookText, scriptText)))
                        return true;
                    i++;
                }
                else
                {
                    if (!ignoredKeys.Contains(ErrorPatternService.BuildKey("del", bookText, "")))
                        return true;
                }
            }
            else if (op.Operation == "insert")
            {
                var scriptText = string.Join(" ", op.Tokens);
                if (!ignoredKeys.Contains(ErrorPatternService.BuildKey("ins", "", scriptText)))
                    return true;
            }
        }
        return false;
    }

    private static string? BuildDiffHtml(HydratedDiff? diff, IReadOnlySet<string>? ignoredKeys)
    {
        if (diff?.Ops is null) return null;

        var sb = new StringBuilder();
        var ops = diff.Ops.ToList();
        var hasIgnored = ignoredKeys is not null && ignoredKeys.Count > 0;

        for (int i = 0; i < ops.Count; i++)
        {
            var op = ops[i];
            var tokens = string.Join(" ", op.Tokens);

            if (op.Operation == "equal")
            {
                sb.Append(tokens);
            }
            else if (op.Operation == "delete")
            {
                var bookText = tokens;
                if (i + 1 < ops.Count && ops[i + 1].Operation == "insert")
                {
                    var next = ops[i + 1];
                    var scriptText = string.Join(" ", next.Tokens);
                    if (hasIgnored && ignoredKeys!.Contains(ErrorPatternService.BuildKey("sub", bookText, scriptText)))
                    {
                        sb.Append(scriptText);
                        i++;
                    }
                    else
                    {
                        sb.Append($"<span class=\"diff-delete\">{bookText}</span>");
                    }
                }
                else
                {
                    if (hasIgnored && ignoredKeys!.Contains(ErrorPatternService.BuildKey("del", bookText, "")))
                    {
                        // Suppress ignored standalone deletion
                    }
                    else
                    {
                        sb.Append($"<span class=\"diff-delete\">{bookText}</span>");
                    }
                }
            }
            else if (op.Operation == "insert")
            {
                if (hasIgnored && ignoredKeys!.Contains(ErrorPatternService.BuildKey("ins", "", tokens)))
                {
                    sb.Append(tokens);
                }
                else
                {
                    sb.Append($"<span class=\"diff-insert\">{tokens}</span>");
                }
            }
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
