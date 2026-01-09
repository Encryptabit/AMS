using System.Text.Json;
using System.Text.RegularExpressions;
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
                .OrderBy(file => file, ChapterFileComparer.Instance)
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

            // Both unmatched: use numeric-aware stem comparison
            return ChapterFileComparer.CompareStemStrings(a.Stem, b.Stem);
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

    /// <summary>
    /// Numeric-aware file comparer for sorting chapter files.
    /// Extracted from CLI REPL's ChapterFileComparer.
    /// </summary>
    internal sealed class ChapterFileComparer : IComparer<FileInfo>
    {
        public static readonly ChapterFileComparer Instance = new();

        private static readonly Regex NumberRegex = new(@"\d+", RegexOptions.Compiled);

        public int Compare(FileInfo? x, FileInfo? y)
        {
            if (ReferenceEquals(x, y)) return 0;
            if (x is null) return -1;
            if (y is null) return 1;

            var keyX = GetSortKey(x);
            var keyY = GetSortKey(y);

            var category = keyX.Category.CompareTo(keyY.Category);
            if (category != 0) return category;

            var number = keyX.PrimaryNumber.CompareTo(keyY.PrimaryNumber);
            if (number != 0) return number;

            var name = string.Compare(keyX.NameLower, keyY.NameLower, StringComparison.Ordinal);
            if (name != 0) return name;

            return string.Compare(x.Name, y.Name, StringComparison.OrdinalIgnoreCase);
        }

        /// <summary>
        /// Compares two stem strings using numeric-aware logic.
        /// Used when FileInfo is not available.
        /// </summary>
        internal static int CompareStemStrings(string? x, string? y)
        {
            if (ReferenceEquals(x, y)) return 0;
            if (x is null) return -1;
            if (y is null) return 1;

            var keyX = GetStemSortKey(x);
            var keyY = GetStemSortKey(y);

            var category = keyX.Category.CompareTo(keyY.Category);
            if (category != 0) return category;

            var number = keyX.PrimaryNumber.CompareTo(keyY.PrimaryNumber);
            if (number != 0) return number;

            return string.Compare(keyX.NameLower, keyY.NameLower, StringComparison.Ordinal);
        }

        private static SortKey GetSortKey(FileInfo file)
        {
            var stem = Path.GetFileNameWithoutExtension(file.Name);
            return GetStemSortKey(stem);
        }

        private static SortKey GetStemSortKey(string stem)
        {
            var match = NumberRegex.Match(stem);
            if (match.Success && int.TryParse(match.Value, out var primary))
            {
                return new SortKey(0, primary, stem.ToLowerInvariant());
            }

            return new SortKey(1, int.MaxValue, stem.ToLowerInvariant());
        }

        private readonly record struct SortKey(int Category, int PrimaryNumber, string NameLower);
    }
}
