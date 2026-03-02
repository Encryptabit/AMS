using System.Collections.Frozen;
using System.Reflection;

namespace Ams.Core.Runtime.Book;

/// <summary>
/// Static singleton frequency dictionary for common English words.
/// Loads ~82k words from SymSpell's frequency_dictionary_en_82_765.txt (MIT license).
/// Format: "word frequency_count" per line, sorted by descending frequency.
/// Rank 1 = most common. Used to identify proper nouns and rare/fantasy words during book indexing.
/// </summary>
public static class EnglishFrequencyDictionary
{
    private const string ResourceName = "Ams.Core.Resources.english-frequency-82k.txt";

    private static readonly Lazy<FrozenDictionary<string, int>> LazyDictionary = new(LoadDictionary);

    private static FrozenDictionary<string, int> Dictionary => LazyDictionary.Value;

    /// <summary>Number of entries in the dictionary.</summary>
    public static int Count => Dictionary.Count;

    /// <summary>
    /// Returns the frequency rank (1 = most common) or -1 if not found.
    /// </summary>
    public static int GetRank(string word)
    {
        if (string.IsNullOrEmpty(word))
            return -1;

        // Lowercase the input efficiently - avoid allocation if already lowercase
        var key = word.AsSpan().Equals(word.ToLowerInvariant().AsSpan(), StringComparison.Ordinal)
            ? word
            : word.ToLowerInvariant();

        return Dictionary.TryGetValue(key, out var rank) ? rank : -1;
    }

    /// <summary>
    /// True if the word is absent from the dictionary or has rank greater than rarityThreshold.
    /// </summary>
    public static bool IsRareOrUnknown(string word, int rarityThreshold = 50_000)
    {
        var rank = GetRank(word);
        return rank < 0 || rank > rarityThreshold;
    }

    private static FrozenDictionary<string, int> LoadDictionary()
    {
        var assembly = Assembly.GetExecutingAssembly();
        using var stream = assembly.GetManifestResourceStream(ResourceName)
                           ?? throw new InvalidOperationException(
                               $"Embedded resource '{ResourceName}' not found in assembly.");

        using var reader = new StreamReader(stream);
        var dict = new Dictionary<string, int>(90_000, StringComparer.Ordinal);
        int rank = 1;
        string? line;
        while ((line = reader.ReadLine()) != null)
        {
            var trimmed = line.Trim();
            if (trimmed.Length == 0)
                continue;

            // Format: "word frequency_count" (space-separated, two columns)
            var spaceIdx = trimmed.IndexOf(' ');
            var key = (spaceIdx > 0 ? trimmed[..spaceIdx] : trimmed).ToLowerInvariant();

            dict.TryAdd(key, rank);
            rank++;
        }

        return dict.ToFrozenDictionary(StringComparer.Ordinal);
    }
}
