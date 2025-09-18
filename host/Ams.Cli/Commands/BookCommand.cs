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

        var failures = new List<string>();
        var warnings = new List<string>();
        bool hasTotalFailures = false;
        bool hasOrderingFailures = false;

        var words = idx.Words ?? Array.Empty<BookWord>();
        var sentenceSegments = idx.Sentences
            .OrderBy(s => s.Index)
            .ToArray();
        var paragraphSegments = idx.Paragraphs
            .OrderBy(s => s.Index)
            .ToArray();

        int wordsCount = words.Length;
        int sentencesCount = sentenceSegments.Length;
        int paragraphsCount = paragraphSegments.Length;

        if (idx.Totals.Words != wordsCount)
        {
            failures.Add($"Totals.words ({idx.Totals.Words}) != words.Length ({wordsCount})");
            hasTotalFailures = true;
        }
        if (idx.Totals.Sentences != sentencesCount)
        {
            failures.Add($"Totals.sentences ({idx.Totals.Sentences}) != sentence segments ({sentencesCount})");
            hasTotalFailures = true;
        }
        if (idx.Totals.Paragraphs != paragraphsCount)
        {
            failures.Add($"Totals.paragraphs ({idx.Totals.Paragraphs}) != paragraph segments ({paragraphsCount})");
            hasTotalFailures = true;
        }

        if (wordsCount > 0)
        {
            if (words[0].WordIndex != 0)
        {
            failures.Add($"First wordIndex is {words[0].WordIndex}, expected 0");
            hasOrderingFailures = true;
        }
            for (int i = 1; i < words.Length; i++)
            {
                if (words[i].WordIndex != i)
                {
                    failures.Add($"wordIndex at position {i} is {words[i].WordIndex}, expected {i}");
                    hasOrderingFailures = true;
                    break;
                }
            }
        }

        if (sentencesCount > 0)
        {
            var sents = sentenceSegments;
            if (sents[0].Start != 0)
            {
                failures.Add($"First sentence.start is {sents[0].Start}, expected 0");
                hasOrderingFailures = true;
            }
            if (sents[^1].End != wordsCount - 1)
                failures.Add($"Last sentence.end is {sents[^1].End}, expected {wordsCount - 1}");
            for (int i = 0; i < sents.Length; i++)
            {
                var s = sents[i];
                if (s.Start < 0 || s.End < s.Start || s.End >= wordsCount)
                {
                    failures.Add($"Sentence {i} has invalid range [{s.Start},{s.End}] for wordsCount {wordsCount}");
                    hasOrderingFailures = true;
                }
                if (s.Index != i)
                {
                    failures.Add($"Sentence index mismatch: sentence.Index={s.Index}, expected {i}");
                    hasOrderingFailures = true;
                }
                if (i > 0)
                {
                    var prev = sents[i - 1];
                    if (s.Start != prev.End + 1)
                    {
                        failures.Add($"Sentence {i} does not continue from previous (prev.end={prev.End}, start={s.Start})");
                        hasOrderingFailures = true;
                    }
                }
                for (int w = s.Start; w <= s.End; w++)
                {
                    if (words[w].SentenceIndex != s.Index)
                    {
                        failures.Add($"word[{w}].sentenceIndex={words[w].SentenceIndex}, expected {s.Index}");
                        hasOrderingFailures = true;
                        break;
                    }
                }
            }
        }

        if (paragraphsCount > 0)
        {
            var paras = paragraphSegments;
            if (paras[0].Start != 0)
            {
                failures.Add($"First paragraph.start is {paras[0].Start}, expected 0");
                hasOrderingFailures = true;
            }
            if (paras[^1].End != wordsCount - 1)
                failures.Add($"Last paragraph.end is {paras[^1].End}, expected {wordsCount - 1}");
            for (int i = 0; i < paras.Length; i++)
            {
                var p = paras[i];
                if (p.Start < 0 || p.End < p.Start || p.End >= wordsCount)
                {
                    failures.Add($"Paragraph {i} has invalid range [{p.Start},{p.End}] for wordsCount {wordsCount}");
                    hasOrderingFailures = true;
                }
                if (p.Index != i)
                {
                    failures.Add($"Paragraph index mismatch: paragraph.Index={p.Index}, expected {i}");
                    hasOrderingFailures = true;
                }
                if (i > 0)
                {
                    var prev = paras[i - 1];
                    if (p.Start != prev.End + 1)
                    {
                        failures.Add($"Paragraph {i} does not continue from previous (prev.end={prev.End}, start={p.Start})");
                        hasOrderingFailures = true;
                    }
                }
                for (int w = p.Start; w <= p.End && w < wordsCount; w++)
                {
                    if (words[w].ParagraphIndex != p.Index)
                    {
                        failures.Add($"word[{w}].paragraphIndex={words[w].ParagraphIndex}, expected {p.Index}");
                        hasOrderingFailures = true;
                        break;
                    }
                }
            }
        }

        if (wordsCount > 1)
        {
            int apostropheSplitCount = 0;
            var examples = new List<string>();
            for (int i = 1; i < words.Length; i++)
            {
                var prev = words[i - 1].Text;
                var cur = words[i].Text;
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
            var paraSentences = new Dictionary<int, List<int>>();
            foreach (var s in sentenceSegments)
            {
                if (s.Start > s.End) continue;
                var pStart = words[s.Start].ParagraphIndex;
                var pEnd = words[s.End].ParagraphIndex;
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

        var canonical = JsonSerializer.Serialize(idx, new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull,
            WriteIndented = false
        });
        var stableHash = Sha256Hex(Encoding.UTF8.GetBytes(canonical));

        Console.WriteLine("\n=== Book Verify Results ===");
        Console.WriteLine($"Source file: {idx.SourceFile}");
        Console.WriteLine($"Words/Sentences/Paragraphs: {wordsCount}/{sentencesCount}/{paragraphsCount}");
        Console.WriteLine($"Counts parity: {(hasTotalFailures ? "FAIL" : "OK")}");
        Console.WriteLine($"Ordering/coverage: {(hasOrderingFailures ? "FAIL" : "OK")}");
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
            Console.WriteLine("\nKeep BookIndex canonical - do not normalize to fix.");
            Console.WriteLine("Adjust decoder tokenization and paragraph style classification (DocX) instead.");
            Environment.ExitCode = 2;
        }
        else
        {
            Console.WriteLine("\nOK: BookIndex is consistent and canonical.");
        }
    }

    private static bool EndsWithLetter(string s)
        => !string.IsNullOrEmpty(s) && char.IsLetter(s[^1]);

    private static bool IsStandaloneApostrophe(string s)
        => s == "'" || s == "\u2019";

    private static bool IsContractionSuffix(string s)
    {
        if (string.IsNullOrEmpty(s)) return false;
        var lower = s.ToLowerInvariant();
        return lower is "'s" or "\u2019s" or "'re" or "'m" or "'ve" or "'ll" or "'d" or "n't";
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











