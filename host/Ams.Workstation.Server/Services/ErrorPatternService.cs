using System.Collections.Generic;
using System.Linq;
using Ams.Core.Artifacts.Hydrate;
using Ams.Workstation.Server.Models;

namespace Ams.Workstation.Server.Services;

/// <summary>
/// Service for aggregating error patterns across all chapters in a book.
/// Analyzes HydratedTranscript diff operations to identify recurring differences.
/// </summary>
/// <remarks>
/// Performance consideration: This service loads all chapters' hydrate files and processes all sentences.
/// For large books (50+ chapters, 10k+ sentences), this may be slow (5-10 seconds).
/// Future optimization: cache patterns in BlazorWorkspace after first aggregation.
/// </remarks>
public class ErrorPatternService
{
    private readonly BlazorWorkspace _workspace;
    private const int MinimumOccurrences = 5;

    public ErrorPatternService(BlazorWorkspace workspace)
    {
        _workspace = workspace;
    }

    /// <summary>
    /// Aggregate error patterns across all chapters in the workspace.
    /// </summary>
    /// <param name="ignoredKeys">Set of pattern keys to mark as ignored.</param>
    /// <returns>Aggregated patterns sorted by occurrence count (descending).</returns>
    public ErrorPatternsResult AggregatePatterns(ISet<string>? ignoredKeys = null)
    {
        var patterns = new Dictionary<string, PatternAggregate>();
        ignoredKeys ??= new HashSet<string>();

        // Iterate all available chapters
        foreach (var chapterTitle in _workspace.AvailableChapters)
        {
            if (!_workspace.TryGetHydratedTranscript(chapterTitle, out var hydrate) || hydrate is null)
            {
                continue;
            }

            // Extract patterns from each sentence's diff
            foreach (var sentence in hydrate.Sentences)
            {
                foreach (var (type, book, script) in ExtractPatterns(sentence.Diff))
                {
                    var key = BuildKey(type, book, script);

                    if (!patterns.TryGetValue(key, out var aggregate))
                    {
                        aggregate = new PatternAggregate(type, book, script);
                        patterns[key] = aggregate;
                    }

                    aggregate.Count++;
                    if (aggregate.Examples.Count < 3)
                    {
                        aggregate.Examples.Add(new PatternExample(chapterTitle, sentence.Id));
                    }
                }
            }
        }

        // Build final result with top 3 examples each, sorted by count descending
        var result = patterns
            .Where(kvp => kvp.Value.Count >= MinimumOccurrences)
            .Select(kvp => new ErrorPattern(
                Key: kvp.Key,
                Type: kvp.Value.Type,
                Book: kvp.Value.Book,
                Script: kvp.Value.Script,
                Count: kvp.Value.Count,
                Examples: kvp.Value.Examples
            ) { Ignored = ignoredKeys.Contains(kvp.Key) })
            .OrderByDescending(p => p.Count)
            .ToList();

        return new ErrorPatternsResult(result, ignoredKeys.ToList());
    }

    /// <summary>
    /// Extract patterns from a single sentence's diff operations.
    /// </summary>
    /// <remarks>
    /// Pattern extraction rules:
    /// - Consecutive (delete, insert) = substitution (sub)
    /// - Standalone delete = deletion (del)
    /// - Standalone insert = insertion (ins)
    /// - "equal" operations are ignored
    /// </remarks>
    private IEnumerable<(string Type, string Book, string Script)> ExtractPatterns(HydratedDiff? diff)
    {
        if (diff?.Ops == null || diff.Ops.Count == 0)
            yield break;

        var ops = diff.Ops.ToList();
        for (int i = 0; i < ops.Count; i++)
        {
            var op = ops[i];

            if (op.Operation == "delete")
            {
                // Check if next op is insert (= substitution)
                if (i + 1 < ops.Count && ops[i + 1].Operation == "insert")
                {
                    var next = ops[i + 1];
                    var bookText = string.Join(" ", op.Tokens);
                    var scriptText = string.Join(" ", next.Tokens);
                    yield return ("sub", bookText, scriptText);
                    i++; // Skip next op (already consumed)
                }
                else
                {
                    // Standalone deletion
                    var bookText = string.Join(" ", op.Tokens);
                    yield return ("del", bookText, "");
                }
            }
            else if (op.Operation == "insert")
            {
                // Standalone insert (not part of substitution - those are caught above)
                var scriptText = string.Join(" ", op.Tokens);
                yield return ("ins", "", scriptText);
            }
            // "equal" ops are ignored
        }
    }

    /// <summary>
    /// Build unique pattern key from type and text values.
    /// </summary>
    public static string BuildKey(string type, string book, string script)
        => $"{type}|{book}|{script}";

    /// <summary>
    /// In-memory aggregation state for a single pattern key.
    /// </summary>
    private sealed class PatternAggregate
    {
        public PatternAggregate(string type, string book, string script)
        {
            Type = type;
            Book = book;
            Script = script;
        }

        public string Type { get; }
        public string Book { get; }
        public string Script { get; }
        public int Count { get; set; }
        public List<PatternExample> Examples { get; } = new(3);
    }
}
