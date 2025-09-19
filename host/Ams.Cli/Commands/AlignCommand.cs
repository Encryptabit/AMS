using System.CommandLine;
using System.Linq;
using System.Text.Json;
using Ams.Core.Alignment.Tx;
using Ams.Core.Artifacts;
using Ams.Core.Alignment.Anchors;
using Ams.Core;
using Ams.Core.Validation;

namespace Ams.Cli.Commands;

public static class AlignCommand
{
    public static Command Create()
    {
        var align = new Command("align", "Alignment utilities");
        align.AddCommand(CreateAnchors());
        align.AddCommand(CreateTranscriptIndex());
        align.AddCommand(CreateHydrateTx());
        return align;
    }

    private static Command CreateAnchors()
    {
        var cmd = new Command("anchors", "Compute n-gram anchors between BookIndex and ASR");

        var indexOption = new Option<FileInfo>("--index", "Path to BookIndex JSON") { IsRequired = true };
        indexOption.AddAlias("-i");
        var asrOption = new Option<FileInfo>("--asr-json", "Path to ASR JSON") { IsRequired = true };
        asrOption.AddAlias("-j");
        var outOption = new Option<FileInfo?>("--out", () => null, "Output JSON file (default: stdout)");

        var detectSectionOption = new Option<bool>("--detect-section", () => true, "Detect section from ASR prefix and restrict window");
        var ngramOption = new Option<int>("--ngram", () => 3, "Anchor n-gram size");
        var targetPerTokensOption = new Option<int>("--target-per-tokens", () => 50, "Approx. 1 anchor per N book tokens");
        var minSeparationOption = new Option<int>("--min-separation", () => 100, "Min token separation when duplicates allowed during relaxation");
        var crossSentencesOption = new Option<bool>("--cross-sentences", () => false, "Allow anchors to cross sentence boundaries");
        var domainStopwordsOption = new Option<bool>("--domain-stopwords", () => true, "Use English/domain stopwords");

        var asrPrefixTokensOption = new Option<int>("--asr-prefix", () => 8, "ASR tokens to consider for section detection");
        var emitWindowsOption = new Option<bool>("--emit-windows", () => false, "Also emit search windows between anchors");

        cmd.AddOption(indexOption);
        cmd.AddOption(asrOption);
        cmd.AddOption(outOption);
        cmd.AddOption(detectSectionOption);
        cmd.AddOption(ngramOption);
        cmd.AddOption(targetPerTokensOption);
        cmd.AddOption(minSeparationOption);
        cmd.AddOption(crossSentencesOption);
        cmd.AddOption(domainStopwordsOption);
        cmd.AddOption(asrPrefixTokensOption);
        cmd.AddOption(emitWindowsOption);

        cmd.SetHandler(async (context) =>
        {
            var indexFile = context.ParseResult.GetValueForOption(indexOption)!;
            var asrFile = context.ParseResult.GetValueForOption(asrOption)!;
            var outFile = context.ParseResult.GetValueForOption(outOption);
            var detectSection = context.ParseResult.GetValueForOption(detectSectionOption);
            var ngram = context.ParseResult.GetValueForOption(ngramOption);
            var targetPerTokens = context.ParseResult.GetValueForOption(targetPerTokensOption);
            var minSeparation = context.ParseResult.GetValueForOption(minSeparationOption);
            var crossSentences = context.ParseResult.GetValueForOption(crossSentencesOption);
            var domainStopwords = context.ParseResult.GetValueForOption(domainStopwordsOption);
            var asrPrefix = context.ParseResult.GetValueForOption(asrPrefixTokensOption);
            var emitWindows = context.ParseResult.GetValueForOption(emitWindowsOption);

            try
            {
                await RunAnchorsAsync(indexFile, asrFile, outFile, detectSection, ngram, targetPerTokens, minSeparation, crossSentences, domainStopwords, asrPrefix, emitWindows);
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine($"Error: {ex.Message}");
                Environment.Exit(1);
            }
        });

        return cmd;
    }

