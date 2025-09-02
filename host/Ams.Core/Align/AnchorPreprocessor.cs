using System;
using System.Collections.Generic;
using System.Linq;
using Ams.Core;

namespace Ams.Align.Anchors;

public sealed record BookAnchorView(
    IReadOnlyList<string> Tokens,
    IReadOnlyList<int> SentenceIndex,
    IReadOnlyList<int> FilteredToOriginalWord,
    IReadOnlyList<int> OriginalWordToFiltered
);

public sealed record AsrAnchorView(
    IReadOnlyList<string> Tokens,
    IReadOnlyList<int> FilteredToOriginalToken
);

public static class AnchorPreprocessor
{
    public static BookAnchorView BuildBookView(BookIndex book)
    {
        var tokens = new List<string>(book.Totals.Words);
        var sentIdx = new List<int>(book.Totals.Words);
        var filteredToOriginal = new List<int>(book.Totals.Words);
        var originalToFiltered = Enumerable.Repeat(-1, book.Totals.Words).ToArray();

        for (int i = 0; i < book.Words.Length; i++)
        {
            var tok = AnchorTokenizer.Normalize(book.Words[i].Text);
            if (string.IsNullOrEmpty(tok)) continue;
            originalToFiltered[i] = tokens.Count;
            tokens.Add(tok);
            sentIdx.Add(book.Words[i].SentenceIndex);
            filteredToOriginal.Add(i);
        }

        return new BookAnchorView(tokens, sentIdx, filteredToOriginal, originalToFiltered);
    }

    public static AsrAnchorView BuildAsrView(AsrResponse asr)
    {
        var tokens = new List<string>(asr.Tokens.Length);
        var filteredToOriginal = new List<int>(asr.Tokens.Length);
        for (int i = 0; i < asr.Tokens.Length; i++)
        {
            var tok = AnchorTokenizer.Normalize(asr.Tokens[i].Word);
            if (string.IsNullOrEmpty(tok)) continue;
            filteredToOriginal.Add(i);
            tokens.Add(tok);
        }
        return new AsrAnchorView(tokens, filteredToOriginal);
    }

    public static bool TryMapSectionWindow(BookAnchorView view, (int startWord, int endWord) section, out (int startFiltered, int endFiltered) window)
    {
        int startWord = section.startWord;
        int endWord = section.endWord;
        int startFiltered = -1;
        int endFiltered = -1;

        for (int i = 0; i < view.FilteredToOriginalWord.Count; i++)
        {
            int original = view.FilteredToOriginalWord[i];
            if (original >= startWord && original <= endWord) { startFiltered = i; break; }
        }
        if (startFiltered >= 0)
        {
            for (int i = view.FilteredToOriginalWord.Count - 1; i >= 0; i--)
            {
                int original = view.FilteredToOriginalWord[i];
                if (original >= startWord && original <= endWord) { endFiltered = i; break; }
            }
        }

        if (startFiltered >= 0 && endFiltered >= 0 && startFiltered <= endFiltered)
        {
            window = (startFiltered, endFiltered);
            return true;
        }
        window = default;
        return false;
    }
}

