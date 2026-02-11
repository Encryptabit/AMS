using System.Collections.Generic;

namespace Ams.Workstation.Server.Models;

/// <summary>
/// Represents an aggregated error pattern found across chapters.
/// </summary>
/// <param name="Key">Unique identifier: "{type}|{book}|{script}" for deduplication.</param>
/// <param name="Type">Pattern type: "del" (deleted from book), "ins" (inserted in script), "sub" (substitution).</param>
/// <param name="Book">Expected text from the book (from delete ops or substitution source).</param>
/// <param name="Script">Actual text from the script/ASR (from insert ops or substitution target).</param>
/// <param name="Count">Total occurrences across all chapters in the book.</param>
/// <param name="Ignored">Whether this pattern is marked as ignored (acceptable variation).</param>
/// <param name="Examples">First 3 occurrences with chapter and sentence references.</param>
public record ErrorPattern(
    string Key,
    string Type,
    string Book,
    string Script,
    int Count,
    IReadOnlyList<PatternExample> Examples)
{
    public bool Ignored { get; set; }
}

/// <summary>
/// Reference to a specific occurrence of an error pattern.
/// </summary>
/// <param name="Chapter">Chapter display name where the pattern occurs.</param>
/// <param name="SentenceId">Sentence ID within the chapter.</param>
public record PatternExample(string Chapter, int SentenceId);

/// <summary>
/// Result of aggregating error patterns across the book.
/// </summary>
/// <param name="Patterns">All error patterns sorted by occurrence count (descending).</param>
/// <param name="IgnoredKeys">List of pattern keys marked as ignored.</param>
public record ErrorPatternsResult(
    IReadOnlyList<ErrorPattern> Patterns,
    IReadOnlyList<string> IgnoredKeys);
