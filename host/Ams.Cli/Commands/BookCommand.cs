using System.CommandLine;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using Ams.Core;

namespace Ams.Cli.Commands;

public static class BookCommand
{
    public static Command Create()
    {
        var book = new Command("book", "Book-related operations");
        book.AddCommand(CreateVerify());
        return book;
    }

    private static Command CreateVerify()
    {
        var verify = new Command("verify", "Verify a canonical BookIndex JSON (read-only)");

        var indexOption = new Option<FileInfo>("--index", "Path to BookIndex JSON") { IsRequired = true };
        indexOption.AddAlias("-i");
        verify.AddOption(indexOption);

        verify.SetHandler(async context =>
        {
            var indexFile = context.ParseResult.GetValueForOption(indexOption)!;
            try
            {
                await RunVerifyAsync(indexFile);
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine($"Error: {ex.Message}");
                Environment.Exit(1);
            }
        });

        return verify;
    }

    private static async Task RunVerifyAsync(FileInfo indexFile)
    {
        Console.WriteLine("Verifying BookIndex (non-mutating)...");
        if (!indexFile.Exists)
            throw new FileNotFoundException($"Index file not found: {indexFile.FullName}");

        var rawJson = await File.ReadAllTextAsync(indexFile.FullName);

        // Try to deserialize as current canonical model
        var jsonOptions = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            ReadCommentHandling = JsonCommentHandling.Skip,
            AllowTrailingCommas = true
        };

        BookIndex? idx = null;
        try
        {
            idx = JsonSerializer.Deserialize<BookIndex>(rawJson, jsonOptions);
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException("Failed to parse BookIndex JSON as canonical schema. Ensure schema matches Ams.Core.BookIndex.", ex);
        }

        if (idx == null)
            throw new InvalidOperationException("Deserialized BookIndex is null.");

        // Perform checks
        var failures = new List<string>();
        var warnings = new List<string>();

        // Counts parity
        var wordsCount = idx.Words?.Length ?? 0;
        var sentencesCount = idx.Sentences?.Length ?? 0;
        var paragraphsCount = idx.Paragraphs?.Length ?? 0;

        if (idx.Totals.Words != wordsCount)
            failures.Add($"Totals.words ({idx.Totals.Words}) != words.Length ({wordsCount})");
        if (idx.Totals.Sentences != sentencesCount)
            failures.Add($"Totals.sentences ({idx.Totals.Sentences}) != sentences.Length ({sentencesCount})");
        if (idx.Totals.Paragraphs != paragraphsCount)
            failures.Add($"Totals.paragraphs ({idx.Totals.Paragraphs}) != paragraphs.Length ({paragraphsCount})");

        // Ordering & coverage checks for words
        if (wordsCount > 0)
        {
            if (idx.Words![0].WordIndex != 0)
                failures.Add($"First wordIndex is {idx.Words![0].WordIndex}, expected 0");
            for (int i = 1; i < idx.Words!.Length; i++)
            {
                if (idx.Words[i].WordIndex != i)
                {
                    failures.Add($"wordIndex at position {i} is {idx.Words[i].WordIndex}, expected {i}");
                    break;
                }
            }
        }

        // Sentences ordering & coverage
        if (sentencesCount > 0)
        {
            var sents = idx.Sentences!;
            if (sents[0].Start != 0)
                failures.Add($"First sentence.start is {sents[0].Start}, expected 0");
            if (sents[^1].End != wordsCount - 1)
                failures.Add($"Last sentence.end is {sents[^1].End}, expected {wordsCount - 1}");
            for (int i = 0; i < sents.Length; i++)
            {
                var s = sents[i];
                if (s.Start < 0 || s.End < s.Start || s.End >= wordsCount)
                    failures.Add($"Sentence {i} has invalid range [{s.Start},{s.End}] for wordsCount {wordsCount}");
                if (s.Index != i)
                    failures.Add($"Sentence index mismatch: sentence.Index={s.Index}, expected {i}");
                if (i > 0)
                {
                    var prev = sents[i - 1];
                    if (s.Start != prev.End + 1)
                        failures.Add($"Sentence {i} does not continue from previous (prev.end={prev.End}, start={s.Start})");
                }
                // Verify word -> sentence mapping
                for (int w = s.Start; w <= s.End; w++)
                {
                    if (idx.Words![w].SentenceIndex != s.Index)
                    {
                        failures.Add($"Word {w} sentenceIndex={idx.Words![w].SentenceIndex} != sentence.Index={s.Index}");
                        break;
                    }
                }
            }
        }

        // Paragraphs ordering & coverage
        if (paragraphsCount > 0)
        {
            var paras = idx.Paragraphs!;
            if (paras[0].Start != 0)
                failures.Add($"First paragraph.start is {paras[0].Start}, expected 0");
            if (paras[^1].End != wordsCount - 1)
                failures.Add($"Last paragraph.end is {paras[^1].End}, expected {wordsCount - 1}");
            for (int i = 0; i < paras.Length; i++)
            {
                var p = paras[i];
                if (p.Start < 0 || p.End < p.Start || p.End >= wordsCount)
                    failures.Add($"Paragraph {i} has invalid range [{p.Start},{p.End}] for wordsCount {wordsCount}");
                if (p.Index != i)
                    failures.Add($"Paragraph index mismatch: paragraph.Index={p.Index}, expected {i}");
                if (i > 0)
                {
                    var prev = paras[i - 1];
                    if (p.Start != prev.End + 1)
                        failures.Add($"Paragraph {i} does not continue from previous (prev.end={prev.End}, start={p.Start})");
                }
                // Verify word -> paragraph mapping
                for (int w = p.Start; w <= p.End; w++)
                {
                    if (idx.Words![w].ParagraphIndex != p.Index)
                    {
                        failures.Add($"Word {w} paragraphIndex={idx.Words![w].ParagraphIndex} != paragraph.Index={p.Index}");
                        break;
                    }
                }
            }
        }

