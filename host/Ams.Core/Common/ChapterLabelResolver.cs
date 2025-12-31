using System.Text.RegularExpressions;

namespace Ams.Core.Common;

/// <summary>
/// Resolves chapter/section labels from various naming conventions.
/// Handles label extraction from directory names and descriptor identifiers.
/// </summary>
public static class ChapterLabelResolver
{
    // Pattern: "03_2_Title" or "05-12 Something" → captures second number
    private static readonly Regex ChapterNumberPattern = new(
        @"^\s*\d+\s*[_-]\s*(\d+)\b",
        RegexOptions.Compiled);

    /// <summary>
    /// Attempts to extract a chapter number from a label string.
    /// Handles patterns like "03_2_Title" or "05-12 Something" extracting the second number.
    /// </summary>
    /// <param name="label">The label to parse (e.g., directory name or chapter ID).</param>
    /// <param name="number">The extracted chapter number if successful.</param>
    /// <returns>True if a chapter number was successfully extracted.</returns>
    public static bool TryExtractChapterNumber(string label, out int number)
    {
        number = 0;
        if (string.IsNullOrWhiteSpace(label))
        {
            return false;
        }

        var match = ChapterNumberPattern.Match(label);
        if (match.Success && int.TryParse(match.Groups[1].Value, out number))
        {
            return true;
        }

        return false;
    }

    /// <summary>
    /// Enumerates candidate labels for section matching from a chapter's identifiers.
    /// Yields the chapter ID first, then the root directory name.
    /// </summary>
    /// <param name="chapterId">The chapter identifier (e.g., "Chapter 01").</param>
    /// <param name="rootPath">The chapter root path (e.g., "C:\Books\01_Chapter").</param>
    /// <returns>Label candidates in priority order.</returns>
    public static IEnumerable<string> EnumerateLabelCandidates(string? chapterId, string? rootPath)
    {
        if (!string.IsNullOrWhiteSpace(chapterId))
        {
            yield return chapterId;
        }

        if (!string.IsNullOrWhiteSpace(rootPath))
        {
            var rootName = Path.GetFileName(
                rootPath.TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar));
            if (!string.IsNullOrWhiteSpace(rootName))
            {
                yield return rootName;
            }
        }
    }
}
