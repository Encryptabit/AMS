using System.Text.Json;
using Ams.Core.Common;
using Ams.Core.Processors.Alignment.Anchors;
using Ams.Core.Runtime.Book;

namespace Ams.Core.Runtime.Chapter;

/// <summary>
/// Discovered chapter information derived from WAV files and book index matching.
/// </summary>
/// <param name="Stem">The file name without extension (e.g., "03_CultistOfCerebon2_Ch3").</param>
/// <param name="DisplayTitle">The matched section title from book index, or stem if no match.</param>
/// <param name="WavPath">Full path to the WAV file.</param>
public sealed record ChapterInfo(string Stem, string DisplayTitle, string WavPath);

/// <summary>
/// Service for discovering chapters in a directory by matching WAV files to book index sections.
/// Consolidates chapter scanning logic used by CLI REPL and Blazor workstation.
/// </summary>
public static class ChapterDiscoveryService
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };

    /// <summary>
    /// Discovers chapters by scanning for WAV files and matching them to book index sections.
    /// </summary>
    /// <param name="rootPath">Directory containing WAV files and book-index.json.</param>
    /// <returns>
    /// List of discovered chapters, sorted by book index order (matched chapters first),
    /// then by numeric-aware file name sorting for unmatched chapters.
    /// </returns>
    public static IReadOnlyList<ChapterInfo> DiscoverChapters(string rootPath)
    {
        ArgumentException.ThrowIfNullOrEmpty(rootPath);

        if (!Directory.Exists(rootPath))
        {
            return Array.Empty<ChapterInfo>();
        }

        var bookIndex = LoadBookIndex(rootPath);
        return DiscoverChaptersCore(rootPath, bookIndex);
    }

    /// <summary>
    /// Discovers chapters using a pre-loaded book index.
    /// </summary>
    /// <param name="rootPath">Directory containing WAV files.</param>
    /// <param name="bookIndex">Book index for section matching, or null for stem-only discovery.</param>
    /// <returns>
    /// List of discovered chapters, sorted by book index order (matched chapters first),
    /// then by numeric-aware file name sorting for unmatched chapters.
    /// </returns>
    public static IReadOnlyList<ChapterInfo> DiscoverChapters(string rootPath, BookIndex? bookIndex)
    {
        ArgumentException.ThrowIfNullOrEmpty(rootPath);

        if (!Directory.Exists(rootPath))
        {
            return Array.Empty<ChapterInfo>();
        }

        return DiscoverChaptersCore(rootPath, bookIndex);
    }

    private static IReadOnlyList<ChapterInfo> DiscoverChaptersCore(string rootPath, BookIndex? bookIndex)
    {
        List<FileInfo> wavFiles;
        try
        {
            wavFiles = Directory.EnumerateFiles(rootPath, "*.wav", SearchOption.TopDirectoryOnly)
                .Select(path => new FileInfo(path))
                .OrderBy(file => file, NaturalStringComparer.FileNameWithoutExtensionIgnoreCase)
                .ToList();
        }
        catch (Exception)
        {
            return Array.Empty<ChapterInfo>();
        }

        if (wavFiles.Count == 0)
        {
            return Array.Empty<ChapterInfo>();
        }

        // Build section order lookup for sorting
        var sectionOrder = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);
        if (bookIndex?.Sections is { Length: > 0 })
        {
            for (int i = 0; i < bookIndex.Sections.Length; i++)
            {
                var title = bookIndex.Sections[i].Title;
                if (!string.IsNullOrEmpty(title) && !sectionOrder.ContainsKey(title))
                {
                    sectionOrder[title] = i;
                }
            }
        }

        var chapters = new List<ChapterInfo>(wavFiles.Count);
        var usedTitles = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        foreach (var wavFile in wavFiles)
        {
            var stem = Path.GetFileNameWithoutExtension(wavFile.Name);
            if (string.IsNullOrEmpty(stem))
            {
                continue;
            }

            string displayTitle = stem;

            // Try to match to a section in the book index
            if (bookIndex is not null)
            {
                var section = SectionLocator.ResolveSectionByTitle(bookIndex, stem);
                if (section?.Title is { Length: > 0 } title && !usedTitles.Contains(title))
                {
                    displayTitle = title;
                    usedTitles.Add(title);
                }
            }

            chapters.Add(new ChapterInfo(stem, displayTitle, wavFile.FullName));
        }

        // Sort: matched chapters by book order, unmatched by file order (already in correct order)
        chapters.Sort((a, b) =>
        {
            var aMatched = sectionOrder.TryGetValue(a.DisplayTitle, out var aIdx);
            var bMatched = sectionOrder.TryGetValue(b.DisplayTitle, out var bIdx);

            if (aMatched && bMatched)
            {
                return aIdx.CompareTo(bIdx);
            }

            if (aMatched != bMatched)
            {
                // Matched chapters come first
                return aMatched ? -1 : 1;
            }

            // Both unmatched: use natural numeric-aware stem comparison.
            return NaturalStringComparer.CompareIgnoreCase(a.Stem, b.Stem);
        });

        return chapters;
    }

    private static BookIndex? LoadBookIndex(string rootPath)
    {
        var indexPath = Path.Combine(rootPath, "book-index.json");
        if (!File.Exists(indexPath))
        {
            return null;
        }

        try
        {
            var json = File.ReadAllText(indexPath);
            return JsonSerializer.Deserialize<BookIndex>(json, JsonOptions);
        }
        catch
        {
            return null;
        }
    }

}