        // Heuristic warnings: apostrophe splits and TOC bursts
        if (wordsCount > 1)
        {
            int apostropheSplitCount = 0;
            var examples = new List<string>();
            for (int i = 1; i < idx.Words!.Length; i++)
            {
                var prev = idx.Words[i - 1].Text;
                var cur = idx.Words[i].Text;
                if (IsContractionSuffix(cur) && EndsWithLetter(prev))
                {
                    apostropheSplitCount++;
                    if (examples.Count < 5)
                        examples.Add($"... {prev} {cur} ... @ {i - 1}-{i}");
                }
                else if (IsStandaloneApostrophe(cur))
                {
                    apostropheSplitCount++;
                    if (examples.Count < 5)
                        examples.Add($"... {prev} {cur} ... @ {i - 1}-{i}");
                }
            }
            if (apostropheSplitCount > 0)
            {
                warnings.Add($"Apostrophe splits detected: {apostropheSplitCount} (e.g., {string.Join("; ", examples)})");
            }
        }

        if (sentencesCount > 0 && paragraphsCount > 0)
        {
            // For each paragraph, compute sentence lengths within paragraph
            var paraSentences = new Dictionary<int, List<int>>();
            foreach (var s in idx.Sentences!)
            {
                if (s.Start > s.End) continue;
                var pStart = idx.Words![s.Start].ParagraphIndex;
                var pEnd = idx.Words![s.End].ParagraphIndex;
                if (pStart == pEnd)
                {
                    if (!paraSentences.TryGetValue(pStart, out var list))
                    {
                        list = new List<int>();
                        paraSentences[pStart] = list;
                    }
                    list.Add(s.End - s.Start + 1);
                }
            }

            int tocLikeParas = 0;
            foreach (var kv in paraSentences)
            {
                var lens = kv.Value;
                if (lens.Count >= 10)
                {
                    var median = Median(lens);
                    if (median <= 3)
                        tocLikeParas++;
                }
            }
            if (tocLikeParas > 0)
            {
                warnings.Add($"Possible TOC bursts: {tocLikeParas} paragraph(s) with many very short sentences (median <= 3 words).");
            }
        }

        // Deterministic hash of canonical serialization
        var canonical = JsonSerializer.Serialize(idx, new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull,
            WriteIndented = false
        });
        var stableHash = Sha256Hex(Encoding.UTF8.GetBytes(canonical));

        // Emit results
        Console.WriteLine("\n=== Book Verify Results ===");
        Console.WriteLine($"Source file: {idx.SourceFile}");
        Console.WriteLine($"Words/Sentences/Paragraphs: {wordsCount}/{sentencesCount}/{paragraphsCount}");
        Console.WriteLine($"Counts parity: {(failures.Any(f => f.Contains("Totals")) ? "FAIL" : "OK")}");
        Console.WriteLine($"Ordering/coverage: {(failures.Except(failures.Where(f=>f.Contains("Totals"))).Any() ? "FAIL" : "OK")}");
        Console.WriteLine($"Warnings: {(warnings.Count == 0 ? "none" : warnings.Count.ToString())}");
        Console.WriteLine($"Determinism hash (canonical JSON): {stableHash}");

        if (warnings.Count > 0)
        {
            Console.WriteLine("\n- Warning details:");
            foreach (var w in warnings)
                Console.WriteLine($"  - {w}");
        }

        if (failures.Count > 0)
        {
            Console.WriteLine("\n- Failures:");
            foreach (var f in failures)
                Console.WriteLine($"  - {f}");
            Console.WriteLine("\nKeep BookIndex canonical — do not normalize to fix.");
            Console.WriteLine("Adjust decoder tokenization and paragraph style classification (DocX) instead.");
            Environment.ExitCode = 2; // Non-zero for CI
        }
        else
        {
            Console.WriteLine("\nOK: BookIndex is consistent and canonical.");
        }
    }

    private static bool EndsWithLetter(string s)
        => !string.IsNullOrEmpty(s) && char.IsLetter(s[^1]);

    private static bool IsStandaloneApostrophe(string s)
        => s == "'" || s == "’";

    private static bool IsContractionSuffix(string s)
    {
        if (string.IsNullOrEmpty(s)) return false;
        var lower = s.ToLowerInvariant();
        return lower is "'s" or "’s" or "'re" or "'m" or "'ve" or "'ll" or "'d" or "n't";
    }

    private static double Median(List<int> values)
    {
        if (values.Count == 0) return 0;
        values.Sort();
        int mid = values.Count / 2;
        if (values.Count % 2 == 1) return values[mid];
        return (values[mid - 1] + values[mid]) / 2.0;
    }

    private static string Sha256Hex(byte[] bytes)
    {
        using var sha = SHA256.Create();
        var hash = sha.ComputeHash(bytes);
        return Convert.ToHexString(hash);
    }
}

