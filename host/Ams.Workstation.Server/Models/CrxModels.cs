using System;
using System.Linq;
using System.Text.RegularExpressions;

namespace Ams.Workstation.Server.Models;

public record CrxEntry(
    int ErrorNumber,
    string Chapter,
    string Timecode,          // HH:MM:SS format
    string ErrorType,         // MR, PRON, DIC, NZ, PL, DIST, MW, ML, TYPO, CHAR
    string Comments,
    int SentenceId,
    double StartTime,
    double EndTime,
    string AudioFile,
    DateTime CreatedAt
);

public record CrxSubmitRequest(
    double Start,
    double End,
    int SentenceId,
    string ErrorType,
    string Comments,
    string Excerpt,
    int PaddingMs = 50
);

public record CrxSubmitResult(
    bool Success,
    int ErrorNumber,
    string Timecode,
    string AudioFile,
    string? Error = null
);

public static class ErrorTypes
{
    public static readonly string[] All = new[]
    {
        "MR",    // Misread
        "PRON",  // Pronunciation
        "DIC",   // Diction
        "NZ",    // Noise
        "PL",    // Plosive
        "DIST",  // Distortion
        "MW",    // Missing Word
        "ML",    // Missing Line
        "TYPO",  // Typo in script
        "CHAR"   // Character voice
    };
}

public static class CrxCommentParser
{
    private static readonly Regex ShouldBeRegex = new(@"^Should be:\s*(.+?)$", RegexOptions.Multiline | RegexOptions.Compiled);
    private static readonly Regex ReadAsRegex = new(@"^Read as:\s*(.+?)$", RegexOptions.Multiline | RegexOptions.Compiled);

    public static string? TryParseShouldBe(string? comments)
    {
        if (string.IsNullOrWhiteSpace(comments)) return null;
        var match = ShouldBeRegex.Match(comments);
        return match.Success ? StripBrackets(match.Groups[1].Value.Trim()) : null;
    }

    public static string? TryParseReadAs(string? comments)
    {
        if (string.IsNullOrWhiteSpace(comments)) return null;
        var match = ReadAsRegex.Match(comments);
        return match.Success ? StripBrackets(match.Groups[1].Value.Trim()) : null;
    }

    public static string StripBrackets(string text)
        => text.Replace("[", "").Replace("]", "").Trim();
}

internal static class CrxChapterMatcher
{
    private static readonly Regex ChapterLabelRegex = new(@"^(chapter|epilogue)\s+(\d+)\b", RegexOptions.IgnoreCase | RegexOptions.Compiled);
    private static readonly Regex CollapseWhitespaceRegex = new(@"\s+", RegexOptions.Compiled);
    private static readonly Regex NonWordRegex = new(@"[^\w\s]", RegexOptions.Compiled);

    public static bool Matches(string? crxChapter, string? currentChapter)
    {
        if (string.IsNullOrWhiteSpace(crxChapter) || string.IsNullOrWhiteSpace(currentChapter))
            return false;

        var crxLabel = TryParseLabel(crxChapter);
        var currentLabel = TryParseLabel(currentChapter);
        if (crxLabel.HasValue && currentLabel.HasValue)
        {
            return crxLabel.Value.Kind == currentLabel.Value.Kind &&
                   crxLabel.Value.Number == currentLabel.Value.Number;
        }

        return string.Equals(
            NormalizeForCompare(crxChapter),
            NormalizeForCompare(currentChapter),
            StringComparison.Ordinal);
    }

    private static (string Kind, int Number)? TryParseLabel(string chapterLabel)
    {
        var match = ChapterLabelRegex.Match(chapterLabel);
        if (!match.Success)
            return null;

        return int.TryParse(match.Groups[2].Value, out var number)
            ? (match.Groups[1].Value.ToLowerInvariant(), number)
            : null;
    }

    private static string NormalizeForCompare(string? text)
    {
        if (string.IsNullOrWhiteSpace(text))
            return string.Empty;

        var normalized = NonWordRegex.Replace(text.ToLowerInvariant().Trim(), " ");
        return CollapseWhitespaceRegex.Replace(normalized, " ").Trim();
    }
}