    private static Command CreateTranscriptIndex()
    {
        var cmd = new Command("tx", "Validate & compare Book vs ASR using anchors; emit TranscriptIndex (*.tx.json)");

        var indexOption = new Option<FileInfo>("--index", "Path to BookIndex JSON") { IsRequired = true };
        indexOption.AddAlias("-i");
        var asrOption = new Option<FileInfo>("--asr-json", "Path to ASR JSON") { IsRequired = true };
        asrOption.AddAlias("-j");
        var audioOption = new Option<FileInfo>("--audio", "Path to audio file for reference") { IsRequired = true };
        audioOption.AddAlias("-a");
        var outOption = new Option<FileInfo>("--out", "Output TranscriptIndex JSON (*.tx.json)") { IsRequired = true };
        outOption.AddAlias("-o");

        var detectSectionOption = new Option<bool>("--detect-section", () => true, "Detect section from ASR prefix and restrict window");
        var asrPrefixTokensOption = new Option<int>("--asr-prefix", () => 8, "ASR tokens to consider for section detection");
        var ngramOption = new Option<int>("--ngram", () => 3, "Anchor n-gram size");
        var targetPerTokensOption = new Option<int>("--target-per-tokens", () => 50, "Approx. 1 anchor per N book tokens");
        var minSeparationOption = new Option<int>("--min-separation", () => 100, "Min token separation when duplicates allowed during relaxation");
        var crossSentencesOption = new Option<bool>("--cross-sentences", () => false, "Allow anchors to cross sentence boundaries");
        var domainStopwordsOption = new Option<bool>("--domain-stopwords", () => true, "Use English/domain stopwords for anchors");

        cmd.AddOption(indexOption);
        cmd.AddOption(asrOption);
        cmd.AddOption(audioOption);
        cmd.AddOption(outOption);
        cmd.AddOption(detectSectionOption);
        cmd.AddOption(asrPrefixTokensOption);
        cmd.AddOption(ngramOption);
        cmd.AddOption(targetPerTokensOption);
        cmd.AddOption(minSeparationOption);
        cmd.AddOption(crossSentencesOption);
        cmd.AddOption(domainStopwordsOption);

        cmd.SetHandler(async (context) =>
        {
            var indexFile = context.ParseResult.GetValueForOption(indexOption)!;
            var asrFile = context.ParseResult.GetValueForOption(asrOption)!;
            var audioFile = context.ParseResult.GetValueForOption(audioOption)!;
            var outFile = context.ParseResult.GetValueForOption(outOption)!;
            var detectSection = context.ParseResult.GetValueForOption(detectSectionOption);
            var asrPrefix = context.ParseResult.GetValueForOption(asrPrefixTokensOption);
            var ngram = context.ParseResult.GetValueForOption(ngramOption);
            var targetPerTokens = context.ParseResult.GetValueForOption(targetPerTokensOption);
            var minSeparation = context.ParseResult.GetValueForOption(minSeparationOption);
            var crossSentences = context.ParseResult.GetValueForOption(crossSentencesOption);
            var domainStopwords = context.ParseResult.GetValueForOption(domainStopwordsOption);

            try
            {
                await RunTranscriptIndexAsync(indexFile, asrFile, audioFile, outFile, detectSection, asrPrefix, ngram, targetPerTokens, minSeparation, crossSentences, domainStopwords);
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine($"Error: {ex.Message}");
                Environment.Exit(1);
            }
        });

        return cmd;
    }

