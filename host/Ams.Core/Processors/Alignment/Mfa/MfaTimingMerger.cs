// MfaTimingMerger.cs
// Robust, token-aware merger of MFA TextGrid timings into hydrate using global alignment.
// - Handles count mismatches (e.g., 1140 TG tokens vs 1119 book tokens)
// - Treats <unk>/unk as a wildcard for the current book token
// - Aligns with Needleman–Wunsch (global) and applies timings
// - Hyphen / quote / unicode normalization made symmetric
//
// Integration notes:
//  - Provide TextGridWord[] from your TextGrid "words" tier.
//  - Provide getBookToken(bookIdx) for indices in [chapterStartBookIdx, chapterEndBookIdx].
//  - Provide WordTarget/SentenceTarget adapters for your hydrate structures.

using System.Globalization;
using System.Text;

namespace Ams.Core.Processors.Alignment.Mfa
{
    // -------- Public surface -------------------------------------------------

    public static class MfaTimingMerger
    {
        /// <summary>
        /// Aligns TextGrid tokens to book tokens and applies timings to hydrate words/sentences.
        /// </summary>
        /// <param name="textGridWords">TextGrid "word" intervals (non-empty text only)</param>
        /// <param name="getBookToken">Fetcher for raw book token by bookIdx</param>
        /// <param name="chapterStartBookIdx">Inclusive start of the chapter word window</param>
        /// <param name="chapterEndBookIdx">Inclusive end of the chapter word window</param>
        /// <param name="wordTargets">Adapters for hydrate words (provide BookIdx and a setter)</param>
        /// <param name="sentenceTargets">Adapters for hydrate sentences (provide book range and setter)</param>
        /// <param name="debugLog">Optional debug logger (e.g., s => Log.Debug(s))</param>
        public static MergeReport MergeAndApply(
            IEnumerable<TextGridWord> textGridWords,
            Func<int, string> getBookToken,
            int chapterStartBookIdx,
            int chapterEndBookIdx,
            IEnumerable<WordTarget> wordTargets,
            IEnumerable<SentenceTarget> sentenceTargets,
            Action<string>? debugLog = null)
        {
            // 1) Build normalized token streams
            var tgTokens   = BuildTimedTgTokens(textGridWords);
            var bookTokens = BuildBookTokens(getBookToken, chapterStartBookIdx, chapterEndBookIdx);

            // 2) Global alignment (unk treated as wildcard)
            var ar = Align(bookTokens, tgTokens);

            // 3) Build bookIdx -> (start,end) timing map (merge duplicates)
            var timingMap = BuildBookTimingMap(ar, tgTokens);

            // 4) Apply to hydrate words & sentences
            int wordsUpdated     = ApplyWordTimings(timingMap, wordTargets);
            int sentencesUpdated = ApplySentenceTimings(timingMap, sentenceTargets);

            // 5) Log summary
            debugLog?.Invoke(
                $"MFA alignment: TG tokens={tgTokens.Count}, Book tokens={bookTokens.Count}, " +
                $"pairs={ar.Pairs.Count}, match={ar.Matches}, wild={ar.WildMatches}, " +
                $"ins={ar.Insertions}, del={ar.Deletions}. " +
                $"Updated {sentencesUpdated} sentences and {wordsUpdated} words."
            );

            return new MergeReport
            {
                TextGridTokenCount = tgTokens.Count,
                BookTokenCount     = bookTokens.Count,
                Pairs              = ar.Pairs.Count,
                Matches            = ar.Matches,
                WildMatches        = ar.WildMatches,
                Insertions         = ar.Insertions,
                Deletions          = ar.Deletions,
                WordsUpdated       = wordsUpdated,
                SentencesUpdated   = sentencesUpdated
            };
        }

        // -------- Core algorithm ---------------------------------------------

        private static List<TgTok> BuildTimedTgTokens(IEnumerable<TextGridWord> intervals)
        {
            var list = new List<TgTok>();
            var seq = 0;

            foreach (var it in intervals)
            {
                if (string.IsNullOrWhiteSpace(it.Text)) continue;
                if (!(it.End > it.Start)) continue;

                var emitted = false;
                foreach (var tok in TokenizeForAlignment(it.Text, forTextGrid: true))
                {
                    list.Add(new TgTok(seq++, tok, it.Start, it.End));
                    emitted = true;
                }

                if (!emitted)
                {
                    list.Add(new TgTok(seq++, UNK, it.Start, it.End));
                }
            }

            return list;
        }

        private static List<BookTok> BuildBookTokens(
            Func<int, string> getBookToken,
            int startIdx,
            int endIdx)
        {
            if (endIdx < startIdx) return new List<BookTok>();

            var list = new List<BookTok>();

            for (int i = startIdx; i <= endIdx; i++)
            {
                var raw = getBookToken(i) ?? "";

                // CRITICAL: We may produce multiple tokens for one bookIdx (e.g., hyphenated),
                // and later MERGE their timings for that single word index.
                var emitted = false;
                foreach (var tok in TokenizeForAlignment(raw, forTextGrid: false))
                {
                    list.Add(new BookTok(i, tok));
                    emitted = true;
                }
                if (!emitted)
                {
                    // Keep a placeholder so indexing doesn't drift.
                    list.Add(new BookTok(i, "")); // empty tokens won't match anything
                }
            }

            return list;
        }

