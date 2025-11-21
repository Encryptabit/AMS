namespace Ams.Core.Runtime.Book;

public static class BookPhonemePopulator
{
    private const int MaxPhonemeVariantsPerWord = 32;

    public static async Task<BookIndex> PopulateMissingAsync(
        BookIndex index,
        IPronunciationProvider pronunciationProvider,
        CancellationToken cancellationToken = default)
    {
        if (index.Words == null || index.Words.Length == 0)
        {
            return index;
        }

        if (pronunciationProvider == null)
        {
            throw new ArgumentNullException(nameof(pronunciationProvider));
        }

        var missingLexemes = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        foreach (var word in index.Words)
        {
            if (HasPhonemes(word))
            {
                continue;
            }

            var lexeme = PronunciationHelper.NormalizeForLookup(word.Text);
            if (!string.IsNullOrEmpty(lexeme))
            {
                missingLexemes.Add(lexeme);
            }
        }

        Log.Debug("Phoneme population: {MissingCount} tokens missing phonemes; {LexemeCount} unique lexemes queued", index.Words.Count(w => !HasPhonemes(w)), missingLexemes.Count);

        if (missingLexemes.Count == 0)
        {
            return index;
        }

        var pronunciations = await pronunciationProvider
            .GetPronunciationsAsync(missingLexemes, cancellationToken)
            .ConfigureAwait(false);

        if (pronunciations.Count == 0)
        {
            return index;
        }

        var updatedWords = new BookWord[index.Words.Length];
        for (int i = 0; i < index.Words.Length; i++)
        {
            var word = index.Words[i];
            if (!HasPhonemes(word))
            {
                var lexeme = PronunciationHelper.NormalizeForLookup(word.Text);
                if (!string.IsNullOrEmpty(lexeme) && pronunciations.TryGetValue(lexeme, out var variants) && variants.Length > 0)
                {
                    updatedWords[i] = word with { Phonemes = MergeVariants(word.Phonemes, variants) };
                    continue;
                }
            }

            updatedWords[i] = word;
        }

        return index with { Words = updatedWords };
    }

    private static bool HasPhonemes(BookWord word)
        => word.Phonemes is { Length: >0 };

    private static string[] MergeVariants(string[]? current, string[] incoming)
    {
        var comparer = StringComparer.OrdinalIgnoreCase;
        var set = new HashSet<string>(comparer);

        void AddRange(IEnumerable<string>? source)
        {
            if (source is null)
            {
                return;
            }

            foreach (var variant in source)
            {
                if (string.IsNullOrWhiteSpace(variant))
                {
                    continue;
                }

                var trimmed = variant.Trim();
                if (trimmed.Length == 0)
                {
                    continue;
                }

                set.Add(trimmed);
                if (set.Count >= MaxPhonemeVariantsPerWord)
                {
                    break;
                }
            }
        }

        AddRange(current);
        if (set.Count < MaxPhonemeVariantsPerWord)
        {
            AddRange(incoming);
        }

        return set.ToArray();
    }
}