    private static Command CreateHydrateTx()
    {
        var cmd = new Command("hydrate", "Hydrate a TranscriptIndex with token values from BookIndex and ASR (for debugging)");

        var indexOption = new Option<FileInfo>("--index", "Path to BookIndex JSON") { IsRequired = true };
        indexOption.AddAlias("-i");
        var asrOption = new Option<FileInfo>("--asr-json", "Path to ASR JSON") { IsRequired = true };
        asrOption.AddAlias("-j");
        var txOption = new Option<FileInfo>("--tx-json", "Path to TranscriptIndex JSON") { IsRequired = true };
        txOption.AddAlias("-t");
        var outOption = new Option<FileInfo>("--out", "Output hydrated JSON file") { IsRequired = true };
        outOption.AddAlias("-o");

        cmd.AddOption(indexOption);
        cmd.AddOption(asrOption);
        cmd.AddOption(txOption);
        cmd.AddOption(outOption);

        cmd.SetHandler(async (context) =>
        {
            var indexFile = context.ParseResult.GetValueForOption(indexOption)!;
            var asrFile = context.ParseResult.GetValueForOption(asrOption)!;
            var txFile = context.ParseResult.GetValueForOption(txOption)!;
            var outFile = context.ParseResult.GetValueForOption(outOption)!;
            try
            {
                await RunHydrateTxAsync(indexFile, asrFile, txFile, outFile);
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine($"Error: {ex.Message}");
                Environment.Exit(1);
            }
        });

        return cmd;
    }

    private static async Task RunHydrateTxAsync(FileInfo indexFile, FileInfo asrFile, FileInfo txFile, FileInfo outFile)
    {
        var jsonOptions = new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase };

        var indexJson = await File.ReadAllTextAsync(indexFile.FullName);
        var book = JsonSerializer.Deserialize<BookIndex>(indexJson, jsonOptions) ?? throw new InvalidOperationException("Failed to parse BookIndex JSON");

        var asrJson = await File.ReadAllTextAsync(asrFile.FullName);
        var asr = JsonSerializer.Deserialize<AsrResponse>(asrJson, jsonOptions) ?? throw new InvalidOperationException("Failed to parse ASR JSON");

        var txJson = await File.ReadAllTextAsync(txFile.FullName);
        var tx = JsonSerializer.Deserialize<TranscriptIndex>(txJson, jsonOptions) ?? throw new InvalidOperationException("Failed to parse TranscriptIndex JSON");

        // Hydrate words with values (1)
        var words = tx.Words.Select(w => new
        {
            bookIdx = w.BookIdx,
            asrIdx = w.AsrIdx,
            bookWord = w.BookIdx.HasValue && w.BookIdx.Value >= 0 && w.BookIdx.Value < book.Words.Length ? book.Words[w.BookIdx.Value].Text : null,
            asrWord = w.AsrIdx.HasValue && w.AsrIdx.Value >= 0 && w.AsrIdx.Value < asr.Tokens.Length ? asr.Tokens[w.AsrIdx.Value].Word : null,
            op = w.Op.ToString(),
            reason = w.Reason,
            score = w.Score
        }).ToList();

        // Helper to slice book/asr strings (2)
        static string JoinBook(BookIndex b, int start, int end)
        {
            if (start < 0 || end >= b.Words.Length || end < start) return string.Empty;
            return string.Join(" ", b.Words.Skip(start).Take(end - start + 1).Select(x => x.Text));
        }
        static string JoinAsr(AsrResponse a, int? start, int? end)
        {
            if (!start.HasValue || !end.HasValue) return string.Empty;
            int s = start.Value, e = end.Value;
            if (s < 0 || e >= a.Tokens.Length || e < s) return string.Empty;
            return string.Join(" ", a.Tokens.Skip(s).Take(e - s + 1).Select(x => x.Word));
        }

        // Hydrate sentences (3)
        var sentences = tx.Sentences.Select(s => new
        {
            id = s.Id,
            bookRange = new { start = s.BookRange.Start, end = s.BookRange.End },
            scriptRange = s.ScriptRange != null ? new { start = s.ScriptRange.Start, end = s.ScriptRange.End } : null,
            bookText = JoinBook(book, s.BookRange.Start, s.BookRange.End),
            scriptText = s.ScriptRange != null ? JoinAsr(asr, s.ScriptRange.Start, s.ScriptRange.End) : string.Empty,
            metrics = s.Metrics,
            status = s.Status
        }).ToList();