        // ---- Global alignment (Needleman–Wunsch) ----------------------------

        private const int MATCH =  2; // exact token match
        private const int WILD  =  2; // tg == UNK wildcard maps to current book token
        private const int MISM  = -2; // mismatch
        private const int GAP   = -1; // insertion / deletion

        private static bool Eq(string a, string b) => a == b;
        private static bool IsWild(string t) => t == UNK;

        private static AlignmentResult Align(List<BookTok> book, List<TgTok> tg)
        {
            int n = book.Count, m = tg.Count;

            var dp = new int[n + 1, m + 1];
            var bt = new byte[n + 1, m + 1]; // 1=diag, 2=up (book gap), 3=left (tg gap)

            for (int i = 1; i <= n; i++) { dp[i,0] = i * GAP; bt[i,0] = 2; }
            for (int j = 1; j <= m; j++) { dp[0,j] = j * GAP; bt[0,j] = 3; }

            for (int i = 1; i <= n; i++)
            {
                var bi = book[i - 1].Tok;
                for (int j = 1; j <= m; j++)
                {
                    var tj = tg[j - 1].Tok;
                    int sMatch = Eq(bi, tj) ? MATCH : (IsWild(tj) ? WILD : MISM);

                    int d = dp[i - 1, j - 1] + sMatch;
                    int u = dp[i - 1, j] + GAP;
                    int l = dp[i, j - 1] + GAP;

                    int best = d; byte move = 1;
                    if (u > best) { best = u; move = 2; }
                    if (l > best) { best = l; move = 3; }

                    dp[i, j] = best;
                    bt[i, j] = move;
                }
            }

            var pairs = new List<Pair>(Math.Max(n, m));
            int matches = 0, wildMatches = 0, insertions = 0, deletions = 0;

            // Backtrace
            int ii = n, jj = m;
            while (ii > 0 || jj > 0)
            {
                byte move = bt[ii, jj];

                if (move == 1) // diag
                {
                    var bTok = book[ii - 1].Tok;
                    var tTok = tg[jj - 1].Tok;

                    if (Eq(bTok, tTok))
                    {
                        matches++;
                        pairs.Add(new Pair(book[ii - 1].BookIdx, tg[jj - 1].TgSeq));
                    }
                    else if (IsWild(tTok))
                    {
                        wildMatches++;
                        pairs.Add(new Pair(book[ii - 1].BookIdx, tg[jj - 1].TgSeq));
                    }
                    // else diag mismatch: skip emitting a pair

                    ii--; jj--;
                }
                else if (move == 2) { deletions++; ii--; }  // gap in tg (drop a book token)
                else if (move == 3) { insertions++; jj--; } // gap in book (drop a tg token)
                else break;
            }

            pairs.Reverse();

            return new AlignmentResult(pairs, matches, wildMatches, insertions, deletions);
        }

        // ---- Build timing map and apply ------------------------------------

        private static Dictionary<int, (double start, double end)> BuildBookTimingMap(
            AlignmentResult ar,
            List<TgTok> tg)
        {
            var map = new Dictionary<int, (double start, double end)>();

            // Pre-lookup for tgSeq -> (start,end)
            var tgTimes = tg.ToDictionary(x => x.TgSeq, x => (x.Start, x.End));

            foreach (var p in ar.Pairs)
            {
                var (start, end) = tgTimes[p.TgSeq];

                if (map.TryGetValue(p.BookIdx, out var cur))
                {
                    // Merge multiple TG tokens mapped to the same bookIdx (e.g., hyphen splits):
                    // take earliest start and latest end
                    map[p.BookIdx] = (Math.Min(cur.start, start), Math.Max(cur.end, end));
                }
                else
                {
                    map[p.BookIdx] = (start, end);
                }
            }

            return map;
        }

        private static int ApplyWordTimings(
            Dictionary<int, (double start, double end)> timingMap,
            IEnumerable<WordTarget> wordTargets)
        {
            int updated = 0;

            foreach (var w in wordTargets)
            {
                if (w.BookIdx is int bi && timingMap.TryGetValue(bi, out var t))
                {
                    var (s, e) = t;
                    w.SetTiming(s, e, e - s);
                    updated++;
                }
            }

            return updated;
        }

        private static int ApplySentenceTimings(
            Dictionary<int, (double start, double end)> timingMap,
            IEnumerable<SentenceTarget> sentences)
        {
            int updated = 0;

            foreach (var s in sentences)
            {
                double? start = null, end = null;

                for (int i = s.BookStartIdx; i <= s.BookEndIdx; i++)
                {
                    if (!timingMap.TryGetValue(i, out var t)) continue;
                    start = start is null ? t.start : Math.Min(start.Value, t.start);
                    end   = end   is null ? t.end   : Math.Max(end.Value, t.end);
                }

                if (start is not null && end is not null)
                {
                    s.SetTiming(start.Value, end.Value, end.Value - start.Value);
                    updated++;
                }
            }

            return updated;
        }