        // Hydrate paragraphs (4)
        var paragraphs = tx.Paragraphs.Select(p => new
        {
            id = p.Id,
            bookRange = new { start = p.BookRange.Start, end = p.BookRange.End },
            sentenceIds = p.SentenceIds,
            bookText = JoinBook(book, p.BookRange.Start, p.BookRange.End),
            metrics = p.Metrics,
            status = p.Status
        }).ToList();

        var hydrated = new
        {
            tx.AudioPath,
            tx.ScriptPath,
            tx.BookIndexPath,
            tx.CreatedAtUtc,
            tx.NormalizationVersion,
            words,
            sentences,
            paragraphs
        };

        var outJson = JsonSerializer.Serialize(hydrated, new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase, WriteIndented = true });
        await File.WriteAllTextAsync(outFile.FullName, outJson);
        Console.WriteLine($"Hydrated TranscriptIndex written to: {outFile.FullName}");
    }

    private static async Task RunTranscriptIndexAsync(
    FileInfo indexFile,
    FileInfo asrFile,
    FileInfo audioFile,
    FileInfo outFile,
    bool detectSection,
    int asrPrefixTokens,
    int ngram,
    int targetPerTokens,
    int minSeparation,
    bool crossSentences,
    bool domainStopwords)
{
    var jsonOptions = new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase };
    var indexJson = await File.ReadAllTextAsync(indexFile.FullName);
    var book = JsonSerializer.Deserialize<BookIndex>(indexJson, jsonOptions) ?? throw new InvalidOperationException("Failed to parse BookIndex JSON");

    var asrJson = await File.ReadAllTextAsync(asrFile.FullName);
    var asr = JsonSerializer.Deserialize<AsrResponse>(asrJson, jsonOptions) ?? throw new InvalidOperationException("Failed to parse ASR JSON");

    // Build normalized views (1)
    var bookView = AnchorPreprocessor.BuildBookView(book);
    var asrView = AnchorPreprocessor.BuildAsrView(asr);

    // Compute anchors & windows (2)
    var stop = domainStopwords ? StopwordSets.EnglishPlusDomain : new HashSet<string>(StringComparer.Ordinal);
    var policy = new AnchorPolicy(
        NGram: ngram,
        TargetPerTokens: targetPerTokens,
        AllowDuplicates: false,
        MinSeparation: minSeparation,
        Stopwords: stop,
        DisallowBoundaryCross: !crossSentences
    );
    var secOpts = new SectionDetectOptions(Detect: detectSection, AsrPrefixTokens: asrPrefixTokens);
    var pipe = AnchorPipeline.ComputeAnchors(book, asr, policy, secOpts, includeWindows: true);

    var windows = pipe.Windows ?? new List<(int bLo, int bHi, int aLo, int aHi)>
    {
        (pipe.BookWindowFiltered.bStart, pipe.BookWindowFiltered.bEnd + 1, 0, asrView.Tokens.Count)
    };

    // Equivalences and fillers (3)
    var equiv = new Dictionary<string, string>(StringComparer.Ordinal) { };
    var fillers = new HashSet<string>(new[] { "uh", "um", "erm", "uhh", "hmm", "mm", "huh", "like" }, StringComparer.Ordinal);

    // Align in filtered coordinates (4)
    var opsNm = TranscriptAligner.AlignWindows(
        bookView.Tokens,
        asrView.Tokens,
        windows,
        equiv,
        fillers);

    var anchorOps = new List<WordAlign>(pipe.Anchors.Count * policy.NGram);
    var anchorSeen = new HashSet<(int BookIdx, int? AsrIdx)>();
    foreach (var anchor in pipe.Anchors)
    {
        for (int k = 0; k < policy.NGram; k++)
        {
            int bookFiltered = anchor.Bp + k;
            if (bookFiltered < 0 || bookFiltered >= pipe.BookFilteredToOriginalWord.Count)
            {
                continue;
            }

            int bookIdx = pipe.BookFilteredToOriginalWord[bookFiltered];
            if (bookIdx < 0 || bookIdx >= book.Words.Length)
            {
                continue;
            }

            int? asrIdx = null;
            int asrFiltered = anchor.Ap + k;
            if (asrFiltered >= 0 && asrFiltered < asrView.FilteredToOriginalToken.Count)
            {
                asrIdx = asrView.FilteredToOriginalToken[asrFiltered];
            }

            if (anchorSeen.Add((bookIdx, asrIdx)))
            {
                anchorOps.Add(new WordAlign(bookIdx, asrIdx, AlignOp.Match, "anchor", 1.0));
            }
        }
    }

    var dpOps = new List<WordAlign>(opsNm.Count);
    foreach (var (bi, aj, op, reason, score) in opsNm)
    {
        int? bookIdx = bi.HasValue ? pipe.BookFilteredToOriginalWord[bi.Value] : (int?)null;
        int? asrIdx = aj.HasValue ? asrView.FilteredToOriginalToken[aj.Value] : (int?)null;
        dpOps.Add(new WordAlign(bookIdx, asrIdx, op, reason, score));
    }

    var combinedOps = anchorOps
        .OrderBy(op => op.BookIdx)
        .ThenBy(op => op.AsrIdx ?? int.MaxValue)
        .Concat(dpOps)
        .ToList();

    var seenOps = new HashSet<(int?, int?, AlignOp)>();
    var wordOps = new List<WordAlign>(combinedOps.Count);
    foreach (var op in combinedOps)
    {
        if (seenOps.Add((op.BookIdx, op.AsrIdx, op.Op)))
        {
            wordOps.Add(op);
        }
    }

    // Section range on original indices (6)
    int secStartWord = 0;
    int secEndWord = book.Words.Length - 1;
    if (pipe.Section != null)
    {
        secStartWord = Math.Max(0, pipe.Section.StartWord);
        secEndWord = Math.Min(book.Words.Length - 1, pipe.Section.EndWord);
    }

    var sentTuples = book.Sentences
        .Where(s => s.Start <= secEndWord && s.End >= secStartWord)
        .Select(s => (s.Index, Math.Max(secStartWord, s.Start), Math.Min(secEndWord, s.End)))
        .ToList();
    var paraTuples = book.Paragraphs
        .Where(p => p.Start <= secEndWord && p.End >= secStartWord)
        .Select(p => (p.Index, Math.Max(secStartWord, p.Start), Math.Min(secEndWord, p.End)))
        .ToList();

    var (sentAlign, paraAlign) = TranscriptAligner.Rollup(
        wordOps,
        sentTuples.Select(t => (t.Index, t.Item2, t.Item3)).ToList(),
        paraTuples.Select(t => (t.Index, t.Item2, t.Item3)).ToList());

    var timedSentences = sentAlign
        .Select(s => s with { Timing = ComputeTiming(s.ScriptRange, asr) })
        .ToList();

    var tx = new TranscriptIndex(
        AudioPath: audioFile.FullName,
        ScriptPath: asrFile.FullName,
        BookIndexPath: indexFile.FullName,
        CreatedAtUtc: DateTime.UtcNow,
        NormalizationVersion: "v1",
        Words: wordOps,
        Sentences: timedSentences,
        Paragraphs: paraAlign);

    var outJson = JsonSerializer.Serialize(tx, new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase, WriteIndented = true });
    await File.WriteAllTextAsync(outFile.FullName, outJson);
    Console.WriteLine($"TranscriptIndex written to: {outFile.FullName}");
}

    private static TimingRange ComputeTiming(ScriptRange? scriptRange, AsrResponse asr)
    {
        if (scriptRange?.Start is not int start || scriptRange.End is not int end || asr.Tokens.Length == 0)
        {
            return TimingRange.Empty;
        }

        start = Math.Clamp(start, 0, asr.Tokens.Length - 1);
        end = Math.Clamp(end, start, asr.Tokens.Length - 1);

        var startToken = asr.Tokens[start];
        var endToken = asr.Tokens[end];
        var startSec = startToken.StartTime;
        var endSec = endToken.StartTime + endToken.Duration;

        return new TimingRange(startSec, endSec);
    }

    private static async Task RunAnchorsAsync(
        FileInfo indexFile,
        FileInfo asrFile,
        FileInfo? outFile,
        bool detectSection,
        int ngram,
        int targetPerTokens,
        int minSeparation,
        bool crossSentences,
        bool domainStopwords,
        int asrPrefixTokens,
        bool emitWindows)
    {
        if (!indexFile.Exists) throw new FileNotFoundException($"Index file not found: {indexFile.FullName}");
        if (!asrFile.Exists) throw new FileNotFoundException($"ASR JSON file not found: {asrFile.FullName}");

        var jsonOptions = new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase };

        var indexJson = await File.ReadAllTextAsync(indexFile.FullName);
        var book = JsonSerializer.Deserialize<BookIndex>(indexJson, jsonOptions) ?? throw new InvalidOperationException("Failed to parse BookIndex JSON");

        var asrJson = await File.ReadAllTextAsync(asrFile.FullName);
        var asr = JsonSerializer.Deserialize<AsrResponse>(asrJson, jsonOptions) ?? throw new InvalidOperationException("Failed to parse ASR JSON");

        // Build policy
        var stop = domainStopwords ? StopwordSets.EnglishPlusDomain : new HashSet<string>(StringComparer.Ordinal);
        var policy = new AnchorPolicy(
            NGram: ngram,
            TargetPerTokens: targetPerTokens,
            AllowDuplicates: false,
            MinSeparation: minSeparation,
            Stopwords: stop,
            DisallowBoundaryCross: !crossSentences
        );

        var secOpts = new SectionDetectOptions(Detect: detectSection, AsrPrefixTokens: asrPrefixTokens);
        var pipe = AnchorPipeline.ComputeAnchors(book, asr, policy, secOpts, includeWindows: emitWindows);

        var anchorsOut = pipe.Anchors.Select(a => new
        {
            bp = a.Bp,
            bpWordIndex = pipe.BookFilteredToOriginalWord[a.Bp],
            ap = a.Ap
        }).ToList();

        var payload = new
        {
            sectionDetected = pipe.SectionDetected,
            section = pipe.Section == null ? null : new { pipe.Section.Id, pipe.Section.Title, pipe.Section.Level, pipe.Section.Kind, pipe.Section.StartWord, pipe.Section.EndWord },
            policy = new { ngram, targetPerTokens, minSeparation, disallowBoundaryCross = !crossSentences, stopwords = domainStopwords ? "domain" : "none" },
            tokens = new { bookCount = pipe.BookTokenCount, bookFilteredCount = pipe.BookFilteredCount, asrCount = pipe.AsrTokenCount, asrFilteredCount = pipe.AsrFilteredCount },
            window = new { bookStart = pipe.BookWindowFiltered.bStart, bookEnd = pipe.BookWindowFiltered.bEnd },
            anchors = anchorsOut,
            windows = pipe.Windows?.Select(w => new { bLo = w.bLo, bHi = w.bHi, aLo = w.aLo, aHi = w.aHi })
        };

        var outputJson = JsonSerializer.Serialize(payload, new JsonSerializerOptions { WriteIndented = true });
        if (outFile == null)
        {
            Console.WriteLine(outputJson);
        }
        else
        {
            await File.WriteAllTextAsync(outFile.FullName, outputJson);
            Console.WriteLine($"Anchors written to: {outFile.FullName}");
        }
    }
}