        // -------- Normalization & tokenization -------------------------------

        private const string UNK = "unk";

        private static IEnumerable<string> TokenizeForAlignment(string? s, bool forTextGrid)
        {
            if (string.IsNullOrWhiteSpace(s)) yield break;

            s = SafeNormalize(s);
            if (string.IsNullOrWhiteSpace(s)) yield break;

            // Unify quotes/dashes
            s = s.Replace('“', '"').Replace('”', '"')
                 .Replace('‘', '\'').Replace('’', '\'')
                 .Replace('—', '-').Replace('–', '-');

            // Lowercase
            s = s.ToLowerInvariant();

            // Treat hyphens as separators to keep TG/book tokenization symmetric
            s = s.Replace('-', ' ');

            // Split by whitespace
            var parts = s.Split((char[]?)null, StringSplitOptions.RemoveEmptyEntries);

            foreach (var raw in parts)
            {
                var tok = TrimPunct(raw);

                if (string.IsNullOrEmpty(tok)) continue;

                // Treat MFA unknown/special as wildcard on the TG side
                if (forTextGrid && (tok == "<unk>" || tok == UNK || tok == "sp" || tok == "sil"))
                {
                    yield return UNK; // sentinel wildcard
                }
                else
                {
                    yield return tok;
                }
            }
        }

        private static string TrimPunct(string t)
        {
            int i = 0, j = t.Length - 1;
            while (i <= j && IsPunctToTrim(t[i])) i++;
            while (j >= i && IsPunctToTrim(t[j])) j--;
            return (i <= j) ? t.Substring(i, j - i + 1) : "";
        }

        private static string SafeNormalize(string input)
        {
            if (string.IsNullOrEmpty(input))
            {
                return input ?? string.Empty;
            }

            Span<char> tmp = stackalloc char[4];

            var builder = new StringBuilder(input.Length);
            for (int i = 0; i < input.Length;)
            {
                var c = input[i];

                if (char.IsSurrogate(c))
                {
                    if (i + 1 < input.Length && char.IsSurrogatePair(c, input[i + 1]))
                    {
                        tmp[0] = c;
                        tmp[1] = input[i + 1];
                        builder.Append(tmp.Slice(0,2));
                        i += 2;
                        continue;
                    }
                    i++;
                    continue;
                }

                if (char.IsControl(c) && !char.IsWhiteSpace(c))
                {
                    i++;
                    continue;
                }

                builder.Append(c);
                i++;
            }

            var cleaned = builder.ToString();
            if (cleaned.Length == 0)
            {
                return string.Empty;
            }

            try
            {
                return cleaned.Normalize(NormalizationForm.FormKC);
            }
            catch (ArgumentException)
            {
                return cleaned;
            }
        }

        private static bool IsPunctToTrim(char c)
        {
            // Keep inner apostrophes (don't), strip most leading/trailing punctuation.
            if (c == '\'' || c == '"') return false;
            return char.IsPunctuation(c);
        }
    }

    // -------- Adapter types (tiny glue you wire to your models) -------------

    /// <summary> Minimal TextGrid "word" interval. Build this from your parser. </summary>
    public readonly record struct TextGridWord(string Text, double Start, double End);

    /// <summary>
    /// Word adapter: expose BookIdx (nullable if this word doesn't map to a book token),
    /// and a setter that can write start/end/duration to your hydrate word object.
    /// </summary>
    public readonly record struct WordTarget(int? BookIdx, Action<double, double, double> SetTiming);

    /// <summary>
    /// Sentence adapter: expose its book range [start,end] (inclusive),
    /// and a setter to write timing back on your hydrate sentence.
    /// </summary>
    public readonly record struct SentenceTarget(int BookStartIdx, int BookEndIdx, Action<double, double, double> SetTiming);

    /// <summary> Summary metrics for logging/telemetry. </summary>
    public sealed class MergeReport
    {
        public int TextGridTokenCount { get; init; }
        public int BookTokenCount     { get; init; }
        public int Pairs              { get; init; }
        public int Matches            { get; init; }
        public int WildMatches        { get; init; }
        public int Insertions         { get; init; }
        public int Deletions          { get; init; }
        public int WordsUpdated       { get; init; }
        public int SentencesUpdated   { get; init; }
    }

    // -------- Internal DTOs -------------------------------------------------

    internal readonly record struct BookTok(int BookIdx, string Tok);
    internal readonly record struct TgTok(int TgSeq, string Tok, double Start, double End);
    internal readonly record struct Pair(int BookIdx, int TgSeq);

    internal sealed class AlignmentResult
    {
        public AlignmentResult(List<Pair> pairs, int matches, int wildMatches, int insertions, int deletions)
        {
            Pairs       = pairs;
            Matches     = matches;
            WildMatches = wildMatches;
            Insertions  = insertions;
            Deletions   = deletions;
        }

        public List<Pair> Pairs { get; }
        public int Matches { get; }
        public int WildMatches { get; }
        public int Insertions { get; }
        public int Deletions { get; }
    }
}
